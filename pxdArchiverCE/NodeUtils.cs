using ParLibrary;
using System.IO;
using Yarhl.FileSystem;
using Yarhl.IO;

namespace pxdArchiverCE
{
    internal static class NodeUtils
    {
        internal static bool AddFile(Node parentNode, string filePath)
        {
            try
            {
                Node newNode = NodeFactory.FromFile(filePath, FileOpenMode.Read);
                newNode.TransformWith(new ParFile());
                ParFile parFile = newNode.GetFormatAs<ParFile>();
                parFile.FileDate = File.GetLastWriteTime(filePath);
                parFile.Attributes = (int)File.GetAttributes(filePath);
                parentNode.Add(newNode);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        internal static bool AddDirectory(Node parentNode, string directoryPath)
        {
            try
            {
                Node newNode = GetDirectoryAsNode(directoryPath);
                parentNode.Add(newNode);
                parentNode.SortChildren((x, y) => string.CompareOrdinal(x.Name.ToLowerInvariant(), y.Name.ToLowerInvariant()));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        internal static Node GetDirectoryAsNode(string directoryPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);

            Node container = NodeFactory.CreateContainer(dirInfo.Name);
            container.Tags["DirectoryInfo"] = dirInfo;

            var files = dirInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                AddFile(container, file.FullName);
            }

            var directories = dirInfo.GetDirectories();
            foreach (DirectoryInfo directory in directories)
            {
                Node directoryNode = GetDirectoryAsNode(directory.FullName);
                container.Add(directoryNode);
            }

            return container;
        }
    }
}
