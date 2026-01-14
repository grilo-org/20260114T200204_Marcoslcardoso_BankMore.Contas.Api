using BankMore.Transferencia.Application.Commands;
using BankMore.Transferencia.Application.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ApplicationException = BankMore.Transferencia.Application.Exceptions.ApplicationException;

namespace BankMore.Transferencia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransferenciaController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransferenciaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Realiza transferência entre contas
    /// </summary>
    [HttpPost]
    [Authorize] // requer token JWT
    public async Task<IActionResult> Post([FromBody] RealizarTransferenciaCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return NoContent(); // HTTP 204
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new { message = "Token inválido ou expirado", type = "TOKEN_INVALIDO" });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { message = ex.Message, type = ex.Tipo });
        }
        catch
        {
            return BadRequest(new { message = "Erro ao processar transferência", type = "TRANSFER_FAILED" });
        }
    }
}
