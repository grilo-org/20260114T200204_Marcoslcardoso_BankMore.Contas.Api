using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using BankMore.Transferencia.Application.Interfaces;

namespace BankMore.Transferencia.Infrastructure.Clients
{
    public class ContaCorrenteClient : IContaCorrenteClient
    {
        private readonly HttpClient _httpClient;

        public ContaCorrenteClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task DebitarAsync(string token, string idRequisicao, decimal valor)
        {
            var payload = new
            {
                idRequisicao,
                valor,
                tipoMovimento = "D"
            };

            await EnviarAsync(token, payload);
        }

        public async Task CreditarAsync(string token, string idRequisicao, long? numeroConta, decimal valor)
        {
            var payload = new
            {
                idRequisicao,
                numeroConta,
                valor,
                tipoMovimento = "C"
            };

            await EnviarAsync(token, payload);
        }

        private async Task EnviarAsync(string token, object payload)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/contas/movimentar")
            {
                Content = JsonContent.Create(payload)
            };

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Erro ao chamar API Conta Corrente");
        }
    }

}
