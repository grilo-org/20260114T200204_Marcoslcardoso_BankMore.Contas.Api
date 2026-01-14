using MediatR;

namespace BankMore.Contas.Application.Queries
{
    public class ConsultarSaldoQuery : IRequest<SaldoDto>
    {
        // Não precisa de parâmetros porque usamos o token para identificar a conta
    }

    public class SaldoDto
    {
        public long NumeroConta { get; set; }
        public string NomeTitular { get; set; }
        public string DataHoraConsulta { get; set; }
        public decimal ValorSaldo { get; set; }
    }
}
