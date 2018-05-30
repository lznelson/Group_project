using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MessageBus.Interfaces;
using Common.Model;
using System.ServiceModel;
using Common;
using MessageBus.Entities;
using System.Data;
using Bank.Business.Components.Transformations;
using System.Transactions;

namespace MessageBus
{
    public class PublisherService : IPublisherService
    {
        public void Publish(Message pMessage)
        {
            //Console.WriteLine("Publish queue-------- Topic: " + pMessage.Topic);
            String topicFrom = pMessage.Topic;
            String forwardAddress = "";
      
            //if(topicFrom == "bank")
            //{
            //    forwardAddress = "bank";
            //}
            if(topicFrom == "bank")
            {
                
                TransferMessage transferMessage = pMessage as TransferMessage;
                bool bTransferResult = transferMessage.BTransfer;
                if (bTransferResult)
                {
                    forwardAddress = "delivery";
                    using(MessageBusEntitiesModelContainer lContainer = new MessageBusEntitiesModelContainer())
                    {
                        DeliveryMessage deliveryMessage = new DeliveryMessage()
                        {
                            Topic = "delivery",
                            OrderNumber = transferMessage.OrderGuid.ToString(),
                            SourceAddress = "Video Store Address",
                            DestinationAddress = lContainer.Orders.Where((pOrder) => pOrder.OrderNumber == transferMessage.OrderGuid).FirstOrDefault().DestinationAddress
                        };
                        pMessage = deliveryMessage;

                        EmailMessage emailMessage = new EmailMessage()
                        {
                            Topic = "email",
                            EmailAddress = lContainer.Orders.Where((pOrder) => pOrder.OrderNumber == transferMessage.OrderGuid).FirstOrDefault().EmailAddress,
                            EmailContent = "Order: " + transferMessage.OrderGuid + " is submitted at " + DateTime.Now.ToString()
                        };

                        PublishMessage(emailMessage, "email");
                    }


                    // Inform Video Store Transfer Success
                    PublishMessage(transferMessage, "videostore");

                    Console.WriteLine("MessageBus: ----------Bank to Delivery Co and Video Store");
                }
                else
                {
                    PublishMessage(transferMessage, "videostore");
                    forwardAddress = "email";
                    using (MessageBusEntitiesModelContainer lContainer = new MessageBusEntitiesModelContainer())
                    {
                        Guid orderGuid = transferMessage.OrderGuid;
                        EmailMessage mEmailMessage = new EmailMessage()
                        {
                            EmailAddress = lContainer.Orders.Where((pOrder) => pOrder.OrderNumber == orderGuid).FirstOrDefault().EmailAddress,
                            EmailContent = "Your order " + orderGuid + " can not be processed, please chekc your credit."
                        };
                        pMessage = mEmailMessage;
                        Console.WriteLine("MessageBus: ----------Bank to Messagebus to email");
                    }

                }
                
            }
            if(topicFrom == "delivery")
            {
                forwardAddress = "email";
                if(pMessage is DeliveryMessage)
                {
                    Console.WriteLine("MessageBus: ----------Delivery Co to Video Store");
                    DeliveryMessage mDeliveryMessage = pMessage as DeliveryMessage;
                    int deliveryStatus = mDeliveryMessage.Status;
                    
                    using (MessageBusEntitiesModelContainer lContainer = new MessageBusEntitiesModelContainer())
                    {
                        Guid orderGuid = new Guid(mDeliveryMessage.OrderNumber);
                        EmailMessage mEmailMessage = new EmailMessage()
                        {
                            EmailAddress = lContainer.Orders.Where((pOrder) => pOrder.OrderNumber == orderGuid).FirstOrDefault().EmailAddress,
                            //EmailContent = "Your order " + mDeliveryMessage.OrderNumber + " has been placed"
                        };
                        // Submited
                        if (deliveryStatus == 0)
                        {
                            mEmailMessage.EmailContent = "Your order " + mDeliveryMessage.OrderNumber + " has been placed";
                       
                        }
                        // Delivered
                        if (deliveryStatus == 1)
                        {
                            mEmailMessage.EmailContent = "Your order " + mDeliveryMessage.OrderNumber + " has been delivered";
                        }
                        pMessage = mEmailMessage;
                    }

                }
            }
            if(topicFrom == "videostore")
            {
                if(pMessage is OrderMessage)
                {
                    Console.WriteLine("MessageBus: ----------Video Store to Bank");
                    OrderMessage orderMessage = pMessage as OrderMessage;
                    SaveOrderToDataBase(orderMessage);
                    OrderMessageToTransferMessage lVisitor = new OrderMessageToTransferMessage();
                    lVisitor.Visit(orderMessage);
                    pMessage = lVisitor.Result;
                    forwardAddress = "bank";
                }
            }

            //foreach (String lHandlerAddress in SubscriptionRegistry.Instance.GetTopicSubscribers(forwardAddress))
            //{
            //    ISubscriberService lSubServ = ServiceFactory.GetService<ISubscriberService>(lHandlerAddress);
            //    Console.WriteLine("Handler Address:" + lHandlerAddress);
            //    lSubServ.PublishToSubscriber(pMessage);
            //}
            PublishMessage(pMessage, forwardAddress);
        }

        private void PublishMessage(Message message, String forwardAddress)
        {
            using (TransactionScope lScope = new TransactionScope())
            {
                foreach (String lHandlerAddress in SubscriptionRegistry.Instance.GetTopicSubscribers(forwardAddress))
                {
                    ISubscriberService lSubServ = ServiceFactory.GetService<ISubscriberService>(lHandlerAddress);
                    Console.WriteLine("Handler Address:" + lHandlerAddress);
                    lSubServ.PublishToSubscriber(message);
                    lScope.Complete();
                }
            }

        }
        //private void PlaceDeliveryForOrder(Message message)
        //{
            

        //    Delivery lDelivery = new Delivery() { DeliveryStatus = DeliveryStatus.Submitted, SourceAddress = "Video Store Address", DestinationAddress = pOrder.Customer.Address, Order = pOrder };

        //    Guid lDeliveryIdentifier = ExternalServiceFactory.Instance.DeliveryService.SubmitDelivery(new DeliveryInfo()
        //    {
        //        OrderNumber = lDelivery.Order.OrderNumber.ToString(),
        //        SourceAddress = lDelivery.SourceAddress,
        //        DestinationAddress = lDelivery.DestinationAddress,
        //        DeliveryNotificationAddress = "net.tcp://localhost:9010/DeliveryNotificationService"
        //    });

        //    lDelivery.ExternalDeliveryIdentifier = lDeliveryIdentifier;
        //    pOrder.Delivery = lDelivery;

        //}

        private void SaveOrderToDataBase(OrderMessage orderMessage)
        {
            using (MessageBusEntitiesModelContainer lContainer = new MessageBusEntitiesModelContainer())
            {
                Order order = new Order()
                {
                    OrderNumber = orderMessage.OrderGuid,
                    SourceAddress = "Video Store Address",
                    DestinationAddress = orderMessage.DestinationAddress,
                    EmailAddress = orderMessage.CustomerEmailAddress
                };
                lContainer.Orders.Add(order);
                lContainer.SaveChanges();
            }
        }

        
    }
}
