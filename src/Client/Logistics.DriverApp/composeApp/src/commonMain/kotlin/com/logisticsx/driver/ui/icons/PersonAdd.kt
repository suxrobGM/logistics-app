/*
 * Copyright 2024 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Path data from compose-material-icons 1.7.3, Filled.PersonAdd.
 */

package com.logisticsx.driver.ui.icons

import androidx.compose.ui.graphics.vector.ImageVector

internal val iconPersonAdd: ImageVector by lazy {
    materialIcon(name = "Filled.PersonAdd") {
        materialPath {
            moveTo(15.0f, 12.0f)
            curveToRelative(2.21f, 0.0f, 4.0f, -1.79f, 4.0f, -4.0f)
            reflectiveCurveToRelative(-1.79f, -4.0f, -4.0f, -4.0f)
            reflectiveCurveToRelative(-4.0f, 1.79f, -4.0f, 4.0f)
            reflectiveCurveToRelative(1.79f, 4.0f, 4.0f, 4.0f)
            close()
            moveTo(6.0f, 10.0f)
            lineTo(6.0f, 7.0f)
            lineTo(4.0f, 7.0f)
            verticalLineToRelative(3.0f)
            lineTo(1.0f, 10.0f)
            verticalLineToRelative(2.0f)
            horizontalLineToRelative(3.0f)
            verticalLineToRelative(3.0f)
            horizontalLineToRelative(2.0f)
            verticalLineToRelative(-3.0f)
            horizontalLineToRelative(3.0f)
            verticalLineToRelative(-2.0f)
            lineTo(6.0f, 10.0f)
            close()
            moveTo(15.0f, 14.0f)
            curveToRelative(-2.67f, 0.0f, -8.0f, 1.34f, -8.0f, 4.0f)
            verticalLineToRelative(2.0f)
            horizontalLineToRelative(16.0f)
            verticalLineToRelative(-2.0f)
            curveToRelative(0.0f, -2.66f, -5.33f, -4.0f, -8.0f, -4.0f)
            close()
        }
    }
}
