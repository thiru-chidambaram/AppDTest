using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Shared;
using Shared.MassTransit;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly BusHost _busHost;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public ValuesController(BusHost busHost)
        {
            _busHost = busHost;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            Log.Info("WebApi Service: Get All Values");

            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(int id)
        {
            Log.Info("WebApi Service: Get Values: {0}", id);
            var request = new GetValuesRequestMessage
            {
                Index = id
            };

            try
            {
                var endpoint = new Uri($"rabbitmq://localhost/{Constants.ServiceQueue}");
                var client = new MessageRequestClient<GetValuesRequestMessage, GetValuesResponseMessage>(_busHost.Bus, endpoint, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));
                var response = await client.Request(request);
                return response?.Message;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error getting value");
                return e.Message;
            }
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
