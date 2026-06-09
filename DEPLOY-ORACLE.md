# Deploy no Oracle Cloud (Always Free) com Docker Compose

Guia para hospedar a **loja Tôa Tôa** (app + MinIO) **de graça** numa VM do
**Oracle Cloud Infrastructure (OCI) Always Free**. Roda o `docker compose` (app + MinIO),
com SQLite persistido em volume.

> Resultado: loja em `http://SEU_IP` (porta 80) e console MinIO em `http://SEU_IP:9001`.

---

## 1. Criar a conta e a VM

1. Crie a conta em https://www.oracle.com/cloud/free/ (pede cartão para verificação, mas os
   recursos **Always Free** não são cobrados).
2. **Compute → Instances → Create instance**:
   - **Shape:** `VM.Standard.A1.Flex` (ARM Ampere — *Always Free*). Sugestão: **2 OCPU / 12 GB**
     (o limite gratuito é 4 OCPU / 24 GB no total).
   - **Image:** Canonical **Ubuntu 22.04/24.04** (ARM).
   - **SSH:** salve a chave privada (`.key`) para acessar.
   - Anote o **IP público**.

> A imagem .NET 10 e o MinIO são multi-arch (têm `arm64`), então a build roda nativa na VM ARM.

## 2. Abrir as portas (DOIS lugares!)

### a) Security List da VCN (firewall do Oracle)
**Networking → Virtual Cloud Networks → sua VCN → Subnet → Security List → Add Ingress Rules**.
Adicione (Source `0.0.0.0/0`, TCP):
- `80` (loja)
- `9000` (API MinIO, usada pelo navegador p/ imagens)
- `9001` (console MinIO) — opcional, pode restringir ao seu IP
- `443` se for usar HTTPS/domínio

### b) Firewall do Ubuntu (na VM)
As imagens Ubuntu do Oracle vêm com firewall restritivo. Na VM:
```bash
sudo iptables -I INPUT 6 -m state --state NEW -p tcp --dport 80 -j ACCEPT
sudo iptables -I INPUT 6 -m state --state NEW -p tcp --dport 9000 -j ACCEPT
sudo iptables -I INPUT 6 -m state --state NEW -p tcp --dport 9001 -j ACCEPT
sudo netfilter-persistent save
# (se usar ufw em vez de iptables: sudo ufw allow 80,9000,9001/tcp)
```

## 3. Instalar Docker na VM

```bash
ssh -i sua-chave.key ubuntu@SEU_IP
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker $USER && newgrp docker   # usar docker sem sudo
```

## 4. Clonar e configurar

```bash
git clone https://github.com/Battousayx/toatoa.git
cd toatoa
cp .env.example .env
nano .env
```
No `.env`, ajuste:
```
PUBLIC_HOST=SEU_IP          # ex.: 159.65.10.20 (ou seu domínio)
APP_PORT=80                 # loja sem :porta na URL
MINIO_USER=admin
MINIO_PASSWORD=uma-senha-forte
```
> `PUBLIC_HOST` é essencial: é o endereço que o **navegador** usa para baixar as imagens do
> MinIO. Se ficar `localhost`, as fotos não carregam para visitantes.

## 5. Subir

```bash
docker compose up -d --build
docker compose logs -f app   # acompanha o boot (Ctrl+C para sair)
```

Acesse:
- Loja: `http://SEU_IP`
- Admin: `http://SEU_IP/admin` (login seed — veja o README; **troque a senha em produção**)
- Console MinIO: `http://SEU_IP:9001`

Migrations e seed rodam sozinhos. Dados persistem nos volumes `toatoa-data` (SQLite) e
`minio-data`.

## 6. Atualizar a aplicação depois

```bash
cd ~/toatoa && git pull && docker compose up -d --build
```

---

## (Opcional) HTTPS com domínio — Caddy (já incluído)

O repo já traz **`Caddyfile`** + **`docker-compose.tls.yml`** prontos. O Caddy emite
certificados **Let's Encrypt automaticamente** e serve a loja e as imagens do MinIO em HTTPS
(o subdomínio CDN evita *mixed content*).

**Pré-requisitos:**
1. Dois registros **A** apontando para o IP da VM, ex.: `loja.seudominio.com` e
   `cdn.seudominio.com`.
2. Abra **80** e **443** (Security List da VCN + iptables). Pode fechar a 9000 ao público
   (o Caddy passa a servir as imagens via `cdn.seudominio.com`).

**No `.env`:**
```
APP_PORT=8080                    # evita conflito com a porta 80 do Caddy
DOMAIN=loja.seudominio.com
CDN_DOMAIN=cdn.seudominio.com
```

**Subir com TLS:**
```bash
docker compose -f docker-compose.yml -f docker-compose.tls.yml up -d --build
```

Acesse `https://loja.seudominio.com`. As imagens são servidas por `https://cdn.seudominio.com`
(o `Minio__PublicBaseUrl` já é ajustado automaticamente pelo override).

---

## Dicas / solução de problemas

- **Fotos não carregam para visitantes:** `PUBLIC_HOST` está como `localhost`. Ajuste no `.env`
  e `docker compose up -d`.
- **Não abre de fora:** confira as **duas** camadas de firewall (Security List da VCN + iptables).
- **Pouca RAM/CPU:** aumente as OCPU/GB do shape A1 (até 4/24 grátis).
- **Backups:** faça snapshot do volume ou copie `toatoa-data`/`minio-data` periodicamente.
- **Resiliência do banco:** SQLite via volume funciona; se quiser, depois migramos para um
  Postgres gerenciado grátis (Neon/Supabase).
