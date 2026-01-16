package com.jfleets.driver.ui.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.automirrored.filled.Chat
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.pulltorefresh.PullToRefreshBox
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.jfleets.driver.ui.components.AppTopBar
import com.jfleets.driver.ui.components.ConversationListItem
import com.jfleets.driver.ui.components.EmptyStateView
import com.jfleets.driver.ui.components.ErrorView
import com.jfleets.driver.ui.components.LoadingIndicator
import com.jfleets.driver.viewmodel.ConversationsUiState
import com.jfleets.driver.viewmodel.CreateConversationState
import com.jfleets.driver.viewmodel.DispatcherInfo
import com.jfleets.driver.viewmodel.MessagesViewModel
import org.koin.compose.viewmodel.koinViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MessagesScreen(
    onConversationClick: (String) -> Unit = {},
    onBack: () -> Unit = {},
    viewModel: MessagesViewModel = koinViewModel()
) {
    val uiState by viewModel.conversationsState.collectAsState()
    val dispatcherInfo by viewModel.dispatcherInfo.collectAsState()
    val createConversationState by viewModel.createConversationState.collectAsState()
    val isRefreshing = uiState is ConversationsUiState.Loading
    val isCreating = createConversationState is CreateConversationState.Creating

    // Handle successful conversation creation - navigate to the new conversation
    LaunchedEffect(createConversationState) {
        if (createConversationState is CreateConversationState.Success) {
            val conversationId =
                (createConversationState as CreateConversationState.Success).conversationId
            viewModel.resetCreateConversationState()
            onConversationClick(conversationId)
        }
    }

    Scaffold(
        topBar = {
            AppTopBar(
                title = "Messages",
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, "Back")
                    }
                },
                actions = {
                    IconButton(onClick = { viewModel.refresh() }) {
                        Icon(Icons.Default.Refresh, "Refresh")
                    }
                }
            )
        },
        floatingActionButton = {
            // Show FAB to message dispatcher if dispatcher info is available and has conversations
            if (dispatcherInfo != null && uiState is ConversationsUiState.Success &&
                (uiState as ConversationsUiState.Success).conversations.isNotEmpty()
            ) {
                FloatingActionButton(
                    onClick = { viewModel.startConversationWithDispatcher() },
                    containerColor = MaterialTheme.colorScheme.primary
                ) {
                    if (isCreating) {
                        CircularProgressIndicator(
                            modifier = Modifier.size(24.dp),
                            color = MaterialTheme.colorScheme.onPrimary,
                            strokeWidth = 2.dp
                        )
                    } else {
                        Icon(
                            Icons.AutoMirrored.Filled.Chat,
                            contentDescription = "Message Dispatcher"
                        )
                    }
                }
            }
        }
    ) { paddingValues ->
        PullToRefreshBox(
            isRefreshing = isRefreshing,
            onRefresh = { viewModel.refresh() },
            modifier = Modifier.padding(paddingValues)
        ) {
            when (val state = uiState) {
                is ConversationsUiState.Loading -> {
                    LoadingIndicator()
                }

                is ConversationsUiState.Success -> {
                    if (state.conversations.isEmpty()) {
                        EmptyMessagesView(
                            dispatcherInfo = dispatcherInfo,
                            isCreating = isCreating,
                            onStartConversation = { viewModel.startConversationWithDispatcher() }
                        )
                    } else {
                        LazyColumn(
                            modifier = Modifier.fillMaxSize(),
                            contentPadding = PaddingValues(16.dp),
                            verticalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            items(state.conversations) { conversation ->
                                ConversationListItem(
                                    conversation = conversation,
                                    onClick = { conversation.id?.let { onConversationClick(it) } }
                                )
                            }
                        }
                    }
                }

                is ConversationsUiState.Error -> {
                    ErrorView(
                        message = state.message,
                        onRetry = { viewModel.refresh() }
                    )
                }
            }
        }
    }
}

@Composable
private fun EmptyMessagesView(
    dispatcherInfo: DispatcherInfo?,
    isCreating: Boolean,
    onStartConversation: () -> Unit
) {
    val message = if (dispatcherInfo != null) {
        "Start a conversation with your dispatcher"
    } else {
        "You'll be able to message your dispatcher once you have an active load"
    }

    EmptyStateView(
        icon = Icons.AutoMirrored.Filled.Chat,
        title = "No conversations yet",
        message = message,
        action = if (dispatcherInfo != null) {
            {
                Button(
                    onClick = onStartConversation,
                    enabled = !isCreating
                ) {
                    if (isCreating) {
                        CircularProgressIndicator(
                            modifier = Modifier.size(20.dp),
                            color = MaterialTheme.colorScheme.onPrimary,
                            strokeWidth = 2.dp
                        )
                        Spacer(modifier = Modifier.width(8.dp))
                    }
                    Text("Message ${dispatcherInfo.name}")
                }
            }
        } else null
    )
}
