using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Persistence.MongoDB;
using NServiceBus.Persistence.MongoDB.DataBus;

class Program
{

    static void Main()
    {
        AsyncMain().GetAwaiter().GetResult();
    }

    static async Task AsyncMain()
    {
        Console.Title = "Samples.DataBus.Sender";
        var endpointConfiguration = new EndpointConfiguration("Samples.DataBus.Sender");
        endpointConfiguration.UseSerialization<JsonSerializer>();

        #region ConfigureDataBus

        var persistence = endpointConfiguration.UsePersistence<MongoDbPersistence>();
        persistence.SetConnectionString("mongodb://localhost:27017/SamplesMongoDBServer");
        var dataBus = endpointConfiguration.UseDataBus<MongoDbDataBus>();

        #endregion

        endpointConfiguration.EnableInstallers();
        endpointConfiguration.SendFailedMessagesTo("error");
        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
        try
        {
            Console.WriteLine("Press 'D' to send a databus large message");
            Console.WriteLine("Press 'N' to send a normal large message exceed the size limit and throw");
            Console.WriteLine("Press any other key to exit");

            while (true)
            {
                var key = Console.ReadKey();
                Console.WriteLine();

                if (key.Key == ConsoleKey.N)
                {
                    await SendMessageTooLargePayload(endpointInstance)
                        .ConfigureAwait(false);
                    continue;
                }

                if (key.Key == ConsoleKey.D)
                {
                    await SendMessageLargePayload(endpointInstance)
                        .ConfigureAwait(false);
                    continue;
                }
                return;
            }
        }
        finally
        {
            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }


    static async Task SendMessageLargePayload(IEndpointInstance endpointInstance)
    {
        #region SendMessageLargePayload

        var message = new MessageWithLargePayload
        {
            SomeProperty = "This message contains a large blob that will be sent on the data bus",
            LargeBlob = new DataBusProperty<byte[]>(new byte[1024*1024*5]) //5MB
        };
        await endpointInstance.Send("Samples.DataBus.Receiver", message)
            .ConfigureAwait(false);

        #endregion

        Console.WriteLine("Message sent");
    }

    static async Task SendMessageTooLargePayload(IEndpointInstance endpointInstance)
    {
        #region SendMessageTooLargePayload

        var message = new AnotherMessageWithLargePayload
        {
            LargeBlob = new byte[1024*1024*5] //5MB
        };
        await endpointInstance.Send("Samples.DataBus.Receiver", message)
            .ConfigureAwait(false);

        #endregion
    }
}