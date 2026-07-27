# FitForge 40-Day Roadmap

## How to Use This Roadmap

This roadmap is designed for learning by building.

Each day should follow this workflow:

1. Read the day goal.
2. Learn the main concept before coding.
3. Create a plan.
4. Implement only that day's scope.
5. Test the result.
6. Review changed files.
7. Commit the work.

Claude should not write code immediately.

Claude should first explain the concept, then create an implementation plan, then wait for approval.

---

# Week 1 - Backend Foundation

## Day 1 - Solution and Repository Setup

### Goal

Create the initial backend solution structure and make sure the API runs successfully.

### Concepts to Teach First

* What is a .NET solution?
* What is a .NET project?
* Why do we split Api, Application, Domain and Infrastructure?
* What is Clean Architecture in simple terms?
* Why controllers should not contain business logic?

### Implementation Tasks

* Create the backend solution.
* Create these projects:

  * FitForge.Api
  * FitForge.Application
  * FitForge.Domain
  * FitForge.Infrastructure
  * FitForge.Tests.Unit
  * FitForge.Tests.Integration
* Add correct project references.
* Add a simple health endpoint.
* Run the API locally.
* Make sure `dotnet build` works.

### Done Criteria

* The solution builds successfully.
* The API starts locally.
* Health endpoint returns OK.
* Project references are correct.
* Initial commit is created.

### Suggested Commit Message

chore: initialize backend solution structure

---

## Day 2 - Docker Compose with SQL Server and Redis

### Goal

Run SQL Server and Redis locally using Docker Compose.

### Concepts to Teach First

* What is Docker?
* What is Docker Compose?
* Why do production projects use containers?
* Why are SQL Server and Redis separated from the API?
* What is the difference between local app and infrastructure services?

### Implementation Tasks

* Add `docker-compose.yml`.
* Add SQL Server service.
* Add Redis service.
* Add environment variables for passwords and ports.
* Start containers.
* Verify SQL Server is reachable.
* Verify Redis is reachable.
* Document Docker commands.

### Done Criteria

* SQL Server container runs.
* Redis container runs.
* Containers can be started with one command.
* Docker commands are documented.

### Suggested Commit Message

chore: add docker compose for sql server and redis

---

## Day 3 - Entity Framework Core and First Migration

### Goal

Connect the API to SQL Server using Entity Framework Core and create the first migration.

### Concepts to Teach First

* What is an ORM?
* What is Entity Framework Core?
* What is DbContext?
* What is a migration?
* Why do we use migrations instead of manually creating tables?

### Implementation Tasks

* Add EF Core packages.
* Create `FitForgeDbContext`.
* Configure SQL Server connection.
* Add configuration to appsettings.
* Register DbContext in dependency injection.
* Create a simple test entity if needed.
* Create first migration.
* Apply migration to database.

### Done Criteria

* API connects to SQL Server.
* Migration is created.
* Database is created or updated.
* `dotnet ef database update` works.

### Suggested Commit Message

feat: configure ef core with sql server

---

## Day 4 - Base Entity, Auditable Entity and Soft Delete Foundation

### Goal

Create reusable base entity classes for consistent database design.

### Concepts to Teach First

* What is a base entity?
* Why do most production tables have Id, CreatedAt and UpdatedAt?
* What is audit information?
* What is soft delete?
* When should soft delete be used and when should it not be used?

### Implementation Tasks

* Create `BaseEntity`.
* Create `AuditableEntity`.
* Add CreatedAt and UpdatedAt fields.
* Optionally add CreatedBy and UpdatedBy.
* Add soft delete fields if needed.
* Configure EF Core conventions if appropriate.
* Prepare infrastructure for future entities.

### Done Criteria

* Base entity classes exist.
* Future entities can inherit from them.
* Code builds successfully.

### Suggested Commit Message

feat: add base entity and audit fields

---

## Day 5 - Global Exception Handling and Standard API Response

### Goal

Create a consistent error handling system for the API.

### Concepts to Teach First

* What is exception handling?
* Why should APIs not return raw exceptions?
* What is middleware?
* What is a standard error response?
* Why is error handling important in production?

### Implementation Tasks

* Create custom exception types if needed.
* Create global exception handling middleware.
* Create standard error response model.
* Handle validation, not found, unauthorized and unexpected errors.
* Make sure stack traces are not returned in production.
* Test by throwing a sample exception.

### Done Criteria

* API returns consistent JSON error responses.
* Unexpected errors are handled safely.
* No raw stack trace is exposed.
* Middleware is registered correctly.

### Suggested Commit Message

feat: add global exception handling

---

# Week 2 - Authentication and Authorization

## Day 6 - ASP.NET Core Identity Setup

### Goal

Set up ASP.NET Core Identity for user management.

### Concepts to Teach First

* What is authentication?
* What is ASP.NET Core Identity?
* Why should we not build password handling manually?
* What is password hashing?
* What is the difference between user, role and claim?

### Implementation Tasks

* Add ASP.NET Core Identity packages.
* Create application user entity.
* Configure Identity with EF Core.
* Add Identity tables migration.
* Seed initial roles:

  * User
  * Coach
  * Admin
* Create register endpoint.

### Done Criteria

* Identity tables are created.
* User registration works.
* Passwords are hashed by Identity.
* Roles are seeded.

### Suggested Commit Message

feat: configure aspnet identity

---

## Day 7 - Login and JWT Access Token

### Goal

Implement login and return a JWT access token.

### Concepts to Teach First

* What is JWT?
* What is an access token?
* What does stateless authentication mean?
* What should be stored inside a token?
* Why should access tokens expire?

### Implementation Tasks

* Create login request DTO.
* Create login response DTO.
* Validate login request.
* Check user credentials with Identity.
* Generate JWT access token.
* Add JWT authentication configuration.
* Protect a test endpoint with `[Authorize]`.

### Done Criteria

* User can login.
* API returns JWT access token.
* Protected endpoint works with valid token.
* Protected endpoint rejects missing or invalid token.

### Suggested Commit Message

feat: add jwt login authentication

---

## Day 8 - Refresh Token Rotation

### Goal

Implement refresh tokens so users can get new access tokens securely.

### Concepts to Teach First

* What is a refresh token?
* Why do we need refresh tokens?
* What is refresh token rotation?
* What happens if a refresh token is stolen?
* Why should refresh tokens be stored in the database?

### Implementation Tasks

* Create RefreshToken entity.
* Add refresh token table migration.
* Generate refresh token during login.
* Create refresh endpoint.
* Rotate refresh token after each use.
* Revoke old refresh token.
* Detect refresh token reuse if possible.

### Done Criteria

* Login returns access token and refresh token.
* Refresh endpoint returns new access token.
* Old refresh token becomes invalid after use.
* Invalid refresh token is rejected.

### Suggested Commit Message

feat: implement refresh token rotation

---

## Day 9 - Logout and Token Revocation

### Goal

Allow users to securely logout by revoking refresh tokens.

### Concepts to Teach First

* Why JWT logout is not as simple as deleting a session?
* What does token revocation mean?
* Why do we revoke refresh tokens instead of access tokens?
* What is the difference between logout from one device and all devices?

### Implementation Tasks

* Create logout endpoint.
* Revoke current refresh token.
* Add revoke all sessions endpoint if appropriate.
* Add database fields for revocation date and reason.
* Test login, refresh and logout flow.

### Done Criteria

* User can logout.
* Revoked refresh token cannot be used again.
* Token flow is documented.

### Suggested Commit Message

feat: add logout and refresh token revocation

---

## Day 10 - Role-Based and Policy-Based Authorization

### Goal

Protect endpoints using roles and policies.

### Concepts to Teach First

* What is authorization?
* Difference between authentication and authorization.
* What is role-based authorization?
* What is policy-based authorization?
* When should we use roles and when should we use policies?

### Implementation Tasks

* Seed User, Coach and Admin roles.
* Add role claim to JWT.
* Create admin-only endpoint.
* Create user-only protected endpoint.
* Add example authorization policy.
* Test access with different roles.

### Done Criteria

* Admin endpoints require Admin role.
* Normal users cannot access admin endpoints.
* JWT contains role information.
* Authorization is tested manually.

### Suggested Commit Message

feat: add role and policy authorization

---

# Week 3 - User Profile and Exercise Library

## Day 11 - User Profile Module

### Goal

Allow users to create and update their fitness profile.

### Concepts to Teach First

* What is a DTO?
* Why should we not expose entities directly?
* What is one-to-one relationship?
* Why does user profile belong to the authenticated user?

### Implementation Tasks

* Create UserProfile entity.
* Create profile request and response DTOs.
* Add profile migration.
* Create create/update profile endpoint.
* Create get my profile endpoint.
* Make endpoints require authentication.
* Ensure user can only access their own profile.

### Done Criteria

* Authenticated user can create profile.
* Authenticated user can update profile.
* User cannot access another user's profile.
* DTOs are used instead of exposing entity.

### Suggested Commit Message

feat: add user profile module

---

## Day 12 - Goals, Equipment and Injury Modeling

### Goal

Model the information needed to personalize workout plans.

### Concepts to Teach First

* What is an enum?
* What is a value object?
* When should we use enums?
* How do user goals affect plan generation?
* Why injury and equipment data matter?

### Implementation Tasks

* Add goal type model.
* Add equipment model.
* Add injury or limitation model.
* Update profile DTOs.
* Add validation rules.
* Update database migration if needed.

### Done Criteria

* User profile includes goal information.
* User profile includes equipment information.
* User profile includes limitation information.
* Validation prevents invalid profile data.

### Suggested Commit Message

feat: extend profile with goals equipment and limitations

---

## Day 13 - Exercise Library Entity Model

### Goal

Create the database model for the exercise library.

### Concepts to Teach First

* What is many-to-many relationship?
* What is one-to-many relationship?
* How should exercises be categorized?
* Why exercise data should be structured?

### Implementation Tasks

* Create Exercise entity.
* Create MuscleGroup entity or enum.
* Create Equipment entity or enum.
* Add difficulty level.
* Add movement pattern.
* Add optional media fields.
* Add migration.
* Seed initial exercises.

### Done Criteria

* Exercise tables are created.
* Initial exercises are seeded.
* Exercise entity supports filtering later.

### Suggested Commit Message

feat: add exercise library model

---

## Day 14 - Admin Exercise CRUD

### Goal

Allow admin users to manage exercises.

### Concepts to Teach First

* What is CRUD?
* Why admin-only write access matters?
* Why normal users should not create system exercises?
* Why validation is important for admin data too?

### Implementation Tasks

* Create exercise create/update DTOs.
* Create exercise response DTO.
* Add FluentValidation validators.
* Add admin-only create endpoint.
* Add admin-only update endpoint.
* Add admin-only delete endpoint.
* Add public or authenticated read endpoint.

### Done Criteria

* Admin can create exercises.
* Admin can update exercises.
* Admin can delete exercises.
* Normal user cannot modify exercises.
* Validation works.

### Suggested Commit Message

feat: add admin exercise crud

---

## Day 15 - Exercise Filtering, Pagination and Sorting

### Goal

Create production-quality list endpoints for exercises.

### Concepts to Teach First

* What is pagination?
* Why should APIs not return all records at once?
* What is filtering?
* What is sorting?
* What is projection in EF Core?
* Why should we avoid unnecessary Include?

### Implementation Tasks

* Add exercise query parameters.
* Support pagination.
* Support filtering by muscle group, equipment and difficulty.
* Support sorting by name or difficulty.
* Return paged response metadata.
* Use projection to DTO.
* Add basic tests if appropriate.

### Done Criteria

* Exercise list supports pagination.
* Exercise list supports filters.
* Response includes total count and page info.
* Query is efficient.

### Suggested Commit Message

feat: add exercise filtering and pagination

---

# Week 4 - Workout Plan Generator

## Day 16 - Workout Plan Data Model

### Goal

Create the entities required to store generated workout plans.

### Concepts to Teach First

* What is an aggregate?
* How should workout plans, days and items relate?
* Why plan data should be stored instead of regenerated every time?
* What is the difference between plan template and generated plan?

### Implementation Tasks

* Create WorkoutPlan entity.
* Create WorkoutDay entity.
* Create WorkoutItem entity.
* Create WorkoutSetPrescription entity if needed.
* Add relationships.
* Add migration.
* Add basic read endpoint.

### Done Criteria

* Workout plan tables exist.
* Workout plan can contain days.
* Workout day can contain exercises.
* Code builds successfully.

### Suggested Commit Message

feat: add workout plan model

---

## Day 17 - Plan Generator V1

### Goal

Generate a simple weekly workout plan based on user goal and weekly availability.

### Concepts to Teach First

* What is business logic?
* Why plan generation should not be inside controller?
* What is a service?
* How do we keep business rules testable?

### Implementation Tasks

* Create plan generator service.
* Read user profile.
* Decide weekly training split.
* Generate basic plan structure.
* Save generated plan to database.
* Add endpoint: generate my plan.

### Done Criteria

* User can generate a weekly plan.
* Plan is based on goal and available days.
* Logic is not inside controller.
* Plan is saved to database.

### Suggested Commit Message

feat: add basic workout plan generator

---

## Day 18 - Equipment-Based Exercise Selection

### Goal

Make plan generation select exercises based on available equipment.

### Concepts to Teach First

* Why personalization matters?
* How equipment affects exercise selection?
* What is rule-based selection?
* How to avoid hardcoding too much?

### Implementation Tasks

* Update exercise model if needed.
* Add equipment filtering in plan generator.
* Select only suitable exercises.
* Add fallback alternatives.
* Test with different user equipment profiles.

### Done Criteria

* Home user receives home-compatible exercises.
* Gym user receives gym-compatible exercises.
* Generator has fallback behavior.
* Equipment rule is understandable.

### Suggested Commit Message

feat: generate plans based on equipment

---

## Day 19 - Injury and Limitation Filtering

### Goal

Prevent risky exercises from being selected for users with limitations.

### Concepts to Teach First

* Why safety rules matter in fitness apps?
* How should limitations affect recommendations?
* What is a contraindication?
* Why should rule-based filtering be explicit?

### Implementation Tasks

* Add exercise risk tags or limitation tags.
* Add profile limitation data if needed.
* Update generator to avoid risky exercises.
* Add simple shoulder limitation example.
* Add tests for injury filtering.

### Done Criteria

* Users with shoulder limitation avoid risky exercises.
* Generator still provides alternatives.
* Safety rule is tested.

### Suggested Commit Message

feat: add limitation-aware plan generation

---

## Day 20 - Unit Tests for Plan Generator

### Goal

Write unit tests for the most important plan generation rules.

### Concepts to Teach First

* What is unit testing?
* What should be unit tested?
* Why business logic should be testable?
* What is Arrange, Act, Assert?
* Why tests matter in production projects?

### Implementation Tasks

* Add xUnit test project setup if not already done.
* Add FluentAssertions.
* Test goal-based split.
* Test equipment-based selection.
* Test limitation filtering.
* Test invalid profile scenarios.

### Done Criteria

* Plan generator has unit tests.
* Tests are readable.
* Tests can be run with `dotnet test`.
* Important rules are covered.

### Suggested Commit Message

test: add workout plan generator unit tests

---

# Week 5 - Workout and Running Logs

## Day 21 - Workout Logging Model and Endpoint

### Goal

Allow users to log completed workouts.

### Concepts to Teach First

* What is a log table?
* Difference between planned workout and completed workout.
* Why logs should be connected to users?
* How to model completed sessions?

### Implementation Tasks

* Create WorkoutLog entity.
* Create WorkoutExerciseLog entity if needed.
* Add create workout log endpoint.
* Add get my workout logs endpoint.
* Add validation.
* Add migration.

### Done Criteria

* User can log a workout.
* User can list own workout logs.
* User cannot access another user's logs.

### Suggested Commit Message

feat: add workout logging

---

## Day 22 - Set-Level Workout Logging

### Goal

Allow users to log sets, reps, weight and RPE.

### Concepts to Teach First

* Why set-level data matters?
* What is RPE?
* How strength progression is tracked?
* Why nested data needs careful validation?

### Implementation Tasks

* Create WorkoutSetLog entity.
* Update workout log DTOs.
* Support multiple sets per exercise.
* Validate reps, weight and RPE.
* Return detailed workout log response.

### Done Criteria

* User can log set-level data.
* Invalid set data is rejected.
* API returns workout details cleanly.

### Suggested Commit Message

feat: add set level workout logging

---

## Day 23 - Running Log Module

### Goal

Allow users to log running sessions.

### Concepts to Teach First

* How running data differs from strength data.
* What is pace?
* What is running type?
* Why duration and distance should be validated?

### Implementation Tasks

* Create RunLog entity.
* Create RunLog request and response DTOs.
* Add create run log endpoint.
* Add get my run logs endpoint.
* Add pace calculation.
* Add validation.

### Done Criteria

* User can create running log.
* Pace is calculated.
* User can list own run logs.
* Invalid distance or duration is rejected.

### Suggested Commit Message

feat: add running log module

---

## Day 24 - Weekly Summary Query

### Goal

Create a weekly summary of user activity.

### Concepts to Teach First

* What is aggregation?
* What is a summary endpoint?
* Why dashboard queries are different from CRUD?
* How to calculate weekly totals?

### Implementation Tasks

* Create weekly summary endpoint.
* Calculate workout count.
* Calculate run count.
* Calculate total running distance.
* Calculate total strength sessions.
* Calculate average RPE if available.
* Use efficient EF Core queries.

### Done Criteria

* User can see weekly summary.
* Summary only includes user's own data.
* Query is reasonably efficient.

### Suggested Commit Message

feat: add weekly activity summary

---

## Day 25 - Dashboard API

### Goal

Create dashboard endpoints for progress visualization.

### Concepts to Teach First

* What makes a dashboard endpoint production-friendly?
* Why dashboards need optimized queries?
* What is projection?
* What data should frontend charts receive?

### Implementation Tasks

* Add dashboard endpoint.
* Return recent workouts.
* Return recent runs.
* Return weekly volume.
* Return consistency data.
* Return chart-friendly response models.

### Done Criteria

* Dashboard API returns useful data.
* Response is frontend-friendly.
* Query does not expose unnecessary entity data.

### Suggested Commit Message

feat: add progress dashboard api

---

# Week 6 - Production Quality

## Day 26 - Serilog Structured Logging

### Goal

Add structured logging to the backend.

### Concepts to Teach First

* What is logging?
* What is structured logging?
* Why Console.WriteLine is not enough?
* What should and should not be logged?
* Why logging matters in production?

### Implementation Tasks

* Add Serilog packages.
* Configure console logging.
* Configure file logging if appropriate.
* Add request logging.
* Add log enrichment.
* Avoid logging sensitive data.

### Done Criteria

* API uses Serilog.
* Requests are logged.
* Errors are logged.
* Sensitive data is not logged.

### Suggested Commit Message

feat: add serilog structured logging

---

## Day 27 - Audit Logging

### Goal

Track important user and admin actions.

### Concepts to Teach First

* What is audit logging?
* Difference between normal logs and audit logs.
* Why admin actions should be traceable?
* What information should audit logs contain?

### Implementation Tasks

* Create AuditLog entity.
* Add audit service.
* Log important actions:

  * Profile update
  * Exercise create/update/delete
  * Plan generation
  * Login/logout if appropriate
* Add admin endpoint to view audit logs.

### Done Criteria

* Important actions create audit records.
* Admin can view audit logs.
* Audit logs include actor, action and timestamp.

### Suggested Commit Message

feat: add audit logging

---

## Day 28 - Redis Caching

### Goal

Use Redis to cache frequently accessed data.

### Concepts to Teach First

* What is caching?
* What is Redis?
* What data should be cached?
* What data should not be cached?
* What is cache invalidation?

### Implementation Tasks

* Add Redis connection.
* Create cache service abstraction.
* Cache exercise list or exercise filters.
* Invalidate cache when admin updates exercises.
* Add configuration for cache duration.

### Done Criteria

* Redis is used by the API.
* Exercise data can be cached.
* Cache is invalidated after changes.
* App still works if cache is empty.

### Suggested Commit Message

feat: add redis caching for exercise queries

---

## Day 29 - Rate Limiting

### Goal

Protect sensitive endpoints from abuse.

### Concepts to Teach First

* What is rate limiting?
* Why login endpoints need protection?
* What is brute force attack?
* What is IP-based limiting?
* What is user-based limiting?

### Implementation Tasks

* Configure ASP.NET Core rate limiting.
* Apply rate limit to login endpoint.
* Apply general rate limit to API if appropriate.
* Test repeated requests.
* Return proper error response.

### Done Criteria

* Login endpoint is rate limited.
* Too many requests return expected response.
* Normal usage is not blocked.

### Suggested Commit Message

feat: add api rate limiting

---

## Day 30 - Health Checks and Swagger Polish

### Goal

Improve operational readiness and API documentation.

### Concepts to Teach First

* What is a health check?
* Why production systems need health endpoints?
* What should Swagger show?
* Why API documentation matters?

### Implementation Tasks

* Add health checks for API.
* Add health check for SQL Server.
* Add health check for Redis.
* Improve Swagger metadata.
* Add JWT support to Swagger.
* Document important endpoints.

### Done Criteria

* Health endpoint works.
* Swagger supports JWT testing.
* API docs are easier to understand.

### Suggested Commit Message

feat: add health checks and improve swagger docs

---

# Week 7 - Testing, CI/CD and Deployment Preparation

## Day 31 - Expand Unit Tests

### Goal

Add more unit tests for important application logic.

### Concepts to Teach First

* Difference between unit test and integration test.
* What should not be unit tested?
* What makes a good test?
* How tests protect refactoring?

### Implementation Tasks

* Review important business rules.
* Add tests for validators.
* Add tests for plan generator.
* Add tests for summary calculations if possible.
* Make test names clear.

### Done Criteria

* Important logic has unit tests.
* Tests are readable.
* `dotnet test` passes.

### Suggested Commit Message

test: expand application unit tests

---

## Day 32 - Integration Test Setup

### Goal

Create integration tests for real API behavior.

### Concepts to Teach First

* What is integration testing?
* Why API tests are different from service tests?
* What is WebApplicationFactory?
* Why integration tests matter for auth and database flows?

### Implementation Tasks

* Configure integration test project.
* Add WebApplicationFactory.
* Add test app configuration.
* Write test for register endpoint.
* Write test for login endpoint.
* Write test for protected endpoint.

### Done Criteria

* Integration tests run.
* Auth flow is tested.
* Tests are repeatable.

### Suggested Commit Message

test: add api integration test setup

---

## Day 33 - Testcontainers

### Goal

Use real SQL Server or required infrastructure in tests.

### Concepts to Teach First

* What is Testcontainers?
* Why use real containers in integration tests?
* Difference between in-memory database and real database.
* Why production-like tests catch more bugs?

### Implementation Tasks

* Add Testcontainers package.
* Start SQL Server container during tests.
* Apply migrations in test setup.
* Run integration tests against real database.
* Add cleanup strategy.

### Done Criteria

* Integration tests use containerized database.
* Tests run reliably.
* Test data is isolated.

### Suggested Commit Message

test: use testcontainers for integration tests

---

## Day 34 - GitHub Actions CI

### Goal

Run build and tests automatically on GitHub.

### Concepts to Teach First

* What is CI?
* What is GitHub Actions?
* Why should tests run on every push?
* What is a workflow file?

### Implementation Tasks

* Add GitHub Actions workflow.
* Restore dependencies.
* Build solution.
* Run tests.
* Cache dependencies if useful.
* Make workflow run on push and pull request.

### Done Criteria

* GitHub Actions workflow exists.
* Build runs automatically.
* Tests run automatically.
* Failed tests fail the workflow.

### Suggested Commit Message

ci: add build and test workflow

---

## Day 35 - Dockerfile and Production Compose

### Goal

Prepare the API for containerized deployment.

### Concepts to Teach First

* What is a Dockerfile?
* Difference between development and production Docker setup.
* What is multi-stage build?
* Why containers help deployment?

### Implementation Tasks

* Add Dockerfile for API.
* Use multi-stage build.
* Add production-style docker compose.
* Pass configuration through environment variables.
* Run API in container.
* Verify API can connect to SQL Server and Redis.

### Done Criteria

* API can be built as Docker image.
* API can run in container.
* Environment variables are used.
* Docker setup is documented.

### Suggested Commit Message

chore: add api dockerfile and production compose

---

# Week 8 - Frontend Foundation

## Day 36 - React TypeScript Project Setup

### Goal

Create the frontend app using React, TypeScript and Vite.

### Concepts to Teach First

* What is React?
* What is TypeScript?
* What is Vite?
* What is a component?
* How frontend talks to backend?

### Implementation Tasks

* Create frontend project with Vite.
* Add TypeScript.
* Add folder structure.
* Add React Router.
* Add Tailwind CSS.
* Create basic layout.
* Add environment variable for API URL.

### Done Criteria

* Frontend runs locally.
* Basic routing works.
* Tailwind works.
* Project structure is clean.

### Suggested Commit Message

feat: initialize react frontend

---

## Day 37 - Login and Register Screens

### Goal

Build authentication screens and connect them to the backend.

### Concepts to Teach First

* What is a form in React?
* What is controlled input?
* What is React Hook Form?
* What is Zod validation?
* How frontend sends login request?

### Implementation Tasks

* Create login page.
* Create register page.
* Add React Hook Form.
* Add Zod validation.
* Connect login to backend.
* Connect register to backend.
* Show loading and error states.

### Done Criteria

* User can register from frontend.
* User can login from frontend.
* Form validation works.
* API errors are displayed.

### Suggested Commit Message

feat: add auth screens

---

## Day 38 - Protected Routes and Token Handling

### Goal

Protect frontend pages that require authentication.

### Concepts to Teach First

* What is a protected route?
* Where should access tokens be stored?
* What is auth state?
* How does frontend attach token to API requests?
* Why token handling must be careful?

### Implementation Tasks

* Create auth state.
* Store token safely for development version.
* Add API client with Authorization header.
* Add protected route component.
* Redirect unauthenticated users to login.
* Add logout button.

### Done Criteria

* Dashboard is protected.
* Logged-out user cannot access private pages.
* Token is sent to backend.
* Logout works.

### Suggested Commit Message

feat: add protected routes and auth state

---

## Day 39 - Profile Onboarding Wizard

### Goal

Create a multi-step onboarding form for user fitness profile.

### Concepts to Teach First

* What is multi-step form?
* How should complex forms be structured?
* Why onboarding matters for personalization?
* How frontend validation and backend validation work together?

### Implementation Tasks

* Create onboarding route.
* Step 1: basic body info.
* Step 2: goal and experience.
* Step 3: equipment.
* Step 4: limitations.
* Submit profile to backend.
* Redirect to dashboard after completion.

### Done Criteria

* User can complete onboarding.
* Profile is saved through API.
* Form is user-friendly.
* Validation works.

### Suggested Commit Message

feat: add profile onboarding wizard

---

## Day 40 - First Dashboard UI

### Goal

Create the first usable dashboard screen.

### Concepts to Teach First

* What is dashboard UI?
* How does frontend consume API data?
* What is TanStack Query?
* How do loading, error and empty states work?
* What data should be shown first?

### Implementation Tasks

* Add TanStack Query.
* Fetch dashboard API.
* Show weekly summary.
* Show recent workouts.
* Show recent runs.
* Add simple charts with Recharts.
* Add loading, error and empty states.

### Done Criteria

* Dashboard loads real backend data.
* User sees useful progress summary.
* API loading/error states are handled.
* First end-to-end user flow works.

### Suggested Commit Message

feat: add initial dashboard

---

# After Day 40 - Next Milestones

After the first 40 days, continue with these improvements:

## Frontend Polish

* Responsive design
* Dark mode
* Better layout
* Better dashboard charts
* Better empty states
* Toast notifications
* Skeleton loaders

## Admin Panel

* Exercise management UI
* User management UI
* Audit log UI
* Role management UI

## Advanced Backend

* Background jobs
* Email confirmation
* Forgot password
* File uploads
* Notification system
* More advanced plan generator
* AI coaching integration

## Deployment

* Deploy backend
* Deploy frontend
* Configure production database
* Configure environment variables
* Add monitoring
* Add proper README
* Record demo video

## Portfolio Preparation

* Write GitHub README
* Add architecture diagram
* Add screenshots
* Add API documentation
* Add demo user credentials
* Add CV bullet points
* Add LinkedIn project post
