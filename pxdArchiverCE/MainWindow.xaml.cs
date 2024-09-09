﻿using ParLibrary.Converter;
using ParLibrary;
using System.Windows;
using Yarhl.FileSystem;
using Microsoft.Win32;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Collections.ObjectModel;
using Yarhl.IO;
using pxdArchiverCE.Controls;

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
        /// Path to the currently opened PARC file.
        /// </summary>
        string PXDArchivePath = string.Empty;

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
            Settings.Init();
            InitializeComponent();
        }


        /// <summary>
        /// Opens a PARC archive from a file.
        /// </summary>
        /// <param name="path">The path to the PARC file.</param>
        private void OpenPAR(string path)
        {
            try
            {
                if (PXDArchive != null)
                {
                    PXDArchive.Dispose();
                    PXDArchive = null;
                }

                var parameters = new ParArchiveReaderParameters
                {
                    Recursive = false
                };

                string parPath = path;

                if (Settings.CopyParToTempLocation)
                {
                    parPath = $"{Settings.PATH_APPDATA_SESSION}/par.tmp";
                    File.Delete(parPath);
                    File.Copy(path, parPath);
                }

                PXDArchivePath = path;

                PXDArchive = NodeFactory.FromFile(parPath, FileOpenMode.Read);
                PXDArchive.TransformWith(new ParArchiveReader(parameters));

                NavigationHistoryPrevious.Clear();
                NavigationHistoryNext.Clear();
                NavigationHistoryCurrent = null;

                OpenDirectory(PXDArchive);
                PopulateTreeView(PXDArchive);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error has occurred when opening the file.\nThe exception message is:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (PXDArchive != null)
                {
                    PXDArchive.Dispose();
                    PXDArchive = null;
                }
                PXDArchivePath = string.Empty;
                datagrid_ParContents.ItemsSource = null;
                treeview_ParFolders.ItemsSource = null;
                NavigationHistoryPrevious.Clear();
                NavigationHistoryNext.Clear();
                NavigationHistoryCurrent = null;
            }
        }


        /// <summary>
        /// Writes the currently opened PARC archive to a file.
        /// </summary>
        /// <param name="path">The path for the new PARC file.</param>
        private void SavePAR(string path)
        {
            try
            {
                var writerParameters = new ParArchiveWriterParameters
                {
                    CompressorVersion = 0x1,
                    OutputPath = path,
                    IncludeDots = !Settings.LegacyMode,
                };

                // Deep clone the node and write the clone so the file handle remains the same.
                Node temp = new Node(PXDArchive);
                File.Delete(path);
                temp.TransformWith(new ParArchiveWriter(writerParameters));
                temp.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error has occurred when saving the file.\nThe exception message is:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /// <summary>
        /// Open a directory and populate the DataGrid with its contents.
        /// </summary>
        /// <param name="node">The node container to use as the directory root.</param>
        /// <param name="isReturn">Is it returning to a previous directory?</param>
        private void OpenDirectory(Node node, bool isReturn = false)
        {
            if (node.Children.Count == 1 && node.Children[0].Name == ".")
            {
                OpenDirectory(node.Children[0], isReturn);
                return;
            }

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
        /// Extract a <see cref="Node"/> to a file in the session directory and attempt to open it with its default program.
        /// </summary>
        /// <param name="node">The node to open.</param>
        /// <param name="directory">The directory (inside the PARC) that file is in.</param>
        private void OpenFile(Node node, string directory)
        {
            string outPath = Path.Combine(Settings.PATH_APPDATA_SESSION_CONTENTS, directory, node.Name);
            if (ExtractFile(node, outPath))
            {
                Util.OpenFileWithDefaultProgram(outPath);
            }
        }


        /// <summary>
        /// Write a <see cref="Node"/> to a file.
        /// </summary>
        /// <param name="node">The node to write.</param>
        /// <param name="outputPath">The path to write to.</param>
        /// <returns>A <see cref="bool"/> indicating if the operation was successful.</returns>
        private bool ExtractFile(Node node, string outputPath)
        {
            if (node.IsContainer) return false;

            if (File.Exists(outputPath))
            {
                if (Util.IsFileLocked(outputPath))
                {
                    MessageBox.Show($"The file \"{Path.GetFileName(outputPath)}\" is currently in use by another process.", "File in use", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                else
                {
                    File.Delete(outputPath);
                }
            }

            var originalParFile = node.GetFormatAs<ParFile>();

            //Yarhl's deep clone does not properly clone formats and attributes so they need to be set manually
            Node nodeTemp = new Node(node).TransformWith(new ParFile());
            var parFile = nodeTemp.GetFormatAs<ParFile>();
            parFile.CanBeCompressed = originalParFile.CanBeCompressed;
            parFile.IsCompressed = originalParFile.IsCompressed;
            parFile.DecompressedSize = originalParFile.DecompressedSize;
            parFile.Attributes = originalParFile.Attributes;
            parFile.FileDate = originalParFile.FileDate;

            if (parFile.IsCompressed)
            {
                nodeTemp.TransformWith<ParLibrary.Sllz.Decompressor>();
            }

            nodeTemp.Stream.WriteTo(outputPath);
            File.SetCreationTime(outputPath, parFile.FileDate);
            File.SetLastWriteTime(outputPath, parFile.FileDate);
            File.SetAttributes(outputPath, (FileAttributes)parFile.Attributes);
            parFile.Dispose();
            nodeTemp.Dispose();

            return true;
        }


        /// <summary>
        /// Write the contents of a directory <see cref="Node"/> to the specified path.
        /// </summary>
        /// <param name="node">The node to extract.</param>
        /// <param name="outputPath">The directory to extract the contents to.</param>
        private void ExtractDirectory(Node node, string outputPath)
        {
            foreach (Node child in node.Children)
            {
                if (child.IsContainer)
                {
                    ExtractDirectory(child, Path.Combine(outputPath, child.Name));
                }
                else
                {
                    ExtractFile(child, Path.Combine(outputPath, child.Name));
                }
            }
        }


        /// <summary>
        /// Populates the DataGrid with the contents of the selected node, which will act as root of that directory.
        /// </summary>
        /// <param name="rootNode">The node to act as root of the directory.</param>
        private void PopulateDataGrid(Node rootNode)
        {
            if (rootNode == null) return;

            string directory = Util.GetNodeDirectory(rootNode);

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
                        Directory = directory,
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
            if (node.Children.Count == 1 && node.Children[0].Name == ".")
            {
                PopulateTreeView(node.Children[0]);
                return;
            }

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

            if (node.Children.Count == 1 && node.Children[0].Name == ".")
            {
                childrenDirectories = BuildParDirectoryList(node.Children[0]);
                return childrenDirectories;
            }

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
                string tempFilePath = Path.Combine(Settings.PATH_APPDATA_ICONS, $"{extension}");
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
        /// Gets a list of selected ParEntries from the DataGrid.
        /// </summary>
        /// <returns>A <see cref="ParEntry"/> list.</returns>
        private List<ParEntry> GetSelectedParEntries()
        {
            List<ParEntry> parEntries = new List<ParEntry>();
            foreach (DataGridCellInfo cellInfo in datagrid_ParContents.SelectedCells)
            {
                if (!parEntries.Contains(cellInfo.Item)) parEntries.Add((ParEntry)cellInfo.Item);
            }
            return parEntries;
        }


        /// <summary>
        /// Update the file date from the <see cref="ParEntry"/> with the specified <see cref="DateTime"/>.
        /// </summary>
        /// <param name="parEntry">The ParEntry.</param>
        /// <param name="date">The date to set.</param>
        private static void SetParEntryDate(ParEntry parEntry, DateTime date)
        {
            if (parEntry.Node.IsContainer) return;
            ParFile parFile = parEntry.Node.GetFormatAs<ParFile>();
            parFile.FileDate = date;
        }


        /// <summary>
        /// Window (app) closing event. Will clean up any temp files.
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (PXDArchive != null) PXDArchive.Dispose();
            Settings.Cleanup();
        }


        /// <summary>
        /// Click event for the File Open menu or toolbar items.
        /// </summary>
        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PXD Archive (*.par)|*.par|" + "All types (*.*)|*.*";
            Nullable<bool> result = openFileDialog.ShowDialog();
            if (result == true)
            {
                // Open file 
                string filePath = openFileDialog.FileName;
                OpenPAR(filePath);
            }
        }


        /// <summary>
        /// Click event for the File Save menu or toolbar items.
        /// </summary>
        private void FileSave_Click(object sender, RoutedEventArgs e)
        {
            if (PXDArchive == null) return;
            string outPath = PXDArchivePath;

            if (!Settings.CopyParToTempLocation)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PXD Archive (*.par)|*.par|" + "All types (*.*)|*.*";
                saveFileDialog.FileName = Path.GetFileName(outPath);
                if (saveFileDialog.ShowDialog() == true)
                {
                    outPath = saveFileDialog.FileName;
                    
                    // Cancel if attempting to save over the same file that is currently open.
                    if (outPath == PXDArchivePath)
                    {
                        MessageBox.Show("This PARC has not been opened from a temporary location.\nIt is not possible to overwrite the original file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else return;
            }

            SavePAR(outPath);
        }


        /// <summary>
        /// Click event for the File Save As... menu or toolbar items.
        /// </summary>
        private void FileSaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (PXDArchive == null) return;
            string outPath = PXDArchivePath;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PXD Archive (*.par)|*.par|" + "All types (*.*)|*.*";
            saveFileDialog.FileName = Path.GetFileName(outPath);
            if (saveFileDialog.ShowDialog() == true)
            {
                outPath = saveFileDialog.FileName;

                // Cancel if attempting to save over the same file that is currently open.
                if (outPath == PXDArchivePath && !Settings.CopyParToTempLocation)
                {
                    MessageBox.Show("This PARC has not been opened from a temporary location.\nIt is not possible to overwrite the original file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                SavePAR(outPath);
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
                OpenFile(parEntry.Node, parEntry.Directory);
            }
            e.Handled = true;
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


        /// <summary>
        /// File drop event for the Directory section of the UI.
        /// </summary>
        private void grid_ParDirectory_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                OpenPAR(files[0]);
            }
        }


        /// <summary>
        /// Load event for the Settings MenuItems. Will set the IsChecked property accordingly.
        /// </summary>
        private void mi_Settings_Loaded(object sender, RoutedEventArgs e)
        {
            mi_Settings_CopyParToTempLocation.IsChecked = Settings.CopyParToTempLocation;
            mi_Settings_LegacyMode.IsChecked = Settings.LegacyMode;
        }


        /// <summary>
        /// Click event for the Settings (CopyParToTempLocation) MenuItem. Will toggle the CopyParToTempLocation setting.
        /// </summary>
        private void mi_Settings_CopyParToTempLocation_Click(object sender, RoutedEventArgs e)
        {
            Settings.CopyParToTempLocation = !Settings.CopyParToTempLocation;
            mi_Settings_CopyParToTempLocation.IsChecked = Settings.CopyParToTempLocation;
            Settings.SaveSettings();
        }


        /// <summary>
        /// Click event for the Settings (LegacyMode) MenuItem. Will toggle the LegacyMode setting.
        /// </summary>
        private void mi_Settings_LegacyMode_Click(object sender, RoutedEventArgs e)
        {
            Settings.LegacyMode = !Settings.LegacyMode;
            mi_Settings_LegacyMode.IsChecked = Settings.LegacyMode;
            Settings.SaveSettings();
        }


        /// <summary>
        /// Click event for the DataGrid's ContextMenu MenuItem (Open). Will open the selected directory or file.
        /// </summary>
        private void datagrid_ParContents_ContextMenu_mi_Open_Click(object sender, RoutedEventArgs e)
        {
            List<ParEntry> parEntries = GetSelectedParEntries();
            if (parEntries.Count == 0) return;

            // Only process the last selection
            ParEntry selection = parEntries.Last();
            if (selection.Node.IsContainer)
            {
                OpenDirectory(selection.Node);
            }
            else
            {
                OpenFile(selection.Node, selection.Directory);
            }
        }


        /// <summary>
        /// Click event for the DataGrid's ContextMenu MenuItem (Extract). Will extract the selected directory or file.
        /// </summary>
        private void datagrid_ParContents_ContextMenu_mi_Extract_Click(object sender, RoutedEventArgs e)
        {
            List<ParEntry> parEntries = GetSelectedParEntries();
            if (parEntries.Count == 0) return;

            // Single file extraction
            if (parEntries.Count == 1 && !parEntries.Last().Node.IsContainer)
            {
                Node node = parEntries.Last().Node;
                string nodeExtension = Path.GetExtension(node.Name);

                SaveFileDialog saveFileDialog = new SaveFileDialog()
                {
                    Filter = $"{nodeExtension.ToUpper().Substring(1)} File (*{nodeExtension})|*{nodeExtension}|" + "All types (*.*)|*.*",
                    FileName = node.Name,
                    
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    ExtractFile(node, saveFileDialog.FileName);
                }
                else return;
            }
            // Multiple files or directories
            else
            {
                var openFolderDialog = new OpenFolderDialog
                {
                    Title = "Choose where to extract the content",
                };

                if (openFolderDialog.ShowDialog() == true)
                {
                    foreach (ParEntry parEntry in parEntries)
                    {
                        if (parEntry.Node.IsContainer)
                        {
                            ExtractDirectory(parEntry.Node, Path.Combine(openFolderDialog.FolderName, parEntry.Node.Name));
                        }
                        else
                        {
                            ExtractFile(parEntry.Node, Path.Combine(openFolderDialog.FolderName, parEntry.Node.Name));
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Click event for the DataGrid's ContextMenu MenuItem (touch > Reset Time). Will set the selected directory or file date to epoch 0.
        /// </summary>
        private void datagrid_ParContents_ContextMenu_mi_Touch_ResetTime_Click(object sender, RoutedEventArgs e)
        {
            List<ParEntry> parEntries = GetSelectedParEntries();
            DateTime resetTime = new DateTime(1970, 1, 1);

            foreach (ParEntry parEntry in parEntries)
            {
                SetParEntryDate(parEntry, resetTime);
            }
            // Reload directory with changes
            PopulateDataGrid(NavigationHistoryCurrent);
        }


        /// <summary>
        /// Click event for the DataGrid's ContextMenu MenuItem (touch > Current Time). Will set the selected directory or file date to the current date.
        /// </summary>
        private void datagrid_ParContents_ContextMenu_mi_Touch_CurrentTime_Click(object sender, RoutedEventArgs e)
        {
            List<ParEntry> parEntries = GetSelectedParEntries();
            DateTime currentTime = DateTime.Now;

            foreach (ParEntry parEntry in parEntries)
            {
                SetParEntryDate(parEntry, currentTime);
            }
            // Reload directory with changes
            PopulateDataGrid(NavigationHistoryCurrent);
        }


        /// <summary>
        /// Click event for the DataGrid's ContextMenu MenuItem (touch > Reset Time). Will set the selected directory or file date to a user defined date.
        /// </summary>
        private void datagrid_ParContents_ContextMenu_mi_Touch_SetTime_Click(object sender, RoutedEventArgs e)
        {
            List<ParEntry> parEntries = GetSelectedParEntries();
            bool isBulk = false;
            DateTime bulkDate = DateTime.Now;

            foreach (ParEntry parEntry in parEntries)
            {
                if (parEntry.Node.IsContainer) continue;
                if (isBulk)
                {
                    SetParEntryDate(parEntry, bulkDate);
                    continue;
                }
                TouchSetTimeDialog touchSetTimeDialog = new TouchSetTimeDialog(parEntry.Name, parEntry.Time);
                if (touchSetTimeDialog.ShowDialog() == true)
                {
                    if (touchSetTimeDialog.IsIgnore) continue;
                    SetParEntryDate(parEntry, touchSetTimeDialog.NewDate);
                    isBulk = touchSetTimeDialog.IsBulk;
                    bulkDate = touchSetTimeDialog.NewDate;
                }
            }
            // Reload directory with changes
            PopulateDataGrid(NavigationHistoryCurrent);
        }


        #region ItemSelection

        /// <summary>
        /// Is the mouse left button down? 
        /// </summary>
        bool mouseLeftButtonDown = false;

        /// <summary>
        /// The point where the left mouse button was first clicked.
        /// </summary>
        System.Windows.Point mouseLeftButtonDownPos;

        /// <summary>
        /// Is the par content's DataGrid scrollviewer scrolling up? (By cursor proximity to the upper edge).
        /// </summary>
        bool isScrollingUp = false;

        /// <summary>
        /// Is the par content DataGrid's scrollviewer scrolling down? (By cursor proximity to the lower edge).
        /// </summary>
        bool isScrollingDown = false;

        /// <summary>
        /// Direction of the last scrollviewer scroll.
        /// </summary>
        ScrollDirection lastScrollDirection = ScrollDirection.NONE;

        /// <summary>
        /// The <see cref="ScrollViewer"/> of the par content DataGrid.
        /// </summary>
        ScrollViewer scrollViewer;


        /// <summary>
        /// Mouse left button down event for the DataGrid. Will prepare and start drawing the selection box.
        /// </summary>
        private void datagrid_ParContents_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Capture and track the mouse
            mouseLeftButtonDown = true;
            mouseLeftButtonDownPos = e.GetPosition(datagrid_ParContents);
            datagrid_ParContents.CaptureMouse();

            // Initial placement of the selection box
            Canvas.SetLeft(rectangle_SelectionBox, mouseLeftButtonDownPos.X);
            Canvas.SetTop(rectangle_SelectionBox, mouseLeftButtonDownPos.Y);
            rectangle_SelectionBox.Width = 0;
            rectangle_SelectionBox.Height = 0;

            // Make the selection box visible
            rectangle_SelectionBox.Visibility = Visibility.Visible;

            // Clear the selected cells
            datagrid_ParContents.SelectedCells.Clear();
        }


        /// <summary>
        /// Mouse left button up event for the DataGrid. Will reset and hide the selection box.
        /// </summary>
        private void datagrid_ParContents_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Release the mouse capture and stop tracking it
            mouseLeftButtonDown = false;
            datagrid_ParContents.ReleaseMouseCapture();

            // Reset the selection box and hide it
            Canvas.SetLeft(rectangle_SelectionBox, 0);
            Canvas.SetTop(rectangle_SelectionBox, 0);
            rectangle_SelectionBox.Width = 0;
            rectangle_SelectionBox.Height = 0;
            rectangle_SelectionBox.Visibility = Visibility.Collapsed;

            // Reset the scroll tracker
            lastScrollDirection = ScrollDirection.NONE;
        }


        /// <summary>
        /// Mouse move event for the DataGrid. Will resize the selection box based on the cursor's position and select/deselect the items underneath it.
        /// </summary>
        private void datagrid_ParContents_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Reposition the selection box if the button is held
            if (mouseLeftButtonDown)
            {
                System.Windows.Point mousePos = e.GetPosition(datagrid_ParContents);

                // Prevent the selection box from getting out of the DataGrid control bounds
                if (mousePos.X < 0) mousePos.X = 0;
                if (mousePos.Y < 0) mousePos.Y = 0;

                isScrollingUp = ((mousePos.Y - 25) <= 0);
                isScrollingDown = ((mousePos.Y + 25) >= datagrid_ParContents.ActualHeight);
                if (isScrollingUp) lastScrollDirection = ScrollDirection.UP;
                if (isScrollingDown) lastScrollDirection = ScrollDirection.DOWN;

                if (mouseLeftButtonDownPos.X < mousePos.X)
                {
                    Canvas.SetLeft(rectangle_SelectionBox, mouseLeftButtonDownPos.X);
                    rectangle_SelectionBox.Width = mousePos.X - mouseLeftButtonDownPos.X;
                }
                else
                {
                    Canvas.SetLeft(rectangle_SelectionBox, mousePos.X);
                    rectangle_SelectionBox.Width = mouseLeftButtonDownPos.X - mousePos.X;
                }

                if (mouseLeftButtonDownPos.Y < mousePos.Y)
                {
                    Canvas.SetTop(rectangle_SelectionBox, mouseLeftButtonDownPos.Y);
                    rectangle_SelectionBox.Height = mousePos.Y - mouseLeftButtonDownPos.Y;
                }
                else
                {
                    Canvas.SetTop(rectangle_SelectionBox, mousePos.Y);
                    rectangle_SelectionBox.Height = mouseLeftButtonDownPos.Y - mousePos.Y;
                }

                System.Windows.Point selectionBoxRelativePoint = rectangle_SelectionBox.TranslatePoint(new System.Windows.Point(0, 0), canvas_SelectionBox);
                Rect selectionBoxArea = new Rect(selectionBoxRelativePoint.X, selectionBoxRelativePoint.Y, rectangle_SelectionBox.Width, rectangle_SelectionBox.Height);

                // Has the current iteration reached an entry that is inside the selection box?
                bool hasReachedSelectionBox = false;
                // Has the current iteration reached an entry that is outside the selection box?
                bool hasExitedSelectionBox = false;
                for (int i = 0; i < datagrid_ParContents.Items.Count; i++)
                {
                    datagrid_ParContents.Focus();
                    DataGridCellInfo cellInfo = new DataGridCellInfo(datagrid_ParContents.Items[i], datagrid_ParContents.Columns[1]); // Get the Name cell (second column)
                    FrameworkElement cellContent = cellInfo.Column.GetCellContent(cellInfo.Item);

                    if (cellContent != null)
                    {
                        DataGridCell cell = cellContent.Parent as DataGridCell;
                        if (cell != null)
                        {
                            System.Windows.Point relativeCellLocation = cell.TranslatePoint(new System.Windows.Point(0, 0), datagrid_ParContents);
                            Rect cellBoundingBox = new Rect(relativeCellLocation.X, relativeCellLocation.Y, cell.ActualWidth, cell.ActualHeight);

                            // Select cells if they are inside the selection box
                            if (selectionBoxArea.IntersectsWith(cellBoundingBox))
                            {
                                hasReachedSelectionBox = true;
                                if (!datagrid_ParContents.SelectedCells.Contains(cellInfo))
                                {
                                    datagrid_ParContents.SelectedCells.Add(cellInfo);
                                }
                            }
                            // Deselect cells if they arent inside the selection box only if they havent been part of a scroll selection
                            else
                            {
                                if (hasReachedSelectionBox) hasExitedSelectionBox = true;
                                
                                if (isScrollingUp || lastScrollDirection == ScrollDirection.UP)
                                {
                                    if (!hasReachedSelectionBox && datagrid_ParContents.SelectedCells.Contains(cellInfo))
                                    {
                                        datagrid_ParContents.SelectedCells.Remove(cellInfo);
                                    }
                                }
                                else if (isScrollingDown || lastScrollDirection == ScrollDirection.DOWN)
                                {
                                    if (hasReachedSelectionBox && hasExitedSelectionBox && datagrid_ParContents.SelectedCells.Contains(cellInfo))
                                    {
                                        datagrid_ParContents.SelectedCells.Remove(cellInfo);
                                    }
                                }
                                else if (datagrid_ParContents.SelectedCells.Contains(cellInfo))
                                {
                                    datagrid_ParContents.SelectedCells.Remove(cellInfo);
                                }
                            }
                        }
                    }
                }

                // Scroll when the mouse approaches the edge
                if (isScrollingDown)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + 1.0);
                }
                else if (isScrollingUp)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - 1.0);
                }
            }
        }


        /// <summary>
        /// Loaded event for the par content DataGrid. Will locate the <see cref="ScrollViewer"/>.
        /// </summary>
        private void datagrid_ParContents_Loaded(object sender, RoutedEventArgs e)
        {
            scrollViewer = Util.FindVisualChild<ScrollViewer>(datagrid_ParContents);
        }

        #endregion
    }


    /// <summary>
    /// Direction the scroll was performed.
    /// </summary>
    enum ScrollDirection
    {
        NONE,
        UP,
        DOWN,
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