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
      "districtId": 43,
      "districtName": "Chattogram",
      "temp2Pm": 23.24,
      "pm25_2Pm": 35.22,
      "rank": 1
    },
    {
      "districtId": 22,
      "districtName": "Nawabganj",
      "temp2Pm": 24.74,
      "pm25_2Pm": 42.5,
      "rank": 2
    },
    {
      "districtId": 62,
      "districtName": "Meherpur",
      "temp2Pm": 24.82,
      "pm25_2Pm": 43.120000000000005,
      "rank": 3
    },
    {
      "districtId": 45,
      "districtName": "Cox's Bazar",
      "temp2Pm": 24.84,
      "pm25_2Pm": 50.7,
      "rank": 4
    },
    {
      "districtId": 60,
      "districtName": "Kushtia",
      "temp2Pm": 24.9,
      "pm25_2Pm": 45.14,
      "rank": 5
    },
    {
      "districtId": 26,
      "districtName": "Dinajpur",
      "temp2Pm": 25.040000000000003,
      "pm25_2Pm": 49.779999999999994,
      "rank": 6
    },
    {
      "districtId": 24,
      "districtName": "Rajshahi",
      "temp2Pm": 25.1,
      "pm25_2Pm": 44.56,
      "rank": 7
    },
    {
      "districtId": 17,
      "districtName": "Tangail",
      "temp2Pm": 25.1,
      "pm25_2Pm": 50.06,
      "rank": 8
    },
    {
      "districtId": 2,
      "districtName": "Faridpur",
      "temp2Pm": 25.18,
      "pm25_2Pm": 46.06,
      "rank": 9
    },
    {
      "districtId": 28,
      "districtName": "Kurigram",
      "temp2Pm": 25.240000000000002,
      "pm25_2Pm": 45.08,
      "rank": 10
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
