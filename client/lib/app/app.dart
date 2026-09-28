import 'package:flutter/material.dart';

import '../core/config/app_config.dart';
import '../core/network/api_client.dart';
import '../features/conversation/conversation_screen.dart';
import '../features/health/health_screen.dart';

class LanguageImmersionCompanionApp extends StatelessWidget {
  const LanguageImmersionCompanionApp({
    super.key,
    this.checkHealth,
    this.createConversation,
    this.sendTurn,
  });

  final HealthCheck? checkHealth;
  final CreateConversation? createConversation;
  final SendTurn? sendTurn;

  @override
  Widget build(BuildContext context) {
    ApiClient createApiClient() => ApiClient(baseUrl: AppConfig.apiBaseUrl);
    final healthCheck = checkHealth ?? () => createApiClient().getHealth();
    final conversationCreator =
        createConversation ?? () => createApiClient().createConversation();
    final turnSender =
        sendTurn ??
        (conversationId, text) =>
            createApiClient().sendTurn(conversationId, text);

    return MaterialApp(
      title: 'Language Immersion Companion',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color(0xFF006B5F),
          brightness: Brightness.light,
        ),
        scaffoldBackgroundColor: const Color(0xFFF5F7F2),
        useMaterial3: true,
      ),
      home: HealthScreen(
        checkHealth: healthCheck,
        connectedBuilder: (health) => ConversationScreen(
          serviceName: health.service,
          createConversation: conversationCreator,
          sendTurn: turnSender,
        ),
      ),
    );
  }
}
