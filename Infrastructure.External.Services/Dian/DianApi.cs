using Domain.Entities.Request;
using Domain.Interfaces.Dian;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.External.Services.Dian
{
    public class DianApi : IDianApi
    {
        private const string URL_API = "http://localhost:5207/api/prueba";
        public async Task<bool> SendToDian(PeajeRequest transaction, CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, URL_API);
            var jsonBody = JsonSerializer.Serialize(transaction);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            using var response = await client.SendAsync(request,cancellationToken);
            return response.IsSuccessStatusCode;
        }
    }
}
