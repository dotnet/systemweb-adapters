# Playwright tests for samples/RemoteAuth/FormsAuth and FormsAuthCore

This project is meant to be used to verify functionality of the FormsAuth and FormsAuthCore sample apps using playwright.

## Set up

- These tests currently expect the sample apps to be run, i.e. run both FormsAuth and FormsAuthCore from the sln.
- Run `dotnet test` to launch the playwright tests.
- Set PWDEBUG=1 in the environment to enable the playwright debugger which is super helpful.

