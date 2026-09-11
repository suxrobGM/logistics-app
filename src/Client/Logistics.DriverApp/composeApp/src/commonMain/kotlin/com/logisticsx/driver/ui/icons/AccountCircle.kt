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
 * Path data from compose-material-icons 1.7.3, Filled.AccountCircle.
 */

package com.logisticsx.driver.ui.icons

import androidx.compose.ui.graphics.vector.ImageVector

internal val iconAccountCircle: ImageVector by lazy {
    materialIcon(name = "Filled.AccountCircle") {
        materialPath {
            moveTo(12.0f, 2.0f)
            curveTo(6.48f, 2.0f, 2.0f, 6.48f, 2.0f, 12.0f)
            reflectiveCurveToRelative(4.48f, 10.0f, 10.0f, 10.0f)
            reflectiveCurveToRelative(10.0f, -4.48f, 10.0f, -10.0f)
            reflectiveCurveTo(17.52f, 2.0f, 12.0f, 2.0f)
            close()
            moveTo(12.0f, 6.0f)
            curveToRelative(1.93f, 0.0f, 3.5f, 1.57f, 3.5f, 3.5f)
            reflectiveCurveTo(13.93f, 13.0f, 12.0f, 13.0f)
            reflectiveCurveToRelative(-3.5f, -1.57f, -3.5f, -3.5f)
            reflectiveCurveTo(10.07f, 6.0f, 12.0f, 6.0f)
            close()
            moveTo(12.0f, 20.0f)
            curveToRelative(-2.03f, 0.0f, -4.43f, -0.82f, -6.14f, -2.88f)
            curveTo(7.55f, 15.8f, 9.68f, 15.0f, 12.0f, 15.0f)
            reflectiveCurveToRelative(4.45f, 0.8f, 6.14f, 2.12f)
            curveTo(16.43f, 19.18f, 14.03f, 20.0f, 12.0f, 20.0f)
            close()
        }
    }
}
