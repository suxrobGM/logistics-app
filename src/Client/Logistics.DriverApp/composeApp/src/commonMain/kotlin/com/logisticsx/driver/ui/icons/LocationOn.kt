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
 * Path data from compose-material-icons 1.7.3, Filled.LocationOn.
 */

package com.logisticsx.driver.ui.icons

import androidx.compose.ui.graphics.vector.ImageVector

internal val iconLocationOn: ImageVector by lazy {
    materialIcon(name = "Filled.LocationOn") {
        materialPath {
            moveTo(12.0f, 2.0f)
            curveTo(8.13f, 2.0f, 5.0f, 5.13f, 5.0f, 9.0f)
            curveToRelative(0.0f, 5.25f, 7.0f, 13.0f, 7.0f, 13.0f)
            reflectiveCurveToRelative(7.0f, -7.75f, 7.0f, -13.0f)
            curveToRelative(0.0f, -3.87f, -3.13f, -7.0f, -7.0f, -7.0f)
            close()
            moveTo(12.0f, 11.5f)
            curveToRelative(-1.38f, 0.0f, -2.5f, -1.12f, -2.5f, -2.5f)
            reflectiveCurveToRelative(1.12f, -2.5f, 2.5f, -2.5f)
            reflectiveCurveToRelative(2.5f, 1.12f, 2.5f, 2.5f)
            reflectiveCurveToRelative(-1.12f, 2.5f, -2.5f, 2.5f)
            close()
        }
    }
}
