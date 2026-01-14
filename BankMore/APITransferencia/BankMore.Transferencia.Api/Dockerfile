FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia a solução mantendo a estrutura de subpasta (importante para as refs internas)
COPY ./BankMore.ContaCorrente.Api/BankMore.Contas.Api.sln ./BankMore.ContaCorrente.Api/

# 2. Copia todos os arquivos .csproj mantendo a estrutura exata do disco
COPY ./BankMore.ContaCorrente.Domain/*.csproj ./BankMore.ContaCorrente.Domain/
COPY ./BankMore.ContaCorrente.Application/*.csproj ./BankMore.ContaCorrente.Application/
COPY ./BankMore.ContaCorrente.Infrastructure/*.csproj ./BankMore.ContaCorrente.Infrastructure/
COPY ./BankMore.ContaCorrente.Api/*.csproj ./BankMore.ContaCorrente.Api/
COPY ./BankMore.ContaCorrente.Tests/*.csproj ./BankMore.ContaCorrente.Tests/

COPY ./BankMore.Transferencia.Infrastructure/*.csproj ./BankMore.Transferencia.Infrastructure/
COPY ./APITransferencia/BankMore.Transferencia.Api/*.csproj ./APITransferencia/BankMore.Transferencia.Api/
COPY ./APITransferencia/BankMore.Transferencia.Application/*.csproj ./APITransferencia/BankMore.Transferencia.Application/
COPY ./APITransferencia/BankMore.Transferencia.Domain/*.csproj ./APITransferencia/BankMore.Transferencia.Domain/

# 3. Restaura
RUN dotnet restore ./BankMore.ContaCorrente.Api/BankMore.Contas.Api.sln

# 4. Copia o código
COPY . .

# 5. Publica
WORKDIR "/src/APITransferencia/BankMore.Transferencia.Api"
RUN dotnet publish "BankMore.Transferencia.Api.csproj" -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
EXPOSE 80
ENTRYPOINT ["dotnet", "BankMore.Transferencia.Api.dll"]