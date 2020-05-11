Class MainWindow 
    Private _maintreenode As StartUpTreeNode
    Private _StudyTreeNode As StudyTreeNode
    Private _maptreeview As OpenGLMapping.MapTreeView
    Private _MapStatusBar As OpenGLMapping.MapStatusBar
    'Private _RecentStudiesFile As String
    Private Sub MainWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles Me.Closing
        CheckForSave(_StudyTreeNode, Nothing)
        GDALAssist.GDALSetup.Dispose()
        Application.Current.Shutdown()
    End Sub
    Private Sub MainWindow_Loaded(sender As Object, e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        Try
            Environment.SetEnvironmentVariable("GDAL_TIFF_OVR_BLOCKSIZE", "256")
            Dim dir As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)
            dir = New Uri(dir).LocalPath
            'Dim dir As String = System.IO.Directory.GetCurrentDirectory
            Dim ToolDir As String = dir & "\GDAL\bin"
            Dim DataDir As String = dir & "\GDAL\data"
            Dim PluginDir As String = dir & "\GDAL\bin\gdalplugins"
            Dim WMSDir As String = dir & "\GDAL\Web Map Services"
            GDALAssist.GDALSetup.Initialize(ToolDir, DataDir, PluginDir, WMSDir, True)
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try


        Dim MW As OpenGLMapping.OpenGLMapWindow
        MW = CType(WFH.Child, OpenGLMapping.OpenGLMapWindow)
        _maptreeview = New OpenGLMapping.MapTreeView
        MaptreeviewGRID.Children.Add(_maptreeview)
        MW.TreeView = _maptreeview

        _maptreeview.MapWindow = WFH.Child
        SelectableLayers.MapTree = _maptreeview
        SelectableLayers.MapWindow = _maptreeview.MapWindow
        Dim StatusBar As New OpenGLMapping.MapStatusBar(MW)
        _MapStatusBar = StatusBar
        Me.StatusBorder.Child = StatusBar

        BMTB.MapTree = _maptreeview
        BMTB.MapWindow = _maptreeview.MapWindow
        _maintreenode = New StartUpTreeNode(_maptreeview, _MapStatusBar, MainMenu, Me)
        StudyTreeView.Items.Add(_maintreenode)
        AddHandler _maintreenode.AddStudyTreeNode, AddressOf AddStudyTreeNode
        AddHandler _maintreenode.RemoveMe, AddressOf RemoveStartUpTreeNode

        CType(BMTB.Items(0), RadioButton).IsChecked = True
        AddHandler BMTB.RadioChecked, AddressOf MapToolbarRadioChecked
        '
        FeatureEditorToolbar.MapWindow = _maptreeview.MapWindow
        FeatureEditorToolbar.MapTree = _maptreeview
        AddHandler FeatureEditorToolbar.RadioChecked, AddressOf FeatureEditorRadioChecked
        '
        '_RecentStudiesFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\HEC\" & Me.GetType.Assembly.GetName.Name & "\RecentStudies.txt"
        'If Not System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\HEC\" & Me.GetType.Assembly.GetName.Name) Then System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\HEC\" & Me.GetType.Assembly.GetName.Name)
        'If System.IO.File.Exists(_RecentStudiesFile) Then
        '    'enable recent studies menu item
        PopulateRecentStudiesMenuItem()
        '    'list recent studies.
        'Else
        '    Dim fs As New System.IO.FileStream(_RecentStudiesFile, IO.FileMode.Create)
        '    Dim sw As New System.IO.StreamWriter(fs)
        '    sw.WriteLine("Recent Studies")
        '    sw.Close() : sw.Dispose()
        '    fs.Close() : fs.Dispose()
        'End If


    End Sub
    Private Sub MapToolbarRadioChecked(ButtonChecked As RadioButton)
        For Each item As Object In FeatureEditorToolbar.Items
            If item.GetType = GetType(RadioButton) Then CType(item, RadioButton).IsChecked = False
        Next
    End Sub
    Private Sub FeatureEditorRadioChecked(ButtonChecked As RadioButton)
        If IsNothing(ButtonChecked) Then
            BMTB.SelectChecked()
        Else
            For Each item As Object In BMTB.Items
                If item.GetType = GetType(RadioButton) Then CType(item, RadioButton).IsChecked = False
            Next
        End If
    End Sub
    Public Sub LoadFromFile(ByVal filepath As String)
        Dim ba As Byte() = System.IO.File.ReadAllBytes(filepath)
        Dim ms As New System.IO.MemoryStream(ba)
        Dim xdoc As New XDocument
        xdoc = XDocument.Load(ms)
        ms.Dispose()
        Dim bamap As Byte() = System.IO.File.ReadAllBytes(System.IO.Path.GetDirectoryName(filepath) & "\" & System.IO.Path.GetFileNameWithoutExtension(filepath) & "MapProperties.xml")
        Dim msmap As New System.IO.MemoryStream(bamap)
        Dim xdocmap As New XDocument
        xdocmap = XDocument.Load(msmap)
        msmap.Dispose()
        Dim xel As XElement = xdoc.Root
        FrameworkTreeNode.SetRootDirectory = System.IO.Path.GetDirectoryName(filepath) & "\" 'System.IO.Path.GetDirectoryName(xel.Attribute("Path"))
        FrameworkTreeNode.SetMapTreeView = _maptreeview
        FrameworkTreeNode.SetMainWindow = Me
        FrameworkTreeNode.SetMainContextMenu = MainMenu
        _maptreeview.LoadFromXElement(xdocmap.Root, System.IO.Path.GetDirectoryName(filepath)) 'System.IO.Path.GetDirectoryName(xel.Attribute("Path")))
        Dim s As New StudyTreeNode(xel)
        s.AddFrameworkChildren(xel)
        RemoveStartUpTreeNode()
        AddStudyTreeNode(s)
    End Sub
    Private Sub AddStudyTreeNode(ByVal studytreenode As StudyTreeNode)
        StudyTreeView.Items.Add(studytreenode)
        _StudyTreeNode = studytreenode

        UpdateRecentStudiesFile(_StudyTreeNode.GetProjectFile)
        'If System.IO.File.Exists(_RecentStudiesFile) Then
        '    'enable recent studies menu item
        PopulateRecentStudiesMenuItem()
        '    'list recent studies.
        'End If

        AddHandler SaveMenuItem.Click, AddressOf studytreenode.OnSaveEvent
        AddHandler SaveAsMenuItem.Click, AddressOf studytreenode.OnSaveAsEvent
        AddHandler CloseExistingMenuItem.Click, AddressOf studytreenode.OnCloseStudyEvent
        AddHandler _StudyTreeNode.AddOSM, AddressOf OpenStreetMap_Click
        AddHandler _StudyTreeNode.AddMQ, AddressOf MapQuest_Click
        AddHandler _StudyTreeNode.CloseStudy, AddressOf RemoveStudyTreeNode
        AddHandler LogDisplay.Click, AddressOf _StudyTreeNode.ViewLogFile
        OpenExistingMenuItem.IsEnabled = False
        CreateNewMenuItem.IsEnabled = False
        CloseExistingMenuItem.IsEnabled = True
        SaveAsMenuItem.IsEnabled = True
        SaveMenuItem.IsEnabled = True
    End Sub
    Private Sub RemoveStudyTreeNode(ByVal studytreenode As StudyTreeNode)
        CheckForSave(studytreenode, Nothing)
        Dim onsaveevent As New RoutedEventHandler(AddressOf studytreenode.OnSaveEvent)
        Dim onsaveas As New RoutedEventHandler(AddressOf studytreenode.OnSaveAsEvent)
        Dim onClose As New RoutedEventHandler(AddressOf studytreenode.OnCloseStudyEvent)
        RemoveHandler SaveMenuItem.Click, onsaveevent
        RemoveHandler SaveAsMenuItem.Click, onsaveas
        RemoveHandler CloseExistingMenuItem.Click, onClose
        RemoveHandler studytreenode.AddOSM, AddressOf OpenStreetMap_Click
        RemoveHandler studytreenode.AddMQ, AddressOf MapQuest_Click
        RemoveHandler studytreenode.CloseStudy, AddressOf RemoveStudyTreeNode
        RemoveHandler LogDisplay.Click, AddressOf _StudyTreeNode.ViewLogFile
        studytreenode.RemoveMainMenuContextOptions()
        StudyTreeView.Items.Remove(studytreenode)
        _StudyTreeNode = Nothing
        StudyTreeView.Items.Add(_maintreenode)
        'For Each fn As OpenGLMapping.FeatureNode In _maptreeview.MapWindow.GISFeatures
        '    fn.RemoveLayer()
        'Next
        Do Until _maptreeview.Items.Count = 0
            CType(_maptreeview.Items(_maptreeview.Items.Count - 1), OpenGLMapping.FeatureNode).RemoveLayer()
        Loop
        OpenExistingMenuItem.IsEnabled = True
        CreateNewMenuItem.IsEnabled = True
        CloseExistingMenuItem.IsEnabled = False
        SaveAsMenuItem.IsEnabled = False
        SaveMenuItem.IsEnabled = False
    End Sub
    Private Sub RemoveStartUpTreeNode()
        StudyTreeView.Items.Remove(_maintreenode)
    End Sub
    Private Sub CheckForSave(ByVal sender As Object, e As System.Windows.RoutedEventArgs)
        If IsNothing(sender) Then Exit Sub ' no study has been loaded.
        Dim sn As StudyTreeNode = TryCast(sender, StudyTreeNode)
        If IsNothing(sn) Then Exit Sub
        If sn.Saved Then 'check for saves.

        Else
            If MsgBox("Your study has unsaved changes, would you like to save?", MsgBoxStyle.YesNo, "Unsaved Changes Exist") = MsgBoxResult.Yes Then
                sn.OnSaveEvent(sn, e)
            Else
                'they said no! 'should i delete the files associated with unsaved nodes?
            End If
        End If
    End Sub
#Region "Main Menu Options"
    Private Sub ExitMenuItem_Click(sender As Object, e As System.Windows.RoutedEventArgs) Handles ExitMenuItem.Click
        Me.Close()
    End Sub
    Private Sub AddMapLayerItem_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles AddMapLayerItem.Click
        Dim fd As New Microsoft.Win32.OpenFileDialog
        With fd
            .Filter = "Shapefile (*.shp) |*.shp|GeoTiff (*.tif) | *.tif|Float Grid (*.flt)|*.flt|All files (*.*) |*.*"
            .FileName = ""
            .Multiselect = False
            .CheckFileExists = True
            .Title = "Please select a GIS File"
            .ShowDialog()
        End With
        If fd.FileName = "" Then
            MsgBox("No selection was made")
        Else
            _maptreeview.AddGISData(fd.FileName)
        End If
    End Sub
    Private Sub MapPropertiesItem_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles MapPropertiesItem.Click

        Dim MapPropertiesWindow As New OpenGLMapping.MapProperties(_maptreeview)
        MapPropertiesWindow.Owner = Me
        MapPropertiesWindow.Show()
    End Sub
    Private Sub OpenStreetMap_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles OpenStreetMap.Click
        Try
            Dim fn As New OpenGLMapping.FeatureNode(New OpenGLMapping.MapWebTiles(_maptreeview.MapWindow, OpenGLMapping.MapWebTiles.WebMapSource.OpenStreetMap), "OSM", _maptreeview.MapWindow)
            _maptreeview.AddGISData(fn)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Private Sub MapQuest_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles MapQuest.Click
        Try
            Dim fn As New OpenGLMapping.FeatureNode(New OpenGLMapping.MapWebTiles(_maptreeview.MapWindow, OpenGLMapping.MapWebTiles.WebMapSource.ESRIWorldImagery), "ESRI World Imagery", _maptreeview.MapWindow)
            _maptreeview.AddGISData(fn)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Private Sub About_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles LoadAbout.Click
        Dim a As New AboutWindow
        a.Owner = Me
        a.Show()
    End Sub
    Private Sub LoadQuickHelp_Click(sender As Object, e As RoutedEventArgs) Handles LoadQuickHelp.Click
        Dim hd As New QuickHelp.HelpDialog("GeoFDAHelp.xml", Me.GetType.Assembly.FullName, Me.GetType.Namespace)
        hd.Show()
    End Sub
    Private Sub QuickstartGuide_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles LoadQuickstart.Click
        MsgBox("Quickstart guide is not yet complete.")
    End Sub
#End Region
    Private Sub PopulateRecentStudiesMenuItem()
        RecentStudiesMenuItem.Items.Clear()
        Dim appname As String = System.Reflection.Assembly.GetExecutingAssembly().GetName.ToString()
        Dim companyname As String = My.Application.Info.CompanyName
        Dim subkey As String = companyname & "\" & appname
        Dim registrykey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(subkey)
        If IsNothing(registrykey) Then
            registrykey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(subkey)
        End If
        Dim registrynextline As String = ""
        Dim idx As Integer = 0
        Dim registrystudies As New List(Of String)
        Do Until idx = registrykey.ValueCount Or registrystudies.Count = 5
            registrynextline = registrykey.GetValue(idx)
            If System.IO.File.Exists(registrynextline) Then
                registrystudies.Add(registrynextline)
            End If
            idx += 1
        Loop
        If registrystudies.Count = 0 Then
        Else
            RecentStudiesMenuItem.IsEnabled = True
            For i = 0 To registrystudies.Count - 1
                Dim study As New MenuItem()
                study.Tag = registrystudies(i)
                study.ToolTip = registrystudies(i)
                Dim tb As New TextBlock
                tb.Text = System.IO.Path.GetFileNameWithoutExtension(registrystudies(i))
                study.Header = tb
                AddHandler study.Click, AddressOf OpenRecent
                RecentStudiesMenuItem.Items.Add(study)
            Next
        End If
    End Sub
    Private Sub UpdateRecentStudiesFile(ByVal filepath As String)
        Dim appname As String = System.Reflection.Assembly.GetExecutingAssembly().GetName.ToString()
        Dim companyname As String = My.Application.Info.CompanyName
        Dim subkey As String = companyname & "\" & appname
        Dim registrykey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(subkey, True)
        If IsNothing(registrykey) Then
            registrykey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(subkey, False)
        End If
        Dim registrystudies As New List(Of String)
        Dim registrynextline As String = ""
        Dim idx As Integer = 0
        Do Until idx = registrykey.ValueCount Or registrystudies.Count = 5
            registrynextline = registrykey.GetValue(idx)
            If System.IO.File.Exists(registrynextline) Then
                registrystudies.Add(registrynextline)
            End If
            idx += 1
        Loop
        If registrystudies.Count = 0 Then
            registrystudies.Add(filepath)
        Else
            If registrystudies.Contains(filepath) Then
                registrystudies.RemoveAt(registrystudies.IndexOf(filepath))
                registrystudies.Insert(0, filepath)
            Else
                registrystudies.Insert(0, filepath)
            End If
        End If
        For i = 0 To registrystudies.Count - 1
            registrykey.SetValue(i, registrystudies(i))
        Next
    End Sub
    Private Sub OpenRecent(sender As Object, e As RoutedEventArgs)
        If StudyTreeView.Items(0).GetType.Name = "StudyTreeNode" Then
            If DirectCast(DirectCast(sender, MenuItem).Header, TextBlock).Text = _StudyTreeNode.Header Then
                MsgBox("That project is currently open") : Exit Sub
            End If
            If MsgBox("A study is currently open, would you like to close it?", MsgBoxStyle.OkCancel) = MsgBoxResult.Ok Then
                RemoveStudyTreeNode(DirectCast(StudyTreeView.Items(0), StudyTreeNode))
            Else
                Exit Sub
            End If
        End If
        Dim mi As MenuItem = DirectCast(sender, MenuItem)
        LoadFromFile(mi.Tag)
    End Sub
End Class
