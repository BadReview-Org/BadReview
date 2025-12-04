# BadReview.Api

## Overview

BadReview.Api is the backend REST API for the BadReview application. It provides endpoints for managing game reviews, user authentication, and integration with the IGDB (Internet Game Database) API. Built with ASP.NET Core 9.0 using minimal APIs architecture.

- Uses FluentValidation for request validation
- Implements service layer architecture 
- Follows REST conventions
- Minimal API approach for lightweight endpoint definitions
- Dependency injection configured in Configuration folder
- Authentication & Authorization with Identity and JWT Tokens, with different policies 
- LINQ and custom mapper class implemented to fetch and transform entities and DTOs
## Technologies

- .NET 9.0
- ASP.NET Core Minimal APIs
- Entity Framework Core 9.0 (Fluent API)
- PostgreSQL (via Npgsql)
- JWT Authentication
- FluentValidation
- Swagger/OpenAPI
- IGDB API Integration


## Architecture


### Folder Structure
```bash
BadReview.Api/
├── Configuration/           # App builder configuration       
├── Data/                    # EF database context and configuration
├── Endpoints/               # API endpoints
├── Mapper/                  # Mapper utility for entities and DTOs
├── Migrations/              # EF migrations folder
├── Models/                  # EF models/entities
│   └── Owned/               # Owned entities
├── Properties/
│   └── launchSettings.json  # Server settings
├── Services/                # Service layer
├── appsettings.json         # Global configuration file
├── BadReview.Api.csproj     # Project file (packages, etc.)
└── Program.cs               # API program's entry point
```

### Endpoints

The API exposes the following endpoint groups:

- **UserEndpoints** - User authentication (login, register, refresh tokens) and profile management
- **GameEndpoints** - Game catalog browsing, trending games, and detailed game information
- **ReviewEndpoints** - User reviews for games (CRUD operations)
- **GenreEndpoints** - Game genres information
- **DeveloperEndpoints** - Game developers information
- **PlatformEndpoints** - Game platforms information

### Service Layer

- **AuthService** - JWT token generation and validation
- **UserService** - User management and authentication logic
- **GameService** - Game data retrieval and caching from IGDB
- **ReviewService** - Review business logic 
- **GenreService** - Genre data retrieval and caching from IGDB
- **DeveloperService** - Developer data retrieval and caching from IGDB
- **PlatformService** - Platform data retrieval and caching from IGDB
- **IGDBClient** - HTTP client for IGDB API integration

## Configuration

### Required Settings

Configure the following in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=BadReviewDb;Username=postgres;Password=pass;Port=5432"
  },
  "IGDB": {
    "ClientId": *IGDB CLIENT ID*,
    "ClientSecret": *IGDB CLIENT SECRET*,
    "TokenURI": "https://id.twitch.tv/oauth2/token",
    "URI": "https://api.igdb.com/v4/"
  },
  "Jwt": {
    "Key": *JWT SECURITY KEY*,
    "Issuer": "https://localhost:5003",
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```
Where \*IGDB CLIENT ID\* and \*IGDB CLIENT SECRET\* are your own IGDB credentials, and \*JWT SECURITY KEY\* is a valid security key used for the JWT's signature validation, e.g.\
`cngiINE+O6E4toJMd7A1CN97Ct77AsWubHFFx57Rkbkclkmozecx+lyZGuB+Nyz6`\
(randomly generated for local development purposes). You can pick another one or use **openssl** to generate a new one.

### IGDB API Setup

1. Register at [Twitch Developer Portal](https://dev.twitch.tv/)
2. Create an application to get Client ID and Client Secret
3. Add credentials to `appsettings.json` or environment variables

## Database

### Migrations

The project uses Entity Framework Core migrations for database schema management.

#### Create a new migration:
```bash
dotnet ef migrations add MigrationName
```

#### Apply migrations:
```bash
dotnet ef database update
```

#### Delete database:
In case any error occurs, you can delete the database as follows:
```bash
dotnet ef database drop --force
```
Then, you can re-apply the migrations or delete migrations folder and create a new one.

### Models

Core domain entities:
- User
- Game
- Review
- Genre
- Developer
- Platform
- GameGenre (many-to-many)
- GameDeveloper (many-to-many)
- GamePlatform (many-to-many)

Owned entities for additional data:

- Image
- CUDate

## Authentication & Authorization

The API uses JWT Bearer tokens with two token types:

- **Access Token** - Short-lived token for API access (policy: `AccessTokenPolicy`)
- **Refresh Token** - Long-lived token for obtaining new access tokens (policy: `RefreshTokenPolicy`)

Protected endpoints require the `Authorization` header:
```
Authorization: Bearer <[access/refresh]_token>
```

## CORS

CORS is configured to allow requests from the Blazor WebAssembly client running on `https://localhost:5193`. You can modify, delete or add more domains in `Configuration/CorsConfig.cs`.

## Running the API

### Development
```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5003`
- Swagger UI: `http://localhost:5003/swagger`

### Build
```bash
dotnet build
```

### Production
```bash
dotnet publish -c Release
```

## API Documentation

Interactive API documentation is available via Swagger UI at `/swagger` when running in development mode.

All endpoints are documented with:
- OpenAPI specifications
- Request/response schemas
- Authentication requirements
- Status codes and error responses

## Dependencies

### NuGet Packages
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.AspNetCore.OpenApi
- Microsoft.EntityFrameworkCore
- Npgsql
- Npgsql.EntityFrameworkCore.PostgreSQL
- Microsoft.EntityFrameworkCore.Tools
- Swashbuckle.AspNetCore

### Project References
- BadReview.Shared (DTOs and shared utilities)


[Back to main README](../README.md)