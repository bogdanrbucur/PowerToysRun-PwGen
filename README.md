# PowerToysRun-PwGen

Windows PowerToys Run module to quickly generate cryptographically safe random passwords.

## Install

### Automatic

Download the release and run `install.ps1` in an elevated terminal to install the plugin in PowerToys Run. The plugin will be visible in PowerToys Run.

### Manual

1. Stop PowerToys
2. Copy the contents of the release archive to `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\PasswordGenerator`. You may need to create the `PasswordGenerator` folder.
3. Start PowerToys

## Use

Activate with `pwg` in PowerToys Run or your own configured phrase to generate 2 passwords:

1. The set length one or default 16 characters long. For example, typing `pwg 10` will generate a 10 character password. Maximum length is 128 characters.
2. An Apple-style password made of 3 groups of 6 characters separated by dashes.

## Dev

### Requirements

[.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### Build

```powershell
dotnet restore
dotnet build -C Release
```

## Attribution

Project heavily inspired by [powertoys-run-unicode](https://github.com/nathancartlidge/powertoys-run-unicode) by `nathancartlidge`

## Changelog

0.9.0 - Initial release
0.9.1 - Changed Apple-style password generation. Limited number of normal password characters to 64.
1.0.0 - Public release. Tweaked install script, instructions and Apple-style password generation.
1.0.1 - Implemented cryptographically secure password generation.
