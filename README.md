## Beenius - Genius for MusicBee
It's just a lyrics provider.

### Features
Using Genius API instead of webpage parsing.

### Installation
Get a release and extract all .dll files into `%APPDATA%\MusicBee\Plugins\` directory.

### Activation
Preferences -> Plugins -> Enable Beenius.  
Preferences -> Tags (2) -> Lyrics -> Genius via Beenius.

### Configuration
Create beenius.conf in the `%APPDATA%\MusicBee\` directory and use this template:

    {
        "allowedDistance": 5,
        "delimiters": ["&", ";", ","],
        "maxResults": 1,
        "addLyricsSource": false,
        "trimTitle": false,
        "removeTags": false
    }

beenius.conf includes several options. You are allowed to use only ones you need, just omit the line and don't forget about commas in JSON.
1. Configurable title distance for minor differences. Defaults to 5. This means that a present N-character difference in search results won't affect the filtering and be considered a hit.
2. Configurable artist delimiters ("A & B, C" => "A"). Defaults to none. Useful when you have several artists for the track but Genius includes only the main one. For example, track `$uicideboy$ & JGrxxn & Black Smurf & Ramirez` will be stripped down to `$uicideboy$` with current configuration and will hit [this page](https://genius.com/Uicideboy-grayscale-lyrics).
3. Configurable search results limit. Defaults to 1. Change if the songs you are searching for does not show up at the first place (not including other types of results) in the search.
4. Configurable lyrics source marker. Plugin will append "Source: Genius via Beenius" to the lyrics' beginning if enabled.
5. Configurable title trim. This option will remove all content in brackets from the title. By default MusicBee removes only features in the round brackets, this option will remove all content in `[]`, `{}`, `<>` and `()`.
6. Configurable tag removal. This option will remove all [Intro], [Verse] and alike tags.
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

https://github.com/mono/taglib-sharp
