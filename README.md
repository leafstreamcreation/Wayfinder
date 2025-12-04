# Wayfinder

.NET Framework 4.8 REST microservice for tracking repetitive tasks.

## Technology Stack

- **Framework**: .NET Framework 4.8
- **Web Framework**: ASP.NET MVC 4 / Web API 2
- **Database**: Oracle Database 12c+ (using Oracle XE 21c container)
- **Authentication**: JWE (JSON Web Encryption) protected JWT tokens
- **Password Hashing**: BCrypt
- **Containerization**: Docker & Docker Compose

## Entities

- **User**: email, color1, color2, color3 (+ password hash)
- **Tag**: name, Task id
- **Task**: title, User id, last finished date, refresh interval, alert threshold percentage, isActive, initial refresh interval
- **TaskTag**: Task id, Tag id (many-to-many relationship)
- **Record**: Task id, finished date, status

## API Endpoints

### Authentication (Public)
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login with email/password

### Users (JWE Protected)
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users/me` - Get current authenticated user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

### Tasks (JWE Protected)
- `GET /api/tasks` - Get all tasks for current user
- `GET /api/tasks/{id}` - Get task by ID
- `POST /api/tasks` - Create a new task
- `PUT /api/tasks/{id}` - Update a task
- `DELETE /api/tasks/{id}` - Delete a task

### Tags (JWE Protected)
- `GET /api/tags` - Get all tags for current user
- `GET /api/tags/{id}` - Get tag by ID
- `GET /api/tags/task/{taskId}` - Get tags by task ID
- `POST /api/tags` - Create a new tag
- `PUT /api/tags/{id}` - Update a tag
- `DELETE /api/tags/{id}` - Delete a tag

### Records (JWE Protected)
- `GET /api/records` - Get all records for current user
- `GET /api/records/{id}` - Get record by ID
- `GET /api/records/task/{taskId}` - Get records by task ID
- `POST /api/records` - Create a new record
- `PUT /api/records/{id}` - Update a record
- `DELETE /api/records/{id}` - Delete a record

### TaskTags (JWE Protected)
- `GET /api/tasktags` - Get all task-tag associations for current user
- `GET /api/tasktags/{id}` - Get task-tag association by ID
- `GET /api/tasktags/task/{taskId}` - Get task-tag associations by task ID
- `GET /api/tasktags/tag/{tagId}` - Get task-tag associations by tag ID
- `POST /api/tasktags` - Create a new task-tag association
- `DELETE /api/tasktags/{id}` - Delete a task-tag association

## Getting Started

### Prerequisites
- Docker and Docker Compose installed
- (Optional) Visual Studio 2019+ for development on Windows

### Running with Docker

1. Clone the repository:
```bash
git clone https://github.com/leafstreamcreation/Wayfinder.git
cd Wayfinder
```

2. Start the services:
```bash
docker-compose up -d
```

3. Wait for Oracle to initialize (may take a few minutes on first run)

4. The API will be available at `http://localhost:8080`

### Configuration

Environment variables (can be set in docker-compose.yml):
- `ORACLE_CONNECTION_STRING` - Oracle database connection string
- `JWT_SECRET_KEY` - Secret key for JWT token signing (min 256 bits)
- `JWE_ENCRYPTION_KEY` - Key for JWE encryption (min 256 bits)
- `JWT_ISSUER` - JWT token issuer
- `JWT_AUDIENCE` - JWT token audience
- `JWT_EXPIRATION_MINUTES` - Token expiration time in minutes

### Example API Usage

#### Register a new user:
```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email": "user@example.com", "password": "password123", "color1": "#FF0000"}'
```

#### Login:
```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "user@example.com", "password": "password123"}'
```

#### Create a task (authenticated):
```bash
curl -X POST http://localhost:8080/api/tasks \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{"title": "My Task", "refreshInterval": 7, "alertThresholdPercentage": 80}'
```

## Project Structure

```
Wayfinder/
├── Wayfinder.sln                    # Solution file
├── Wayfinder.API/                   # Main API project
│   ├── Controllers/                 # API Controllers
│   │   ├── AuthController.cs        # Authentication endpoints
│   │   ├── UsersController.cs       # User management
│   │   ├── TagsController.cs        # Tag management
│   │   ├── TasksController.cs       # Task management
│   │   ├── TaskTagsController.cs    # Task-tag relationship management
│   │   └── RecordsController.cs     # Record management
│   ├── Models/                      # Entity models and DTOs
│   │   ├── User.cs
│   │   ├── Tag.cs
│   │   ├── Task.cs
│   │   ├── TaskTag.cs
│   │   ├── Record.cs
│   │   └── DTOs.cs
│   ├── Services/                    # Business logic and data access
│   │   ├── OracleDbContext.cs       # Database context
│   │   ├── UserRepository.cs
│   │   ├── TagRepository.cs
│   │   ├── TaskRepository.cs
│   │   ├── TaskTagRepository.cs
│   │   ├── RecordRepository.cs
│   │   ├── AuthService.cs
│   │   └── JwtService.cs
│   ├── Filters/                     # Authentication filters
│   │   └── JweAuthenticationFilter.cs
│   ├── App_Start/
│   │   └── WebApiConfig.cs
│   ├── Web.config
│   └── Global.asax
├── scripts/
│   └── init-db.sql                  # Database initialization script
├── docker-compose.yml               # Docker Compose configuration
├── Dockerfile                       # Windows container Dockerfile
├── Dockerfile.mono                  # Linux container Dockerfile (Mono)
└── README.md
```

## Security

- Passwords are hashed using BCrypt
- JWT tokens are encrypted using JWE (AES-256-KW with AES-256-CBC-HMAC-SHA-512)
- All endpoints except authentication are protected with JWE authentication
- Users can only access their own data

## License

MIT License
