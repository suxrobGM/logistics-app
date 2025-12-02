package com.jfleets.driver.util

import android.icu.text.MeasureFormat
import android.icu.util.Measure
import android.icu.util.MeasureUnit
import com.jfleets.driver.model.DistanceUnit
import java.util.Locale


actual fun Double.formatDistance(unit: DistanceUnit): String {
    val locale = Locale.getDefault()
    val measureFormat = MeasureFormat.getInstance(locale, MeasureFormat.FormatWidth.SHORT)

    return when (unit) {
        DistanceUnit.MILES -> {
            val measure = Measure(this.toMiles(), MeasureUnit.MILE)
            measureFormat.format(measure)
        }

        DistanceUnit.KILOMETERS -> {
            val measure = Measure(this.toKilometers(), MeasureUnit.KILOMETER)
            measureFormat.format(measure)
        }
    }
}
