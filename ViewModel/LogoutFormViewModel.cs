using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AulasSiencb2.Models;
using AulasSiencb2.Resources;
using AulasSiencb2.Services;
using AulasSiencb2.Views;
using Serilog;

namespace AulasSiencb2.ViewModel
{
    internal class Opcion
    {
        public string NameOption { get; set; } 
        public bool IsChecked { get; set; }
        public JsonObject Data { get; set; }
    }
    internal class LogoutFormViewModel : INotifyPropertyChanged
    {
        private ApplicationState applicationState;
        private ScreenLogOutViewmodel screenLogOutViewmodel;
        private bool _dialogResult;
        private bool _userPenalty = false;

        public bool UserPenalty
        {
            get { return _userPenalty; }
            set { _userPenalty = value; Debug.WriteLine($"PENALTY: {_userPenalty}"); OnPropertyChanged(nameof(UserPenalty)); }
        }

        private UsePC _usePcAvaible {  get; set; } 
        private string _userName {  get; set; }
        private string _department {  get; set; }

        private ObservableCollection<Opcion> _services { get; set; }
        private ObservableCollection<Opcion> _programs {  get; set; }  

        public ObservableCollection<Opcion> Services
        {
            get { return _services; }
            set { _services = value; 
                  OnPropertyChanged(nameof(Services));
                 }

        }

        public ObservableCollection<Opcion> Programs
        {
            get { return _programs; }
            set
            {
                _programs = value;
                OnPropertyChanged(nameof(Programs));
            }

        }

        public string Department
        {
            get { return _department; }
            set { _department = value; 
                    OnPropertyChanged(nameof(Department));
                }
        }

        public string UserName
        {
            get { return _userName; }
            set { _userName = value;
                  OnPropertyChanged(nameof(UserName)); 
                  }
        }
        public UsePC UsePcAvaible
        {
            get { return _usePcAvaible; }
            set
            {
                _usePcAvaible = value;
            }
        }


        public ICommand RegistryUserLogoutCommand { get; set; }

        public LogoutFormViewModel(ApplicationState state, ScreenLogOutViewmodel viewModel)
        {
            applicationState = state;
            
            screenLogOutViewmodel = viewModel;
            RegistryUserLogoutCommand = new SimpleCommand(() => RegistryUserLogout());
        }

        public async void RegistryUserLogout()
        {
            ApiService api = new ApiService();
            JsonObject response;
            
            if (_userPenalty)
            {
                InputPasswordDialog inputPasswordDialog = new InputPasswordDialog();
                if ((bool)inputPasswordDialog.ShowDialog())
                {
                    if (applicationState.Session.StatusCode == Session.REGISTER_LOGIN_LOCAL_OK)
                    {
                        applicationState.Session.penalty = true;
                        applicationState.SessionOut.Add(applicationState.Session);
                        screenLogOutViewmodel.CloseWindow(true);
                    }
                    else
                    {
                        api = new ApiService();
                        User us = applicationState.Session.User;
                        string dateByUser = DateTime.Now.ToString("yyy/MM/dd HH:mm:ss");
                        response = await api.RegisterUserLogoutByUser(us.UserNumber, us.Curp, dateByUser);
                        if (response["statusCode"] != Session.REGISTER_LOGOUT_OK)
                        {
                            applicationState.Session.penalty = true;
                            applicationState.SessionOut.Add(applicationState.Session);
                            //MessageBox.Show("Error al registrar la salida. Por favor consulta a la unidad de informatica");
                        }
                        else
                        {
                            MessageBox.Show("No se pudo registrar la salida del usuario. Intentelo mas tarde");
                            return;
                        }
                        screenLogOutViewmodel.CloseWindow(true);
                    }
                        
                }
                inputPasswordDialog = null;
                System.GC.Collect();
                return;
            }

            screenLogOutViewmodel.ShowLoading();
            JsonObject dataServices = new JsonObject();
            foreach (Opcion op in Services)
            {
                if (op.IsChecked)
                {
                    dataServices = op.Data;
                    break;
                }
            }

            JsonArray dataPrograms = new JsonArray();
            foreach (Opcion op in Programs)
            {
                if (op.IsChecked)
                {
                    dataPrograms.Add(op.Data);
                }
            }

            JsonObject data = new JsonObject();

            data["servicios"] = dataServices;
            data["paqueteria"] = dataPrograms;
            string date = DateTime.Now.ToString("yyy/MM/dd HH:mm:ss");
            if (applicationState.Session.StatusCode == Session.REGISTER_LOGIN_LOCAL_OK)
            {
                applicationState._data = data;
                applicationState.Session.StatusCodeOut = Session.REGISTER_LOGOUT_ERROR_NOT_FOUND;
                applicationState.Session.DateOut = date;
                applicationState.SessionOut.Add(applicationState.Session);
                screenLogOutViewmodel.CloseWindow(true);
            }

            
            
            applicationState.Session.DateOut = date;
            response = await api.RegisterUserLogout(applicationState.Session, data, date);

            switch((int)response["statusCode"])
            {
                case Session.REGISTER_LOGOUT_OK:
                    Debug.WriteLine("Logout OK");
                    screenLogOutViewmodel.CloseWindow(true);
                    return;
                case Session.REGISTER_LOGOUT_ERROR_NOT_FOUND:
                    if(applicationState.Session.StatusCode == Session.REGISTER_LOGIN_LOCAL_OK)
                    {
                        applicationState._data = data;
                        applicationState.Session.StatusCodeOut = Session.REGISTER_LOGOUT_ERROR_NOT_FOUND;
                        applicationState.SessionOut.Add(applicationState.Session);
                        screenLogOutViewmodel.CloseWindow(true);
                    }
                    else
                    {
                        screenLogOutViewmodel.ShowForm();
                        MessageBox.Show("Error al registrar la salida. Por favor consulta a la unidad de informatica");
                    }
                    break;
                case Session.REGISTER_LOGOUT_ERROR_BAD_REQUEST:
                    Debug.WriteLine($"BadReQuest: {response["message"]}");
                    applicationState.Session.StatusCodeOut = Session.REGISTER_LOGOUT_ERROR_BAD_REQUEST;
                    applicationState.SessionOut.Add(applicationState.Session);
                    screenLogOutViewmodel.CloseWindow(true);
                    break;
                default:
                    if (applicationState.Session.StatusCode == Session.REGISTER_LOGIN_LOCAL_OK)
                    {
                        applicationState._data = data;
                        applicationState.Session.StatusCodeOut = Session.REGISTER_LOGOUT_ERROR_NOT_FOUND;
                        applicationState.SessionOut.Add(applicationState.Session);
                        screenLogOutViewmodel.CloseWindow(true);
                    }
                    else
                    {
                        screenLogOutViewmodel.ShowForm();
                        MessageBox.Show("Error al registrar la salida. Por favor consulta a la unidad de informatica");
                    }
                    break;
            }
        }
        public void LoadOptions()
        {
            try
            {
                UserName = applicationState.Session.User.Name;
                Department = applicationState.Session.User.Department;
            }
            catch (Exception ex) { 
            
                Log.Error($"Error al cargar los datos del usuario en la salida: {ex.Message} - {ex.StackTrace}");
            }
            ObservableCollection<Opcion> tempServices = new ObservableCollection<Opcion>();
            foreach (JsonObject jo in UsePcAvaible.Services)
            {
                tempServices.Add(new Opcion { NameOption = jo["NOMBRE_SERVICIO"], Data = jo, IsChecked=false});
            }
            Services = tempServices;

            ObservableCollection<Opcion> tempPrograms = new ObservableCollection<Opcion>();
            foreach (JsonObject jo in UsePcAvaible.Programs)
            {
                tempPrograms.Add(new Opcion { NameOption = jo["NOMBRE_PROGRAMA"], Data = jo, IsChecked = false });
            }
            Programs = tempPrograms;


        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
