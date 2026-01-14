using BankMore.Transferencia.Application.Events;
using KafkaFlow;
using KafkaFlow.Consumers;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.Transferencia.Infrastructure.Consumers;

public class TransferenciaConcluidaConsumer : IMessageHandler<TransferenciaConcluidaEvent>
{
    // ⭐ Defina apenas o diretório windows
    //private static readonly string LogDirectory = @"C:\Projetos\BankMore\APITransferencia\Logs";
    //private static readonly string LogFilePath = Path.Combine(LogDirectory, "logTransferencia.txt");

    // Caminho Linux, válido dentro do container
    private static readonly string LogDirectory = "/app/logs";
    private static readonly string LogFilePath = "/app/logs/logTransferencia.txt";

    // Use a pasta /tmp que sempre tem permissão de escrita no Linux
    //private static readonly string LogDirectory = "/tmp/logs";
    //private static readonly string LogFilePath = "/tmp/logs/logTransferencia.txt";

    public async Task Handle(
        IMessageContext context,
        TransferenciaConcluidaEvent message)
    {
        var logLine = new StringBuilder()
            .Append($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] ")
            .Append($"Transferência concluída | ")
            .Append($"IdRequisicao: {message.IdRequisicao} | ")
            .Append($"ContaDestino: {message.ContaDestino} | ")
            .Append($"Valor: {message.Valor:C}")
            .AppendLine()
            .ToString();

        try
        {
            Console.WriteLine($"[KAFKA CONSUMER] Mensagem recebida: {message.IdRequisicao}");
            // ⭐ Garante que o diretório existe
            Directory.CreateDirectory(LogDirectory);
            
            await File.AppendAllTextAsync(LogFilePath, logLine);
        }
        catch (Exception ex)
        {
            // Log do erro (opcional - pode usar ILogger se injetado)
            Console.WriteLine($"Erro ao gravar log: {ex.Message}");
        }
    }
}