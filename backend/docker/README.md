# Docker setup — Redis topologies

This folder contains a Docker Compose setup that brings up:

- A Redis master (`redis-master`) using `redis:8.6.0`, mapped to host port `8000`.
- A Redis slave (`redis-slave-1`) using `redis:8.6.0`, mapped to host port `8001` and configured to replicate from `redis-master`.
- A 3-node Redis Cluster (`redis-cluster-7000`, `redis-cluster-7001`, `redis-cluster-7002`) using `redis:8.6.0` (auto-bootstrap).

Start services (from repository root):

```powershell
cd docker
docker compose up -d
```

Verify master/slave:

```bash
redis-cli -p 8000 ping     # expect PONG
redis-cli -p 8001 ping     # expect PONG
redis-cli -p 8001 INFO replication  # verify master_link_status:up
```

Verify cluster nodes (optional):

```bash
redis-cli -p 7000 cluster nodes
```

Remove

```bash
docker compose down -v
```

Notes:
- The master is intentionally mapped to `8000` (host) to avoid colliding with a local Redis running on `6379`. If you want the app to connect to the containerized master without changing configuration, either stop local Redis or change `src/Roman.RedisManager.Web/appsettings.json` to point to `localhost:8000`.
- Named volumes are used to avoid Windows host-path mounting issues.
