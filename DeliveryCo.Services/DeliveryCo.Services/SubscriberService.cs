using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Common.Model;
using DeliveryCo.Services.Interfaces;
using DeliveryCo.Business.Components.Interfaces;
using System.ServiceModel;
using Microsoft.Practices.ServiceLocation;


namespace DeliveryCo.Services
{
    public class SubscriberService : ISubscriberService
    {
        [OperationBehavior(TransactionScopeRequired = true)]
        public void PublishToSubscriber(Message pMessage)
        {
            DeliveryProvider.SubmitDelivery(pMessage);
        }
        private IDeliveryProvider DeliveryProvider
        {
            get { return ServiceLocator.Current.GetInstance<IDeliveryProvider>(); }
        }


    }
}
