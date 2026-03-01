using System;
using System.Collections.Generic;
using System.Text;
using VACalcApp.Services;

namespace VACalcApp.Models
{
    public class CalculationLogEntry
    {
        public DateTime LogTimestamp { get; set; } = DateTime.Now;
        public decimal DepositAmount { get; set; }
        public decimal DepositInterestRate { get; set; }
        public string? BankName { get; set; } = string.Empty;
        public string? BankBrandColor { get; set; } = "White";
        public InterestCalculationMethod CalculationMethod { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public int DurationMonths { get; set; }
        public int DurationDays { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public decimal CalculatedIncome { get; set; }
        public string? Comment { get; set; } = string.Empty;

        public string DisplayDescription => $"Расчёт для {BankName} от {PeriodStartDate:dd.MM.yyyy}";
    }

}
