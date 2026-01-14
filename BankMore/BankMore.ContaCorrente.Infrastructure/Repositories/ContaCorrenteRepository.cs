using BankMore.Contas.Application.Interfaces;
using BankMore.Contas.Domain.Entities;
using BankMore.Contas.Domain.ValueObjects;
using BankMore.Contas.Infrastructure.Persistence;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.Contas.Infrastructure.Repositories
{
    public class ContaCorrenteRepository : IContaCorrenteRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public ContaCorrenteRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<int> CriarAsync(BankMore.Contas.Domain.Entities.ContaCorrente conta)
        {
            using var conn = _factory.Create();

            var sql = @"
                INSERT INTO contacorrente
                (idcontacorrente, cpf, numero, nome, senha_hash, salt, ativo)
                VALUES
                (@Id, @Cpf, @Numero, @Nome, @SenhaHash, @Salt, 1);
            ";

            await conn.ExecuteAsync(sql, new
            {
                Id = conta.IdContaCorrente.ToString(),
                Cpf = conta.Cpf.Numero,
                conta.Numero,
                conta.Nome,
                SenhaHash = conta.SenhaHash,
                conta.Salt
            });

            return conta.Numero;
        }


        public async Task<BankMore.Contas.Domain.Entities.ContaCorrente?> ObterPorCpfOuNumeroAsync(string cpfOuNumero)
        {
            using var conn = _factory.Create();

            // Tenta converter número, se for um int válido
            int numeroConta = 0;
            if (!int.TryParse(cpfOuNumero, out numeroConta))
                numeroConta = 0;

            var sql = @"
                SELECT idcontacorrente AS IdContaCorrente,
                       cpf,
                       numero,
                       nome,
                       senha_hash AS SenhaHash,
                       salt,
                       ativo
                FROM contacorrente
                WHERE cpf = @Cpf OR numero = @Numero
                LIMIT 1;
            ";

            // Executa query e retorna um objeto dinâmico
            var resultado = await conn.QueryFirstOrDefaultAsync(sql, new
            {
                Cpf = cpfOuNumero,
                Numero = numeroConta
            });

            if (resultado == null)
                return null;

            // Constrói a entidade manualmente, usando Value Object Cpf
            var conta = new BankMore.Contas.Domain.Entities.ContaCorrente(
                new Cpf(resultado.cpf),
                resultado.nome,
                resultado.SenhaHash,
                resultado.salt,
                (int)resultado.numero
            );

            // Define o Id e Ativo manualmente
            typeof(BankMore.Contas.Domain.Entities.ContaCorrente)
                .GetProperty("IdContaCorrente")!
                .SetValue(conta, Guid.Parse(resultado.IdContaCorrente));

            typeof(BankMore.Contas.Domain.Entities.ContaCorrente)
                .GetProperty("Ativo")!
                .SetValue(conta, resultado.ativo == 1);

            return conta;
        }


        public async Task<BankMore.Contas.Domain.Entities.ContaCorrente?> ObterPorIdAsync(Guid id)
        {
            using var conn = _factory.Create();
            var sql = @"SELECT idcontacorrente AS IdContaCorrente,
                       cpf,
                       numero,
                       nome,
                       senha_hash AS SenhaHash,
                       salt,
                       ativo
                FROM contacorrente
                WHERE idcontacorrente = @Id";

            var resultado = await conn.QueryFirstOrDefaultAsync(sql, new { Id = id.ToString() });
            if (resultado == null) return null;

            var conta = new BankMore.Contas.Domain.Entities.ContaCorrente(
                new Cpf(resultado.cpf),
                resultado.nome,
                resultado.SenhaHash,
                resultado.salt,
                (int)resultado.numero
            );

            typeof(BankMore.Contas.Domain.Entities.ContaCorrente).GetProperty("IdContaCorrente")!.SetValue(conta, Guid.Parse(resultado.IdContaCorrente));
            typeof(BankMore.Contas.Domain.Entities.ContaCorrente).GetProperty("Ativo")!.SetValue(conta, resultado.ativo == 1);

            return conta;
        }

        public async Task<BankMore.Contas.Domain.Entities.ContaCorrente?> ObterPorNumeroContaAsync(long numeroConta)
        {
            using var conn = _factory.Create();
            var sql = @"SELECT idcontacorrente AS IdContaCorrente,
                       cpf,
                       numero,
                       nome,
                       senha_hash AS SenhaHash,
                       salt,
                       ativo
                FROM contacorrente
                WHERE numero = @Id";

            var resultado = await conn.QueryFirstOrDefaultAsync(sql, new { Id = numeroConta.ToString() });
            if (resultado == null) return null;

            var conta = new BankMore.Contas.Domain.Entities.ContaCorrente(
                new Cpf(resultado.cpf),
                resultado.nome,
                resultado.SenhaHash,
                resultado.salt,
                (int)resultado.numero
            );

            typeof(BankMore.Contas.Domain.Entities.ContaCorrente).GetProperty("IdContaCorrente")!.SetValue(conta, Guid.Parse(resultado.IdContaCorrente));
            typeof(BankMore.Contas.Domain.Entities.ContaCorrente).GetProperty("Ativo")!.SetValue(conta, resultado.ativo == 1);

            return conta;
        }

        public async Task InativarAsync(Guid id)
        {
            using var conn = _factory.Create();
            var sql = @"UPDATE contacorrente SET ativo = 0 WHERE idcontacorrente = @Id";
            await conn.ExecuteAsync(sql, new { Id = id.ToString() });
        }

        public async Task MovimentarAsync(Guid idConta, string tipoMovimento, decimal valor, string idRequisicao, long numeroConta)
        {
            using var conn = _factory.Create();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            var sqlMovimento = @"
                INSERT INTO movimento (idmovimento, idcontacorrente, idrequisicao, datamovimento, tipomovimento, valor)
                VALUES (@IdMovimento, @IdConta, @idRequisicao, @DataMovimento, @TipoMovimento, @Valor)";
            await conn.ExecuteAsync(sqlMovimento, new
            {
                IdMovimento = Guid.NewGuid().ToString(),
                IdConta = idConta.ToString(),
                idRequisicao = idRequisicao,
                DataMovimento = DateTime.UtcNow.ToString("dd/MM/yyyy"),
                TipoMovimento = tipoMovimento,
                Valor = valor
            }, transaction);

            var sqlIdempotencia = @"
                INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado)
                VALUES (@IdRequisicao, @Requisicao, @Resultado)";
            await conn.ExecuteAsync(sqlIdempotencia, new
            {
                IdRequisicao = idRequisicao,
                Requisicao = $"Movimentacao {tipoMovimento} {valor} conta {numeroConta}",
                Resultado = "OK"
            }, transaction);

            transaction.Commit();
        }

        public async Task<IEnumerable<Movimento>> ObterMovimentosAsync(Guid idConta)
        {
            using var conn = _factory.Create();
            conn.Open();
            var sql = @"SELECT * FROM movimento WHERE idcontacorrente = @IdConta";
            return await conn.QueryAsync<Movimento>(sql, new { IdConta = idConta.ToString() });
        }


    }
}

