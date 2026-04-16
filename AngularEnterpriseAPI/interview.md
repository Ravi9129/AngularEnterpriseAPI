# AngularEnterpriseAPI — Interview Questions & Answers

This document contains commonly asked backend interview questions for a .NET Web API project (like `AngularEnterpriseAPI`) with deep explanations, real-world scenarios, and code snippets. Each entry includes when, where, why, and how to choose or implement the approach.

---

## 1) Authentication vs Authorization

Q: What is the difference between authentication and authorization?

A: Authentication verifies who a user is. Authorization determines what an authenticated user is allowed to do.

When: Always authenticate before authorizing. Where: In APIs, authentication typically happens at the entry point (middleware or filters). Why: Separating concerns reduces complexity and improves security.

Real-world problem: Mobile client obtains tokens and calls the API. The API must confirm the token is valid (authentication) and that the user has the right role to access a resource (authorization).

Code snippet (JWT validation in middleware):

```csharp
// simplified registration in Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
      options.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = jwt.Issuer,
          ValidAudience = jwt.Audience,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
      };
  });
```

When to use roles vs policies: Use roles for coarse-grained access. Use policies for fine-grained or context-dependent checks.

---

## 2) JWT and Refresh Token Flow

Q: How do JWTs and refresh tokens work together?

A: JWTs are short-lived tokens used to authenticate requests. Refresh tokens are long-lived credentials stored securely (often in DB) used to obtain new JWTs when the JWT expires.

Why: Short-lived JWTs minimize the impact of token leakage. Refresh tokens allow sessions without forcing frequent logins.

Real-world problem: SPA calls API with an access token; when it expires, the SPA uses a refresh token to get a new access token without asking the user to log in.

Best practices: store refresh tokens server-side (DB) or rotate them with each use, mark them revoked when suspicious.

Code snippet (issue JWT + refresh token):

```csharp
// TokenService: create access token
public string CreateAccessToken(User user)
{
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), ... };
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(_settings.Issuer, _settings.Audience, claims, expires: DateTime.UtcNow.AddMinutes(15), signingCredentials: creds);
    return new JwtSecurityTokenHandler().WriteToken(token);
}

// Create and persist refresh token separately (DB entity RefreshToken).
```

---

## 3) Password Reset Flow

Q: How should a password reset work securely?

A: Generate a single-use, time-limited token, store a hashed token (or token id with relation) in DB, send a URL via email with the token, verify token on reset, then remove or mark token used.

Real-world: If tokens are permanent or leaked, accounts get compromised. Use short expiry (e.g., 1 hour) and single-use tokens.

Code snippet (generate and validate):

```csharp
// Generate
var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
var hashed = Hash(token);
db.PasswordResetTokens.Add(new PasswordResetToken { UserId = userId, TokenHash = hashed, ExpiresAt = DateTime.UtcNow.AddHours(1) });
await db.SaveChangesAsync();
// Email link: https://app/reset?token={token}&user={userId}

// Validate
var stored = await db.PasswordResetTokens.SingleOrDefaultAsync(t => t.UserId == userId);
if (stored == null || stored.ExpiresAt < DateTime.UtcNow || !VerifyHash(token, stored.TokenHash))
    throw new InvalidOperationException("Invalid token");
```

---

## 4) Entity Framework Core and Repository Pattern

Q: When to use repository pattern over DbContext directly?

A: EF Core exposes `DbContext` which already behaves like a Unit of Work and `DbSet` resembles repositories. Use a repository abstraction when you need to isolate data access for testing, or provide a custom API surface; avoid unnecessary abstraction.

Real-world problem: You have multiple data sources (SQL + external service). Use repository pattern to encapsulate and mock data access in service tests.

Example: `IRepository<T>` with common CRUD, `UserRepository` for user-specific queries.

EF Core paging sample using LINQ:

```csharp
var query = _dbContext.Users.Where(u => u.IsActive);
var total = await query.CountAsync();
var items = await query.OrderBy(u => u.Email).Skip((page-1)*size).Take(size).ToListAsync();
return new PagedResponse<UserDto>(items, page, size, total);
```

---

## 5) Middleware: Logging, Error Handling, Rate Limiting

Q: Where should cross-cutting concerns live?

A: Use middleware for request-level concerns (logging, error handling, rate limiting, audit) in the HTTP pipeline. Use filters for controller/action-level behaviors.

Real-world problem: Record every request's user and path for audit and throttle abusive clients globally.

Middleware basic structure:

```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    public RequestLoggingMiddleware(RequestDelegate next) => _next = next;
    public async Task Invoke(HttpContext ctx)
    {
        var sw = Stopwatch.StartNew();
        await _next(ctx);
        sw.Stop();
        // persist path, user, status, duration
    }
}
```

Rate limiting strategies: token bucket, leaky bucket, sliding window; choose distributed store (Redis) for multiple instances.

---

## 6) Dependency Injection and Service Lifetimes

Q: Differences between `Transient`, `Scoped`, and `Singleton`?

A: `Transient` creates a new instance every injection. `Scoped` creates one instance per HTTP request. `Singleton` creates one instance for the application lifetime.

When to use `Scoped`: use for `DbContext` so all repos share the same context per request. `Singleton` for stateless caches or configuration. `Transient` for lightweight, short-lived services.

Problem: Registering `DbContext` as `Singleton` leads to concurrency issues and memory leaks.

Registration example:

```csharp
services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(conn));
services.AddScoped<IUserRepository, UserRepository>();
services.AddSingleton<IJwtSettings>(_ => configuration.GetSection("Jwt").Get<JwtSettings>());
```

---

## 7) Async/Await and Performance

Q: Why use async/await in web APIs?

A: `async/await` frees thread pool threads while waiting for I/O, increasing throughput under load. Use async for database calls, HTTP calls, and file I/O.

Pitfalls: avoid using `Task.Run` to make CPU-bound work appear async. Avoid `.Result` and `.Wait()` which block threads and can deadlock.

Example:

```csharp
public async Task<UserDto> GetUserAsync(int id)
{
    var user = await _dbContext.Users.FindAsync(id);
    return _mapper.Map<UserDto>(user);
}
```

---

## 8) Validation and Security

Q: How to validate inputs and protect the API?

A: Use model validation (DataAnnotations), FluentValidation for complex rules, and centralize validation in filters. Sanitize inputs, use parameterized queries (EF Core handles this), always use HTTPS, set up proper CORS policies, and protect secrets with a secret store (Azure Key Vault, environment variables).

Real-world: Prevent account enumeration by returning generic messages on login failures and careful rate limiting on auth endpoints.

Example using FluentValidation:

```csharp
public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
    }
}
```

---

## 9) AutoMapper and DTO Mapping

Q: Why use DTOs and AutoMapper?

A: DTOs separate internal entity models from API contracts (security, shape, versioning). AutoMapper reduces repetitive mapping code.

Example profile:

```csharp
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<User, UserResponseDto>();
        CreateMap<CreateUserDto, User>();
    }
}
```

When to avoid AutoMapper: very complex mappings might be clearer with explicit code.

---

## 10) Pagination, Sorting, and Filtering

Q: How to design scalable pagination?

A: Prefer cursor-based pagination for large datasets (better performance, stable when data changes). Offset pagination (`Skip/Take`) is simple for small/medium datasets.

Real-world: User list in admin UI with page numbers often uses offset pagination; endless scroll favors cursor pagination.

Example offset pagination with total count:

```csharp
var total = await usersQuery.CountAsync();
var items = await usersQuery.OrderBy(u => u.Id).Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
return new PagedResponse<UserDto>(items, page, pageSize, total);
```

---

## 11) Unit and Integration Testing

Q: How to test controllers and services?

A: Unit test services with mocked repositories (`Moq`/`NSubstitute`). For integration tests, use `WebApplicationFactory<TEntryPoint>` and an in-memory or test container DB to exercise real pipeline behavior.

Real-world: Test critical business logic (e.g., token rotation) with unit tests and API flows (e.g., password reset) with integration tests.

Example (xUnit + Moq):

```csharp
[Fact]
public async Task CreateUser_CallsRepository()
{
    var repo = new Mock<IUserRepository>();
    var svc = new UserService(repo.Object, mapper, ...);
    await svc.CreateAsync(new CreateUserDto { Email = "a@b.com", Password = "P@ssword1" });
    repo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
}
```

---

## 12) Caching Strategies

Q: When to cache and which store to use?

A: Cache results of expensive, read-heavy operations. Use in-memory cache for single-instance apps and distributed cache (Redis) for multi-instance deployments.

Real-world: Caching product catalogs or feature flags improves performance. Never cache sensitive per-user data in a shared cache without proper keys/isolation.

Example basic memory cache:

```csharp
if (!_cache.TryGetValue(key, out ProductDto product))
{
    product = await _productRepo.GetAsync(id);
    _cache.Set(key, product, TimeSpan.FromMinutes(5));
}
```

---

## 13) Deployment, Docker, and CI/CD

Q: How to containerize and CI your API?

A: Create a `Dockerfile`, build images in CI (GitHub Actions), push to a registry, and deploy to Kubernetes or App Service. Use multi-stage builds to keep images small.

Dockerfile snippet:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "AngularEnterpriseAPI.dll"]
```

CI tip: run `dotnet test` and `dotnet ef database update` carefully (migrations may run in deployment step only).

---

## 14) Performance Profiling and Optimization

Q: Where do you start when the API is slow?

A: Measure (APM, logs, benchmarks). Identify hotspots: database queries (N+1), blocking sync calls, large payloads. Optimize by indexing DB, using projection (`Select` to DTO), caching, and batching queries.

Real-world: A report endpoint that loads related entities one-by-one can be fixed by eager loading with `Include` or a single projection query.

Example fix (N+1 -> projection):

```csharp
// N+1 bad
var users = await db.Users.ToListAsync();
foreach(var u in users) { var roles = await db.UserRoles.Where(ur => ur.UserId == u.Id).ToListAsync(); }

// better: single query projection
var usersWithRoles = await db.Users
    .Select(u => new { u.Id, u.Email, Roles = u.UserRoles.Select(ur => ur.Role.Name) })
    .ToListAsync();
```

---

## 15) SOLID Principles and Design Patterns

Q: How do SOLID principles apply to Web APIs?

A: Use SRP (single responsibility) for classes (e.g., controllers orchestrate, services hold business logic), DI for OCP and DIP, Liskov substitution for substitutable abstractions, and Interface segregation to keep clients small interfaces.

Patterns: use Repository, Unit of Work, Strategy (pluggable auth, email providers), Mediator (CQRS) for complex domains.

Example: Use `IEmailService` abstraction to swap SMTP vs SendGrid implementations without changing consumers.

---

## 16) Observability: Logging, Metrics, Tracing

Q: What should you log and measure?

A: Log structured messages (errors, warnings, important events), expose metrics (request rate, error rate, latency), and use distributed tracing to follow requests across services. Avoid logging sensitive data.

Tools: Serilog, Prometheus, OpenTelemetry.

Example: Add request duration metric and correlated traces using `Activity` and OpenTelemetry.

---

## How to use this document

- Review questions and answers before interviews. Use code snippets as starting points but adapt to your project, coding standards, and .NET version.
- Add organization-specific details (naming, infrastructure) where relevant.

---

If you want, I can expand any specific topic with deeper code examples (unit tests, integration tests, Docker Compose, GitHub Actions workflows) or tailor the content to match your codebase exact classes such as `TokenService`, `PasswordResetService`, or middleware implementations present in this repository.
