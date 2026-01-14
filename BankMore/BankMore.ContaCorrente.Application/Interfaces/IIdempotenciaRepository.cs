namespace BankMore.Contas.Application.Interfaces
{
    public interface IIdempotenciaRepository
    {
        Task<bool> ExisteAsync(string idRequisicao);
        Task RegistrarAsync(string idRequisicao, string requisicao, string resultado);
    }
}
