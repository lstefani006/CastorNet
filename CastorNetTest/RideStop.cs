//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CastorNetTest
{
    using System;
    using System.Collections.Generic;
    
    public partial class RideStop
    {
        public int OperatorId { get; set; }
        public int RideId { get; set; }
        public int OrderNum { get; set; }
        public Nullable<int> NodeId { get; set; }
        public Nullable<int> ArrivalDay { get; set; }
        public Nullable<System.DateTime> ArrivalTime { get; set; }
        public Nullable<System.DateTime> DepartureTime { get; set; }
    
        public virtual Node Node { get; set; }
    }
}