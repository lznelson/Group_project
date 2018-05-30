using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Model;

namespace DeliveryCo.Business.Components.Interfaces
{
    public interface IDeliveryProvider
    {
        void SubmitDelivery(Message message);
    }
}
