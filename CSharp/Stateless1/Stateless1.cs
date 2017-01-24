using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using System.IO;

namespace Stateless1
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class Stateless1 : StatelessService, ITestStatelessService
    {
        private Guid serviceId = Guid.NewGuid();

        public Stateless1(StatelessServiceContext context)
            : base(context)
        { }

        public Task<TestCount> GetCount()
        {
            return Task.Run<TestCount>(() =>
            {
                return new TestCount
                {
                    Count = new Random().Next(1000),
                    Id = serviceId.ToString(),
                    Time = DateTime.Now
                };
            });
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] { new ServiceInstanceListener(context =>
            this.CreateServiceRemotingListener(context)) };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }

        protected override void OnAbort()
        {
            File.AppendAllText(@"%TEMP%/Stateless1.log", 
                string.Format("[{0}] {1} : Abort\r\n", DateTime.Now.ToString("yyyy-MM-dd ")));
            base.OnAbort();
        }

        protected override Task OnCloseAsync(CancellationToken cancellationToken)
        {
            File.AppendAllText(@"%TEMP%/Stateless1.log",
                string.Format("[{0}] {1} : Close\r\n", DateTime.Now.ToString("yyyy-MM-dd ")));
            return base.OnCloseAsync(cancellationToken);
        }
    }
    public interface ITestStatelessService : IService
    {
        Task<TestCount> GetCount();
    }

    public class TestCount
    {
        public DateTime Time { get; set; }
        public string Id { get; set; }
        public int Count { get; set; }
    }
}
