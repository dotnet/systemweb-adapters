# Playwright tests for MvcApp and MvcCoreApp

This project is meant to be used to verify functionality of the MvcApp and MvcCoreApp sample apps using playwright.

## Set up

- These tests currently expect the sample apps to be run, i.e. run both MvcApp and MvcCoreApp from the sln.
- Run `dotnet test` to launch the playwright tests.
- Set PWDEBUG=1 in the environment to enable the playwright debugger which is super helpful.

