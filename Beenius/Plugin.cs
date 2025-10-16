using Beenius;
using NLog;
using System;
using System.IO;
using System.Windows;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private static Logger Logger;

        public static string configFile;
        public static string logFile;
        public static string name = "Beenius";

        private MusicBeeApiInterface musicBee;
        private PluginInfo info = new PluginInfo();
        private GeniusClient geniusClient;

        public PluginInfo Initialise(IntPtr apiPtr)
        {
            musicBee = new MusicBeeApiInterface();
            musicBee.Initialise(apiPtr);

            configFile = Path.Combine(musicBee.Setting_GetPersistentStoragePath(), "beenius.conf");
            logFile = Path.Combine(musicBee.Setting_GetPersistentStoragePath(), @"beenius.log");

            info.PluginInfoVersion = PluginInfoVersion;
            info.Name = "Beenius";
            info.Description = "Genius support for MusicBee";
            info.Author = "slonopot";
            info.TargetApplication = "MusicBee";
            info.Type = PluginType.LyricsRetrieval;
            info.VersionMajor = 1;
            info.VersionMinor = 4;
            info.Revision = 0;
            info.MinInterfaceVersion = 20;
            info.MinApiRevision = 25;
            info.ReceiveNotifications = ReceiveNotificationFlags.StartupOnly;
            info.ConfigurationPanelHeight = 20;

            try
            {
                var target = new NLog.Targets.FileTarget(name)
                {
                    FileName = logFile,
                    Layout = "${date} | ${level} | ${callsite} | ${message}",
                    DeleteOldFileOnStartup = true,
                    Name = name
                };
                if (LogManager.Configuration == null)
                {
                    var config = new NLog.Config.LoggingConfiguration();
                    config.AddTarget(target);
                    config.AddRuleForAllLevels(target, name);
                    LogManager.Configuration = config;

                }
                else
                {
                    LogManager.Configuration.AddTarget(target);
                    LogManager.Configuration.AddRuleForAllLevels(target, name);
                }

                LogManager.ReconfigExistingLoggers();

                Logger = LogManager.GetLogger(name);

                geniusClient = new GeniusClient(BeeniusLyricsProvider);
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred during Beenius startup: " + e.Message);
                throw;
            }

            return info;
        }

        private string BeeniusLyricsProvider = "Genius via Beenius";

        public String[] GetProviders()
        {
            return new string[] { BeeniusLyricsProvider };
        }

        private (string, string, string, string) TryGetFileMetadata(String source)
        {
            var tfile = TagLib.File.Create(source);
            string title = tfile.Tag.Title;
            string artist = String.Join(" & ", tfile.Tag.Performers);
            string albumArtist = null;
            if (tfile.Tag.AlbumArtists.Length > 0)
                albumArtist = String.Join(" & ", tfile.Tag.AlbumArtists);
            string album = tfile.Tag.Album;
            Logger.Debug("Extracted metadata from {source}: artist={artist}, albumArtist={albumArtist}, title={title}, album={album}", source, artist, albumArtist, title, album);
            return (artist, albumArtist, title, album);
        }

        public String RetrieveLyrics(String source, String artist, String title, String album, bool preferSynced, String providerName)
        {
            Logger.Debug("source={source}, artist={artist}, title={title}, album={album}, preferSynced={preferSynced}, providerName={providerName}", source, artist, title, album, preferSynced, providerName);

            if (providerName != BeeniusLyricsProvider) return null;

            string albumArtist = null;

            if (source != string.Empty)
            {
                try { (artist, albumArtist, title, album) = TryGetFileMetadata(source); }
                catch { Logger.Debug("Failed to extract metadata from {source}", source); }
            }
            try
            {
                var lyrics = geniusClient.getLyrics(artist, title);
                if (string.IsNullOrEmpty(lyrics) && !string.IsNullOrEmpty(albumArtist) && artist != albumArtist) 
                    lyrics = geniusClient.getLyrics(albumArtist, title);
                return lyrics;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex);
                return null;
            }
        }

        public void ReceiveNotification(String source, NotificationType type) { }

        public void SaveSettings() { }

        public bool Configure(IntPtr panelHandle) { return false; } //fixes the popup
        public void Uninstall() { MessageBox.Show("Just delete the plugin files from the Plugins folder yourself, this plugin is not very sophisticated to handle it itself."); }

    }
}
