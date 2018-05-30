using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace DeliveryCo.Business.Entities.Partial
{
    public partial class DeliveryInfo : IVisitable
    {
        public void Accept(IVisitor pVisitor)
        {
            pVisitor.Visit(this);
        }
    }
}
