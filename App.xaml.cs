using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AulasSiencb2.Models;
using AulasSiencb2.Services;
using AulasSiencb2.ViewModel;
using AulasSiencb2.Views;
using Hardcodet.Wpf.TaskbarNotification;
using Serilog;
using Serilog.Core;

namespace AulasSiencb2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon taskbarIcon;
        private MenuItem itemShutdown, itemScreenLock, itemConfig, itemLogout, itemService;
        private MenuItem[] arrayItemUser, arrayItemAdmin;
        private ContextMenu menu;
        private ApplicationState _actualState;
        private Task? backRegistry;
        private CancellationTokenSource? cts;


        protected override void OnStartup(StartupEventArgs e)
        {
            var procesos = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);

            if (procesos.Length > 1)
            {
                MessageBox.Show("La aplicación ya está en ejecución.");
                Application.Current.Shutdown();
                return;
            }
            base.OnStartup(e);
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            
            itemShutdown.Click += ShutDown;

            itemScreenLock.Click += LockScreen;

            itemLogout.Click += LogOut;

            itemService.Click += Services;

            taskbarIcon.Icon = new System.Drawing.Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/imgs/favicon.ico"));
            LoadOptionsTaskBarIcon(arrayItemUser);


            LockScreen(this, new EventArgs());

        }

        
        public App()
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.Console()
               .WriteTo.File("logs/log.txt",
                           outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
               .CreateLogger();
            Log.Information("App Iniciada");

            ApiService api = new ApiService();

            
            _actualState = new();
            taskbarIcon = new TaskbarIcon();

            itemShutdown = new MenuItem
            {
                Header = "Finalizar Aplicacion"
            };

            itemScreenLock = new MenuItem
            {
                Header = "Bloquear"
            };

            itemConfig = new MenuItem
            {
                Header = "Configurar"
            };

            itemLogout = new MenuItem
            {
                Header = "Registrar Salida"
            };

            itemService = new MenuItem
            {
                Header = "Registrar Servicio"
            };

            arrayItemAdmin = new MenuItem[] { itemShutdown, itemConfig, itemScreenLock };
            arrayItemUser = new MenuItem[] { itemLogout, itemService };
            menu = new ContextMenu();
            backRegistry = null;
            cts = null;
        }

        private void LoadOptionsTaskBarIcon(MenuItem[] menuItems)
        {
            menu.Items.Clear();
            foreach (MenuItem item in menuItems)
            {
                menu.Items.Add(item);   
            }
            taskbarIcon.ContextMenu = menu;
        }

        private void ShutDown(object sender, EventArgs e)
        {
            //Debug.WriteLine("ShutDown");
            this.Shutdown();
        }
        private void LockScreen(object sender, EventArgs e)
        {
            //InitDaemon();
            ScreenLock screenLock = new();
            screenLock.DataContext = new ScreenLockViewModel(_actualState);
            screenLock.Configure();
            screenLock.WindowState = WindowState.Maximized;
            bool? userType = screenLock.ShowDialog();
            //StopDaemon();
            if (userType == true)
            {
                LoadOptionsTaskBarIcon(arrayItemUser);
            }
            else
            {
                LoadOptionsTaskBarIcon(arrayItemAdmin);
            }
        }

        private void LogOut(object sender, EventArgs e)
        {
            if(_actualState.Session.StatusCode == Session.REGISTER_LOGIN_LOCAL_OK)
            {
                LockScreen(this, new EventArgs());
            }
            else
            {
                ScreenLogOutViewmodel scrrenLogoutViewModel = new ScreenLogOutViewmodel(_actualState);
                ScreenLogOut screenLogOut = new ScreenLogOut();
                screenLogOut.DataContext = scrrenLogoutViewModel;
                screenLogOut.Configure();

                bool? resultado = screenLogOut.ShowDialog();

                if (resultado == true)
                {
                    LockScreen(this, new EventArgs());
                }
            }
            
        }

        private void Services(object sender, EventArgs e)
        {
            ScreenServices screenService = new();
            screenService.DataContext = new ScreenServiceViewModel(_actualState);
            screenService.Configure();
            bool? resultado = screenService.ShowDialog();
            Debug.WriteLine($"Resultado: {resultado}");
        }

        private void InitDaemon()
        {
            try
            {
                if(_actualState.SessionsIn.Count <= 0 && _actualState.SessionOut.Count <= 0)
                {
                    return;
                }
                if (backRegistry != null)
                {
                    if (cts != null)
                    {
                        cts.Cancel();
                    }
                }
                cts = new CancellationTokenSource();
                backRegistry = Task.Run(() => RegistryLocalSessions(_actualState.SessionsIn, _actualState.SessionOut, cts.Token), cts.Token);
                Log.Information("Demonio de registro iniciado.");
                Debug.WriteLine("Demonio de registro iniciado.");
                System.GC.Collect();

            }
            catch (Exception ex)
            {
                Log.Error($"Error al iniciar el demonio de registro: {ex.Message} - {ex.StackTrace}");
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);

            }
        }
        private void StopDaemon()
        {
            try
            {
                if (backRegistry != null)
                {
                    if (cts != null)
                    {
                        cts.Cancel();
                    }
                    backRegistry = null;
                    cts = null;
                    Log.Information("Demonio de registro detenido.");
                    Debug.WriteLine("Demonio de registro detenido.");
                    System.GC.Collect();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error al detener el demonio de registro: {ex.Message} - {ex.StackTrace}");
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);

            }
        }
        private async void RegistryLocalSessions(List<Session> sessionsIn, List<Session> sessionsOut, CancellationToken ct)
        {
            try
            {
                ApiService api = new ApiService();

                Debug.WriteLine($"Numero de registros de entrada: {sessionsIn.Count}");
                foreach (Session session in sessionsIn)
                {
                    int statusCode = Session.REGISTER_NULL;
                    do
                    {
                        if (ct.IsCancellationRequested)
                        {
                            return;
                        }
                        string log = $"Registrando entrada: {session.User.UserNumber} - {session.User.Curp}";
                        Debug.WriteLine(log);
                        Log.Information(log);
                        JsonObject response = await api.RegisterUserLogin(session.User.UserNumber, session.User.Curp, session.DateIn);
                        statusCode = (int)response["statusCode"];

                        if (statusCode != Session.REGISTER_LOGIN_OK)
                        {
                            Thread.Sleep(2000);
                        }
                    } while (statusCode != Session.REGISTER_LOGIN_OK);

                }

                Debug.WriteLine($"Numero de registros de entrada: {sessionsOut.Count}");
                foreach (Session session in sessionsOut)
                {
                    int statusCode = Session.REGISTER_NULL;
                    do
                    {
                        if (ct.IsCancellationRequested)
                        {
                            return;
                        }
                        string log = $"Registrando salida: {session.User.UserNumber} - {session.User.Curp}";
                        Debug.WriteLine(log);
                        Log.Information(log);
                        JsonObject response = await api.RegisterUserLogoutByUser(session.User.UserNumber, session.User.Curp, session.DateOut);
                        statusCode = (int)response["statusCode"];
                        if (statusCode != Session.REGISTER_LOGOUT_OK)
                        {
                            Thread.Sleep(2000);
                        }
                    } while (statusCode != Session.REGISTER_LOGOUT_OK);

                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error al registrar las sesiones locales: {ex.Message} - {ex.StackTrace}");
                Debug.WriteLine(ex.Message);
            }

        }
    }

}
