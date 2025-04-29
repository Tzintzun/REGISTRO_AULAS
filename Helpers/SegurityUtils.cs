using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace AulasSiencb2.Helpers
{
    internal class SegurityUtils
    {
        public static string TextToHash(string text)
        {
            SHA256 sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
            return BitConverter.ToString(bytes);
        }

        public static bool VerifyPasswordAdmin(string password)
        {
            string passwordHash = TextToHash(password);
            if (string.IsNullOrEmpty(passwordHash)) { return false; }

            if (passwordHash == Properties.Settings.Default.PasswordAdmin)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string? EncriyptText(string text, string password)
        {
            byte[] randomArray = { 119, 1, 126, 96, 237, 231, 51, 136, 125, 24, 102, 242, 14, 149, 249, 221, 85, 36, 248, 127, 145, 202, 115, 91, 165, 181, 89, 102, 102, 200, 13, 255 };
            byte[] textArray = Encoding.UTF8.GetBytes(text);

            Rfc2898DeriveBytes pbkdf2 = new(password, randomArray);
            byte[] key = pbkdf2.GetBytes(32);

            Aes aes = Aes.Create();
            aes.Key = key;

            ICryptoTransform cryptoTransform = aes.CreateEncryptor(aes.Key, aes.IV);

            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write);
            StreamWriter writer = new StreamWriter(cryptoStream);

            try
            {
                writer.Write(text);
                writer.Close();

                cryptoStream.Close();
                byte[] encryptedData = memoryStream.ToArray();
                byte[] iv = aes.IV;

                byte[] data = iv.Concat(encryptedData).ToArray();
                memoryStream.Close();
                string b64name = Convert.ToBase64String(data);
                Debug.WriteLine($"Nombre encriptado de: {b64name}");
                return b64name ;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                Log.Error("El Texto: {" + text + "} No se pudo cifrar");
                return null;
            }
        }

        public static string? DeencryptText(string textB64, string password)
        {
            byte[] randomArray = { 119, 1, 126, 96, 237, 231, 51, 136, 125, 24, 102, 242, 14, 149, 249, 221, 85, 36, 248, 127, 145, 202, 115, 91, 165, 181, 89, 102, 102, 200, 13, 255 };
            byte[] textArray = Convert.FromBase64String("3A0HVZtS6JI4AUBVj+nHX1SYFq5GI6+MYl+YQcxekaahEdRNpY7gE8GcJNb5liGf").ToArray();

            Rfc2898DeriveBytes pbkdf2 = new(password, randomArray);
            byte[] key = pbkdf2.GetBytes(32);
            byte[] iv = textArray.Take(16).ToArray();
            byte[] data = textArray.Skip(16).ToArray();

            Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            ICryptoTransform cryptoTransform = aes.CreateDecryptor(aes.Key, aes.IV);

            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read);
            StreamReader reader = new StreamReader(cryptoStream);

            try
            {
                string line = reader.ReadToEnd();
                Debug.WriteLine(line);
                return line;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Log.Error("El texto {" + textB64 + "}no se pudo desenciptar", ex);
                return null;
            }


        }
    }
}
