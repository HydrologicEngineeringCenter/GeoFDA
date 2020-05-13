Namespace ComputableObjects
    Public Class Consequences_Inventory
        Private _Structures As List(Of Consequences_Structure)
        Public Event ReportMessage(ByVal message As String)
        Sub New()
            _Structures = New List(Of Consequences_Structure)
        End Sub
        Sub New(ByVal structurelist As List(Of Consequences_Structure))
            Structures = structurelist
        End Sub
        Sub New(ByVal structurelist As List(Of FDAStructure))
            _Structures = New List(Of Consequences_Structure)
            For i = 0 To structurelist.Count - 1
                _Structures.Add(structurelist(i))
            Next

        End Sub
		Sub LoadFromFDATxtFile(ByVal textFilePath As String, ByVal occupancytypes As ComputableObjects.OccupancyTypes, ByVal inputmonetaryunit As String, ByVal outputmonetaryunit As String)
			_Structures = New List(Of Consequences_Structure)
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
			Dim tmpocctype As ComputableObjects.OccupancyType
			If tabbedline.Contains("East") AndAlso tabbedline.Contains("North") Then
				'geospatial structure inventory identified...
				If Not sr.EndOfStream Then tabbedline = Split(sr.ReadLine, vbTab)
				Do Until tabbedline(0) = "WSP_Name" Or sr.EndOfStream
					'get the appropriate damage category and occtype.
					tmpdamcatname = tabbedline(Array.IndexOf(headers, "Cat_Name"))
					tmpoccname = tabbedline(Array.IndexOf(headers, "Occ_Name"))
					Try
						tmpocctype = occupancytypes.GetOcctypeByNameAndDamCat(tmpoccname, tmpdamcatname)
						If IsNothing(tmpocctype) Then RaiseEvent ReportMessage("The occtype: " & tmpoccname & " and damage category: " & tmpdamcatname & " combination does not exist.")
						_Structures.Add(New FDAStructure(tabbedline, headers, tmpocctype, inputmonetaryunit, outputmonetaryunit))
					Catch ex As Exception
						RaiseEvent ReportMessage(tabbedline(Array.IndexOf(headers, "Struc_Name")) & " was not imported: " & ex.Message)
					End Try

					'update tabbedline
					If Not sr.EndOfStream Then tabbedline = Split(sr.ReadLine, vbTab)
				Loop
				If _Structures.Count = 0 Then RaiseEvent ReportMessage("There were no structures defined in " & textFilePath)
			Else
				'not geospatial, give up.
				RaiseEvent ReportMessage("The structure inventory in the file:" & vbNewLine & textFilePath & vbNewLine & "did not contain geospatial information, structure inventory not imported.")
			End If
		End Sub
		Public Sub LoadStructuresFromFile(ByVal structureshppath As String, ByVal occtypepath As String, ByVal damcatsfile As String)
            'read the occtype file
            Dim otypes As New OccupancyTypes(occtypepath)
            'read the damcats file
            Dim dcats As New DamageCategories(damcatsfile)
            'read the structures...
            Dim dbfpath As String = System.IO.Path.ChangeExtension(structureshppath, ".dbf")
            Dim dbf As New DataBase_Reader.DBFReader(dbfpath)
            Dim shpreader As New LifeSimGIS.ShapefileReader(structureshppath)
            Dim points As LifeSimGIS.PointFeatures = shpreader.ToFeatures
            Dim blankot As New OccupancyType
            Dim blankdc As New DamageCategory
            Dim tmppoint As LifeSimGIS.PointD
            Dim dc As DamageCategory
            Dim oc As OccupancyType
            For i = 0 To dbf.NumberOfRows - 1
                'get the point location
                tmppoint = points.Points(i)
                'get the right damcat
                dc = dcats.GetDamageCategoryByName(dbf.GetCell("DamCat", i))
                If IsNothing(dc) Then
                    'no damagecategory of that name exists
                    RaiseEvent ReportMessage("No damage category of the name " & dbf.GetCell("DamCat", i) & " exists")
                    _Structures.Add(New FDAStructure(dbf.GetCell("St_Name", i), blankot, dbf.GetCell("Val_Struct", i), dbf.GetCell("Found_Ht", i), tmppoint))
                Else
                    'keep on truckin.
                    oc = otypes.GetOcctypeByNameAndDamCat(dbf.GetCell("OccType", i), dc.Name)
                    If IsNothing(oc) Then
                        'no occtype with that damcat exists...
                        RaiseEvent ReportMessage("No occupancy type of the name " & dbf.GetCell("OccType", i) & " exists with the damcat of name " & dbf.GetCell("DamCat", i))
                        _Structures.Add(New FDAStructure(dbf.GetCell("St_Name", i), blankot, dbf.GetCell("Val_Struct", i), dbf.GetCell("Found_Ht", i), tmppoint))
                    Else
                        'keep on truckin'get the rest of the data
                        _Structures.Add(New FDAStructure(dbf.GetCell("St_Name", i), oc, dbf.GetCell("Val_Struct", i), dbf.GetCell("Found_Ht", i), tmppoint))
                    End If
                End If




            Next
        End Sub
        Public Property Structures As List(Of Consequences_Structure)
            Get
                Return _Structures
            End Get
            Set(value As List(Of Consequences_Structure))
                _Structures = value
            End Set
        End Property
        Public Sub WriteToFDAShapefile(ByVal outputPath As String, ByVal projection As GDALAssist.Projection)
            'create a structure inventory shapefile on disk using the correct naming convention.
            If IsNothing(_Structures) Then RaiseEvent ReportMessage("There are no structures in this structure inventory") : Exit Sub
            If _Structures.Count = 0 Then RaiseEvent ReportMessage("There are no structures in this structure invnetory") : Exit Sub
            'get all the structures into a pointfile and a datatable.
            Dim points As New List(Of LifeSimGIS.PointD)
            Dim dt As New System.Data.DataTable
            dt.Columns.Add(New System.Data.DataColumn("St_Name", GetType(String)))
            dt.Columns.Add(New System.Data.DataColumn("DamCat", GetType(String)))
            dt.Columns.Add(New System.Data.DataColumn("OccType", GetType(String)))
            dt.Columns.Add(New System.Data.DataColumn("Found_Ht", GetType(Double)))
            dt.Columns.Add(New System.Data.DataColumn("Ground_Ht", GetType(Double)))
            dt.Columns.Add(New System.Data.DataColumn("FFE", GetType(Double)))
            dt.Columns.Add(New System.Data.DataColumn("Val_Struct", GetType(Double)))
            dt.Columns.Add(New System.Data.DataColumn("Val_Cont", GetType(Double)))
            dt.Columns.Add(New System.Data.DataColumn("Val_Other", GetType(Double)))
            dt.Columns.Add(New System.Data.DataColumn("Yr_Built", GetType(Integer)))
            dt.Columns.Add(New System.Data.DataColumn("Begin_Dmg", GetType(Double)))
            dt.Columns.Add(New System.Data.DataColumn("Num_Struct", GetType(Integer)))
            dt.Columns.Add(New System.Data.DataColumn("UseFFE", GetType(Boolean)))
            dt.Columns.Add(New System.Data.DataColumn("UseDBF_GE", GetType(Boolean)))
            dt.Columns.Add(New System.Data.DataColumn("Mod_Name", GetType(String)))
            Dim fda As FDAStructure
            For i = 0 To _Structures.Count - 1
                points.Add(_Structures(i).Location)
                fda = _Structures(i)
                With fda
                    dt.Rows.Add({.Name, .Occtype.DamageCategory.Name, .Occtype.Name, .FH, .GroundEle, .FirstFloorElevation, .StructureValue, .ContentValue, .OtherValue, .Year, .BeginDamage, .NumStructs, False, False, .St_Module})
                End With

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
        Public Function WriteToFDAString() As String
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
            For i = 0 To _Structures.Count - 1
                str.AppendLine(_Structures(i).WriteToFDAString)
            Next
            Return str.ToString
        End Function
    End Class
End Namespace

