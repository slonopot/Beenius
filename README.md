## Beenius - Genius for MusicBee

It's just a lyrics provider.

### Features
Using Genius API instead of webpage parsing.

### Installation
Get a release and extract two .dll files into `C:\Program Files (x86)\MusicBee\Plugins\` directory (or whatever you've installed your MusicBee to).

### Activation
Preferences -> Plugins -> Enable Beeniuus.  
Preferences -> Tags (2) -> Lyrics -> Genius via Beenius.

### Configuration
Create beenius.conf in the Plugins directory and use this template:

    {
        "allowedDistance": 5,
        "delimiters": ["&", ";", ","],
        "maxResults": 1,
        "token": "ZTejoT_ojOEasIkT9WrMBhBQOz6eYKK5QULCMECmOhvwqjRZ6WbpamFe3geHnvp3"
    }

beenius.conf includes four options. You are allowed to use only ones you need, just omit the line and don't forget about commas in JSON.
1. Configurable title distance for minor differences. Defaults to 5. This means that a present N-character difference in search results won't affect the filtering and be considered a hit.
2. Configurable artist delimiters ("A & B, C" => "A"). Defaults to none. Useful when you have several artists for the track but Genius includes only the main one. For example, track `$uicideboy$ & JGrxxn & Black Smurf & Ramirez` will be stripped down to `$uicideboy$` with current configuration and will hit [this page](https://genius.com/Uicideboy-grayscale-lyrics).
3. Configurable search results limit. Defaults to 1. Change if the songs you are searching for does not show up at the first place (not including other types of results) in the search.
4. Configurable token. Plugin is using an anonymous Android app token which might be eventually revoked so you're able to replace it with your personal one.

Restart MusicBee to apply changes.

### Logic

1. Plugin gets either the "artist" field or the first artist in the extended list if you've edited it manually alongside with the "title".
2. Plugin searches for results just like they are. Results (artist + title) are allowed to differ no more than `allowedDistance` characters. Results are limited to `maxResults`.
3. Plugin checks for result artist aliases (Mos Def is Yasiin Bey even in his old tracks).
4. Plugin strips down the artist using the delimiters (if provided), searches and handles aliases.

### Log

You can find log at `%APPDATA%\MusicBee\beenius.log`.

### Shoutouts
https://github.com/toptensoftware/JsonKit
https://nlog-project.org/
