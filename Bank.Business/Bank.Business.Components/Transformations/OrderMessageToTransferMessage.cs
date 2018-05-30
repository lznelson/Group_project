using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Common.Model;

namespace Bank.Business.Components.Transformations
{
    public class OrderMessageToTransferMessage: IVisitor
    {
        public TransferMessage Result { get; set; }

        public void Visit(IVisitable pVisitable)
        {
            if (pVisitable is OrderMessage)
            {
                OrderMessage orderMessage = pVisitable as OrderMessage;
                Result = new TransferMessage()
                {
                    OrderGuid = orderMessage.OrderGuid,
                    Total = orderMessage.Total,
                    FromAccountNumber = orderMessage.FromAccountNumber,
                    ToAccountNumber = orderMessage.ToAccountNumber,
                    Topic = "bank"
                };
            }
        }
    }
}
