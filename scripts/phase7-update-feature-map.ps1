# Phase 7 — update .claude/feature-map.md to use the new Modules/{Module}/{Feature}/ paths.

$ErrorActionPreference = 'Stop'

# Entity -> { Module, Feature }
$map = [ordered]@{
    'Accident'           = @('Compliance',     'Accidents')
    'AiDispatch'         = @('Integrations',   'AiDispatch')
    'ApiKey'             = @('IdentityAccess', 'ApiKeys')
    'BlogPost'           = @('Platform',       'BlogPosts')
    'Contact'            = @('Platform',       'Contacts')
    'ContactSubmission'  = @('Platform',       'Contacts')
    'Container'          = @('Operations',     'Containers')
    'CustomerUser'       = @('IdentityAccess', 'CustomerUsers')
    'Customer'           = @('IdentityAccess', 'Customers')
    'DemoRequest'        = @('Platform',       'DemoRequests')
    'Document'           = @('Integrations',   'Documents')
    'Dvir'               = @('Compliance',     'Dvir')
    'Eld'                = @('Compliance',     'Eld')
    'Employee'           = @('IdentityAccess', 'Employees')
    'Expense'            = @('Financial',      'Expenses')
    'Feature'            = @('IdentityAccess', 'Features')
    'GetNotifications'   = @('Platform',       'Notifications')
    'Inspection'         = @('Compliance',     'Inspections')
    'Invitation'         = @('IdentityAccess', 'Invitations')
    'Invoice'            = @('Financial',      'Invoices')
    'LoadBoard'          = @('Integrations',   'LoadBoard')
    'Load'               = @('Operations',     'Loads')
    'Maintenance'        = @('Operations',     'Maintenance')
    'Messaging'          = @('Integrations',   'Messaging')
    'PaymentLink'        = @('Financial',      'PaymentLinks')
    'Payment'            = @('Financial',      'Payments')
    'Payroll'            = @('Financial',      'Payroll')
    'Portal'             = @('Platform',       'Portal')
    'Privacy'            = @('Compliance',     'Privacy')
    'Reports'            = @('Platform',       'Reports')
    'Roles'              = @('IdentityAccess', 'Roles')
    'Safety'             = @('Compliance',     'Safety')
    'Stats'              = @('Platform',       'Stats')
    'StripeConnect'      = @('Financial',      'StripeConnect')
    'Subscription'       = @('IdentityAccess', 'Subscriptions')
    'Tax'                = @('Financial',      'Tax')
    'Tenant'             = @('IdentityAccess', 'Tenants')
    'Terminal'           = @('Operations',     'Terminals')
    'TimeEntry'          = @('Operations',     'TimeEntries')
    'Tracking'           = @('Operations',     'Tracking')
    'Trip'               = @('Operations',     'Trips')
    'Truck'              = @('Operations',     'Trucks')
    'UpdateNotification' = @('Integrations',   'UpdateNotifications')
    'User'               = @('IdentityAccess', 'Users')
    'Webhooks'           = @('Integrations',   'Webhooks')
}

$file = '.claude/feature-map.md'
$text = [System.IO.File]::ReadAllText($file)

# Convention block at the top
$text = $text -replace '`src/Core/Logistics\.Application/Commands/\{Feature\}/\{Verb\}\{Entity\}/`', '`src/Core/Logistics.Application/Modules/{Module}/{Feature}/Commands/{Verb}{Entity}/`'
$text = $text -replace '`src/Core/Logistics\.Application/Queries/\{Feature\}/Get\{Entity\}/`',     '`src/Core/Logistics.Application/Modules/{Module}/{Feature}/Queries/Get{Entity}/`'
$text = $text -replace '`src/Core/Logistics\.Application/Events/`',                                '`src/Core/Logistics.Application/Modules/{Module}/{Feature}/Events/`'
$text = $text -replace 'Application/Commands/Webhooks/',                                           'Application/Modules/Integrations/Webhooks/Commands/'

# Per-entity path replacements. Order matters — longer keys first so prefixes don't shadow.
foreach ($e in ($map.Keys | Sort-Object { -$_.Length })) {
    $module  = $map[$e][0]
    $feature = $map[$e][1]
    $newBase = "Modules/$module/$feature"
    foreach ($kind in 'Commands','Queries','Events') {
        # Match `Commands/{Entity}/` and `Commands/{Entity}` (when followed by inline backtick or punct)
        $text = $text -replace ("\b" + $kind + "/" + [regex]::Escape($e) + "/"), ($newBase + "/$kind/")
        $text = $text -replace ("\b" + $kind + "/" + [regex]::Escape($e) + "\b"), ($newBase + "/$kind")
    }
}

[System.IO.File]::WriteAllText($file, $text, (New-Object System.Text.UTF8Encoding($false)))
Write-Host "feature-map.md updated"
