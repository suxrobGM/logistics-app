package com.logisticsx.driver.ui.components

import androidx.compose.foundation.Canvas
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.gestures.detectDragGestures
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clipToBounds
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Path
import androidx.compose.ui.graphics.StrokeCap
import androidx.compose.ui.graphics.StrokeJoin
import androidx.compose.ui.graphics.drawscope.Stroke
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.unit.dp
import com.logisticsx.driver.ui.theme.LocalExtendedColors
import com.logisticsx.driver.ui.theme.Radius
import com.logisticsx.driver.ui.theme.Spacing

data class PathData(
    val points: List<Offset> = emptyList()
)

@Composable
fun SignaturePad(
    modifier: Modifier = Modifier,
    onSignatureComplete: (List<PathData>) -> Unit,
    onClear: () -> Unit = {}
) {
    val extendedColors = LocalExtendedColors.current
    val paths = remember { mutableStateListOf<PathData>() }
    var currentPath by remember { mutableStateOf<PathData?>(null) }
    var isEmpty by remember { mutableStateOf(true) }

    val padDescription = if (isEmpty) {
        "Signature pad. Draw your signature here."
    } else {
        "Signature pad with signature drawn. Use Clear to reset or Confirm to save."
    }

    Column(modifier = modifier) {
        Text(
            text = "Signature",
            style = MaterialTheme.typography.titleMedium,
            modifier = Modifier.padding(bottom = Spacing.sm)
        )

        Box(
            modifier = Modifier
                .fillMaxWidth()
                .height(200.dp)
                .border(
                    width = 1.dp,
                    color = MaterialTheme.colorScheme.outline,
                    shape = RoundedCornerShape(Radius.md)
                )
                .background(
                    color = extendedColors.signatureBackground,
                    shape = RoundedCornerShape(Radius.md)
                )
                .clipToBounds()
                .semantics { contentDescription = padDescription }
                .pointerInput(Unit) {
                    detectDragGestures(
                        onDragStart = { offset ->
                            currentPath = PathData(points = listOf(offset))
                            isEmpty = false
                        },
                        onDrag = { change, _ ->
                            currentPath?.let { path ->
                                currentPath = path.copy(
                                    points = path.points + change.position
                                )
                            }
                        },
                        onDragEnd = {
                            currentPath?.let { path ->
                                paths.add(path)
                                currentPath = null
                            }
                        }
                    )
                }
        ) {
            Canvas(modifier = Modifier.fillMaxSize()) {
                val strokeWidth = 3.dp.toPx()

                // Draw completed paths
                paths.forEach { pathData ->
                    if (pathData.points.size > 1) {
                        val path = Path().apply {
                            moveTo(pathData.points.first().x, pathData.points.first().y)
                            pathData.points.drop(1).forEach { point ->
                                lineTo(point.x, point.y)
                            }
                        }
                        drawPath(
                            path = path,
                            color = extendedColors.signatureStroke,
                            style = Stroke(
                                width = strokeWidth,
                                cap = StrokeCap.Round,
                                join = StrokeJoin.Round
                            )
                        )
                    }
                }

                // Draw current path
                currentPath?.let { pathData ->
                    if (pathData.points.size > 1) {
                        val path = Path().apply {
                            moveTo(pathData.points.first().x, pathData.points.first().y)
                            pathData.points.drop(1).forEach { point ->
                                lineTo(point.x, point.y)
                            }
                        }
                        drawPath(
                            path = path,
                            color = extendedColors.signatureStroke,
                            style = Stroke(
                                width = strokeWidth,
                                cap = StrokeCap.Round,
                                join = StrokeJoin.Round
                            )
                        )
                    }
                }
            }

            if (isEmpty) {
                Text(
                    text = "Sign here",
                    color = extendedColors.signaturePlaceholder,
                    modifier = Modifier.align(Alignment.Center)
                )
            }
        }

        Spacer(modifier = Modifier.height(Spacing.sm))

        Row(
            modifier = Modifier.fillMaxWidth()
        ) {
            OutlinedButton(
                onClick = {
                    paths.clear()
                    currentPath = null
                    isEmpty = true
                    onClear()
                },
                modifier = Modifier.weight(1f)
            ) {
                Text("Clear")
            }

            Spacer(modifier = Modifier.width(Spacing.sm))

            Button(
                onClick = {
                    if (!isEmpty) {
                        onSignatureComplete(paths.toList())
                    }
                },
                enabled = !isEmpty,
                modifier = Modifier.weight(1f)
            ) {
                Text("Confirm")
            }
        }
    }
}
