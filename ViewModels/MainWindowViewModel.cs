using DynamicData;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.SourceGenerators;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

using VACalcApp.Models;
using VACalcApp.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VACalcApp.ViewModels
{

    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly IBankAccountInterestCalculator _bankAccountInterestCalculator;
        private readonly IBankService _bankService;


        private readonly ISettingsStorage _settingsStorageService;
        private AppSettings _currentAppSettings = new();

        private CompositeDisposable _disposables = new CompositeDisposable();


        #region Properties

        [Reactive]
        public partial decimal DepositAmount { get; set; } = 100000;

        [Reactive]
        public partial decimal DepositInterestRate { get; set; } = 10;

        [Reactive]
        public partial Bank? SelectedBank { get; set; }

        [Reactive]
        public partial ObservableCollection<Bank> Banks { get; set; } = new();


        [Reactive]
        public partial InterestCalculationMethod CalculationMethod { get; set; } = 0;

        [Reactive]
        public partial DateTime PeriodStartDate { get; set; } = DateTime.Today;

        [Reactive]
        public partial int DurationMonths { get; set; } = 1;

        [Reactive]
        public partial int DurationDays { get; set; }

        [Reactive]
        public partial DateTime PeriodEndDate { get; set; } = DateTime.Today.AddMonths(1);

        [Reactive]
        public partial decimal CalculatedIncome { get; set; } = decimal.Zero;

        [Reactive]
        public partial string? Comment { get; set; }


        //Calculation Log

        [Reactive]
        public partial ObservableCollection<CalculationLogEntry> CalculationLog { get; set; } = new();

        [Reactive]
        public partial CalculationLogEntry? SelectedCalculationLogEntry { get; set; }


        //Validation Status 

        [Reactive]
        public partial string StatusIcon { get; set; }

        [Reactive]
        public partial string StatusTitle { get; set;}

        [Reactive]
        public partial string? StatusDescription { get; set; }


        //ObservableAsPropertyHelpers
        private readonly ObservableAsPropertyHelper<bool> _isChangeDurationDaysEnabled;

        public bool IsChangeDurationDaysEnabled => _isChangeDurationDaysEnabled.Value;


        #endregion


        #region Commands
        public ReactiveCommand<Unit, Unit> ShowAboutCommand { get; }
        public ReactiveCommand<Unit, Unit> ExitAppCommand { get; }
        public ReactiveCommand<Unit, Unit> CalculateCommand { get; }


        public ReactiveCommand<Unit, Unit> CalculationLogDeleteEntryCommand { get; }
        public ReactiveCommand<Unit, Unit> CalculationLogClearAllCommand { get; }

        #endregion

        #region Interactions          
        public Interaction<Unit, Unit> ShowAboutInteraction { get; } = new();
        public Interaction<Unit, Unit> ExitAppCloseWindowInteraction { get; } = new();
        #endregion

        public MainWindowViewModel(IBankAccountInterestCalculator bankAccountInterestCalculator,
                                   IBankService bankService,
                                   ISettingsStorage settingsStorageService)
        {

            _bankAccountInterestCalculator = bankAccountInterestCalculator ?? throw new ArgumentNullException(nameof(bankAccountInterestCalculator));

            _bankService = bankService;
                LoadBanks();        

            _settingsStorageService = settingsStorageService ?? throw new ArgumentNullException(nameof(settingsStorageService));            

            _currentAppSettings = _settingsStorageService.Load();
                SettingsToProperties(_currentAppSettings);


            SubscribeToPropertyChanges();


            //Calulator Tab Commands
            ShowAboutCommand = ReactiveCommand.CreateFromTask(ShowAboutAsync);
            ExitAppCommand = ReactiveCommand.CreateFromTask(ExitAppCloseWindow);


            var canExecuteCalculateCommand = this.WhenAnyValue(
                         x => x.DepositAmount,
                         x => x.DepositInterestRate,
                         x => x.DurationMonths)
                         .Select(values =>
                         {
                             var (amount, rate, duration) = values;
                             return amount > 0 && rate > 0 && rate <= 100 && duration > 0;
                         })
                         .DistinctUntilChanged()
                         .ObserveOn(AvaloniaScheduler.Instance);

            CalculateCommand = ReactiveCommand.CreateFromTask(CalculateAsync,
                                                  canExecuteCalculateCommand);           



            //ClaculationLog Tab Commands
            var CanExecuteCalculationLogDeleteEntryCommand = this.WhenAnyValue(
                                                                    x => x.CalculationLog.Count,
                                                                    x => x.SelectedCalculationLogEntry)
                                                                    .Select(tuple =>
                                                                      {
                                                                          var (count, logentry) = tuple;
                                                                          return count > 0 && logentry != null;
                                                                       })                        
                                                                    .DistinctUntilChanged()
                                                                    .ObserveOn(AvaloniaScheduler.Instance); 

            CalculationLogDeleteEntryCommand = ReactiveCommand.Create(CalculationLogDeleteEntry,
                                                                             CanExecuteCalculationLogDeleteEntryCommand);

            var CanExecuteCalculationLogClearAllCommand = this.WhenAnyValue(x => x.CalculationLog.Count)
                                                                .Select(count => count > 0)
                                                                .DistinctUntilChanged()
                                                                .ObserveOn(AvaloniaScheduler.Instance);

            CalculationLogClearAllCommand = ReactiveCommand.Create(CalculationLogClearAll,
                                                                           CanExecuteCalculationLogClearAllCommand);



            // Управление состоянием поля ввода месяца  OAPH initialization 
            _isChangeDurationDaysEnabled = this.WhenAnyValue(x => x.CalculationMethod)
                .Select(method => method != InterestCalculationMethod.MinimalMonthlyAmount)
                .ToProperty(this, x => x.IsChangeDurationDaysEnabled);

        }

        private void SubscribeToPropertyChanges()
        {
            // Пересчёт даты и количества дней окончания периода
            var periodSubscription = this.WhenAnyValue(
                x => x.PeriodStartDate,
                x => x.DurationMonths,
                x => x.CalculationMethod)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .ObserveOn(AvaloniaScheduler.Instance)
                .Subscribe(_ => UpdatePeriod());

            // Сброс рассчитанной суммы при изменении параметров
            var calculatedAmountSubscription = this.WhenAnyValue(
                x => x.DepositAmount,
                x => x.DepositInterestRate,
                x => x.PeriodStartDate,
                x => x.DurationMonths,
                x => x.CalculationMethod)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .ObserveOn(AvaloniaScheduler.Instance)
                .Subscribe(_ => this.CalculatedIncome = decimal.Zero);
             

            // Управление отображением статуса
            var statusValidationSubscription = this.WhenAnyValue(x => x.CalculatedIncome)
                .ObserveOn(AvaloniaScheduler.Instance)
                .DistinctUntilChanged() 
                .Subscribe(income =>
                {
                    var status = income > 0 ? ValidationStatus.Success : ValidationStatus.Ready;
                    DisplayValidationStatus(status);
                });


            _disposables.Add(calculatedAmountSubscription);           
            _disposables.Add(periodSubscription);
            _disposables.Add(statusValidationSubscription);            
        }

        private void SettingsToProperties(AppSettings settings)
        {
            if (settings == null) return;

            DepositAmount = settings.DepositAmount;
            DepositInterestRate = settings.DepositInterestRate;            
            DurationMonths = settings.DurationMonths;
            CalculationMethod = settings.CalculationMethod;

            SelectedBank = (settings.SelectedBankID.HasValue) ?
                           Banks?.FirstOrDefault(b => b.Id == settings.SelectedBankID.Value) : null;

            UpdatePeriod();
        }


        private void PropertiesToSettings()
        {
            _currentAppSettings.DepositAmount = DepositAmount;
            _currentAppSettings.DepositInterestRate = DepositInterestRate;
            _currentAppSettings.SelectedBankID = SelectedBank?.Id;
            _currentAppSettings.DurationMonths = DurationMonths;
            _currentAppSettings.CalculationMethod = CalculationMethod;

        }


        private void PropertiesToParameters(CalculationParameters parameters)
        {
            parameters.DepositAmount = DepositAmount;
            parameters.DepositInterestRate = DepositInterestRate;            

            parameters.PeriodStartDate = PeriodStartDate;
            parameters.DurationMonths = DurationMonths;
            parameters.DurationDays = DurationDays;
            parameters.PeriodEndDate = PeriodEndDate;

            parameters.CalculationMethod = CalculationMethod;                      

        }

        private void UpdatePeriod()        {           

            if (CalculationMethod == InterestCalculationMethod.MinimalMonthlyAmount)
            {
                DurationMonths = 1;
            };            

            PeriodEndDate = PeriodStartDate.AddMonths(DurationMonths);
            DurationDays = (PeriodEndDate - PeriodStartDate).Days;
        }


        private void LoadBanks()
        {
            try
            {
                Banks.Clear();
                var banks = _bankService.GetBanks();

                if (banks != null)
                {
                    foreach (var bank in banks)
                    {
                        Banks.Add(bank);
                    }
                    Debug.WriteLine($"Successfully loaded {Banks.Count} banks");
                }
                else
                {
                    Debug.WriteLine("No banks returned from service");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load banks: {ex.Message}");
            }
        }


        public void SaveSettingsOnWindowClosing() 
        {  
            PropertiesToSettings();
            try
            {
               _settingsStorageService.Save(_currentAppSettings);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        private async Task CalculateAsync()
        {
            try
            {
                Debug.WriteLine($"Amount: {DepositAmount}");
                Debug.WriteLine($"Rate: {DepositInterestRate}");
                Debug.WriteLine($"Bank: {SelectedBank}");
                Debug.WriteLine($"Dep Type: {CalculationMethod}");
                Debug.WriteLine($"Start: {PeriodStartDate}");                

                CalculationParameters parameters = new();
                PropertiesToParameters(parameters);

                CalculatedIncome = _bankAccountInterestCalculator.Calculate(parameters);                

                AddCalculationLogEntry(parameters,
                                       selectedBank: SelectedBank,
                                       calculatedAmount: CalculatedIncome,
                                       comment: Comment);

                Console.Beep();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to calculate: {ex.Message}");
            }
        }


        private void AddCalculationLogEntry(CalculationParameters parameters, 
                                                             Bank selectedBank, 
                                                             decimal calculatedAmount, 
                                                             string? comment)
        {
            var logEntry = new CalculationLogEntry
            {
                DepositAmount = parameters.DepositAmount,
                DepositInterestRate = parameters.DepositInterestRate,
                BankName = selectedBank?.Name ?? string.Empty,
                BankBrandColor = selectedBank?.BrandColor ?? "#FFFFFF", 
                CalculationMethod = parameters.CalculationMethod,
                PeriodStartDate = parameters.PeriodStartDate,
                DurationMonths = parameters.DurationMonths,
                DurationDays = parameters.DurationDays,
                PeriodEndDate = parameters.PeriodEndDate,
                CalculatedIncome = calculatedAmount,
                Comment = comment ?? string.Empty
            };

            AvaloniaScheduler.Instance.Schedule(_ => CalculationLog.Insert(0, logEntry));

            Debug.WriteLine($"Log entry added. Total entries: {CalculationLog.Count}");
        }

        private void CalculationLogDeleteEntry()
        {
            AvaloniaScheduler.Instance.Schedule(_ =>
            {
                if (SelectedCalculationLogEntry != null)
                {
                    CalculationLog.Remove(SelectedCalculationLogEntry);
                    SelectedCalculationLogEntry = null;

                }
            });            
        }

        private void CalculationLogClearAll()
        {
            AvaloniaScheduler.Instance.Schedule(_ => CalculationLog.Clear());
        }        

        private async Task ShowAboutAsync()
        {
            try
            {
                await ShowAboutInteraction.Handle(Unit.Default);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to open about: {ex.Message}");
            }
        }

        private async Task ExitAppCloseWindow()
        {
            try
            {
     
                await ExitAppCloseWindowInteraction.Handle(Unit.Default);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to close app: {ex.Message}");
            }
        }



        public void DisplayValidationStatus(ValidationStatus status)
        {            
            var (title, description, icon) = PrepareValidationStatusRecord(status);
            StatusTitle = title;
            StatusDescription = description;
            StatusIcon = icon;
        }


        private (string title, string description, string icon) PrepareValidationStatusRecord(ValidationStatus status) => status switch
        {
            ValidationStatus.Ready => ("Введите данные. ", "Нажмите на кнопку \"Рассчитать\"", "/Assets/info.png"),
            ValidationStatus.Success => ("Успешно. ", "Раcчет выполнен", "/Assets/info.png"),
            ValidationStatus.Error => ("Ошибка! ", "Проверьте ввод данных...", "/Assets/warning.png"),
            _ => throw new ArgumentOutOfRangeException(nameof(status), $"Неизвестный статус валидации: {status}"),
        };


        /// <summary>
        /// Освобождение ресурсов при уничтожении ViewModel
        /// </summary>
        public void Dispose()
        {
            _disposables?.Dispose();

        }

    }
}