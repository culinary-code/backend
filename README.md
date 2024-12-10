# Culinary Code

Recipe app using ChatGPT to generate recipes.

## Table of Contents

- [Features](#features)
- [Technologies Used](#technologies-used)
- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
- [Authentication](#authentication)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [Contributing](#contributing)
- [License](#license)

## Features

- Feature 1
- Feature 2
- Feature 3

## Technologies Used

- [.NET Core](https://dotnet.microsoft.com/) (8.0)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [PostgreSQL](https://www.postgresql.org/)
- [Swagger](https://swagger.io/) for API documentation
- [AutoMapper](https://automapper.org/) for object mapping
- [OpenAI GPT-4o mini](https://openai.com/index/gpt-4o-mini-advancing-cost-efficient-intelligence/) for recipe
  generation
- [OpenAI DALL-E](https://openai.com/index/dall-e/) for recipe image generation
- [Docker](https://www.docker.com/) for containerization
- [GitHub Actions](https://github.com/features/actions) for CI/CD

Visit the project wiki for more in-depth information about the technologies used.

## Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (8.0)
- [PostgreSQL](https://www.postgresql.org/download/) (15)
- A code editor
  like [Visual Studio](https://visualstudio.microsoft.com/), [Visual Studio Code](https://code.visualstudio.com/)
  or [Rider](https://www.jetbrains.com/rider/)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/culinary-code/backend.git
   ```

2. Navigate to the project directory:
   ```bash
   cd backend
   ```

3. Restore the NuGet packages:
   ```bash
   dotnet restore
   ```

4. Run database migrations:
   ```bash
   dotnet ef database update
   ```

## API Endpoints

- `GET /api/endpoint1` - Description of endpoint 1
- `POST /api/endpoint2` - Description of endpoint 2
- `PUT /api/endpoint3` - Description of endpoint 3
- `DELETE /api/endpoint4` - Description of endpoint 4

For detailed information about the API, refer to the [Swagger documentation](http://localhost:5000/swagger) after
running the application.

## Authentication

- Describe the authentication mechanism (e.g., JWT, API keys).
- Include any necessary steps to obtain tokens or credentials.

## Database Setup

- When starting up the application, the database will be created and seeded with initial data by Entity Framework Core.

## Running the Application

To start the application, run:

Set all the necessary environment variables using a method of your choice (e.g., environment variables in CLI, secrets
manager, Rider run config).

Here is an example of environment variables for local development in the CLI (bash):

```bash
export ASPNETCORE_ENVIRONMENT=Development;                         # Development, Staging, Production
export ASPNETCORE_HTTPS_PORT=443;   
export ASPNETCORE_URLS=https://0.0.0.0:7098\;http://0.0.0.0:5114;
export AzureOpenAI__ApiKey={YOUR_API_KEY};                         # Azure OpenAI Service API key
export AzureOpenAI__Endpoint={YOUR_ENDPOINT};                      # Azure OpenAI Service endpoint  
export AzureStorage__ConnectionString={YOUR_CONNECTION_STRING};    # Azure Blog Storage connection string for saving recipe images
export AzureStorage__ContainerName={YOUR_CONTAINER_NAME};          # Azure Blog Storage container name for saving recipe images
export Database__ConnectionString={YOUR_CONNECTION_STRING};        # PostgreSQL connection string
export Keycloak__AdminPassword={YOUR_PASSWORD};                    # Keycloak admin password
export Keycloak__AdminUsername={YOUR_USERNAME};                    # Keycloak admin username (only used for local development)
export Keycloak__BaseUrl=http://localhost:8180;                    # Keycloak base URL
export Keycloak__ClientId={YOUR_CLIENT_ID};                        # Keycloak client ID
export Keycloak__FrontendUrl=http://localhost:8180;                # Keycloak frontend URL in case the issuer URL is different than the base URL
export Keycloak__Realm={YOUR_REALM};                               # Keycloak realm
export LocalLlmServer__ServerUrl=http://localhost:4891;            # Local LLM server URL
export RecipeJob__CronSchedule=0 0 2 * * ?;                        # Cron schedule for recipe generation job
export RecipeJob__MinAmount=5                                      # Minimum amount of recipes to maintain in the database, if the amount is lower, the recipe generation job will be triggered at the scheduled time
```

```bash
dotnet run --project WEBAPI
```

Access the API at `https://localhost:7098` or `http://localhost:5114`.

Access Swagger documentation at `https://localhost:7098/swagger` or `http://localhost:5114/swagger`.

## Contributing

1. Fork the repository.
2. Create a new branch:
   ```bash
   git checkout -b feature/YourFeature
   ```
3. Commit your changes:
   ```bash
   git commit -m "Add some feature"
   ```
4. Push to the branch:
   ```bash
   git push origin feature/YourFeature
   ```
5. Open a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
