namespace BankMore.Transferencia.Application.Events;

public class TransferenciaConcluidaEvent
{
    public string IdRequisicao { get; init; } = default!;
    public long ContaDestino { get; init; }
    public decimal Valor { get; init; }
    public DateTime Data { get; init; }
}
