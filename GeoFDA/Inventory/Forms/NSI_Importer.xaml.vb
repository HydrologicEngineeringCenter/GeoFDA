Public Class NSI_Importer
    Private _ouputDir As String
    Private _monetaryUnit As String
    Public Event ProgressComplete(ByVal itworked As Boolean, ByVal uniqueName As String)
    Public Event CheckName(ByVal uniquename As String, ByRef conflict As Boolean)
    Sub New()
        InitializeComponent()
    End Sub
    Sub New(ByVal pointshapes As List(Of String), ByVal polyshapes As List(Of String), ByVal outputdirectory As String, ByVal monetaryunits As String)
        InitializeComponent()
        CmbStudyArea.FilePaths = polyshapes
        CmbUserDefinedNSIShape.FilePaths = pointshapes
        Dim validSAshapetypes As New List(Of LifeSimGIS.ShapefileReader.ShapeTypeEnumerable)
        Dim validSIshapetypes As New List(Of LifeSimGIS.ShapefileReader.ShapeTypeEnumerable)
        validSIshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.Point)
        validSIshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PointM)
        validSIshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PointZM)
        validSAshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.Polygon)
        validSAshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PolygonM)
        validSAshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PolygonZM)
        CmbStudyArea.ValidShapeTypes = validSAshapetypes
        CmbUserDefinedNSIShape.ValidShapeTypes = validSIshapetypes
        _ouputDir = outputdirectory
        Try
            Dim dummy As Net.IPHostEntry = Net.Dns.GetHostEntry("www.google.com")
        Catch ex As Net.Sockets.SocketException
            RdbFromWeb.IsChecked = False
            RdbFromShp.IsChecked = True
            RdbFromWeb.IsEnabled = False
        End Try

    End Sub
    Public ReadOnly Property GetFilePath As String
        Get
            Return _ouputDir & GetUniqueName & ".shp"
        End Get
    End Property
    Public ReadOnly Property GetUniqueName As String
        Get
            Return TxtName.Text
        End Get
    End Property
    Private Sub ClipPointsToShp(ByVal points As LifeSimGIS.PointFeatures, ByVal pointsdbf As DataBase_Reader.DBFReader, ByVal Polys As LifeSimGIS.PolygonFeatures)
        'clip the point shapefile.
        Dim indexes() As Integer = Polys.OptimizedPointInPolygonsGDI(points)
        Dim newpoints As New List(Of LifeSimGIS.PointD)
        Dim dt As New System.Data.DataTable
        For i = 0 To pointsdbf.ColumnNames.Count - 1 'need to follow the same naming convention as the shapefile importer
            dt.Columns.Add(New System.Data.DataColumn(pointsdbf.ColumnNames(i), pointsdbf.ColumnTypes(i)))
        Next
        If Not pointsdbf.DataBaseOpen Then pointsdbf.Open()
        ConvertMonetaryUnits(pointsdbf)
        If Not pointsdbf.DataBaseOpen Then pointsdbf.Open()
        For i = 0 To indexes.Count - 1 'this does not take into account the dbf and the order of the rows.
            If indexes(i) <> -1 Then
                newpoints.Add(points.Points(i))
                dt.Rows.Add(pointsdbf.GetRow(i))
            End If
        Next
        pointsdbf.Close()
        'create a new point feature and save it somewhere?
        'delete it and any auxilary files.
        Try
            If System.IO.File.Exists(GetFilePath) Then Kill(GetFilePath)
            If System.IO.File.Exists(System.IO.Path.ChangeExtension(GetFilePath, ".dbf")) Then Kill(System.IO.Path.ChangeExtension(GetFilePath, ".dbf"))
            If System.IO.File.Exists(System.IO.Path.ChangeExtension(GetFilePath, ".shx")) Then Kill(System.IO.Path.ChangeExtension(GetFilePath, ".shx"))
            If System.IO.File.Exists(System.IO.Path.ChangeExtension(GetFilePath, ".prj")) Then Kill(System.IO.Path.ChangeExtension(GetFilePath, ".prj"))
        Catch ex As Exception
            MsgBox("Could not delete one of the main existing files, make sure the file is not in use by other programs") : Exit Sub
        End Try

        Dim output As New LifeSimGIS.ShapefileWriter(GetFilePath) 'probably check if the output path already exists.
        Dim prjfile As String = System.IO.Path.ChangeExtension(points.GetSource, ".prj") 'probably check if file exists
        If System.IO.File.Exists(prjfile) Then
            Dim prj As GDALAssist.Projection = New GDALAssist.ESRIProjection(prjfile)
            output.WriteFeatures(New LifeSimGIS.PointFeatures(newpoints.ToArray), dt, prj)
        Else
            'projection not defined.
            output.WriteFeatures(New LifeSimGIS.PointFeatures(newpoints.ToArray), dt)
        End If
        'output.WriteFeatures(New LifeSimGIS.PointFeatures(newpoints.ToArray), dt)
    End Sub
    Private Sub ConvertMonetaryUnits(dbf As DataBase_Reader.DBFReader)
        If _monetaryUnit <> "$'s" Then 'nsi is always in dollars.
            Dim sval As Double() = Array.ConvertAll(dbf.GetColumn("Val_Struct"), AddressOf Double.Parse)
            Dim cval As Double() = Array.ConvertAll(dbf.GetColumn("Val_Cont"), AddressOf Double.Parse)
            Dim oval As Double() = Array.ConvertAll(dbf.GetColumn("Val_Other"), AddressOf Double.Parse)
            If _monetaryUnit = "1,000$'s" Then
                For i = 0 To sval.Count - 1
                    sval(i) /= 1000
                    cval(i) /= 1000
                    oval(i) /= 1000
                Next
            Else 'mustbe millions
                For i = 0 To sval.Count - 1
                    sval(i) /= 1000000
                    cval(i) /= 1000000
                    oval(i) /= 1000000
                Next
            End If
            dbf.EditColumn("Val_Struct", sval)
            dbf.EditColumn("Val_Cont", cval)
            dbf.EditColumn("Val_Other", oval)
        End If
    End Sub
    Private Sub ConvertColumnNamesinUserDefinedShapeFile(dbf As DataBase_Reader.DBFReader)
        Dim sval As Double() = Array.ConvertAll(dbf.GetColumn("val_struct"), AddressOf Double.Parse)
        Dim cval As Double() = Array.ConvertAll(dbf.GetColumn("val_cont"), AddressOf Double.Parse)
        Dim oval As Double() = Array.ConvertAll(dbf.GetColumn("val_other"), AddressOf Double.Parse)
        Dim fheights(sval.Count) As Double
        Dim names As New List(Of String)
        Dim damcats As New List(Of String)
        Dim occtypes As New List(Of String)
        Dim tmpnames As Object() = dbf.GetColumn("fd_name")
        Dim tmpdamcats As Object() = dbf.GetColumn("st_damcat")
        Dim tmpocctypes As Object() = dbf.GetColumn("st_damfun")
        For i = 0 To sval.Count - 1
            names.Add(tmpnames(i))
            damcats.Add(tmpdamcats(i))
            occtypes.Add(tmpocctypes(i))
        Next
        dbf.DeleteColumn("val_struct")
        dbf.DeleteColumn("val_cont")
        dbf.DeleteColumn("val_other")
        dbf.DeleteColumn("fd_name")
        dbf.DeleteColumn("st_damcat")
        dbf.DeleteColumn("st_damfun")
        dbf.AddColumn("Val_Struct", sval)
        dbf.AddColumn("Val_Cont", cval)
        dbf.AddColumn("Val_Other", oval)
        dbf.AddColumn("DamCat", damcats.ToArray)
        dbf.AddColumn("St_Name", names.ToArray)
        dbf.AddColumn("OccType", occtypes.ToArray)
        dbf.AddColumn("Found_Ht", fheights)
    End Sub
    Sub processcomplete(ByVal ItWorked As Boolean, ByVal Result As Consequences_Assist.API.NSI.NSI_Structures)
        If Not ItWorked Then MsgBox("The NSI download process did not work, please try again at a later time") : RaiseEvent ProgressComplete(False, "") : Me.Close() : Exit Sub
        'process the NSI structures result.
        'develop the point shapefile?
        'develop the datatable?
        Dim prj As GDALAssist.Projection = New GDALAssist.ESRIProjection(System.IO.Path.ChangeExtension(CmbStudyArea.GetSelectedItemPath, ".prj"))
        Result.CreateFDAStructureInventoryShapefile(GetFilePath, prj)
        'clip
        Dim pointreader As New LifeSimGIS.ShapefileReader(GetFilePath)
        Dim points As LifeSimGIS.PointFeatures = pointreader.ToFeatures
        Dim polyshp As New LifeSimGIS.ShapefileReader(CmbStudyArea.GetSelectedItemPath)
        Dim polys As LifeSimGIS.PolygonFeatures = polyshp.ToFeatures
        Dim pointsdbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(GetFilePath, ".dbf"))
        ClipPointsToShp(points, pointsdbf, polys)
        RaiseEvent ProgressComplete(True, TxtName.Text)
        Me.Close()
    End Sub
    Private Sub CMDOk_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles CMDOk.Click
        CMDClose.IsEnabled = False
        'do the NSI fetching
        If CmbStudyArea.GetSelectedItemPath = "" Then MsgBox("You have not selected a study area shapefile") : Exit Sub
        If TxtName.Text = "" Then MsgBox("You have not defined a unique name for the structure inventory") : Exit Sub
        For Each badChar As Char In System.IO.Path.GetInvalidFileNameChars
            If TxtName.Text.Contains(badChar) Then MsgBox("Invalid character in file name.") : Exit Sub
        Next
        Dim cancel As Boolean = False
        RaiseEvent CheckName(TxtName.Text, cancel)
        If cancel Then
            'TxtName.Text = TxtName.Text & "_1"
            MsgBox("Name Conflict, please choose a different name.")
            Exit Sub
        End If

        If System.IO.File.Exists(GetFilePath) Then MsgBox("A file already exists with the name you have selected in the study directory, but you didnt have any name conflicts with existing structure inventories.  You should clean that directory, or choose a new name.") : Exit Sub

        Dim polyshp As New LifeSimGIS.ShapefileReader(CmbStudyArea.GetSelectedItemPath)
        Dim polyprjpath As String = System.IO.Path.ChangeExtension(CmbStudyArea.GetSelectedItemPath, ".prj")
        If System.IO.File.Exists(polyprjpath) Then
            'good
        Else
            MsgBox("Your study area shapefile did not have a defined projection") : Exit Sub
        End If
        Dim polys As LifeSimGIS.PolygonFeatures = polyshp.ToFeatures
        If RdbFromShp.IsChecked Then
            If CmbUserDefinedNSIShape.GetSelectedItemPath = "" Then MsgBox("You have selected to use a User Defined Point Shapefile, but you did not defined the shapefile.") : Exit Sub
            'clip to the study area
            Dim pointshp As New LifeSimGIS.ShapefileReader(CmbUserDefinedNSIShape.GetSelectedItemPath)
            Dim points As LifeSimGIS.PointFeatures = pointshp.ToFeatures
            Dim pointsdbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(CmbUserDefinedNSIShape.GetSelectedItemPath, ".dbf"))
            If Not pointsdbf.ColumnNames.Contains("val_struct") Then MsgBox("Your user supplied point shapefile Dbf does not meet the output specifications of the NSI, there must be a column named val_struct") : Exit Sub
            If Not pointsdbf.ColumnNames.Contains("val_cont") Then MsgBox("Your user supplied point shapefile Dbf does not meet the output specifications of the NSI, there must be a column named val_cont") : Exit Sub
            If Not pointsdbf.ColumnNames.Contains("val_other") Then MsgBox("Your user supplied point shapefile Dbf does not meet the output specifications of the NSI, there must be a column named val_other") : Exit Sub
            If Not pointsdbf.ColumnNames.Contains("fd_name") Then MsgBox("Your user supplied point shapefile Dbf does not meet the output specifications of the NSI, there must be a column named fd_name") : Exit Sub
            If Not pointsdbf.ColumnNames.Contains("st_damcat") Then MsgBox("Your user supplied point shapefile Dbf does not meet the output specifications of the NSI, there must be a column named st_damcat") : Exit Sub
            If Not pointsdbf.ColumnNames.Contains("st_damfun") Then MsgBox("Your user supplied point shapefile Dbf does not meet the output specifications of the NSI, there must be a column named st_damfun") : Exit Sub
            ConvertColumnNamesinUserDefinedShapeFile(pointsdbf)
            ClipPointsToShp(points, pointsdbf, polys)
            'DialogResult = True
            Me.Close()
            Else
                'polys.Reproject(New , New GDALAssist.EPSGProjection(4326))
                Dim nsi As New Consequences_Assist.API.NSI.NSI_Interface(New GDALAssist.ESRIProjection(polyprjpath))
            AddHandler nsi.NSIStructuresDownloadComplete, AddressOf processcomplete
            Try
                nsi.GetNSIStructures(polyshp.Extent.BaseExtent)
            Catch ex As Exception
                MsgBox("Server Error, please try again: " & vbNewLine & ex.InnerException.ToString)
            End Try

        End If

    End Sub
    Private Sub CMDClose_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles CMDClose.Click
        'DialogResult = False
        Me.Close()
    End Sub
End Class
