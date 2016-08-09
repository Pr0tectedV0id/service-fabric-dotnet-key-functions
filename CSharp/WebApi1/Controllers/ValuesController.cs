using Microsoft.ServiceFabric.Services.Remoting.Client;
using Stateful1;
using Stateless1;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.ServiceFabric.Actors.Client;
using Actor1.Interfaces;
using Microsoft.ServiceFabric.Actors;

namespace WebApi1.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values/getfromstatelessservice/1 
        public async Task<string> GetFromStatelessService(int id)
        {
            ServiceEventSource.Current.Message("Get invoked: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            var helloWorldClient = ServiceProxy.Create<ITestStatelessService>(new Uri("fabric:/Application1/Stateless1"));
            var message = await helloWorldClient.GetCount();
            return string.Format("[{0}] {1}: {2}", message.Time.ToString("yyyy-MM-dd HH:mm:ss"), message.Id, message.Count);
        }

        // GET api/values/getfromstatefulservice/5 
        public async Task<string> GetFromStatefulService(int partitionid)
        {
            var helloWorldClient = ServiceProxy.Create<ITestService>(new Uri("fabric:/Application1/Stateful1"), new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionid));
            var message = await helloWorldClient.GetCount();
            return message;
        }

		// GET api/values/settostatefulservice/[0-5]?value=123 
        [HttpGet]
        public async Task<string> SetToStatefulService(int partitionid, long value)
        {
            /* Explnation of partition key
             * if there are 2 partitions with key from 0-5.
             * 0-2 partition key will map to partition #1
             * 3-5 partition key will map to partition #2.
             * different partitions have different state.
             * which means if you set value 10 to any one of partition key 0-2, 
             * then partition #1 will have value 10 but partition #2 still have value 0
            */
            var helloWorldClient = ServiceProxy.Create<ITestService>(new Uri("fabric:/Application1/Stateful1"), new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionid));
            await helloWorldClient.SetCount(value);
            return String.Format("id: {0} has been set to {1}", partitionid, value);
        }


		// GET api/values/settoactor/1?value=123
        [HttpGet]
        public async Task<string> SetToActor(long actorid, int value)
        {
            var start = DateTime.Now;
            var actor = ActorProxy.Create<IActor1>(new ActorId(actorid), new Uri("fabric:/Application1/Actor1ActorService "));
            await actor.SetCountAsync(value);
            return String.Format(
                "{0}\n\nActor id: {1} has been set to {2}\n\n{3}",
                start.ToString("yyyy-MM-dd HH:mm:ss"), actorid, value, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

		// GET api/values/getfromactor/1
        [HttpGet]
        public async Task<string> GetFromActor(long actorid)
        {
            var actor = ActorProxy.Create<IActor1>(new ActorId(actorid), new Uri("fabric:/Application1/Actor1ActorService "));
            var value = await actor.GetCountAsync();
            return value.ToString();
        }
    }
}
