using System;
using System.Collections.Generic;
using System.Text;
using VACalcApp.Models;

namespace VACalcApp.Services
{

    public enum InterestCalculationMethod {SimpleInterest = 0, CompoundInterestMonthly = 1, MinimalMonthlyAmount = 2}
    public enum ValidationStatus { Ready, Success, Error }

    internal class BankAccountInterestCalculator : IBankAccountInterestCalculator
    {

        private decimal DailyRate(decimal AnnualInterestRate) => AnnualInterestRate / 100m / 365m;

        public decimal Calculate(CalculationParameters parameters)
        {
            switch ((InterestCalculationMethod)parameters.CalculationMethod)
            {
                case InterestCalculationMethod.SimpleInterest:                    // Простые проценты
                case InterestCalculationMethod.MinimalMonthlyAmount:              //Накопительный счет
                    return CalculateSimpleInterest(parameters);
                case InterestCalculationMethod.CompoundInterestMonthly:           // Ежемесячная капитализация с выплатой в конце срока
                    return CalculateCompoundInterest(parameters);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(parameters.CalculationMethod),
                        "Неподдерживаемый вид расчета");
            }            
        }


        private decimal CalculateSimpleInterest(CalculationParameters p)
        {            
            return p.DepositAmount * DailyRate(p.DepositInterestRate) * p.DurationDays;
        }

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

        private decimal CalculateMinimumBalanceInterest(CalculationParameters p)
        {
            // минимальный остаток за месяц            
            return p.DepositAmount * DailyRate(p.DepositInterestRate) * p.DurationDays;
        }


    }
}
