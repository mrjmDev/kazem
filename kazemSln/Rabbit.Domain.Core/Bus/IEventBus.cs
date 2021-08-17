using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Domain.Core.Bus
{
    public interface IEventBus
    {

        Task SendCommand<T>(T command) where T : Command;
        void Publish<T>(T @event) where T : EventArgs;

        void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>;
    }


    public interface IEventHandler
    {

    }

    public interface IEventHandler<in TEvent>: IEventHandler
        where TEvent: Event
    {
        Task Handle(TEvent @event);
    }

    


    //commands Folder
    public abstract class Command :Message
    {
        public DateTime Timestamp { get; protected set; }
        protected Command()
        {
            Timestamp = DateTime.Now;
        }
    }





    //Events Folder
    public abstract class Message : IRequest<bool> //meditor
    {
        public string MessageType{get;protected set;}

        protected Message()
        {
            MessageType = GetType().Name;
        }

    }

    public abstract class Event
    {
        public DateTime Timestamp { get; protected set; }

        protected Event()
        {
            Timestamp = DateTime.Now;

        }
    }
}
