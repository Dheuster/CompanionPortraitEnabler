using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace OwlcatModification.Modifications.CompanionPortraitEnabler.Utility
{
    [Serializable]
    public class Settings
    {
        // Properties:
        public Settings.General          general          { set; get; }
        public Settings.PortraitSettings portraitSettings { set; get; }
        public Settings.BodySettings     bodySettings     { set; get; }
        public Settings.SnapshotSettings snapshotSettings { set; get; }
        public Settings.Logging          logging          { set; get; }
        public Settings.Permissions      permissions      { set; get; }

        public string UninstallMode                       { set; get; } = null;
		public string Disabled                            { set; get; } = null;

        // Constructor
        public Settings()
        {
            general = new General();
            portraitSettings = new PortraitSettings();
            bodySettings = new BodySettings();
            snapshotSettings = new SnapshotSettings();
            logging = new Logging();
            permissions = new Permissions();
        }

        // Support Classes
        [Serializable]
        public class General
        {
            public string Name     { set; get; } = null;
            public string Website  { set; get; } = null;
            public string Comments { set; get; } = null;
        }

        [Serializable]
        public class PortraitSettings
        {
            public string  PortraitHome         { set; get; } = null; //"πpcPortraits";
            public string  AllowPortraits       { set; get; } = null;
            public string  AllowPortraitRules   { set; get; } = null;
            public string  CreateMissingFolders { set; get; } = null;
        }

        [Serializable]
        public class BodySettings
        {
            public string AllowBodies          { set; get; } = null;
            public string AvoidNudity          { set; get; } = null;
            public string AutoScale            { set; get; } = null;
            public string AllowBodyRules       { set; get; } = null;
        }

        [Serializable]
        public class SnapshotSettings
        {
            public string SnapshotHome         { set; get; } = null; //"πpcSnapshots";
            public string AllowPartyInfo       { set; get; } = null;
            public string AllowDialogueInfo    { set; get; } = null;
            public string AllowEquipmentInfo   { set; get; } = null;
            public string AllowBodySnapshots   { set; get; } = null;
        }

        [Serializable]
        public class Logging
        {
            public string LogTrace             { set; get; } = null; 
            public string LogDebug             { set; get; } = null;
        }

        [Serializable]
        public class Permissions
        {
            public string AllowShortcutCreation { set; get; } = null;
        }
    }
}
