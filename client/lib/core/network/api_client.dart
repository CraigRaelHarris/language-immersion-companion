import 'dart:convert';

import 'package:http/http.dart' as http;

class ApiClient {
  ApiClient({
    required String baseUrl,
    http.Client? httpClient,
    this.timeout = const Duration(seconds: 8),
  }) : _baseUrl = baseUrl.replaceFirst(RegExp(r'/$'), ''),
       _httpClient = httpClient ?? http.Client();

  final String _baseUrl;
  final http.Client _httpClient;
  final Duration timeout;

  Future<HealthStatus> getHealth() async {
    final response = await _httpClient
        .get(Uri.parse('$_baseUrl/api/health'))
        .timeout(timeout);

    if (response.statusCode != 200) {
      throw ApiException('Health check returned ${response.statusCode}.');
    }

    try {
      final json = jsonDecode(response.body) as Map<String, dynamic>;
      return HealthStatus.fromJson(json);
    } on FormatException catch (error) {
      throw ApiException('Health check returned invalid JSON.', error);
    } on TypeError catch (error) {
      throw ApiException(
        'Health check returned an unexpected response.',
        error,
      );
    }
  }
}

class HealthStatus {
  const HealthStatus({required this.status, required this.service});

  factory HealthStatus.fromJson(Map<String, dynamic> json) {
    return HealthStatus(
      status: json['status'] as String,
      service: json['service'] as String,
    );
  }

  final String status;
  final String service;
}

class ApiException implements Exception {
  const ApiException(this.message, [this.cause]);

  final String message;
  final Object? cause;

  @override
  String toString() => message;
}
