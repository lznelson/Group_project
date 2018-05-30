using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Common.Model;
using DeliveryCo.Business.Entities;

namespace DeliveryCo.Business.Components
{
    public class DeliveryInfoToDeliveryMessage : IVisitor
    {
        public DeliveryMessage Result { get; set; }

        public void Visit(IVisitable pVisitable)
        {
            if (pVisitable is DeliveryInfo)
            {
                DeliveryInfo deliveryInfo = pVisitable as DeliveryInfo;
                Result = new DeliveryMessage()
                {

                };
            }
        }
    }
}
