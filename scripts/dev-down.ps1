$ErrorActionPreference = "Stop"

Write-Host ">> Parando infraestrutura local do Supabase..."
npx --yes supabase stop

Write-Host "Infraestrutura local parada."
