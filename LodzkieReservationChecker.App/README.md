# Availability Checker for rezerwacje.lodzkie.eu

The https://rezerwacje.lodzkie.eu website allows you to register a visit at
"Wydział Spraw Obywatelskich i Cudzoziemców" in Łódź, Poland. This website
sucks, the dates are made available sometime after midnight. In order to
register, you will spend a few nights checking the website repeatedly, hoping
you will find a free slot. Additionally, you need to be careful, because the
server seems to use some kind of rate limiting. If you try to check too many
times (>10?) in an hour, you will get blocked for the rest of the day.

This is a simple Selenium-based app that automates the process of checking and
it plays a sound as soon as it finds a free slot (hopefully waking you up).

The configuration is done directly in the `Program.cs` file, because I was lazy
to do it in a better way. At the top of the file there are a few options to
configure, like the sound to play.

Note that the coded procedure checks for a specific type of visit. If your visit
or location is different, you will need to modify the code slightly.

## Usage

The app is built with .NET 6, so make sure you have it installed. Additionally,
it makes use of Node.js to play the sound, so install it as well. .NET Core does
not have a cross-platform way of playing music, so I used the
[play-sound](https://www.npmjs.com/package/play-sound) library from npm.

First, run the `setup.sh` script to install npm packages:

```sh
./setup.sh
```

Then, run the app:

```sh
dotnet run
```