package com.logisticsx.driver.ui.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.imePadding
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.rememberLazyListState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.logisticsx.driver.api.models.MessageDto
import com.logisticsx.driver.ui.components.AppTopBar
import com.logisticsx.driver.ui.components.EmptyStateView
import com.logisticsx.driver.ui.components.MessageBubble
import com.logisticsx.driver.ui.components.UiStateContent
import com.logisticsx.driver.ui.icons.AppIcons
import com.logisticsx.driver.viewmodel.ChatData
import com.logisticsx.driver.viewmodel.ChatViewModel
import com.logisticsx.driver.viewmodel.base.UiState
import org.koin.compose.viewmodel.koinViewModel
import org.koin.core.parameter.parametersOf

@Composable
fun ConversationScreen(
    conversationId: String,
    onBack: () -> Unit = {},
    viewModel: ChatViewModel = koinViewModel(key = conversationId) { parametersOf(conversationId) }
) {
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    var messageText by remember { mutableStateOf("") }
    val listState = rememberLazyListState()

    LaunchedEffect(uiState) {
        val state = uiState
        if (state is UiState.Success) {
            val messages = state.data.messages
            if (messages.isNotEmpty()) {
                listState.animateScrollToItem(messages.size - 1)
            }
        }
    }

    Scaffold(
        topBar = {
            AppTopBar(
                title = uiState.getConversationTitle(),
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(AppIcons.ArrowBack, contentDescription = "Back")
                    }
                }
            )
        }
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .imePadding()
        ) {
            Box(
                modifier = Modifier
                    .weight(1f)
                    .fillMaxWidth()
                    .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f))
            ) {
                UiStateContent(uiState, onRetry = { viewModel.loadMessages() }) { chatData ->
                    if (chatData.messages.isEmpty()) {
                        EmptyStateView(
                            icon = AppIcons.ChatBubbleOutline,
                            title = "No messages yet",
                            message = "Start the conversation!"
                        )
                    } else {
                        LazyColumn(
                            state = listState,
                            modifier = Modifier.fillMaxSize(),
                            contentPadding = PaddingValues(16.dp),
                            verticalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            if (chatData.hasMore) {
                                item {
                                    Box(
                                        modifier = Modifier.fillMaxWidth(),
                                        contentAlignment = Alignment.Center
                                    ) {
                                        TextButton(onClick = { viewModel.loadMessages(append = true) }) {
                                            Text("Load earlier messages")
                                        }
                                    }
                                }
                            }

                            items(chatData.messages) { message: MessageDto ->
                                MessageBubble(
                                    message = message,
                                    isOwnMessage = viewModel.isOwnMessage(message),
                                    onMessageVisible = {
                                        if (message.isRead != true && !viewModel.isOwnMessage(message)) {
                                            message.id?.let { viewModel.markAsRead(it) }
                                        }
                                    }
                                )
                            }
                        }
                    }
                }
            }

            MessageInput(
                messageText = messageText,
                onMessageTextChange = { messageText = it },
                onSend = {
                    if (messageText.isNotBlank()) {
                        viewModel.sendMessage(messageText)
                        messageText = ""
                    }
                }
            )
        }
    }
}

@Composable
private fun MessageInput(
    messageText: String,
    onMessageTextChange: (String) -> Unit,
    onSend: () -> Unit
) {
    Surface(
        modifier = Modifier.fillMaxWidth(),
        color = MaterialTheme.colorScheme.background
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = 12.dp, vertical = 8.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            OutlinedTextField(
                value = messageText,
                onValueChange = onMessageTextChange,
                modifier = Modifier.weight(1f),
                placeholder = { Text("Type a message...") },
                maxLines = 4,
                shape = RoundedCornerShape(24.dp)
            )

            Spacer(modifier = Modifier.width(8.dp))

            Surface(
                onClick = onSend,
                modifier = Modifier.size(48.dp),
                shape = CircleShape,
                color = MaterialTheme.colorScheme.primary
            ) {
                Box(contentAlignment = Alignment.Center) {
                    Icon(
                        imageVector = AppIcons.Send,
                        contentDescription = "Send",
                        tint = MaterialTheme.colorScheme.onPrimary
                    )
                }
            }
        }
    }
}

/**
 * Extension function to get conversation title from UiState<ChatData>.
 */
private fun UiState<ChatData>.getConversationTitle(): String {
    return when (this) {
        is UiState.Success -> {
            val chatData = this.data
            chatData.conversation?.name?.takeIf { it.isNotBlank() }
                ?: chatData.conversation?.participants
                    ?.mapNotNull { it.employeeName }
                    ?.filter { it.isNotBlank() }
                    ?.takeIf { it.isNotEmpty() }
                    ?.joinToString(", ")
                ?: chatData.messages
                    .filter { it.senderId != chatData.currentUserId }
                    .mapNotNull { it.senderName }
                    .filter { it.isNotBlank() }
                    .distinct()
                    .takeIf { it.isNotEmpty() }
                    ?.joinToString(", ")
                ?: "Chat"
        }
        else -> "Chat"
    }
}
