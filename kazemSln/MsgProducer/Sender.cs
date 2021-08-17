using RabbitMQ.Client;
using System;
using System.Text;

namespace MsgProducer
{
    class Sender
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection =factory.CreateConnection())
                using(var chanel=connection.CreateModel())
            {
                chanel.QueueDeclare("BasicTestQ", false, false, false, null);
                string msg = "This is the Msg";
                var body = Encoding.UTF8.GetBytes(msg);
                chanel.BasicPublish("", "BasicTestQ", null, body);

                Console.WriteLine("Message Sent");
            }

            Console.ReadLine();
        }
    }
}
