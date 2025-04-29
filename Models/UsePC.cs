using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AulasSiencb2.Models
{
    
    internal class UsePC
    {
        public const int GET_USEPC_OK = 1;
        public const int GET_USEPC_ERROR = 2;
        public required List<JsonObject> Services { get; set; }
        public required List<JsonObject>  Programs{ get; set; }

        public override string ToString()
        {
            return "Servicios:\n\t" + JsonSerializer.Serialize(Services) + "\nProgramas:\n\t" + JsonSerializer.Serialize(Programs);
        }

    }
}
