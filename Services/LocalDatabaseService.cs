using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AulasSiencb2.Helpers;
using Serilog;
using Microsoft.Data.Sqlite;

namespace AulasSiencb2.Services
{
    internal class LocalDatabaseService
    {
        public static string? VerifyUser(string userNumber, string curp)
        {
            string? nombre = null;

            string contextPath = AppContext.BaseDirectory;
            string dbPath = System.IO.Path.Combine(contextPath, "Assets", "localUsers.db");

            using (SqliteConnection connection = new SqliteConnection($"Data Source = {dbPath}"))
            {
                try
                {
                    connection.Open();

                    Debug.WriteLine("Conexcio extableciado a BD Local");
                    Log.Information("Conexcio extableciado a BD Local");

                    var getUserQueryCmd = connection.CreateCommand();
                    getUserQueryCmd.CommandText = @Properties.Settings.Default.GetUserQuery;
                    getUserQueryCmd.Parameters.AddWithValue("@userNumber", SegurityUtils.TextToHash(userNumber));
                    getUserQueryCmd.Parameters.AddWithValue("@userCurp", SegurityUtils.TextToHash(curp));
                    Debug.WriteLine(SegurityUtils.TextToHash(userNumber));
                    Debug.WriteLine(SegurityUtils.TextToHash(curp));
                    Debug.WriteLine("Ejecutando Query");

                    using (SqliteDataReader response = getUserQueryCmd.ExecuteReader())
                    {
                        //Debug.WriteLine(response.HasRows);
                        
                        while (response.Read())
                        {
                            Debug.WriteLine((string)response["USUARIO_NOMBRE"]);
                            nombre = "ALUMNO DE LA ENCB";
                                //SegurityUtils.DeencryptText((string)response["USUARIO_NOMBRE"], userNumber + curp);
                            Log.Information("Usuario encontrado en BD Local:\n\tBoleta: " + userNumber + "\n\tCURP: " + curp +"\n\tNAME: " + nombre);
                 
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Error("Error al intetar extrar a usuario de la BD Local", ex);
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    return null;
                }
                return nombre;
            }
        }

        public static bool? InsertNewUser(string userNumber, string curp, string fullName)
        {
            bool? result = null;

            string contextPath = AppContext.BaseDirectory;
            string dbPath = System.IO.Path.Combine(contextPath, "Assets", "localUsers.db");
            using (SqliteConnection connection = new SqliteConnection($"Data Source = {dbPath}"))
            {
                try
                {
                    connection.Open();
                    Debug.WriteLine("Conexcio extableciado a BD Local");
                    Log.Information("Conexcio extableciado a BD Local");

                    string? encryptedName = SegurityUtils.EncriyptText(fullName, userNumber + curp);

                    if (encryptedName == null)
                    {
                        Debug.WriteLine("No se pudo encriptar el nombre del usuario:\n\tBoleta: " + userNumber + "\n\tCURP: " + curp);
                        Log.Error("No se pudo encriptar el nombre del usuario:\n\tBoleta: " + userNumber + "\n\tCURP: " + curp);
                        return false;
                    }

                    var insertUserQueryCmd = connection.CreateCommand();
                    insertUserQueryCmd.CommandText = @Properties.Settings.Default.InsertUserQuery;
                    insertUserQueryCmd.Parameters.AddWithValue("@userNumber", SegurityUtils.TextToHash(userNumber));
                    insertUserQueryCmd.Parameters.AddWithValue("@userCurp", SegurityUtils.TextToHash(curp));
                    insertUserQueryCmd.Parameters.AddWithValue("@name", encryptedName);

                    insertUserQueryCmd.ExecuteNonQuery();
                    Log.Information("Usuario registrado en BD local:\n\tboleta: " + userNumber + "\n\tCURP: " + curp);
                    result = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    Log.Error("Error al insertar un nuevo usuario en BD local", ex);
                    return false;
                }

                return result;

            }

        }
    }
}
