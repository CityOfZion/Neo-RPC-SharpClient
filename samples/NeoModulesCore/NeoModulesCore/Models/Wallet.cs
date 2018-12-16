using System;
using System.ComponentModel.DataAnnotations;

namespace NeoModulesCore.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public string Password { get; set; }
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; }
    }
}
