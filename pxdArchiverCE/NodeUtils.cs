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


        /// <summary>
        /// Creates a deep copy of the chosen directory <see cref="Node"/>, ensuring the properties are kept identical to the original.
        /// </summary>
        /// <param name="original">The original node.</param>
        /// <returns>A copy of the node.</returns>
        internal static Node CloneDirectory(Node original)
        {
            Node copy = new Node(original);
            foreach (Node childOriginal in Navigator.IterateNodes(original))
            {
                Node childCopy = Navigator.SearchNode(copy, childOriginal.Path);
                if (childOriginal.IsContainer)
                {
                    continue;
                }
                else
                {
                    ParFile parFileOriginal = childOriginal.GetFormatAs<ParFile>();
                    ParFile parFileCopy = childCopy.TransformWith(new ParFile()).GetFormatAs<ParFile>();

                    parFileCopy.CanBeCompressed = parFileOriginal.CanBeCompressed;
                    parFileCopy.IsCompressed = parFileOriginal.IsCompressed;
                    parFileCopy.DecompressedSize = parFileOriginal.DecompressedSize;
                    parFileCopy.Attributes = parFileOriginal.Attributes;
                    parFileCopy.FileDate = parFileOriginal.FileDate;
                    parFileCopy.Timestamp = parFileOriginal.Timestamp;
                }
            }
            return copy;
        }


        /// <summary>
        /// Creates a deep copy of the chosen file <see cref="Node"/>, ensuring the properties are kept identical to the original.
        /// </summary>
        /// <param name="original">The original node.</param>
        /// <returns>A copy of the node.</returns>
        internal static Node CloneFile(Node original)
        {
            Node copy = new Node(original);
            ParFile parFileOriginal = original.GetFormatAs<ParFile>();
            ParFile parFileCopy = copy.TransformWith(new ParFile()).GetFormatAs<ParFile>();

            parFileCopy.CanBeCompressed = parFileOriginal.CanBeCompressed;
            parFileCopy.IsCompressed = parFileOriginal.IsCompressed;
            parFileCopy.DecompressedSize = parFileOriginal.DecompressedSize;
            parFileCopy.Attributes = parFileOriginal.Attributes;
            parFileCopy.FileDate = parFileOriginal.FileDate;
            parFileCopy.Timestamp = parFileOriginal.Timestamp;

            return copy;
        }
    }
}
