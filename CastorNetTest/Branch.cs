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
    
    public partial class Branch
    {
        public Branch()
        {
            this.BranchElement = new HashSet<BranchElement>();
            this.BranchGroupElement = new HashSet<BranchGroupElement>();
        }
    
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public Nullable<int> DistanceKm { get; set; }
    
        public virtual ICollection<BranchElement> BranchElement { get; set; }
        public virtual ICollection<BranchGroupElement> BranchGroupElement { get; set; }
    }
}