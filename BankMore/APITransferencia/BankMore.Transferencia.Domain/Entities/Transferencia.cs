using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BankMore.Transferencia.Domain.Enums;
using BankMore.Transferencia.Domain.Exceptions;
namespace BankMore.Transferencia.Domain.Entities;

public class Transferencia
{
    public Guid IdTransferencia { get; private set; }
    public string IdRequisicao { get; private set; }
    public long ContaOrigem { get; private set; }
    public long ContaDestino { get; private set; }
    public decimal Valor { get; private set; }
    public TransferenciaStatus Status { get; private set; }
    public DateTime DataTransferencia { get; private set; }

    private Transferencia() { }

    public Transferencia(
        string idRequisicao,
        long contaOrigem,
        long contaDestino,
        decimal valor)
    {
        
        if (string.IsNullOrWhiteSpace(idRequisicao))
            throw new DomainException("INVALID_REQUEST", "Id da requisição é obrigatório");

        if (valor <= 0)
            throw new DomainException("INVALID_VALUE", "Valor deve ser positivo");


        IdTransferencia = Guid.NewGuid();
        IdRequisicao = idRequisicao;
        ContaOrigem = contaOrigem;
        ContaDestino = contaDestino;
        Valor = valor;
        Status = TransferenciaStatus.PENDENTE;
        DataTransferencia = DateTime.UtcNow;
    }

    public void MarcarFalha()
    {
        Status = TransferenciaStatus.FALHA;
    }

    public void MarcarEstornada()
    {
        Status = TransferenciaStatus.ESTORNADA;
    }
}


