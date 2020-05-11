Public Class StartUpTreeNode
    Inherits TreeViewItem
    Private _maptreeview As OpenGLMapping.MapTreeView
    Private _statusbar As OpenGLMapping.MapStatusBar
    Private _MainWindow As Window
    Private _maincontextmenu As Menu
    Public Event RemoveMe()
    Public Event AddStudyTreeNode(ByVal studytreenode As StudyTreeNode)
    Sub New(ByVal mapwindow As OpenGLMapping.MapTreeView, ByVal statusbar As OpenGLMapping.MapStatusBar, ByVal maincontextmenu As Menu, ByVal mainwindow As Window)
        _maptreeview = mapwindow
        _statusbar = statusbar
        _MainWindow = mainwindow
        _maincontextmenu = maincontextmenu
        SetHeader = "No Study"
        Dim c As New ContextMenu

        Dim NewStudy As New MenuItem()
        NewStudy.Header = "Create New Study"
        AddHandler NewStudy.Click, AddressOf CreateNewStudy
        c.Items.Add(NewStudy)

        Dim ExistingStudy As New MenuItem()
        ExistingStudy.Header = "Open Existing Study"
        AddHandler ExistingStudy.Click, AddressOf OpenExistingStudy
        c.Items.Add(ExistingStudy)

        MyBase.ContextMenu = c


        Dim filemenu As MenuItem = CType(maincontextmenu.Items(0), MenuItem)

        Dim CreateNew As MenuItem = CType(filemenu.Items(0), MenuItem)
        Dim openexisting As MenuItem = CType(filemenu.Items(1), MenuItem)

        AddHandler CreateNew.Click, AddressOf CreateNewStudy
        AddHandler openexisting.Click, AddressOf OpenExistingStudy

    End Sub
    Public WriteOnly Property SetHeader As String
        Set(value As String)
            MyBase.Header = value
        End Set
    End Property
    Public Property Mapcontainerobject As OpenGLMapping.MapTreeView
        Get
            Return _maptreeview
        End Get
        Set(value As OpenGLMapping.MapTreeView)
            _maptreeview = value
        End Set
    End Property
    Sub CreateNewStudy(sender As Object, e As System.Windows.RoutedEventArgs)
        'Dim zw As New ZipWizard()
        'zw.Show()
        Dim cns As New CreateNewStudy
        cns.Owner = _MainWindow
        cns.ShowDialog()
        If cns.UserPressedOk Then
            Do Until _maptreeview.Items.Count = 0
                CType(_maptreeview.Items(_maptreeview.Items.Count - 1), OpenGLMapping.FeatureNode).RemoveLayer()
            Loop
            FrameworkTreeNode.SetRootDirectory = cns.StudyPath & "\" & cns.StudyName & "\"
            FrameworkTreeNode.SetMapTreeView = _maptreeview
            FrameworkTreeNode.SetMainContextMenu = _maincontextmenu
            FrameworkTreeNode.SetMainWindow = _MainWindow
            Dim s As New StudyTreeNode(cns.StudyName)
            RaiseEvent AddStudyTreeNode(s)
            s.AddFrameworkChildren()
            RaiseEvent RemoveMe()
        Else
            'its not ok!
        End If
    End Sub
    Sub OpenExistingStudy(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim fd As New Microsoft.Win32.OpenFileDialog
        With fd
            .Filter = "GeoFDA files (*.GeoFDA*) |*.GeoFDA*"
            .Multiselect = False
            .CheckFileExists = True
            .Title = "Please browse to an existing GeoFDA study File"
            .ShowDialog()
        End With
        If fd.FileName <> "" Then
            ''build an xdoc and send off the root.
            Do Until _maptreeview.Items.Count = 0
                CType(_maptreeview.Items(_maptreeview.Items.Count - 1), OpenGLMapping.FeatureNode).RemoveLayer()
            Loop
            Dim ba As Byte() = System.IO.File.ReadAllBytes(fd.FileName)
            Dim ms As New System.IO.MemoryStream(ba)
            Dim xdoc As New XDocument
            xdoc = XDocument.Load(ms)
            ms.Dispose()
            Dim bamap As Byte() = System.IO.File.ReadAllBytes(System.IO.Path.GetDirectoryName(fd.FileName) & "\" & System.IO.Path.GetFileNameWithoutExtension(fd.FileName) & "MapProperties.xml")
            Dim msmap As New System.IO.MemoryStream(bamap)
            Dim xdocmap As New XDocument
            xdocmap = XDocument.Load(msmap)
            msmap.Dispose()
            Dim xel As XElement = xdoc.Root
            FrameworkTreeNode.SetRootDirectory = System.IO.Path.GetDirectoryName(fd.FileName) & "\" 'System.IO.Path.GetDirectoryName(xel.Attribute("Path"))
            FrameworkTreeNode.SetMapTreeView = _maptreeview
            FrameworkTreeNode.SetMainWindow = _MainWindow
            FrameworkTreeNode.SetMainContextMenu = _maincontextmenu
            _maptreeview.LoadFromXElement(xdocmap.Root, System.IO.Path.GetDirectoryName(fd.FileName)) 'System.IO.Path.GetDirectoryName(xel.Attribute("Path")))
            Dim s As New StudyTreeNode(xel)
            s.AddFrameworkChildren(xel)
            RaiseEvent AddStudyTreeNode(s)
            RaiseEvent RemoveMe()
        Else
            MsgBox("No Selection was Made")
        End If
    End Sub
End Class
