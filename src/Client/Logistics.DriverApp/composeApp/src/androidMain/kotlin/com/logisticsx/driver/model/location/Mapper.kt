package com.logisticsx.driver.model.location

import android.location.Address
import android.location.Location
import com.logisticsx.driver.api.models.GeoPoint
import com.logisticsx.driver.api.models.Address as AddressDto

fun Address?.toAddressDto(): AddressDto = AddressDto(
    line1 = this?.thoroughfare ?: "",
    line2 = this?.subThoroughfare ?: "",
    city = this?.locality ?: "",
    zipCode = this?.postalCode ?: "",
    state = this?.adminArea ?: "",
    // ISO-3166-1 alpha-2 code (e.g. "US"), not the display name — the backend
    // requires a 2-letter country code.
    country = this?.countryCode ?: "",
)

fun Location.toGeoPoint(): GeoPoint = GeoPoint(
    latitude = this.latitude,
    longitude = this.longitude
)
