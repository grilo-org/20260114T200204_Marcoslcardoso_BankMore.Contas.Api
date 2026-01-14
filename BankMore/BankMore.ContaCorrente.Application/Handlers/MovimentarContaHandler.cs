using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using BankMore.Contas.Application.Commands;
using BankMore.Contas.Application.Interfaces;

namespace BankMore.Contas.Application.Handlers
{
    public class MovimentarContaHandler : IRequestHandler<MovimentarContaCommand, Unit>
    {
        private readonly IContaCorrenteRepository _repository;
        private readonly  IIdempotenciaRepository _idempotenciaRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MovimentarContaHandler(
            IContaCorrenteRepository repository,
            IIdempotenciaRepository idempotenciaRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _idempotenciaRepository = idempotenciaRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Unit> Handle(MovimentarContaCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User
                         ?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("TOKEN_INVALIDO");

            if (await _idempotenciaRepository.ExisteAsync(request.IdRequisicao))
                return Unit.Value;

            BankMore.Contas.Domain.Entities.ContaCorrente conta;

            if (!request.NumeroConta.HasValue)
            {
                conta = await _repository.ObterPorIdAsync(Guid.Parse(userId));
            }
            else
            {
                conta = await _repository.ObterPorNumeroContaAsync(request.NumeroConta.Value);
            }

            if (conta == null)
                throw new ArgumentException("INVALID_ACCOUNT");

            if (!conta.Ativo)
                throw new InvalidOperationException("INACTIVE_ACCOUNT");

            if (request.Valor <= 0)
                throw new ArgumentException("INVALID_VALUE");

            if (request.TipoMovimento != "C" && request.TipoMovimento != "D")
                throw new ArgumentException("INVALID_TYPE");

            if (request.TipoMovimento == "D" &&
                request.NumeroConta.HasValue &&
                request.NumeroConta.Value != conta.Numero)
                throw new ArgumentException("INVALID_TYPE");

            var numeroContaMovimentada = request.NumeroConta ?? conta.Numero;

            await _repository.MovimentarAsync(
                conta.IdContaCorrente,
                request.TipoMovimento,
                request.Valor,
                request.IdRequisicao,
                numeroContaMovimentada
            );

            return Unit.Value;
        }
    }
}
