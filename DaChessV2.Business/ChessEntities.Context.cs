﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DaChessV2.Business
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ChessEntities : DbContext
    {
        public ChessEntities()
            : base("name=ChessEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<BoardType> BoardType { get; set; }
        public virtual DbSet<Party> Party { get; set; }
        public virtual DbSet<PartyState> PartyState { get; set; }
        public virtual DbSet<PlayerState> PlayerState { get; set; }
        public virtual DbSet<PartyHistory> PartyHistory { get; set; }
        public virtual DbSet<PartyCadence> PartyCadence { get; set; }
        public virtual DbSet<Logs> Logs { get; set; }
        public virtual DbSet<LogType> LogType { get; set; }
    }
}
