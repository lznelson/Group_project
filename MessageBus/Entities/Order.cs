//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MessageBus.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Order
    {
        public int Id { get; set; }
        public Nullable<System.Guid> OrderNumber { get; set; }
        public string SourceAddress { get; set; }
        public string DestinationAddress { get; set; }
        public string EmailAddress { get; set; }
    }
}