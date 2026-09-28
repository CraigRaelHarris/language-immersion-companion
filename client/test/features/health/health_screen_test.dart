import 'dart:async';

import 'package:flutter_test/flutter_test.dart';
import 'package:foreign_friend/app/app.dart';
import 'package:foreign_friend/core/network/api_client.dart';

void main() {
  testWidgets('shows the agreed product direction and connected API', (
    tester,
  ) async {
    await tester.pumpWidget(
      ForeignFriendApp(
        checkHealth: () async =>
            const HealthStatus(status: 'healthy', service: 'ForeignFriend.Api'),
      ),
    );
    await tester.pumpAndSettle();

    expect(find.text('English → isiZulu'), findsOneWidget);
    expect(
      find.text('A shared conversation with two AI friends.'),
      findsOneWidget,
    );
    expect(find.text('Connected to ForeignFriend.Api'), findsOneWidget);
  });

  testWidgets('shows loading while the health request is pending', (
    tester,
  ) async {
    final pendingHealth = Completer<HealthStatus>();

    await tester.pumpWidget(
      ForeignFriendApp(checkHealth: () => pendingHealth.future),
    );
    await tester.pump();

    expect(find.text('Connecting to the API…'), findsOneWidget);
    expect(find.byType(ForeignFriendApp), findsOneWidget);
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
        service: 'ForeignFriend.Api',
      );
    }

    await tester.pumpWidget(ForeignFriendApp(checkHealth: checkHealth));
    await tester.pumpAndSettle();
    expect(find.text('The API is unavailable.'), findsOneWidget);

    await tester.tap(find.text('Retry'));
    await tester.pumpAndSettle();

    expect(attempt, 2);
    expect(find.text('Connected to ForeignFriend.Api'), findsOneWidget);
  });
}
