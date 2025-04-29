using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AulasSiencb2.Resources;
using System.Windows.Input;
using System.Windows.Controls;

namespace AulasSiencb2.ViewModel
{
    internal class LoginFormViewModel : INotifyPropertyChanged
    {
        private ApplicationState _applicationState;
        private ScreenLockViewModel _screenLockViewModel;
        private string _userNumber;
        private string _curp;
        private string _message;

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public string UserNumber
        {
            get { return _userNumber; }
            set
            {
                _userNumber = value;
                OnPropertyChanged(nameof(UserNumber));
            }
        }
        public string CURP
        {
            get { return _curp; }
            set
            {
                _curp = value;
                OnPropertyChanged(nameof(CURP));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand LoginCommand { get; }
        public ICommand CleanLoginCommand { get; }
        public ICommand Keyenter { get; }

        public LoginFormViewModel(ApplicationState state, ScreenLockViewModel screenLock)
        {
            _applicationState = state;
            _screenLockViewModel = screenLock;
            _userNumber = string.Empty;
            _curp = string.Empty;
            LoginCommand = new SimpleCommand(() => RegistryUser());
            CleanLoginCommand = new SimpleCommand(() => CleanLogin());

        }

        public void KeyEnter(object sender, EventArgs e)
        {
            RegistryUser();
        }

        public void CleanLogin()
        {
            UserNumber = string.Empty;
            CURP = string.Empty;
            Message = string.Empty;
        }
        private async void RegistryUser()
        {
            if(string.IsNullOrEmpty(_userNumber) || string.IsNullOrEmpty(_curp))
            {
                Message = "Porfavor, llena todos los campos";
                return;
            }

            if(_curp.Length != 18)
            {
                Message = "El CURP debe tener 18 caracteres";
                return;
            }
            await _screenLockViewModel.FetchRegisterUser(_userNumber, _curp);
        }
    }
}
