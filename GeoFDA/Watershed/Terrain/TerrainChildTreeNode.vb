Public Class TerrainChildTreeNode
    Inherits FrameworkTreeNode
    Private Const _FolderName As String = "Terrain"
    Private _AddedToMapWindow As Boolean = False
    Private _Ext As String
    'Private _TerrainFeatureNode As OpenGLMapping.FeatureNode
    Public Event RemoveMapFeatureNode(updatemapwindow As Boolean)
    Sub New(ByVal InputFilePath As String, ByVal terrainname As String)
        MyBase.New(terrainname)
        Select Case System.IO.Path.GetExtension(InputFilePath)
            Case ".flt"
                'we need a progress bar.
                _Ext = ".flt"
                LifeSimGIS.GDALUtils.ConvertToTilePyramidCompressedTiff(InputFilePath, GetTerrainPath, True, True, True)
            Case ".tif"
                _Ext = ".tif"
                If System.IO.File.Exists(GetTerrainPath) Then
                    If MsgBox("The selected terrain already exists, would you like to overwrite it?", MsgBoxStyle.YesNo, "File Conflict") = MsgBoxResult.Yes Then
                        ''it must be a vrt also..

                        Kill(GetTerrainPath)
                        System.IO.File.Copy(InputFilePath, GetTerrainPath)
                    Else
                        'no, so now what?

                    End If
                Else
                    System.IO.File.Copy(InputFilePath, GetTerrainPath)
                End If
            Case ".vrt"
                _Ext = ".vrt"
                Dim l As New LifeSimGIS.VRTReader(InputFilePath)
                Dim vrthelp As New VRT_Helper(l)
                vrthelp.SaveAs(terrainname, GetTerrainPath)
                'copy over the raster files
                'copy over the vrt file and rename

            Case Else
                MsgBox("The file type you selected is not supported")
        End Select

        SetContextMenu()
    End Sub
    Sub New(ByVal ele As XElement)
        MyBase.New(System.IO.Path.GetFileNameWithoutExtension(ele.Attribute("Path")))
    End Sub
    Public Overrides ReadOnly Property GetSubDirectory As String
        Get
            Return _FolderName '_ParentFolder & "\" & _FolderName
        End Get
    End Property
    Public Overrides ReadOnly Property GetNodeName As String
        Get
            Return "TerrainChild"
        End Get
    End Property
    Public ReadOnly Property GetTerrainPath As String
        Get
            Return GetCurrentDirectory & "\" & Header & _Ext 'this is risky with the directory being shared.
        End Get
    End Property
    Public Overrides Sub SetContextMenu()
        Dim c As New ContextMenu
        If _AddedToMapWindow Then
            Dim AddTerrainToMap As New MenuItem()
            AddTerrainToMap.Header = "Remove From Map Window"
            AddHandler AddTerrainToMap.Click, AddressOf RemoveFromMapWindow
            c.Items.Add(AddTerrainToMap)
        Else
            Dim AddTerrainToMap As New MenuItem()
            AddTerrainToMap.Header = "Add to Map Window"
            AddHandler AddTerrainToMap.Click, AddressOf AddToMapWindow
            c.Items.Add(AddTerrainToMap)
        End If

        Dim Rename As New MenuItem
        Rename.Header = "Rename"
        AddHandler Rename.Click, AddressOf ReNameTerrain
        c.Items.Add(Rename)

        Dim SaveAs As New MenuItem
        SaveAs.Header = "Save As"
        AddHandler SaveAs.Click, AddressOf SaveTerrainAs
        c.Items.Add(SaveAs)

        Dim Remove As New MenuItem
        Remove.Header = "Remove From Study"
        AddHandler Remove.Click, AddressOf RemoveFromStudy
        c.Items.Add(Remove)
        MyBase.ContextMenu = c
    End Sub
    Public Overrides Sub AddFrameworkChildren(ele As System.Xml.Linq.XElement)
        _Ext = System.IO.Path.GetExtension(ele.Attribute("Path")) ''check if it is .ext or just ext...
        ReadFromXMLElement(ele)
    End Sub
    Public Function GetAllAssociatedFiles() As List(Of String)
        Dim ret As New List(Of String)
        If _Ext = ".tif" Then
            ret.Add(GetTerrainPath)
            Return ret
        Else
            ret.Add(GetTerrainPath)
            Dim l As New LifeSimGIS.VRTReader(GetTerrainPath)
            ret.AddRange(l.RasterFiles)
            Return ret
        End If
    End Function
    Sub AddToMapWindow(sender As Object, e As System.Windows.RoutedEventArgs)
        If _AddedToMapWindow Then
            'it is already there
        Else
            If System.IO.File.Exists(GetTerrainPath) Then
                Dim tnode As New OpenGLMapping.FeatureNode(GetTerrainPath, GetMapTreeView.MapWindow)
                AddHandler tnode.RemoveFromMapWindow, AddressOf FeatureRemoved
                AddHandler RemoveMapFeatureNode, AddressOf tnode.RemoveLayer
                Select Case tnode.Features.GetType
                    Case GetType(OpenGLMapping.MapRaster)
                        With CType(tnode.Features, OpenGLMapping.MapRaster).Ramp
                            .CurrentRamp = OpenGLMapping.ColorRamp.RampType.Terrain
                            .SetCurrentRamp()
                        End With
                    Case GetType(OpenGLMapping.MapRasterSet)
                        With CType(tnode.Features, OpenGLMapping.MapRasterSet).Ramp
                            .CurrentRamp = OpenGLMapping.ColorRamp.RampType.Terrain
                            .SetCurrentRamp()
                        End With
                End Select

                GetMapTreeView.AddGISData(tnode)
            Else
                ReportMessage("The file defined by the Terrain file path: " & GetTerrainPath & " is missing.") : Exit Sub
            End If

        End If

        'add a handler to the remove event to handle the case that the map layer was removed by the user from the maps tab.

        For Each mi As MenuItem In MyBase.ContextMenu.Items
            If mi.Header = "Add to Map Window" Then
                mi.Header = "Remove From Map Window"
                RemoveHandler mi.Click, AddressOf AddToMapWindow
                AddHandler mi.Click, AddressOf RemoveFromMapWindow
            End If
        Next

        MyBase.FontWeight = System.Windows.FontWeights.Bold
        _AddedToMapWindow = True
    End Sub
    Private Sub RemoveFromMapWindow(sender As Object, e As System.Windows.RoutedEventArgs)
        'how do i know which one to remove?
        RaiseEvent RemoveMapFeatureNode(True)
        For Each mi As MenuItem In MyBase.ContextMenu.Items
            If mi.Header = "Remove From Map Window" Then
                mi.Header = "Add to Map Window"
                AddHandler mi.Click, AddressOf AddToMapWindow
                RemoveHandler mi.Click, AddressOf RemoveFromMapWindow
            End If
        Next
        MyBase.FontWeight = System.Windows.FontWeights.Normal
        _AddedToMapWindow = False
    End Sub
    Private Sub SaveTerrainAs(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim wasaddedtomapwindow As Boolean = False
        'rename the files
        Dim rename As New Rename(Header)
        rename.Title = "Save As"
        rename.Owner = GetMainWindow
        If rename.ShowDialog() Then
            'check for name conflicts.
            Dim ianodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(TerrainChildTreeNode))
            If ianodes.Count > 0 Then
                'potential for name conflicts
                Dim nameconflict As Boolean = False
                Do
                    nameconflict = False
                    For j = 0 To ianodes.Count - 1
                        If ianodes(j).Header = rename.NewName Then
                            nameconflict = True
                        End If
                    Next
                    If nameconflict Then
                        rename = New Rename(rename.NewName & "_1")
                        rename.Title = "Save As"
                        rename.Owner = GetMainWindow
                        If rename.ShowDialog() Then
                            'loop
                        Else
                            'user aborted
                            Exit Sub
                        End If
                    End If
                Loop Until nameconflict = False
            End If
            Dim parentfile As TerrainParentTreeNode = CType(Parent, TerrainParentTreeNode)

            Dim childfile As New TerrainChildTreeNode(GetTerrainPath, rename.NewName)
            parentfile.AddFrameworkTreeNode(childfile)
            childfile.AddFrameworkChildren()
            CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
        Else
            'user closed.
        End If
    End Sub
    Private Sub ReNameTerrain(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim rename As New Rename(Header)
        rename.Owner = GetMainWindow
        If rename.ShowDialog() Then
            Dim wasaddedtomapwindow As Boolean = False
            If _AddedToMapWindow Then
                'remove it from the map window first.
                wasaddedtomapwindow = True
                RemoveFromMapWindow(Nothing, Nothing)
            End If
            'check for name conflicts.
            Dim ianodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(TerrainChildTreeNode))
            If ianodes.Count > 0 Then
                'potential for name conflicts
                Dim nameconflict As Boolean = False
                Do
                    nameconflict = False
                    For j = 0 To ianodes.Count - 1
                        If ianodes(j).Header = rename.NewName Then
                            nameconflict = True
                        End If
                    Next
                    If nameconflict Then
                        rename = New Rename(rename.NewName & "_1")
                        rename.Owner = GetMainWindow
                        If rename.ShowDialog() Then
                            'loop
                        Else
                            'user aborted
                            Exit Sub
                        End If
                    End If
                Loop Until nameconflict = False
            End If
            If _Ext.ToLower = ".vrt" Then
                Dim l As New LifeSimGIS.VRTReader(GetTerrainPath)
                Dim vrthelp As New VRT_Helper(l)
                vrthelp.Rename(rename.NewName)
            Else
                Dim newname As String = GetCurrentDirectory & "\" & rename.NewName & _Ext
                'copy files, kill old files, change header name.
                System.IO.File.Copy(GetTerrainPath, newname)
                Kill(GetTerrainPath)
            End If



            Dim stn As StudyTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode)

            MyBase.Header = rename.NewName
            If wasaddedtomapwindow Then
                AddToMapWindow(Nothing, Nothing)
            End If
            stn.WriteToXML()
        End If

    End Sub
    Private Sub RemoveFromStudy(sender As Object, e As System.Windows.RoutedEventArgs)

        'check if any outputs exist that use this terrain
        Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
        Dim msg As New System.Text.StringBuilder
        If nodes.Count = 0 Then
        Else
            Dim onodes As New List(Of OutputChildTreeNode)
            For i = 0 To nodes.Count - 1
                onodes.Add(CType(nodes(i), OutputChildTreeNode))
                For j = 0 To onodes.Last.GetPlans.Count - 1
                    If onodes.Last.GetPlans(j).GetTerrain.GetTerrainPath = GetTerrainPath Then
                        msg.AppendLine("terrain is used by " & onodes.Last.Header & " in hydraulic wsp " & onodes.Last.GetPlans(j).Header)
                    End If
                Next
            Next
        End If
        nodes = GetAllFrameworkTreenodesOfType(GetType(HydraulicsChildTreenode))
        If nodes.Count = 0 Then
        Else
            Dim pnodes As New List(Of HydraulicsChildTreenode)
            For i = 0 To nodes.Count - 1
                pnodes.Add(CType(nodes(i), HydraulicsChildTreenode))
                If pnodes.Last.GetTerrain.GetTerrainPath = GetTerrainPath Then
                    msg.AppendLine("terrain is used by hydraulic wsp " & pnodes.Last.Header)
                End If

            Next
        End If
        Dim dontkill As Boolean = False
        If msg.ToString = "" Then
            'if the terrain is not used by a hydraulics, then delete
            DeleteFromStudy()
        Else
            If MsgBox(msg.ToString & vbNewLine & "Would you still like to delete?", MsgBoxStyle.OkCancel, "Warning") = MsgBoxResult.Cancel Then
            Else
                DeleteFromStudy()
            End If
        End If
    End Sub
    Private Sub DeleteFromStudy()
        'check if any outputs exist that use this terrain
        Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
        If nodes.Count = 0 Then
        Else
            For i = 0 To nodes.Count - 1
                For Each h As HydraulicsChildTreenode In DirectCast(nodes(i), OutputChildTreeNode).GetPlans
                    If h.Header = Header Then
                        DirectCast(nodes(i), OutputChildTreeNode).DeleteFromStudy()
                    End If
                Next
            Next
        End If
        nodes = GetAllFrameworkTreenodesOfType(GetType(HydraulicsChildTreenode))
        If nodes.Count = 0 Then
        Else
            For i = 0 To nodes.Count - 1
                If DirectCast(nodes(i), HydraulicsChildTreenode).GetTerrain.GetTerrainPath = GetTerrainPath Then
                    DirectCast(nodes(i), HydraulicsChildTreenode).DeleteFromStudy()
                End If
            Next
        End If

        If _AddedToMapWindow Then
            'remove it from the map window first.
            RemoveFromMapWindow(Nothing, Nothing)
        End If
        If _Ext.ToLower = ".vrt" Then
            Dim l As New LifeSimGIS.VRTReader(GetTerrainPath)
            Dim vrthelp As New VRT_Helper(l)
            vrthelp.Delete()
        Else
            Kill(GetTerrainPath)
        End If

        Dim tn As TerrainParentTreeNode = CType(Parent, TerrainParentTreeNode)
        Dim sn As StudyTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode)
        tn.FirstLevelSubNodes.Remove(Me)
        tn.Items.Remove(Me)
        sn.RemoveFrameworkTreeNode(Me)
        sn.WriteToXML()
    End Sub
    Private Sub FeatureRemoved(fn As OpenGLMapping.FeatureNode)
        RemoveHandler RemoveMapFeatureNode, AddressOf fn.RemoveLayer
        For Each mi As MenuItem In MyBase.ContextMenu.Items
            If mi.Header = "Remove From Map Window" Then
                mi.Header = "Add to Map Window"
                AddHandler mi.Click, AddressOf AddToMapWindow
                RemoveHandler mi.Click, AddressOf RemoveFromMapWindow
            End If
        Next
        MyBase.FontWeight = System.Windows.FontWeights.Normal
        _AddedToMapWindow = False
    End Sub
    Public Overrides Sub OnSaveEvent(sender As Object, e As System.Windows.RoutedEventArgs)

    End Sub
    Public Overrides Sub ReadFromXMl(path As String)

    End Sub
    Public Overrides Sub WriteToXML()

    End Sub
    Public Overrides Sub ReadFromXMLElement(xele As System.Xml.Linq.XElement)
        '_TerrainFileName = System.IO.Path.GetFileNameWithoutExtension(xele.Attribute("Path").Value)
        For Each fn As OpenGLMapping.FeatureNode In GetMapTreeView.MapWindow.GISFeatures
            If Not IsNothing(fn.Features.Features) AndAlso fn.Features.Features.GetSource = GetTerrainPath Then
                _AddedToMapWindow = True
                '_TerrainFeatureNode = fn 'since terrains can be large, load it once and dont unload it.
                AddHandler fn.RemoveFromMapWindow, AddressOf FeatureRemoved
                AddHandler RemoveMapFeatureNode, AddressOf fn.RemoveLayer
            End If
        Next
        If _AddedToMapWindow Then AddToMapWindow(Me, Nothing) 'this should work... if it has the proper attribute
    End Sub

    Public Overrides Function WriteToXMLElement() As System.Xml.Linq.XElement
        Dim tfp As New XElement(GetNodeName)
        tfp.SetAttributeValue("Path", ConvertToRelativePath(GetTerrainPath))
        Return tfp
    End Function

End Class
