import 'package:flutter/material.dart';

import '../core/config/app_config.dart';
import '../core/network/api_client.dart';
import '../features/health/health_screen.dart';

class ForeignFriendApp extends StatelessWidget {
  const ForeignFriendApp({super.key, this.checkHealth});

  final HealthCheck? checkHealth;

  @override
  Widget build(BuildContext context) {
    final healthCheck =
        checkHealth ?? ApiClient(baseUrl: AppConfig.apiBaseUrl).getHealth;

    return MaterialApp(
      title: 'Foreign Friend',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color(0xFF006B5F),
          brightness: Brightness.light,
        ),
        scaffoldBackgroundColor: const Color(0xFFF5F7F2),
        useMaterial3: true,
      ),
      home: HealthScreen(checkHealth: healthCheck),
    );
  }
}
