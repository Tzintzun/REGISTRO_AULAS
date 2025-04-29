using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using AulasSiencb2.Models;
using AulasSiencb2.Resources;
using AulasSiencb2.Services;

namespace AulasSiencb2.ViewModel
{
    internal class ScreenLogOutViewmodel : INotifyPropertyChanged
    {
        private ApplicationState _applicationState;
        private LoadingViewViewModel _loadingView;
        private LogoutFormViewModel _logoutViewModel;
        private object? _logoutUserControl;
        public bool _dialogResult;

        public event Action CloseWindowEvent;

        public object? UserControl
        {
            get { return _logoutUserControl; }
            set
            {
                _logoutUserControl = value;
                OnPropertyChanged(nameof(UserControl));
            }
        }


        public ScreenLogOutViewmodel(ApplicationState applicationState)
        {
            _applicationState = applicationState;
            _loadingView = new LoadingViewViewModel();
            _logoutViewModel = new LogoutFormViewModel(applicationState, this);
            UserControl = _loadingView;
            _dialogResult = false;
        }

        public void ShowLoading()
        {
            UserControl = _loadingView;
        }

        public void ShowForm()
        {
            UserControl = _logoutViewModel;
        }

        public void CloseWindow(bool status)
        {
            Debug.WriteLine("CloseWindow LogOut");
            _dialogResult = status;
            CloseWindowEvent?.Invoke();
        }

        public async Task LoadOptions()
        {
            ApiService api = new();
            JsonObject data = await api.GetPcPackaging(_applicationState.Session.PkEquipo);
            if (data != null)
            {
                Debug.WriteLine(data.ToString());

                if (data["statusCode"] == UsePC.GET_USEPC_OK)
                {
                    List<JsonObject> services = new List<JsonObject>();
                    foreach(JsonObject jo in (JsonArray)data["servicios"])
                    {
                        services.Add(jo);
                    }

                    List<JsonObject> programs = new List<JsonObject>();
                        
                    foreach(JsonObject jo in (JsonArray)data["paqueteria"])
                    {
                        programs.Add(jo);
                    }
                    UsePC availablePackaging = new UsePC
                    {
                        Programs = programs,
                        Services = services
                    };

                    _logoutViewModel.UsePcAvaible = availablePackaging;
                    _logoutViewModel.LoadOptions();
                }
                if (data["statusCode"] == UsePC.GET_USEPC_ERROR)
                {
                    Debug.WriteLine("Error al obtener los datos del equipo");
                    System.Windows.MessageBox.Show("Error al obtener la paqueteria del equipo","Registro Usuario", MessageBoxButton.YesNo);
                }


            }
            UserControl = _logoutViewModel;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
