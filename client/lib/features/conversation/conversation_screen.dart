import 'package:flutter/material.dart';

import '../../core/network/api_client.dart';

typedef CreateConversation = Future<ConversationSnapshot> Function();
typedef SendTurn = Future<ConversationSnapshot> Function(
  String conversationId,
  String text,
);

class ConversationScreen extends StatefulWidget {
  const ConversationScreen({
    required this.serviceName,
    required this.createConversation,
    required this.sendTurn,
    super.key,
  });

  final String serviceName;
  final CreateConversation createConversation;
  final SendTurn sendTurn;

  @override
  State<ConversationScreen> createState() => _ConversationScreenState();
}

class _ConversationScreenState extends State<ConversationScreen> {
  final _messageController = TextEditingController();
  final _scrollController = ScrollController();
  ConversationSnapshot? _conversation;
  Object? _loadError;
  String? _sendError;
  var _isLoading = true;
  var _isSending = false;

  @override
  void initState() {
    super.initState();
    _createConversation();
  }

  @override
  void dispose() {
    _messageController.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  Future<void> _createConversation() async {
    setState(() {
      _isLoading = true;
      _loadError = null;
    });

    try {
      final conversation = await widget.createConversation();
      if (!mounted) return;
      setState(() {
        _conversation = conversation;
        _isLoading = false;
      });
    } catch (error) {
      if (!mounted) return;
      setState(() {
        _loadError = error;
        _isLoading = false;
      });
    }
  }

  Future<void> _sendMessage() async {
    final text = _messageController.text.trim();
    final conversation = _conversation;
    if (text.isEmpty || conversation == null || _isSending) return;

    setState(() {
      _isSending = true;
      _sendError = null;
    });

    try {
      final updated = await widget.sendTurn(conversation.id, text);
      if (!mounted) return;
      _messageController.clear();
      setState(() {
        _conversation = updated;
        _isSending = false;
      });
      WidgetsBinding.instance.addPostFrameCallback((_) => _scrollToBottom());
    } catch (error) {
      if (!mounted) return;
      setState(() {
        _sendError = 'Message not sent. Please try again.';
        _isSending = false;
      });
    }
  }

  void _scrollToBottom() {
    if (_scrollController.hasClients) {
      _scrollController.animateTo(
        _scrollController.position.maxScrollExtent,
        duration: const Duration(milliseconds: 250),
        curve: Curves.easeOut,
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Language Immersion Companion'),
            Text('English → isiZulu', style: TextStyle(fontSize: 13)),
          ],
        ),
        actions: [
          Tooltip(
            message: 'Connected to ${widget.serviceName}',
            child: const Padding(
              padding: EdgeInsets.only(right: 16),
              child: Icon(Icons.cloud_done_outlined),
            ),
          ),
        ],
      ),
      body: _buildBody(),
    );
  }

  Widget _buildBody() {
    if (_isLoading) {
      return const Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            CircularProgressIndicator(),
            SizedBox(height: 12),
            Text('Starting your conversation…'),
          ],
        ),
      );
    }

    if (_loadError != null) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text('Could not start a conversation.'),
            const SizedBox(height: 12),
            FilledButton.icon(
              onPressed: _createConversation,
              icon: const Icon(Icons.refresh),
              label: const Text('Retry'),
            ),
          ],
        ),
      );
    }

    final conversation = _conversation!;
    return Column(
      children: [
        Expanded(
          child: conversation.turns.isEmpty
              ? const _EmptyConversation()
              : ListView(
                  controller: _scrollController,
                  padding: const EdgeInsets.all(16),
                  children: _messages(conversation),
                ),
        ),
        if (_sendError != null)
          MaterialBanner(
            content: Text(_sendError!),
            actions: [
              TextButton(
                onPressed: () => setState(() => _sendError = null),
                child: const Text('Dismiss'),
              ),
            ],
          ),
        _MessageComposer(
          controller: _messageController,
          isSending: _isSending,
          onSend: _sendMessage,
        ),
      ],
    );
  }

  List<Widget> _messages(ConversationSnapshot conversation) {
    final names = {
      for (final participant in conversation.participants)
        participant.id: participant.displayName,
    };

    return [
      for (final turn in conversation.turns) ...[
        _MessageBubble(
          author: names[turn.learnerParticipantId] ?? 'You',
          text: turn.learnerText,
          isLearner: true,
        ),
        for (final response in turn.friendResponses)
          _MessageBubble(
            author: names[response.participantId] ?? 'Friend',
            text: response.text,
            isLearner: false,
          ),
      ],
    ];
  }
}

class _EmptyConversation extends StatelessWidget {
  const _EmptyConversation();

  @override
  Widget build(BuildContext context) {
    return const Center(
      child: Padding(
        padding: EdgeInsets.all(32),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.forum_outlined, size: 56),
            SizedBox(height: 16),
            Text('Thandi and Sipho are ready to chat.'),
            SizedBox(height: 8),
            Text('Try “Sawubona!” to say hello.'),
          ],
        ),
      ),
    );
  }
}

class _MessageBubble extends StatelessWidget {
  const _MessageBubble({
    required this.author,
    required this.text,
    required this.isLearner,
  });

  final String author;
  final String text;
  final bool isLearner;

  @override
  Widget build(BuildContext context) {
    final colors = Theme.of(context).colorScheme;
    return Align(
      alignment: isLearner ? Alignment.centerRight : Alignment.centerLeft,
      child: Container(
        constraints: const BoxConstraints(maxWidth: 560),
        margin: const EdgeInsets.only(bottom: 12),
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(
          color: isLearner
              ? colors.primaryContainer
              : colors.secondaryContainer,
          borderRadius: BorderRadius.circular(16),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(author, style: const TextStyle(fontWeight: FontWeight.bold)),
            const SizedBox(height: 4),
            Text(text),
          ],
        ),
      ),
    );
  }
}

class _MessageComposer extends StatelessWidget {
  const _MessageComposer({
    required this.controller,
    required this.isSending,
    required this.onSend,
  });

  final TextEditingController controller;
  final bool isSending;
  final VoidCallback onSend;

  @override
  Widget build(BuildContext context) {
    return SafeArea(
      top: false,
      child: Material(
        elevation: 8,
        child: Padding(
          padding: const EdgeInsets.all(12),
          child: Row(
            children: [
              Expanded(
                child: TextField(
                  key: const Key('message-input'),
                  controller: controller,
                  enabled: !isSending,
                  textInputAction: TextInputAction.send,
                  onSubmitted: (_) => onSend(),
                  decoration: const InputDecoration(
                    hintText: 'Write a message…',
                    border: OutlineInputBorder(),
                  ),
                ),
              ),
              const SizedBox(width: 8),
              IconButton.filled(
                key: const Key('send-message'),
                onPressed: isSending ? null : onSend,
                icon: isSending
                    ? const SizedBox.square(
                        dimension: 20,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Icon(Icons.send),
                tooltip: 'Send message',
              ),
            ],
          ),
        ),
      ),
    );
  }
}
