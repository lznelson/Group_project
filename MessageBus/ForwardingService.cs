using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageBus.Interfaces;
using Common.Model;
using System.ServiceModel;
using Common;

namespace MessageBus
{
    public class ForwardingService : IForwardingService
    {
        public void Forward(Message message)
        {
            String topic = message.Topic;
            
        }
    }
}
