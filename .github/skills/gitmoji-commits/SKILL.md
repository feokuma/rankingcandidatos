---
name: gitmoji-commits
description: "Use quando o usuário pedir para criar commits, padronizar mensagens de commit, ou aplicar padrão gitmoji. Gera mensagens no formato gitmoji + tipo + escopo opcional + descrição curta em português, e orienta sobre separação de commits por objetivo."
user-invocable: true
---

# Gitmoji Commits

## Objetivo

Padronizar commits do projeto com gitmoji para deixar o histórico mais legível, consistente e semântico.

## Formato padrão

Use este formato para a primeira linha do commit:

```text
<gitmoji> <tipo>(<escopo opcional>): <descrição curta>
```

Exemplos válidos:

- `✨ feat(api): adicionar endpoint de ranking de candidatos`
- `🐛 fix(frontend): corrigir envio de avaliação negativa`
- `📝 docs(readme): atualizar instruções de execução local`
- `♻️ refactor(api): simplificar validação de candidato`
- `✅ test(api): adicionar testes de cadastro de candidato`
- `🔧 chore(repo): ajustar gitignore para bin e obj`

## Regras de escrita

1. Escrever em português (pt-BR).
2. Usar verbo no infinitivo: adicionar, corrigir, atualizar, remover.
3. Preferir mensagens curtas e específicas.
4. Não misturar mudanças não relacionadas no mesmo commit.
5. Quando houver múltiplas intenções, dividir em commits menores.

## Gitmojis recomendados

- `✨` para novas funcionalidades (`feat`)
- `🐛` para correções (`fix`)
- `📝` para documentação (`docs`)
- `♻️` para refatoração (`refactor`)
- `✅` para testes (`test`)
- `🔧` para manutenção/configuração (`chore`)
- `🚀` para melhorias de performance (`perf`)
- `🎨` para ajustes de estilo/interface (`style`)
- `🔥` para remoção de código/arquivos (`remove`)

## Checklist antes de commitar

1. Verificar mudanças com `git status`.
2. Revisar diff com `git diff` (ou `git diff --staged`).
3. Garantir que backend/frontend compilam quando aplicável.
4. Agrupar arquivos por objetivo e commitar de forma incremental.

## Comandos sugeridos

```bash
git add <arquivos>
git commit -m "✨ feat(api): adicionar endpoint de ranking de candidatos"
```

```bash
git add README.md
git commit -m "📝 docs(readme): atualizar como executar a aplicação"
```
