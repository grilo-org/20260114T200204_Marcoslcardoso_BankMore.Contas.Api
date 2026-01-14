using BankMore.Contas.Application.Commands;
using BankMore.Contas.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankMore.ContaCorrente.Api.Controllers;

[ApiController]
[Route("api/contas")]
public class ContaCorrenteController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContaCorrenteController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("CriarContaCorrente")]
    public async Task<IActionResult> CriarConta([FromBody] CriarContaCorrenteCommand command)
    {
        try
        {
            var numeroConta = await _mediator.Send(command);
            return Ok(new { numeroConta });
        }
        catch (ArgumentException ex) when (ex.Message == "INVALID_DOCUMENT")
        {
            return BadRequest(new
            {
                message = "CPF inválido",
                type = "INVALID_DOCUMENT"
            });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginContaCorrenteCommand command)
    {
        try
        {
            var token = await _mediator.Send(command);
            return Ok(new { token });
        }
        catch (UnauthorizedAccessException ex) when (ex.Message == "USER_UNAUTHORIZED")
        {
            return Unauthorized(new
            {
                message = "Usuário ou senha inválidos",
                type = "USER_UNAUTHORIZED"
            });
        }
    }


    [HttpPost("inativar")]
    [Authorize]
    public async Task<IActionResult> Inativar([FromBody] InativarContaCorrenteCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return NoContent(); // HTTP 204
        }
        catch (UnauthorizedAccessException ex) when (ex.Message == "USER_UNAUTHORIZED")
        {
            return new ObjectResult(new { message = "Senha inválida", type = "USER_UNAUTHORIZED" })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }
        catch (UnauthorizedAccessException ex) when (ex.Message == "TOKEN_INVALIDO")
        {
            return new ObjectResult(new { message = "Token inválido ou expirado", type = "TOKEN_INVALIDO" })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
        catch (ArgumentException ex) when (ex.Message == "INVALID_ACCOUNT")
        {
            return new ObjectResult(new { message = "Conta não encontrada", type = "INVALID_ACCOUNT" })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }
    }

    [HttpPost("movimentar")]
    [Authorize]
    public async Task<IActionResult> Movimentar([FromBody] MovimentarContaCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message, type = "TOKEN_INVALIDO" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message, type = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message, type = ex.Message });
        }
    }

    [HttpGet("saldo")]
    [Authorize]
    public async Task<IActionResult> Saldo()
    {
        try
        {
            var saldo = await _mediator.Send(new ConsultarSaldoQuery());
            return Ok(saldo); // HTTP 200
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message, type = "TOKEN_INVALIDO" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message, type = "INVALID_ACCOUNT" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message, type = "INACTIVE_ACCOUNT" });
        }
    }


}
