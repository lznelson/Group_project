using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace Common.Model
{
    [DataContract]
    public class EmailMessage : Message
    {
        [DataMember]
        public String EmailAddress { get; set; }
        [DataMember]
        public String EmailContent { get; set; }
    }
}
