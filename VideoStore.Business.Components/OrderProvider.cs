using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VideoStore.Business.Components.Interfaces;
using VideoStore.Business.Entities;
using System.Transactions;
using Microsoft.Practices.ServiceLocation;
using DeliveryCo.MessageTypes;
using VideoStore.Business.Components.PublisherService;
using System.ServiceModel;
using Common.Model;

namespace VideoStore.Business.Components
{
    public class OrderProvider : IOrderProvider
    {
        public IEmailProvider EmailProvider
        {
            get { return ServiceLocator.Current.GetInstance<IEmailProvider>(); }
        }

        public IUserProvider UserProvider
        {
            get { return ServiceLocator.Current.GetInstance<IUserProvider>(); }
        }

        public void SubmitOrder(Entities.Order pOrder)
        {
           
            using (TransactionScope lScope = new TransactionScope())
            {
                LoadMediaStocks(pOrder);
                MarkAppropriateUnchangedAssociations(pOrder);
                using (VideoStoreEntityModelContainer lContainer = new VideoStoreEntityModelContainer())
                {
                    try
                    {
                        //Specify the binding to be used for the client.
                        NetMsmqBinding binding = new NetMsmqBinding(NetMsmqSecurityMode.None);

                        //Specify the address to be used for the client.
                        EndpointAddress address =
                           new EndpointAddress("net.msmq://localhost/private/PublisherMessageQueue");

                        pOrder.OrderNumber = Guid.NewGuid();
                        OrderToOrderMessage lVisitor = new OrderToOrderMessage(UserProvider.ReadUserById(pOrder.Customer.Id).BankAccountNumber, RetrieveVideoStoreAccountNumber());
                        lVisitor.Visit(pOrder);

                        PublisherServiceClient lClient = new PublisherServiceClient(binding, address);
                        lClient.Publish(lVisitor.Result);
                        //TransferFundsFromCustomer(UserProvider.ReadUserById(pOrder.Customer.Id).BankAccountNumber, pOrder.Total ?? 0.0);

                        pOrder.UpdateStockLevels();
                        pOrder.OrderStatus = "unpayed";
                        //PlaceDeliveryForOrder(pOrder);
                        lContainer.Orders.ApplyChanges(pOrder);
                        lContainer.SaveChanges();
                        lScope.Complete();
                        
                    }
                    catch (Exception lException)
                    {
                        // SendOrderErrorMessage(pOrder, lException);
                        
                        // throw;
                    }
                }
            }
            
        }

        public void ChangeOrderPaymentStatus(Message pMessage)
        {
            TransferMessage tMessage = pMessage as TransferMessage;
            
            Guid pOrderNumber = tMessage.OrderGuid;
            Console.WriteLine("change ----- - - - --"+pOrderNumber);
            using(TransactionScope lScope = new TransactionScope())
            {
                using(VideoStoreEntityModelContainer lContainer = new VideoStoreEntityModelContainer())
                {
                    
                    Order order = lContainer.Orders.Include("OrderItems.Media.Stocks").Include("Customer.LoginCredential").Where((tOrder) => tOrder.OrderNumber == pOrderNumber).FirstOrDefault();
                    //LoadMediaStocks(order);
                    MarkAppropriateUnchangedAssociations(order);
                    Console.WriteLine("Change status");
                    if (tMessage.BTransfer)
                    {
                        order.OrderStatus = "payed";
                    }else
                    {
                        Console.WriteLine("revert stock");
                        order.OrderStatus = "fail";
                        order.RevertStockLevels();
                    }
                        
                    lContainer.Orders.ApplyChanges(order);
                    lContainer.SaveChanges();
                    lScope.Complete();

                }
            }
        }



        private void MarkAppropriateUnchangedAssociations(Order pOrder)
        {
            pOrder.Customer.MarkAsUnchanged();
            pOrder.Customer.LoginCredential.MarkAsUnchanged();
            foreach (OrderItem lOrder in pOrder.OrderItems)
            {
                lOrder.Media.Stocks.MarkAsUnchanged();
                lOrder.Media.MarkAsUnchanged();
            }
        }

        private void LoadMediaStocks(Order pOrder)
        {
            using (VideoStoreEntityModelContainer lContainer = new VideoStoreEntityModelContainer())
            {
            
                foreach (OrderItem lOrder in pOrder.OrderItems)
                {
                    lOrder.Media.Stocks = lContainer.Stocks.Where((pStock) => pStock.Media.Id == lOrder.Media.Id).FirstOrDefault();    
                }
            }
        }

        

        //private void SendOrderErrorMessage(Order pOrder, Exception pException)
        //{
        //    EmailProvider.SendMessage(new EmailMessage()
        //    {
        //        ToAddress = pOrder.Customer.Email,
        //        Message = "There was an error in processsing your order " + pOrder.OrderNumber + ": "+ pException.Message +". Please contact Video Store"
        //    });
        //}

        //private void SendOrderPlacedConfirmation(Order pOrder)
        //{
        //    EmailProvider.SendMessage(new EmailMessage()
        //    {
        //        ToAddress = pOrder.Customer.Email,
        //        Message = "Your order " + pOrder.OrderNumber + " has been placed"
        //    });
        //}

        private void PlaceDeliveryForOrder(Order pOrder)
        {
            Delivery lDelivery = new Delivery() { DeliveryStatus = DeliveryStatus.Submitted, SourceAddress = "Video Store Address", DestinationAddress = pOrder.Customer.Address, Order = pOrder };

            //Guid lDeliveryIdentifier = ExternalServiceFactory.Instance.DeliveryService.SubmitDelivery(new DeliveryInfo()
            //{ 
            //    OrderNumber = lDelivery.Order.OrderNumber.ToString(),  
            //    SourceAddress = lDelivery.SourceAddress,
            //    DestinationAddress = lDelivery.DestinationAddress,
            //    DeliveryNotificationAddress = "net.tcp://localhost:9010/DeliveryNotificationService"
            //});

            //lDelivery.ExternalDeliveryIdentifier = lDeliveryIdentifier;
            //pOrder.Delivery = lDelivery;
            
        }

        private void TransferFundsFromCustomer(int pCustomerAccountNumber, double pTotal)
        {
            try
            {
                ExternalServiceFactory.Instance.TransferService.Transfer(pTotal, pCustomerAccountNumber, RetrieveVideoStoreAccountNumber());
            }
            catch(Exception e)
            {
                throw new Exception("Error Transferring funds for order.");
            }
        }


        private int RetrieveVideoStoreAccountNumber()
        {
            return 123;
        }


    }
}
