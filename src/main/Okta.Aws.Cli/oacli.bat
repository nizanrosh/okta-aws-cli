@echo off
if "%OS%" == "Windows_NT" setlocal

setlocal enabledelayedexpansion

set OKTA_AWS_CLI_HOME="%~dp0"

setlocal DISABLEDELAYEDEXPANSION

%OKTA_AWS_CLI_HOME%okta-aws-cli.exe %*