<Window x:Class="pxdArchiverCE.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:pxd="clr-namespace:pxdArchiverCE.Controls"
        xmlns:local="clr-namespace:pxdArchiverCE"
        mc:Ignorable="d"
        Title="PXD Archiver Community Edition" Height="800" Width="1400" PreviewKeyDown="Window_PreviewKeyDown" Closing="Window_Closing">

    <Window.Resources>
        <Style x:Key="ImageEnabled" TargetType="Image">
            <Style.Triggers>
                <Trigger Property="IsEnabled" Value="False">
                    <Setter Property="Opacity" Value="0.25"/>
                </Trigger>
            </Style.Triggers>
        </Style>
        <BooleanToVisibilityConverter x:Key="BooleanToVisibilityConverter"/>
    </Window.Resources>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="1*"/>
        </Grid.RowDefinitions>

        <Menu Grid.Row="0">
            <MenuItem Header="File">
                <MenuItem Header="New" Click="FileNew_Click"/>
                <MenuItem Header="Open" Click="FileOpen_Click"/>
                <MenuItem Header="Save" Click="FileSave_Click"/>
                <MenuItem Header="Save as..." Click="FileSaveAs_Click"/>
            </MenuItem>
            <MenuItem Header="Edit">
                <MenuItem x:Name="mi_Edit_Touch" Header="touch">
                    <MenuItem x:Name="mi_Edit_Touch_ResetTime" Header="Reset time" Click="mi_Edit_Touch_ResetTime_Click"/>
                    <MenuItem x:Name="mi_Edit_Touch_CurrentTime" Header="Current time" Click="mi_Edit_Touch_CurrentTime_Click"/>
                    <MenuItem x:Name="mi_Edit_Touch_SetTime" Header="Set time" Click="mi_Edit_Touch_SetTime_Click"/>
                </MenuItem>
            </MenuItem>
            <MenuItem Header="View" Visibility="Collapsed">
                <!-- TO BE IMPLEMENTED -->
            </MenuItem>
            <MenuItem Header="Settings">
                <MenuItem x:Name="mi_Settings_CopyParToTempLocation" Header="Copy PARC to temp directory" IsCheckable="True" Loaded="mi_Settings_Loaded" Click="mi_Settings_CopyParToTempLocation_Click"
                          ToolTip="Copy the PARC file to a temporary location.&#x0a;This may increase initial load times due to the copy process but will allow overwriting the original file."/>
                <MenuItem x:Name="mi_Settings_LegacyMode" Header="Legacy Mode" IsCheckable="True" Loaded="mi_Settings_Loaded" Click="mi_Settings_LegacyMode_Click"
                          ToolTip="Format PARC file directories to be compatible with older versions of the PXD Engine."/>
                <MenuItem x:Name="mi_Settings_AlternativeFileSorting" Header="Alternative File Sorting" IsCheckable="True" Loaded="mi_Settings_Loaded" Click="mi_Settings_AlternativeFileSorting_Click"
                          ToolTip="Alternative file and directory sorting. This option may be required by some old games such as Kenzan!."/>
                <MenuItem x:Name="mi_Settings_HandleNestedPar" Header="Handle nested PARC files" IsCheckable="True" Loaded="mi_Settings_Loaded" Click="mi_Settings_HandleNestedPar_Click"
                          ToolTip="Nested PARC files will be treated as folders."/>
                <MenuItem x:Name="mi_Settings_SizeDisplayUnit" Header="Size Display Unit" IsCheckable="False" ToolTip="Unit for representing file sizes.">
                    <MenuItem x:Name="mi_Settings_SizeDisplayUnitAuto" Header="Automatic" IsCheckable="True" Tag="AUTO" Loaded="mi_Settings_Loaded" Click="mi_Settings_SizeDisplayUnit_Click"/>
                    <MenuItem x:Name="mi_Settings_SizeDisplayUnitBytes" Header="Bytes" IsCheckable="True" Tag="BYTES" Loaded="mi_Settings_Loaded" Click="mi_Settings_SizeDisplayUnit_Click"/>
                </MenuItem>
                <Separator/>
                <MenuItem Header="Advanced">
                    <MenuItem x:Name="mi_Settings_Advanced_OpenWorkDir" Header="Open work directory" IsCheckable="False" Click="mi_Settings_Advanced_OpenWorkDir_Click"/>
                    <Separator/>
                    <MenuItem x:Name="mi_Settings_Advanced_Session" Header="Session: [ID]" IsCheckable="False" IsEnabled="False"/>
                    <MenuItem x:Name="mi_Settings_Advanced_OpenSessionDir" Header="Open session directory" IsCheckable="False" Click="mi_Settings_Advanced_OpenSessionDir_Click"/>
                </MenuItem>
            </MenuItem>
            <MenuItem Header="Help">
                <MenuItem x:Name="mi_Help_About" Header="About pxdArchiverCE" Click="mi_Help_About_Click">
                    <MenuItem.Icon>
                        <Image Source="/Resources/Icons/pxdArchiverCE/pxdArchiverCE_16.png" RenderOptions.BitmapScalingMode="NearestNeighbor"/>
                    </MenuItem.Icon>
                </MenuItem>
                <MenuItem x:Name="mi_Help_GitHub" Header="GitHub" Click="mi_Help_GitHub_Click">
                    <MenuItem.Icon>
                        <Image Source="/Resources/Icons/github.png" RenderOptions.BitmapScalingMode="HighQuality"/>
                    </MenuItem.Icon>
                </MenuItem>
                <MenuItem x:Name="mi_Help_Licenses" Header="Licenses" Click="mi_Help_Licenses_Click"/>
                <Separator/>
                <MenuItem x:Name="mi_Help_Donate" Header="Donate" Click="mi_Help_Donate_Click">
                    <MenuItem.Icon>
                        <Image Source="/Resources/Icons/ko-fi.png" RenderOptions.BitmapScalingMode="HighQuality"/>
                    </MenuItem.Icon>
                </MenuItem>
            </MenuItem>
        </Menu>

        <Grid Grid.Row="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="0.25*"/>
                <ColumnDefinition Width="0.75*"/>
            </Grid.ColumnDefinitions>

            <ToolBarTray Grid.Column="0" HorizontalAlignment="Stretch" IsLocked="True">
                <ToolBar ToolTip="General" Loaded="ToolBar_Loaded">
                    <Button ToolTip="New" Click="FileNew_Click" >
                        <Image Source="/Resources/Images/File.png" Width="16" Height="16"/>
                    </Button>
                    <Button ToolTip="Open" Click="FileOpen_Click">
                        <Image Source="/Resources/Images/Folder.png" Width="16" Height="16"/>
                    </Button>

                    <Button ToolTip="Save" Click="FileSave_Click">
                        <Image Source="/Resources/Images/Save.png" Width="16" Height="16"/>
                    </Button>
                </ToolBar>
            </ToolBarTray>
            <ToolBarTray Grid.Column="1" HorizontalAlignment="Stretch" IsLocked="True">
                <ToolBar ToolTip="Navigation" Loaded="ToolBar_Loaded">
                    <Button x:Name="btn_Navigation_Previous" ToolTip="Previous" IsEnabled="False" Click="btn_Navigation_Previous_Click">
                        <Image Source="/Resources/Images/Previous.png" Width="16" Height="16" Style="{StaticResource ImageEnabled}"/>
                    </Button>
                    <Button x:Name="btn_Navigation_Next" ToolTip="Next" IsEnabled="False" Click="btn_Navigation_Next_Click">
                        <Image Source="/Resources/Images/Next.png" Width="16" Height="16" Style="{StaticResource ImageEnabled}"/>
                    </Button>
                    <Button x:Name="btn_Navigation_DirectoryUp" ToolTip="Directory Up" IsEnabled="False" Click="btn_Navigation_DirectoryUp_Click">
                        <Image Source="/Resources/Images/OneLevelUp.png" Width="16" Height="16" Style="{StaticResource ImageEnabled}"/>
                    </Button>
                    
                </ToolBar>
            </ToolBarTray>
            <ToolBar Grid.Column="1" ToolBarTray.IsLocked="True" ToolTip="Search" HorizontalAlignment="Right" Margin="0,0,20,0" Loaded="ToolBar_Loaded">
                <TextBox x:Name="tb_Navigation_Input" AllowDrop="True" Text="" RenderTransformOrigin="0.5,0.5" Width="177" Height="25" HorizontalAlignment="Left" KeyDown="tb_Search_KeyDown">
                    <TextBox.Style>
                        <Style TargetType="TextBox" BasedOn="{StaticResource {x:Static ToolBar.TextBoxStyleKey}}">
                            <Setter Property="Template">
                                <Setter.Value>
                                    <ControlTemplate TargetType="TextBox">
                                        <Border BorderThickness="{TemplateBinding Border.BorderThickness}"
                                                BorderBrush="{TemplateBinding Border.BorderBrush}"
                                                Background="{TemplateBinding Background}">
                                            <Grid>
                                                <ScrollViewer x:Name="PART_ContentHost" />
                                                <TextBlock HorizontalAlignment="Left"
                                                           VerticalAlignment="Center"
                                                           FontStyle="Italic"
                                                           Foreground="#999999"
                                                           Visibility="{Binding RelativeSource={RelativeSource TemplatedParent}, 
                                                           Path=Text.IsEmpty, 
                                                           Converter={StaticResource BooleanToVisibilityConverter}}"
                                                           Text="  Search"/>
                                            </Grid>
                                        </Border>
                                    </ControlTemplate>
                                </Setter.Value>
                            </Setter>
                        </Style>
                    </TextBox.Style>
                </TextBox>
                <Button x:Name="btn_Navigation_Search" ToolTip="Search" Click="btn_Navigation_Search_Click" BorderBrush="#00707070" Background="#00DDDDDD" HorizontalAlignment="Left" Width="22">
                    <Image Source="Resources/Images/Search.png" Width="16" Height="16"/>
                </Button>
            </ToolBar>
        </Grid>


        <Grid Grid.Row="2">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="0.25*"/>
                <ColumnDefinition Width="0.75*"/>
            </Grid.ColumnDefinitions>

            <Grid x:Name="grid_ParDirectory" Grid.Column="0" AllowDrop="True" Drop="grid_ParDirectory_Drop">
                <TreeView x:Name="treeview_ParFolders" VirtualizingPanel.IsVirtualizing="True" VirtualizingPanel.VirtualizationMode="Recycling">
                    <TreeView.Resources>
                        <Style TargetType="{x:Type TreeViewItem}">
                            <Setter Property="IsExpanded" Value="{Binding IsExpanded, Mode=TwoWay}"/>
                            <EventSetter Event="MouseLeftButtonUp" Handler="TreeViewItem_MouseLeftButtonUp"/>
                            <EventSetter Event="Drop" Handler="TreeViewItem_Drop"/>
                            <EventSetter Event="DragEnter" Handler="TreeViewItem_DragEnter"/>
                            <EventSetter Event="DragLeave" Handler="TreeViewItem_DragLeave"/>
                        </Style>
                        <HierarchicalDataTemplate DataType="{x:Type local:ParDirectory}" ItemsSource="{Binding Children}">
                            <StackPanel Orientation="Horizontal">
                                <Image x:Name="treeviewitem_FolderIcon" Source="/Resources/Images/FolderClosed.png" Margin="0,0,5,0" Width="16" Height="16"/>
                                <TextBlock Text="{Binding Name}"/>
                            </StackPanel>
                            <HierarchicalDataTemplate.Triggers>
                                <DataTrigger Binding="{Binding IsExpanded}" Value="True">
                                    <Setter TargetName="treeviewitem_FolderIcon" Property="Source" Value="/Resources/Images/FolderOpen.png"/>
                                </DataTrigger>
                            </HierarchicalDataTemplate.Triggers>
                        </HierarchicalDataTemplate>
                    </TreeView.Resources>
                </TreeView>
            </Grid>

            <GridSplitter Grid.Column="0" Width="5"/>

            
            <Grid Grid.Column="1" x:Name="grid_ParContentsParent">
                <pxd:PxdDataGrid x:Name="datagrid_ParContents" Background="White" EnableRowVirtualization="True" EnableColumnVirtualization="True" GridLinesVisibility="None" HeadersVisibility="Column" SelectionUnit="Cell"
                       IsReadOnly="True" AutoGenerateColumns="False" ScrollViewer.CanContentScroll="True" HorizontalScrollBarVisibility="Auto" ScrollViewer.VerticalScrollBarVisibility="Visible"
                      CanUserResizeRows="False" CanUserReorderColumns="False" SelectionMode="Extended" AllowDrop="True" Drop="datagrid_ParContents_Drop" MouseLeftButtonDown="datagrid_ParContents_MouseLeftButtonDown" MouseLeftButtonUp="datagrid_ParContents_MouseLeftButtonUp" 
                      MouseMove="datagrid_ParContents_MouseMove" Loaded="datagrid_ParContents_Loaded">
                    <DataGrid.Resources>
                        <Style TargetType="{x:Type DataGridCell}">
                            <Setter Property="BorderThickness" Value="0"/>
                            <Setter Property="FocusVisualStyle" Value="{x:Null}"/>
                            <Setter Property="Focusable" Value="False"/>
                            <Setter Property="IsTabStop" Value="False"/>
                            <Setter Property="IsHitTestVisible" Value="False"/>
                            <Setter Property="Margin" Value="10,0,30,0"/>
                        </Style>
                        <Style x:Key="NoMargin" TargetType="{x:Type DataGridCell}">
                            <Setter Property="BorderThickness" Value="0"/>
                            <Setter Property="FocusVisualStyle" Value="{x:Null}"/>
                            <Setter Property="Focusable" Value="False"/>
                            <Setter Property="IsTabStop" Value="False"/>
                            <Setter Property="IsHitTestVisible" Value="False"/>
                            <Setter Property="Margin" Value="0"/>
                        </Style>
                        <ContextMenu  x:Key="SelectableColumnContextMenu" DataContext="{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}">
                            <MenuItem x:Name="datagrid_ParContents_ContextMenu_mi_Open" Header="Open" FontWeight="Bold" Click="datagrid_ParContents_ContextMenu_mi_Open_Click"/>
                            <MenuItem x:Name="datagrid_ParContents_ContextMenu_mi_Extract" Header="Extract" Click="datagrid_ParContents_ContextMenu_mi_Extract_Click"/>
                            <MenuItem x:Name="datagrid_ParContents_ContextMenu_mi_Compression" Header="Compression">
                                <MenuItem x:Name="datagrid_ParContents_ContextMenu_mi_Compression_None" Header="None" Click="datagrid_ParContents_ContextMenu_mi_Compression_None_Click"/>
                                <MenuItem x:Name="datagrid_ParContents_ContextMenu_mi_Compression_Normal" Header="Normal" Click="datagrid_ParContents_ContextMenu_mi_Compression_Normal_Click"/>
                                <MenuItem x:Name="datagrid_ParContents_ContextMenu_mi_Compression_High" Header="High" Click="datagrid_ParContents_ContextMenu_mi_Compression_High_Click"/>
                            </MenuItem>
                            <MenuItem x:Name="datagrid_ParContents_ContextMenu_mi_Touch" Header="touch">
                                <MenuItem x:Name="datagrid_ParContents_ContextMenu_mi_Touch_ResetTime" Header="Reset time" Click="datagrid_ParContents_ContextMenu_mi_Touch_ResetTime_Click"/>
                                <MenuItem x:Name="datagrid_ParContents_ContextMenu_mi_Touch_CurrentTime" Header="Current time" Click="datagrid_ParContents_ContextMenu_mi_Touch_CurrentTime_Click"/>
                                <MenuItem x:Name="datagrid_ParContents_ContextMenu_mi_Touch_SetTime" Header="Set time" Click="datagrid_ParContents_ContextMenu_mi_Touch_SetTime_Click"/>
                            </MenuItem>
                            <MenuItem x:Name="datagrid_ParContents_ContextMenu_mi_Rename" Header="Rename" Click="datagrid_ParContents_ContextMenu_mi_Rename_Click"/>
                            <Separator/>
                            <MenuItem x:Name="datagrid_ParContents_ContextMenu_mi_Delete" Header="Delete" Click="datagrid_ParContents_ContextMenu_mi_Delete_Click"/>
                        </ContextMenu>
                        <Style x:Key="Selectable" TargetType="{x:Type DataGridCell}">
                            <Setter Property="BorderThickness" Value="0"/>
                            <Setter Property="FocusVisualStyle" Value="{x:Null}"/>
                            <Setter Property="Focusable" Value="True"/>
                            <Setter Property="IsTabStop" Value="True"/>
                            <Setter Property="IsHitTestVisible" Value="True"/>
                            <Setter Property="Margin" Value="10,0,30,0"/>
                            <Setter Property="AllowDrop" Value="True"/>
                            <Setter Property="ContextMenu" Value="{StaticResource SelectableColumnContextMenu}"/>
                            <EventSetter Event="MouseDoubleClick" Handler="DataGridCell_MouseDoubleClick"/>
                            <EventSetter Event="PreviewMouseLeftButtonDown" Handler="DataGridCell_MouseLeftButtonDown"/>
                            <EventSetter Event="MouseLeftButtonUp" Handler="DataGridCell_MouseLeftButtonUp"/>
                            <EventSetter Event="Drop" Handler="DataGridCell_Drop"/>
                            <EventSetter Event="DragEnter" Handler="DataGridCell_DragEnter"/>
                            <EventSetter Event="DragLeave" Handler="DataGridCell_DragLeave"/>
                        </Style>
                    </DataGrid.Resources>
                    
                    <DataGrid.Columns>
                        <DataGridTemplateColumn Header="" Width="SizeToCells" CanUserSort="False" CanUserResize="False" CellStyle="{StaticResource NoMargin}">
                            <DataGridTemplateColumn.CellTemplate>
                                <DataTemplate>
                                    <Image Source="{Binding Icon}" Height="16" Width="16" HorizontalAlignment="Right" VerticalAlignment="Center" RenderOptions.BitmapScalingMode="Linear"/>
                                </DataTemplate>
                            </DataGridTemplateColumn.CellTemplate>
                        </DataGridTemplateColumn>
                        <DataGridTextColumn Header="Name" Binding="{Binding Name}" CellStyle="{StaticResource Selectable}"/>
                        <DataGridTextColumn Header="Type" Binding="{Binding Type}"/>
                        <DataGridTextColumn Header="Size" Binding="{Binding Size}" CanUserSort="False"/>
                        <DataGridTextColumn Header="Compressed Size" Binding="{Binding CompressedSize}" CanUserSort="False"/>
                        <DataGridTextColumn Header="Ratio" Binding="{Binding Ratio}" CanUserSort="False"/>
                        <DataGridTextColumn Header="Time" Binding="{Binding Time, StringFormat=\{0:dd/MM/yyyy HH:mm:ss\}}"/>
                        <DataGridTextColumn Header="Directory" Binding="{Binding Directory}" CanUserSort="False"/>
                    </DataGrid.Columns>
                </pxd:PxdDataGrid>
                
                <!-- Click-through canvas overlay to render the selection box -->
                <Canvas x:Name="canvas_SelectionBox" IsHitTestVisible="False">
                    <Rectangle x:Name="rectangle_SelectionBox" Visibility="Collapsed" Stroke="Black" StrokeThickness="1.5" StrokeDashArray="1,1" Fill="#3049a6f2"/>
                </Canvas>
            </Grid>
        </Grid>

    </Grid>
</Window>
