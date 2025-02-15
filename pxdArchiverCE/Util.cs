using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows;
using Yarhl.FileSystem;
using System.Reflection;
using System.Drawing;

namespace pxdArchiverCE
{
    internal static class Util
    {
        /// <summary>
        /// Gets the product name of the application according to the <see cref="AssemblyProductAttribute"/>.
        /// </summary>
        /// <returns>The product name.</returns>
        public static string GetAssemblyProductName()
        {
            return Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyProductAttribute>().FirstOrDefault().Product;
        }


        /// <summary>
        /// Check if a file is currently in use by another process.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>A <see cref="bool"/> indicating if the file is currently locked.</returns>
        public static bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Check if a file is currently in use by another process.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>A <see cref="bool"/> indicating if the file is currently locked.</returns>
        public static bool IsFileLocked(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return IsFileLocked(fileInfo);
        }


        /// <summary>
        /// Summon an <b>explorer.exe</b> process with the file path as parameter, attempting to open it with its default program.
        /// </summary>
        /// <param name="path">The file to open.</param>
        public static void OpenFileWithDefaultProgram(string path)
        {
            using Process fileOpener = new Process();

            fileOpener.StartInfo.FileName = "explorer";
            fileOpener.StartInfo.Arguments = $"\"{path}\"";
            fileOpener.Start();
        }


        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    var childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }


        public static Stream GetEmbeddedFile(string name)
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            string resourceName = asm.GetManifestResourceNames().SingleOrDefault(str => str.EndsWith(name));
            if (resourceName == null) return null;
            Stream resFilestream = asm.GetManifestResourceStream(resourceName);
            if (resFilestream == null) return null;
            else return resFilestream;
        }


        public static string FormatBytes(long bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return $"{dblSByte:F2} {Suffix[i]}";
        }



        //https://stackoverflow.com/a/3780110
        public static string GetFileTypeDescription(string fileNameOrExtension)
        {
            SHFILEINFO shfi;
            if (IntPtr.Zero != SHGetFileInfo(
                                fileNameOrExtension,
                                FILE_ATTRIBUTE_NORMAL,
                                out shfi,
                                (uint)Marshal.SizeOf(typeof(SHFILEINFO)),
                                SHGFI_USEFILEATTRIBUTES | SHGFI_TYPENAME))
            {
                return shfi.szTypeName;
            }
            return null;
        }



        public static Icon GetSmallIcon(string filePath)
        {
            SHFILEINFO shfi;
            if (IntPtr.Zero != SHGetFileInfo(
                                filePath,
                                0,
                                out shfi,
                                (uint)Marshal.SizeOf(typeof(SHFILEINFO)),
                                SHGFI_ICON | SHGFI_SMALLICON))
            {
                return Icon.FromHandle(shfi.hIcon);
            }
            return null;
        }



        [DllImport("shell32")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint flags);

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        private const uint FILE_ATTRIBUTE_READONLY = 0x00000001;
        private const uint FILE_ATTRIBUTE_HIDDEN = 0x00000002;
        private const uint FILE_ATTRIBUTE_SYSTEM = 0x00000004;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        private const uint FILE_ATTRIBUTE_ARCHIVE = 0x00000020;
        private const uint FILE_ATTRIBUTE_DEVICE = 0x00000040;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        private const uint FILE_ATTRIBUTE_TEMPORARY = 0x00000100;
        private const uint FILE_ATTRIBUTE_SPARSE_FILE = 0x00000200;
        private const uint FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400;
        private const uint FILE_ATTRIBUTE_COMPRESSED = 0x00000800;
        private const uint FILE_ATTRIBUTE_OFFLINE = 0x00001000;
        private const uint FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000;
        private const uint FILE_ATTRIBUTE_ENCRYPTED = 0x00004000;
        private const uint FILE_ATTRIBUTE_VIRTUAL = 0x00010000;

        private const uint SHGFI_ICON = 0x000000100;     // get icon
        private const uint SHGFI_DISPLAYNAME = 0x000000200;     // get display name
        private const uint SHGFI_TYPENAME = 0x000000400;     // get type name
        private const uint SHGFI_ATTRIBUTES = 0x000000800;     // get attributes
        private const uint SHGFI_ICONLOCATION = 0x000001000;     // get icon location
        private const uint SHGFI_EXETYPE = 0x000002000;     // return exe type
        private const uint SHGFI_SYSICONINDEX = 0x000004000;     // get system icon index
        private const uint SHGFI_LINKOVERLAY = 0x000008000;     // put a link overlay on icon
        private const uint SHGFI_SELECTED = 0x000010000;     // show icon in selected state
        private const uint SHGFI_ATTR_SPECIFIED = 0x000020000;     // get only specified attributes
        private const uint SHGFI_LARGEICON = 0x000000000;     // get large icon
        private const uint SHGFI_SMALLICON = 0x000000001;     // get small icon
        private const uint SHGFI_OPENICON = 0x000000002;     // get open icon
        private const uint SHGFI_SHELLICONSIZE = 0x000000004;     // get shell size icon
        private const uint SHGFI_PIDL = 0x000000008;     // pszPath is a pidl
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;     // use passed dwFileAttribute
    }
}
