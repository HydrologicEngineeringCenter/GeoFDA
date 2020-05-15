Public Class Gridded_WSP
    Private _reaches As List(Of Reach)
    Private _reach As Reach
    Private _Grids As List(Of LifeSimGIS.RasterFeatures)
    Private _AreDepthGrids As Boolean = False
    Private _TGrid As LifeSimGIS.RasterFeatures
    Private _ReachShapeFile As String

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
					currentDepth = (freqdepths(j)(i) + invert) 'if oustise of extent it will return nodata(0) value, add invert for wse evaluation
					If currentDepth = -99 Then
					Else
						If currentDepth = (_Grids(j).GridReader.NoData(0) + invert) Then currentDepth = -99
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
