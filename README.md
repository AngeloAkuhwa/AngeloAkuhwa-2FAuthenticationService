# Two-Factor Authentication (2FA) Service

## Overview
This project implements a Two-Factor Authentication (2FA) service as part of the backend system for secure user authentication. 
It includes endpoints for sending a confirmation code (simulated for current purposes) and verifying the received confirmation code.

## Prerequisites
* .NET 6 SDK
* An IDE (Visual Studio, VS Code, etc.)

## Setup Instructions
### Clone the Repository:
  * git clone https://github.com/AngeloAkuhwa/AngeloAkuhwa-2FAuthenticationService.git
  * cd AngeloAkuhwa-2FAuthenticationService

### Install Dependencies:
  * Navigate to the project directory and restore the dependencies:
    dotnet restore
### Configure Application Settings:
  * Navigate to appsettings.json.
  * Modify the settings to suit your environment (Database connection strings, JWT settings, etc.).
### Database Setup:
  * Ensure your database server is running (if using a local database).
  * Apply database migrations: dotnet ef database update

### Run the Application:
  * Start the application: dotnet run
  

## API Endpoints

### User Authentication
- **Login**
- **URL:** `/api/authentication/login`
- **Method:** POST
- **Description:** Authenticates a user.
- **Body:**
  ```json
  {
    "email": "user@example.com",
    "password": "YourPassword123"
  }
  ```

### Two-Factor Authentication

#### Enable Authenticator
- **URL:** `/api/authentication/enable-authenticator/{phoneNumber}`
- **Method:** GET
- **Description:** Initiates the 2FA setup for a given phone number.
- **URL Parameters:** `phoneNumber=[string]`

#### Verify Authenticator
- **URL:** `/api/authentication/verify-authenticator`
- **Method:** POST
- **Description:** Verifies the 2FA code sent to the user's phone.
- **Body:**
```json
{
  "phoneNumber": "123456789",
  "token": "123456"
}
```

## Configuration and Setup

### App Settings

This project utilizes an `appsettings.json.tmpl` file as a template for application settings. To set up your local configuration after cloning the repository, follow these steps:

#### Rename the Configuration File
- Locate the `appsettings.json.tmpl` in the project's root directory.
- Rename it to `appsettings.json`.
  - On Windows: Right-click the file and select "Rename".
  - On Unix/Linux/MacOS: Use the command `mv appsettings.json.tmpl appsettings.json`.

#### Update Configuration Settings
- Open the `appsettings.json` in a text editor.
- Update the settings with your local values, like database connection strings and JWT secret keys.
- Be careful not to commit sensitive data, such as JWT secret keys, to the public repository.

#### .gitignore Update
- `appsettings.json` should be listed in `.gitignore` to avoid sharing sensitive data.
- If it's not included, add `appsettings.json` to `.gitignore`.

## Testing
### Running Unit Tests
Execute unit tests with the command:
dotnet test

## Extending the Service
To integrate with real SMS gateways for sending 2FA codes, modify the `GenerateAndSend2FATokenAsync` method in the `AuthenticationService`.

## Contribution
Contributions are welcome. Please adhere to the standard pull request process for contributions.

