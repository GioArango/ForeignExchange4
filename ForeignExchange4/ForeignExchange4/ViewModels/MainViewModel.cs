﻿namespace ForeignExchange4.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Net.Http;
    using System.Windows.Input;
    using GalaSoft.MvvmLight.Command;
    using Models;
    using Newtonsoft.Json;
    using Xamarin.Forms;
    using ForeignExchange4.Helpers;
    using ForeignExchange4.Services;
    using System.Threading.Tasks;

    public class MainViewModel : INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Services
        ApiService apiService;
        DialogService dialogServices;
        DataService dataServices;
        #endregion

        #region Attributes
        bool _isRunning;
        bool _isEnabled;
        string _result;
        ObservableCollection<Rate> _rates;
        Rate _sourceRate;
        Rate _targetRate;
        string _status;
        List<Rate> rates;
        #endregion

        #region Properties
        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(Status)));
                }
            }
        }

        public string Amount
        {
            get;
            set;
        }

        public ObservableCollection<Rate> Rates
        {
            get
            {
                return _rates;
            }
            set
            {
                if (_rates != value)
                {
                    _rates = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(Rates)));
                }
            }
        }

        public Rate SourceRate
        {
            get
            {
                return _sourceRate;
            }
            set
            {
                if (_sourceRate != value)
                {
                    _sourceRate = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(SourceRate)));
                }
            }
        }

        public Rate TargetRate
        {
            get
            {
                return _targetRate;
            }
            set
            {
                if (_targetRate != value)
                {
                    _targetRate = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(TargetRate)));
                }
            }
        }

        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(IsRunning)));
                }
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(IsEnabled)));
                }
            }
        }

        public string Result
        {
            get
            {
                return _result;
            }
            set
            {
                if (_result != value)
                {
                    _result = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(Result)));
                }
            }
        }
        #endregion

        #region Constructors
        public MainViewModel()
        {
            apiService = new ApiService();
            dataServices = new DataService();
            dialogServices = new DialogService();

            LoadRates();
        }
        #endregion

        #region Methods
        async void LoadRates()
        {
            IsRunning = true;
            Result = Lenguages.Loading;

            var connection = await apiService.CheckConnection();

            if (!connection.IsSuccess)
            {
                LoadLocalData();
            }
            else
            {
                await LoadDataFromAPI();
            }

            if (rates.Count == 0)
            {
                IsRunning = false;
                IsEnabled = false;
                Result = Lenguages.RateValidationNull;
                Status = Lenguages.NoRateLoaded;
                return;
            }
           
            Rates = new ObservableCollection<Rate>(rates);

            IsRunning = false;
            IsEnabled = true;
            Result = Lenguages.Ready;
            Status = Lenguages.StatusRateValidation;
        }

        private void LoadLocalData()
        {
            rates = dataServices.Get<Rate>(false);
            Status = Lenguages.LoadDataLocalRate;
        }

        async Task LoadDataFromAPI()
        {
            var url = "http://apiexchangerates.azurewebsites.net"; //Application.Current.Resources["URLAPI"].ToString();

            var response = await apiService.GetList<Rate>(
                url,
                "api/Rates");

            if (!response.IsSuccess)
            {
                LoadLocalData();
                return;
            }

            // Storage data local 
            rates = (List<Rate>)response.Result;
            dataServices.DeleteAll<Rate>();
            dataServices.Save(rates);

            Status = Lenguages.StatusRateValidation;
        }
        #endregion

        #region Commands
        public ICommand SwitchCommand
        {
            get
            {
                return new RelayCommand(Switch);
            }
        }

        void Switch()
        {
            var aux = SourceRate;
            SourceRate = TargetRate;
            TargetRate = aux;
            Convert();
        }

        public ICommand ConvertCommmand
        {
            get
            {
                return new RelayCommand(Convert);
            }
        }

        async void Convert()
        {
            if (string.IsNullOrEmpty(Amount))
            {
                await dialogServices.ShowMessage(
                    Lenguages.Error,
                    Lenguages.AmountValidation);
                return;
            }

            decimal amount = 0;
            if (!decimal.TryParse(Amount, out amount))
            {
                await dialogServices.ShowMessage(
                    Lenguages.Error,
                    Lenguages.AmountValidation);
                return;
            }

            if (SourceRate == null)
            {
                await dialogServices.ShowMessage(
                    Lenguages.Error,
                    Lenguages.SourceRateValidation);
                return;
            }

            if (TargetRate == null)
            {
                await dialogServices.ShowMessage(
                    Lenguages.Error,
                    Lenguages.TargetRateValidation);
                return;
            }

            var amountConverted = amount /
                                  (decimal)SourceRate.TaxRate *
                                  (decimal)TargetRate.TaxRate;

            Result = string.Format(
                "{0} {1:C2} = {2} {3:C2}",
                SourceRate.Code,
                amount,
                TargetRate.Code,
                amountConverted);
        }
        #endregion
    }
}
