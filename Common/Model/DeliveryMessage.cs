using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace Common.Model
{
    [DataContract]
    public class DeliveryMessage : Message
    {
        [DataMember]
        public String SourceAddress { get; set; }
        [DataMember]
        public String DestinationAddress { get; set; }
        [DataMember]
        public String OrderNumber { get; set; }
        [DataMember]
        public Guid DeliveryIdentifier { get; set; }
        [DataMember]
        public String DeliveryNotificationAddress { get; set; }
        [DataMember]
        public Int32 Status { get; set; }
    }
}
