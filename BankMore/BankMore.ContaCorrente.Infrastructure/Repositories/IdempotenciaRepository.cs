using BankMore.Contas.Application.Interfaces;
using BankMore.Contas.Infrastructure.Persistence;
using Dapper;

namespace BankMore.Contas.Infrastructure.Repositories
{
    public class IdempotenciaRepository : IIdempotenciaRepository
    {
        private readonly SqliteConnectionFactory _factory;

        public IdempotenciaRepository(SqliteConnectionFactory factory)
        {
            _factory = factory;
        }



        public async Task<bool> ExisteAsync(string idRequisicao)
        {
            using var conn = _factory.Create();
            var sql = @"SELECT COUNT(1) FROM idempotencia WHERE chave_idempotencia = @Id";
            var count = await conn.ExecuteScalarAsync<int>(sql, new { Id = idRequisicao });
            return count > 0;
        }

        public async Task RegistrarAsync(string idRequisicao, string requisicao, string resultado)
        {
            using var conn = _factory.Create();
            var sql = @"INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado)
                        VALUES (@IdRequisicao, @Requisicao, @Resultado)";
            await conn.ExecuteAsync(sql, new { IdRequisicao = idRequisicao, Requisicao = requisicao, Resultado = resultado });
        }
    }
}
