Public Class Gridded_WSP
    Private _reaches As List(Of Reach)
    Private _reach As Reach
    Private _Grids As List(Of LifeSimGIS.RasterFeatures)
    Private _AreDepthGrids As Boolean = False
    Private _TGrid As LifeSimGIS.RasterFeatures
    Private _ReachShapeFile As String
    Sub New(ByVal terrainpath As String, ByVal gridpaths As List(Of String), ByVal reachshapepath As String, ByVal DepthGrids As Boolean)
        _TGrid = New LifeSimGIS.RasterFeatures(terrainpath)
        _Grids = New List(Of LifeSimGIS.RasterFeatures)
        Dim tmpgrd As LifeSimGIS.RasterFeatures
        Dim tmpgrdprj As GDALAssist.Projection
        Dim tgridprj As GDALAssist.Projection
        Using gdr As GDALAssist.GDALRaster = New GDALAssist.GDALRaster(terrainpath)
            tgridprj = gdr.GetProjection()
        End Using
        If tgridprj.IsValid <> GDALAssist.SRSValidation.Corrupt Then
            For i = 0 To gridpaths.Count - 1
                If gridpaths(i) = "" Then
                    _Grids.Add(Nothing)
                Else
                    tmpgrd = New LifeSimGIS.RasterFeatures(gridpaths(i))
                    Using gdr As GDALAssist.GDALRaster = New GDALAssist.GDALRaster(gridpaths(i))
                        tmpgrdprj = gdr.GetProjection
                    End Using
                    If tgridprj.IsEqual(tmpgrdprj) Then
                        _Grids.Add(tmpgrd)
                    Else
                        tmpgrd.Reproject(tmpgrdprj, tgridprj)
                        _Grids.Add(tmpgrd)
                    End If
                End If
            Next
        Else
            ''abort? 
            Throw New Exception("Terrain grid projection is corrupt")
        End If

        _AreDepthGrids = DepthGrids
        _ReachShapeFile = reachshapepath
    End Sub
    Sub New(ByVal gridpaths As List(Of String), ByVal depthgrids As Boolean, ByVal tgridprj As GDALAssist.Projection)
        _Grids = New List(Of LifeSimGIS.RasterFeatures)
        Dim tmpgrd As LifeSimGIS.RasterFeatures
        Dim tmpgrdprj As GDALAssist.Projection
        If tgridprj.IsValid <> GDALAssist.SRSValidation.Corrupt Then
            For i = 0 To gridpaths.Count - 1
                If gridpaths(i) = "" Then
                    _Grids.Add(Nothing)
                Else
                    tmpgrd = New LifeSimGIS.RasterFeatures(gridpaths(i))
                    Using gdr As GDALAssist.GDALRaster = New GDALAssist.GDALRaster(gridpaths(i))
                        tmpgrdprj = gdr.GetProjection
                    End Using
                    If tgridprj.IsEqual(tmpgrdprj) Then
                        _Grids.Add(tmpgrd)
                    Else
                        tmpgrd.Reproject(tmpgrdprj, tgridprj)
                        _Grids.Add(tmpgrd)
                    End If
                End If
            Next
        Else
            ''abort? 
            Throw New Exception("Terrain grid projection is corrupt")
        End If
        _reaches = New List(Of Reach)
        _AreDepthGrids = depthgrids
    End Sub
    Public ReadOnly Property GetReaches As List(Of Reach)
        Get
            Return _reaches
        End Get
    End Property
	Public Sub CombineReachesAndStructures(ByRef Structures As StructureInventory, ByVal indexlocations As List(Of IndexLocation), ByVal outputpath As String)
		_reaches = New List(Of Reach)
		'_reach = New Reach()
		Dim rshapereader As New LifeSimGIS.ShapefileReader(_ReachShapeFile)
		Dim rdbfreader As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(_ReachShapeFile, ".dbf"))
		Dim rshape As LifeSimGIS.PolygonFeatures = rshapereader.ToFeatures

		Dim points As New List(Of LifeSimGIS.PointD)
		For i = 0 To Structures.GetStructureList.Count - 1
			points.Add(Structures.GetStructureList(i).GetLocation)
		Next
		Dim pointshp As New LifeSimGIS.PointFeatures(points.ToArray)
		Dim rindexes() As Integer = rshape.OptimizedPointInPolygonsGDI(pointshp, 4000, 4000, 5)
		Dim eles As Single() = _TGrid.GridReader.SampleValues(points.ToArray)
		Dim structurelists As New Dictionary(Of Integer, List(Of FDA_Structure))
		For i = 0 To rindexes.Count - 1
			If rindexes(i) = -1 Then
			Else
				Structures.GetStructureList(i).SetSidReach = indexlocations(rindexes(i)).GetReachName 'rdbfreader.GetCell(_uniquenamefield, rindexes(i)) 'the reach identifier for each structure
				Structures.GetStructureList(i).Stream = indexlocations(rindexes(i)).GetRiverName 'rdbfreader.GetCell(_uniquenamefield, rindexes(i)) & "_River"
				Structures.GetStructureList(i).SetGroundElevation = eles(i)
				If structurelists.Keys.Contains(rindexes(i)) Then
					structurelists.Item(rindexes(i)).Add(Structures.GetStructureList(i))
				Else
					Dim l As New List(Of FDA_Structure)
					l.Add(Structures.GetStructureList(i))
					structurelists.Add(rindexes(i), l)
				End If
			End If

		Next
		'clear out structures...
		Structures.Clear()
		For j = 0 To rshape.Polygons.Count - 1
			'write out a separate wsp for each damage reach
			If structurelists.Keys(j) = -1 Or Not structurelists.Keys.Contains(j) Then
			Else
				'add the structures back in (all sorted differently and including terrain and station)
				Structures.AddRange(createReachFromPoints(indexlocations(j), structurelists.Item(j), outputpath))
			End If
		Next

	End Sub
	Public Function createReachFromPoints(ByVal indexlocation As IndexLocation, ByVal structures As List(Of FDA_Structure), ByVal outputpath As String) As List(Of FDA_Structure)
		Dim points As New List(Of LifeSimGIS.PointD)
		Dim nodata(structures.Count - 1) As Single
		For i = 0 To structures.Count - 1
			points.Add(structures(i).GetLocation)
			nodata(i) = -99
		Next
		Dim reportstring As New System.Text.StringBuilder
		reportstring.AppendLine("Error Report")
		Dim ReportForSingleStation As New System.Text.StringBuilder
		Dim totcells As Integer = structures.Count
		Dim counter As Integer = 0
		Dim counter2 As Double = 0
		Dim freqdepths As New List(Of Single())

		For Each grid As LifeSimGIS.RasterFeatures In _Grids
			If IsNothing(grid) Then
				freqdepths.Add(nodata)
			Else
				freqdepths.Add(grid.GridReader.SampleValues(points.ToArray))
			End If

		Next
		Dim tmpdepths As New List(Of Single)
		Dim tmpdepth1 As Single = -99
		Dim tmpdepth2 As Single = -99
		Dim r As New List(Of RiverStation)
		r.Add(indexlocation.GetStationing)
		Dim station As Double = 0
		Dim invert As Double = 0
		For i = 0 To structures.Count - 1
			tmpdepths.Clear()
			tmpdepth1 = -99
			station = structures(i).GetStationing 'indexlocation.GetStationing.Station + structures(i).GetStationing
			invert = structures(i).GetGroundElevation
			If _AreDepthGrids Then
				For j = 0 To _Grids.Count - 1
					ReportForSingleStation = New System.Text.StringBuilder
					tmpdepth2 = freqdepths(j)(i) + invert 'if oustise of extent it will return nodata(0) value, add invert for wse evaluation
					If tmpdepth2 = -99 Then
					Else
                        If tmpdepth2 = (_Grids(j).GridReader.NoData(0) + invert) Then
                            tmpdepth2 = -99
                        End If
                    End If
					If tmpdepth1 = -99 AndAlso tmpdepth2 <> -99 AndAlso j <> 0 Then
						tmpdepths(j - 1) = -2 'based on conversation with nick
					End If
					If tmpdepth1 <= tmpdepth2 Then
						tmpdepths.Add(tmpdepth2) 'assumes depth grids.
					Else
						tmpdepths.Add(tmpdepth1 + 0.01)
						tmpdepth2 = tmpdepth1 + 0.01
						ReportForSingleStation.AppendLine("WSE is not monotonically increasing at " & station & " for grid " & System.IO.Path.GetFileNameWithoutExtension(_Grids(j).GetSource))
					End If
					tmpdepth1 = tmpdepth2
				Next
			Else
				For j = 0 To _Grids.Count - 1
					ReportForSingleStation = New System.Text.StringBuilder
					tmpdepth2 = freqdepths(j)(i) 'if oustise of extent it will return nodata(0) value
					If tmpdepth2 = -99 Then
					Else
						If tmpdepth2 = _Grids(j).GridReader.NoData(0) Then tmpdepth2 = -99
					End If
					If tmpdepth1 = -99 AndAlso tmpdepth2 <> -99 AndAlso j <> 0 Then
						tmpdepths(j - 1) = invert - 2 'based on conversation with nick
					End If
					If tmpdepth1 <= tmpdepth2 - 0.01 Then 'FDA precision is .01
						tmpdepths.Add(tmpdepth2) 'assumes wse.
					Else
						tmpdepths.Add(tmpdepth1 + 0.01) 'FDA may allow for flat water surface elevations... consider not adding .01
						tmpdepth2 = tmpdepth1 + 0.01
						ReportForSingleStation.AppendLine("WSE is not monotonically increasing at " & station & " for grid " & System.IO.Path.GetFileNameWithoutExtension(_Grids(j).GetSource))
					End If
					tmpdepth1 = tmpdepth2
				Next
			End If

			If tmpdepths.Last = -99 Then
				'dont add it?
				'and flag the structure for removal
				reportstring.AppendLine("Structure at " & station & " never gets wet")
				r.Add(New RiverStation(station, invert, indexlocation.GetStationing.Probabilities, indexlocation.GetStationing.Flows, tmpdepths.ToArray))
			Else
				reportstring.AppendLine(ReportForSingleStation.ToString)
				r.Add(New RiverStation(station, invert, indexlocation.GetStationing.Probabilities, indexlocation.GetStationing.Flows, tmpdepths.ToArray))
			End If
		Next
		Dim fs As New System.IO.FileStream(outputpath & "\" & indexlocation.GetReachName & "_errors.txt", IO.FileMode.Create, IO.FileAccess.Write)
		Dim sw As New System.IO.StreamWriter(fs)
		'MsgBox(reportstring, MsgBoxStyle.OkOnly, "Errors Occured")
		sw.Write(reportstring.ToString)
		sw.Close() : sw.Dispose()
		fs.Close() : fs.Dispose()
		_reaches.Add(New Reach(indexlocation, "Created By GridsToWSE", FDA_Computation.BankEnum.Both, r))
		Return structures
	End Function
	Public Sub ADDWSPINFO(ByVal indexlocation As IndexLocation, ByVal structures As List(Of ComputableObjects.FDAStructure), ByVal outputpath As String)
		Dim points As New List(Of LifeSimGIS.PointD)
		Dim nodata(structures.Count - 1) As Single
		For i = 0 To structures.Count - 1
			points.Add(structures(i).Location)
			nodata(i) = -99
		Next
		Dim reportstring As New System.Text.StringBuilder
		reportstring.AppendLine("Error Report")
		Dim ReportForSingleStation As New System.Text.StringBuilder
		Dim freqdepths As New List(Of Single())
		'_Grids.Reverse()
		For Each grid As LifeSimGIS.RasterFeatures In _Grids
			If IsNothing(grid) Then
				freqdepths.Add(nodata)
			Else
				freqdepths.Add(grid.GridReader.SampleValues(points.ToArray))
			End If

		Next
		Dim tmpdepths As New List(Of Single)
		Dim previousdepth As Single = -99
		Dim currentDepth As Single = -99
		Dim r As New List(Of RiverStation)
		r.Add(indexlocation.GetStationing)
		Dim station As Double = 0
		Dim invert As Double = 0
		For i = 0 To structures.Count - 1
			tmpdepths.Clear()
			previousdepth = -99
			station = structures(i).Stationing
			invert = structures(i).GroundEle
			If _AreDepthGrids Then
				For j = 0 To _Grids.Count - 1
					ReportForSingleStation = New System.Text.StringBuilder
					currentDepth = freqdepths(j)(i) + invert 'if oustise of extent it will return nodata(0) value, add invert for wse evaluation
					If currentDepth = -99 Then
					Else
						If currentDepth = _Grids(j).GridReader.NoData(0) + invert Then currentDepth = -99
					End If
					If j <> 0 AndAlso tmpdepths(j - 1) < -98 AndAlso currentDepth <> tmpdepths(j - 1) + 0.01 Then
						'update the previous value to be 2 ft below invert
						tmpdepths(j - 1) = invert - 2 'based on conversation with nick
					End If
					If previousdepth <= currentDepth - 0.01 Then 'FDA precision is .01
						tmpdepths.Add(currentDepth) 'assumes wse.
					Else
						tmpdepths.Add(previousdepth + 0.01) 'FDA may allow for flat water surface elevations... consider not adding .01
						currentDepth = previousdepth + 0.01
						ReportForSingleStation.AppendLine("WSE is not monotonically increasing at " & station & " for grid " & System.IO.Path.GetFileNameWithoutExtension(_Grids(j).GetSource))
					End If
					previousdepth = currentDepth
				Next
			Else
				For j = 0 To _Grids.Count - 1
					ReportForSingleStation = New System.Text.StringBuilder
					currentDepth = freqdepths(j)(i) 'if oustise of extent it will return nodata(0) value
					If currentDepth = -99 Then
					Else
						If currentDepth = _Grids(j).GridReader.NoData(0) Then currentDepth = -99
					End If
					If j <> 0 AndAlso tmpdepths(j - 1) < -98 AndAlso currentDepth > tmpdepths(j - 1) + 0.01 Then
						'update the previous value to be 2 ft below invert
						tmpdepths(j - 1) = invert - 2 'based on conversation with nick
					End If
					If previousdepth <= currentDepth - 0.01 Then 'FDA precision is .01
						tmpdepths.Add(currentDepth) 'assumes wse.
					Else
						tmpdepths.Add(previousdepth + 0.01) 'FDA may allow for flat water surface elevations... consider not adding .01
						currentDepth = previousdepth + 0.01
						ReportForSingleStation.AppendLine("WSE is not monotonically increasing at " & station & " for grid " & System.IO.Path.GetFileNameWithoutExtension(_Grids(j).GetSource))
					End If
					previousdepth = currentDepth
				Next
			End If

			If tmpdepths.Last < -98 Then
				'dont add it?
				'and flag the structure for removal
				'reportstring.AppendLine("Structure at " & station & " never gets wet")
				r.Add(New RiverStation(station, invert, indexlocation.GetStationing.Probabilities, indexlocation.GetStationing.Flows, tmpdepths.ToArray))
			Else
				If ReportForSingleStation.ToString = "" Then
				Else
					reportstring.AppendLine(ReportForSingleStation.ToString)
				End If
				r.Add(New RiverStation(station, invert, indexlocation.GetStationing.Probabilities, indexlocation.GetStationing.Flows, tmpdepths.ToArray))
			End If
		Next
		If reportstring.ToString = "Error Report" & vbNewLine Then
		Else
			Dim fs As New System.IO.FileStream(outputpath & "\" & indexlocation.GetReachName & "_errors.txt", IO.FileMode.Create, IO.FileAccess.Write)
			Dim sw As New System.IO.StreamWriter(fs)
			sw.Write(reportstring.ToString)
			sw.Close() : sw.Dispose()
			fs.Close() : fs.Dispose()
		End If

		_reaches.Add(New Reach(indexlocation, "Created By GeoFDA", FDA_Computation.BankEnum.Both, r))
	End Sub
	Public Function WriteToEconImportFormat(ByVal plan As String, ByVal Year As String) As String
        Dim s As New System.Text.StringBuilder
        Dim r As New Reach(_reaches(0).GetRiverName, plan, _reaches(0).GetStations)
        For i = 1 To _reaches.Count - 1
            r.AddStations(_reaches(i).GetStations)
        Next
        s.AppendLine(r.WriteToEconWSP(r.GetReachName, plan, Year))
        'For i = 0 To _reaches.Count - 1
        '    s.AppendLine(_reaches(i).WriteToEconWSP(_reaches(i).GetReachName & "_wsp", plan, Year))
        'Next
        Return s.ToString
    End Function

    Public Overrides Function ToString() As String
        Dim s As New System.Text.StringBuilder
        For i = 0 To _reaches.Count - 1
            s.AppendLine(_reaches(i).ToString)
        Next
        Return s.ToString
    End Function
End Class
