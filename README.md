# NBA SQL Server Docker Setup

This Docker Compose configuration sets up a SQL Server 2025 instance with optional database restoration from backup.

## Prerequisites

- Docker and Docker Compose installed
- `nba.bak` backup file placed in the `./backups` directory (if you want to restore from backup)

## Directory Structure

```
.
├── docker-compose.yml
├── backups/
│   └── nba.bak
└── README.md
```

## First Time Setup (With Database Restore)

If you want to restore the NBA database from backup:

1. **Start the services with the init profile:**
   ```bash
   docker compose --profile init up -d
   ```
   This will:
   - Start the SQL Server container
   - Wait for SQL Server to be healthy
   - Restore the `nba.bak` backup to the database
   - The restore container will exit after completion

2. **Remove the restore container:**
   ```bash
   docker compose rm -f restore-db
   ```

## Subsequent Runs

For normal startup (without restore):

```bash
docker compose up -d
```

This will only start the SQL Server container without running the restore process.

## Connection Details

- **Host:** `localhost`
- **Port:** `14333`
- **Username:** `sa`
- **Password:** `Password12`
- **Database:** `nba` (after restore)

### Connection String Example
```
Server=localhost,14333;Database=nba;User Id=sa;Password=Password12;TrustServerCertificate=True;
```

## Stopping the Services

```bash
docker compose down
```

## Accessing SQL Server

You can connect to the SQL Server using any SQL client (Azure Data Studio, SQL Server Management Studio, etc.) or via command line:

```bash
docker exec -it nba_sqlserver25 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Password12 -C
```

## Important Notes

- The restore process checks if the `nba` database already exists before restoring
- If the database exists, the restore is skipped automatically
- Database files are stored inside the container at `/var/opt/mssql/data/`
- The SQL Server Agent is enabled by default
- Encryption is set to optional for easier local development

## Troubleshooting

### Database restore fails
- Ensure `nba.bak` exists in the `./backups` directory
- Check that the logical file names match (default: `nba` and `nba_log`)
- View restore container logs: `docker logs nba_restore`

### SQL Server won't start
- Check container logs: `docker logs nba_sqlserver25`
- Verify port 14333 is not already in use
- Ensure Docker has enough memory allocated (at least 2GB recommended)

### Connection refused
- Wait for the healthcheck to pass (may take 30-60 seconds on first start)
- Check if the container is running: `docker ps`
- Verify firewall settings aren't blocking port 14333