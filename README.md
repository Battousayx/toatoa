# ToaToa 👗

E-commerce de **vestidos** em **Blazor (.NET 10)** com **MudBlazor**, contendo loja pública
(vitrine) e módulo administrativo completo (catálogo, PDV, caixa e relatórios).

## Stack

- **.NET 10** / Blazor Web App — render **Interactive Auto** com prerender (SEO)
- **MudBlazor 9.5** — UI (tema fashion premium minimalista)
- **EF Core 10 + SQLite** — 2 contextos: Identity (`app.db`) e catálogo (`catalogo.db`)
- **ASP.NET Core Identity** — autenticação do admin (role `Admin`)
- **MinIO** (S3-compatible) — armazenamento das fotos dos vestidos

## Funcionalidades

### Loja pública
- Home com hero, categorias em destaque e vitrine de produtos
- Catálogo com filtro por categoria
- Página do vestido com galeria de fotos e seleção de tamanho/cor
- Carrinho (sacola) com ajuste de quantidade

### Admin (`/admin`, requer login com role Admin)
- Dashboard com indicadores
- CRUD de **Categorias**
- CRUD de **Vestidos** com **variantes** (tamanho/cor/estoque) e **galeria de fotos no MinIO**
- **PDV** — ponto de venda com baixa de estoque e formas de pagamento
- **Caixa** — abertura (fundo), sangria/suprimento, fechamento com conferência e diferença
- **Vendas do dia** — relatório com totais por forma de pagamento

## Como rodar

### Opção A — Standalone com Docker Compose (recomendado)

Sobe a **aplicação + MinIO** juntos, com um comando só:

```bash
docker compose up -d --build
```

- Loja: http://localhost:5099
- Console MinIO: http://localhost:9001 (minioadmin / minioadmin)

Migrations e seed (role Admin, usuário e dados de exemplo) rodam no startup. Os bancos
SQLite e os arquivos do MinIO persistem em volumes (`toatoa-data`, `minio-data`).

> **Hospedando fora do localhost:** as imagens dos vestidos são servidas pelo MinIO
> direto ao navegador. Ajuste `Minio__PublicBaseUrl` no `docker-compose.yml` para o
> host/domínio público (ex.: `https://cdn.seudominio.com`) e exponha a porta 9000.

### Opção B — Desenvolvimento local (.NET SDK)

Pré-requisitos: .NET 10 SDK + Docker (só para o MinIO).

```bash
docker compose up -d minio                 # só o MinIO
cd ToaToa
dotnet run --no-launch-profile --urls "http://localhost:5099"
```

> **Nota (ICU/globalização):** se a máquina não tiver `libicu`, rode em invariant mode com
> `export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1`. A formatação de R$ é feita por
> `MoneyHelper.Brl()` e funciona mesmo assim. (No container Docker a `libicu` já existe.)

### Acesso
- Loja: http://localhost:5099/
- Admin: http://localhost:5099/admin
- **Login admin:** `admin@toatoa.local` / `Admin@123` *(troque em produção!)*

## Configuração

`ToaToa/appsettings.json`:
- `ConnectionStrings:DefaultConnection` / `CatalogoConnection` — bancos SQLite
- `Minio` — endpoint, credenciais, bucket (`toatoa-vestidos`) e URL pública

## Estrutura

```
ToaToa/
├─ docker-compose.yml          # MinIO
├─ ToaToa/                     # Servidor (host, Identity, EF, admin, loja)
│  ├─ Domain/                  # Entidades (Vestido, Venda, Caixa, ...)
│  ├─ Data/                    # DbContexts, migrations, seed
│  ├─ Services/                # Serviços de aplicação + MinIO + carrinho
│  ├─ Theme/                   # Tema MudBlazor
│  └─ Components/
│     ├─ Layout/               # LojaLayout, AdminLayout
│     └─ Pages/{Loja,Admin}/   # Páginas
└─ ToaToa.Client/              # WebAssembly (componentes interativos)
```

## Escopo futuro
- Checkout/pagamento (gateway) e gestão de pedidos no admin
- Testes automatizados (bUnit / xUnit)
- Deploy (Docker/Kubernetes)
