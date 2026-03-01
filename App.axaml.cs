using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System;
using VACalcApp.Services;
using VACalcApp.ViewModels;
using VACalcApp.Views;

namespace VACalcApp
{
    public partial class App : Application
    {

        private IServiceProvider? serviceProvider;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {

            var services = new ServiceCollection();

                services.AddSingleton<IBankAccountInterestCalculator, BankAccountInterestCalculator>();
                services.AddSingleton<IBankService, BankService>();

                services.AddSingleton<ISettingsStorage, SettingsStorageService>();

                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<AboutWindowViewModel>();

            serviceProvider = services.BuildServiceProvider();


            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        //Get Service for access in other places
        public static T? GetService<T>() where T : class
        {
            return (Current as App)?.serviceProvider?.GetRequiredService<T>();
        }

    }
}