package com.logisticsx.driver.ui.components.capture

import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.logisticsx.driver.ui.components.PathData
import com.logisticsx.driver.ui.components.SignaturePad
import com.logisticsx.driver.util.SignatureConverter

/**
 * Signature pad wired to a capture form. Owns the paths → base64 PNG conversion so the
 * three capture screens (DVIR, POD, Condition Report) no longer repeat that glue.
 */
@Composable
fun SignatureCaptureField(
    onCaptured: (paths: List<PathData>, base64: String) -> Unit,
    onClear: () -> Unit,
    modifier: Modifier = Modifier
) {
    SignaturePad(
        modifier = modifier.fillMaxWidth(),
        onSignatureComplete = { paths ->
            onCaptured(paths, SignatureConverter.pathsToBase64Png(paths) ?: "")
        },
        onClear = onClear
    )
}
