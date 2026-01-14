using MediatR;

namespace BankMore.Contas.Application.Commands
{
    public class MovimentarContaCommand : IRequest<Unit>
    {
        public string IdRequisicao { get; set; }       // Para idempotência
        public long? NumeroConta { get; set; }        // Número da conta (opcional)
        public decimal Valor { get; set; }            // Valor da movimentação
        public string TipoMovimento { get; set; }     // "C" = crédito, "D" = débito
    }
}
