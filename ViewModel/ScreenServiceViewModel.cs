using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AulasSiencb2.Services;

namespace AulasSiencb2.ViewModel
{
    class ScreenServiceViewModel : INotifyPropertyChanged
    {

        private ApplicationState _applicationState;
        private LoadingViewViewModel _loadingView;
        private ServiceFormViewModel _serviceViewModel;

        private object? _serviceUserControl;
        public bool _dialogResult;

        public event Action CloseWindowEvent;

        public object? UserControl
        {
            get { return _serviceUserControl; }
            set
            {
                _serviceUserControl = value;
                OnPropertyChanged(nameof(UserControl));
            }
        }

        public async Task LoadBance()
        {
            ApiService apiService = new ApiService();
            decimal data = await apiService.GetBalance(_applicationState.Session.User.PkUsuario, _applicationState.Session.User.PkCategoria);
            Debug.WriteLine(data);
            if (data == null || data < 0) 
            {
                MessageBox.Show("Error al cargar el saldo");
                return;
            }
            _serviceViewModel.Balance = data;

        }

        public async Task LoadServices()
        {
            ApiService apiService = new ApiService();
            JsonArray data = await apiService.GetServices();
            if (data == null)
            {
                MessageBox.Show("Error al cargar los servicios");
                return;
            }

            if(data.Count < 0)
            {
                MessageBox.Show("No hay servicios disponibles");
                return;
            }
            List<Service> services = new List<Service>();
            foreach (JsonObject obj in data)
            {
                Service service = new Service(obj);
                services.Add(service);

            }
            _serviceViewModel.Services = services;
            _serviceViewModel.LoadOptions();

            UserControl = _serviceViewModel;
        }

        public void ShowLoading()
        {
            UserControl = _loadingView;
        }

        public void ShowForm()
        {
            UserControl = _serviceViewModel;
        }

        public void CloseWindow(bool status)
        {
            Debug.WriteLine("CloseWindow LogOut");
            _dialogResult = status;
            CloseWindowEvent?.Invoke();
        }

        public ScreenServiceViewModel(ApplicationState applicationState)
        {
            _applicationState = applicationState;
            _loadingView = new LoadingViewViewModel();
            _serviceViewModel = new ServiceFormViewModel(applicationState, this);
            UserControl = _loadingView;
            _dialogResult = false;
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
