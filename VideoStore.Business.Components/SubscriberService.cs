using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Common.Model;

namespace VideoStore.Business.Components
{
    public class SubscriberService : ISubscriberService
    {
        public void PublishToSubscriber(Message pMessage)
        {
            System.Diagnostics.Debug.WriteLine("Video ------------ Receive");

        }
    }
}
