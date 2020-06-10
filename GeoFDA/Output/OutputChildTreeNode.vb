Public Class OutputChildTreeNode
    Inherits FrameworkTreeNode
    Private Const _ParentFolder As String = "FDA Import File"
    Private _description As String
    Private _occtype As OccupancyTypeTreeNode
    Private _structureinventory As StructureInventoryChildTreeNode
    Private _damagereaches As ImpactAreaChildTreeNode
    Private _plans As List(Of HydraulicsChildTreenode)
    Private _RequiresRecompute As Boolean = False
    Private _LastComputedDate As DateTime = Nothing
    Sub New(ByVal uniqueName As String, ByVal description As String, ByVal StructureInventory As StructureInventoryChildTreeNode, ByVal occtype As OccupancyTypeTreeNode, ByVal DamageReaches As ImpactAreaChildTreeNode, ByVal selectedplans As List(Of HydraulicsChildTreenode))
        MyBase.New(uniqueName)
        _description = description
        _structureinventory = StructureInventory
        _occtype = occtype 'CType(GetAllFrameworkTreenodesOfType(GetType(OccupancyTypeTreeNode))(0), OccupancyTypeTreeNode)
        _damagereaches = DamageReaches
        _plans = selectedplans

    End Sub
    Sub New(ByVal xele As XElement)
        MyBase.New(xele.Attribute("Header").Value)
    End Sub
    Public Overrides Sub AddFrameworkChildren()
        For i As Integer = 0 To _plans.Count - 1
            Dim pctn As New PlanChildTreeNode(_damagereaches, _plans(i))
            AddHandler pctn.IndexLocationsAreReadyEvent, AddressOf statusreport
            Me.AddFrameworkTreeNode(pctn)
            pctn.AddFrameworkChildren()
        Next
        statusreport(Nothing, Nothing)
    End Sub
    Public Property GetStructureInventoryNode As StructureInventoryChildTreeNode
        Get
            Return _structureinventory
        End Get
        Set(value As StructureInventoryChildTreeNode)
            _structureinventory = value
        End Set
    End Property
    Public Property GetOccType As OccupancyTypeTreeNode
        Get
            Return _occtype
        End Get
        Set(value As OccupancyTypeTreeNode)
            _occtype = value
        End Set
    End Property
    Public Property GetPlans As List(Of HydraulicsChildTreenode)
        Get
            Return _plans
        End Get
        Set(value As List(Of HydraulicsChildTreenode))
            _plans = value
        End Set
    End Property
    Public Property GetImpactAreas As ImpactAreaChildTreeNode
        Get
            Return _damagereaches
        End Get
        Set(value As ImpactAreaChildTreeNode)
            _damagereaches = value
        End Set
    End Property
    Public Overrides ReadOnly Property GetNodeName As String
        Get
            Return Me.GetType.Name
        End Get
    End Property

    Public Overrides ReadOnly Property GetSubDirectory As String
        Get
            Return _ParentFolder & "\" & Me.Header
        End Get
    End Property

    Public Overrides Sub OnSaveEvent(sender As Object, e As System.Windows.RoutedEventArgs)

    End Sub
    Public Sub AlertUserThatUpdatesAreNeeded(ByVal message As String, ByVal askusertorecompute As Boolean, ByVal requiresuserinputstochange As Boolean)
        If Not OutputExists() Then Exit Sub
        If Not IndexLocationsAreReady() Then Exit Sub
        For Each mi As MenuItem In MyBase.ContextMenu.Items
            If mi.Header = "Compute Econ Import File" Then
                mi.IsEnabled = Not requiresuserinputstochange
            End If
        Next
        For Each mi As MenuItem In MyBase.ContextMenu.Items
            If mi.Header = "Zip" Then
                mi.IsEnabled = Not askusertorecompute
            End If
        Next
        ReportMessage(message & " for " & Header & " " & Now.ToString)
        _RequiresRecompute = askusertorecompute Or requiresuserinputstochange
        If requiresuserinputstochange Then
            If askusertorecompute Then
                MsgBox("You need to recompute, but before you recompute, there are errors to address")
                If MyBase.ToolTip = Nothing Then
                    MyBase.ToolTip = "Data has changed that requires a recompute."
                Else
                    If MyBase.ToolTip.ToString.Contains("Data has changed that requires a recompute.") Then

                    Else
                        MyBase.ToolTip = MyBase.ToolTip & vbNewLine & "Data has changed that requires a recompute."
                    End If
                End If
                MyBase.Foreground = Brushes.Red
                _RequiresRecompute = True
            Else
                If MyBase.ToolTip = Nothing Then
                    MyBase.ToolTip = "Data has changed that requires a recompute."
                Else
                    If MyBase.ToolTip.ToString.Contains("Data has changed that requires a recompute.") Then

                    Else
                        MyBase.ToolTip = MyBase.ToolTip & vbNewLine & "Data has changed that requires a recompute."
                    End If
                End If
                MyBase.Foreground = Brushes.Red
                _RequiresRecompute = True
            End If
        Else
            If askusertorecompute Then
                If MsgBox(message & vbNewLine & "Would you like to recompute " & MyBase.Header & "?", MsgBoxStyle.YesNo, "Data has changed") = MsgBoxResult.Yes Then
                    ComputeExisting(Nothing, Nothing)
                    MyBase.Cursor = Nothing
                Else
                    ''they said no.
                    If MyBase.ToolTip = Nothing Then
                        MyBase.ToolTip = "Data has changed that requires a recompute."
                    Else
                        If MyBase.ToolTip.ToString.Contains("Data has changed that requires a recompute.") Then

                        Else
                            MyBase.ToolTip = MyBase.ToolTip & vbNewLine & "Data has changed that requires a recompute."
                        End If
                    End If
                    MyBase.Foreground = Brushes.Red
                    _RequiresRecompute = True
                End If
            Else
                If MyBase.ToolTip = Nothing Then
                    MyBase.ToolTip = "Data has changed that requires a recompute."
                Else
                    If MyBase.ToolTip.ToString.Contains("Data has changed that requires a recompute.") Then

                    Else
                        MyBase.ToolTip = MyBase.ToolTip & vbNewLine & "Data has changed that requires a recompute."
                    End If
                End If
                MyBase.Foreground = Brushes.Red
                _RequiresRecompute = True
            End If
        End If
    End Sub
    Private Function OutputExists() As Boolean
        If System.IO.File.Exists(GetCurrentDirectory & "\" & Header & ".fdai") Then Return True
        If System.IO.File.Exists(GetCurrentDirectory & "\" & Header & ".zip") Then Return True
        Return False
    End Function
    Public Overrides Sub ReadFromXMl(path As String)

    End Sub
    Public Overrides Sub SetContextMenu()
        Dim c As New ContextMenu

        Dim Properties As New MenuItem()
        Properties.Header = "Properties"
        AddHandler Properties.Click, AddressOf PropertiesForOutputTreeNode
        c.Items.Add(Properties)

        Dim Rename As New MenuItem()
        Rename.Header = "Rename"
        AddHandler Rename.Click, AddressOf RenameOutputTreeNode
        c.Items.Add(Rename)

        Dim SaveAs As New MenuItem()
        SaveAs.Header = "Save As"
        AddHandler SaveAs.Click, AddressOf SaveAsOutputTreeNode
        c.Items.Add(SaveAs)


        Dim RemoveFromStudy As New MenuItem()
        RemoveFromStudy.Header = "Remove From Study"
        AddHandler RemoveFromStudy.Click, AddressOf RemoveFromStudyOutputTreeNode
        c.Items.Add(RemoveFromStudy)


        Dim Compute As New MenuItem()
        Compute.Header = "Compute Econ Import File"
        AddHandler Compute.Click, AddressOf ComputeExisting
        Compute.IsEnabled = False
        c.Items.Add(Compute)

        Dim Zip As New MenuItem()
        Zip.Header = "Zip"
        AddHandler Zip.Click, AddressOf Zipper
        Zip.IsEnabled = False
        c.Items.Add(Zip)

        MyBase.ContextMenu = c
    End Sub

    Private Sub PropertiesForOutputTreeNode(sender As Object, e As RoutedEventArgs)
        Dim msg As New System.Text.StringBuilder
        msg.AppendLine("Econ Import File Name: " & MyBase.Header)
        msg.AppendLine("Description: " & _description)
        If IsNothing(_LastComputedDate) OrElse _LastComputedDate.ToString = "1/1/0001 12:00:00 AM" Then
            msg.AppendLine("Last Computed Date: Never Computed")
        Else
            msg.AppendLine("Last Computed Date: " & _LastComputedDate.ToString)
        End If

        msg.AppendLine("Needs Recompute: " & _RequiresRecompute.ToString)
        msg.AppendLine("Structure Inventory: " & _structureinventory.Header)
        msg.AppendLine("Impact Areas: " & _damagereaches.Header)
        msg.AppendLine("Hydraulic Plans: ")
        Dim gridtype As String = ""
        For Each h As HydraulicsChildTreenode In _plans
            msg.AppendLine("Plan Name : " & h.Header)
            If h.IsDepthGrid Then
                gridtype = "Depth Grids"
            Else
                gridtype = "Water Surface Elevation Grids"
            End If
            msg.AppendLine("    Plan Year: " & h.GetYear)
            msg.AppendLine("    Grid Type: " & gridtype)
        Next
        MsgBox(msg.ToString, MsgBoxStyle.OkOnly, "Econ Import File Properties")
    End Sub

    Private Sub DeletePlanFiles()
        If System.IO.File.Exists(GetCurrentDirectory & "\" & Header & ".fdai") Then System.IO.File.Delete(GetCurrentDirectory & "\" & Header & ".fdai")
        If System.IO.File.Exists(GetCurrentDirectory & "\" & Header & ".xml") Then System.IO.File.Delete(GetCurrentDirectory & "\" & Header & ".xml")
        If System.IO.File.Exists(GetCurrentDirectory & "\" & Header & ".zip") Then System.IO.File.Delete(GetCurrentDirectory & "\" & Header & ".zip")
        _LastComputedDate = Nothing
    End Sub
    Sub statusreport(sender As Object, e As RoutedEventArgs)
        If IndexLocationsAreReady() Then
            If _RequiresRecompute Then
            Else
                MyBase.Foreground = Brushes.Black
                MyBase.ToolTip = Nothing
            End If

            For Each mi As MenuItem In MyBase.ContextMenu.Items
                If mi.Header = "Compute Econ Import File" Then
                    mi.IsEnabled = True
                End If
            Next
        Else
            MyBase.Foreground = Brushes.Red
            MyBase.ToolTip = "Please finish defining all of the index location information files"
        End If
    End Sub
    Private Function IndexLocationsAreReady() As Boolean
        For i = 0 To FirstLevelSubNodes.Count - 1
            If Not CType(FirstLevelSubNodes(i), PlanChildTreeNode).IndexLocationsAreReady Then Return False
        Next
        Return True
    End Function
    Public Function GetPathsAndFileTypes() As List(Of ZipWizardRowItems)
        Dim list As New List(Of ZipWizardRowItems)
        'get the type of Hydraulic Grid nodes so that we can determine which files to add.
        'all plans must share watershed. so only retrieve those files once.
        Dim basedirectory As String = Left(GetRootDirectory, Len(GetRootDirectory) - 1)
        If Not IsNothing(_plans(0).GetTerrain) Then
            list.Add(New ZipWizardRowItems(_plans(0).GetTerrain.GetTerrainPath, "Terrain", basedirectory))
        End If
        'If Not IsNothing(_plans(0).GetStream) Then
        '    list.Add(New ZipWizardRowItems(_plans(0).GetWatershed.GetStream.GetStreamPath, "Stream", basedirectory))
        'End If
        'If Not IsNothing(_plans(0).GetWatershed.GetCrossSections) Then
        '    list.Add(New ZipWizardRowItems(_plans(0).GetWatershed.GetCrossSections.GetCrossSectionShapefilePath, "Cross Sections", basedirectory))
        '    list.Add(New ZipWizardRowItems(_plans(0).GetWatershed.GetCrossSections.GetStprageAreaShapefilePath, "Storage Areas", basedirectory))
        'End If
        Select Case _plans(0).GetType 'all plans must be of the same type
            Case GetType(HydraulicsChildTreenode)
                Dim ghct As HydraulicsChildTreenode
                Dim gridtype As String = ""
                For i = 0 To _plans.Count - 1
                    ghct = CType(_plans(i), HydraulicsChildTreenode)
                    If ghct.IsDepthGrid Then
                        gridtype = "Depth Grid "
                    Else
                        gridtype = "WSE Grid "
                    End If
                    For j = 0 To ghct.GetGridPaths.Count - 1
                        list.Add(New ZipWizardRowItems(ghct.GetGridPaths(j), gridtype & ghct.GetProbabilities(j).ToString, basedirectory))
                    Next
                Next
                'Case GetType(CrossSectionChildTreeNode)
                '    Dim cshct As CrossSectionHydraulicsChildTreenode
                '    For i = 0 To _plans.Count - 1
                '        cshct = CType(_plans(i), CrossSectionHydraulicsChildTreenode)
                '        list.Add(New ZipWizardRowItems(cshct.GetDSSPath, "DSS File", basedirectory))
                '    Next
            Case Else
                'should never happen
        End Select

        list.Add(New ZipWizardRowItems(CType(GetAllFrameworkTreenodesOfType(GetType(DamageCategoryTreeNode))(0), DamageCategoryTreeNode).GetDamageCategoryPath, "Damage Category File", basedirectory))

        list.Add(New ZipWizardRowItems(_occtype.getOcctypeFilepath, "Occupancy Type File", basedirectory))
        list.Add(New ZipWizardRowItems(_structureinventory.GetStructurePath, "Structure Inventory Shapefile", basedirectory))
        list.Add(New ZipWizardRowItems(_damagereaches.GetImpactAreaPath, "Impact Area Shapefile", basedirectory))
        ' all of the index location files.
        'for i = 0 to _damagereaches.count bla bla bla.
        Dim dbr As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(_damagereaches.GetImpactAreaPath, ".dbf"))
        Dim impactareas As String() = Array.ConvertAll(dbr.GetColumn("ImpactArea"), New Converter(Of Object, String)(Function(x) Convert.ToString(x)))
        For i = 0 To _plans.Count - 1
            For j = 0 To impactareas.Count - 1
                If System.IO.File.Exists(GetCurrentDirectory & "\" & _plans(i).Header & "\" & impactareas(j) & ".xml") Then
                    list.Add(New ZipWizardRowItems(GetCurrentDirectory & "\" & _plans(i).Header & "\" & impactareas(j) & ".xml", "Index file", basedirectory))
                End If
            Next
        Next
        dbr.Close()
        'ascii file
        list.Add(New ZipWizardRowItems(GetCurrentDirectory & "\" & Header & ".fdai", "FDA Ascii import file", basedirectory))
        'plan file
        '
        list.Add(New ZipWizardRowItems(GetCurrentDirectory & "\" & Me.Header & ".xml", "Ascii Source File", basedirectory))
        Return list
    End Function

    Private Function isComputeErrorFree() As Boolean
        Dim isNoErrors As Boolean = True
        Dim dbr As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(_damagereaches.GetImpactAreaPath, ".dbf"))
        Dim impactareas As String() = Array.ConvertAll(dbr.GetColumn("ImpactArea"), New Converter(Of Object, String)(Function(x) Convert.ToString(x)))
        Dim stations As Double() = Array.ConvertAll(dbr.GetColumn("Index"), New Converter(Of Object, Double)(Function(x) Double.Parse(x.ToString)))
        dbr.Close()
        Dim _directory As String = GetCurrentDirectory & "\" '& Me.Header & "\"
        Dim pct As PlanChildTreeNode
        For i = 0 To FirstLevelSubNodes.Count - 1
            pct = CType(FirstLevelSubNodes(i), PlanChildTreeNode)
            If Not pct.IndexLocationsAreReady Then
                ReportMessage("The Plan: " & FirstLevelSubNodes(i).Header & " does not have all index locations defined.")
                isNoErrors = False
            Else
                ''possibly unnecessary to check this since i no longer allow invalid saves on the ok click for the index locations form.
                Dim idx As New XMLIndexLocation
                Dim msg As String
                For j = 0 To impactareas.Count - 1
                    idx.ReadFromXml(_directory & pct.Header & "\" & impactareas(j) & ".xml")
                    msg = idx.Validate
                    If Not msg = "" Then ReportMessage("For the reach named " & idx.ReachName & " the following errors exist" & vbNewLine & msg) : isNoErrors = False
                Next

            End If
        Next
        If _occtype.checkforerrors Then ReportMessage("Occtypes have errors, aborting compute") : isNoErrors = False
        Dim dtypes As DamageCategoryTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(DamageCategoryTreeNode))(0), DamageCategoryTreeNode)
        If dtypes.CheckForErrors Then ReportMessage("Damage Categories have errors, aborting compute") : isNoErrors = False
        Dim fnlist As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))
        Dim s As StudyTreeNode = CType(fnlist(0), StudyTreeNode)
        If s.CheckForErrors Then ReportMessage("There is a problem with the base and future years") : isNoErrors = False
        If _structureinventory.ContainsErrors Then ReportMessage("There are errors with the structure inventory") : isNoErrors = False

        Return isNoErrors
    End Function

    Private Sub ComputeExisting(sender As Object, e As System.Windows.RoutedEventArgs)
        'check index locations exist
        MyBase.Cursor = Cursors.Wait
        If Not isComputeErrorFree() Then MyBase.Cursor = Nothing : Exit Sub

        Dim fnlist As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))
        Dim s As StudyTreeNode = CType(fnlist(0), StudyTreeNode)
        'write all of the current settings to xml
        WriteToXML()
        Dim messages As New System.Text.StringBuilder
        Dim messagecount As Integer = 0
        'create a list of damage categories
        'create a list of occupancy types
        Dim tmpdamcat As String
        Dim tmpocctype As String
        'get the structures
        Dim structurereader As New LifeSimGIS.ShapefileReader(_structureinventory.GetStructurePath)
        Dim structureshp As LifeSimGIS.PointFeatures = structurereader.ToFeatures
        ''create a projection
        Dim structuresprj As GDALAssist.Projection
        If System.IO.File.Exists(System.IO.Path.ChangeExtension(_structureinventory.GetStructurePath, ".prj")) Then
            structuresprj = New GDALAssist.ESRIProjection(System.IO.Path.ChangeExtension(_structureinventory.GetStructurePath, ".prj"))
            structuresprj = New GDALAssist.WKTProjection(structuresprj.GetWKT)
            structuresprj = New GDALAssist.Proj4Projection(structuresprj.GetProj4)
        Else
            structuresprj = New GDALAssist.WKTProjection("")
        End If
        ''what if the path doesnt exist?

        Dim structuredbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(_structureinventory.GetStructurePath, ".dbf"))
        Dim structs As New List(Of ComputableObjects.FDAStructure)

        'sort structures into damage reaches.
        Dim damagereachreader As New LifeSimGIS.ShapefileReader(_damagereaches.GetImpactAreaPath)
        Dim damageReachShp As LifeSimGIS.PolygonFeatures = damagereachreader.ToFeatures
        Dim damagereachprj As GDALAssist.Projection
        If System.IO.File.Exists(System.IO.Path.ChangeExtension(_damagereaches.GetImpactAreaPath, ".prj")) Then
            damagereachprj = New GDALAssist.ESRIProjection(System.IO.Path.ChangeExtension(_damagereaches.GetImpactAreaPath, ".prj"))
            damagereachprj = New GDALAssist.WKTProjection(damagereachprj.GetWKT)
            damagereachprj = New GDALAssist.Proj4Projection(damagereachprj.GetProj4)
        Else
            damagereachprj = New GDALAssist.WKTProjection("")
        End If
        Dim damagereachdbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(_damagereaches.GetImpactAreaPath, ".dbf"))

        'create a dictionary of damagereach/structures for gridded computes
        Dim structureDict As New Dictionary(Of Integer, List(Of ComputableObjects.FDAStructure))


        'load the terrain file

        Dim tgrid As New LifeSimGIS.RasterFeatures(_plans(0).GetTerrain.GetTerrainPath, False)
        Dim tgridprj As GDALAssist.Projection
        Using gdr As GDALAssist.GDALRaster = New GDALAssist.GDALRaster(_plans(0).GetTerrain.GetTerrainPath)
            tgridprj = gdr.GetProjection()
        End Using
        '' check projections
        If tgridprj.IsValid <> GDALAssist.SRSValidation.Corrupt Then
            If structuresprj.IsValid <> GDALAssist.SRSValidation.Corrupt Then
                If Not OpenGLMapping.MapTreeView.DeepProjectionEqual(tgridprj, structuresprj) Then
                    messages.Append("Reprojecting the Structure Inventory")
                    structureshp.Reproject(structuresprj, tgridprj)
                End If
            Else
                ''abort
                ReportMessage("Structure Projection is Corrupt and not equal to the terrain projection")
                MsgBox("Error with the Structure Inventory projection") : Exit Sub
            End If
            If damagereachprj.IsValid <> GDALAssist.SRSValidation.Corrupt Then
                If Not OpenGLMapping.MapTreeView.DeepProjectionEqual(damagereachprj, tgridprj) Then
                    messages.Append("Reprojecting the Impact Areas")
                    damageReachShp.Reproject(damagereachprj, tgridprj)
                End If
            Else
                ''abort?
                ReportMessage("Impact Area Projection is Corrupt and not equal to the terrain projection")
                MsgBox("Error with the Impact Area projection") : Exit Sub
            End If
        Else
            '' abort?
            ReportMessage("Terrain Projection is Corrupt")
            MsgBox("Error with the Terrain projection") : Exit Sub
        End If
        Dim indexes() As Integer = damageReachShp.OptimizedPointInPolygonsGDI(structureshp)
        Dim groundelevations() As Single = tgrid.GridReader.SampleValues(structureshp.Points.ToArray)
        Dim rejectcount As Integer = 0
        For i = 0 To indexes.Count - 1
            If indexes(i) = -1 Then
                'not in a damage reach, reject.
                Debug.Print("The Structure: " & structuredbf.GetCell("St_Name", i) & " was not in an Impact Area")
                messages.AppendLine("The Structure: " & structuredbf.GetCell("St_Name", i) & " was not in an Impact Area")
                messagecount += 1
                If messagecount Mod 100 = 0 Then ReportMessage(messages.ToString) : messages.Clear()
                rejectcount += 1
            Else
                If groundelevations(i) = tgrid.GridReader.NoData(0) Then
                    'doesnt have a terrain value
                    Debug.Print("The Structure: " & structuredbf.GetCell("St_Name", i) & " was in Impact Area " & damagereachdbf.GetCell("ImpactArea", indexes(i)) & " but did not have a valid ground elevation")
                    messages.AppendLine("The Structure: " & structuredbf.GetCell("St_Name", i) & " was in Impact Area " & damagereachdbf.GetCell("ImpactArea", indexes(i)) & " but did not have a valid ground elevation")
                    messagecount += 1
                    If messagecount Mod 100 = 0 Then ReportMessage(messages.ToString) : messages.Clear()
                    rejectcount += 1
                Else
                    'each occupancy type must live in only one damage category
                    tmpdamcat = structuredbf.GetCell("DamCat", i)
                    tmpocctype = structuredbf.GetCell("OccType", i)
                    Dim ot As New ComputableObjects.OccupancyType(tmpocctype, tmpdamcat)

                    structs.Add(New ComputableObjects.FDAStructure(structuredbf.GetCell("St_Name", i), ot, structuredbf.GetCell("Val_Struct", i), structuredbf.GetCell("Found_Ht", i)))
                    structs(i - rejectcount).Location = structureshp.Points(i)
                    structs(i - rejectcount).SidReach = damagereachdbf.GetCell("ImpactArea", indexes(i))


                    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                    ' If the user didn't define GE in the dbf then use the ground elevations from the terrain. Otherwise default to what they set.
                    If Not structuredbf.GetCell("UseDBF_GE", i) Then
                        structs(i - rejectcount).GroundEle = groundelevations(i) 'what if the ground elevation has already been set? what if they are using a FFE?
                    Else
                        structs(i - rejectcount).GroundEle = structuredbf.GetCell("Ground_Ht", i)
                    End If
                    ' If the user didn't define the FFE, then compute it using foundation height. Otherwise default to what they set.
                    If Not structuredbf.GetCell("UseFFE", i) Then
                        structs(i - rejectcount).FirstFloorElevation = structs(i - rejectcount).FH + structs(i - rejectcount).GroundEle
                    Else
                        structs(i - rejectcount).FirstFloorElevation = structuredbf.GetCell("FFE", i)
                    End If
                    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''



                    ''"Val_Cont"
                    structs(i - rejectcount).ContentValue = structuredbf.GetCell("Val_Cont", i)
                    ''"Val_Other"
                    structs(i - rejectcount).ContentValue = structuredbf.GetCell("Val_Other", i)
                    Select Case _plans(0).GetType.Name
                        Case "HydraulicsChildTreenode"
                            structs(i - rejectcount).Stationing = (i + 1) 'only for gridded plans
                            structs(i - rejectcount).Stream = s.Header 'this is the name of the import file.  this can screw everythign up later...
                    End Select
                    If structureDict.ContainsKey(indexes(i)) Then
                        structureDict.Item(indexes(i)).Add(structs(i - rejectcount))
                    Else
                        structureDict.Add(indexes(i), New List(Of ComputableObjects.FDAStructure))
                        structureDict.Item(indexes(i)).Add(structs(i - rejectcount))
                    End If
                End If
            End If
        Next
        Dim inventory As New ComputableObjects.Consequences_Inventory(structs)
        'create wse's for each plan for each damage reach?

        Dim planinfos As New List(Of PlanInfo)
        Dim rivers As New List(Of Stream)
        Dim reaches As New List(Of Reach)
        Dim wsps As New System.Text.StringBuilder
        Select Case _plans(0).GetType.Name
            Case "CrossSectionHydraulicsChildTreenode"
                'loop trough the plans
                'loop through the f parts for each plan
            Case "HydraulicsChildTreenode"
                Dim gwps As New List(Of Gridded_WSP)
                rivers.Add(New Stream(s.Header, "added programmatically during Gridded Compute in GeoFDA"))
                For Each plan As HydraulicsChildTreenode In _plans
                    Dim gwp As New Gridded_WSP(plan.GetGridPaths.ToList, plan.IsDepthGrid, tgridprj)
                    planinfos.Add(New PlanInfo(plan.Header, plan.GetDescription))
                    'gather all relevant index locations
                    Dim indexlocations As New List(Of IndexLocation)
                    Dim xindex As New XMLIndexLocation
                    Dim reachesadded As Boolean = False
                    Dim damagereachnames() As String = Array.ConvertAll(damagereachdbf.GetColumn("ImpactArea"), New Converter(Of Object, String)(Function(x) Convert.ToString(x)))
                    For i = 0 To damagereachnames.Count - 1
                        indexlocations.Add(xindex.ReadFromXMLToIndex(GetCurrentDirectory & "\" & plan.Header & "\" & damagereachnames(i) & ".xml"))
                        indexlocations(i).SetRiverName = s.Header
                        If structureDict.ContainsKey(i) Then
                            gwp.ADDWSPINFO(indexlocations(i), structureDict(i), GetCurrentDirectory & "\" & plan.Header)
                            reaches = gwp.GetReaches
                            reachesadded = True
                        End If

                    Next
                    If reachesadded Then
                        wsps.Append(gwp.WriteToEconImportFormat(plan.Header, plan.GetYear))
                        gwps.Add(gwp)
                    End If
                Next
        End Select

        Dim si As New StudyInfo(planinfos, rivers, reaches, s.GetBaseYear, s.GetFutureYear)
        Dim dcatnode As DamageCategoryTreeNode = GetAllFrameworkTreenodesOfType(GetType(DamageCategoryTreeNode))(0)
        Dim dcats As New ComputableObjects.DamageCategories(dcatnode.GetDamageCategoryPath)
        Dim occtypenode As OccupancyTypeTreeNode = GetAllFrameworkTreenodesOfType(GetType(OccupancyTypeTreeNode))(0)
        Dim otypes As New ComputableObjects.OccupancyTypes(occtypenode.getOcctypeFilepath)

        Dim econtxtfile As New System.Text.StringBuilder
        econtxtfile.Append(si.ToString)

        econtxtfile.AppendLine(dcats.WriteToFDAString)
        econtxtfile.Append(otypes.WriteToFDAString)
        econtxtfile.Append(inventory.WriteToFDAString)

        econtxtfile.Append(wsps.ToString)
        'write out the txt file.
        Dim fs As New System.IO.FileStream(GetCurrentDirectory & "\" & Header & ".fdai", IO.FileMode.Create)
        Dim sw As New System.IO.StreamWriter(fs)
        sw.Write(econtxtfile.ToString)
        sw.Dispose() : sw.Close()
        fs.Dispose() : fs.Close()

        If messages.Length > 0 Then ReportMessage(messages.ToString)
        'For Each mi As MenuItem In MyBase.ContextMenu.Items
        '    If mi.Header = "Zip" Then
        '        mi.IsEnabled = True
        '    End If
        'Next
        _LastComputedDate = Now
        ReportMessage("Computation completed for " & Header & ". " & _LastComputedDate)
        If messages.Length > 0 Then
            MsgBox("Computation completed for " & Header & vbNewLine & "Compute messages were added to the log file.")
        Else
            MsgBox("Computation completed for " & Header)
        End If
        MyBase.Cursor = Nothing
        EnableZipMenuItem()
        _RequiresRecompute = False
        statusreport(Nothing, Nothing)
        'ZipResults()
    End Sub
    Private Sub EnableZipMenuItem()
        For Each mi As MenuItem In MyBase.ContextMenu.Items
            If mi.Header = "Zip" Then
                mi.IsEnabled = True
            End If
        Next
    End Sub
    Private Sub DisableZipMenuItem()
        For Each mi As MenuItem In MyBase.ContextMenu.Items
            If mi.Header = "Zip" Then
                mi.IsEnabled = False
            End If
        Next
    End Sub
    Private Sub Zipper(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim fnlist As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))
        Dim s As StudyTreeNode = CType(fnlist(0), StudyTreeNode)

        Dim zw As New ZipWizard(s.GetProjectFile, GetMapTreeView, Me, Left(GetRootDirectory, Len(GetRootDirectory) - 1), GetCurrentDirectory & "\" & Me.Header & ".zip")
        If zw.ShowDialog() Then
            s.WriteToXML()
        Else

        End If
    End Sub
    Public Overrides Sub AddFrameworkChildren(ele As System.Xml.Linq.XElement)
        ReadFromXMLElement(ele)

        'AddFrameworkChildren()
    End Sub
    Public Overrides Sub WriteToXML()
        Dim root As New XElement("FDA_ASCII_Import_File")
        root.SetAttributeValue("Path", ".\" & Me.Header & ".txt")
        Dim proj As New XElement("GeoFDA_Project_File")
        Dim fnlist As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))
        Dim s As StudyTreeNode = CType(fnlist(0), StudyTreeNode)
        proj.SetAttributeValue("Path", ConvertToRelativePath(s.GetProjectFile))
        root.Add(proj)
        Dim map As New XElement("GeoFDA_Current_Map_File")
        map.SetAttributeValue("Path", ".\" & s.Header & "MapProperties.xml")
        root.Add(map)
        Dim nodes As New XElement("Data_Elements")
        nodes.Add(_plans(0).GetTerrain.WriteToXMLElement)
        nodes.Add(_damagereaches.WriteToXMLElement)
        Dim plans As New XElement("Plans")
        For i = 0 To _plans.Count - 1
            plans.Add(_plans(i).WriteToXMLElement)
        Next
        nodes.Add(plans)
        root.Add(nodes)
        Dim xdoc As New XDocument
        xdoc.Add(root)
        xdoc.Save(GetCurrentDirectory & "\" & Me.Header & ".xml")
    End Sub
    Public Overrides Sub ReadFromXMLElement(xele As System.Xml.Linq.XElement)
        If Not IsNothing(xele.Attribute("Description")) Then
            _description = xele.Attribute("Description").Value
        End If
        Dim ats As IEnumerable(Of XAttribute) = xele.Attributes
        For i = 0 To ats.Count - 1
            If ats(i).Name = "LastComputedDate" Then

                Dim computetime As String = xele.Attribute("LastComputedDate").Value
                If computetime = "Never" Then
                    _RequiresRecompute = False
                Else
                    EnableZipMenuItem()
                    _LastComputedDate = DateTime.Parse(computetime)
                    _RequiresRecompute = Boolean.Parse(xele.Attribute("NeedsReCompute").Value)
                End If
            End If
        Next
        If _RequiresRecompute Then
            MyBase.Foreground = Brushes.Red
            MyBase.ToolTip = "Data has changed that requires a recompute."
        End If
        'refer to the watershed readfromxmelement
        _damagereaches = CType(GetFrameworkTreenodeByNameAndType(xele.Element("DamageReach").Value, GetType(ImpactAreaChildTreeNode)), ImpactAreaChildTreeNode)
        _structureinventory = CType(GetFrameworkTreenodeByNameAndType(xele.Element("Inventory").Value, GetType(StructureInventoryChildTreeNode)), StructureInventoryChildTreeNode)
        _occtype = CType(GetFrameworkTreenodeByNameAndType(xele.Element("Occtypes").Value, GetType(OccupancyTypeTreeNode)), OccupancyTypeTreeNode)
        Dim x As XElement = xele.Element("Plans")
        _plans = New List(Of HydraulicsChildTreenode)

        For Each p As XElement In x.Elements
            ats = p.Attributes
            Dim yearvalue As Integer = Nothing
            For i = 0 To ats.Count - 1
                If ats(i).Name = "PlanYear" Then
                    yearvalue = CInt(p.Attribute("PlanYear"))
                End If
            Next
            Select Case p.Attribute("Type").Value
                'Case "CrossSectionHydraulicsChildTreenode"
                '    _plans.Add(CType(GetFrameworkTreenodeByNameAndType(p.Value, GetType(CrossSectionChildTreeNode)), HydraulicsChildTreenode))
                Case "HydraulicsChildTreenode"
                    Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(HydraulicsChildTreenode))
                    For Each h As HydraulicsChildTreenode In nodes
                        If IsNothing(yearvalue) OrElse yearvalue = 0 Then
                            If h.Header = p.Value Then
                                _plans.Add(h)
                                Exit For
                            End If
                        Else
                            If h.Header = p.Value AndAlso h.GetYear = yearvalue Then
                                _plans.Add(h)
                            End If
                        End If
                    Next
            End Select
        Next
        Dim planinfos As XElement = xele.Element("PlanInfo")
        For Each p As XElement In planinfos.Elements
            Dim pctn As New PlanChildTreeNode(p)
            AddHandler pctn.IndexLocationsAreReadyEvent, AddressOf statusreport
            Me.AddFrameworkTreeNode(pctn)
            pctn.AddFrameworkChildren(p)
        Next
        'If System.IO.File.Exists(GetCurrentDirectory & "\" & Header & ".fdai") Then EnableZipMenuItem() 'probably should be revisited, maybe store a state and a date last created...
        statusreport(Nothing, Nothing)
    End Sub
    Public Overrides Function WriteToXMLElement() As System.Xml.Linq.XElement
        Dim ret As New XElement(GetNodeName)
        ret.SetAttributeValue("Header", Me.Header)
        ret.SetAttributeValue("Description", _description)
        If IsNothing(_LastComputedDate) OrElse _LastComputedDate.ToString = "1/1/0001 12:00:00 AM" Then
            ret.SetAttributeValue("LastComputedDate", "Never")
        Else
            ret.SetAttributeValue("LastComputedDate", _LastComputedDate.ToString)
        End If
        ret.SetAttributeValue("NeedsReCompute", _RequiresRecompute)
        Dim watershed As New XElement("Terrain", _plans(0).GetTerrain.Header)
        ret.Add(watershed)
        Dim occtype As New XElement("Occtypes", _occtype.Header)
        ret.Add(occtype)
        Dim inventory As New XElement("Inventory", _structureinventory.Header)
        ret.Add(inventory)
        Dim reach As New XElement("DamageReach", _damagereaches.Header)
        ret.Add(reach)
        Dim plans As New XElement("Plans")
        Dim plan As XElement
        For i = 0 To _plans.Count - 1
            plan = New XElement("Plan", _plans(i).Header)
            plan.SetAttributeValue("Type", _plans(i).GetType.Name)
            plan.SetAttributeValue("PlanYear", _plans(i).GetYear)
            plans.Add(plan)
        Next
        ret.Add(plans)
        Dim planinfo As New XElement("PlanInfo")
        For i = 0 To FirstLevelSubNodes.Count - 1
            planinfo.Add(FirstLevelSubNodes(i).WriteToXMLElement)
        Next
        ret.Add(planinfo)
        'add all the selected elements
        Return ret
    End Function
    Public Function CheckForUnecessaryFiles() As List(Of String)
        Dim result As New List(Of String)
        Dim plan As List(Of FrameworkTreeNode) = FirstLevelSubNodes
        For Each node As PlanChildTreeNode In plan
            result.AddRange(node.CheckForUnecessaryFiles)
        Next

        Dim dinfo As New System.IO.DirectoryInfo(GetCurrentDirectory)
        Dim finfo As System.IO.FileInfo() = dinfo.GetFiles
        Dim dirs As System.IO.DirectoryInfo() = dinfo.GetDirectories
        For Each d As System.IO.DirectoryInfo In dirs
            Dim deletedir As Boolean = True
            For i = 0 To _plans.Count - 1
                If d.Name = _plans(i).Header Then deletedir = False
            Next
            If deletedir Then result.Add(d.FullName)
        Next
        For Each file As System.IO.FileInfo In finfo
            If System.IO.Path.GetFileNameWithoutExtension(file.FullName) = MyBase.Header Or file.Attributes = IO.FileAttributes.System Then
            Else
                result.Add(file.FullName)
            End If
        Next
        Return result
    End Function
    Public Sub UpdatePlanFolderForRenameOfHydraulicsTreeNode(htn As HydraulicsChildTreenode, newname As String)
        Dim plannodes As List(Of FrameworkTreeNode) = FirstLevelSubNodes
        For Each plan As PlanChildTreeNode In plannodes
            If plan.HydraulicChildTreeNode.Header = htn.Header AndAlso plan.HydraulicChildTreeNode.GetYear = htn.GetYear Then
                'copy over xml files only
                If System.IO.Directory.Exists(GetCurrentDirectory & "\" & newname) Then
                Else
                    System.IO.Directory.CreateDirectory(GetCurrentDirectory & "\" & newname)
                End If
                Dim dbr As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(_damagereaches.GetImpactAreaPath, ".dbf"))
                Dim impactareas As String() = Array.ConvertAll(dbr.GetColumn("ImpactArea"), New Converter(Of Object, String)(Function(x) Convert.ToString(x)))
                For j = 0 To impactareas.Count - 1
                    If System.IO.File.Exists(GetCurrentDirectory & "\" & plan.Header & "\" & impactareas(j) & ".xml") Then
                        Dim newdestination As String = GetCurrentDirectory & "\" & newname & "\" & impactareas(j) & ".xml"
                        System.IO.File.Copy(GetCurrentDirectory & "\" & plan.Header & "\" & impactareas(j) & ".xml", newdestination)
                    End If
                Next
                System.IO.Directory.Delete(GetCurrentDirectory & "\" & plan.Header, True)
                AlertUserThatUpdatesAreNeeded("The Plan node named: " & plan.Header & " was renamed: " & newname & " all index files were copied over, and any computations should be recomputed", False, False)
            End If

        Next
    End Sub
    Private Sub SaveAsOutputTreeNode(sender As Object, e As RoutedEventArgs)
        'rename the files
        Dim rename As New Rename(Header)
        rename.Title = "Save As"
        rename.Owner = GetMainWindow
        If rename.ShowDialog() Then
            'check for name conflicts.
            Dim outNodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
            If outNodes.Count > 0 Then
                'potential for name conflicts
                Dim nameconflict As Boolean = False
                Do
                    nameconflict = False
                    For j = 0 To outNodes.Count - 1
                        If outNodes(j).Header = rename.NewName Then
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
            Dim newname As String = GetRootDirectory & "\" & "FDA Import File" & "\" & rename.NewName & "\"

            'copy files, kill old files, change header name.
            For i = 0 To _plans.Count - 1
                If Not System.IO.Directory.Exists(newname & "\" & _plans(i).Header) Then
                    System.IO.Directory.CreateDirectory(newname & _plans(i).Header)
                End If
            Next

            'copy over xml files only
            Dim dbr As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(_damagereaches.GetImpactAreaPath, ".dbf"))
            Dim impactareas As String() = Array.ConvertAll(dbr.GetColumn("ImpactArea"), New Converter(Of Object, String)(Function(x) Convert.ToString(x)))
            For i = 0 To _plans.Count - 1
                For j = 0 To impactareas.Count - 1
                    If System.IO.File.Exists(GetCurrentDirectory & "\" & _plans(i).Header & "\" & impactareas(j) & ".xml") Then

                        Dim newdestination As String = newname & "\" & _plans(i).Header & "\" & impactareas(j) & ".xml"
                        System.IO.File.Copy(GetCurrentDirectory & "\" & _plans(i).Header & "\" & impactareas(j) & ".xml", newdestination)
                    End If
                Next
            Next

            'if the old folder had a .fdai file then tell the user to recompute
            Dim parentfile As OutputParentTreeNode = CType(Parent, OutputParentTreeNode)
            Dim childfile As New OutputChildTreeNode(rename.NewName, _description & " Copied from " & Header, GetStructureInventoryNode, GetOccType, GetImpactAreas, GetPlans)
            parentfile.AddFrameworkTreeNode(childfile)
            childfile.AddFrameworkChildren()
            childfile.IndexLocationsAreReady()
            If System.IO.File.Exists(GetCurrentDirectory & "\" & Header & ".fdai") Then
                'MsgBox("Renaming requires that you re-compute.")
                childfile._RequiresRecompute = True
                childfile._LastComputedDate = Nothing
                childfile.statusreport(Nothing, Nothing)
            End If
            statusreport(Nothing, Nothing)

            'Save
            CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
        Else
            'user closed.
        End If
    End Sub
    Public Sub DeleteFromStudy()
        DeletePlanFiles()
        System.IO.Directory.Delete(GetCurrentDirectory, True) 'output folder and child hydraulics folder.
        ''remove from studytreenode
        Dim stn As StudyTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode)
        Dim sn As OutputParentTreeNode = CType(Parent, OutputParentTreeNode)
        sn.FirstLevelSubNodes.Remove(Me)
        sn.Items.Remove(Me)
        stn.RemoveFrameworkTreeNode(Me)
        stn.WriteToXML()
    End Sub
    Private Sub RemoveFromStudyOutputTreeNode(sender As Object, e As RoutedEventArgs)
        DeleteFromStudy()
    End Sub

    Private Sub RenameOutputTreeNode(sender As Object, e As RoutedEventArgs)
        Dim rename As New Rename(Header)
        rename.Owner = GetMainWindow
        If rename.ShowDialog() Then

            'check for name conflicts.
            Dim outNodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
            If outNodes.Count > 0 Then
                'potential for name conflicts
                Dim nameconflict As Boolean = False
                Do
                    nameconflict = False
                    For j = 0 To outNodes.Count - 1
                        If outNodes(j).Header = rename.NewName Then
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
            Dim newname As String = GetRootDirectory & "\" & "FDA Import File" & "\" & rename.NewName & "\"

            'copy files, kill old files, change header name.
            For i = 0 To _plans.Count - 1
                If Not System.IO.Directory.Exists(newname & "\" & _plans(i).Header) Then
                    System.IO.Directory.CreateDirectory(newname & _plans(i).Header)
                End If
            Next

            'copy over xml files only
            Dim dbr As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(_damagereaches.GetImpactAreaPath, ".dbf"))
            Dim impactareas As String() = Array.ConvertAll(dbr.GetColumn("ImpactArea"), New Converter(Of Object, String)(Function(x) Convert.ToString(x)))
            For i = 0 To _plans.Count - 1
                For j = 0 To impactareas.Count - 1
                    If System.IO.File.Exists(GetCurrentDirectory & "\" & _plans(i).Header & "\" & impactareas(j) & ".xml") Then

                        Dim newdestination As String = newname & "\" & _plans(i).Header & "\" & impactareas(j) & ".xml"
                        System.IO.File.Copy(GetCurrentDirectory & "\" & _plans(i).Header & "\" & impactareas(j) & ".xml", newdestination)
                    End If
                Next
            Next

            'if the old folder had a .fdai file then tell the user to recompute
            If System.IO.File.Exists(GetCurrentDirectory & "\" & Header & ".fdai") Then
                'MsgBox("Renaming requires that you re-compute.")
                _RequiresRecompute = True
                _LastComputedDate = Nothing
            End If

            'delete old directory (use recursive delete to ensure all files get deleted)
            System.IO.Directory.Delete(GetCurrentDirectory, True)

            MyBase.Header = rename.NewName
            statusreport(Nothing, Nothing)

            'Save
            CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()

        End If

    End Sub

End Class
