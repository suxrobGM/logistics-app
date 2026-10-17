# LogisticsX Driver - Store Review Guide

What to enter in the Google Play Console and App Store Connect so the driver app passes review, and
the video the Play foreground-service declaration needs. Every answer below comes from the code in
`private/src/Client/Logistics.DriverApp`. If the app changes what it collects, change this page.

- Package / bundle ID: `com.logisticsx.driver`
- Privacy policy: <https://logisticsx.app/privacy>
- Account deletion: <https://logisticsx.app/account-deletion>, and in the app under More > Privacy and data > Delete my account
- Release build: the `Driver App Release` workflow (`driver-release.yml`) in the private repo builds the signed prod AAB (tag `driver-vX.Y.Z` or run it by hand)

## Reviewer test account

The reviewer signs in to the production API with the seeded demo driver. The accounts are listed in
[test-credentials.md](../getting-started/test-credentials.md).

| Field    | Value              |
| -------- | ------------------ |
| Email    | `driver1@test.com` |
| Password | `Test12345#`       |

`driver1` belongs to the `us` demo tenant (Heartland Logistics LLC), which has a truck and loads
assigned, so Today, Trips and Documents are not empty. The tenant is resolved from the account, so
the reviewer types nothing else. To show the dispatcher side, sign in to <https://tms.logisticsx.app>
as `dispatcher1@test.com` with the same password.

Before each submission, check that the demo account still signs in on a release build and that its
truck has at least one dispatched trip.

## Google Play: foreground service declaration

Play Console > App content > Foreground service permissions. The app declares one foreground
service, `LocationTrackingService`, with `foregroundServiceType="location"` and the
`FOREGROUND_SERVICE_LOCATION` permission. It does not request `ACCESS_BACKGROUND_LOCATION`, so the
separate background location declaration does not apply.

| Question                         | Answer                                                                                                                                                                                                                                                                                                                                                                |
| -------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Foreground service type          | Location                                                                                                                                                                                                                                                                                                                                                              |
| Task                             | Sharing the truck's live location with the driver's carrier (dispatchers and managers) and with the customer of the active load, while the driver is on duty.                                                                                                                                                                                                         |
| How the user starts it           | The driver turns on the On duty switch on the Today screen. The first time, the app shows a full-screen disclosure, then the system location prompt. Without location access the switch stays off and the app says why.                                                                                                                                               |
| How the user stops it            | The driver turns the switch off (Off duty), or signs out. Either stops the service and removes the notification.                                                                                                                                                                                                                                                      |
| What the user sees while it runs | A persistent notification, "On Duty - sharing location" with the truck number, for the whole time the service runs.                                                                                                                                                                                                                                                   |
| Why it cannot be deferred        | Dispatch needs the truck's position every 30 seconds to plan the next load, answer customer ETA questions and detect arrival at a stop (500 m geofence). A WorkManager job would run minutes late or not at all on a phone that sits in a cradle with the screen off for hours, which is the normal state while driving. Stopping it mid-shift leaves dispatch blind. |
| Impact if the system stops it    | Location stops reaching dispatch until the driver opens the app again; the app restarts tracking only from the foreground.                                                                                                                                                                                                                                            |
| Video                            | See the script below. Upload it as an unlisted YouTube video and paste the link.                                                                                                                                                                                                                                                                                      |

## Video script (about 30 seconds)

Record a release build on a phone or emulator running Android 14 or later, with a fresh install so
the disclosure and permission prompt appear. Keep a browser with the TMS portal ready on a second
screen, or cut to a screen recording of it. No narration is needed; on-screen captions are enough.

| Time    | Shot                                                                                                                                         | Caption                                             |
| ------- | -------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------- |
| 0-4 s   | Launch the app. Sign in as `driver1@test.com`.                                                                                               | Driver signs in                                     |
| 4-9 s   | The Location sharing disclosure appears. Scroll it slowly so all four rows are readable. Tap "I understand, continue".                       | Prominent disclosure before any permission prompt   |
| 9-12 s  | The system location prompt appears. Choose "While using the app".                                                                            | Foreground location only                            |
| 12-15 s | Today screen. Turn on the On duty switch.                                                                                                    | Driver starts sharing                               |
| 15-19 s | Pull down the notification shade: "On Duty - sharing location" with the truck number is there. Press Home so the app goes to the background. | Persistent notification while on duty               |
| 19-25 s | Cut to the TMS portal, signed in as `dispatcher1@test.com`: Trucks > Map view. The truck's marker is at the phone's position.                | Dispatch sees the truck live                        |
| 25-30 s | Back on the phone, open the app and turn the switch off. Pull down the shade: the notification is gone.                                      | Off duty stops sharing and removes the notification |

On an emulator, set a route under Extended controls > Location before recording so the marker
moves. If the portal map takes a moment, the hub sends a fix at most every 15 to 30 seconds; start
the cut once the marker has appeared.

## Google Play: Data safety form

Play Console > App content > Data safety. All data is encrypted in transit (HTTPS only; the prod
build turns cleartext off). Users can request deletion in the app and on the website. Nothing is
sold or used for advertising, and there are no ad SDKs.

"Shared" in Play's sense means passed to a third party. The carrier that employs the driver is the
customer the app serves, so what only the carrier's staff see is collected, not shared. The load's
customer (the shipper or receiver) is a third party, and it sees some of the driver's data: the
truck's live location and the driver's name on the public tracking page, and the BOL, POD and
condition report photos and signatures in the customer portal and on the eBOL. Those types are
declared as shared, for App functionality. Firebase (Crashlytics, Cloud Messaging) processes data as
LogisticsX's service provider, which Play does not count as sharing.

| Data type (Play category)                  | Collected | Shared | Optional | Purposes                              | Where it comes from in the app                                                                                 |
| ------------------------------------------ | --------- | ------ | -------- | ------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| Location: Precise location                 | Yes       | Yes    | Yes      | App functionality                     | GPS fixes while on duty, with a reverse-geocoded street address; the load's customer sees it on tracking       |
| Location: Approximate location             | Yes       | Yes    | Yes      | App functionality                     | Used when the driver grants only approximate location; shown the same way                                      |
| Personal info: Name                        | Yes       | Yes    | No       | App functionality, Account management | Profile and sign-in; the load's customer sees it on the tracking page and the eBOL                             |
| Personal info: Email address               | Yes       | No     | No       | App functionality, Account management | Sign-in and profile                                                                                            |
| Personal info: Phone number                | Yes       | No     | Yes      | App functionality, Account management | Profile screen                                                                                                 |
| Personal info: User IDs                    | Yes       | No     | No       | App functionality, Account management | Account and tenant IDs on every request                                                                        |
| Photos and videos: Photos                  | Yes       | Yes    | Yes      | App functionality                     | BOL, POD, condition report and DVIR photos, and drawn signatures; the load's customer gets BOL/POD/eBOL copies |
| Messages: Other in-app messages            | Yes       | No     | Yes      | App functionality                     | Chat with dispatch                                                                                             |
| App activity: Other user-generated content | Yes       | No     | Yes      | App functionality                     | Notes on captures, DVIR defects, deletion reason                                                               |
| App info and performance: Crash logs       | Yes       | No     | No       | App functionality, Analytics          | Firebase Crashlytics (Android build)                                                                           |
| App info and performance: Diagnostics      | Yes       | No     | No       | App functionality, Analytics          | Crashlytics device and OS details attached to crashes                                                          |
| Device or other IDs                        | Yes       | No     | No       | App functionality                     | FCM registration token for push notifications, Crashlytics installation ID                                     |

Not collected: financial info (earnings on the Stats screen are read from the carrier's payroll,
not entered in the app), health, contacts, calendar, files, audio, web browsing, installed apps.

## App Store Connect: App Privacy

Same data, Apple's names: Precise Location, Photos or Videos, Name, Email Address, Phone Number,
User ID, Device ID, Other User Content. All are linked to the user, used for App Functionality, and
not used for tracking. The iOS build has no Crashlytics, so declare no crash or diagnostics data for
iOS. `iosApp/iosApp/PrivacyInfo.xcprivacy` lists the same types; keep the two in step.

## TestFlight and App Store review notes

Paste into App Store Connect > App Review Information > Notes, with the demo account above in the
sign-in fields.

```text
LogisticsX Driver is the companion app for truck drivers whose carrier uses the LogisticsX TMS. Accounts are created by the carrier, so there is no sign-up. Use the demo driver in the sign-in fields; it belongs to a demo carrier with a truck and loads assigned.

Location: the app asks for When In Use location only. When the driver turns on the On duty switch on the Today screen, it shares the truck's position with the carrier's dispatchers (and the customer of the active load) until the driver turns it off. We use the location background mode so sharing continues while the phone is locked in a cradle during a drive; iOS shows the blue location indicator the whole time. Turning the switch off, or signing out, stops location updates.

Push notifications: sent through Firebase Cloud Messaging when dispatch assigns, changes or removes a load, or sends a message. The app asks for permission after sign-in. Tapping a notification opens the load, trip or conversation.

Camera: used to photograph the bill of lading, proof of delivery and cargo condition, and to scan VIN and container barcodes.

Account deletion: More > Privacy and data > Delete my account. Deletion takes effect after a 30-day grace period. Privacy policy: https://logisticsx.app/privacy

The app uses only standard HTTPS (ITSAppUsesNonExemptEncryption = NO).
```

Before the first TestFlight build:

- Add `iosApp/iosApp/GoogleService-Info.plist` from the Firebase console (it is gitignored), enable
  the Push Notifications capability on the App ID, and upload an APNs key to Firebase. See the
  driver app README.
- Build and upload with the `driver-release.yml` GitHub Actions workflow in the private repo
  (no Mac needed; setup in the driver app README). It sets the build number one past the highest
  either store has seen, on the command line, which overrides the `CURRENT_PROJECT_VERSION` on the
  `iosApp` target.
