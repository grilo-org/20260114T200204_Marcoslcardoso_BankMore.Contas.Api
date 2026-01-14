using BankMore.Transferencia.Application.Interfaces;
using BankMore.Transferencia.Domain.Entities;
using BankMore.Transferencia.Domain.Enums;
using BankMore.Transferencia.Infrastructure.Persistence;
using Dapper;
using System;
using System.Threading.Tasks;

namespace BankMore.Transferencia.Infrastructure.Repositories;

public class TransferenciaRepository : ITransferenciaRepository
{
    private readonly SqliteConnectionFactory _factory;

    public TransferenciaRepository(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<bool> ExisteIdRequisicaoAsync(string idRequisicao)
    {
        using var conn = _factory.Create();
        var sql = @"SELECT COUNT(1) FROM transferencia WHERE idrequisicao = @IdRequisicao";
        var count = await conn.ExecuteScalarAsync<int>(sql, new { IdRequisicao = idRequisicao });
        return count > 0;
    }

    public async Task RegistrarAsync(BankMore.Transferencia.Domain.Entities.Transferencia transferencia)
    {
        using var conn = _factory.Create();

        var sql = @"
            INSERT INTO transferencia (
                idtransferencia,
                idrequisicao,
                idcontacorrente_origem,
                idcontacorrente_destino,
                valor,
                status,
                datamovimento
            )
            VALUES (
                @IdTransferencia,
                @IdRequisicao,
                @IdContaOrigem,
                @IdContaDestino,
                @Valor,
                @Status,
                @DataMovimento
            )
        ";

        await conn.ExecuteAsync(sql, new
        {
            IdTransferencia = transferencia.IdTransferencia.ToString(),
            transferencia.IdRequisicao,
            IdContaOrigem = transferencia.ContaOrigem.ToString(),
            IdContaDestino = transferencia.ContaDestino.ToString(),
            transferencia.Valor,
            Status = transferencia.Status.ToString(),
            DataMovimento = transferencia.DataTransferencia.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }

    public async Task RegistrarSucessoAsync(string idRequisicao, decimal valor, long numeroContaDestino)
    {
        using var conn = _factory.Create();

        var sql = @"
            UPDATE transferencia
            SET status = @Status
            WHERE idrequisicao = @IdRequisicao
        ";

        await conn.ExecuteAsync(sql, new
        {
            IdRequisicao = idRequisicao,
            Status = TransferenciaStatus.SUCESSO.ToString()
        });
    }

    public async Task RegistrarFalhaAsync(string idRequisicao)
    {
        using var conn = _factory.Create();

        var sql = @"
            UPDATE transferencia
            SET status = @Status
            WHERE idrequisicao = @IdRequisicao
        ";

        await conn.ExecuteAsync(sql, new
        {
            IdRequisicao = idRequisicao,
            Status = TransferenciaStatus.FALHA.ToString()
        });
    }
}
