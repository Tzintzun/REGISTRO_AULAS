using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AulasSiencb2.Models;
using Serilog;
using System.Windows.Media.Imaging;
using FluentFTP;
using System.Json;
using System.Configuration;
using UrlCombineLib;
using System.Net.Http.Headers;
using AulasSiencb2.ViewModel;

namespace AulasSiencb2.Services
{
    internal class ApiService
    {
        private const string urlLogin = "login";
        private const string urlLogout = "logout";
        private const string urlLogoutUsuario = "logout/usuario";
        private const string urlPaqueteria = "paqueteria";
        private const string urlSancionar = "sancionar";
        private const string urlMonedero = "monedero";
        private const string urlServicios = "productos/udi";
        private const string urlSolicitarImpresion = "solicitar/impresion";
        public async Task<JsonObject> RegisterUserLogin(string userNumber, string curp, string time)
        {
            try
            {
                //string endpoint = new Uri(new Uri(Properties.Settings.Default.UrlServer), urlLogin).ToString();
                string endpoint = UrlCombine.Combine(Properties.Settings.Default.UrlServer, urlLogin);
                Debug.WriteLine(endpoint);
                //string data = "{ 'numID': '" + userNumber + "', 'CURP': '" + curp + "', 'FECHA_HORA': '" + time + "', 'IP': '" + GetIPLocal() + "'}";
                var data = new JsonObject
                {
                    {"numID", userNumber },
                    {"CURP", curp },
                    {"FECHA_HORA", time },
                    {"IP", GetIPLocal()}
                };
                HttpResponseMessage response = await this.PostAsync(endpoint, data.ToString());
                JsonObject dataResponse = new JsonObject();
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Created:
                        {
                            string jsonString = await response.Content.ReadAsStringAsync();
                            JsonValue value = JsonObject.Parse(jsonString);
                           
                            dataResponse = (JsonObject)value;
                            dataResponse["statusCode"] = Session.REGISTER_LOGIN_OK;
                            Log.Information($"Usuario {userNumber} registró Entrada");
                        }

                        break;
                    case HttpStatusCode.NotFound:
                        {
                            dataResponse["statusCode"] = Session.REGISTER_USER_NOT_FOUND;
                            dataResponse["message"] = await response.Content.ReadAsStringAsync();
                            Log.Information($"No se pudo registrar al usuario ({404}): {userNumber} - {curp}\n\t{dataResponse["message"]}");
                        }
                        break;
                    case HttpStatusCode.OK:
                        {
                            dataResponse["statusCode"] = Session.REGISTER_LOGIN_PENALTY;
                            dataResponse["message"] = await response.Content.ReadAsStringAsync();
                            Log.Information($"El usuario: {userNumber} - {curp} Cuenta con una sancion:\n\t{dataResponse["message"]}");
                        }
                        break;

                    case HttpStatusCode.BadRequest:
                        {
                            string message = await response.Content.ReadAsStringAsync();
                            dataResponse["message"] = message;
                            if (message.Contains("sesión abierta"))
                            {
                                dataResponse["statusCode"] = Session.REGISTER_LOGIN_ACTIVE;
                                Debug.WriteLine("Sesion abierta");
                                Log.Information($"El usuario (400): {userNumber} - {curp} cuenta con una sesión abierta:");

                            }
                            else
                            {
                                dataResponse["statusCode"] = Session.REGISTER_LOGIN_ERROR;
                                Log.Information($"El usuario (400): {userNumber} - {curp} no puede iniciar secion porque: {dataResponse["message"]}");

                            }
                        }
                        break;
                    case HttpStatusCode.InternalServerError:
                        {
                            Log.Information($"Error interno (500) del servidor al intentar Registrar sesion usuario {userNumber} - {curp}");
                            dataResponse = GetLocalUser(userNumber, curp);
                        }
                        break;
                    default:
                        {
                            Log.Information($"Codigo de respuesta desconocido ({response.StatusCode}) al intentar Registrar sesion usuario {userNumber} - {curp}");

                            dataResponse = GetLocalUser(userNumber, curp);
                        }
                        break;

                }
                return dataResponse;

            }
            catch (Exception ex)
            {
                Log.Error($"Error al intentar Registrar sesion usuario {userNumber} - {curp}", ex.Message);
                Debug.WriteLine(ex.StackTrace);
                return GetLocalUser(userNumber, curp);
            }


        }

        public async Task<JsonObject> RegisterUserLogoutByUser(string userNumber, string curp, string time)
        {
            try
            {
                string endpoint = UrlCombine.Combine(Properties.Settings.Default.UrlServer, urlLogoutUsuario);
                var data = new JsonObject {
                    {"numID", userNumber },
                    {"CURP", curp },
                    {"FECHA_HORA", time },
                };
                HttpResponseMessage response = await this.PostAsync(endpoint, data.ToString());
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Log.Information($"{response.Content.ReadAsStringAsync().Result}. Del usuario: {userNumber} - {curp}");
                    return new JsonObject
                    {
                        {"statusCode", Session.REGISTER_LOGOUT_OK }
                    };
                }
                else
                {
                    Log.Error($"Error al intentar registrar salida de sesion usuario {userNumber} - {curp} \n\t{response.Content.ReadAsStringAsync().Result}");
                    return new JsonObject
                    {
                        {"statusCode", Session.REGISTER_LOGOUT_ERROR },
                        {"message", "El usuario cuenta con una sesión activa.\nError al intentar cerrar la sesión activa.\nConsulta al personal de la Unidad de Informática"}
                    };
                }
                
            }
            catch (Exception ex)
            {
                Log.Error($"Error al intentar registrar salida de sesion usuario {userNumber} - {curp}", ex.Message);
                Debug.WriteLine(ex.StackTrace);
                return new JsonObject
                {
                    {"statusCode", Session.REGISTER_LOGOUT_ERROR},
                    {"message", "El usuario cuenta con una sesión activa.\nError al intentar cerrar la sesión activa.\nConsulta al personal de la Unidad de Informática"}
                };
            }
            
        }

        public async Task<JsonObject> RegisterUserLogout(Session session, JsonObject dataUsePc, string time)
        {
            try
            {
                string endpoint = UrlCombine.Combine(Properties.Settings.Default.UrlServer, urlLogout);
                JsonObject data = new JsonObject
                {
                    {"USUARIO", session.User.PkUsuario},
                    {"CURP", session.User.Curp },
                    {"EQUIPO", session.PkEquipo },
                    {"REGISTRO", session.PkRegistro },
                    {"FECHA_HORA", time },
                    {"CATEGORIA", session.User.PkCategoria },
                    {"SERVICIOS", dataUsePc }
                };

                HttpResponseMessage response = await this.PostAsync(endpoint, data.ToString());
                Debug.WriteLine(response.StatusCode);
                JsonObject dataResponse = new JsonObject();

                switch (response.StatusCode)
                {
                    case HttpStatusCode.Created:
                        {
                            dataResponse["statusCode"] = Session.REGISTER_LOGOUT_OK;
                            Log.Information($"Usuario {session.User.UserNumber} registró Salida");
                        }
                        break;
                    case HttpStatusCode.NotFound:
                        {
                            dataResponse["statusCode"] = Session.REGISTER_LOGOUT_ERROR_NOT_FOUND;
                            dataResponse["message"] = await response.Content.ReadAsStringAsync();
                            Log.Information($"No se pudo registrar la salida del usuario (404): {session.User.UserNumber} - {session.User.Curp}\n\t{dataResponse["message"]}");
                        }
                        break;
                    case HttpStatusCode.BadRequest:
                        {
                            dataResponse["statusCode"] = Session.REGISTER_LOGOUT_ERROR_BAD_REQUEST;
                            dataResponse["message"] = await response.Content.ReadAsStringAsync();
                            Log.Information($"No se pudo registrar la salida del usuario (400): {session.User.UserNumber} - {session.User.Curp}\n\t{dataResponse["message"]}");
                        }
                        break;
                    default:
                        {
                            dataResponse["statusCode"] = Session.REGISTER_LOGOUT_ERROR;
                            dataResponse["message"] = "Error al intentar cerrar la sesión. Inténtalo de nuevo o consulte a la unidad de informática.";
                            Log.Information($"No se pudo registrar la salida del usuario ({response.StatusCode}): {session.User.UserNumber} - {session.User.Curp}\n\tCodigo desconocido: {dataResponse["message"]}");
                        }
                        break;
                }
                return dataResponse;
            }
            catch(Exception ex)
            {
                Log.Error($"Error al intentar cerrar sesion usuario {session.User.UserNumber} - {session.User.Curp} :\n\t{ex.Message}\n\t{ex.StackTrace}");
                JsonObject dataResponse = new JsonObject();
                dataResponse["statusCode"] = Session.REGISTER_LOGOUT_ERROR;
                dataResponse["message"] = "Error al intentar cerrar la sesión. Inténtalo de nuevo o consulte a la unidad de informática.";
                return dataResponse;
            }
        }

        public async Task<JsonObject> RegistryUserPenalty(Session session)
        {
            try
            {
                string endpoint = UrlCombine.Combine(Properties.Settings.Default.UrlServer, urlSancionar);
                JsonObject data = new JsonObject
                {
                    {"ID_USUARIO", session.User.PkUsuario},
                    {"CATEGORIA", session.User.PkCategoria },
                    {"PK_REGISTRO", session.PkRegistro }
                };

                HttpResponseMessage response = await this.PostAsync(endpoint, data.ToString());

                switch (response.StatusCode) { 
                    case HttpStatusCode.Created:
                        {
                            Log.Information($"Sancion registrada para el usuario: {session.User.UserNumber} - {session.User.Curp}");
                            return new JsonObject
                            {
                                {"statusCode", Session.REGISTER_LOGOUT_PENALTY }
                            };
                        }
                        break;
                    case HttpStatusCode.BadRequest:
                        {
                            Log.Information($"Error al intentar registrar sancion para el usuario: {session.User.UserNumber} - {session.User.Curp}\n\t{response.Content.ReadAsStringAsync()}");
                            return new JsonObject
                            {
                                {"statusCode", Session.REGISTER_LOGOUT_BY_USER_ERROR }
                            };
                        }
                        break;
                    default:
                        {
                            Log.Information($"Error al intentar registrar sancion para el usuario: {session.User.UserNumber} - {session.User.Curp}\n\t{response.Content.ReadAsStringAsync()}");
                            return new JsonObject
                            {
                                {"statusCode", Session.REGISTER_LOGOUT_BY_USER_ERROR }
                            };
                        }
                        break;
                }

            }
            catch(Exception e)
            {
                Log.Error($"Error al intentar registrar sancion para el usuario: {session.User.UserNumber} - {session.User.Curp}\n\t{e.Message}\n\t{e.StackTrace}");
                return new JsonObject
                {
                    {"statusCode", Session.REGISTER_LOGOUT_PENALTY_ERROR }
                };  

            }
        }
        private JsonObject GetLocalUser(string number, string curp)
        {
            JsonObject dataResponse = new JsonObject();
            string? name = LocalDatabaseService.VerifyUser(number, curp);
            if (!string.IsNullOrEmpty(name))
            {
                dataResponse["statusCode"] = Session.REGISTER_LOGIN_LOCAL_OK;
                dataResponse["fullName"] = name;
                dataResponse["userNumber"] = number;
                dataResponse["curp"] = curp;
            }
            else
            {
                dataResponse["statusCode"] = Session.REGISTER_USER_LOCAL_NOT_FOUND;
                dataResponse["message"] = "Error al conectarse al servidor, inténtalo más tarde.";
            }
            Debug.WriteLine(dataResponse.ToString());
            return dataResponse;
        }

        /*public async Task<BitmapImage> GetUserImage(string pathFtpImage, string userNumber)
        {
            BitmapImage bitmap = new BitmapImage();
            
            try
            {
                string endpoint = UrlCombine.Combine(Properties.Settings.Default.UrlServerPhotos, userNumber);

                HttpResponseMessage response = await GetAsync(endpoint);
                if(response != null)
                {
                    Debug.WriteLine(response.Content.Headers.ContentType);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        if(response.Content.Headers.ContentType.ToString().Contains("image/jpeg"))
                        {
                            byte[] data = await response.Content.ReadAsByteArrayAsync();
                            MemoryStream stream = new MemoryStream(data);
                            bitmap.BeginInit();
                            bitmap.StreamSource = stream;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            return bitmap;
                        }
                        

                    }
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Error("Errror al descargar imagen: " + e);
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,,/Assets/imgs/default_user.png"); // Asignar el stream
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Cargar inmediatamente
                bitmap.EndInit();
                return bitmap;
            }
            bitmap.BeginInit();
            bitmap.UriSource = new Uri("pack://application:,,,/Assets/imgs/default_user.png"); // Asignar el stream
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // Cargar inmediatamente
            bitmap.EndInit();
            return bitmap;
        }*/

        public async Task<BitmapImage> GetUserImage(string pathFtpImage)
        {
            string zipPath = Path.GetTempPath();
            string directorioSalida = System.IO.Path.Combine(Path.GetTempPath(), "Descomprimido");
            
            if (System.IO.Path.Exists(zipPath + pathFtpImage))
            {
                for(int i = 0; i < 10; i++)
                {
                    try
                    {
                        File.Delete(zipPath + pathFtpImage);
                        break;
                    }
                    catch (Exception e)
                    {
                        Log.Error("Error al eliminar archivo zip: " + e.ToString());
                        await Task.Delay(1000);
                    }
                }
            }
            if (System.IO.Path.Exists(directorioSalida))
            {
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        Directory.Delete(directorioSalida, true);
                        break;
                    }
                    catch (Exception e)
                    {
                        Log.Error("Error al eliminar directorio: " + e.ToString());
                        await Task.Delay(1000);
                    }
                }
            }


            BitmapImage bitmap = new BitmapImage();
            if (String.IsNullOrEmpty(pathFtpImage))
            {

                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,,/Assets/imgs/default_user.png"); // Asignar el stream
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Cargar inmediatamente
                bitmap.EndInit();
                return bitmap;
            }
            try
            {


                Directory.CreateDirectory(directorioSalida);

                using (FtpClient cliente = new FtpClient(Properties.Settings.Default.ftpServer, "siencb_fotografias", "siE0T61as#16"))
                {
                    cliente.AutoConnect();
                    if (cliente.FileExists(pathFtpImage))
                    {
                        cliente.DownloadFile(zipPath + pathFtpImage, pathFtpImage);
                    }
                }
                Serilog.Log.Information("Ruta direcotrio: " + directorioSalida);
                ZipFile.ExtractToDirectory(zipPath + pathFtpImage, directorioSalida);

                string? imagenPath = Directory.GetFiles(directorioSalida, "*.jpg").FirstOrDefault();

                if (!string.IsNullOrEmpty(imagenPath))
                {

                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagenPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    return bitmap;
                }
                File.Delete(zipPath + pathFtpImage);
                Directory.Delete(directorioSalida, true);

            }
            catch (Exception e)
            {
                Serilog.Log.Error("Errror al descargar imagen: " + e);
                File.Delete(zipPath + pathFtpImage);
                Directory.Delete(directorioSalida, true);
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,,/Assets/imgs/default_user.png"); // Asignar el stream
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Cargar inmediatamente
                bitmap.EndInit();
                return bitmap;
            }
            bitmap.BeginInit();
            bitmap.UriSource = new Uri("pack://application:,,,/Assets/imgs/default_user.png"); // Asignar el stream
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // Cargar inmediatamente
            bitmap.EndInit();
            return bitmap;
        }


        public async Task<JsonObject> GetPcPackaging(int pkPc)
        {
            string endpoint = UrlCombine.Combine(Properties.Settings.Default.UrlServer, urlPaqueteria, pkPc.ToString());
            HttpResponseMessage result = await GetAsync(endpoint);

            Debug.WriteLine("Status code: ",result.StatusCode);
            JsonObject? dataResponse;
            switch (result.StatusCode)
            {
                case HttpStatusCode.OK:
                    {
                        string data = await result.Content.ReadAsStringAsync();
                        Debug.WriteLine(data);
                        dataResponse = (JsonObject.Parse(data) as JsonObject);
                        if(dataResponse != null)
                        {
                            Properties.Settings.Default.UsePC = dataResponse.ToString();
                            Log.Information($"Paqueteria del PC: {pkPc}. Guardada corretamente");
                            dataResponse["statusCode"] = UsePC.GET_USEPC_OK;
                            Log.Information($"Paqueteria del PC: {pkPc}. Obtenida Correctamente");
                        }
                        else
                        {
                            string dataLocal = Properties.Settings.Default.UsePC.Trim();
                            if (!string.IsNullOrEmpty(dataLocal))
                            {
                                dataResponse = (JsonObject.Parse(dataLocal) as JsonObject);
                                if (dataResponse != null)
                                {
                                    dataResponse["statusCode"] = UsePC.GET_USEPC_OK;
                                    Log.Information($"Paqueteria del PC: {pkPc}. Obtenida de manera local");
                                }
                                else
                                {
                                    dataResponse = new JsonObject();
                                    dataResponse["statusCode"] = UsePC.GET_USEPC_ERROR;
                                    dataResponse["message"] = "No se pudo cargar la paquetería. Inténtelo más tarde o consulte al personal de la UDI.";
                                    Log.Error($"Paqueteria del PC: {pkPc}. No se pudo contevertir de String a JsonObject");
                                }
                            }
                            else
                            {
                                dataResponse = new JsonObject();
                                dataResponse["statusCode"] = UsePC.GET_USEPC_ERROR;
                                dataResponse["message"] = "No se pudo cargar la paquetería. Inténtelo más tarde o consulte al personal de la UDI.";
                                Log.Error($"Paqueteria del PC: {pkPc}. No se pudo obtener de manera local");
                            }

                        }
                        
                    }
                break;
                case HttpStatusCode.NotFound:
                    {
                        string dataLocal = Properties.Settings.Default.UsePC.Trim();
                        if (!string.IsNullOrEmpty(dataLocal))
                        {
                            dataResponse = (JsonObject.Parse(dataLocal) as JsonObject);
                            if (dataResponse != null)
                            {
                                dataResponse["statusCode"] = UsePC.GET_USEPC_OK;
                                Log.Information($"Paqueteria del PC: {pkPc}. Obtenida de manera local");
                            }
                            else
                            {
                                dataResponse = new JsonObject();
                                dataResponse["statusCode"] = UsePC.GET_USEPC_ERROR;
                                dataResponse["message"] = "No se pudo cargar la paquetería. Inténtelo más tarde o consulte al personal de la UDI.";
                                Log.Error($"Paqueteria del PC: {pkPc}. No se pudo contevertir de String a JsonObject");
                            }
                        }
                        else
                        {
                            dataResponse = new JsonObject();
                            dataResponse["statusCode"] = UsePC.GET_USEPC_ERROR;
                            dataResponse["message"] = "No se pudo cargar la paquetería. Inténtelo más tarde o consulte al personal de la UDI.";
                            Log.Error($"Paqueteria del PC: {pkPc}. No se encontro el id del PC");
                        }
                    }
                    break;
                default:
                    {
                        string dataLocal = Properties.Settings.Default.UsePC.Trim();
                        if (!string.IsNullOrEmpty(dataLocal))
                        {
                            dataResponse = (JsonObject.Parse(dataLocal) as JsonObject);
                            if (dataResponse != null)
                            {
                                dataResponse["statusCode"] = UsePC.GET_USEPC_OK;
                                Log.Information($"Paqueteria del PC: {pkPc}. Obtenida de manera local");
                            }
                            else
                            {
                                dataResponse = new JsonObject();
                                dataResponse["statusCode"] = UsePC.GET_USEPC_ERROR;
                                dataResponse["message"] = "No se pudo cargar la paquetería. Inténtelo más tarde o consulte al personal de la UDI.";
                                Log.Error($"Paqueteria del PC: {pkPc}. No se pudo contevertir de String a JsonObject");
                            }
                        }
                        else
                        {
                            dataResponse = new JsonObject();
                            dataResponse["statusCode"] = UsePC.GET_USEPC_ERROR;
                            dataResponse["message"] = "No se pudo cargar la paquetería. Inténtelo más tarde o consulte al personal de la UDI.";
                            Log.Error($"Paqueteria del PC: {pkPc}. Error al hacer peticion GET");
                        }
                    }
                    break;
            }
            return dataResponse;
        }

        public async Task<JsonArray> GetServices()
        {
            try
            {
                string endpoint = UrlCombine.Combine(Properties.Settings.Default.UrlServer, urlServicios);
                HttpResponseMessage response = await GetAsync(endpoint);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine(data);
                    JsonValue value = JsonObject.Parse(data);
                    return (JsonArray)value;
                }
                else
                {
                    Log.Error("Error al obtener servicios", response.Content.ReadAsStringAsync().Result);
                    return new JsonArray();
                }
            }
            catch (Exception e)
            {
                Log.Error("Error al obtener servicios", e.StackTrace);
                return new JsonArray();
            }
        }
        public async Task<Decimal> GetBalance(int idUser, int idClass)
        {
            try
            {
                string endpoint = UrlCombine.Combine(Properties.Settings.Default.UrlServer, urlMonedero, idClass.ToString(), idUser.ToString());
                Debug.WriteLine(endpoint);
                HttpResponseMessage response = await GetAsync(endpoint);

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        {
                            string data = await response.Content.ReadAsStringAsync();
                            Debug.WriteLine(data);
                            JsonValue value = JsonObject.Parse(data);
                            return (decimal)value["saldo"];
                        }
                    default:
                        {
                            Log.Error("Error al obtener servicios", response.Content.ReadAsStringAsync().Result);
                            return -1;
                        }
                }
            }
            catch (Exception e)
            {
                Log.Error("Error al obtener servicios", e.StackTrace);
                return -1m;
            }
        }

        public async Task<JsonObject> RequestService(Session session, Service service)
        {
            try
            {
                string endpoint = UrlCombine.Combine(Properties.Settings.Default.UrlServer, urlSolicitarImpresion);
                string date = DateTime.Now.ToString("yyy/MM/dd HH:mm:ss");
                JsonObject data = new JsonObject
                {
                    {"FECHA", date },
                    {"TIPO_BENEFICIARIO", session.User.PkCategoria },
                    {"ID_BENEFICIARIO", session.User.PkUsuario },
                    {"PK_COSTO_PRODUCTO", service.pkCostoProducto },
                    {"CANTIDAD", service.countPages },
                    {"SUBTOTAL", service.subTotal },
                    {"IMPUESTO", service.ivaTotal },
                    {"TOTAL", service.ivaTotal + service.subTotal }
                };
                HttpResponseMessage response = await this.PostAsync(endpoint, data.ToString());
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Created:
                        {
                            Log.Information($"Solicitud de servicio: {service.nombreProducto} se registro con exito para {session.User.UserNumber} con data: {data.ToString()}");
                            return new JsonObject
                            {
                                {"success", true },
                                {"message", "Solicitud registrada con éxito." }
                            };
                        }
                        
                    case HttpStatusCode.BadRequest:
                        {
                            Log.Information($"Error al registrar la informacion del servicio: {service.nombreProducto} para {session.User.UserNumber} con data: {data.ToString()}");
                            return new JsonObject
                            {
                                
                                {"success", false },
                                {"message", "Error al registrar los detalles del servicio. Por favor intentelo de nuevo" }
                            };
                        }
                    default:
                        {
                            Log.Information($"Error al registrar la informacion del servicio: {service.nombreProducto} para {session.User.UserNumber} con data: {data.ToString()}");
                            return new JsonObject
                            {
                                {"success", false },
                                {"message", "Error desconocido al solicitar servicio" }
                            };
                        }
                };
            }
            catch (Exception e)
            {
                Log.Error("Error al solicitar servicio", e.StackTrace);
                return new JsonObject
                {
                    {"success", false },
                    {"message", "Error al solicitar servicio" }
                };
            }
        }

        public static string GetIPLocal()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        public async Task<HttpResponseMessage> GetAsync(string endpoint)
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(40);
                try
                {

                    //StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.GetAsync(endpoint);
#if DEBUG
                    await Task.Delay(2000);
#endif
                    return response;
                }
                catch (Exception ex)
                {
                    Log.Error("Error al hacer GET", ex);
                    HttpResponseMessage responseError = new HttpResponseMessage();
                    responseError.StatusCode = System.Net.HttpStatusCode.InternalServerError;
#if DEBUG
                    await Task.Delay(2000);
#endif
                    return responseError;
                }
            }
        }

        public async Task<HttpResponseMessage> PostAsync(string endpoint, string data)
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(40);
                try
                {

                    StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                    Debug.WriteLine(data);
                    HttpResponseMessage response = await client.PostAsync(endpoint, content);
#if DEBUG
                    await Task.Delay(2000);
#endif
                    return response;
                }
                catch (Exception ex)
                {
                    Log.Error("Error al hacer GET", ex);
                    Debug.WriteLine(ex.StackTrace);
                    HttpResponseMessage responseError = new HttpResponseMessage();
                    responseError.StatusCode = System.Net.HttpStatusCode.InternalServerError;
#if DEBUG
                    await Task.Delay(2000);
#endif
                    return responseError;
                }
            }
        }
    }
}
