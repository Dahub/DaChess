﻿namespace DaChess.WebApi.Models
{
    public abstract class AbstractModel
    {
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
    }
}