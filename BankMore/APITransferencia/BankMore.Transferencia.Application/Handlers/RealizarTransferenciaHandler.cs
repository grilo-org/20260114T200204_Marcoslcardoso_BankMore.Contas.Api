using BankMore.Transferencia.Application.Commands;
using BankMore.Transferencia.Application.Exceptions;
using BankMore.Transferencia.Application.Interfaces;
using BankMore.Transferencia.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BankMore.Transferencia.Application.Handlers;

public class RealizarTransferenciaHandler : IRequestHandler<RealizarTransferenciaCommand, Unit>
{
    private readonly IContaCorrenteClient _contaClient;
    private readonly ITransferenciaRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransferenciaEventProducer _eventProducer;

    public RealizarTransferenciaHandler(
        IContaCorrenteClient contaClient,
        ITransferenciaRepository repository,
        IHttpContextAccessor httpContextAccessor,
        ITransferenciaEventProducer eventProducer)
    {
        _contaClient = contaClient;
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
        _eventProducer = eventProducer;
    }

    public async Task<Unit> Handle(
        RealizarTransferenciaCommand request,
        CancellationToken cancellationToken)
    {
        //  Pega token do header Authorization
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

         var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("TOKEN_INVALIDO");

        var user = _httpContextAccessor.HttpContext?.User;

        if (user == null || !user.Identity!.IsAuthenticated)
            throw new UnauthorizedAccessException("TOKEN_INVALIDO");

        var contaClaim = user.Claims.FirstOrDefault(c => c.Type == "numero_conta")?.Value;

        var token = authHeader.Replace("Bearer ", "");

        //  Validação de valor
        if (request.Valor <= 0)
            throw new BankMore.Transferencia.Application.Exceptions.ApplicationException("INVALID_VALUE", "Valor deve ser positivo");

        //  Idempotência: verifica se a transferência já foi processada
        if (await _repository.ExisteIdRequisicaoAsync(request.IdRequisicao))
            return Unit.Value;

        //  Cria a entidade Transferência (ContaOrigem é obtida pelo token)
        var transferencia = new BankMore.Transferencia.Domain.Entities.Transferencia(
            request.IdRequisicao,
            Convert.ToInt64(contaClaim),
            request.ContaDestino, 
            request.Valor
        );

        //  Persiste a transferência (status inicial SUCESSO)
        await _repository.RegistrarAsync(transferencia);

        // UUIDs separados para cada movimentação, garantindo idempotência na conta corrente
        var idDebito = Guid.NewGuid().ToString();
        var idCredito = Guid.NewGuid().ToString();
        var idEstorno = Guid.NewGuid().ToString();

        try
        {
            //  Débito na conta origem
            await _contaClient.DebitarAsync(token, idDebito, request.Valor);

            // Crédito na conta destino
            await _contaClient.CreditarAsync(token, idCredito, request.ContaDestino, request.Valor);

            // Atualiza status para SUCESSO
            await _repository.RegistrarSucessoAsync(request.IdRequisicao, request.Valor, request.ContaDestino);

            // Evento Kafka (fire-and-forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _eventProducer.PublicarAsync(
                        new Application.Events.TransferenciaConcluidaEvent
                        {
                            IdRequisicao = request.IdRequisicao,
                            ContaDestino = request.ContaDestino,
                            Valor = request.Valor,
                            Data = DateTime.UtcNow
                        }
                    );
                }
                catch (Exception ex)
                {
                    // IMPORTANTE: nunca deixar exceção subir aqui
                    // Logar apenas (senão o processo pode morrer)
                    Console.WriteLine($"Erro ao publicar evento Kafka: {ex.Message}");
                }
            });
        }
        catch
        {
            // 🔹 Estorno: devolve o valor debitado em caso de falha
            try
            {
                await _contaClient.CreditarAsync(token, idEstorno, null, request.Valor);
            }
            catch
            {
                //  Caso o estorno falhe, a operação fica inconsistente.
                // Aqui podemos logar, enviar alerta ou gravar em uma tabela de pendências
            }

            // Atualiza status para FALHA
            await _repository.RegistrarFalhaAsync(request.IdRequisicao);

            throw new BankMore.Transferencia.Application.Exceptions.ApplicationException("TRANSFERENCIA_FALHA", "Erro ao realizar transferência");
        }

        return Unit.Value;
    }
}
