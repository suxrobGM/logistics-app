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
 * Path data from compose-material-icons 1.7.3, Filled.BarChart.
 */

package com.logisticsx.driver.ui.icons

import androidx.compose.ui.graphics.vector.ImageVector

internal val iconBarChart: ImageVector by lazy {
    materialIcon(name = "Filled.BarChart") {
        materialPath {
            moveTo(4.0f, 9.0f)
            horizontalLineToRelative(4.0f)
            verticalLineToRelative(11.0f)
            horizontalLineToRelative(-4.0f)
            close()
        }
        materialPath {
            moveTo(16.0f, 13.0f)
            horizontalLineToRelative(4.0f)
            verticalLineToRelative(7.0f)
            horizontalLineToRelative(-4.0f)
            close()
        }
        materialPath {
            moveTo(10.0f, 4.0f)
            horizontalLineToRelative(4.0f)
            verticalLineToRelative(16.0f)
            horizontalLineToRelative(-4.0f)
            close()
        }
    }
}
