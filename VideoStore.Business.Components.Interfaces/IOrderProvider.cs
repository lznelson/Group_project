using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VideoStore.Business.Entities;
using Common.Model;

namespace VideoStore.Business.Components.Interfaces
{
    public interface IOrderProvider
    {
        void SubmitOrder(Order pOrder);

        void ChangeOrderPaymentStatus(Message pMessage);
    }
}
