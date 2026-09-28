import 'package:flutter/material.dart';

import '../../core/network/api_client.dart';

typedef HealthCheck = Future<HealthStatus> Function();

class HealthScreen extends StatefulWidget {
  const HealthScreen({required this.checkHealth, super.key});

  final HealthCheck checkHealth;

  @override
  State<HealthScreen> createState() => _HealthScreenState();
}

class _HealthScreenState extends State<HealthScreen> {
  late Future<HealthStatus> _health;

  @override
  void initState() {
    super.initState();
    _health = widget.checkHealth();
  }

  void _retry() {
    setState(() {
      _health = widget.checkHealth();
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 560),
              child: Card(
                elevation: 0,
                child: Padding(
                  padding: const EdgeInsets.all(32),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(
                        Icons.forum_rounded,
                        size: 56,
                        color: Theme.of(context).colorScheme.primary,
                      ),
                      const SizedBox(height: 16),
                      Text(
                        'Foreign Friend',
                        style: Theme.of(context).textTheme.headlineMedium,
                      ),
                      const SizedBox(height: 8),
                      Text(
                        'English → isiZulu',
                        style: Theme.of(context).textTheme.titleMedium,
                      ),
                      const SizedBox(height: 8),
                      const Text(
                        'A shared conversation with two AI friends.',
                        textAlign: TextAlign.center,
                      ),
                      const SizedBox(height: 28),
                      FutureBuilder<HealthStatus>(
                        future: _health,
                        builder: (context, snapshot) {
                          if (snapshot.connectionState !=
                              ConnectionState.done) {
                            return const _LoadingStatus();
                          }

                          if (snapshot.hasError) {
                            return _ErrorStatus(onRetry: _retry);
                          }

                          return _ConnectedStatus(health: snapshot.requireData);
                        },
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _LoadingStatus extends StatelessWidget {
  const _LoadingStatus();

  @override
  Widget build(BuildContext context) {
    return const Column(
      children: [
        CircularProgressIndicator(),
        SizedBox(height: 12),
        Text('Connecting to the API…'),
      ],
    );
  }
}

class _ConnectedStatus extends StatelessWidget {
  const _ConnectedStatus({required this.health});

  final HealthStatus health;

  @override
  Widget build(BuildContext context) {
    final colors = Theme.of(context).colorScheme;

    return Semantics(
      liveRegion: true,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 14),
        decoration: BoxDecoration(
          color: colors.primaryContainer,
          borderRadius: BorderRadius.circular(16),
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.check_circle, color: colors.primary),
            const SizedBox(width: 10),
            Flexible(child: Text('Connected to ${health.service}')),
          ],
        ),
      ),
    );
  }
}

class _ErrorStatus extends StatelessWidget {
  const _ErrorStatus({required this.onRetry});

  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    return Semantics(
      liveRegion: true,
      child: Column(
        children: [
          Text(
            'The API is unavailable.',
            style: TextStyle(color: Theme.of(context).colorScheme.error),
          ),
          const SizedBox(height: 12),
          FilledButton.icon(
            onPressed: onRetry,
            icon: const Icon(Icons.refresh),
            label: const Text('Retry'),
          ),
        ],
      ),
    );
  }
}
