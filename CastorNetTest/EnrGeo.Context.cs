﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class enrEntities : DbContext
    {
        public enrEntities()
            : base("name=enrEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Branch> Branch { get; set; }
        public DbSet<BranchElement> BranchElement { get; set; }
        public DbSet<BranchGroup> BranchGroup { get; set; }
        public DbSet<BranchGroupElement> BranchGroupElement { get; set; }
        public DbSet<Node> Node { get; set; }
        public DbSet<Ride> Ride { get; set; }
        public DbSet<RideStop> RideStop { get; set; }
    }
}