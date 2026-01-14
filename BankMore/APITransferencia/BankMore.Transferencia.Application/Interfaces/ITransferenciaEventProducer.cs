using BankMore.Transferencia.Application.Events;
using System.Threading.Tasks;

namespace BankMore.Transferencia.Application.Interfaces;

public interface ITransferenciaEventProducer
{
    Task PublicarAsync(TransferenciaConcluidaEvent evento);
}
