@echo off
echo Checking if smtp4dev is installed...

where smtp4dev >nul 2>&1
if %errorlevel% neq 0 (
    echo smtp4dev not found. Installing...
    dotnet tool install -g Rnwood.Smtp4dev
    if %errorlevel% neq 0 (
        echo Failed to install smtp4dev
        exit /b 1
    )
    echo smtp4dev installed successfully.
) else (
    echo smtp4dev is already installed.
)

echo.
echo Starting smtp4dev...
echo Web UI: http://localhost:5000
echo SMTP Port: 25
echo.
smtp4dev --urls=http://localhost:5000 --smtpport=25
