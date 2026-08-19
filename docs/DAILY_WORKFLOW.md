# Daily Learning Workflow

Every development day must follow this structure.

## Step 1 - Explain the Day

Explain:
- What we are building today
- Why this matters
- Which part of the application it belongs to
- What I should understand before coding

Use beginner-friendly language.

## Step 2 - Teach the Concept

Before writing project code, teach the main concept.

Examples:
- DTO
- JWT
- Refresh token
- Middleware
- Dependency Injection
- Entity Framework migration
- Redis cache
- Rate limiting
- Integration testing
- React protected route

Use short code examples if needed.

## Step 3 - Plan Before Editing

Before changing files:
- inspect the current project structure
- list files that will be changed
- explain why each change is needed
- wait for my approval

## Step 4 - Implement Small

Make small, reviewable changes.
Do not implement unrelated future features.

## Step 5 - Explain Changed Files

After coding, explain:
- which files changed
- what each file does
- how the flow works
- what I should pay attention to

I am completely new to the C#/.NET ecosystem — no assumed terminology, no assumed conventions, no assumed architecture knowledge. Define every new term the first time it comes up, in plain language, before using it again.

## Step 5b - Write the Daily Log File

Write `docs/daily-logs/day-XX.md` (zero-padded to match `docs/ROADMAP_40_DAYS.md`, e.g. `day-03.md`). This is a standalone written reference — after reading only this file I should understand what we did, why, and how, well enough to have zero follow-up questions. It must:

1. Explain what we built, why, what it enables later, and what would break or be missing without it — theory and technical detail mixed as needed, "why" before "how".
2. Define every new C#/.NET term the first time it's used (bridge to Node/KeystoneJS/GraphQL equivalents where that helps).
3. Walk through every file created or changed that day, one at a time, **line by line** — not a summary.
4. End with how to test/verify the result, and a common failure mode.

Do this before moving on to anything else the same day.

## Step 6 - Test

Tell me:
- which command to run
- what result I should see
- how to manually test it
- what common error may happen

## Step 7 - Commit

Suggest one clean commit message.