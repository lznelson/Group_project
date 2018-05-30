using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Common.Model;

namespace MessageBus.Interfaces
{
    [ServiceContract]
    public interface IForwardingService
    {
        [OperationContract(IsOneWay = true)]
        void Forward(Message pMessage);
    }
}
