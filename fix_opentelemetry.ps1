# Скрипт виправлення версій OpenTelemetry
# Виконувати в папці з файлом Medical_center.csproj

Write-Host "Cleaning project..." -ForegroundColor Yellow
dotnet clean Medical_center.csproj

Write-Host "Installing PRERELEASE versions of OpenTelemetry..." -ForegroundColor Yellow

# Видаляємо стару версію, якщо була
dotnet remove Medical_center.csproj package OpenTelemetry.Exporter.Prometheus.AspNetCore

# Встановлюємо нову версію з прапорцем --prerelease (це вирішить проблему з MapPrometheusScrapingEndpoint)
dotnet add Medical_center.csproj package OpenTelemetry.Exporter.Prometheus.AspNetCore --prerelease
dotnet add Medical_center.csproj package OpenTelemetry.Extensions.Hosting --prerelease
dotnet add Medical_center.csproj package OpenTelemetry.Instrumentation.AspNetCore --prerelease
dotnet add Medical_center.csproj package OpenTelemetry.Instrumentation.Http --prerelease
dotnet add Medical_center.csproj package OpenTelemetry.Instrumentation.Runtime --prerelease

Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore Medical_center.csproj

Write-Host "Building project..." -ForegroundColor Green
dotnet build Medical_center.csproj

if ($LastExitCode -eq 0) {
    Write-Host "SUCCESS! Build passed." -ForegroundColor Green
    Write-Host "Now you can run: dotnet run" -ForegroundColor Cyan
} else {
    Write-Host "Build FAILED. Please check errors above." -ForegroundColor Red
}