# Two-Factor Authentication (2FA) Service

## Overview
  This project implements a Two-Factor Authentication (2FA) service as part of the backend system for secure user authentication. 
  It includes endpoints for sending a confirmation code (simulated for current purposes) and verifying the received confirmation code.

## Prerequisites
  * .NET Core SDK (Version: specify_version_here)
  * An IDE (Visual Studio, VS Code, etc.)

## Setup Instructions
  ### Clone the Repository:
    git clone https://github.com/AngeloAkuhwa/AngeloAkuhwa-2FAuthenticationService.git)https://github.com/AngeloAkuhwa/AngeloAkuhwa-2FAuthenticationService.git
    cd AngeloAkuhwa-2FAuthenticationService.git

  ### Install Dependencies:
    * Navigate to the project directory and restore the dependencies:
      dotnet restore
  ### Configure Application Settings:
    * Navigate to appsettings.json or relevant configuration file.
    * Modify the settings to suit your environment (Database connection strings, JWT settings, etc.).
  ### Database Setup:
    * Ensure your database server is running (if using a local database).
    * Apply database migrations: dotnet ef database update

  ### Run the Application:
    * Start the application: dotnet run
    
## API Endpoints
  ### User Authentication
    * Login
    * URL: /api/authentication/login
    * Method: POST
    * Description: Authenticates a user.
    * Body:
       {
        "email": "user@example.com",
        "password": "YourPassword123"
       }

## Two-Factor Authentication
  ### Enable Authenticator
    * URL: /api/authentication/enable-authenticator/{phoneNumber}
    * Method: GET
    * Description: Initiates the 2FA setup for a given phone number.
    * URL Parameters: phoneNumber=[string]
  ### Verify Authenticator
    * URL: /api/authentication/verify-authenticator
    * Method: POST
    * Description: Verifies the 2FA code sent to the user's phone.
    * Body:
        {
          "phoneNumber": "123456789",
          "token": "123456"
        }

  ## Testing
    ### Run unit tests using the following command: dotnet test
  ## Extending the Service
    * To integrate with real SMS gateways for sending 2FA codes, update the GenerateAndSend2FATokenAsync method in the AuthenticationService.
  ## Contribution
    * Contributions to this project are welcome. Please follow the standard pull request process.


