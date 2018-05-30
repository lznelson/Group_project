using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeliveryCo.Business.Components.Interfaces;
using System.Transactions;
using DeliveryCo.Business.Entities;
using System.Threading;
using DeliveryCo.Services.Interfaces;
using Common;
using Common.Model;
using System.ServiceModel;
using DeliveryCo.Business.Components.PublisherService;


namespace DeliveryCo.Business.Components
{
    public class DeliveryProvider : IDeliveryProvider
    {
        //public Guid SubmitDelivery(DeliveryCo.Business.Entities.DeliveryInfo pDeliveryInfo)
        //{
        //    using(TransactionScope lScope = new TransactionScope())
        //    using(DeliveryDataModelContainer lContainer = new DeliveryDataModelContainer())
        //    {
        //        pDeliveryInfo.DeliveryIdentifier = Guid.NewGuid();
        //        pDeliveryInfo.Status = 0;
        //        lContainer.DeliveryInfoes.AddObject(pDeliveryInfo);
        //        lContainer.SaveChanges();
        //        ThreadPool.QueueUserWorkItem(new WaitCallback((pObj) => ScheduleDelivery(pDeliveryInfo)));
        //        lScope.Complete();
        //    }
        //    return pDeliveryInfo.DeliveryIdentifier;
        //}

        public void SubmitDelivery(Message message)
        {
            //Specify the binding to be used for the client.
            NetMsmqBinding binding = new NetMsmqBinding(NetMsmqSecurityMode.None);

            //Specify the address to be used for the client.
            EndpointAddress address =
               new EndpointAddress("net.msmq://localhost/private/PublisherMessageQueue");
            PublisherServiceClient lClient = new PublisherServiceClient(binding, address);

            using (TransactionScope lScope = new TransactionScope())
            using (DeliveryDataModelContainer lContainer = new DeliveryDataModelContainer())
            {
                DeliveryMessage mDeliveryMessage = message as DeliveryMessage;
                mDeliveryMessage.DeliveryIdentifier = Guid.NewGuid();
                mDeliveryMessage.Status = 0;
                mDeliveryMessage.Topic = "delivery";
                DeliveryInfo pDeliveryInfo = new DeliveryInfo()
                {
                    OrderNumber = mDeliveryMessage.OrderNumber,
                    SourceAddress = mDeliveryMessage.SourceAddress,
                    DestinationAddress = mDeliveryMessage.DestinationAddress,
                    DeliveryNotificationAddress = "net.msmq://localhost/private/DeliveryNotificationQueue"
                };
                lContainer.DeliveryInfoes.AddObject(pDeliveryInfo);
                lContainer.SaveChanges();
                //lClient.Publish(mDeliveryMessage);
                //Console.WriteLine("Delivery submitted and planing deliver to " + pDeliveryInfo.DestinationAddress);
                ThreadPool.QueueUserWorkItem(new WaitCallback((pObj) => ScheduleDelivery(pDeliveryInfo, mDeliveryMessage.DeliveryIdentifier, lClient)));


                lScope.Complete();
            }
        }

        private void ScheduleDelivery(DeliveryInfo pDeliveryInfo, Guid pDeliveryIdentifier, PublisherServiceClient lClient)
        {
            
            Thread.Sleep(10000);
            Console.WriteLine("Order: " + pDeliveryInfo.DestinationAddress + " Delivered at " + DateTime.Now.ToString());
            //notifying of delivery completion
            using (TransactionScope lScope = new TransactionScope())
            using (DeliveryDataModelContainer lContainer = new DeliveryDataModelContainer())
            {
                pDeliveryInfo.Status = 1;
                //IDeliveryNotificationService lService = DeliveryNotificationServiceFactory.GetDeliveryNotificationService(pDeliveryInfo.DeliveryNotificationAddress);
                //lService.NotifyDeliveryCompletion(pDeliveryInfo.DeliveryIdentifier, DeliveryInfoStatus.Delivered);
                DeliveryMessage mDeliveryMessage = new DeliveryMessage()
                {
                    OrderNumber = pDeliveryInfo.OrderNumber,
                    DeliveryIdentifier = pDeliveryIdentifier,
                    Status = 1,
                    Topic = "delivery"
                };
                lClient.Publish(mDeliveryMessage);
                lScope.Complete();
            }

        }
    }
}
