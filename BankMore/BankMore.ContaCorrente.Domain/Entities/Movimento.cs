using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.Contas.Domain.Entities
{
    public class Movimento
    {
        public string IdMovimento { get; set; }
        public string IdContaCorrente { get; set; }
        public string DataMovimento { get; set; }
        public string TipoMovimento { get; set; } // "C" ou "D"
        public decimal Valor { get; set; }
    }

}
