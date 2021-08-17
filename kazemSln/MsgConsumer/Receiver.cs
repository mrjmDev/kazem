using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace MsgConsumer
{
    public class Receiver
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var chanel=connection.CreateModel())
                {
                    chanel.QueueDeclare("BasicTestQ", false, false, false, null);
                    var consumer = new EventingBasicConsumer(chanel);
                    // consumer.Received += Consumer_Received;


                    consumer.Received += (model, e) =>
                     {
                         var body = e.Body.Span;
                         var msg = Encoding.UTF8.GetString(body);
                         Console.WriteLine("recived =>  {0}", msg);
                     };
                   chanel.BasicConsume("BasicTestQ", true, consumer);


                }
                
            }
            Console.WriteLine("finish");
            Console.ReadLine();
        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            // throw new NotImplementedException();
            var body = e.Body;
            var msg = Encoding.UTF8.GetString(body.ToArray());
            Console.WriteLine("recived =>  {0}", msg);
        }
    }
}
