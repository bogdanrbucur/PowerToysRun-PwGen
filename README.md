# PowerToysRun-PwGen

Windows PowerToys Run module to quickly generate random passwords.

## Prerequisites

[.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

## Install

Run `install.ps1` which will build the project and install it as a module in PowerToys Run.

## Use

Activate with `pwd` or the configured phrase to generate 2 passwords:
1. The set length one. For example, typing `pwd 10` will generate a 10 character password
2. An Apple-style password

## Attribution

Project structure based upon [powertoys-run-unicode](https://github.com/nathancartlidge/powertoys-run-unicode) by `nathancartlidge`