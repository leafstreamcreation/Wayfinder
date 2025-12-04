# Wayfinder API - .NET Framework 4.8 with ASP.NET MVC 4
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2019

# Set working directory
WORKDIR /inetpub/wwwroot

# Copy published files
COPY ./Wayfinder.API/bin/ ./bin/
COPY ./Wayfinder.API/Web.config ./
COPY ./Wayfinder.API/Global.asax ./

# Expose port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD powershell -Command "try { $response = Invoke-WebRequest -Uri 'http://localhost/api/health' -UseBasicParsing; if ($response.StatusCode -eq 200) { exit 0 } else { exit 1 } } catch { exit 1 }"
