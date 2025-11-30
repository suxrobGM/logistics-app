package com.jfleets.driver.navigation

sealed class Screen(val route: String) {
    object Login : Screen("login")
    object Dashboard : Screen("dashboard")
    object Stats : Screen("stats")
    object PastLoads : Screen("past_loads")
    object LoadDetail : Screen("load_detail/{loadId}") {
        fun createRoute(loadId: String) = "load_detail/$loadId"
    }

    object Account : Screen("account")
    object Settings : Screen("settings")
    object About : Screen("about")
}
