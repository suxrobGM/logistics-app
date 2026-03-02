package com.logisticsx.driver.ui.theme

import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.CompositionLocalProvider
import androidx.compose.runtime.Immutable
import androidx.compose.runtime.staticCompositionLocalOf
import androidx.compose.ui.graphics.Color

@Immutable
data class ExtendedColorScheme(
    val diagramBackground: Color,
    val diagramBodyFill: Color,
    val diagramBodyOutline: Color,
    val diagramCabin: Color,
    val diagramGlass: Color,
    val diagramGlassOutline: Color,
    val diagramWheel: Color,
    val diagramHeadlight: Color,
    val diagramTaillight: Color,
    val diagramMirror: Color,
    val damageSevere: Color,
    val damageModerate: Color,
    val damageMinor: Color,
    val messageReadCheck: Color,
    val signatureStroke: Color,
    val signatureBackground: Color,
    val signaturePlaceholder: Color
)

private val LightExtendedColors = ExtendedColorScheme(
    diagramBackground = DiagramBackground,
    diagramBodyFill = DiagramBodyFill,
    diagramBodyOutline = DiagramBodyOutline,
    diagramCabin = DiagramCabin,
    diagramGlass = DiagramGlass,
    diagramGlassOutline = DiagramGlassOutline,
    diagramWheel = DiagramWheel,
    diagramHeadlight = DiagramHeadlight,
    diagramTaillight = DiagramTaillight,
    diagramMirror = DiagramMirror,
    damageSevere = DamageSevere,
    damageModerate = DamageModerate,
    damageMinor = DamageMinor,
    messageReadCheck = MessageReadCheck,
    signatureStroke = SignatureStroke,
    signatureBackground = SignatureBackground,
    signaturePlaceholder = SignaturePlaceholder
)

private val DarkExtendedColors = ExtendedColorScheme(
    diagramBackground = DiagramBackgroundDark,
    diagramBodyFill = DiagramBodyFillDark,
    diagramBodyOutline = DiagramBodyOutlineDark,
    diagramCabin = DiagramCabinDark,
    diagramGlass = DiagramGlassDark,
    diagramGlassOutline = DiagramGlassOutlineDark,
    diagramWheel = DiagramWheelDark,
    diagramHeadlight = DiagramHeadlight,
    diagramTaillight = DiagramTaillight,
    diagramMirror = DiagramBodyOutlineDark,
    damageSevere = DamageSevere,
    damageModerate = DamageModerate,
    damageMinor = DamageMinor,
    messageReadCheck = MessageReadCheck,
    signatureStroke = SignatureStrokeDark,
    signatureBackground = SignatureBackgroundDark,
    signaturePlaceholder = SignaturePlaceholderDark
)

val LocalExtendedColors = staticCompositionLocalOf { LightExtendedColors }

private val DarkColorScheme = darkColorScheme(
    primary = PrimaryBlueDark,
    secondary = PrimaryLightBlue,
    tertiary = InfoBlue,
    background = BackgroundDark,
    surface = SurfaceDark,
    error = ErrorRed,
    onPrimary = TextPrimary,
    onSecondary = TextPrimary,
    onTertiary = TextPrimary,
    onBackground = SurfaceLight,
    onSurface = SurfaceLight
)

private val LightColorScheme = lightColorScheme(
    primary = PrimaryBlue,
    secondary = PrimaryDarkBlue,
    tertiary = InfoBlue,
    background = BackgroundLight,
    surface = SurfaceLight,
    error = ErrorRed,
    onPrimary = SurfaceLight,
    onSecondary = SurfaceLight,
    onTertiary = SurfaceLight,
    onBackground = TextPrimary,
    onSurface = TextPrimary
)

@Composable
fun LogisticsDriverTheme(
    darkTheme: Boolean = isSystemInDarkTheme(),
    content: @Composable () -> Unit
) {
    val colorScheme = when {
        darkTheme -> DarkColorScheme
        else -> LightColorScheme
    }
    val extendedColors = when {
        darkTheme -> DarkExtendedColors
        else -> LightExtendedColors
    }

    CompositionLocalProvider(LocalExtendedColors provides extendedColors) {
        MaterialTheme(
            colorScheme = colorScheme,
            typography = Typography,
            content = content
        )
    }
}
