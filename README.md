# BadReview

BadReview is a **full stack learning project built entirely in .NET 9**.\
This is a web application whose target audience are enthusiast gamers who want to **discover, rate and review new games**.
The application's concept is inspired by platforms like **IGDB** (API that's used to fetch videogames metadata) or **IMDB**, among others.

---

You can access the **website application** for the current release (v1.0) here: <a href="https://badreview.tech/" target="_blank" rel="noopener noreferrer">https://badreview.tech/</a>

### Developers

- <a href="https://www.linkedin.com/in/lautaroperalta" target="_blank" rel="noopener noreferrer">Lautaro Peralta</a>
- <a href="https://www.linkedin.com/in/manuelmhs1104" target="_blank" rel="noopener noreferrer">Manuel Herrera</a>

## Description

This **REST API and SPA** technology stack was built on top of **ASP.NET Core and Blazor WebAssembly**, using technologies such as Minimal APIs, Entity Framework Core, Fluent Validation, PostgreSQL (previously SQL Server), Postman, JWT Authentication, MudBlazor, etc.\
Videogame metadata is retrieved from the <a href="https://igdb.com" target="_blank" rel="noopener noreferrer">IGDB API</a> to show the latests game's information.

---

The program is bundled as a **single .NET's solution** which contains **three projects**: **BadReview.Api**, **BadReview.Client** and **BadReview.Shared**.

For more technical documentation, read some of the followings:

- **BadReview.Api**: This is a REST API built with .NET's Minimal APIs and Entity Framework Core.\
  <a href="BadReview.Api/README.md" target="_blank" rel="noopener noreferrer">API documentation</a>

- **BadReview.Client**: This is a SPA built with Blazor WebAssembly. We use MudBlazor's component library for richer UI components and styling.\
  <a href="BadReview.Client/README.md" target="_blank" rel="noopener noreferrer">Client documentation</a>

- **BadReview.Shared**: Contains shared DTOs, validators, definitions and utils to provide a common interface between the API and client.\
  <a href="BadReview.Shared/README.md" target="_blank" rel="noopener noreferrer">Shared documentation</a>

## Features

The user can access the following features through the SPA client:

- **Accounts**: Create a new account and update it's information. Basic but fully functional log in and log out system using JWT.
- **Explore**: Search for videogames, developers, platforms or genres using various filters.
- **Trending**: view the most relevant games at the moment (according to IGDB's information).
- **Review**: Create, update and delete reviews for any videogame. Mark it as a favorite, rate it, log dates and/or write a critic.
- **Profile page**: Access your own (or others) profile page, to see general information, latest activities and favorite games.

---

To read the full features and roadmap documentation, refer to the v1.0 release changelog.

## Requirements

If you wish to run this application locally, read these requirements. Otherwise, you can use the application through the website (read **Usage** section below).

---

To build and run the app, you will need to install in a compatible OS:
- <a href="https://dotnet.microsoft.com/en-us/download/dotnet/9.0" target="_blank" rel="noopener noreferrer">.Net 9</a>
- <a href="https://www.postgresql.org/download/" target="_blank" rel="noopener noreferrer">PostgreSQL 16</a>
- <a href="https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools" target="_blank" rel="noopener noreferrer">dotnet ef Tool</a>

During development, we tested the app in Windows 10/11 and Ubuntu 24.04 and used a <a href="https://hub.docker.com/_/postgres" target="_blank" rel="noopener noreferrer">Docker Image</a> for Postgres.\
To install the `dotnet ef` tool, **once .Net 9 is installed**, you can simply execute: `dotnet tool install --global dotnet-ef`. This will install it globally, otherwise you can decide to install it locally.\
All other dependencies (.NET packages) should be installed automatically just building the application. You can read the .csproj files in each project to know exactly all packages included (<a href="BadReview.Api/BadReview.Api.csproj" target="_blank" rel="noopener noreferrer">Api</a>, <a href="BadReview.Client/BadReview.Client.csproj" target="_blank" rel="noopener noreferrer">Client</a>, <a href="BadReview.Shared/BadReview.Shared.csproj" target="_blank" rel="noopener noreferrer">Shared</a>).\
Finally, in order to make requests to the **IGDB API** you will need to get the necessary credentials to do so. Simply follow the instructions in the following link: <a href="https://api-docs.igdb.com/#account-creation" target="_blank" rel="noopener noreferrer">https://api-docs.igdb.com/#account-creation</a>

## Usage

You can use the already built application (currently v1.0 release) through the <a href="https://badreview.tech/" target="_blank" rel="noopener noreferrer">website</a>.

---

If you intend to run the app locally:

Ensure the requirements are met. Then, clone the <a href="https://github.com/BadReview-Org/BadReview.git" target="_blank" rel="noopener noreferrer">repository</a> and enter the directory:

### PostgreSQL
If you are using a Docker image you can run: 

``` bash
# To create a new container "postgres-badreview"
# with "postgres" username and "pass" password
sudo docker run --name postgres-badreview -e POSTGRES_PASSWORD=pass -p 5432:5432 -v postgres_data:/var/lib/postgresql/data -d postgres:16`

# Once the docker container is created
sudo docker stop postgres-badreview # stop
sudo docker restart postgres-badreview # restart
```

### API

Enter the **BadReview.Api folder**.

#### appsettings.json

You will need to create a new **appsettings.json** file inside the BadReview.Api folder.\
The content of this new file must be:

```
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=BadReviewDb;Username=postgres;Password=pass;Port=5432"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "IGDB": {
    "ClientId": *IGDB CLIENT ID*,
    "ClientSecret": *IGDB CLIENT SECRET*,
    "TokenURI": "https://id.twitch.tv/oauth2/token",
    "URI": "https://api.igdb.com/v4/"
  },
  "Jwt": {
    "Key": *JWT SECURITY KEY*,
    "Issuer": "https://localhost:5003"
  }
}
```

Where \*IGDB CLIENT ID\* and \*IGDB CLIENT SECRET\* are your own IGDB credentials, and \*JWT SECURITY KEY\* is a valid security key used for the JWT's signature validation, e.g.\
`cngiINE+O6E4toJMd7A1CN97Ct77AsWubHFFx57Rkbkclkmozecx+lyZGuB+Nyz6`\
(randomly generated for local development purposes). You can pick another one or use **openssl** to generate a new one.

#### Database initialization

First, you will need to execute the migrations (**BadReview.Api/Migrations** folder). To do so, run: `dotnet ef database update`\
Only in case there's an error, you could completely delete the Migrations folder, create and execute a new one:

``` bash
rm -rf Migrations # Delete migrations folder
dotnet ef migrations add InitialCreate # Create a new migration
dotnet ef database drop --force # Drop the current database if necessary
dotnet ef database update # Run the migration
```

Once the database is initialized, you can just run `dotnet run` command to start the API. This will leave the server running and listening requests at the ports specified in the **Properties/launchSettings.json** file (**5003 by default**).\
You can now use <a href="https://www.postman.com/" target="_blank" rel="noopener noreferrer">Postman</a> or any other client to test the API endpoints, or launch the **BadReview.Client** app to do so.

### Client

Enter the **BadReview.Client** folder.\
Simply execute the `dotnet run` command to launch the client app. You can now access the SPA at the port specified in **Properties/launchSettings.json** file (**5193 by default**).