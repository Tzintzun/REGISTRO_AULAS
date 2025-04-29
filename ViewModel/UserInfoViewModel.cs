using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AulasSiencb2.Resources;
using AulasSiencb2.Services;
using Serilog;

namespace AulasSiencb2.ViewModel
{
    internal class UserInfoViewModel : INotifyPropertyChanged
    {
        private string _department;
        private BitmapImage _photo;
        private string _userName;
        private ScreenLockViewModel _lockViewModel;
        private ApplicationState _applicationState;

        public ICommand AceptUserSessionCommand { get; set; }
        public ICommand CancelUserSessionCommand { get; set; }

        public UserInfoViewModel(ApplicationState state,ScreenLockViewModel lockViewModel)
        {
            _lockViewModel = lockViewModel;
            _department = string.Empty;
            _userName = string.Empty;
            ApiService apiService = new ApiService();
            _applicationState = state;
            AceptUserSessionCommand = new SimpleCommand(() => AceptUserSession());
            CancelUserSessionCommand = new SimpleCommand(() => CancelUserSession());
            
        }

        public string Department
        {
            get { return _department; }
            set { _department = value; OnPropertyChanged(nameof(Department)); }
        }

        public BitmapImage Photo
        {
            get { return _photo; }
            set
            {
                _photo = value;
                OnPropertyChanged(nameof(Photo));
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        public void AceptUserSession()
        {
            if (_lockViewModel != null)
            {
                Log.Information($"El usuario: {_applicationState.Session.User.UserNumber} acepto su ingreso. Fecha: {_applicationState.Session.DateIn}");
                _lockViewModel.CloseWindowUser();
            }
            else
            {
                _lockViewModel.ShowMessageUser("Error en la aplicación");
            }
        }

        public async void CancelUserSession()
        {
            await _lockViewModel.FetchRegisterUserLogoutByUser();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
