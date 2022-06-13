## Beenius - Genius for MusicBee

It's just a lyrics provider.

### Features
1. Using Genius API instead of webpage parsing.
2. Author and track title in the library should be the same as in the Genius page. There's a 5-character tolerance for minor edits.

### Installation
Get a release and extract two .dll files into MusicBee/Plugins/ directory


### Activation
Preferences -> Plugins -> Enable Beeniuus.  
Preferences -> Tags (2) -> Lyrics -> Genius via Beenius.

### Configuration
beenius.conf includes two options:  
1. Configurable artist delimiters ("A & B, C" => "A"). Defaults to none. Useful when you have several artists for the track but Genius includes only the main one. For example, track `$uicideboy$ & JGrxxn & Black Smurf & Ramirez` will be stripped down to `$uicideboy$` with current configuration and will hit [this page](https://genius.com/Uicideboy-grayscale-lyrics). Use empty array [] to disable.  
2. Configurable title distance for minor differences. Defaults to 5. This means that a present N-character difference in search results won't affect the filtering and be considered a hit.

You should either preserve beenius.conf with all options or delete it completely. Restart MusicBee to apply changes.

### Shoutouts
https://github.com/toptensoftware/JsonKit
