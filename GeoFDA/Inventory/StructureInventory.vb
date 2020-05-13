Public Class StructureInventory
    Private _structures As List(Of FDA_Structure)
    Public Event ReportMessage(ByVal message As String)
    Sub New()

    End Sub
    Sub New(ByVal textFilePath As String, ByVal occupancytypes As Consequences_Assist.ComputableObjects.OccupancyTypes)
        _structures = New List(Of FDA_Structure)
        'open a reader
        Dim fs As New System.IO.FileStream(textFilePath, IO.FileMode.Open, IO.FileAccess.Read)
        Dim sr As New System.IO.StreamReader(fs)
        Dim tabbedline() As String
        Dim tmpline As String = ""
        Dim startdata As Integer = 0
        Dim parameter As Integer = 0
        tabbedline = Split(sr.ReadLine, vbTab)
        Do Until tabbedline(0) = "Struc_Name" Or sr.EndOfStream

            tabbedline = Split(sr.ReadLine, vbTab)

        Loop
        'check for easting and northing.
        Dim headers As String() = tabbedline.ToArray
        Dim tmpdamcatname As String
        Dim tmpoccname As String
        Dim tmpocctype As Consequences_Assist.ComputableObjects.OccupancyType
        If tabbedline.Contains("East") AndAlso tabbedline.Contains("North") Then
            'geospatial structure inventory identified...
            Do Until tabbedline(0) = "WSP_Name" Or sr.EndOfStream
                'get the appropriate damage category and occtype.
                tmpdamcatname = tabbedline(Array.IndexOf(headers, "Cat_Name"))
                tmpoccname = tabbedline(Array.IndexOf(headers, "Occ_Name"))
                Try
                    tmpocctype = occupancytypes.GetOcctypeByNameAndDamCat(tmpoccname, tmpdamcatname)
                    If IsNothing(tmpocctype) Then Throw New Exception("The occtype: " & tmpoccname & " and damage category: " & tmpdamcatname & " combination does not exist.")
                    _structures.Add(New FDA_Structure(tabbedline, headers, tmpocctype))
                Catch ex As Exception
                    RaiseEvent ReportMessage(tabbedline(Array.IndexOf(headers, "Struc_Name")) & " was not imported: " & ex.Message)
                End Try

                'update tabbedline
                If Not sr.EndOfStream Then tabbedline = Split(sr.ReadLine, vbTab)
            Loop
            If _structures.Count = 0 Then Throw New Exception("There were no structures defined in " & textFilePath)
        Else
            'not geospatial, give up.
            Throw New Exception("The structure inventory in the file:" & vbNewLine & textFilePath & vbNewLine & "did not contain geospatial information, structure inventory not imported.")
        End If
    End Sub
    Sub LoadFromFDATxtFile(ByVal textFilePath As String, ByVal occupancytypes As Consequences_Assist.ComputableObjects.OccupancyTypes)
        _structures = New List(Of FDA_Structure)
        'open a reader
        Dim fs As New System.IO.FileStream(textFilePath, IO.FileMode.Open, IO.FileAccess.Read)
        Dim sr As New System.IO.StreamReader(fs)
        Dim tabbedline() As String
        Dim tmpline As String = ""
        Dim startdata As Integer = 0
        Dim parameter As Integer = 0
        tabbedline = Split(sr.ReadLine, vbTab)
        Do Until tabbedline(0) = "Struc_Name" Or sr.EndOfStream

            tabbedline = Split(sr.ReadLine, vbTab)

        Loop
        'check for easting and northing.
        Dim headers As String() = tabbedline.ToArray
        Dim tmpdamcatname As String
        Dim tmpoccname As String
        Dim tmpocctype As Consequences_Assist.ComputableObjects.OccupancyType
        If tabbedline.Contains("East") AndAlso tabbedline.Contains("North") Then
            'geospatial structure inventory identified...
            If Not sr.EndOfStream Then tabbedline = Split(sr.ReadLine, vbTab)
            Do Until tabbedline(0) = "WSP_Name" Or sr.EndOfStream
                'get the appropriate damage category and occtype.
                tmpdamcatname = tabbedline(Array.IndexOf(headers, "Cat_Name"))
                tmpoccname = tabbedline(Array.IndexOf(headers, "Occ_Name"))
                Try
                    tmpocctype = OccupancyTypes.GetOcctypeByNameAndDamCat(tmpoccname, tmpdamcatname)
                    If IsNothing(tmpocctype) Then Throw New Exception("The occtype: " & tmpoccname & " and damage category: " & tmpdamcatname & " combination does not exist.")
                    _structures.Add(New FDA_Structure(tabbedline, headers, tmpocctype))
                Catch ex As Exception
                    RaiseEvent ReportMessage(tabbedline(Array.IndexOf(headers, "Struc_Name")) & " was not imported: " & ex.Message)
                End Try

                'update tabbedline
                If Not sr.EndOfStream Then tabbedline = Split(sr.ReadLine, vbTab)
            Loop
            If _structures.Count = 0 Then Throw New Exception("There were no structures defined in " & textFilePath)
        Else
            'not geospatial, give up.
            Throw New Exception("The structure inventory in the file:" & vbNewLine & textFilePath & vbNewLine & "did not contain geospatial information, structure inventory not imported.")
        End If
    End Sub
    Sub New(ByVal structures As List(Of FDA_Structure))
        _structures = structures
    End Sub
    Public ReadOnly Property GetStructureList As List(Of FDA_Structure)
        Get
            Return _structures
        End Get
    End Property
    Public Sub Clear()
        _structures.Clear()
    End Sub
    Public Sub AddRange(ByVal Structures As List(Of FDA_Structure))
        _structures.AddRange(Structures)
    End Sub
    Public Sub WriteToShapefile(ByVal outputPath As String, ByVal projection As GDALAssist.Projection)
        'create a structure inventory shapefile on disk using the correct naming convention.
        If IsNothing(_structures) Then Throw New Exception("There are no structures in this structure inventory")
        If _structures.Count = 0 Then Throw New Exception("There are no structures in this structure invnetory")
        'get all the structures into a pointfile and a datatable.
        Dim points As New List(Of LifeSimGIS.PointD)
        Dim dt As New System.Data.DataTable
        dt.Columns.Add(New System.Data.DataColumn("St_Name", GetType(String)))
        dt.Columns.Add(New System.Data.DataColumn("DamCat", GetType(String)))
        dt.Columns.Add(New System.Data.DataColumn("OccType", GetType(String)))
        dt.Columns.Add(New System.Data.DataColumn("Stream", GetType(String)))
        For i = 0 To _structures.Count - 1
            points.Add(_structures(i).GetLocation)
            dt.Rows.Add({_structures(i).GetName, _structures(i).GetOccupancyType.DamageCategory.Name, _structures(i).GetOccupancyType.Name, _structures(i).Stream})
        Next
        Dim shpwriter As New LifeSimGIS.ShapefileWriter(outputPath)
        Dim pfeatures As New LifeSimGIS.PointFeatures(points.ToArray)
        pfeatures.CalculateExtent()
        If IsNothing(projection) Then
            'dont project it

            shpwriter.WriteFeatures(pfeatures, dt)
        Else
            'project it.
            shpwriter.WriteFeatures(pfeatures, dt, projection)
        End If

    End Sub
    Public Overrides Function ToString() As String
        Dim str As New System.Text.StringBuilder
        str.Append("Struc_Name" & vbTab)
        str.Append("Cat_Name" & vbTab)
        str.Append("Stream_Name" & vbTab)
        str.Append("Occ_Name" & vbTab)
        str.Append("Station" & vbTab)
        str.Append("Bank" & vbTab)
        str.Append("Year" & vbTab)
        str.Append("Struc_Val" & vbTab)
        str.Append("Cont_Val" & vbTab)
        str.Append("Other_Val" & vbTab)
        str.Append("1F_Stage" & vbTab)
        str.Append("Grnd_Stage" & vbTab)
        str.Append("Found_Ht" & vbTab)
        str.Append("SID_Rch" & vbTab)
        str.Append("Northing" & vbTab)
        str.Append("Easting" & vbTab)
        str.Append("Module" & vbTab)
        str.Append(vbNewLine)
        For i = 0 To _structures.Count - 1
            str.AppendLine(_structures(i).ToString)
        Next
        Return str.ToString
    End Function
End Class
