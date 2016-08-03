//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class Party
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Party()
        {
            this.PartyHistory = new HashSet<PartyHistory>();
        }
    
        public int Id { get; set; }
        public int FK_BoardType { get; set; }
        public int FK_PartyState { get; set; }
        public int FK_White_PlayerState { get; set; }
        public int FK_Black_PlayerState { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public string WhiteToken { get; set; }
        public string BlackToken { get; set; }
        public string PartyName { get; set; }
        public string Board { get; set; }
        public string JsonHistory { get; set; }
        public string Seed { get; set; }
        public string EnPassantCase { get; set; }
        public string LastMoveCase { get; set; }
        public int FK_PartyCadence { get; set; }
        public Nullable<int> WhiteTimeLeft { get; set; }
        public Nullable<int> BlackTimeLeft { get; set; }
        public Nullable<int> WhiteFischer { get; set; }
        public Nullable<int> BlackFischer { get; set; }
    
        public virtual BoardType BoardType { get; set; }
        public virtual PlayerState BlackPlayerState { get; set; }
        public virtual PartyState PartyState { get; set; }
        public virtual PlayerState WhitePlayerState { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PartyHistory> PartyHistory { get; set; }
        public virtual PartyCadence PartyCadence { get; set; }
    }
}
