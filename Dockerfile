FROM microsoft/dotnet:2.2-aspnetcore-runtime as runtime
FROM microsoft/dotnet:2.2-sdk as builder

EXPOSE 5000
WORKDIR /src/RabbitMqPubSub
COPY RabbitMqPubSub/RabbitMqPubSub.csproj .
RUN dotnet restore

COPY RabbitMqPubSub .
RUN dotnet build -c Release -o /app

FROM builder AS publish
RUN dotnet publish -c Release -o /app

FROM runtime as final

WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RabbitMqPubSub.dll"]