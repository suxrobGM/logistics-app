package com.jfleets.driver.util

import com.jfleets.driver.model.DistanceUnit
import platform.Foundation.NSLengthFormatter
import platform.Foundation.NSLengthFormatterUnitKilometer
import platform.Foundation.NSLengthFormatterUnitMile

actual fun Double.formatDistance(unit: DistanceUnit): String {
    val formatter = NSLengthFormatter()
    formatter.numberFormatter.maximumFractionDigits = 1UL

    return when (unit) {
        DistanceUnit.MILES -> {
            formatter.stringFromValue(this.toMiles(), NSLengthFormatterUnitMile)
        }

        DistanceUnit.KILOMETERS -> {
            formatter.stringFromValue(
                this.toKilometers(),
                NSLengthFormatterUnitKilometer
            )
        }
    }
}
