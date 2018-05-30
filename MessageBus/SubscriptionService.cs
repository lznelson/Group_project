using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MessageBus.Interfaces;
using System.ServiceModel;
using Common.Model;

namespace MessageBus
{


    public class SubscriptionService : ISubscriptionService
    {
        private const String emailAddress = "net.msmq://localhost/private/ToEmailQueue";
        private const String bankAddress = "net.msmq://localhost/private/ToBankQueue";
        private const String deliveryAddress = "net.msmq://localhost/private/ToDeliveryQueue";
        private const String videoAddress = "net.msmq://localhost/private/ToVideoStoreQueue";
        public void AddToSubscriptionRegistry()
        {
            Subscribe("email", emailAddress);
            Subscribe("bank", bankAddress);
            Subscribe("delivery", deliveryAddress);
            Subscribe("videostore", videoAddress);
        }
        public void Subscribe(string pTopic, string pCaller)
        {
            Console.WriteLine("Subscription for " + pTopic + " received");
            SubscriptionRegistry.Instance.AddSubscription(pTopic, pCaller);
        }

        public void Unsubscribe(string pTopic, string pCaller)
        {
            SubscriptionRegistry.Instance.RemoveSubscription(pTopic, pCaller);
        }
    }
}
