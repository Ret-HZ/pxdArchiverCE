using System.IO;

namespace pxdArchiverCE
{
    internal static class Settings
    {
        internal static string PATH_TEMP = Path.Combine(Path.GetTempPath(), "pxdArchiverCE");
        internal static string PATH_TEMP_SESSION = Path.Combine(PATH_TEMP, new Guid().ToString());
        internal static string PATH_TEMP_ICONS = Path.Combine(PATH_TEMP, "Icons");


        internal static void Init()
        {
            Directory.CreateDirectory(PATH_TEMP);
            Directory.CreateDirectory(PATH_TEMP_ICONS);
        }
    }
}
