package com.jfleets.driver.model.employee

import com.jfleets.driver.api.models.EmployeeDto

fun EmployeeDto.fullName(): String {
    return listOfNotNull(firstName, lastName).joinToString(" ")
}
