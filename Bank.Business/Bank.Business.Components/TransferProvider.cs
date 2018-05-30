using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bank.Business.Components.Interfaces;
using Bank.Business.Entities;
using System.Transactions;
using Bank.Services.Interfaces;
using Common.Model;
using Common;
using Bank.Business.Components.PublisherService;
using System.ServiceModel;

namespace Bank.Business.Components
{
    public class TransferProvider : ITransferProvider
    {

        //public void Transfer(double pAmount, int pFromAcctNumber, int pToAcctNumber)
        public void Transfer(Message message)
        {
            using (TransactionScope lScope = new TransactionScope())
            using (BankEntityModelContainer lContainer = new BankEntityModelContainer())
            {
                //Specify the binding to be used for the client.
                NetMsmqBinding binding = new NetMsmqBinding(NetMsmqSecurityMode.None);

                //Specify the address to be used for the client.
                EndpointAddress address =
                   new EndpointAddress("net.msmq://localhost/private/PublisherMessageQueue");
                PublisherServiceClient lClient = new PublisherServiceClient(binding, address);

                try
                {
                    TransferMessage transferMessage = message as TransferMessage;
                    double pAmount = transferMessage.Total;
                    int pFromAcctNumber = transferMessage.FromAccountNumber;
                    int pToAcctNumber = transferMessage.ToAccountNumber;
                    Account lFromAcct = GetAccountFromNumber(pFromAcctNumber);
                    Account lToAcct = GetAccountFromNumber(pToAcctNumber);
                    lFromAcct.Withdraw(pAmount);
                    lToAcct.Deposit(pAmount);
                    lContainer.Attach(lFromAcct);
                    lContainer.Attach(lToAcct);
                    lContainer.ObjectStateManager.ChangeObjectState(lFromAcct, System.Data.EntityState.Modified);
                    lContainer.ObjectStateManager.ChangeObjectState(lToAcct, System.Data.EntityState.Modified);
                    lContainer.SaveChanges();
                    Console.WriteLine("Amount: " + pAmount + " FromAccount: " + pFromAcctNumber + " ToAccount: " + pToAcctNumber);
                    // Return successful result
                    TransferMessage result = new TransferMessage()
                    {
                        Topic = "bank",
                        OrderGuid = transferMessage.OrderGuid,
                        BTransfer = true
                    };
                    //HostServiceAndPublishMessage(result);
                    lClient.Publish(result);
                    lScope.Complete();
  
                }
                catch (Exception lException)
                {
                    // Return Failure result
                    TransferMessage transferMessage = message as TransferMessage;
                    TransferMessage result = new TransferMessage()
                    {
                        Topic = "bank",
                        OrderGuid = transferMessage.OrderGuid,
                        BTransfer = false,
                        Comment = lException.Message
                    };
                    // HostServiceAndPublishMessage(result);
                    lClient.Publish(result);
                    lScope.Complete();
                    Console.WriteLine("Error occured while transferring money:  " + lException.Message);
                    //throw;

                }
            }
        }

        private void HostServiceAndPublishMessage(TransferMessage message)
        {
            //Specify the binding to be used for the client.
            NetMsmqBinding binding = new NetMsmqBinding(NetMsmqSecurityMode.None);

            //Specify the address to be used for the client.
            EndpointAddress address =
               new EndpointAddress("net.msmq://localhost/private/PublisherMessageQueue");
            PublisherServiceClient lClient = new PublisherServiceClient(binding, address);
            lClient.Publish(message);
        }
        private Account GetAccountFromNumber(int pToAcctNumber)
        {
            using (BankEntityModelContainer lContainer = new BankEntityModelContainer())
            {
                return lContainer.Accounts.Where((pAcct) => (pAcct.AccountNumber == pToAcctNumber)).FirstOrDefault();
            }
        }
    }
}
