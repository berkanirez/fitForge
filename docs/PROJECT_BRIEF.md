# FitForge - Project Brief

## 1. Project Name

**FitForge**

## 2. One-Sentence Description

FitForge is a production-oriented personal hybrid fitness assistant that helps users generate personalized workout plans, log strength/running sessions, track progress, and receive rule-based coaching insights.

## 3. Main Goal

The goal of this project is not to build a simple CRUD application.

The goal is to build a real, market-ready, portfolio-level full-stack application using modern backend and frontend practices.

This project should help me:

* Learn production-level backend development by building
* Improve my C# and ASP.NET Core skills
* Learn frontend development from zero using React and TypeScript
* Create a strong GitHub/CV project
* Understand how senior developers structure real-world applications
* Practice authentication, authorization, security, testing, logging, caching, Docker and CI/CD
* Build a project that can later evolve into a real SaaS or mobile product

## 4. My Background

I am a junior backend developer.

I have experience or basic knowledge in:

* C#
* .NET
* ASP.NET Core basics
* SQL
* Git
* Backend concepts
* KeystoneJS
* GraphQL
* Postman
* Database modeling

My frontend knowledge is beginner level.

I want to learn by building real features, not by only studying theory.

Claude should act as a senior full-stack mentor and pair programmer.

## 5. Product Idea

FitForge is a personal hybrid fitness assistant.

The user creates a profile by entering:

* Height
* Weight
* Age
* Training goal
* Available equipment
* Injury or limitation information
* Weekly training availability
* Current strength level
* Current running level

Then the system helps the user:

* Generate a weekly hybrid training plan
* Log strength workouts
* Log running sessions
* Track progress
* See weekly volume and performance trends
* Receive simple rule-based coaching feedback

## 6. Target User

The target user is someone who wants to train with a hybrid approach:

* Strength training
* Calisthenics
* Running
* Fat loss
* Muscle maintenance
* Athletic performance

The user may train at home or in the gym.

The app should support different equipment options.

Examples:

* Bodyweight only
* Dumbbells
* Barbell
* Pull-up bar
* Full gym

## 7. Core Features

### 7.1 Authentication

The application must support:

* Register
* Login
* Logout
* JWT access token
* Refresh token rotation
* Refresh token revoke
* Role-based authorization
* Policy-based authorization

Roles:

* User
* Coach
* Admin

### 7.2 User Profile

The user can create and update a fitness profile.

Profile fields may include:

* Height
* Weight
* Age
* Gender, optional
* Main goal
* Training experience
* Weekly available training days
* Available equipment
* Injury or limitation notes
* Preferred training style

Example goals:

* Fat loss
* Muscle gain
* Hybrid performance
* Running improvement
* Strength improvement
* General fitness

### 7.3 Exercise Library

The app contains an exercise library.

Each exercise may have:

* Name
* Description
* Muscle group
* Equipment
* Difficulty
* Movement type
* Video URL, optional
* Image URL, optional
* Safety notes
* Alternative exercises

Admin users should be able to create, update and delete exercises.

Normal users should only be able to read exercises.

### 7.4 Workout Plan Generator

The app should generate a weekly workout plan based on user profile data.

The first version can be rule-based.

The generator should consider:

* User goal
* Weekly availability
* Equipment
* Injuries
* Training experience
* Running level
* Strength level

The generated plan may include:

* Strength day
* Upper body day
* Lower body day
* Full body day
* Running day
* Mobility day
* Rest day

Each workout item may include:

* Exercise
* Sets
* Reps
* RPE
* Rest duration
* Notes

### 7.5 Workout Logging

The user can log completed workouts.

Workout log data may include:

* Workout date
* Exercises completed
* Sets
* Reps
* Weight
* RPE
* Notes
* Completion status

### 7.6 Running Logging

The user can log running sessions.

Run log data may include:

* Date
* Distance
* Duration
* Pace
* Running type
* RPE
* Notes

Running types:

* Easy run
* Tempo run
* Interval run
* Long run
* Recovery run

### 7.7 Progress Dashboard

The user can see progress data.

Dashboard may include:

* Weekly training count
* Weekly running distance
* Total workout volume
* Average RPE
* Body weight trend
* Workout consistency
* Running pace trend
* Streak
* Goal completion rate

### 7.8 Rule-Based Coaching Insights

The app should provide simple coaching insights.

Examples:

* If average RPE is too high, suggest easier week.
* If user skipped many workouts, suggest reducing weekly volume.
* If running distance increased too fast, warn about recovery.
* If consistency is high, suggest progressive overload.
* If user has shoulder injury, avoid risky exercises.

This does not need real AI in the first version.

Later, an AI API can be integrated.

### 7.9 Admin Panel

Admin features:

* Manage exercises
* Manage users
* View audit logs
* Manage plan templates
* View system health information

## 8. Tech Stack

### Backend

* C#
* .NET 10
* ASP.NET Core Web API
* SQL Server
* Entity Framework Core
* ASP.NET Core Identity
* JWT authentication
* Refresh token rotation
* FluentValidation
* Serilog
* Redis
* Docker Compose
* xUnit
* FluentAssertions
* Testcontainers
* GitHub Actions

### Frontend

* React
* TypeScript
* Vite
* React Router
* TanStack Query
* React Hook Form
* Zod
* Tailwind CSS
* shadcn/ui
* Recharts
* Vitest
* React Testing Library
* Playwright, later

### Tools

* VS Code
* Claude Code
* Git
* GitHub
* Docker Desktop
* Postman or Bruno
* SQL Server Management Studio or Azure Data Studio

## 9. Architecture

The backend should be built as a modular monolith.

Do not use microservices at this stage.

The solution should use a Clean Architecture style.

Recommended backend projects:

* FitForge.Api
* FitForge.Application
* FitForge.Domain
* FitForge.Infrastructure
* FitForge.Tests.Unit
* FitForge.Tests.Integration
* FitForge.Worker, optional later

### Project Responsibilities

#### FitForge.Api

Responsible for:

* Controllers or minimal endpoints
* HTTP request/response handling
* Authentication configuration
* Authorization configuration
* Middleware registration
* Swagger
* Dependency injection composition
* Health checks

Controllers should not contain business logic.

#### FitForge.Application

Responsible for:

* Use cases
* Services
* Commands
* Queries
* DTOs
* Validators
* Interfaces
* Application-level business logic

#### FitForge.Domain

Responsible for:

* Entities
* Value objects
* Enums
* Domain rules
* Domain events, later

Domain should not depend on infrastructure.

#### FitForge.Infrastructure

Responsible for:

* Entity Framework Core
* DbContext
* Repository implementations, if needed
* Identity implementation
* Redis implementation
* External services
* Email service, later
* File storage, later

#### Tests

Responsible for:

* Unit tests
* Integration tests
* API endpoint tests
* Business rule tests

## 10. Backend Production Requirements

The backend should include:

* Clean project structure
* DTOs
* Validation
* Global exception handling
* Standard error response
* Authentication
* Authorization
* Refresh token rotation
* Secure password handling
* Rate limiting
* CORS configuration
* Pagination
* Filtering
* Sorting
* Logging
* Audit logging
* Health checks
* Caching
* Background jobs, later
* Tests
* Docker support
* CI/CD pipeline
* Environment-based configuration
* Swagger documentation

## 11. Security Requirements

Important rules:

* Never expose entities directly from API responses.
* Never store secrets in code.
* Use environment variables for secrets.
* Do not return stack traces in production.
* Validate all user input.
* Use authorization policies for protected endpoints.
* Use refresh token rotation.
* Revoke refresh tokens on logout.
* Add rate limiting to sensitive endpoints.
* Use HTTPS in production.
* Store passwords only through ASP.NET Core Identity hashing.
* Add audit logs for important actions.

## 12. Database Design Principles

Use SQL Server with Entity Framework Core.

Important rules:

* Use migrations.
* Use clear entity names.
* Add CreatedAt and UpdatedAt where needed.
* Add CreatedBy and UpdatedBy where useful.
* Use soft delete only where it makes sense.
* Avoid unnecessary nullable fields.
* Add indexes for frequently queried fields.
* Use pagination for list endpoints.
* Avoid N+1 query problems.
* Use projection instead of returning full entities when possible.

## 13. Frontend Principles

The frontend should be beginner-friendly but production-oriented.

Important frontend principles:

* Use TypeScript.
* Use reusable components.
* Use route protection for authenticated pages.
* Use TanStack Query for server state.
* Use React Hook Form for forms.
* Use Zod for frontend validation.
* Use a clean folder structure.
* Handle loading, error and empty states.
* Keep API calls in a separate service layer.
* Build responsive screens.
* Do not overcomplicate state management.

## 14. Learning Style

Claude should teach before coding.

For every development day, Claude should:

1. Explain what we are building.
2. Explain why it matters in a production application.
3. Explain the main concept in beginner-friendly language.
4. Show a small example if useful.
5. Create an implementation plan.
6. Wait for approval before editing files.
7. Make small, focused changes.
8. Explain every changed file.
9. Explain how to test the feature.
10. Suggest a clean commit message.

## 15. Coding Style

General rules:

* Write clean and readable code.
* Prefer explicit names.
* Avoid unnecessary abstractions.
* Do not over-engineer.
* Keep controllers thin.
* Keep business logic out of controllers.
* Use DTOs.
* Use validators.
* Use async/await properly.
* Use cancellation tokens where appropriate.
* Use dependency injection.
* Write tests for important logic.
* Update docs when important decisions change.

## 16. Done Definition

A feature is done only when:

* It works locally.
* It follows the architecture.
* It has validation.
* It has proper error handling.
* It has authorization if needed.
* It has tests if the logic is important.
* It is documented if needed.
* The code builds successfully.
* The feature can be manually tested.
* The commit message is clean.

## 17. Long-Term Vision

Version 1 should be a strong portfolio project.

Later versions can include:

* AI coaching assistant
* Mobile app
* Coach-client management
* Subscription system
* Stripe payment
* Nutrition tracking
* Wearable device integration
* Exercise video uploads
* Real notification system
* Public landing page
* Multi-language support
