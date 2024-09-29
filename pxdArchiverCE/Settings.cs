using IniParser;
using IniParser.Model;
using System.IO;

namespace pxdArchiverCE
{
    internal static class Settings
    {
        // PATHS
        internal static string PATH_APPDATA = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "pxdArchiverCE");
        internal static Guid SESSION_GUID = Guid.NewGuid();
        internal static string PATH_APPDATA_SESSION = Path.Combine(PATH_APPDATA, "Sessions", SESSION_GUID.ToString());
        internal static string PATH_APPDATA_SESSION_CONTENTS = Path.Combine(PATH_APPDATA_SESSION, "Contents");
        internal static string PATH_APPDATA_ICONS = Path.Combine(PATH_APPDATA, "Icons");
        internal static string PATH_APPDATA_SETTINGS = Path.Combine(PATH_APPDATA, "Settings.ini");


        // SETTINGS
        internal static int IniVersion = 1;
        internal static bool CopyParToTempLocation = true;
        internal static bool LegacyMode = false;
        internal static SizeDisplayUnit SizeDisplayUnit = SizeDisplayUnit.AUTO;


        /// <summary>
        /// Initialize application data directories and settings.
        /// </summary>
        internal static void Init()
        {
            Directory.CreateDirectory(PATH_APPDATA);
            Directory.CreateDirectory(PATH_APPDATA_SESSION);
            Directory.CreateDirectory(PATH_APPDATA_SESSION_CONTENTS);
            Directory.CreateDirectory(PATH_APPDATA_ICONS);


            if (!File.Exists(PATH_APPDATA_SETTINGS))
            {
                var stream = Util.GetEmbeddedFile("SettingsTemplate.ini");
                File.WriteAllBytes(PATH_APPDATA_SETTINGS, stream.ToArray());
            }

            FileIniDataParser iniParser = new FileIniDataParser();
            IniData settings = iniParser.ReadFile(PATH_APPDATA_SETTINGS);

            IniVersion = int.Parse(settings["GENERAL"]["IniVersion"]);
            CopyParToTempLocation = bool.Parse(settings["PARC"]["CopyParToTempLocation"]);
            LegacyMode = bool.Parse(settings["PARC"]["LegacyMode"]);
            SizeDisplayUnit = (SizeDisplayUnit)int.Parse(settings["GUI"]["SizeDisplayUnit"]);
        }


        /// <summary>
        /// Remove temporary files and folders.
        /// </summary>
        internal static void Cleanup()
        {
            Directory.Delete(PATH_APPDATA_SESSION, true);
        }


        /// <summary>
        /// Update the settings file.
        /// </summary>
        internal static void SaveSettings()
        {
            FileIniDataParser iniParser = new FileIniDataParser();
            IniData settings = iniParser.ReadFile(PATH_APPDATA_SETTINGS);
            settings["PARC"]["CopyParToTempLocation"] = CopyParToTempLocation.ToString().ToLower();
            settings["PARC"]["LegacyMode"] = LegacyMode.ToString().ToLower();
            settings["GUI"]["SizeDisplayUnit"] = Convert.ToInt32(SizeDisplayUnit).ToString();
            iniParser.WriteFile(PATH_APPDATA_SETTINGS, settings);
        }
    }



    /// <summary>
    /// Unit used to represent file sizes in the GUI.
    /// </summary>
    enum SizeDisplayUnit
    {
        AUTO,
        BYTES,
    }
}
