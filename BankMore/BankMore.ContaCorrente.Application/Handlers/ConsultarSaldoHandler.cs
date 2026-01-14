using BankMore.Contas.Application.Interfaces;
using BankMore.Contas.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BankMore.Contas.Application.Handlers
{
    public class ConsultarSaldoHandler : IRequestHandler<ConsultarSaldoQuery, SaldoDto>
    {
        private readonly IContaCorrenteRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ConsultarSaldoHandler(IContaCorrenteRepository repository, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<SaldoDto> Handle(ConsultarSaldoQuery request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User
                         ?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("TOKEN_INVALIDO");

            var conta = await _repository.ObterPorIdAsync(Guid.Parse(userId));
            if (conta == null)
                throw new ArgumentException("INVALID_ACCOUNT");
            if (!conta.Ativo)
                throw new InvalidOperationException("INACTIVE_ACCOUNT");

            // Pega todos os movimentos da conta
            var movimentos = await _repository.ObterMovimentosAsync(conta.IdContaCorrente);

            // Calcula saldo: créditos - débitos
            var saldo = movimentos
                .Where(m => m.TipoMovimento == "C").Sum(m => m.Valor) -
                movimentos.Where(m => m.TipoMovimento == "D").Sum(m => m.Valor);

            return new SaldoDto
            {
                NumeroConta = conta.Numero,
                NomeTitular = conta.Nome,
                DataHoraConsulta = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss"),
                ValorSaldo = saldo
            };
        }
    }
}
