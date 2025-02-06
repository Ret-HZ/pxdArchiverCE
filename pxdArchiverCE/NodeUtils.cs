using ParLibrary;
using System.IO;
using System.Windows;
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
                Node newNode = NodeFactory.FromArray(Path.GetFileName(filePath), File.ReadAllBytes(filePath));
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
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        /// <summary>
        /// Sort the node's children alphabetically and based on if they are directories or not.
        /// </summary>
        /// <param name="root">The root node to sort.</param>
        /// <param name="sortWithUpperInvariant">Alternative sorting mode. (Needed by Kenzan)</param>
        // https://github.com/Kaplas80/TF3.YakuzaPlugins/pull/15
        internal static void SortNodes(Node root, bool sortWithUpperInvariant = false)
        {
            if (sortWithUpperInvariant)
            {
                root.SortChildren((x, y) =>
                {
                    // Determine if each item is a directory. Nested par directories will be treated as files.
                    bool xIsTrueDirectory = x.IsContainer && !x.Name.EndsWith(".par", StringComparison.OrdinalIgnoreCase);
                    bool yIsTrueDirectory = y.IsContainer && !y.Name.EndsWith(".par", StringComparison.OrdinalIgnoreCase);

                    // Ensure true directories come first.
                    if (xIsTrueDirectory && !yIsTrueDirectory) return -1;
                    if (!xIsTrueDirectory && yIsTrueDirectory) return 1;

                    return string.CompareOrdinal(x.Name.ToUpperInvariant(), y.Name.ToUpperInvariant());
                });
            }
            else
            {
                root.SortChildren((x, y) =>
                {
                    // Determine if each item is a directory. Nested par directories will be treated as files.
                    bool xIsTrueDirectory = x.IsContainer && !x.Name.EndsWith(".par", StringComparison.OrdinalIgnoreCase);
                    bool yIsTrueDirectory = y.IsContainer && !y.Name.EndsWith(".par", StringComparison.OrdinalIgnoreCase);

                    // Ensure true directories come first.
                    if (xIsTrueDirectory && !yIsTrueDirectory) return -1;
                    if (!xIsTrueDirectory && yIsTrueDirectory) return 1;

                    return string.CompareOrdinal(x.Name.ToLowerInvariant(), y.Name.ToLowerInvariant());
                });
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
        /// Count the amount of files inside a list of <see cref="Node"/>s, including their children.
        /// </summary>
        /// <param name="nodes">The nodes to count.</param>
        /// <returns>The total amount of files.</returns>
        internal static int CountFiles(List<Node> nodes)
        {
            int count = 0;

            foreach (Node node in nodes)
            {
                if (node.IsContainer)
                {
                    count += CountFiles(node);
                }
                else
                {
                    count++;
                }
            }
            return count;
        }


        /// <summary>
        /// Count the amount of files inside a  <see cref="Node"/>.
        /// </summary>
        /// <param name="nodes">The nodes to count.</param>
        /// <returns>The total amount of files.</returns>
        internal static int CountFiles(Node node)
        {
            int count = 0;

            if (node.IsContainer)
            {
                count += CountFiles(node.Children.ToList());
            }
            else
            {
                count++;
            }
            
            return count;
        }


        /// <summary>
        /// Set the chosen compression level on the target <see cref="Node"/>. 0 = None, 1 = Normal, 2 = High
        /// </summary>
        /// <param name="node">The node to compress.</param>
        /// <param name="compression">The compression level.</param>
        /// <param name="progressManager">The progress manager.</param>
        internal static void Compress(Node node, byte compression, ProgressManager progressManager)
        {
            if (node.IsContainer)
            {
                foreach (Node childNode in node.Children)
                {
                    Compress(childNode, compression, progressManager);
                }
            }
            else
            {
                // Update the progress window UI
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    progressManager.UpdateProgress();
                    progressManager.SetDescriptionText($"{Path.Combine(Util.GetNodeDirectory(node), node.Name)}");
                });

                if (progressManager.CheckIsCancelledByUser())
                {
                    return;
                }

                ParFile parFile = node.GetFormatAs<ParFile>();
                // Ensure the file is decompressed before further actions
                if (parFile.IsCompressed)
                {
                    node.TransformWith<ParLibrary.Sllz.Decompressor>();
                }

                if (compression == 0x00) return;

                ParLibrary.Sllz.CompressorParameters parameters = new ParLibrary.Sllz.CompressorParameters()
                {
                    Version = 0x00,
                    Endianness = 0x00,
                };

                if (compression == 0x01)
                {
                    parameters.Version = 0x01;
                }
                else if (compression == 0x02)
                {
                    parameters.Version = 0x02;
                }

                try
                {
                    node.TransformWith(new ParLibrary.Sllz.Compressor(parameters));
                }
                catch (Exception ex)
                {
                    // Leave uncompressed
                    parFile = node.GetFormatAs<ParFile>();
                    if (parFile.IsCompressed)
                    {
                        node.TransformWith<ParLibrary.Sllz.Decompressor>();
                        return;
                    }
                }

                // Leave uncompressed if the compressed result ends up being larger
                parFile = node.GetFormatAs<ParFile>();
                if (parFile.DecompressedSize < parFile.Stream.Length)
                {
                    node.TransformWith<ParLibrary.Sllz.Decompressor>();
                }
            }
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
