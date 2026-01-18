package com.jfleets.driver.ui.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.layout.widthIn
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.filled.DoneAll
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.jfleets.driver.api.models.MessageDto
import com.jfleets.driver.util.formatTime

// WhatsApp-style blue color for read messages
private val ReadCheckColor = Color(0xFF53BDEB)

/**
 * A chat message bubble that displays message content with sender info and timestamp.
 */
@Composable
fun MessageBubble(
    message: MessageDto,
    isOwnMessage: Boolean,
    modifier: Modifier = Modifier,
    onMessageVisible: () -> Unit = {}
) {
    LaunchedEffect(message.id ?: message.hashCode()) {
        onMessageVisible()
    }

    Row(
        modifier = modifier.fillMaxWidth(),
        horizontalArrangement = if (isOwnMessage) Arrangement.End else Arrangement.Start
    ) {
        if (!isOwnMessage) {
            Avatar(
                name = message.senderName ?: "?",
                size = 36.dp,
                backgroundColor = MaterialTheme.colorScheme.secondary,
                textColor = MaterialTheme.colorScheme.onSecondary,
                textStyle = MaterialTheme.typography.bodySmall
            )
            Spacer(modifier = Modifier.width(8.dp))
        }

        Card(
            modifier = Modifier.widthIn(max = 280.dp),
            colors = CardDefaults.cardColors(
                containerColor = if (isOwnMessage)
                    MaterialTheme.colorScheme.primary
                else
                    MaterialTheme.colorScheme.surface
            ),
            shape = RoundedCornerShape(
                topStart = 16.dp,
                topEnd = 16.dp,
                bottomStart = if (isOwnMessage) 16.dp else 4.dp,
                bottomEnd = if (isOwnMessage) 4.dp else 16.dp
            )
        ) {
            MessageContent(
                message = message,
                isOwnMessage = isOwnMessage
            )
        }
    }
}

@Composable
private fun MessageContent(
    message: MessageDto,
    isOwnMessage: Boolean
) {
    Column(modifier = Modifier.padding(12.dp)) {
        if (!isOwnMessage && message.senderName != null) {
            Text(
                text = message.senderName,
                style = MaterialTheme.typography.labelSmall,
                fontWeight = FontWeight.SemiBold,
                color = MaterialTheme.colorScheme.primary
            )
            Spacer(modifier = Modifier.height(4.dp))
        }

        Text(
            text = message.content ?: "",
            style = MaterialTheme.typography.bodyMedium,
            color = if (isOwnMessage)
                MaterialTheme.colorScheme.onPrimary
            else
                MaterialTheme.colorScheme.onSurface
        )

        Spacer(modifier = Modifier.height(4.dp))

        // Timestamp and read status row
        Row(
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.End
        ) {
            message.sentAt?.let { sentAt ->
                Text(
                    text = sentAt.formatTime(),
                    style = MaterialTheme.typography.labelSmall,
                    color = if (isOwnMessage)
                        MaterialTheme.colorScheme.onPrimary.copy(alpha = 0.7f)
                    else
                        MaterialTheme.colorScheme.onSurfaceVariant
                )
            }

            // Read status indicator (only for own messages)
            if (isOwnMessage) {
                Spacer(modifier = Modifier.width(4.dp))
                Icon(
                    imageVector = if (message.isRead == true) Icons.Default.DoneAll else Icons.Default.Check,
                    contentDescription = if (message.isRead == true) "Read" else "Sent",
                    modifier = Modifier.size(14.dp),
                    tint = if (message.isRead == true)
                        ReadCheckColor
                    else
                        MaterialTheme.colorScheme.onPrimary.copy(alpha = 0.7f)
                )
            }
        }
    }
}
