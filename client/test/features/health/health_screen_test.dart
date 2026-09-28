import 'dart:async';

import 'package:flutter_test/flutter_test.dart';
import 'package:language_immersion_companion/app/app.dart';
import 'package:language_immersion_companion/core/network/api_client.dart';

void main() {
  testWidgets('shows the agreed product direction and connected API', (
    tester,
  ) async {
    await tester.pumpWidget(
      LanguageImmersionCompanionApp(
        checkHealth: () async => const HealthStatus(
          status: 'healthy',
          service: 'LanguageImmersionCompanion.Api',
        ),
      ),
    );
    await tester.pumpAndSettle();

    expect(find.text('English → isiZulu'), findsOneWidget);
    expect(
      find.text('A shared conversation with two AI friends.'),
      findsOneWidget,
    );
    expect(
      find.text('Connected to LanguageImmersionCompanion.Api'),
      findsOneWidget,
    );
  });

  testWidgets('shows loading while the health request is pending', (
    tester,
  ) async {
    final pendingHealth = Completer<HealthStatus>();

    await tester.pumpWidget(
      LanguageImmersionCompanionApp(checkHealth: () => pendingHealth.future),
    );
    await tester.pump();

    expect(find.text('Connecting to the API…'), findsOneWidget);
    expect(find.byType(LanguageImmersionCompanionApp), findsOneWidget);
  });

  testWidgets('retries after the API is unavailable', (tester) async {
    var attempt = 0;

    Future<HealthStatus> checkHealth() async {
      attempt++;
      if (attempt == 1) {
        throw const ApiException('Unavailable');
      }
      return const HealthStatus(
        status: 'healthy',
        service: 'LanguageImmersionCompanion.Api',
      );
    }

    await tester.pumpWidget(
      LanguageImmersionCompanionApp(checkHealth: checkHealth),
    );
    await tester.pumpAndSettle();
    expect(find.text('The API is unavailable.'), findsOneWidget);

    await tester.tap(find.text('Retry'));
    await tester.pumpAndSettle();

    expect(attempt, 2);
    expect(
      find.text('Connected to LanguageImmersionCompanion.Api'),
      findsOneWidget,
    );
  });
}
