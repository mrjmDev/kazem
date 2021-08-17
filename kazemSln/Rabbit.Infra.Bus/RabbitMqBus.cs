using MediatR;
using Newtonsoft.Json;
using Rabbit.Domain.Core.Bus;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Infra.Bus
{
    public sealed class RabbitMqBus : IEventBus
    {

        private readonly IMediator _mediator;
        private readonly Dictionary<string, List<Type>> _handlers;
        private List<Type> _eventTypes;

        public RabbitMqBus(IMediator mediator)
        {
            _mediator = mediator;
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();
        }

        public Task SendCommand<T>(T command) where T : Command
        {
            //throw new NotImplementedException();

            return _mediator.Send(command);
        }


        public void Publish<T>(T @event) where T : EventArgs
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var chanel = connection.CreateModel())
            {
                var evName = @event.GetType().Name;
                chanel.QueueDeclare(evName, false, false, false, null);
                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);
                chanel.BasicPublish("", evName, null, body);
            }
        }

        public void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            //throw new NotImplementedException();
            var evName = typeof(T).Name;
            var handlerType = typeof(TH);

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }
            if (!_handlers.ContainsKey(evName))
            {
                _handlers.Add(evName, new List<Type>());
            }
            if (_handlers[evName].Any(s => s.GetType() == handlerType))
            {
                throw new ArgumentException("asdasd => " + evName + "  " + nameof(handlerType));
            }

            _handlers[evName].Add(handlerType);

            StartBasicConsume<T>();
        }

        private void StartBasicConsume<T>() where T : Event
        {


            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                DispatchConsumersAsync = true
            };
            var connection = factory.CreateConnection();
            var chanel = connection.CreateModel();

            var eventName = typeof(T).Name;

            chanel.QueueDeclare(eventName, false, false, false, null);


            var consumer = new AsyncEventingBasicConsumer(chanel);
            consumer.Received += Consumer_Received;
            chanel.BasicConsume(eventName, true, consumer);
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var evName = e.RoutingKey;

            var msg = Encoding.UTF8.GetString(e.Body.Span);


            try
            {
                await ProcessEvent(evName, msg).ConfigureAwait(false);
            }
            catch(Exception ex)
            {

            }
        }

        private async Task ProcessEvent(string evName, string msg)
        {
            if (_handlers.ContainsKey(evName))
            {
                var subs = _handlers[evName];
                foreach(var sub in subs)
                {
                    var handler = Activator.CreateInstance(sub);
                    if (handler == null) continue;

                    var eventType = _eventTypes.SingleOrDefault(t => t.Name == evName);

                    var @event = JsonConvert.DeserializeObject(msg, eventType);

                    var concretType = typeof(IEventHandler<>).MakeGenericType(eventType);

                    await (Task)concretType.GetMethod("Handle").Invoke(handler, new object[] { @event });

                }
            }
        }
    }
}
