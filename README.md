# AuditFlow

AuditFlow is a **portfolio project** that demonstrates a **distributed, event-driven auditing system** in .NET. It models a real-world requirement: tracking **who changed what, when, and why** on product data. Architecture is production-inspired (API + message broker + background consumer + SQL) but operated in a cost‑conscious way.

---

## 💡 Project Idea (What this shows)

- Clean separation of **business operations** (Products CRUD) from **audit logging**.
- **Event-driven** change capture: API writes business data and emits an audit event; Consumer processes the event and writes detailed audit rows.
- Practical cloud mix: **API hosted** publicly, while **Consumer runs locally** to keep the stack **$0**.
- Both API and Consumer target the **same SQL database** (`AuditFlowDb`) so you can inspect or join business and audit data easily.

---

## 🏗️ Architecture Overview

- **AuditFlow.API** – ASP.NET Core Web API (JWT, Swagger) for managing products. Emits audit events.
- **AuditFlow.Consumer** – Background worker that consumes audit events from **Amazon SQS** and writes audit rows to SQL.
- **Azure SQL (AuditFlowDb)** – Single database used by **both** API and Consumer.
- **Amazon SQS** – Message broker (SQS queue) between API and Consumer.

**Data flow**: Client → **API** → **Azure SQL** (Products) + **SQS** event → **Consumer** (local) → **Azure SQL** (Audit tables).

---

## 🚀 Hosting Setup (Current)

- **API**: Hosted on Render (public).
- **Consumer**: **Runs locally only** (kept off the cloud to stay free).
- **Database**: **Azure SQL** – both API and Consumer point to the same DB **`AuditFlowDb`**.
- **Broker**: **Amazon SQS**.

> When the Consumer is **not running**, SQS safely queues audit messages. When you start the Consumer locally, it **catches up** and processes the backlog.

---

## 🔑 Authentication (Testing)

A short‑lived JWT is available for testing via the API:

```
POST /dev/token
```

Use the returned token as `Authorization: Bearer <token>` when calling other endpoints.

> Note: `/dev/token` exists purely for testing on this demo stack.

---

## ✅ Prerequisites

- **.NET 9 SDK**
- **EF Core Tools** (once):
  ```bash
  dotnet tool install -g dotnet-ef
  ```
- **SQL Server** (Azure SQL connection string), and permission to create tables.
- **AWS credentials** for SQS:
  - Locally via **AWS CLI/profile** or environment variables:
    `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_REGION` (e.g., `ap-southeast-2`).

---

## 🗄️ Database & Migrations (Required for **all** run modes)

> EF migrations must be applied **before** running the API/Consumer in any mode (local/dev/prod/Docker). Because EF tooling reads `appsettings*.json`, **temporarily set the desired DB connection string in `appsettings.json` or `appsettings.Development.json`** and run the updates.

Apply migrations for **both** projects (they each have their own migrations) to the **same database** `AuditFlowDb`:

```bash
# 1) API migrations → AuditFlowDb
cd src/AuditFlow.API
dotnet ef database update

# 2) Consumer migrations → AuditFlowDb (same connection string)
cd ../AuditFlow.Consumer
dotnet ef database update
```

If you prefer not to edit files, you can supply the connection string via env var while running the EF command:

```bash
# Example: override connection string just for the migration step
ConnectionStrings__DefaultConnection="<YOUR_AZURE_SQL_CONN_STR>" dotnet ef database update
```

> **Docker Debug tip (Visual Studio “Docker” launch profile):** When debugging inside a Docker container and connecting **from the container to your host SQL Server/SQL container**, use
> `Server=host.docker.internal,1400` (port **1400**, not 1433). Port 1433 is typically used by your local SQL Server instance on the host, so the SQL container is usually mapped to **1400→1433**.

---

## 🖥️ Running Locally (Two Ways)

You can run locally with either a **local SQL Server install** or a **SQL Server container**. In both cases, the **Consumer points to the same `AuditFlowDb`** used by the API.

### A) Local machine (MSSQL installed locally)

1. **Set connection string** in `appsettings.Development.json` of **both** API and Consumer (or export `ConnectionStrings__DefaultConnection`). Target the same DB (e.g., `AuditFlowDb`).
2. **Apply migrations** (both projects) – see **Database & Migrations** above.
3. **Start API**:
   ```bash
   cd src/AuditFlow.API
   dotnet run
   ```
4. **Start Consumer** (local worker; no HTTP required):
   ```bash
   cd src/AuditFlow.Consumer
   # Ensure AWS env vars or profile are available
   dotnet run
   ```
5. **Try the API**: open Swagger at `http://localhost:<port>/`, call `POST /dev/token`, then use Products endpoints. Watch the Consumer console to see audits being processed.

### B) Local via Docker (MSSQL container)

1. **Start SQL Server in Docker** — map **host port 1400 → container 1433** (so it doesn’t clash with local SQL Server on 1433):
   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_strong_password123" \
     -p 1400:1433 --name mssql -d mcr.microsoft.com/mssql/server:2022-latest
   ```
2. **Set connection string** for both API & Consumer to point at the container (notice **1400** and **host.docker.internal** if connecting from containers started by VS/Compose):
   - From **host** processes (your shell):
     `Server=localhost,1400;Database=AuditFlowDb;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True;`
   - From **another container** (e.g., VS “Docker” launch profile):
     `Server=host.docker.internal,1400;Database=AuditFlowDb;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True;`
3. **Apply migrations** for API and Consumer – see **Database & Migrations**.
4. **Run API & Consumer** with `dotnet run` as in option A, or build/run containers (see below).

---

## 🐳 Build & Run with Docker

### Build (locally)

> Below are typical examples. Adjust paths if your Dockerfiles differ.

**API image**:
```bash
docker build -f "src/AuditFlow.API/Dockerfile" -t rohitkrsoni/audit-flow-api:latest .
```

**Consumer image** (only if you have a Dockerfile for it):
```bash
docker build -f "src/AuditFlow.Consumer/Dockerfile" -t rohitkrsoni/audit-flow-consumer:latest .
```

### Run images locally (non‑debug, no launch profiles)

**API container** (map default HTTP/HTTPS ports and pass AWS + optional connection string):
```bash
docker run --rm -p 8080:8080 -p 8081:8081 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e AWS_ACCESS_KEY_ID=<YOUR_AWS_ACCESS_KEY> \
  -e AWS_SECRET_ACCESS_KEY=<YOUR_AWS_SECRET> \
  -e AWS_REGION=ap-southeast-2 \
  # Optional: override DB if not using appsettings.json value:
  -e ConnectionStrings__DefaultConnection="<YOUR_AZURE_SQL_CONN_STR>" \
  rohitkrsoni/audit-flow-api:latest
```

**Consumer container** (no ports needed unless you add HTTP endpoints):
```bash
docker run --rm \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e AWS_ACCESS_KEY_ID=<YOUR_AWS_ACCESS_KEY> \
  -e AWS_SECRET_ACCESS_KEY=<YOUR_AWS_SECRET> \
  -e AWS_REGION=ap-southeast-2 \
  -e ConnectionStrings__DefaultConnection="<YOUR_AZURE_SQL_CONN_STR>" \
  rohitkrsoni/audit-flow-consumer:latest
```

> If your local setup uses an AWS profile directory instead of env vars, you can also mount credentials into the container (development only).

### Publish to a registry

Your two-step publish flow (API example shown; mirror for Consumer if you have its Dockerfile):

```bash
# API
docker build -f "src/AuditFlow.API/Dockerfile" -t rohitkrsoni/audit-flow-api:latest .
docker push rohitkrsoni/audit-flow-api:latest

# Consumer (if applicable)
docker build -f "src/AuditFlow.Consumer/Dockerfile" -t rohitkrsoni/audit-flow-consumer:latest .
docker push rohitkrsoni/audit-flow-consumer:latest
```

---

## 🧪 Quick Demo Flow (Hosted API + Local Consumer)

1. **Start Consumer locally** with AWS creds + Azure SQL connection.
2. **Open hosted API (Swagger)** at `https://<your-render-api-url>/`.
3. `POST /dev/token` → get JWT.
4. Create/update/delete products.
5. Watch local Consumer logs to see audit events being processed into **the same `AuditFlowDb`**.

---

## 📚 API Overview

- `POST /dev/token` – retrieve test JWT
- `POST /products` – create product
- `GET /products` – list (paginated)
- `GET /products/{id}` – get by id
- `PUT /products/{id}` – update
- `DELETE /products/{id}` – delete
- `GET /health` – health check

---

## ⚠️ Notes / Responsible Usage

- This is a **portfolio/demo** project; **/dev/token** is for testing only.
- Azure SQL is cost‑controlled; data may be cleared if the DB fills up.
- Keep your AWS & DB credentials out of source control. Provide them via environment variables or secret stores.

---

## 🔮 Future Work

- Optional hosted background worker (Render Worker / Azure Container Apps / AWS Lambda + SQS).
- Read‑only **Audit Viewer** endpoints (e.g., `/audits`) to visualize the flow online.
- `docker-compose` to start API, Consumer, SQL, and local SQS emulation in one command.
- Integration/unit tests across API, Consumer, and Shared.
