package com.jfleets.driver.navigation

sealed class Screen(val route: String) {
    object Login : Screen("login")
    object Dashboard : Screen("dashboard")
    object Stats : Screen("stats")
    object PastLoads : Screen("past_loads")
    object LoadDetail : Screen("load_detail/{loadId}") {
        fun createRoute(loadId: String) = "load_detail/$loadId"
    }

    object Messages : Screen("messages")
    object Conversation : Screen("conversation/{conversationId}") {
        fun createRoute(conversationId: String) = "conversation/$conversationId"
    }

    object PodCapture : Screen("pod_capture/{loadId}/{captureType}?tripStopId={tripStopId}") {
        fun createRoute(loadId: String, captureType: String, tripStopId: String? = null): String {
            val base = "pod_capture/$loadId/$captureType"
            return if (tripStopId != null) "$base?tripStopId=$tripStopId" else base
        }
    }

    object ConditionReport : Screen("condition_report/{loadId}/{inspectionType}") {
        fun createRoute(loadId: String, inspectionType: String) = "condition_report/$loadId/$inspectionType"
    }

    object Account : Screen("account")
    object Settings : Screen("settings")
    object About : Screen("about")
}
