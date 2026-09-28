# Foreign Friend

An MVP conversational language-learning app built with Flutter and ASP.NET Core.

## Product contract

- The learner's native language is English and target language is isiZulu (`zu-ZA`).
- Every conversation has exactly three persistent participants: the learner and two AI friends.
- At least one friend replies to every learner turn.
- Both friends participate regularly, may respond to each other, and neither may be absent for more than two learner turns.
- The friends speak isiZulu by default, using limited English scaffolding for absolute beginners or when the learner is struggling.
- LLM and speech-provider credentials stay on the backend and are never compiled into Flutter.

Milestone 0 proves the Flutter web-to-API development path. It does not yet include onboarding, conversation persistence, AI, or audio.

## Prerequisites

- Flutter 3.47.5 or compatible stable release
- Dart 3.13.4 or compatible version bundled with Flutter
- Google Chrome
- .NET SDK 10

Android is the next platform, but its SDK and emulator are not yet configured.

## Run locally

Open two terminals from the repository root.

Start the API:

```powershell
dotnet run --project .\server\ForeignFriend.Api
```

Start Flutter in Chrome:

```powershell
cd .\client
flutter run -d chrome --dart-define=API_BASE_URL=http://localhost:5291
```

The app should display `Connected to ForeignFriend.Api`.

## Verify

```powershell
dotnet test .\language-immersion-companion.slnx

cd .\client
flutter analyze
flutter test
flutter build web --dart-define=API_BASE_URL=http://localhost:5291
```

## Configuration

The Flutter API URL is supplied at build or run time with `API_BASE_URL`. The local default is `http://localhost:5291`.

Build-time Dart values are not secrets. Future LLM and speech credentials will be supplied only to the ASP.NET Core backend through environment variables or .NET user secrets.