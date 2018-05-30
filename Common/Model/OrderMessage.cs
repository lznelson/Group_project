using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Common.Model
{
    [DataContract]
    public class OrderMessage : Message
    {

        [DataMember]
        public Guid OrderGuid { get; set; }
        [DataMember]
        public double Total { get; set; }
        [DataMember]
        public int FromAccountNumber { get; set; }
        [DataMember]
        public int ToAccountNumber { get; set; }
        [DataMember]
        public String DestinationAddress { get; set; }
        [DataMember]
        public String SourceAddress { get; set; }
        [DataMember]
        public String CustomerEmailAddress { get; set; }

    }
}
