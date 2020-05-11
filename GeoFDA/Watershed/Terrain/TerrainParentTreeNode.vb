Public Class TerrainParentTreeNode
    Inherits FrameworkTreeNode
    Private _directorypath As String
    'Private Const _ParentFolder As String = "Watershed"
    Private Const _FolderName As String = "Terrain"
    Private Const _header As String = "Terrain"
    Sub New()
        MyBase.New(_header)
    End Sub
    Sub New(ByVal header As String)
        MyBase.New(header)
    End Sub
    Public Overrides ReadOnly Property GetSubDirectory As String
        Get
            Return _FolderName '_ParentFolder & "\" & _FolderName
        End Get
    End Property
    Public Overrides ReadOnly Property GetNodeName As String
        Get
            Return _FolderName
        End Get
    End Property
    Public Overrides Sub SetContextMenu()
        Dim c As New ContextMenu

        Dim AddTerrainData As New MenuItem()
        AddTerrainData.Header = "Import Terrain"
        AddHandler AddTerrainData.Click, AddressOf AddTerrain
        c.Items.Add(AddTerrainData)

        Dim AddTerrainDataMain As New MenuItem()
        AddTerrainDataMain.Header = "Import Terrain"
        AddHandler AddTerrainDataMain.Click, AddressOf AddTerrain

        For Each mi As MenuItem In GetMainContextMenu.Items
            If mi.Header = "_Study" Then
                Dim tm As New MenuItem()
                tm.Header = "Terrain"
                tm.Items.Add(AddTerrainDataMain)
                mi.Items.Add(tm)
            End If
        Next

        MyBase.ContextMenu = c
    End Sub
    Sub AddTerrain(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim terrainbrowser As New TerrainBrowser()
        terrainbrowser.Owner = GetMainWindow
        If terrainbrowser.ShowDialog() Then
            'ok
            'check if name already exists.
            'copy file over with new name.
            Dim fnodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(TerrainChildTreeNode))
            If fnodes.Count = 0 Then
            Else
                Dim tnodes As New List(Of String)
                For i = 0 To fnodes.Count - 1
                    tnodes.Add(CType(fnodes(i), TerrainChildTreeNode).Header)
                Next
                Dim nonameconflicts As Boolean = False
                Do Until nonameconflicts
                    If tnodes.Contains(terrainbrowser.TxtName.Text) Then
                        MsgBox("Name already exists, please select a different name")
                        terrainbrowser = New TerrainBrowser(terrainbrowser.TxtName.Text, terrainbrowser.TerrainPathBrowser.Path)
                        terrainbrowser.Owner = GetMainWindow
                        If terrainbrowser.ShowDialog Then
                        Else
                            MsgBox("User aborted") : Exit Sub
                        End If
                    Else
                        nonameconflicts = True
                    End If
                Loop
            End If
            Dim tc As New TerrainChildTreeNode(terrainbrowser.TerrainPathBrowser.Path, terrainbrowser.TxtName.Text)
            MyBase.AddFrameworkTreeNode(tc)
            CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
        Else
            'not ok.
            MsgBox("No selection was made")
        End If

    End Sub
    Public Overrides Sub OnSaveEvent(sender As Object, e As System.Windows.RoutedEventArgs)

    End Sub
    Public Overrides Sub ReadFromXMl(path As String)

    End Sub

    Public Overrides Sub WriteToXML()

    End Sub
    Public Overrides Sub AddFrameworkChildren(ele As System.Xml.Linq.XElement)
        ReadFromXMLElement(ele)
        Dim dinfo As New System.IO.DirectoryInfo(GetCurrentDirectory)
        Dim cleandir As Boolean = False
        For Each file As System.IO.FileInfo In dinfo.GetFiles
            Dim keepfile As Boolean = False
            Dim terraintreenode As TerrainChildTreeNode
            For Each Header As FrameworkTreeNode In FirstLevelSubNodes
                terraintreenode = DirectCast(Header, TerrainChildTreeNode)
                Dim list As List(Of String) = terraintreenode.GetAllAssociatedFiles
                If list.Contains(file.FullName) Then
                    keepfile = True
                End If
            Next
            If Not keepfile And Not file.Attributes = 38 Then
                cleandir = True
            End If
        Next
        If cleandir Then
            Dim cleandirectory As New MenuItem
            cleandirectory.Header = "Clean Directory"
            AddHandler cleandirectory.Click, AddressOf Clean
            MyBase.ContextMenu.Items.Add(cleandirectory)
            MyBase.Foreground = System.Windows.Media.Brushes.Red
            MyBase.ToolTip = "Unnecessary files exist in this directory."
        End If
    End Sub
    Private Sub Clean(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim filelist As New List(Of String)
        Dim dinfo As New System.IO.DirectoryInfo(GetCurrentDirectory)
        For Each file As System.IO.FileInfo In dinfo.GetFiles
            Dim keepfile As Boolean = False
            Dim terraintreenode As TerrainChildTreeNode
            For Each Header As FrameworkTreeNode In FirstLevelSubNodes
                terraintreenode = DirectCast(Header, TerrainChildTreeNode)
                Dim list As List(Of String) = terraintreenode.GetAllAssociatedFiles
                If list.Contains(file.FullName) Then
                    keepfile = True
                End If
            Next
            If Not keepfile And Not file.Attributes = 38 Then
                filelist.Add(file.FullName)
            End If
        Next
        If filelist.Count > 0 Then
            Dim msg As String = ""
            For i = 0 To filelist.Count - 1
                msg = msg & filelist(i) & vbNewLine
            Next
            If MsgBox("Files that should be deleted were found:" & vbNewLine & vbNewLine & msg & vbNewLine & "Would you like to delete?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                For i = 0 To filelist.Count - 1
                    Dim remove As Boolean = False
                    Dim loc As Int16 = 0
                    For j = 0 To GetMapTreeView.GetAllFeatureNodes.Count - 1
                        If GetMapTreeView.GetAllFeatureNodes(j).Features.Features.GetSource = filelist(i) Then
                            ''If GetMapTreeView.GetAllFeatureNodes(j).EditingMode Then
                            ''MsgBox("The Feature " & GetMapTreeView.GetAllFeatureNodes(j).FeatureNodeHeader & " is in edit mode would you like to abort?")
                            '' End If
                            remove = True
                            loc = j

                        End If
                    Next
                    If remove Then GetMapTreeView.GetAllFeatureNodes(loc).RemoveLayer(True)
                    Kill(filelist(i))
                Next
                'remove the menu item.
                MyBase.Foreground = System.Windows.Media.Brushes.Black
                MyBase.ToolTip = ""
                For Each mi As MenuItem In MyBase.ContextMenu.Items
                    If mi.Header = "Clean Directory" Then
                        MyBase.ContextMenu.Items.Remove(mi) : Exit For
                    End If
                Next
            End If
        End If
    End Sub
    Public Overrides Sub ReadFromXMLElement(xele As System.Xml.Linq.XElement)
        For Each element As XElement In xele.Elements
            If System.IO.File.Exists(ConvertFromRelativePath(element.Attribute("Path"))) Then
                Dim t As New TerrainChildTreeNode(element)
                AddFrameworkTreeNode(t)
                t.AddFrameworkChildren(element)
            Else
                MsgBox(ConvertFromRelativePath(element.Attribute("Path")) & vbNewLine & "File Not Found.")
            End If

        Next
    End Sub
End Class
