using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using VideoStore.Services;
using System.ServiceModel.Configuration;
using System.Configuration;
using System.ComponentModel.Composition.Hosting;
using VideoStore.Services.Interfaces;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity.ServiceLocatorAdapter;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using VideoStore.Business.Entities;
using System.Transactions;
using System.ServiceModel.Description;
using VideoStore.Business.Components.Interfaces;
using VideoStore.WebClient.CustomAuth;
using System.Data.Entity;
using Common;

namespace VideoStore.Process
{
    public class Program
    {
        private static global::Common.SubscriberServiceHost mHost;
        private const String cAddress = "net.msmq://localhost/private/ToVideoStoreQueue";
        private const String cMexAddress = "net.tcp://localhost:9021/ToVideoStoreQueueService/mex";
        static void Main(string[] args)
        {

            HostSubscribeService();
            ResolveDependencies();
            InsertDummyEntities();
            

            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(1);

            var timer = new System.Threading.Timer((e) =>
            {
                checkOrderPaymentTimeout();
                
            }, null, startTimeSpan, periodTimeSpan);

            HostServices();

        }

 

        public static void checkOrderPaymentTimeout()
        { 
            using (TransactionScope lScope = new TransactionScope())
            using (VideoStoreEntityModelContainer lContainer = new VideoStoreEntityModelContainer())
            {
                List<Order> orders = lContainer.Orders.Include("OrderItems.Media.Stocks").Include("Customer.LoginCredential").Where((tOrder) => tOrder.OrderStatus == "unpayed").ToList();
                Console.WriteLine(String.Format("Check Order Payment Timeout, order list length: {0}", orders.Count));
                foreach (Order lOrder in orders)
                {
                    DateTime orderTime = lOrder.OrderDate;
                    DateTime now = DateTime.Now;
                    TimeSpan timeSpan = now.Subtract(orderTime);
                    double houres = timeSpan.TotalSeconds;
                    if (houres > 110.0)
                    {
                        lOrder.OrderStatus = "expired";
                        //LoadMediaStocks(lOrder);
                        MarkAppropriateUnchangedAssociations(lOrder);
                        lOrder.RevertStockLevels();
                        lContainer.Orders.ApplyChanges(lOrder);
                        
                        lContainer.SaveChanges();
                        lScope.Complete();
                    }
                }
            }
        }

        private static void MarkAppropriateUnchangedAssociations(Order pOrder)
        {
            pOrder.Customer.MarkAsUnchanged();
            pOrder.Customer.LoginCredential.MarkAsUnchanged();
            foreach (OrderItem lOrder in pOrder.OrderItems)
            {
                lOrder.Media.Stocks.MarkAsUnchanged();
                lOrder.Media.MarkAsUnchanged();
            }
        }

        private static void LoadMediaStocks(Order pOrder)
        {
            using (VideoStoreEntityModelContainer lContainer = new VideoStoreEntityModelContainer())
            {

                foreach (OrderItem lOrder in pOrder.OrderItems)
                {
                    //int orderItemID = lOrder.Id;
                    //OrderItem lOrderItem =  lContainer.OrderItems.Include("Media").Where((pOrderItem) => pOrderItem.Id == orderItemID).FirstOrDefault();
                    //int mediaID = lOrderItem.Media.Id;

                    //Media lMedia = lContainer.Media.Include("Stocks").Where((pMedia) => pMedia.Id == mediaID).FirstOrDefault();
                    //lOrder.Media = lMedia;
                    lOrder.Media.Stocks = lContainer.Stocks.Where((pStock) => pStock.Media.Id == lOrder.Media.Id).FirstOrDefault();
                }
            }
        }


        private static void HostSubscribeService()
        {
            //System.Diagnostics.Debug.WriteLine("Email Host Subscribe Service");
            mHost = new SubscriberServiceHost(typeof(SubscriberService), cAddress, cMexAddress, true, ".\\private$\\ToVideoStoreQueue");
            Console.WriteLine("Video Store Queue Hosted");
        }


        private static void InsertDummyEntities()
        {
            InsertCatalogueEntities();
            CreateOperator();
            CreateUser();
        }

        private static void CreateUser()
        {
            using (VideoStoreEntityModelContainer lContainer = new VideoStoreEntityModelContainer())
            {
                if (lContainer.Users.Where((pUser) => pUser.Name == "Customer").Count() > 0)
                    return;
            }

           
            User lCustomer = new User()
            {
                Name = "Customer",
                LoginCredential = new LoginCredential() { UserName = "Customer", Password = "COMP5348" },
                Email = "David@Sydney.edu.au",
                Address = "1 Central Park",
                BankAccountNumber = 456,
            };

            ServiceLocator.Current.GetInstance<IUserProvider>().CreateUser(lCustomer);
        }

        private static void InsertCatalogueEntities()
        {
            using (TransactionScope lScope = new TransactionScope())
            using (VideoStoreEntityModelContainer lContainer = new VideoStoreEntityModelContainer())
            {
                if (lContainer.Media.Count() == 0)
                {
                    Media lGreatExpectations = new Media()
                    {
                        Director = "Rene Clair",
                        Genre = "Fiction",
                        Price = 20.0,
                        Title = "And Then there were None"
                    };

                    lContainer.Media.AddObject(lGreatExpectations);


                    Stock lGreatExpectationsStock = new Stock()
                    {
                        Media = lGreatExpectations,
                        Quantity = 5,
                        Warehouse = "Neutral Bay"
                    };

                    lContainer.Stocks.AddObject(lGreatExpectationsStock);

                    Media lSoloist = new Media()
                    {
                        Director = "The Soloist",
                        Genre = "Fiction",
                        Price = 15.0,
                        Title = "The Soloist"
                    };

                    lContainer.Media.AddObject(lSoloist);

                    Stock lSoloistStock = new Stock()
                    {
                        Media = lSoloist,
                        Quantity = 7,
                        Warehouse = "Neutral Bay"
                    };

                    lContainer.Stocks.AddObject(lSoloistStock);

                    for (int i = 0; i < 10; i++)
                    {
                        Media lItem = new Media()
                        {
                            Director = String.Format("Director {0}", i.ToString()),
                            Genre = String.Format("Genre {0}", i),
                            Price = i,
                            Title = String.Format("Title {0}", i)
                        };

                        lContainer.Media.AddObject(lItem);

                        Stock lStock = new Stock()
                        {
                            Media = lItem,
                            Quantity = 7,
                            Warehouse = String.Format("Warehouse {0}", i)
                        };

                        lContainer.Stocks.AddObject(lStock);
                    }


                    lContainer.SaveChanges();
                    lScope.Complete();
                }

                

            }

        }

   

        private static void CreateOperator()
        {
            Role lOperatorRole = new Role() { Name = "Operator" };
            using (VideoStoreEntityModelContainer lContainer = new VideoStoreEntityModelContainer())
            {
                if (lContainer.Roles.Count() > 0)
                {
                    return;
                }
            }
            User lOperator = new User()
            {
                Name = "Operator",
                LoginCredential = new LoginCredential() { UserName = "Operator", Password = "COMP5348" },
                Email = "Wang@Sydney.edu.au",
                Address = "1 Central Park"
            };

            lOperator.Roles.Add(lOperatorRole);

            ServiceLocator.Current.GetInstance<IUserProvider>().CreateUser(lOperator);
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


        private static void HostServices()
        {
            List<ServiceHost> lHosts = new List<ServiceHost>();
            try
            {

                Configuration lAppConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                ServiceModelSectionGroup lServiceModel = ServiceModelSectionGroup.GetSectionGroup(lAppConfig);

                System.ServiceModel.Configuration.ServicesSection lServices = lServiceModel.Services;
                foreach (ServiceElement lServiceElement in lServices.Services)
                {
                    ServiceHost lHost = new ServiceHost(Type.GetType(GetAssemblyQualifiedServiceName(lServiceElement.Name)));
                    lHost.Open();
                    lHosts.Add(lHost);
                }
                Console.WriteLine("VideoStore Service Started, press Q key to quit");
                while (Console.ReadKey().Key != ConsoleKey.Q) ;
            }
            finally
            {
                foreach (ServiceHost lHost in lHosts)
                {
                    lHost.Close();
                }
            }
        }

        private static String GetAssemblyQualifiedServiceName(String pServiceName)
        {
            return String.Format("{0}, {1}", pServiceName, System.Configuration.ConfigurationManager.AppSettings["ServiceAssemblyName"].ToString());
        }
    }
}
