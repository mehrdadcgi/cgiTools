# CGI Tracker

ASP.NET Web Forms (.NET Framework 4.8) ticket tracker with Telerik UI, SQL Server, and S3-ready attachments.

## Run

1. Open `cgi_tracker.sln` in Visual Studio 2022.
2. Confirm SQL connection in `cgi_tracker/Web.config` (`cgi_tracker` on `localhost`).
3. Set project as startup and press F5 (IIS Express).

## First users

Use **Create an account** on the login page, or register:

- Role `Client` — create tickets  
- Role `Support` / `Admin` — manage status + upload attachments  

Passwords are hashed with ASP.NET Identity `PasswordHasher` (username/password only; no social login).

## Attachments

Set AWS keys in `Web.config` `appSettings`:

- `AwsAccessKey`, `AwsSecretKey`, `AwsS3Bucket`, `AwsRegion`

If not set, files are stored under `App_Data/uploads` (local fallback).
