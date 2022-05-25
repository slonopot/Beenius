using Beenius;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private MusicBeeApiInterface musicBee;
        private PluginInfo info = new PluginInfo();
        private GeniusClient geniusClient;

        public PluginInfo Initialise(IntPtr apiPtr)
        {
            musicBee = new MusicBeeApiInterface();
            musicBee.Initialise(apiPtr);

            info.PluginInfoVersion = PluginInfoVersion;
            info.Name = "Beenius";
            info.Description = "Genius support for MusicBee";
            info.Author = "thebeginning";
            info.TargetApplication = "MusicBee";
            info.Type = PluginType.LyricsRetrieval;
            info.VersionMajor = 0;
            info.VersionMinor = 0;
            info.Revision = 1;
            info.MinInterfaceVersion = 20;
            info.MinApiRevision = 25;
            info.ReceiveNotifications = ReceiveNotificationFlags.StartupOnly;
            info.ConfigurationPanelHeight = 20;

            try
            {
                geniusClient = new GeniusClient();
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

        public String RetrieveLyrics(String source, String artist, String title, String album, bool preferSynced, String providerName)
        {
            var lyrics = geniusClient.getLyrics(artist, title, album);
            return lyrics;
        }

        public void ReceiveNotification(String source, NotificationType type) {}


    }
}
