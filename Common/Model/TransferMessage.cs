using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Common.Model
{
    [DataContract]
    public class TransferMessage : Message
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
        public bool BTransfer { get; set; }
        [DataMember]
        public String Comment { get; set; }

    }
}
