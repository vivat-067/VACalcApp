using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

using ReactiveUI;
using ReactiveUI.Avalonia;

using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables.Fluent;


using VACalcApp.ViewModels;

namespace VACalcApp.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            ViewModel = App.GetService<MainWindowViewModel>();

            InitializeComponent();
            InitializeInteractionHandlers();
            
            this.Closing += OnWindowClosing;
        }

        private void InitializeInteractionHandlers()
        {


            this.WhenActivated(disposableRegistration =>
            {
                // Handle exit app interaction
                ViewModel.ExitAppCloseWindowInteraction
                    .RegisterHandler(async interaction =>
                    {
                        try
                        {
                            if (Application.Current?.ApplicationLifetime is
                                IClassicDesktopStyleApplicationLifetime desktopLifetime)
                            {
                                desktopLifetime.Shutdown();
                            }
                            interaction.SetOutput(Unit.Default);
                        }
                        catch (Exception ex)
                        {                            
                            // Log.Error(ex, "Ошибка при завершении работы приложения");
                            interaction.SetOutput(Unit.Default);
                        }

                    }).DisposeWith(disposableRegistration);

                // Handle About interaction  
                ViewModel.ShowAboutInteraction
                    .RegisterHandler(async interaction =>
                    {
                        try
                        {
                            await new AboutWindow().ShowDialog(this);
                            interaction.SetOutput(Unit.Default);
                        }
                        catch (Exception ex)
                        {
                            // Log.Error(ex, "Show About err");
                            interaction.SetOutput(Unit.Default);
                        }
                    })
                    .DisposeWith(disposableRegistration);


            });
        }

        private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewModel?.SaveSettingsOnWindowClosing();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);         
            this.Closing -= OnWindowClosing;     // Отписываемся от события при удалении из визуального дерева
        }

    }
}
