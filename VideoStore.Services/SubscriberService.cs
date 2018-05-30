using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.ServiceModel;
using VideoStore.Services.Interfaces;
using VideoStore.Business.Components.Interfaces;
using Common.Model;

namespace VideoStore.Services
{
    public class SubscriberService : ISubscriberService
    {
        private IOrderProvider OrderProvider
        {
            get
            {
                return ServiceFactory.GetService<IOrderProvider>();
            }
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public void PublishToSubscriber(Message pMessage)
        {
            Console.WriteLine("Receive message");
            OrderProvider.ChangeOrderPaymentStatus(pMessage);
        }
    }
}