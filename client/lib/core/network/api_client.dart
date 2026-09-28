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

  Future<ConversationSnapshot> createConversation() async {
    final response = await _httpClient
        .post(
          Uri.parse('$_baseUrl/api/conversations'),
          headers: const {'Content-Type': 'application/json'},
          body: jsonEncode({'learnerName': 'You'}),
        )
        .timeout(timeout);

    return _readConversation(response, expectedStatusCode: 201);
  }

  Future<ConversationSnapshot> sendTurn(
    String conversationId,
    String text,
  ) async {
    final response = await _httpClient
        .post(
          Uri.parse('$_baseUrl/api/conversations/$conversationId/turns'),
          headers: const {'Content-Type': 'application/json'},
          body: jsonEncode({'text': text}),
        )
        .timeout(timeout);

    return _readConversation(response, expectedStatusCode: 200);
  }

  ConversationSnapshot _readConversation(
    http.Response response, {
    required int expectedStatusCode,
  }) {
    if (response.statusCode != expectedStatusCode) {
      throw ApiException(
        'Conversation request returned ${response.statusCode}.',
      );
    }

    try {
      final json = jsonDecode(response.body) as Map<String, dynamic>;
      return ConversationSnapshot.fromJson(json);
    } on FormatException catch (error) {
      throw ApiException('Conversation request returned invalid JSON.', error);
    } on TypeError catch (error) {
      throw ApiException(
        'Conversation request returned an unexpected response.',
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

class ConversationSnapshot {
  const ConversationSnapshot({
    required this.id,
    required this.participants,
    required this.turns,
  });

  factory ConversationSnapshot.fromJson(Map<String, dynamic> json) {
    return ConversationSnapshot(
      id: json['id'] as String,
      participants: (json['participants'] as List<dynamic>)
          .map(
            (participant) => ConversationParticipant.fromJson(
              participant as Map<String, dynamic>,
            ),
          )
          .toList(growable: false),
      turns: (json['turns'] as List<dynamic>)
          .map(
            (turn) => ConversationTurn.fromJson(turn as Map<String, dynamic>),
          )
          .toList(growable: false),
    );
  }

  final String id;
  final List<ConversationParticipant> participants;
  final List<ConversationTurn> turns;
}

class ConversationParticipant {
  const ConversationParticipant({
    required this.id,
    required this.displayName,
    required this.role,
  });

  factory ConversationParticipant.fromJson(Map<String, dynamic> json) {
    return ConversationParticipant(
      id: json['id'] as String,
      displayName: json['displayName'] as String,
      role: json['role'] as String,
    );
  }

  final String id;
  final String displayName;
  final String role;
}

class ConversationTurn {
  const ConversationTurn({
    required this.sequenceNumber,
    required this.learnerParticipantId,
    required this.learnerText,
    required this.friendResponses,
  });

  factory ConversationTurn.fromJson(Map<String, dynamic> json) {
    return ConversationTurn(
      sequenceNumber: json['sequenceNumber'] as int,
      learnerParticipantId: json['learnerParticipantId'] as String,
      learnerText: json['learnerText'] as String,
      friendResponses: (json['friendResponses'] as List<dynamic>)
          .map(
            (response) =>
                FriendResponse.fromJson(response as Map<String, dynamic>),
          )
          .toList(growable: false),
    );
  }

  final int sequenceNumber;
  final String learnerParticipantId;
  final String learnerText;
  final List<FriendResponse> friendResponses;
}

class FriendResponse {
  const FriendResponse({required this.participantId, required this.text});

  factory FriendResponse.fromJson(Map<String, dynamic> json) {
    return FriendResponse(
      participantId: json['participantId'] as String,
      text: json['text'] as String,
    );
  }

  final String participantId;
  final String text;
}

class ApiException implements Exception {
  const ApiException(this.message, [this.cause]);

  final String message;
  final Object? cause;

  @override
  String toString() => message;
}
