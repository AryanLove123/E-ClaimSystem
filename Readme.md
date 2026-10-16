# E-Claim System

An insurance claim management platform built with **ASP.NET Core Web API** (layered architecture), **Entity Framework Core + MySQL** and an **Angular** frontend. Claims move through a **configurable, database-driven workflow** (adjuster review, adjustment, approval) with role-based access, email and in-app notifications, audit logging and full containerization (Docker Compose and Kubernetes).

## Features

| Area | What the application does |
|---|---|
| **Users & security** | Registration with **email verification (Brevo)**, BCrypt password hashing, **JWT authentication**, role-based authorization (Claimant, Adjuster, Approver, Admin), profile management, admin user management |
| **Claims** | Claim creation as Draft, submission, supporting document upload (PDF/JPG/PNG), automatic **severity calculation** from the amount, status tracking, complete claim history |
| **Workflow engine** | Workflow **definitions** stored in the database and matched by claim type / amount / severity; per-claim workflow **instances** with step-level progress; automatic assignment to the next adjuster/approver; **pause and resume** for additional-document requests (no duplicate instances) |
| **Review & approval** | Adjuster review, amount adjustment, request additional documents, complete review; approver approve / reject; on final approval the claim moves to **Payment Pending** |
| **Notifications** | **Email notifications through Brevo** and an **in-app notification center** (bell icon, red unread badge, latest 10, mark as read, click-through to the claim, auto-refresh) |
| **Search & reports** | Server-side search/filter (status, type, severity, date range, claim/policy number) with paging; claim reports with Excel/PDF export |
| **Audit & logging** | Audit log of business actions, error log stored in the database, global exception handling with a trace id on every error response |
| **Data integrity** | Optimistic concurrency on claims, unique constraints and indexes, EF Core migrations, automatic seed data |
| **Quality & delivery** | FluentValidation, xUnit tests, Swagger/OpenAPI, Dockerfiles, Docker Compose, Kubernetes manifests |

## Project structure

```
backend/
  EClaim.Domain/          entities, enums, exceptions
  EClaim.Application/     services (business logic), DTOs, validators, interfaces
  EClaim.Infrastructure/  EF Core + MySQL, migrations, seed, JWT, Brevo email, file storage
  EClaim.API/             controllers, middleware, Program.cs, Dockerfile
  EClaim.Tests/           xUnit tests
frontend/eclaim-ui/       Angular app (Dockerfile + nginx.conf)
k8s/                      Kubernetes manifests
docker-compose.yml        one-command run of MySQL + API + frontend
```

## Demo credentials

The application seeds these users on first start (migrations and seed run automatically when the API starts):

| Role | Email | Password |
|---|---|---|
| Claimant | claimant.demo@example.com | P@ssw0rd! |
| Adjuster | adjuster.demo@example.com | P@ssw0rd! |
| Approver | approver.demo@example.com | P@ssw0rd! |
| Admin | admin.demo@example.com | P@ssw0rd! |

New users can also self-register as Claimants; the verification link is sent by email (Brevo).

Choose **one** of the three ways to run the project below.

---

# Option A - Run locally (no containers)

## A1. Prerequisites

- [.NET 8 SDK and above](https://dotnet.microsoft.com/download)
- [MySQL Server 8.x](https://dev.mysql.com/downloads/mysql/) running locally
- [Node.js 20 LTS](https://nodejs.org/) (includes npm)
- Angular CLI: `npm install -g @angular/cli`
- EF Core tooling: `dotnet tool install --global dotnet-ef`
- VS Code (recommended extensions: C# Dev Kit, Angular Language Service)

## A2. Configure the backend

Edit `backend/EClaim.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=eclaimdb;User=root;Password=YOUR_PASSWORD;TreatTinyAsBoolean=true;"
  },
  "Jwt": {
    "Issuer": "EClaimSystem",
    "Audience": "EClaimSystemClient",
    "Secret": "REPLACE_WITH_A_LONG_RANDOM_STRING_AT_LEAST_32_CHARACTERS",
    "ExpirationMinutes": 60
  },
  "Email": {
    "BrevoApiKey": "YOUR_BREVO_API_KEY",
    "FromAddress": "your-verified-brevo-sender@example.com"
  }
}
```

## A3. Create the database schema

The schema is created entirely from EF Core migrations; no tables are created by hand. From the `backend` folder:

```powershell
dotnet ef database update --project EClaim.Infrastructure --startup-project EClaim.API
```

(The API also applies pending migrations and seeds demo data automatically on startup.)

## A4. Build and run the backend

```powershell
cd backend\EClaim.API
dotnet restore
dotnet build
dotnet run
```

The API listens on `http://localhost:5108`; Swagger is at `http://localhost:5108/swagger`.

## A5. Run the frontend

```powershell
cd frontend\eclaim-ui
npm install
npm start
```

Check `src/environments/environment.ts` - `apiUrl` must be `http://localhost:5108/api`. The app opens at **http://localhost:4200**.

## A6. Try the application

1. Log in as the **Claimant**, create a Vehicle claim (e.g. 20,000), upload a document and **Submit** it. The adjuster's bell shows *New claim assigned to you*.
2. Log in as the **Adjuster** (use a second browser or profile to see live updates). Open the claim, adjust the amount, then either **Request Additional Documents** (the workflow pauses and the claimant is notified) or **Complete Review**.
3. As the Claimant, upload the requested document and click **Resume Review** - the same workflow instance continues.
4. Log in as the **Approver** and **Approve** (the claim moves to *Payment Pending*) or **Reject**. The claimant is notified by in-app notification and email.
5. Open the claim's **History** tab to see every status change, and log in as **Admin** to manage users, view workflows and reports.

---

# Option B - Run with Docker Compose

## B1. Prerequisites

Docker with Compose v2 (Docker Desktop, or Docker Engine inside WSL). Nothing else - MySQL, .NET and Node run inside the containers.

## B2. Configure and start

From the repository root:

```bash
cp .env.example .env        # then edit the passwords / JWT secret / Brevo key in .env
docker compose up --build
```

`.env` holds:

```
MYSQL_ROOT_PASSWORD=...
MYSQL_PASSWORD=...
JWT_SECRET=...                 # 32+ characters
BREVO_API_KEY=...      #enables verification emails
```

## B3. Open the application

| What | URL |
|---|---|
| Application | http://localhost:4200 |
| API Swagger | http://localhost:5108/swagger |

## B4. What happens on start

1. The **MySQL** container starts with a persistent volume and passes its health check.
2. The **API** container starts, applies all EF Core migrations, seeds roles, demo users and workflows, then begins listening on port 8080.
3. The **frontend** container (nginx) serves the Angular build and forwards `/api` requests to the API container.

## B5. Useful commands

```bash
docker compose ps                    # container status
docker compose logs -f eclaim-api    # API logs
docker compose down                  # stop (data is kept)
docker compose down -v               # stop and delete database + uploads (fresh start)
```

---

# Option C - Run on Kubernetes (minikube)

## C1. Prerequisites

Docker, [kubectl](https://kubernetes.io/docs/tasks/tools/) and [minikube](https://minikube.sigs.k8s.io/docs/start/). (Docker Desktop's built-in Kubernetes also works - skip the minikube commands and open http://localhost:30080.)

## C2. Start the cluster and build the images inside it

From the repository root:

```bash
minikube start --driver=docker
eval $(minikube docker-env)          # point docker at minikube so the cluster can use local images

docker build -t eclaim-api:1.0 -f backend/EClaim.API/Dockerfile .
docker build -t eclaim-frontend:1.0 -f frontend/eclaim-ui/Dockerfile ./frontend/eclaim-ui
```

## C3. Create the secret

Create `k8s/secret.yaml` (git-ignored) with your own values. Keep the password inside the connection string equal to `MYSQL_PASSWORD`:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: eclaim-secrets
  namespace: eclaim
type: Opaque
stringData:
  MYSQL_ROOT_PASSWORD: "ChangeMe_Root_123"
  MYSQL_PASSWORD: "ChangeMe_App_123"
  ConnectionStrings__DefaultConnection: "Server=eclaim-mysql;Port=3306;Database=eclaimdb;User=eclaim;Password=ChangeMe_App_123;TreatTinyAsBoolean=true;AllowPublicKeyRetrieval=true;"
  Jwt__Secret: "ChangeMe_use_a_long_random_string_of_at_least_32_characters"
  Email__BrevoApiKey: "YOUR_BREVO_API_KEY"
```

## C4. Deploy

```bash
kubectl apply -f k8s/00-namespace.yaml
kubectl apply -f k8s/secret.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/mysql.yaml
kubectl apply -f k8s/api.yaml
kubectl apply -f k8s/frontend.yaml

kubectl get pods -n eclaim -w        # wait until every pod is Running / Ready (Ctrl+C to stop watching)
```

## C5. Open the application

```bash
minikube service eclaim-frontend -n eclaim
```

Keep this terminal open and browse to the URL provided. For Swagger: `kubectl port-forward -n eclaim svc/eclaim-api 5108:8080` then http://localhost:5108/swagger.

## C6. What each manifest does

| Manifest | Purpose |
|---|---|
| `00-namespace.yaml` | Isolates everything in the `eclaim` namespace |
| `secret.yaml` / `configmap.yaml` | Sensitive values (DB passwords, JWT secret, Brevo key) and non-sensitive settings, injected as environment variables |
| `mysql.yaml` | MySQL Deployment with a persistent volume and a stable `eclaim-mysql` Service |
| `api.yaml` | API Deployment (an init container waits for MySQL), uploads volume, readiness/liveness probes, `eclaim-api` Service |
| `frontend.yaml` | nginx Deployment (2 replicas) serving the Angular app and proxying `/api` to the API |

## C7. Useful commands

```bash
kubectl get all -n eclaim                              # everything that is running
kubectl logs deploy/eclaim-api -n eclaim               # API logs
kubectl delete pod -l app=eclaim-api -n eclaim         # self-healing: Kubernetes recreates the pod
kubectl scale deploy/eclaim-frontend --replicas=4 -n eclaim
kubectl delete namespace eclaim                        # remove everything
minikube start                                         # after a restart of the machine / WSL
```

---

## Configuration at a glance

| Setting | Local | Docker Compose | Kubernetes |
|---|---|---|---|
| Database host | `localhost` | `eclaim-mysql` | `eclaim-mysql` |
| Where settings come from | `appsettings.json` | `.env` | `secret.yaml` + `configmap.yaml` |
| Schema + seed | `dotnet ef database update` / API startup | API startup | API startup |
| Frontend calls the API at | `http://localhost:5108/api` | `/api` (nginx proxy) | `/api` (nginx proxy) |
| Application URL | http://localhost:4200 | http://localhost:4200 | http://localhost:8081 (port-forward) |
