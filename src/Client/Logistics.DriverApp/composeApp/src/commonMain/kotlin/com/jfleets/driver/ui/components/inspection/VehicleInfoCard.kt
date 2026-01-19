package com.jfleets.driver.ui.components.inspection

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.jfleets.driver.api.models.VehicleInfoDto

/**
 * Card displaying decoded vehicle information.
 */
@Composable
fun VehicleInfoCard(
    vehicleInfo: VehicleInfoDto,
    modifier: Modifier = Modifier
) {
    Card(
        modifier = modifier.fillMaxWidth(),
        colors = CardDefaults.cardColors(
            containerColor = MaterialTheme.colorScheme.secondaryContainer
        )
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Text(
                text = "Vehicle Information",
                style = MaterialTheme.typography.titleSmall
            )
            Spacer(modifier = Modifier.height(8.dp))

            if (vehicleInfo.year != null) {
                Text("Year: ${vehicleInfo.year}")
            }
            if (!vehicleInfo.make.isNullOrBlank()) {
                Text("Make: ${vehicleInfo.make}")
            }
            if (!vehicleInfo.model.isNullOrBlank()) {
                Text("Model: ${vehicleInfo.model}")
            }
            if (!vehicleInfo.bodyClass.isNullOrBlank()) {
                Text("Body: ${vehicleInfo.bodyClass}")
            }
        }
    }
}
