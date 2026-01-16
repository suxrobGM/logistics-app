package com.jfleets.driver.navigation

import androidx.compose.runtime.snapshots.SnapshotStateList
import androidx.navigation3.runtime.NavKey

/**
 * Simple navigator that wraps a back stack for navigation operations.
 */
class Navigator(private val backStack: SnapshotStateList<NavKey>) {
    /**
     * Current destination (last item in the back stack).
     */
    val currentDestination: NavKey?
        get() = backStack.lastOrNull()

    /**
     * Navigates to the specified route by adding it to the back stack.
     */
    fun navigate(route: NavKey) {
        backStack.add(route)
    }

    /**
     * Navigates to a route, removing all entries up to and including the specified route.
     * Useful for login/logout flows.
     */
    fun navigateAndClear(route: NavKey, clearUpTo: NavKey) {
        // Remove entries until we find clearUpTo (inclusive)
        while (backStack.isNotEmpty() && backStack.last() != clearUpTo) {
            backStack.removeLastOrNull()
        }
        // Remove clearUpTo itself
        if (backStack.lastOrNull() == clearUpTo) {
            backStack.removeLastOrNull()
        }
        // Add the new route
        backStack.add(route)
    }

    /**
     * Clears the back stack and navigates to the specified route.
     * Useful for logout scenarios.
     */
    fun clearAndNavigate(route: NavKey) {
        backStack.clear()
        backStack.add(route)
    }

    /**
     * Navigates to a top-level route (for bottom nav), clearing duplicates.
     */
    fun navigateToTopLevel(route: NavKey) {
        // Remove any existing instance of this route to avoid duplicates
        backStack.removeAll { it == route }
        // Keep only the root, then add the new route
        if (backStack.size > 1) {
            val root = backStack.first()
            backStack.clear()
            backStack.add(root)
        }
        if (backStack.lastOrNull() != route) {
            backStack.add(route)
        }
    }

    /**
     * Goes back by removing the last entry from the back stack.
     * Returns true if navigation happened, false if at root.
     */
    fun goBack(): Boolean {
        return if (backStack.size > 1) {
            backStack.removeLastOrNull()
            true
        } else {
            false
        }
    }
}
