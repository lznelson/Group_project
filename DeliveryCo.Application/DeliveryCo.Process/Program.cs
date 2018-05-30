using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using DeliveryCo.Services;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Microsoft.Practices.Unity.ServiceLocatorAdapter;
using Microsoft.Practices.ServiceLocation;
using System.Configuration;
using Common;

namespace DeliveryCo.Process
{


    class Program
    {
        private static global::Common.SubscriberServiceHost mHost;
        private const String cAddress = "net.msmq://localhost/private/ToDeliveryQueue";
        private const String cMexAddress = "net.tcp://localhost:9019/ToDeliveryQueueService/mex";

        static void Main(string[] args)
        {
            ResolveDependencies();
            HostSubscribeService();

        }
        private static void HostSubscribeService()
        {
            System.Diagnostics.Debug.WriteLine("Bank Host Subscribe Service");
            mHost = new SubscriberServiceHost(typeof(SubscriberService), cAddress, cMexAddress, true, ".\\private$\\ToDeliveryQueue");
            Console.WriteLine("Delivery Services started. Press Q to quit.");
            while (Console.ReadKey().Key != ConsoleKey.Q) ;
        }

        private static void ResolveDependencies()
        {

            UnityContainer lContainer = new UnityContainer();
            UnityConfigurationSection lSection
                    = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
            lSection.Containers["containerOne"].Configure(lContainer);
            UnityServiceLocator locator = new UnityServiceLocator(lContainer);
            ServiceLocator.SetLocatorProvider(() => locator);
        }
    }
}
