$ErrorActionPreference = "Stop"

Write-Host ">> Resetando banco local do Supabase (migrations + seed)..."
npx --yes supabase db reset

Write-Host "Banco local resetado."
