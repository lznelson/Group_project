using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Messaging;
using System.ServiceModel.Description;
using MessageBus;
using MessageBus.Entities;
using Common.Model;

namespace MessageBus
{
    class Program
    {
        private static readonly String sPublishQueuePath = ".\\private$\\PublisherMessageQueue";

        static void Main(string[] args)
        {

            SubscriptionService service = new SubscriptionService();
            service.AddToSubscriptionRegistry();

            EnsureMessageQueuesExists();
            using (ServiceHost lPubHost = new ServiceHost(typeof(PublisherService)))
            using (ServiceHost lSubHost = new ServiceHost(typeof(SubscriptionService)))
            {
                AttachMessageInspectorToHost(lPubHost);
                //AttachMessageInspectorToHost(lSubHost);
                lPubHost.Open();
                lSubHost.Open();
                Console.WriteLine("Message Bus Started Press Q to quit");
                while (Console.ReadKey().Key != ConsoleKey.Q) ;
            }
        }

        private void HostForwardingService()
        {

        }

        private static void EnsureMessageQueuesExists()
        {
            // Create the transacted MSMQ queue if necessary.
            if (!MessageQueue.Exists(sPublishQueuePath))
                MessageQueue.Create(sPublishQueuePath, true);
        }

        private static void AttachMessageInspectorToHost(ServiceHost pHost)
        {
            ServiceMessageInspector lInspector = new ServiceMessageInspector();
            foreach (ServiceEndpoint lEndPoint in pHost.Description.Endpoints)
            {
                //System.Diagnostics.Debug.WriteLine(lEndPoint.Address);
                lEndPoint.Behaviors.Add(lInspector);
            }
        }

    }
}
