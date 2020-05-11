Public Class StructureInventoryParentNode
    Inherits FrameworkTreeNode
    Private Const _parentfolder As String = "Inventory"
    Private Const _FolderName As String = "Structures"
    Private Const _header As String = "Structure Inventories"
    Sub New()
        MyBase.New(_header)
    End Sub
    Public Overrides ReadOnly Property GetSubDirectory As String
        Get
            Return _parentfolder & "\" & _FolderName
        End Get
    End Property
    Public Overrides ReadOnly Property GetNodeName As String
        Get
            Return _FolderName
        End Get
    End Property
    'Sub NSIImportFunction(sender As Object, e As System.Windows.RoutedEventArgs)
    '    ' Dim t As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(TerrainChildTreeNode))
    '    Dim studyareas As New List(Of String)
    '    Dim pointshapes As New List(Of String)
    '    For Each fn As OpenGLMapping.FeatureNode In GetMapTreeView.MapWindow.GISFeatures
    '        Select Case fn.Features.GetType
    '            Case GetType(OpenGLMapping.MapPolygons)
    '                studyareas.Add(fn.Features.Features.GetSource)
    '            Case GetType(OpenGLMapping.MapPoints)
    '                pointshapes.Add(fn.Features.Features.GetSource)
    '            Case Else
    '                'not a polygon or a point
    '        End Select
    '    Next
    '    Dim snode As StudyTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode)
    '    Dim nsi As New NSI_Importer(pointshapes, studyareas, GetCurrentDirectory & "\", snode.MonetaryUnit)
    '    nsi.Owner = GetMainWindow
    '    AddHandler nsi.ProgressComplete, AddressOf CreateTreeNode
    '    AddHandler nsi.CheckName, AddressOf CheckNSINameConflict
    '    nsi.Show()


    'End Sub
    'Private Sub CheckNSINameConflict(ByVal uniquename As String, ByRef cancel As Boolean)
    '    Dim templist As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(StructureInventoryChildTreeNode))
    '    If templist.Count = 0 Then
    '    Else
    '        For j = 0 To templist.Count - 1
    '            If templist(j).Header = uniquename Then
    '                cancel = True
    '            End If
    '        Next

    '    End If
    'End Sub
    Sub CreateTreeNode(ByVal itworked As Boolean, ByVal uniquename As String)
        If itworked Then
            CType(GetAllFrameworkTreenodesOfType(GetType(OccupancyTypeTreeNode))(0), OccupancyTypeTreeNode).BrowseToDefaultFile(Nothing, Nothing)
            Dim sic As New StructureInventoryChildTreeNode(uniquename, StructureGenerationMethod.NSI)
            AddFrameworkTreeNode(sic)
            CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
        End If
    End Sub
    Sub ImportByPointShapefile(sender As Object, e As System.Windows.RoutedEventArgs)

        Dim shapes As New List(Of String)
        For Each fn As OpenGLMapping.FeatureNode In GetMapTreeView.MapWindow.GISFeatures
            Select Case fn.Features.GetType
                Case GetType(OpenGLMapping.MapPolygons)
                    shapes.Add(fn.Features.Features.GetSource)
                Case GetType(OpenGLMapping.MapPoints)
                    shapes.Add(fn.Features.Features.GetSource)
                Case Else
                    'not a polygon or a point
            End Select
        Next
        Dim snode As StudyTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode)
        Dim si As New ImportFromShapefile(shapes, snode.MonetaryUnit)
        AddHandler si.RaiseError, AddressOf ReportMessage
        si.Owner = GetMainWindow
        si.ShowDialog()
        Dim StructureInventoryName As String = ""
        If si.DialogResult Then
            StructureInventoryName = si.NameTextBox.Text
            Dim templist As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(StructureInventoryChildTreeNode))
            If templist.Count = 0 Then
            Else

                Dim nameconflicts As Boolean = True
                Do Until Not nameconflicts
                    For j = 0 To templist.Count - 1
                        If templist(j).Header = StructureInventoryName Then
                            MsgBox("Name conflict, please rename")
                            Dim r As New Rename(StructureInventoryName & "_1")
                            If r.ShowDialog Then
                                'user pressed ok.
                                StructureInventoryName = r.TxtName.Text
                            Else
                                MsgBox("User aborted") : Exit Sub
                            End If
                        Else
                            nameconflicts = False
                        End If
                    Next
                Loop
            End If

            'make the shapefile.
            Dim generationmethod As StructureGenerationMethod
            Dim sr As New LifeSimGIS.ShapefileReader(si.CmbStructureInventoryPaths.GetSelectedItemPath)
            If sr.ShapeType = LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.Point Or sr.ShapeType = LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PointM Or sr.ShapeType = LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PointZM Then
                generationmethod = StructureGenerationMethod.PointShapefile
                System.IO.File.Copy(si.CmbStructureInventoryPaths.GetSelectedItemPath, GetCurrentDirectory & "\" & StructureInventoryName & ".shp")
                System.IO.File.Copy(System.IO.Path.ChangeExtension(si.CmbStructureInventoryPaths.GetSelectedItemPath, ".shx"), GetCurrentDirectory & "\" & StructureInventoryName & ".shx")
                'write the dbf since it is not being written by the shapefile writer.
                Dim dbf As New DataBase_Reader.DBFReader(GetCurrentDirectory & "\" & StructureInventoryName & ".dbf", si.GetDataTable)

            Else
                'if it is a polygon.  create a point shapefile.
                generationmethod = StructureGenerationMethod.PolygonShapefile
                Dim polys As LifeSimGIS.PolygonFeatures = sr.ToFeatures
                Dim points As LifeSimGIS.PointFeatures = polys.CalculateCentroids
                Dim sw As New LifeSimGIS.ShapefileWriter(GetCurrentDirectory & "\" & StructureInventoryName & ".shp")
                sw.WriteFeatures(points, si.GetDataTable)

            End If
            'always copy the projection (check if it exists?)
            If System.IO.File.Exists(System.IO.Path.ChangeExtension(si.CmbStructureInventoryPaths.GetSelectedItemPath, ".prj")) Then
                System.IO.File.Copy(System.IO.Path.ChangeExtension(si.CmbStructureInventoryPaths.GetSelectedItemPath, ".prj"), GetCurrentDirectory & "\" & StructureInventoryName & ".prj")
            Else
                'projection undefined use map projection?

            End If
            Dim i As New StructureInventoryChildTreeNode(StructureInventoryName, generationmethod)
            AddFrameworkTreeNode(i)
            i.AddFrameworkChildren()
            CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
        Else
            'no action necessary
        End If
    End Sub
    Sub ImportByHAZUS(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim shapes As New List(Of String)
        For Each fn As OpenGLMapping.FeatureNode In GetMapTreeView.MapWindow.GISFeatures
            Select Case fn.Features.GetType
                Case GetType(OpenGLMapping.MapPolygons)
                    shapes.Add(fn.Features.Features.GetSource)
                Case Else
                    'not a polygon
            End Select
        Next
        Dim snode As StudyTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode)
        Dim sf As New ImportFromHAZUS(GetCurrentDirectory, shapes, snode.MonetaryUnit)
        AddHandler sf.ReportMessage, AddressOf ReportMessage
        'sf.SetOutputDirectory(GetCurrentDirectory)
        'sf.SetValidShapes(shapes)
        ' Dim sf As New ImportFromHAZUS(GetCurrentDirectory, shapes)
        sf.Owner = GetMainWindow
        sf.ShowDialog()
        If sf.DialogResult Then
            'i have already checked for name conflicts.
            CType(GetAllFrameworkTreenodesOfType(GetType(OccupancyTypeTreeNode))(0), OccupancyTypeTreeNode).BrowseToDefaultFile(Nothing, Nothing)
            Dim i As New StructureInventoryChildTreeNode(System.IO.Path.GetFileNameWithoutExtension(sf.GetStructurePath), StructureGenerationMethod.HAZUS)
            AddFrameworkTreeNode(i)
            i.AddFrameworkChildren()
            CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
        Else
            'not ok.
        End If
    End Sub
    Public Overrides Sub OnSaveEvent(sender As Object, e As System.Windows.RoutedEventArgs)

    End Sub
    Public Overrides Sub ReadFromXMl(path As String)

    End Sub

    Public Overrides Sub SetContextMenu()
        '' add sub contextmenus
        Dim c As New ContextMenu

        Dim AddStructureData As New MenuItem()
        AddStructureData.Header = "Import Structures"
        'AddHandler AddStructureData.Click, AddressOf AddStructures

        'Dim AddByNSI As New MenuItem
        'AddByNSI.Header = "From NSI"
        'AddHandler AddByNSI.Click, AddressOf NSIImportFunction
        'AddStructureData.Items.Add(AddByNSI)

        Dim AddByHAZUS As New MenuItem
        AddByHAZUS.Header = "From HAZUS"
        AddHandler AddByHAZUS.Click, AddressOf ImportByHAZUS
        'AddByHAZUS.IsEnabled = False
        AddStructureData.Items.Add(AddByHAZUS)

        Dim AddByPoint As New MenuItem
        AddByPoint.Header = "From Shapefile"
        AddHandler AddByPoint.Click, AddressOf ImportByPointShapefile
        AddStructureData.Items.Add(AddByPoint)


        'Dim AddByNSIMAIN As New MenuItem
        'AddByNSIMAIN.Header = "From _NSI"
        'AddHandler AddByNSIMAIN.Click, AddressOf NSIImportFunction

        Dim AddByHAZUSMain As New MenuItem
        AddByHAZUSMain.Header = "From _HAZUS"
        AddHandler AddByHAZUSMain.Click, AddressOf ImportByHAZUS

        Dim AddByPointMain As New MenuItem
        AddByPointMain.Header = "From _Shapefile"
        AddHandler AddByPointMain.Click, AddressOf ImportByPointShapefile

        For Each mi As MenuItem In GetMainContextMenu.Items
            If mi.Header = "_Study" Then
                For Each submi As MenuItem In mi.Items
                    If submi.Header = "_Inventory" Then
                        Dim importstructs As New MenuItem
                        importstructs.Header = "_Import Structures"
                        'importstructs.Items.Add(AddByNSIMAIN)
                        importstructs.Items.Add(AddByHAZUSMain)
                        importstructs.Items.Add(AddByPointMain)
                        submi.Items.Add(importstructs)
                    End If
                Next
            End If
        Next

        c.Items.Add(AddStructureData)
        MyBase.ContextMenu = c
    End Sub
    Public Overrides Sub RemoveMainMenuContextOptions()
        For Each mi As MenuItem In GetMainContextMenu.Items
            If mi.Header = "_Study" Then
                For Each submi As MenuItem In mi.Items
                    If submi.Header = "_Inventory" Then
                        For Each subsubmi As MenuItem In submi.Items
                            If subsubmi.Header = "_Import Structures" Then
                                For Each sssmi As MenuItem In subsubmi.Items
                                    'If sssmi.Header = "From _NSI" Then RemoveHandler sssmi.Click, AddressOf NSIImportFunction
                                    If sssmi.Header = "From _HAZUS" Then RemoveHandler sssmi.Click, AddressOf ImportByHAZUS
                                    If sssmi.Header = "From _Shapefile" Then RemoveHandler sssmi.Click, AddressOf ImportByPointShapefile
                                    subsubmi.Items.Remove(sssmi)
                                Next
                                submi.Items.Remove(subsubmi)
                            End If
                        Next

                    End If
                Next
            End If
        Next
        MyBase.RemoveMainMenuContextOptions()
    End Sub
    Public Overrides Sub WriteToXML()

    End Sub
    Public Overrides Sub AddFrameworkChildren()
        'add any inventory subnodes
    End Sub
    Public Overrides Sub AddFrameworkChildren(ele As System.Xml.Linq.XElement)
        readfromxmlelement(ele)
        Dim dinfo As New System.IO.DirectoryInfo(GetCurrentDirectory)
        Dim cleandir As Boolean = False
        For Each file As System.IO.FileInfo In dinfo.GetFiles
            Dim keepfile As Boolean = False
            For Each Header As FrameworkTreeNode In FirstLevelSubNodes
                If System.IO.Path.GetFileNameWithoutExtension(file.FullName) = Header.Header Then
                    keepfile = True
                End If
            Next
            If Not keepfile Then
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
            For Each Header As FrameworkTreeNode In FirstLevelSubNodes
                If System.IO.Path.GetFileNameWithoutExtension(file.FullName) = Header.Header Then
                    keepfile = True
                End If
            Next
            If Not keepfile And Not file.Attributes = IO.FileAttributes.System Then
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
                        If (GetMapTreeView.GetAllFeatureNodes(j).Features.Features Is Nothing) Then Continue For
                        If GetMapTreeView.GetAllFeatureNodes(j).Features.Features.GetSource = filelist(i) Then
                            ''If GetMapTreeView.GetAllFeatureNodes(j).EditingMode Then
                            ''MsgBox("The Feature " & GetMapTreeView.GetAllFeatureNodes(j).FeatureNodeHeader & " is in edit mode would you like to abort?")
                            '' End If
                            'GetMapTreeView.GetAllFeatureNodes(j).RemoveLayer(True)
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
        'add any inventory subnodes
        For Each element As XElement In xele.Elements
            If System.IO.File.Exists(ConvertFromRelativePath(element.Attribute("Path"))) Then
                Dim s As New StructureInventoryChildTreeNode(element)
                AddFrameworkTreeNode(s)
                s.AddFrameworkChildren(element)
            Else
                MsgBox(ConvertFromRelativePath(element.Attribute("Path")) & vbNewLine & "File not Found")
            End If

        Next
    End Sub
End Class
