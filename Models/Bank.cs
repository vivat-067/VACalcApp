using System;
using System.Collections.Generic;
using System.Text;

namespace VACalcApp.Models
{
    public class Bank
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string? BrandColor { get; set; }
        public override string ToString() => Name;
        
    }
}
