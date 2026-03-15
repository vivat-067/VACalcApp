using System;
using System.Collections.Generic;
using System.Text;
using VACalcApp.Models;

namespace VACalcApp.Services
{

    public enum InterestCalculationMethod { SimpleInterest = 0, CompoundInterestMonthly = 1, MinimalMonthlyAmount = 2 }
    public enum ValidationStatus { Ready, Success, Error }

    internal class BankAccountInterestCalculator : IBankAccountInterestCalculator
    {

        private decimal DailyRate(decimal AnnualInterestRate) => AnnualInterestRate / 100m / 365m;

        public decimal Calculate(CalculationParameters parameters)
        {
            return parameters.CalculationMethod switch
            {
                InterestCalculationMethod.SimpleInterest or
                InterestCalculationMethod.MinimalMonthlyAmount => CalculateSimpleInterest(parameters),
                InterestCalculationMethod.CompoundInterestMonthly => CalculateCompoundInterest(parameters),
                _ => throw new ArgumentOutOfRangeException(
                                            nameof(parameters.CalculationMethod),
                                            "Неподдерживаемый вид расчета"),
            };
        }


        // Простые проценты
        private decimal CalculateSimpleInterest(CalculationParameters p)
        {
            return p.DepositAmount * DailyRate(p.DepositInterestRate) * p.DurationDays;
        }

        // Ежемесячная капитализация с выплатой в конце срока
        private decimal CalculateCompoundInterest(CalculationParameters p)
        {
            decimal dailyRate = DailyRate(p.DepositInterestRate);
            decimal amount = p.DepositAmount;

            for (int i = 0; i < p.DurationDays; i++)
            {
                amount += amount * dailyRate;
            }

            return amount - p.DepositAmount;
        }



    }
}
