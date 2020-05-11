Public Class HydraulicsChildTreenode
    Inherits FrameworkTreeNode
    Private Const _ParentFolder As String = "Hydraulics"
    Private Const _FolderName As String = "GriddedData"
    Private _AddedToMapWindow As Boolean = False
    'Private _GridPaths(7) As String
    Private _GridNames(7) As String
    Private _description As String
    Private _Probs(7) As Single
    Private _planyear As Integer
    Private _SelectedTerrain As TerrainChildTreeNode
    Private _IsDepthGrids As Boolean = False
    Public Event RemoveMapFeatureNode(updatemapwindow As Boolean)
    Sub New(ByVal uniqueName As String, ByVal desc As String, ByVal grids() As String, ByVal probs() As Single, ByVal terrain As TerrainChildTreeNode, ByVal isdepthgrid As Boolean, ByVal planyear As Integer)
        MyBase.New(uniqueName)
        Dim counter As Integer = 0
        If System.IO.Directory.Exists(GetCurrentDirectory & "\" & Header) Then
        Else
            System.IO.Directory.CreateDirectory(GetCurrentDirectory & "\" & Header)
        End If
        For i = 0 To grids.Count - 1
            _GridNames(i) = System.IO.Path.GetFileName(grids(i))
            If System.IO.Path.GetExtension(grids(i)).ToLower = ".vrt" Then
                Dim name As String = Header
                If System.IO.File.Exists(GetGridPaths(i)) Then
                    _GridNames(i) = System.IO.Path.GetFileNameWithoutExtension(GetGridPaths(i)) & "_" & planyear & System.IO.Path.GetExtension(GetGridPaths(i))
                    name = name & "_" & planyear
                End If
                Dim l As New LifeSimGIS.VRTReader(grids(i))
                Dim vrthelp As New VRT_Helper(l)
                vrthelp.SaveAs(GetCurrentDirectory & "\", name)
            Else
                If System.IO.File.Exists(GetGridPaths(i)) Then
                    _GridNames(i) = System.IO.Path.GetFileNameWithoutExtension(GetGridPaths(i)) & "_" & planyear & System.IO.Path.GetExtension(GetGridPaths(i))
                    System.IO.File.Copy(grids(i), GetGridPaths(i))
                Else
                    System.IO.File.Copy(grids(i), GetGridPaths(i))
                End If
            End If

        Next
        _Probs = probs
        _SelectedTerrain = terrain
        _planyear = planyear
        _IsDepthGrids = isdepthgrid
    End Sub
    Sub New(ByVal xele As XElement)
        MyBase.New(xele.Attribute("Header").Value)
    End Sub
    Public Overrides ReadOnly Property GetNodeName As String
        Get
            Return Me.GetType.Name
        End Get
    End Property
    Public ReadOnly Property GetGridPaths As String()
        Get
            Dim gridpaths(7) As String
            For i = 0 To _GridNames.Count - 1
                gridpaths(i) = GetCurrentDirectory & "\" & Header & "\" & _GridNames(i)
            Next
            Return gridpaths
        End Get
    End Property
    Public ReadOnly Property IsDepthGrid As Boolean
        Get
            Return _IsDepthGrids
        End Get
    End Property
    Public ReadOnly Property GetDescription As String
        Get
            Return _description
        End Get
    End Property
    Public ReadOnly Property GetTerrain As TerrainChildTreeNode
        Get
            Return _SelectedTerrain
        End Get
    End Property
    Public ReadOnly Property GetYear As Integer
        Get
            Return _planyear
        End Get
    End Property
    Public WriteOnly Property SetYear As Integer
        Set(value As Integer)
            _planyear = value
        End Set
    End Property
    Public Overrides ReadOnly Property GetSubDirectory As String
        Get
            Return _ParentFolder & "\" & _FolderName
        End Get
    End Property
    Public ReadOnly Property GetProbabilities As Single()
        Get
            Return _Probs
        End Get
    End Property
    Public Overrides Sub OnSaveEvent(sender As Object, e As System.Windows.RoutedEventArgs)

    End Sub

    Public Overrides Sub ReadFromXMl(path As String)

    End Sub

    Public Overrides Sub SetContextMenu()
        Dim c As New ContextMenu

        Dim AddToMap As New MenuItem()
        AddToMap.Header = "Add to Map Window"
        AddHandler AddToMap.Click, AddressOf AddToMapWindow
        c.Items.Add(AddToMap)

        Dim Rename As New MenuItem
        Rename.Header = "Rename"
        AddHandler Rename.Click, AddressOf RenameGridNode
        c.Items.Add(Rename)

        Dim SaveAs As New MenuItem
        SaveAs.Header = "Save As"
        AddHandler SaveAs.Click, AddressOf SaveGridNodeAs
        c.Items.Add(SaveAs)

        Dim Remove As New MenuItem
        Remove.Header = "Remove From Study"
        AddHandler Remove.Click, AddressOf RemoveFromStudy
        c.Items.Add(Remove)

        Dim Properties As New MenuItem
        Properties.Header = "Properties"
        AddHandler Properties.Click, AddressOf DisplayProperties
        c.Items.Add(Properties)

        MyBase.ContextMenu = c
    End Sub
    Sub AddToMapWindow(sender As Object, e As System.Windows.RoutedEventArgs)
        If _AddedToMapWindow Then

        Else
            'already added
            Dim fns(7) As OpenGLMapping.FeatureNode
            Dim max As Double = 0
            Dim min As Double = Double.MaxValue
            For i = 0 To _GridNames.Count - 1
                fns(i) = New OpenGLMapping.FeatureNode(GetGridPaths(i), GetMapTreeView.MapWindow)
                Select Case fns(i).Features.GetType
                    Case GetType(OpenGLMapping.MapRaster)
                        With CType(fns(i).Features, OpenGLMapping.MapRaster).Ramp
                            If max < .DataMax Then max = .DataMax
                            If min > .DataMin Then min = .DataMin
                        End With
                    Case GetType(OpenGLMapping.MapRasterSet)
                        With CType(fns(i).Features, OpenGLMapping.MapRasterSet).Ramp
                            If max < .DataMax Then max = .DataMax
                            If min > .DataMin Then min = .DataMin
                        End With
                End Select
            Next
            For i = 0 To fns.Count - 1
                Select Case fns(i).Features.GetType
                    Case GetType(OpenGLMapping.MapRaster)
                        With CType(fns(i).Features, OpenGLMapping.MapRaster).Ramp
                            .CurrentRamp = OpenGLMapping.ColorRamp.RampType.LightBlueDarkBlue
                            .HistogramMax = max
                            .HistogramMin = min
                            .StretchType = OpenGLMapping.ColorRamp.ColorStretch.MinMax
                            .SetCurrentRamp()
                        End With
                    Case GetType(OpenGLMapping.MapRasterSet)
                        With CType(fns(i).Features, OpenGLMapping.MapRasterSet).Ramp
                            .CurrentRamp = OpenGLMapping.ColorRamp.RampType.LightBlueDarkBlue
                            .HistogramMax = max
                            .HistogramMin = min
                            .StretchType = OpenGLMapping.ColorRamp.ColorStretch.MinMax
                            .SetCurrentRamp()
                        End With
                End Select

                AddHandler fns(i).RemoveFromMapWindow, AddressOf FeatureRemoved
                AddHandler RemoveMapFeatureNode, AddressOf fns(i).RemoveLayer
                GetMapTreeView.AddGISData(fns(i))
            Next
        End If


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
        'this will remove all... if they use the remove from map window, it only will remove one.
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
    Private Sub SaveGridNodeAs(sender As Object, e As System.Windows.RoutedEventArgs)
        'rename the files
        Dim rename As New Rename(Header)
        rename.Title = "Save As"
        rename.Owner = GetMainWindow
        If rename.ShowDialog() Then
            'check for name conflicts.
            Dim ianodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(HydraulicsChildTreenode))
            If ianodes.Count > 0 Then
                'potential for name conflicts
                Dim nameconflict As Boolean = False
                Do
                    nameconflict = False
                    For j = 0 To ianodes.Count - 1
                        If ianodes(j).Header = rename.NewName AndAlso DirectCast(ianodes(j), HydraulicsChildTreenode).GetYear = GetYear Then
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
            Dim parentfile As HydraulicsParentTreeNode = CType(Parent, HydraulicsParentTreeNode)
            Dim childfile As New HydraulicsChildTreenode(rename.NewName, _description & " Copied from " & Header, GetGridPaths, GetProbabilities, GetTerrain, IsDepthGrid, _planyear)
            parentfile.AddFrameworkTreeNode(childfile)
            childfile.AddFrameworkChildren()
            CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
        Else
            'user closed.
        End If


    End Sub
    Private Sub ReNameGridnode(sender As Object, e As System.Windows.RoutedEventArgs)

        'check if any outputs exist that use these grids
        Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
        Dim pnodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(PlanChildTreeNode))
        Dim msg As New System.Text.StringBuilder
        If nodes.Count = 0 Then
        Else
            Dim onodes As New List(Of OutputChildTreeNode)
            For i = 0 To nodes.Count - 1
                onodes.Add(CType(nodes(i), OutputChildTreeNode))
                For j = 0 To onodes.Last.GetPlans.Count - 1
                    If onodes.Last.GetPlans(j).Header = MyBase.Header AndAlso DirectCast(onodes.Last.GetPlans(j), HydraulicsChildTreenode).GetYear = GetYear Then
                        msg.AppendLine("Hydraulic node is used by " & onodes.Last.Header)
                    End If
                Next
            Next
        End If
        Dim dontkill As Boolean = False
        If msg.ToString = "" Then
        Else
            If MsgBox(msg.ToString & vbNewLine & "Would you still like to rename?", MsgBoxStyle.OkCancel, "Warning") = MsgBoxResult.Cancel Then dontkill = True
            'should i do anything for the output nodes? if they say ok?
        End If
        'rename the files
        If Not dontkill Then
            Dim rename As New Rename(Header)
            rename.Owner = GetMainWindow
            If rename.ShowDialog() Then
                ''remove from map window.
                Dim wasaddedtomapwindow As Boolean = False
                If _AddedToMapWindow Then
                    'remove it from the map window first.
                    wasaddedtomapwindow = True
                    RemoveFromMapWindow(Nothing, Nothing)
                End If
                'check for name conflicts.

                Dim ianodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(HydraulicsChildTreenode))
                If ianodes.Count > 0 Then
                    'potential for name conflicts
                    Dim nameconflict As Boolean = False
                    Do
                        nameconflict = False
                        For j = 0 To ianodes.Count - 1
                            If ianodes(j).Header = rename.NewName AndAlso DirectCast(ianodes(j), HydraulicsChildTreenode).GetYear = GetYear Then
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
                Dim newname As String = GetCurrentDirectory & "\" & rename.NewName & "\"
                'copy files, kill old files, change header name.
                ''check if directory exists, if not create it
                If Not System.IO.Directory.Exists(newname) Then
                    System.IO.Directory.CreateDirectory(newname)
                End If
                If System.IO.Path.GetExtension(_GridNames(0)).ToLower = ".vrt" Then
                    For i = 0 To _GridNames.Count - 1
                        Dim l As New LifeSimGIS.VRTReader(GetGridPaths(i))
                        Dim vrthelp As New VRT_Helper(l)
                        vrthelp.SaveAs(newname, _GridNames(i))
                    Next
                Else

                    For i = 0 To _GridNames.Count - 1
                        System.IO.File.Copy(GetGridPaths(i), newname & _GridNames(i))
                    Next
                End If

                Dim fi As New System.IO.FileInfo(GetGridPaths(0))
                fi.Directory.Delete(True)
                'For i = 0 To _GridNames.Count - 1
                '    Kill(GetGridPaths(i))
                'Next
                For Each outnode As OutputChildTreeNode In nodes
                    For i As Integer = 0 To outnode.GetPlans.Count - 1
                        If outnode.GetPlans(i).GetYear = GetYear AndAlso outnode.GetPlans(i).Header = Header Then
                            outnode.UpdatePlanFolderForRenameOfHydraulicsTreeNode(Me, rename.NewName)
                            outnode.AlertUserThatUpdatesAreNeeded("The water surface profile named: " & Header & " was used by the FDA input file " & outnode.Header & " and has been renamed to: " & rename.NewName & " this requires a recompute", False, False)
                        End If
                    Next
                Next
                    MyBase.Header = rename.NewName
                    For Each Pchildnode As PlanChildTreeNode In pnodes
                        If Pchildnode.HydraulicChildTreeNode.Header = Header AndAlso Pchildnode.HydraulicChildTreeNode.GetYear = GetYear Then
                            Pchildnode.Header = Header
                        End If
                    Next
                    If wasaddedtomapwindow Then
                        AddToMapWindow(Nothing, Nothing)
                    End If
                    CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
            End If

        End If

    End Sub
    Private Sub RemoveFromStudy(sender As Object, e As System.Windows.RoutedEventArgs)

        'check if any outputs exist that use this inventory.
        Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
        Dim msg As New System.Text.StringBuilder
        If nodes.Count = 0 Then
        Else
            Dim onodes As New List(Of OutputChildTreeNode)
            For i = 0 To nodes.Count - 1
                onodes.Add(CType(nodes(i), OutputChildTreeNode))
                For j = 0 To onodes.Last.GetPlans.Count - 1
                    If onodes.Last.GetPlans(j).Header = MyBase.Header AndAlso DirectCast(onodes.Last.GetPlans(j), HydraulicsChildTreenode).GetYear = GetYear Then
                        msg.AppendLine("Hydraulics Node is used by " & onodes.Last.Header)
                    End If
                Next
            Next
        End If
        Dim dontkill As Boolean = False
        If msg.ToString = "" Then
            'if the hydraulics is not used by an output, then delete
            DeleteFromStudy()
        Else
            If MsgBox(msg.ToString & vbNewLine & "Would you still like to delete?", MsgBoxStyle.OkCancel, "Warning") = MsgBoxResult.Cancel Then
            Else
                DeleteFromStudy()
            End If
        End If


    End Sub
    Public Sub DeleteFromStudy()

        If _AddedToMapWindow Then
            'remove it from the map window first.
            RemoveFromMapWindow(Nothing, Nothing)
        End If
        If System.IO.Path.GetExtension(_GridNames(0)).ToLower = ".vrt" Then
            For i = 0 To _GridNames.Count - 1
                Dim l As New LifeSimGIS.VRTReader(GetGridPaths(i))
                Dim vrthelp As New VRT_Helper(l)
                vrthelp.Delete()
            Next
        Else
            For i = 0 To _GridNames.Count - 1
                Kill(GetGridPaths(i))
            Next
        End If


        Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
        If nodes.Count = 0 Then
        Else
            For i = 0 To nodes.Count - 1
                For Each h As HydraulicsChildTreenode In DirectCast(nodes(i), OutputChildTreeNode).GetPlans
                    If h.Header = Header AndAlso h.GetYear = GetYear Then
                        DirectCast(nodes(i), OutputChildTreeNode).DeleteFromStudy()
                    End If
                Next
            Next
        End If

        ''update main xml
        Dim sn As StudyTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode)
        Dim hn As HydraulicsParentTreeNode = CType(Parent, HydraulicsParentTreeNode)
        hn.FirstLevelSubNodes.Remove(Me)
        hn.Items.Remove(Me)
        sn.RemoveFrameworkTreeNode(Me)
        sn.WriteToXML()

    End Sub
    Private Sub FeatureRemoved(fn As OpenGLMapping.FeatureNode)
        'this will only remove the specific grid they clicked on.  i dont know how to force all of them to remove at the same time.
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
    Public Overrides Sub AddFrameworkChildren(ele As System.Xml.Linq.XElement)
        ReadFromXMLElement(ele)
    End Sub
    Public Overrides Sub WriteToXML()

    End Sub
    Public Overrides Sub ReadFromXMLElement(xele As System.Xml.Linq.XElement)
        Dim i As Integer = 0
        _IsDepthGrids = CBool(xele.Attribute("IsDepthGrid").Value)
        _planyear = CInt(xele.Attribute("Year").Value)
        For Each x As XElement In xele.Element("Grids").Elements
            _GridNames(i) = System.IO.Path.GetFileName(x.Attribute("Path").Value)
            _Probs(i) = CSng(x.Attribute("Prob").Value)
            i += 1
        Next
        Dim counter As Integer = 0
        Dim fns As New List(Of OpenGLMapping.FeatureNode)
        For Each fn As OpenGLMapping.FeatureNode In GetMapTreeView.MapWindow.GISFeatures
            For i = 0 To _GridNames.Count - 1
                If Not IsNothing(fn.Features.Features) AndAlso fn.Features.Features.GetSource = GetGridPaths(i) Then
                    counter += 1
                    fns.Add(fn)
                End If
            Next
        Next
        If counter = _GridNames.Count Then
            _AddedToMapWindow = True
            For i = 0 To fns.Count - 1
                AddHandler fns(i).RemoveFromMapWindow, AddressOf FeatureRemoved
                AddHandler RemoveMapFeatureNode, AddressOf fns(i).RemoveLayer
            Next
        End If
        _SelectedTerrain = CType(GetFrameworkTreenodeByNameAndType(xele.Element("Terrain").Value, GetType(TerrainChildTreeNode)), TerrainChildTreeNode)
        If _AddedToMapWindow Then AddToMapWindow(Me, Nothing)
    End Sub
    Public Overrides Function WriteToXMLElement() As System.Xml.Linq.XElement
        Dim xel As New XElement(GetNodeName)
        xel.SetAttributeValue("Header", Header)
        xel.SetAttributeValue("IsDepthGrid", _IsDepthGrids)
        xel.SetAttributeValue("Year", _planyear)
        Dim grids As New XElement("Grids")
        For i = 0 To _GridNames.Count - 1
            Dim x As New XElement("GridPath")
            x.SetAttributeValue("Path", ConvertToRelativePath(GetGridPaths(i)))
            x.SetAttributeValue("Prob", _Probs(i))
            grids.Add(x)
        Next
        xel.Add(grids)
        Dim wxel As New XElement("Terrain")
        wxel.Value = _SelectedTerrain.Header
        xel.Add(wxel)
        Return xel
    End Function

    Private Sub DisplayProperties(sender As Object, e As RoutedEventArgs)
        Dim msg As New System.Text.StringBuilder
        msg.AppendLine("Water Surface Profile: " & MyBase.Header)
        msg.AppendLine("Plan Year: " & _planyear)
        msg.AppendLine("Description: " & _description)
        msg.AppendLine("Terrain: " & _SelectedTerrain.Header)
        Dim gridtype As String = ""
        If _IsDepthGrids Then
            gridtype = "Depth Grids"
        Else
            gridtype = "Water Surface Elevation Grids"
        End If
        msg.AppendLine("Grid Type: " & gridtype)
        msg.AppendLine()
        msg.AppendLine("Grid Names and Probabilities: ")
        For i = 0 To _GridNames.Count - 1
            msg.AppendLine(_GridNames(i) & " - " & _Probs(i))
        Next
        MsgBox(msg.ToString, MsgBoxStyle.OkOnly, "Water Surface Profile Properties")
    End Sub

End Class
