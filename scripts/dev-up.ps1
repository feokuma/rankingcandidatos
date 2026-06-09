param(
    [switch]$StartApps
)

$ErrorActionPreference = "Stop"

function Assert-Command {
    param([string]$Name)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Comando '$Name' nao encontrado."
    }
}

Assert-Command docker
Assert-Command dotnet
Assert-Command npm

Write-Host ">> Validando Docker..."
$null = docker info

Write-Host ">> Subindo infraestrutura local do Supabase..."
npx --yes supabase start

Write-Host ""
Write-Host "Infraestrutura pronta."
Write-Host "- API Supabase: http://127.0.0.1:54321"
Write-Host "- Studio Supabase: http://127.0.0.1:54323"
Write-Host "- Banco Postgres: 127.0.0.1:54322"

if ($StartApps) {
    Write-Host ""
    Write-Host ">> Iniciando API e Frontend em terminais separados..."

    Start-Process pwsh -ArgumentList "-NoExit", "-Command", "dotnet run --project backend/RankingCandidatos.Api"
    Start-Process pwsh -ArgumentList "-NoExit", "-Command", "Set-Location frontend/rankingcandidatos-web; npm install; npm run dev"
}
