
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using VACalcApp.Models;

namespace VACalcApp.Services
{
    public interface IBankService
    {
        public IEnumerable<Bank> GetBanks();
    }
}