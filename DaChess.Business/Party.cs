//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DaChess.Business
{
    using System;
    using System.Collections.Generic;
    
    public partial class Party
    {
        public int Id { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public string PartLink { get; set; }
        public int FK_Board_Type { get; set; }
        public string Board { get; set; }
        public string History { get; set; }
        public bool WhiteTurn { get; set; }
        public string Seed { get; set; }
        public string EnPassantCase { get; set; }
        public Nullable<bool> WhiteCanPromote { get; set; }
        public Nullable<bool> BlackCanPromote { get; set; }
        public string WhiteToken { get; set; }
        public string BlackToken { get; set; }
        public int FK_White_Player_Stat { get; set; }
        public int FK_Black_Player_Stat { get; set; }
    
        public virtual Board BoardType { get; set; }
        public virtual PlayerState BlackPlayerState { get; set; }
        public virtual PlayerState WhitePlayerState { get; set; }
    }
}
