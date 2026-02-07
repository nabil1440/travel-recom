# Travel Recommendation API

A small service that recommends travel destinations based on temperature and air quality forecasts, with a live district leaderboard.

## Run with Docker

```bash
docker compose -f docker-compose.dev.yml up -d --build
```

The API will be available at:

- http://localhost:5000

To stop the stack:

```bash
docker compose -f docker-compose.dev.yml down
```

## API Documentation

### POST /api/travel/recommendation

Request body:

```json
{
  "latitude": 23.8103,
  "longitude": 90.4125,
  "destination": "Rajshahi",
  "travelDate": "2026-02-10"
}
```

Response:

```json
{
  "recommendation": "Recommended",
  "reason": "Your destination is 0.5°C cooler and has significantly better air quality. Enjoy your trip!"
}
```

Notes:

- Returns 400 if the travel date is in the past or destination is empty.

### GET /api/leaderboard?count=10

Response:

```json
{
  "districts": [
    {
      "districtId": 1,
      "districtName": "Dhaka",
      "temp2Pm": 28.5,
      "pm25_2Pm": 42.1,
      "rank": 1
    }
  ]
}
```

Notes:

- Returns 503 if leaderboard data is not ready yet.

### GET /health

Response:

```json
{
  "status": "ok",
  "checks": {
    "api": "ok",
    "database": "ok",
    "redis": "ok"
  }
}
```

### GET /hangfire

Hangfire dashboard for background jobs (no auth in dev).

## Dependencies

- .NET 10 SDK (for local development and tests)
- Docker + Docker Compose (recommended for running the stack)
- PostgreSQL 16
- Redis 7
- Key libraries: EF Core, Npgsql, Hangfire, StackExchange.Redis

## Design Considerations

- Clean architecture separation:
  - Api: HTTP endpoints and input validation
  - AppCore: domain models and service logic
  - Infrastructure: persistence, caching, external integrations
- Background processing via Hangfire:
  - A weather fetch job runs at startup and hourly
- Redis is used for leaderboard storage and leader election
- PostgreSQL is the source of truth for districts, snapshots, and forecasts
- Health endpoint validates API, database, and Redis readiness
- Seed data is loaded only when the database is empty
