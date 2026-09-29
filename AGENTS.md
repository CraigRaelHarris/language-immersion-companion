# AGENTS.md

## Purpose

This repository contains the Language Immersion Companion, a conversational
English-to-isiZulu learning application.

The current implementation is an MVP consisting of:

- A Flutter client in `client/`
- An ASP.NET Core API in `server/`
- Backend domain and endpoint tests
- Flutter widget tests
- Deterministic friend responses
- In-memory conversation storage

Preserve the product contract documented in `README.md`.

## Product invariants

All changes must preserve these rules unless the task explicitly changes the
product contract:

1. The learner's native language is English.
2. The target language is isiZulu (`zu-ZA`).
3. A conversation has exactly three persistent participants:
   - One learner
   - Two AI friends
4. At least one friend responds to every learner turn.
5. Both friends participate regularly.
6. Neither friend may be absent for more than two learner turns.
7. Friends use isiZulu by default.
8. English scaffolding is allowed for absolute beginners or struggling learners.
9. LLM, speech, and other provider credentials must remain on the backend.
10. Secrets must never be compiled into the Flutter application.

## Repository structure

```text
client/
  lib/
    app/                  Application composition
    core/                 Shared configuration and networking
    features/             Feature-specific UI and logic
  test/                   Flutter and widget tests

server/
  LanguageImmersionCompanion.Api/
    Domain/               Domain entities, policies, and invariants
    Features/             HTTP endpoints, contracts, and infrastructure
  LanguageImmersionCompanion.Api.Tests/
                          Domain and API integration tests

postman/                  Local Postman configuration
README.md                 Product contract and development instructions```

## General development rules
- Make the smallest change that completely addresses the task.
- Follow existing naming, formatting, and project structure.
- Do not introduce abstractions without a current use case.
- Keep domain rules independent of HTTP, persistence, and provider concerns.
- Validate untrusted input at the API boundary and enforce important invariants again in the domain.
- Prefer immutable records and read-only collections for externally visible data.
- Preserve nullable-reference-type correctness.
- Do not add generated files, build outputs, credentials, or local configuration.
- Update tests and documentation when behavior or configuration changes.
- Avoid unrelated formatting or refactoring in focused changes.

## Backend conventions
The backend targets .NET 10 and uses ASP.NET Core minimal APIs.

## Architecture
- Place business invariants under Domain/.
- Place endpoint-specific contracts and handlers under the corresponding Features/ directory.
- Do not place business rules exclusively in endpoint handlers.
- Do not expose domain objects directly as HTTP response models.
- Keep provider integrations behind interfaces when adding generative AI, speech, or durable persistence.
- Register services with the narrowest appropriate lifetime.
- Protect shared mutable state from concurrent requests.
- Accept and propagate CancellationToken for asynchronous I/O.
- Use structured logging; do not log secrets or sensitive conversation content.

## API behavior
- Use /api for application endpoints.
- Return consistent validation problem responses for invalid input.
- Return 404 Not Found for unknown resources.
- Add OpenAPI names, tags, summaries, response types, and status codes to new endpoints.
- Set reasonable maximum lengths for learner names and message text.
- Do not expose exception details or provider responses to clients.
- Keep production CORS origins explicit and configuration-driven.

## Backend testing
Add or update tests for:
- Domain invariants
- Valid endpoint behavior
- Validation failures
- Missing resources
- Serialization contracts
- Concurrency-sensitive behavior where applicable
Prefer domain unit tests for business rules and WebApplicationFactory<Program> integration tests for HTTP behavior.

Run:

`dotnet test .\language-immersion-companion.slnx`

## Flutter conventions
The Flutter client targets the stable versions documented in README.md.

## Architecture
- Keep application composition under lib/app/.
- Keep shared configuration and HTTP concerns under lib/core/.
- Keep feature UI and feature-specific logic under lib/features/.
- Inject network operations into widgets when doing so improves testability.
- Reuse application-scoped HTTP clients instead of constructing one per request.
- Dispose controllers, clients, subscriptions, and other owned resources.
- Check mounted after asynchronous work before changing widget state.
- Prevent duplicate submissions while a request is pending.
- Preserve learner input when submission fails.
- Provide visible loading, empty, error, and retry states.

## Networking
- Read the API base URL from API_BASE_URL.
- Treat Dart build-time values as public information.
- Never add provider credentials or other secrets to Dart code.
- Apply timeouts to network requests.
- Convert transport and decoding failures into consistent application errors.
- Validate response shapes rather than assuming all successful responses are correctly formed.
- URL-encode dynamic path and query values.

## Accessibility and localization
- Use semantic labels or tooltips for icon-only controls.
- Ensure errors and changing statuses are announced when appropriate.
- Do not rely only on color to communicate state.
- Keep user-facing text ready for future localization.
- Preserve isiZulu spelling and diacritics exactly.
- Avoid treating English translations as the primary conversation content.

## Flutter testing
Add or update tests for:

- Loading, success, empty, and error states
- Retry behavior
- Message submission
- Duplicate-submission prevention
- Input preservation after errors
- HTTP status and timeout handling
- Invalid or unexpected JSON
- DTO parsing
Run from client/:

```flutter analyze
flutter test
flutter build web --dart-define=API_BASE_URL=http://localhost:5291```

## Security and privacy
- Never commit API keys, tokens, passwords, connection strings, or user secrets.
- Use environment variables or .NET user secrets for local backend credentials.
- Do not put secrets in --dart-define; Flutter build-time values are extractable.
- Avoid logging full learner messages unless explicitly required and protected.
- Apply request-size, message-length, rate, and storage limits before exposing endpoints publicly.
- Treat LLM output as untrusted input.
- Validate generated responses before adding them to a conversation.
- Do not return internal provider errors to the Flutter client.

## Persistence and external providers
The current store and response generator are deliberately temporary.

When replacing them:
- Keep the domain model independent of the persistence implementation.
- Preserve participant identity and turn ordering.
- Make turn creation atomic.
- Handle concurrent submissions deterministically.
- Do not hold process-local locks across remote I/O.
- Define provider timeouts and cancellation behavior.
- Add retry logic only for safe, idempotent operations.
- Ensure retries cannot create duplicate turns.
- Keep deterministic fakes available for automated tests.

## Generated and ignored files
Do not commit:

- **/bin/
- **/obj/
- client/.dart_tool/
- client/build/
- Test coverage output
- Local databases
- Logs
- .env files
- Local secret or settings files
Do not manually edit generated Flutter platform or .NET build artifacts unless a task specifically requires regeneration.

## Documentation
Update README.md when changing:

- Prerequisites
- Setup or run commands
- Ports or API URLs
- Configuration names
- Supported platforms
- API capabilities
- Major MVP limitations
Keep examples compatible with PowerShell because the documented local workflow uses Windows paths and commands.

## Completion checklist
Before considering a change complete:

1. Confirm product invariants are preserved.
2. Review the diff for unrelated changes.
3. Confirm no secrets or generated outputs were added.
4. Add or update relevant tests.
5. Run backend tests for backend changes.
6. Run Flutter analysis and tests for client changes.
7. Build the affected application when practical.
8. Update documentation for user-visible or configuration changes.
9. Summarize changed behavior and any checks that could not be run.

