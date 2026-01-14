using BankMore.Contas.Application.Commands;
using BankMore.Contas.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using BCrypt.Net;

namespace BankMore.Contas.Application.Handlers
{
    public class InativarContaCorrenteHandler : IRequestHandler<InativarContaCorrenteCommand, Unit>
    {
        private readonly IContaCorrenteRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InativarContaCorrenteHandler(
            IContaCorrenteRepository repository,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Unit> Handle(InativarContaCorrenteCommand request, CancellationToken cancellationToken)
        {
            // ✅ Pega o claim "sub" diretamente do JWT
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("TOKEN_INVALIDO");

            // ✅ Busca a conta pelo GUID
            var conta = await _repository.ObterPorIdAsync(Guid.Parse(userId));
            if (conta == null)
                throw new ArgumentException("INVALID_ACCOUNT");

            // ✅ Valida a senha
            var senhaValida = BCrypt.Net.BCrypt.Verify(request.Senha + conta.Salt, conta.SenhaHash);
            if (!senhaValida)
                throw new UnauthorizedAccessException("USER_UNAUTHORIZED");

            // ✅ Inativa a conta
            await _repository.InativarAsync(conta.IdContaCorrente);

            return Unit.Value;
        }
    }
}
