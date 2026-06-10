# Handoff — Loja Tôa Tôa

> Transferência para um novo agente continuar a **loja Tôa Tôa** (e-commerce de vestidos,
> Blazor .NET 10 + MudBlazor). **Já está EM PRODUÇÃO.**
>
> ⚠️ Repositório público: dados sensíveis (IP de origem da VM, chave SSH, senhas) foram
> **omitidos** — estão fora do versionamento (com o responsável pela infra).

## 1. Estado: PUBLICADO

- **Produção:** https://toatoafesta.com (HTTPS via Cloudflare) — loja + admin no ar.
- **Repo:** este repositório (branch `master`).
- **Plano original:** `jolly-sniffing-lemon` (sessão de planejamento).
- **Guia de deploy:** [`DEPLOY-ORACLE.md`](DEPLOY-ORACLE.md) (seção "HTTPS via Cloudflare").

## 2. Infra de produção (Oracle Cloud + Cloudflare)

- **VM:** Oracle Cloud Always Free, E2.1.Micro (x86, 2 vCPU, ~1 GB RAM + **4 GB swap**),
  Ubuntu 22.04, São Paulo. IP público, usuário `ubuntu` e chave SSH **fora do repo**.
- **Rede OCI:** VCN + subnet público + Internet Gateway + rota 0.0.0.0/0; portas 80/443/9000/9001/22
  liberadas (Security List + iptables da VM).
- **Cloudflare:** `toatoafesta.com` e `cdn.toatoafesta.com` → IP da VM (Proxied).
  **SSL/TLS = Flexible** (essencial; sem isso dá ERR_SSL_VERSION/525).
- **Containers** (`docker compose` em `~/toatoa` na VM): `toatoa-app` (8080), `toatoa-caddy`
  (80, roteia por host), `toatoa-minio` (9000/9001). Volumes `toatoa-data` (SQLite) e `minio-data`.
- **Caddy** (HTTP, atrás do Cloudflare): `DOMAIN → app:8080`, `CDN_DOMAIN → minio:9000`.
  Arquivos: `Caddyfile.cf` + `docker-compose.cf.yml`. Variáveis no `.env` (não versionado;
  ver `.env.example`): `APP_PORT=8080`, `PUBLIC_HOST`, `DOMAIN`, `CDN_DOMAIN`, credenciais MinIO.

## 3. Como redeployar

```bash
git push origin master                 # local: commit + push
# na VM (via SSH):
cd ~/toatoa && git pull
nohup sudo docker compose -f docker-compose.yml -f docker-compose.cf.yml up -d --build > ~/deploy.log 2>&1 &
tail -f ~/deploy.log                    # build ~8-12 min na VM de 1GB (reaproveita cache)
```
> Mudança só de config/env: `up -d` sem `--build` (rápido).
> Após deploy, se ver conteúdo antigo: Cloudflare → Caching → **Purge Everything**.

## 4. Armadilhas já resolvidas (NÃO repetir)

- **Dev local:** .NET 10 em `~/.dotnet` (não apt); sem `libicu` → rodar com
  `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1`. Moeda via `MoneyHelper.Brl()` (não `ToString("C2")`).
- **Build local antes de `dotnet run --no-build`** após criar migration.
- **Interatividade por página:** Router estático no servidor (`Components/Routes.razor`); páginas
  com `@rendermode InteractiveServer`. NÃO usar interatividade global.
- **Layout é estático** → `@onclick` no layout não funciona. Toggles de menu usam
  **`MudStaticNavDrawerToggle`**. Links de home usam `Href="/"` (não `Href=""`).
- **EF:** dois contextos (`ApplicationDbContext`/`app.db`, `CatalogoDbContext`/`catalogo.db`),
  comandos com `--context`; connection strings com `/`. Pacote `EntityFrameworkCore.Design 10.*`
  explícito. Nas páginas admin usar `ToaToa.Domain.Caixa` (colide com a página `Caixa.razor`).
- **ImageSharp v2.1** (Apache-2.0); v4 exige licença.
- **`pkill -f "ToaToa.dll"`/`"docker compose up"`** casa com o próprio shell/SSH — evitar.
- **Mixed-content:** loja HTTPS exige imagens HTTPS → MinIO via `CDN_DOMAIN` (Caddy). Vestidos
  de exemplo usam imagens externas (picsum) https.

## 5. Preferências do usuário (seguir sempre)

- pt-br; explicar antes/depois e pedir permissão em ações relevantes.
- Commits com `user=devops` / `email=git@cti-cdsit`.
- UI/layouts **sempre** com componentes MudBlazor.
- NÃO criar contas/serviços em nome do usuário; não guardar senhas.

## 6. Próximos passos sugeridos

- **Checkout/pagamento** (gateway) — "Finalizar compra" é placeholder hoje.
- Logo **vetorial oficial** (a atual foi extraída de foto; fundo removido via ImageSharp).
- Testar **upload de foto no admin de produção** (vai pro MinIO → `cdn`).
- **Trocar senhas** (admin seed e MinIO) em produção.
- Revisar warning do `MudFileUpload` (VestidoEditar.razor) p/ a UI de upload ficar 100%.
- Testes automatizados (bUnit/xUnit); responsividade de páginas específicas.

## 7. Suggested skills

- **verify** / **run** — validar mudanças rodando o app.
- **code-review** / **simplify** — antes de mergear features.
- **ssh-remote** — operações na VM de produção.
- **update-config** — allowlist de comandos (dotnet/git/ssh) p/ menos prompts.
- **claude-api** — se a tarefa mencionar Claude/Anthropic/LLM.
