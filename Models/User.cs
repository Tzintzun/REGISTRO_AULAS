using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AulasSiencb2.Models
{
    internal class User
    {
        public const int USER_NORMAL = 0;
        public const int USER_ADMIN = 1;

        public int UserType { get; set; }
        public string UserNumber { get; set; }
        public string Curp { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public string UriPhoto { get; set; }
        public int PkCategoria { get; set; }
        public int PkUsuario { get; set; }

        public User(string userNumber, string curp, string name, int userType)
        {
            UserType = userType;
            UserNumber = userNumber;
            Curp = curp;
            Name = name;
            Department = string.Empty;
            PkCategoria = -1;
            PkUsuario = -1;
            UriPhoto = string.Empty;
        }

        public User(string userNumer, string curp)
        {
            UserNumber = userNumer;
            Curp = curp;

            UserType = User.USER_NORMAL;
            Name = string.Empty;
            Department = string.Empty;
            UriPhoto = string.Empty;
            PkCategoria = -1;
            PkUsuario = -1;

        }

        public override string ToString()
        {
            return "Usuario:\n\tBoleta:" + UserNumber + "\n\tCUERP: " + Curp;
        }
    }
}
