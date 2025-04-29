using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AulasSiencb2.Helpers;
using AulasSiencb2.Resources;

namespace AulasSiencb2.ViewModel
{
    internal class InputPasswordViewModel : INotifyPropertyChanged
    {
        private string _password;
        private string _message;
        private ScreenLockViewModel _screenView;
        private bool _dialogResult;

        public ICommand VerifyPasswordCommand { get; set; }
        public ICommand CloseInputPasswordCommand { get; set; }



        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
            }
        }


        public InputPasswordViewModel(ScreenLockViewModel screen)
        {
            _password = string.Empty;
            _message = string.Empty;
            _screenView = screen;
            VerifyPasswordCommand = new SimpleCommand(() => VerifyPassword());
            CloseInputPasswordCommand = new SimpleCommand(() => CloseInputPassword());
        }

        private  void VerifyPassword()
        {
            
            if (string.IsNullOrEmpty(_password))
            {
                Message = "Porfavor, introduce la contraña.";
                return;
            }

            
            if (SegurityUtils.VerifyPasswordAdmin(_password))
            {

                _screenView.CloseWindowAdmin();
            }
            else
            {
                Message = "Contraseña incorrecta";
            }
        }

        private async void CloseInputPassword()
        {
            //Debug.WriteLine("Entre al Comando Cerrar Input");
            await _screenView.CloseInputPassword();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
