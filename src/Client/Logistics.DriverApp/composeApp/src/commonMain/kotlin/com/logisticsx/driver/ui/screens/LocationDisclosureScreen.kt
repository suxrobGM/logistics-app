package com.logisticsx.driver.ui.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.logisticsx.driver.permission.RequestLocationPermissionFlow
import com.logisticsx.driver.service.PreferencesManager
import com.logisticsx.driver.ui.components.AppTopBar
import com.logisticsx.driver.ui.components.SectionCard
import com.logisticsx.driver.ui.components.capture.SubmitButton
import kotlinx.coroutines.launch
import org.koin.compose.koinInject

/**
 * Prominent disclosure screen shown once per logged-in account before any
 * location permission prompt. Required by Google Play policy for apps that
 * collect location in a foreground service. After the user accepts, the
 * runtime location permission is requested and the [HAS_ACCEPTED_LOCATION_DISCLOSURE]
 * flag is persisted so this screen does not show again.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun LocationDisclosureScreen(
    onAccepted: () -> Unit,
    onDeclined: () -> Unit
) {
    val preferencesManager: PreferencesManager = koinInject()
    val coroutineScope = rememberCoroutineScope()
    var triggerPermission by remember { mutableStateOf(false) }

    RequestLocationPermissionFlow(
        trigger = triggerPermission,
        onComplete = {
            triggerPermission = false
            coroutineScope.launch {
                preferencesManager.saveHasAcceptedLocationDisclosure(true)
                onAccepted()
            }
        }
    )

    Scaffold(
        topBar = { AppTopBar(title = "Location sharing") }
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .verticalScroll(rememberScrollState())
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            Text(
                text = "We share your truck's location with your dispatcher",
                style = MaterialTheme.typography.headlineSmall,
                fontWeight = FontWeight.Bold
            )

            Text(
                text = "Before you continue, please read how location is used. " +
                    "You control when sharing is on or off.",
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )

            SectionCard(title = "What we share") {
                Text(
                    text = "Your truck's GPS coordinates and approximate street address. " +
                        "No other data from your phone is collected.",
                    style = MaterialTheme.typography.bodyMedium
                )
            }

            SectionCard(title = "When we share") {
                Text(
                    text = "Only while you are On Duty. Tap \"Go On Duty\" on the " +
                        "Dashboard to start sharing; tap \"Go Off Duty\" to stop. " +
                        "Sharing continues while the app is in the background or your " +
                        "screen is off, but only as long as you remain On Duty.",
                    style = MaterialTheme.typography.bodyMedium
                )
            }

            SectionCard(title = "Who sees it") {
                Text(
                    text = "Your dispatcher at your trucking company and the customer " +
                        "of your currently active load. Not shared with third parties.",
                    style = MaterialTheme.typography.bodyMedium
                )
            }

            SectionCard(title = "How you know it's on") {
                Text(
                    text = "While you are On Duty, a persistent notification appears " +
                        "in your status bar showing your truck number. Pull it down " +
                        "any time to confirm sharing is active.",
                    style = MaterialTheme.typography.bodyMedium
                )
            }

            Spacer(modifier = Modifier.height(8.dp))

            SubmitButton(
                text = "I understand — continue",
                onClick = { triggerPermission = true },
                isLoading = false,
                enabled = true
            )

            OutlinedButton(
                onClick = onDeclined,
                modifier = Modifier
                    .fillMaxWidth()
                    .height(56.dp)
            ) {
                Text("Cancel")
            }
        }
    }
}
