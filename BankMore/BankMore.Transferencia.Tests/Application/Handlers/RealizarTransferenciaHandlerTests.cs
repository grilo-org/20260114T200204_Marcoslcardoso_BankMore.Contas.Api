using BankMore.Transferencia.Application.Commands;
using BankMore.Transferencia.Application.Handlers;
using BankMore.Transferencia.Application.Interfaces;
using Moq;

[Fact]
public async Task Deve_realizar_transferencia_quando_comando_for_valido()
{
    // Arrange
    var repositoryMock = new Mock<ITransferenciaRepository>();
    var eventProducerMock = new Mock<IEventProducer>();

    var handler = new RealizarTransferenciaHandler(
        repositoryMock.Object,
        eventProducerMock.Object
    );

    var command = new RealizarTransferenciaCommand
    {
        IdRequisicao = Guid.NewGuid().ToString(),
        ContaDestino = 123456,
        Valor = 100m
    };

    // Act
    await handler.Handle(command, CancellationToken.None);

    // Assert
}
