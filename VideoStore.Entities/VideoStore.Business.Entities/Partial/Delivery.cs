using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace VideoStore.Business.Entities
{
    public enum DeliveryStatus { Submitted, Delivered, Failed }

    public partial class Delivery : IVisitable
    {
        public DeliveryStatus DeliveryStatus
        {
            get
            {
                return (DeliveryStatus)this.Status;
            }
            set
            {
                this.Status = (int)value;
            }
        }

        public void Accept(IVisitor pVisitor)
        {
            pVisitor.Visit(this);
        }
    }
}
