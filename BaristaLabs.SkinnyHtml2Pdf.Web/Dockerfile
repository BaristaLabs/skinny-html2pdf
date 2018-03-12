# Genius: https://www.atlascode.com/blog/running-asp-net-core-in-an-alpine-linux-docker-container/
FROM microsoft/dotnet:2.1-runtime-alpine AS base

# Installs latest Chromium (64) package.
RUN apk update && apk upgrade && \
    echo @edge http://nl.alpinelinux.org/alpine/edge/community >> /etc/apk/repositories && \
    echo @edge http://nl.alpinelinux.org/alpine/edge/main >> /etc/apk/repositories && \
    apk add --no-cache \
      chromium@edge \
      nss@edge

ENV CHROME_LAUNCH_LOCAL true
ENV CHROME_BIN /usr/bin/chromium-browser
ENV CHROME_REMOTE_DEBUGGING_PORT 9222
ENV CHROME_NO_SANDBOX true

WORKDIR /app
EXPOSE 8080

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

# Add user so we don't need --no-sandbox.
RUN addgroup -S baristauser && adduser -S -g baristauser baristauser \
    && mkdir -p /home/baristauser/Downloads \
    && chown -R baristauser:baristauser /home/baristauser \
    && chown -R baristauser:baristauser /app

# Run everything after as non-privileged user.
USER baristauser

ENTRYPOINT ["dotnet", "BaristaLabs.SkinnyHtml2Pdf.Web.dll"]
