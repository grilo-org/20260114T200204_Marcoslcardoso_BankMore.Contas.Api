using BankMore.Contas.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.Contas.Domain.Entities
{
    public class ContaCorrente
    {
        public Guid IdContaCorrente { get; private set; }
        public Cpf Cpf { get; private set; }
        public int Numero { get; private set; }
        public string Nome { get; private set; }
        public string SenhaHash { get; private set; }
        public string Salt { get; private set; }
        public bool Ativo { get; private set; }

        public ContaCorrente(Cpf cpf, string nome, string senhaHash, string salt, int numero)
        {
            IdContaCorrente = Guid.NewGuid();
            Cpf = cpf;
            Nome = nome;
            SenhaHash = senhaHash;
            Salt = salt;
            Numero = numero;
            Ativo = true;
        }
    }
}
