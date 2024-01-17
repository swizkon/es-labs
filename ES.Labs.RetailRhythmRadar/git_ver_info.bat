

@echo off

for /f "delims=" %%G in ('git rev-parse HEAD') do set myVar=%%G

echo The commit hash is: %myVar%

echo namespace RetailRhythmRadar.Configuration; > Configuration/Versioning.cs
echo public static class VersionInfo >> Configuration/Versioning.cs
echo {>> Configuration/Versioning.cs
echo	public const string GitVersion = "%myVar%";>> Configuration/Versioning.cs
echo }>> Configuration/Versioning.cs

REM git rev-parse HEAD >> Configuration/Versioning.cs
