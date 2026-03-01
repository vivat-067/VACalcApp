using System;
using System.Collections.Generic;
using System.Text;

using VACalcApp.Models;

namespace VACalcApp.Services
{
    public interface ISettingsStorage
    {
        AppSettings Load();
        void Save(AppSettings settings);
    }
}
