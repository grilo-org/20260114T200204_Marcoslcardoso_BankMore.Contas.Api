
using BankMore.Transferencia.Application.Events;
using BankMore.Transferencia.Application.Interfaces;
using KafkaFlow;
using KafkaFlow.Producers;

namespace BankMore.Transferencia.Infrastructure
{
    public class TransferenciaEventProducer : ITransferenciaEventProducer
    {
        private readonly IProducerAccessor _producerAccessor;

        public TransferenciaEventProducer(IProducerAccessor producerAccessor)
        {
            _producerAccessor = producerAccessor;
        }

     
        public async Task PublicarAsync(TransferenciaConcluidaEvent evento)
        {
            if (evento == null)
            {
                Console.WriteLine("[PRODUCER] Erro: Evento nulo!");
                return;
            }
            try
            {
                Console.WriteLine($"[PRODUCER] Tentando publicar evento: {evento.IdRequisicao}");

                var producer = _producerAccessor.GetProducer("transferencia-producer");

                await producer.ProduceAsync(
                    messageKey: evento.IdRequisicao,
                    messageValue: evento
                );

                Console.WriteLine($"[PRODUCER] Evento {evento.IdRequisicao} publicado com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PRODUCER ERROR] Falha ao publicar: {ex.Message}");
            }
        }
    }
}