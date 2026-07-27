# Local Infrastructure (Docker)

FitForge runs SQL Server and Redis as local Docker containers. The API itself
still runs directly on your machine with `dotnet run` — only the database and
cache are containerized at this stage.

## First-time setup

1. Copy `.env.example` to `.env` in the repo root.
2. Adjust `SA_PASSWORD`, `SQLSERVER_PORT` or `REDIS_PORT` if needed.
3. `.env` is gitignored — never commit it.

## Common commands

Run these from the repo root (where `docker-compose.yml` lives).

| Action | Command |
| --- | --- |
| Start both containers in the background | `docker compose up -d` |
| Stop both containers (keeps data) | `docker compose stop` |
| Stop and remove containers (keeps data, volumes survive) | `docker compose down` |
| Stop and remove containers **and** delete all data | `docker compose down -v` |
| See container status | `docker compose ps` |
| Follow logs for both services | `docker compose logs -f` |
| Follow logs for one service | `docker compose logs -f sqlserver` |

## Verifying the containers work

**SQL Server:**
```
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "<your SA_PASSWORD>" -C -Q "SELECT 1"
```
Expect a result set with `1`.

**Redis:**
```
docker compose exec redis redis-cli ping
```
Expect `PONG`.

## Notes

- Data persists across `docker compose down` because it's stored in named
  Docker volumes (`fitforge_sqlserver_data`, `fitforge_redis_data`), not
  inside the container itself. Only `down -v` deletes it.
- The `-C` flag on `sqlcmd` trusts the server's self-signed certificate,
  which is expected for local development.
