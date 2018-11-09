using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using NLog;
using Shared;

namespace WorkerService
{
    public class Consumer: IConsumer, IConsumer<GetValuesRequestMessage>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private Random _random = new Random();

        public async Task Consume(ConsumeContext<GetValuesRequestMessage> context)
        {
            var response = new GetValuesResponseMessage
            {
                Message = $"Value of Index: {context.Message.Index} = {_random.Next(0, 100)}"
            };            

            await context.RespondAsync(response);

            Log.Info("Received GetValues Message: Index: {0}, Response: {1}", context.Message.Index, response.Message);
        }
    }
}
