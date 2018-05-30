using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Common.Model;
using Bank.Services.Interfaces;
using Bank.Business.Components.Interfaces;
using System.ServiceModel;
using Microsoft.Practices.ServiceLocation;


namespace Bank.Services
{
    public class SubscriberService : ISubscriberService
    {
        [OperationBehavior(TransactionScopeRequired = true)]
        public void PublishToSubscriber(Message pMessage)
        {
            TransferProvider.Transfer(pMessage);
        }
        private ITransferProvider TransferProvider
        {
            get { return ServiceLocator.Current.GetInstance<ITransferProvider>(); }
        }


    }
}
