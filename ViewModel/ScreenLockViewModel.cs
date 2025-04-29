using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AulasSiencb2.Models;
using AulasSiencb2.Services;
using System.Json;
using System.Windows.Input;
using AulasSiencb2.Resources;
using System.Diagnostics;
using System.Windows;

namespace AulasSiencb2.ViewModel
{
    internal class ScreenLockViewModel : INotifyPropertyChanged
    {
        private object? _loginUserControl;
        private ApplicationState? _applicationState;

        private LoginFormViewModel _loginFormViewModel;
        private InputPasswordViewModel _inputPasswordViewModel;
        private LoadingViewViewModel _loadingViewViewModel;
        private UserInfoViewModel _userInfoViewModel;
        private bool? _dialogResult;

        public ICommand ShowInputPasswordCommand { get; }
        public event Action CloseWindowEvent;

        public object? UserControl
        {
            get { return _loginUserControl; }
            set
            {
                _loginUserControl = value;
                OnPropertyChanged(nameof(UserControl));
            }
        }

        public bool? DialogResult { 
            get
            {
                return _dialogResult;
            } 
        }


        public ScreenLockViewModel(ApplicationState state)
        {
            _applicationState = state;
            _loginFormViewModel = new LoginFormViewModel(state, this);
            _inputPasswordViewModel = new InputPasswordViewModel(this);
            _loadingViewViewModel = new LoadingViewViewModel();
            _userInfoViewModel = new UserInfoViewModel(state, this);
            ShowInputPasswordCommand = new SimpleCommand(() => ShowInputPassword());
            UserControl = _loginFormViewModel;
            _dialogResult = null;

        }
        public void CloseWindowAdmin()
        {
            Debug.WriteLine("Cerrando ventana Admin");
            _dialogResult = false;
            CloseWindowEvent?.Invoke();
        }
        public void CloseWindowUser()
        {
            Debug.WriteLine("Cerrando ventana User");
            _dialogResult = true;
            CloseWindowEvent?.Invoke();
        }
        public async Task CloseInputPassword()
        {
            Debug.WriteLine("Cambiando vista LoginForm");
            UserControl = _loginFormViewModel;
            return;
        }
        public void ShowInputPassword()
        {
           UserControl = _inputPasswordViewModel;
        }

        public void ShowMessageUser(string message)
        {
            _loginFormViewModel.Message = message;
            UserControl = _loginFormViewModel;
            return ;
        }

        public async Task FetchRegisterUser(string userNumber, string curp)
        {

            UserControl = _loadingViewViewModel;
            string date = DateTime.Now.ToString("yyy/MM/dd HH:mm:ss");
            ApiService apiService = new ApiService();


            JsonObject response = await apiService.RegisterUserLogin(userNumber, curp, date);

            switch ((int)response["statusCode"])
            {
                case (Session.REGISTER_LOGIN_OK):
                    {
                        User user = new User(userNumber, curp, response["nombreCompleto"], User.USER_NORMAL);
                        user.Department = response["carrera"];
                        user.PkCategoria = response["pkCategoria"];
                        user.PkUsuario = response["pkUsuario"];
                        user.UriPhoto = response["foto"];
                        Session s = new Session(user);
                        s.DateIn = date;
                        s.PkEquipo = response["pkEquipo"];
                        s.PkRegistro = response["idRegistro"];
                        s.StatusCode = Session.REGISTER_LOGIN_OK;

                        string? verifyLocalUser =  LocalDatabaseService.VerifyUser(userNumber, curp);
                        Debug.WriteLine(verifyLocalUser);
                        if (string.IsNullOrEmpty(verifyLocalUser))
                        {
                            LocalDatabaseService.InsertNewUser(userNumber, curp, user.Name.ToUpper());
                        }
                        _userInfoViewModel.Department = user.Department;
                        _userInfoViewModel.UserName = user.Name;
                        _userInfoViewModel.Photo = await apiService.GetUserImage(user.UriPhoto);


                        _applicationState.Session = s;
                        UserControl = _userInfoViewModel;
                    }
                    break;
                case (Session.REGISTER_LOGIN_PENALTY):
                {
                    _loginFormViewModel.Message = response["message"];
                    UserControl = _loginFormViewModel;
                }
                    break;
                case Session.REGISTER_USER_NOT_FOUND:
                    {
                        _loginFormViewModel.Message = response["message"];
                        UserControl = _loginFormViewModel;
                    }
                    break;
                case Session.REGISTER_USER_LOCAL_NOT_FOUND:
                    {
                        _loginFormViewModel.Message = "Error de conexion: Usuario no encontrado";
                        UserControl = _loginFormViewModel;
                    }
                    break;
                case Session.REGISTER_LOGIN_ERROR:
                    {
                        if (((string)response["message"]).Contains("Invalid value"))
                        {
                            _loginFormViewModel.Message = "Boleta, Numero de empleado o CURP incorrectos. Por favor verifique sus datos.";
                        }
                        else
                        {
                            _loginFormViewModel.Message = response["message"];
                        }
                        
                        UserControl = _loginFormViewModel;
                    }
                    break;
                case Session.REGISTER_LOGIN_LOCAL_OK:
                    {
                        User user = new User(response["userNumber"], response["curp"]);
                        user.Name = response["fullName"];
                        Session s = new Session(user);
                        s.DateIn = date;
                        _applicationState.Session = s;
                        s.StatusCode = Session.REGISTER_LOGIN_LOCAL_OK;
                        _applicationState.SessionsIn.Add(s);

                        _userInfoViewModel.Department = "ENCB";
                        _userInfoViewModel.UserName = user.Name;
                        _userInfoViewModel.Photo = await apiService.GetUserImage("");
                        UserControl = _userInfoViewModel;
                    }
                    break;
                case Session.REGISTER_LOGIN_ACTIVE:
                    {
                        JsonObject resultLogout = await apiService.RegisterUserLogoutByUser(userNumber, curp, date);

                        if (resultLogout["statusCode"] == Session.REGISTER_LOGOUT_OK)
                        {
                            await FetchRegisterUser(userNumber, curp);
                        }
                        else
                        {
                            _loginFormViewModel.Message = response["message"];
                            UserControl = _loginFormViewModel;
                        }
                    }
                    break;
            }
        }

        public async Task FetchRegisterUserLogoutByUser()
        {
            if(_applicationState.Session == null)
            {
                _loginFormViewModel.CleanLogin();
                _applicationState.Session = null;
                UserControl = _loginFormViewModel;
            }
            if(_applicationState.Session.PkRegistro == -1)
            {
                _loginFormViewModel.CleanLogin();
                _applicationState.SessionsIn.Remove(_applicationState.Session);
                _applicationState.Session = null;
                UserControl = _loginFormViewModel;
            }
            UserControl = _loadingViewViewModel;
            string date = DateTime.Now.ToString("yyy/MM/dd HH:mm:ss");
            _applicationState.Session.DateOut = date;
            ApiService apiService = new ApiService();
            JsonObject response = await apiService.RegisterUserLogoutByUser(_applicationState.Session.User.UserNumber, _applicationState.Session.User.Curp, date);
            Debug.WriteLine(response.ToString());
            
            switch ((int)response["statusCode"])
            {
                case Session.REGISTER_LOGOUT_OK:
                    {

                        _loginFormViewModel.CleanLogin();
                        _applicationState.Session = null;
                        UserControl = _loginFormViewModel;
                    }
                break;
                case Session.REGISTER_LOGOUT_ERROR:
                    {
                        _loginFormViewModel.CleanLogin();
                        _loginFormViewModel.Message = response["message"];
                        _applicationState.Session.StatusCode = Session.REGISTER_LOGOUT_BY_USER_ERROR;
                        _applicationState.SessionOut.Add(_applicationState.Session);
                        _applicationState.Session = null;
                        UserControl = _loginFormViewModel;
                    }
                break;

            }
            
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
