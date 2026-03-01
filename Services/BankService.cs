using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VACalcApp.Models;

namespace VACalcApp.Services
{
    public class BankService : IBankService
    {
        private IEnumerable<Bank>? _banks;

        public IEnumerable<Bank> GetBanks() =>
            _banks = _banks ?? LoadBanksFromDataSource();

        private IEnumerable<Bank> LoadBanksFromDataSource()
        {
            // To-Do: implement loading from file DataSource
            return new List<Bank>
            {
                new Bank { Id = 1, Name = "ВТБ", BrandColor="Blue" },
                new Bank { Id = 2, Name = "Альфа Банк", BrandColor="Red" },
                new Bank { Id = 3, Name = "Газпром Банк", BrandColor="#5199FF" },
                new Bank { Id = 4, Name = "Сбербанк", BrandColor="#4D8802" }
            };
        }
    }
}