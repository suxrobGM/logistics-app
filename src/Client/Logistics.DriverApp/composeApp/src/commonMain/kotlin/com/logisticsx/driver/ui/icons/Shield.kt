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
 * Vendored verbatim from compose-material-icons 1.7.3, Filled.Shield.
 */

package com.logisticsx.driver.ui.icons

import androidx.compose.ui.graphics.vector.ImageVector

internal val iconShield: ImageVector by lazy {
    materialIcon(name = "Filled.Shield") {
        materialPath {
            moveTo(12.0f, 1.0f)
            lineTo(3.0f, 5.0f)
            verticalLineToRelative(6.0f)
            curveToRelative(0.0f, 5.55f, 3.84f, 10.74f, 9.0f, 12.0f)
            curveToRelative(5.16f, -1.26f, 9.0f, -6.45f, 9.0f, -12.0f)
            verticalLineTo(5.0f)
            lineToRelative(-9.0f, -4.0f)
            close()
        }
    }
}
