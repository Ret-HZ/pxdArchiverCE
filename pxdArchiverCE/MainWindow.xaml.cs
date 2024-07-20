using ParLibrary.Converter;
using ParLibrary;
using System.Windows;
using Yarhl.FileSystem;
using Microsoft.Win32;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Collections.ObjectModel;

namespace pxdArchiverCE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Currently opened PARC file.
        /// </summary>
        Node PXDArchive;

        /// <summary>
        /// Cache for associated extension icons.
        /// </summary>
        Dictionary<string, BitmapImage> FileIconCache = new Dictionary<string, BitmapImage>();

        /// <summary>
        /// Previously opened nodes.
        /// </summary>
        Stack<Node> NavigationHistoryPrevious = new Stack<Node>();
        
        /// <summary>
        /// Previously opened nodes after going back in navigation.
        /// </summary>
        Stack<Node> NavigationHistoryNext = new Stack<Node>();

        /// <summary>
        /// Currently opened node.
        /// </summary>
        Node NavigationHistoryCurrent;


        public MainWindow()
        {
            InitializeComponent();
            Settings.Init();
        }


        /// <summary>
        /// Opens a PARC archive from a file.
        /// </summary>
        /// <param name="path">The path to the PARC file.</param>
        private void OpenPAR(string path)
        {
            var parameters = new ParArchiveReaderParameters
            {
                Recursive = false
            };

            PXDArchive = NodeFactory.FromFile(path);
            PXDArchive.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(parameters);

            NavigationHistoryPrevious.Clear();
            NavigationHistoryNext.Clear();
            NavigationHistoryCurrent = null;

            OpenDirectory(PXDArchive.Children[0]);
            PopulateTreeView(PXDArchive.Children[0]);
        }


        /// <summary>
        /// Open a directory and populate the DataGrid with its contents.
        /// </summary>
        /// <param name="node">The node container to use as the directory root.</param>
        /// <param name="isReturn">Is it returning to a previous directory?</param>
        private void OpenDirectory(Node node, bool isReturn = false)
        {
            if (NavigationHistoryCurrent != null)
            {
                if (isReturn) //Dont add the directory to the list of nexts if we are returning to it
                {
                    NavigationHistoryNext.Push(NavigationHistoryCurrent);
                }
                else
                {
                    NavigationHistoryPrevious.Push(NavigationHistoryCurrent);
                }
            }

            NavigationHistoryCurrent = node;
            PopulateDataGrid(node);
            UpdateNavigationToolbar();
        }


        /// <summary>
        /// Populates the DataGrid with the contents of the selected node, which will act as root of that directory.
        /// </summary>
        /// <param name="rootNode">The node to act as root of the directory.</param>
        private void PopulateDataGrid(Node rootNode)
        {
            if (rootNode == null) return;

            List<ParEntry> entries = new List<ParEntry>();
            foreach (Node node in rootNode.Children)
            {
                long sizeBytes = 0;
                long sizeBytesCompressed = 0;

                string type;
                string size;
                string compressedSize;
                string ratio;
                DateTime time;

                //Folders
                if (node.IsContainer)
                {
                    foreach (Node child in Navigator.IterateNodes(node))
                    {
                        if (!child.IsContainer)
                        {
                            var file = child.GetFormatAs<ParFile>();
                            sizeBytes += file.DecompressedSize;
                            sizeBytesCompressed += file.Stream.Length;
                        }
                    }

                    type = "Folder";
                    time = new DateTime(1970, 1, 1);
                }
                //Files
                else
                {
                    var file = node.GetFormatAs<ParFile>();
                    sizeBytes += file.DecompressedSize;
                    sizeBytesCompressed += file.Stream.Length;
                    time = file.FileDate;
                    type = Util.GetFileTypeDescription(Path.GetExtension(node.Name));
                }

                size = Util.FormatBytes(sizeBytes);
                compressedSize = Util.FormatBytes(sizeBytesCompressed);
                ratio = $"{(sizeBytes > 0 ? (int)((1.0 * sizeBytesCompressed / sizeBytes) * 100) : "---")}%";


                entries.Add(
                    new ParEntry()
                    {
                        Icon = GetDataGridEntryIcon(node.Name, node.IsContainer),
                        Name = node.Name,
                        Type = type,
                        Size = size,
                        CompressedSize = compressedSize,
                        Ratio = ratio,
                        Time = time,
                        Directory = node.Path,
                        Node = node,
                    });
            }

            datagrid_ParContents.ItemsSource = entries;
        }


        /// <summary>
        /// Generates an ObservableCollection with the directory structure and populates the TreeView.
        /// </summary>
        private void PopulateTreeView(Node node)
        {
            ObservableCollection<ParDirectory> parDirectories = new ObservableCollection<ParDirectory>();
            ParDirectory rootDirectory = new ParDirectory()
            {
                Name = "Directory",
                Node = node,
                Children = BuildParDirectoryList(node),
                IsExpanded = true,
            };
            parDirectories.Add(rootDirectory);
            treeview_ParFolders.ItemsSource = parDirectories;
        }


        /// <summary>
        /// Recursively iterates through each node's children and generates a <see cref="ParDirectory"/> collection with them if they are a container.
        /// </summary>
        /// <param name="node">The root node.</param>
        private ObservableCollection<ParDirectory> BuildParDirectoryList(Node node)
        {
            ObservableCollection<ParDirectory> childrenDirectories = new ObservableCollection<ParDirectory>();
            foreach (Node child in node.Children)
            {
                if (!child.IsContainer) continue;

                ParDirectory directory = new ParDirectory()
                {
                    Name = child.Name,
                    Node = child,
                };
                if (child.Children.Count > 0)
                {
                    directory.Children = BuildParDirectoryList(child);
                }

                childrenDirectories.Add(directory);
            }
            return childrenDirectories;
        }


        /// <summary>
        /// Gets a <see cref="BitmapImage"/> that corresponds to the <paramref name="fileName"/> extension's associated icon.
        /// </summary>
        /// <returns>A <see cref="BitmapImage"/>.</returns>
        private BitmapImage GetDataGridEntryIcon(string fileName, bool isDirectory = false)
        {
            //Folders
            if (isDirectory)
            {
                Uri uri = new Uri("pack://application:,,,/Resources/Images/FolderClosed.png", UriKind.RelativeOrAbsolute);
                BitmapImage bmp = new BitmapImage(uri);
                return bmp;
            }

            string extension = Path.GetExtension(fileName);

            //Files without extensions
            if (extension == string.Empty)
            {
                Uri uri = new Uri("pack://application:,,,/Resources/Images/File.png", UriKind.RelativeOrAbsolute);
                BitmapImage bmp = new BitmapImage(uri);
                return bmp;
            }

            //Check cache. If not present, create dummy file and get the associated icon
            if (FileIconCache.ContainsKey(extension))
            {
                return FileIconCache[extension];
            }
            else
            {
                string tempFilePath = Path.Combine(Settings.PATH_TEMP_ICONS, $"{extension}");
                File.Create(tempFilePath).Dispose();
                Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(tempFilePath);
                BitmapImage bmp = icon.ToBitmap().ToBitmapImage();
                FileIconCache.TryAdd(extension, bmp);
                return bmp;
            }
        }


        /// <summary>
        /// Update the state of the Navigation Toolbar buttons.
        /// </summary>
        private void UpdateNavigationToolbar()
        {
            btn_Navigation_DirectoryUp.IsEnabled = (NavigationHistoryCurrent != null && NavigationHistoryCurrent.Parent != null && NavigationHistoryCurrent.Name != ".") ? true : false;
            btn_Navigation_Previous.IsEnabled = (NavigationHistoryPrevious.Count > 0) ? true : false;
            btn_Navigation_Next.IsEnabled = (NavigationHistoryNext.Count > 0) ? true : false;
        }


        /// <summary>
        /// Click event for the File Open menu or toolbar items.
        /// </summary>
        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            if (PXDArchive != null)
            {
                PXDArchive.Dispose();
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PXD Archive (*.par)|*.par|" + "All types (*.*)|*.*";
            Nullable<bool> result = openFileDialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    // Open file 
                    string filePath = openFileDialog.FileName;
                    OpenPAR(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error has occurred when opening the file.\nThe exception message is:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        /// <summary>
        /// Double click event for DataGrid cells (file and folder names).
        /// </summary>
        private void DataGridCell_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DataGridCell cell = (DataGridCell)sender;
            ParEntry parEntry = (ParEntry)cell.DataContext;
            if (parEntry.Node.IsContainer)
            {
                OpenDirectory(parEntry.Node);
            }
            else
            {
                //TODO handle files
            }
        }


        /// <summary>
        /// Mouse click event for TreeView items (folders). Will change the DataGrid's contents to those of the selected folder.
        /// </summary>
        private void TreeViewItem_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = (TreeViewItem)sender;
            if (treeViewItem.IsSelected)
            {
                ParDirectory parDirectory = (ParDirectory)treeViewItem.DataContext;
                OpenDirectory(parDirectory.Node);
            }
        }


        /// <summary>
        /// Removes the overflow arrow from a <see cref="ToolBar"/> when inside a <see cref="ToolBarTray"/>.
        /// </summary>
        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }

            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness(0);
            }
        }


        /// <summary>
        /// Mouse click event for the Navigation (Previous) button. Will navigate to the previously visited directory.
        /// </summary>
        private void btn_Navigation_Previous_Click(object sender, RoutedEventArgs e)
        {
            Node previousDirectory = NavigationHistoryPrevious.Pop();
            OpenDirectory(previousDirectory, true);
        }


        /// <summary>
        /// Mouse click event for the Navigation (Next) button. Will navigate to the next directory if the Navigation (Previous) button has been used.
        /// </summary>
        private void btn_Navigation_Next_Click(object sender, RoutedEventArgs e)
        {
            Node nextDirectory = NavigationHistoryNext.Pop();
            OpenDirectory(nextDirectory);
        }


        /// <summary>
        /// Mouse click event for the Navigation (Directory Up) button. Will navigate to the directory one level above the current one.
        /// </summary>
        private void btn_Navigation_DirectoryUp_Click(object sender, RoutedEventArgs e)
        {
            OpenDirectory(NavigationHistoryCurrent.Parent);
        }
    }


    /// <summary>
    /// Class for displaying the archive's contents (DataGrid).
    /// </summary>
    public class ParEntry
    {
        public ParEntry() { }

        public Node Node;

        public BitmapImage Icon { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Size { get; set; }
        public string CompressedSize { get; set; }
        public string Ratio { get; set; }
        public DateTime Time { get; set; }
        public string Directory { get; set; }
    }


    /// <summary>
    /// Class for displaying the archive's directory structure (TreeView).
    /// </summary>
    public class ParDirectory
    {
        public ParDirectory() { }

        /// <summary>
        /// Directory name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Node corresponding to this directory.
        /// </summary>
        public Node Node { get; set; }

        /// <summary>
        /// Directories inside this one.
        /// </summary>
        public ObservableCollection<ParDirectory> Children { get; set; }

        /// <summary>
        /// Binded to the <see cref="TreeViewItem"/>'s IsExpanded property.
        /// </summary>
        private bool isExpanded = false;
        public bool IsExpanded {
            get
            {
                return isExpanded;
            }
            set
            { // Prevent folders without subdirectories from displaying the expanded icon
                if (Children != null && Children.Count > 0) isExpanded = value;
            } 
        }
    }
}