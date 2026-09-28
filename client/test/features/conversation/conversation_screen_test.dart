import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:language_immersion_companion/core/network/api_client.dart';
import 'package:language_immersion_companion/features/conversation/conversation_screen.dart';

void main() {
  testWidgets('sends a message and displays the friend response', (
    tester,
  ) async {
    String? sentConversationId;
    String? sentText;

    await tester.pumpWidget(
      testApp(
        sendTurn: (conversationId, text) async {
          sentConversationId = conversationId;
          sentText = text;
          return conversationWithTurn;
        },
      ),
    );
    await tester.pumpAndSettle();

    await tester.enterText(find.byKey(const Key('message-input')), 'Sawubona');
    await tester.tap(find.byKey(const Key('send-message')));
    await tester.pumpAndSettle();

    expect(sentConversationId, 'conversation-1');
    expect(sentText, 'Sawubona');
    expect(find.text('Sawubona'), findsOneWidget);
    expect(find.text('Thandi'), findsOneWidget);
    expect(
      find.text(
        'Sawubona! Ngiyajabula ukukhuluma nawe. '
        '(Hello! I am happy to speak with you.)',
      ),
      findsOneWidget,
    );
  });

  testWidgets('disables input while sending to prevent duplicate turns', (
    tester,
  ) async {
    final pendingTurn = Completer<ConversationSnapshot>();
    var sendCount = 0;

    await tester.pumpWidget(
      testApp(
        sendTurn: (_, _) {
          sendCount++;
          return pendingTurn.future;
        },
      ),
    );
    await tester.pumpAndSettle();

    await tester.enterText(find.byKey(const Key('message-input')), 'Sawubona');
    await tester.tap(find.byKey(const Key('send-message')));
    await tester.pump();
    await tester.tap(find.byKey(const Key('send-message')));

    expect(sendCount, 1);
    expect(
      tester.widget<TextField>(find.byKey(const Key('message-input'))).enabled,
      isFalse,
    );

    pendingTurn.complete(conversationWithTurn);
    await tester.pumpAndSettle();
  });

  testWidgets('keeps text and shows an error after a failed send', (
    tester,
  ) async {
    await tester.pumpWidget(
      testApp(
        sendTurn: (_, _) async => throw const ApiException('Unavailable'),
      ),
    );
    await tester.pumpAndSettle();

    await tester.enterText(find.byKey(const Key('message-input')), 'Sawubona');
    await tester.tap(find.byKey(const Key('send-message')));
    await tester.pumpAndSettle();

    expect(find.text('Message not sent. Please try again.'), findsOneWidget);
    expect(find.text('Sawubona'), findsOneWidget);
    expect(
      tester.widget<TextField>(find.byKey(const Key('message-input'))).enabled,
      isTrue,
    );
  });
}

Widget testApp({required SendTurn sendTurn}) {
  return MaterialApp(
    home: ConversationScreen(
      serviceName: 'LanguageImmersionCompanion.Api',
      createConversation: () async => emptyConversation,
      sendTurn: sendTurn,
    ),
  );
}

const participants = [
  ConversationParticipant(id: 'learner', displayName: 'You', role: 'learner'),
  ConversationParticipant(id: 'thandi', displayName: 'Thandi', role: 'friend'),
  ConversationParticipant(id: 'sipho', displayName: 'Sipho', role: 'friend'),
];

const emptyConversation = ConversationSnapshot(
  id: 'conversation-1',
  participants: participants,
  turns: [],
);

const conversationWithTurn = ConversationSnapshot(
  id: 'conversation-1',
  participants: participants,
  turns: [
    ConversationTurn(
      sequenceNumber: 1,
      learnerParticipantId: 'learner',
      learnerText: 'Sawubona',
      friendResponses: [
        FriendResponse(
          participantId: 'thandi',
          text:
              'Sawubona! Ngiyajabula ukukhuluma nawe. '
              '(Hello! I am happy to speak with you.)',
        ),
      ],
    ),
  ],
);
