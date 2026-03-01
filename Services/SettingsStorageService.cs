using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

using VACalcApp.Models;

namespace VACalcApp.Services
{
    internal class SettingsStorageService : ISettingsStorage
    {
        private const string SettingsFileName = "vacalc-settings.json";

        private static string SettingsFolder =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VACalc");

        private static string SettingsPath => Path.Combine(SettingsFolder, SettingsFileName);


        public AppSettings Load()
        {
            try
            {
                Directory.CreateDirectory(SettingsFolder);
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();       
          
                    return settings;
                }
                return new AppSettings();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load settings: {ex.Message}");
                var settings = new AppSettings();

                return settings;
            }
        }

        public void Save(AppSettings settings)
        {
            try
            {     
                Directory.CreateDirectory(SettingsFolder);

                // Сериализуем данные перед вызовом события
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(settings, options);              
             
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }
    }
}