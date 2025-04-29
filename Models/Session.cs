using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AulasSiencb2.Models
{
    internal class Session(User user)
    {
       
        public const int REGISTER_NULL = 0;
        public const int REGISTER_LOGIN_OK = 1;
        public const int REGISTER_LOGOUT_OK = 2;
        public const int REGISTER_LOGIN_ERROR = 3;
        public const int REGISTER_LOGOUT_ERROR = 4;
        public const int REGISTER_USER_NOT_FOUND = 5;
        public const int REGISTER_LOGIN_PENALTY = 6;
        public const int REGISTER_LOGIN_ACTIVE = 7;
        public const int REGISTER_LOGIN_LOCAL_OK = 8;
        public const int REGISTER_USER_LOCAL_NOT_FOUND = 9;

        public const int REGISTER_LOGOUT_BY_USER_ERROR = 10;

        public const int REGISTER_LOGOUT_ERROR_NOT_FOUND = 11;
        public const int REGISTER_LOGOUT_ERROR_BAD_REQUEST = 12;
        public const int REGISTER_LOGOUT_PENALTY = 13;
        public const int REGISTER_LOGOUT_PENALTY_ERROR = 14;


        public User User { get; set; } = user;
        public String DateIn { get; set; } = String.Empty;
        public String DateOut { get; set; } = String.Empty;
        public int PkEquipo { get; set; } = -1;
        public int PkRegistro { get; set; } = -1;
        public UsePC? UsePC { get; set; } = null;

        public int StatusCode { get; set; } = 0;
        public int StatusCodeOut { get; set; } = 0;

        public bool penalty { get; set; } = false;

    }
}
