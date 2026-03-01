using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using VACalcApp.Services;

namespace VACalcApp.Models
{
    public class CalculationParameters
    {        
        public decimal DepositAmount { get; set; } = decimal.Zero;
        public decimal DepositInterestRate { get; set; } = Decimal.Zero;           
        
        public DateTime PeriodStartDate { get; set; }
        public int DurationMonths { get; set; }

        public int DurationDays { get; set; }

        public DateTime PeriodEndDate { get; set; }
        

        public InterestCalculationMethod CalculationMethod { get; set; }

    }
}
