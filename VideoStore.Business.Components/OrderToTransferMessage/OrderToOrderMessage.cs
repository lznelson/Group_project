using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Model;
using VideoStore.Business.Entities;

namespace VideoStore.Business.Components
{
    public class OrderToOrderMessage: IVisitor
    {
        private int mFromAccountNumber;
        private int mToAccountNumber;

        public OrderToOrderMessage(int pFromAccountNumber, int pToAccountNumber)
        {
            mFromAccountNumber = pFromAccountNumber;
            mToAccountNumber = pToAccountNumber;

        }

        public OrderMessage Result { get; set; }

        public void Visit(IVisitable pVisitable)
        {
            if(pVisitable is Order)
            {
                Order lOrder = pVisitable as Order;
                Result = new OrderMessage()
                {
                    OrderGuid = lOrder.OrderNumber,
                    FromAccountNumber = mFromAccountNumber,
                    ToAccountNumber = mToAccountNumber,
                    CustomerEmailAddress = lOrder.Customer.Email,
                    DestinationAddress = lOrder.Customer.Address,
                    SourceAddress = "Video Store Address",
                    Total = lOrder.Total ?? 0,
                    Topic = "videostore"
                };
            }
        }
    }
}
