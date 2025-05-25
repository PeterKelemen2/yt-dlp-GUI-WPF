# yt-dlp GUI Application

A simple **GUI frontend for yt-dlp** built with WPF (.NET). This application allows you to easily download videos from YouTube and other supported sites using yt-dlp, with customizable arguments and remote transfer options.
<p align="center">
	<img alt="Screenshot" src="https://i.imgur.com/IbaYTiZ.png" width="450"/>
</p>


## Features

- Enter a video URL to download
- Set custom **yt-dlp arguments** to control download options
- Choose a **local download location**
- Optionally **transfer downloaded files to a remote server**
- Configure remote server credentials and transfer path
- Simple and lightweight WPF interface


## Getting Started

### Prerequisites

- Windows 10 or later
- [.NET 9.0 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [yt-dlp executable](https://github.com/yt-dlp/yt-dlp/releases) (You can specify the path inside the app)

### Usage

1. Enter the **video URL** in the text box.
2. Specify any **yt-dlp arguments**.
3. Select your **local save path**.
4. (Optional) Enable **post-download transfer** to a remote server and configure remote connection settings.
5. Click **Download** to start.

The output and progress will be shown in the text area below.


## Configuration

- **yt-dlp Path:** Set the location of your yt-dlp executable.
- **Post-install Actions:** Enable to transfer files or run remote scripts after download.
- **Remote Server:** Enter host, username, password, and remote directory for transfer.


## Building from Source

Open the solution in [JetBrains Rider](https://www.jetbrains.com/rider/) or Visual Studio, restore NuGet packages, and build the project.


## License

This project is open source under the [MIT License](LICENSE).


## Contributions & Issues

Feel free to open issues or submit pull requests to improve the application.


## Acknowledgments

- [yt-dlp](https://github.com/yt-dlp/yt-dlp) for the powerful downloader backend
- WPF for the UI framework
