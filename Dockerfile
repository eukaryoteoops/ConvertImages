FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY . .
RUN dotnet publish ConvertImages.csproj -c Release -o /app
FROM microsoft/dotnet:2.2-aspnetcore-runtime-alpine
WORKDIR /app
COPY --chown=root --from=build /app . 
RUN mkdir images
EXPOSE 80/tcp
ENTRYPOINT ["dotnet", "ConvertImages.dll"]
