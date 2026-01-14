using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.Transferencia.Application.DTOs;

public class MovimentacaoContaDto
{
    public string IdRequisicao { get; set; }
    public long? NumeroConta { get; set; }
    public decimal Valor { get; set; }
    public string TipoMovimento { get; set; }
}

