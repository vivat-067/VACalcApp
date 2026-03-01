using System;
using System.Collections.Generic;
using System.Text;
using VACalcApp.Services;

namespace VACalcApp.Models
{
    public class AppSettings
    {

        public decimal DepositAmount { get; set; } =Decimal.Zero;

        public decimal DepositInterestRate { get; set; } = 10;

        public int? SelectedBankID { get; set; }

        public int DurationMonths { get; set; } = 1;

        public InterestCalculationMethod CalculationMethod { get; set; } = InterestCalculationMethod.SimpleInterest;

        public AppSettings() { }


    }
}
