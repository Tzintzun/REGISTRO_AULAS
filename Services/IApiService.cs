using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AulasSiencb2.Services
{
    interface IApiService
    {
        Task<HttpResponseMessage> GetAsync(string endpoint, string data);
        Task<HttpResponseMessage> PostAsync(string endpoint, string data);
    }
}
