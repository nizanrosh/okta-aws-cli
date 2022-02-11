# okta-aws-cli
A dotnet open source which provides aws credentials using Okta

## :sunny: .NET Runtime
This project is built with DotNet 6.0 and is mandatory to install before using.

You can find and install it [here](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

Verify your dotnet version:

![image](https://user-images.githubusercontent.com/31489258/153608978-cced639e-af42-4485-8c15-5333325b0883.png)

## :gift: Installation

The Installer publishes the code to the app directory and adds it to your system's path.

The installer can be found at the root folder under its own directory.

- ### Windows
  - Run Installer.exe

- ### Linux (Ubuntu, etc) / macOS
  - It will be easier to run the installer correctly with the following command, while in its directory:
```
dotnet Installer.dll
```

Open terminal / cmd and run:
```
okta-aws-cli --version
```
If everything ran smoothly, you should see the following:

![image](https://user-images.githubusercontent.com/31489258/153494233-0a947687-7236-40e1-8d7b-25d31c753397.png)

## :tada: Usage

```cmd
okta-aws-cli <command>
```

- `run` will run the cli app, follow the prompts accordingly.
- `configure` provides the option to configure your user settings in order to avoid prompting each time you run the cli.  
You can skip configurations you wish to keep emtpy, I.E. - aws region.  
The cli will prompt for mandatory parameters in case they are not in your user settings.
- `--version` will display the current version of the app

## :clipboard: User Settings

The user settings file can be found at the users home directory.

- Windows: `C:\Users\<username>\.okta-aws-cli\usersettings.json`
- Linux (Ubuntu, etc) / macOS: `~/.okta-aws-cli/usersettings.json`

You can use `okta-aws-cli configure` to configure the user settings file or alternatively, fill it manually.

UserSettings exmaple:

```json
{
  "UserSettings": {
    "OktaDomain": "YOUR_OKTA_DOMAIN", //https://ORGANIZATION.okta.com
    "Username": "YOUR_OKTA_USER_NAME", //nizanrosh@github.com
    "Password": "YOUR_PASSWORD",
    "AppUrl": "YOUR_APP_URL", //If specified, extraction of the AppUrl will be skipped.
    "MfaType": "YOUR_OKTA_MFA_TYPE",
    "ProfileName": "YOUR_AWS_PROFILE_NAME", //The profile name you want your aws credentials to be under.
    "Region": "YOUR_AWS_REGION"
  }
}
```

## :books: Exmaples

### `run`
![run](https://user-images.githubusercontent.com/31489258/153608221-e7d2b06c-8bf8-4055-950b-43ad19b7b27a.gif)

### `configure`
![configure](https://user-images.githubusercontent.com/31489258/153611859-c686798f-5bac-4bce-ae2c-bb07f34805a2.gif)

## License

This project is licensed under the [MIT License](https://github.com/nizanrosh/okta-aws-cli/blob/main/LICENSE).
