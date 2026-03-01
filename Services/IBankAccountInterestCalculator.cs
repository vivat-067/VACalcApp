using System;
using System.Collections.Generic;
using System.Text;
using VACalcApp.Models;

namespace VACalcApp.Services
{
    public interface IBankAccountInterestCalculator
    {
        decimal Calculate(CalculationParameters parameters);
    }
}
