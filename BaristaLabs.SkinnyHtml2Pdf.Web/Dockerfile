# Genius: https://www.atlascode.com/blog/running-asp-net-core-in-an-alpine-linux-docker-container/
FROM microsoft/dotnet:2.1-runtime-alpine AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk-alpine AS build
WORKDIR /src
#COPY *.sln ./
COPY BaristaLabs.SkinnyHtml2Pdf.Web/BaristaLabs.SkinnyHtml2Pdf.Web.csproj BaristaLabs.SkinnyHtml2Pdf.Web/
#RUN dotnet restore
COPY . .
WORKDIR /src/BaristaLabs.SkinnyHtml2Pdf.Web
RUN dotnet build -c Release -o /app -r alpine.3.6-x64

FROM build AS publish
RUN dotnet publish -c  Release -o /app/linux -r linux-x64
RUN dotnet publish -c Release -o /app/alpine -r alpine.3.6-x64

FROM base AS final
WORKDIR /app
COPY --from=publish /app/linux/libuv.so .
COPY --from=publish /app/alpine .
ENTRYPOINT ["dotnet", "BaristaLabs.SkinnyHtml2Pdf.Web.dll"]
