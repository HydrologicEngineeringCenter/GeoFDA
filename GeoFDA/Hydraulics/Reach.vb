Public Class Reach
    Private _indexlocation As IndexLocation
    Private _desc As String
	Private _Bank As ComputableObjects.BankEnum
	Private _Stations As List(Of RiverStation)
	Sub New(ByVal indexlocation As IndexLocation, ByVal desc As String, ByVal bank As ComputableObjects.BankEnum, ByVal stations As List(Of RiverStation))
		_indexlocation = indexlocation
		_desc = desc
		_Bank = bank
		_Stations = stations
		SortStations()
	End Sub
	Sub New(ByVal river As String, ByVal reach As String, ByVal stations As List(Of RiverStation))
        _indexlocation = New IndexLocation(river, reach, stations(0))
        _desc = "added from RAS WSP Import (through GeoFDA)"
		_Bank = ComputableObjects.BankEnum.Both
		_Stations = stations
    End Sub
    Public ReadOnly Property GetRiverName As String
        Get
            Return _indexlocation.GetRiverName
        End Get
    End Property
    Public ReadOnly Property GetDescription As String
        Get
            Return _desc
        End Get
    End Property
    Public ReadOnly Property GetIndexLocation As IndexLocation
        Get
            Return _indexlocation
        End Get
    End Property
	Public ReadOnly Property GetBank As ComputableObjects.BankEnum
		Get
			Return _Bank
		End Get
	End Property
	Public ReadOnly Property GetStations As List(Of RiverStation)
        Get
            Return _Stations
        End Get
    End Property
    Public ReadOnly Property GetReachName As String
        Get
            Return _indexlocation.GetReachName
        End Get
    End Property
    Public ReadOnly Property GetUpstreamStation As Double
        Get
            Return _Stations(0).Station
        End Get
    End Property
    Public ReadOnly Property GetDownstreamStation As Double
        Get
            Return _Stations.Last.Station
        End Get
    End Property
    Public Sub AddStation(ByVal station As RiverStation)
        _Stations.Add(station)
        SortStations() 'probably should figure out where to insert, and just insert it there...
    End Sub
    Public Sub AddStations(ByVal stations As List(Of RiverStation))
        _Stations.AddRange(stations)
        SortStations()
    End Sub
    Private Sub SortStations()
        'needs to be tested
        _Stations.Sort(Function(s1 As RiverStation, s2 As RiverStation)
                           Return s1.Station.CompareTo(s2.Station)
                       End Function)
    End Sub
    ''' <summary>
    ''' Takes an input stage and gets the full river station information for that location
    ''' </summary>
    ''' <param name="Station"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetInterpolatedStationInfo(ByVal Station As Double) As RiverStation
        If Station < _Stations(0).Station Then Throw New ArgumentException("The Station " & Station & " is smaller than the smallest station value of this reach")
        If Station > _Stations(_Stations.Count - 1).Station Then Throw New ArgumentException("The Station " & Station & " is larger than the largest station value of this reach")
        Dim lowerindex As Integer = 0
        Dim upperindex As Integer = 0
        For i = 1 To _Stations.Count - 1
            If Station <= _Stations(i).Station Then
                upperindex = i
                lowerindex = i - 1
            End If
        Next
        Dim interpvalue As Double = (Station - _Stations(lowerindex).Station) / (_Stations(upperindex).Station - _Stations(lowerindex).Station)
        Dim flow As New List(Of Single)
        Dim stage As New List(Of Single)
        For i = 0 To _Stations(0).Flows.Count - 1
            flow.Add(_Stations(lowerindex).Flows(i) + (interpvalue * (_Stations(upperindex).Flows(i) - _Stations(lowerindex).Flows(i))))
            stage.Add(_Stations(lowerindex).WaterSurfaceElevations(i) + (interpvalue * (_Stations(upperindex).WaterSurfaceElevations(i) - _Stations(lowerindex).WaterSurfaceElevations(i))))
        Next
        Dim invert As Double = _Stations(lowerindex).Invert + (interpvalue * (_Stations(upperindex).Invert - _Stations(lowerindex).Invert))
        Return New RiverStation(Station, invert, _Stations(lowerindex).Probabilities, flow.ToArray, stage.ToArray)
    End Function
    Public Overrides Function ToString() As String
        Return GetReachName & vbTab & _desc & vbTab & GetRiverName & vbTab & GetUpstreamStation & vbTab & GetDownstreamStation & vbTab & _Bank & vbTab & _indexlocation.GetStationing.Station & vbTab & vbNull
    End Function
    Public Function WriteToEconWSP(ByVal WSP_Name As String, ByVal plan As String, ByVal Year As String) As String
        Dim t As New System.Text.StringBuilder
        t.Append("WSP_Name" & vbTab & "Description" & vbTab & vbTab & "Plan" & vbTab & vbTab & "Year" & vbTab & vbTab & "Stream" & vbTab & vbTab & "Type" & vbTab & vbTab & "Notes" & vbNewLine) 'kurt's line had two extra vbtabs at the end before the newline
        t.Append(WSP_Name & vbTab & _desc & vbTab & vbTab & plan & vbTab & vbTab & Year & vbTab & vbTab & GetRiverName & vbTab & vbTab & "DISCHARGE_FREQ" & vbTab & vbTab & vbNull & vbNewLine)
        t.Append("WSP_Probability" & vbNewLine)
        t.Append("WSP_Probability" & vbNewLine)
        'write probability line
        t.Append(vbTab & vbTab)
        For i = 0 To _Stations(0).Probabilities.Count - 1
            t.Append(_Stations(0).Probabilities(i) & vbTab)
        Next
        t.Append(vbTab)
        For i = 0 To _Stations(0).Probabilities.Count - 1
            t.Append(_Stations(0).Probabilities(i) & vbTab)
        Next
        'write header line
        t.Append(vbNewLine)
        t.Append("WSP_Station" & vbTab & "Invert" & vbTab)
        For i = 0 To _Stations(0).Probabilities.Count - 1
            t.Append("Stage" & vbTab)
        Next
        t.Append(vbTab)
        For i = 0 To _Stations(0).Probabilities.Count - 1
            t.Append("Q" & vbTab)
        Next
        t.Append(vbNewLine)
        ' write out each wsp station
        For j = 0 To _Stations.Count - 1
            t.AppendLine(_Stations(j).ToString)
        Next
        Return t.ToString
    End Function
    Public Sub WriteToRasWSP(ByVal filepath As String, ByVal plan As String)
        Dim fs As New System.IO.FileStream(filepath, IO.FileMode.Create, IO.FileAccess.Write)
        Dim sw As New System.IO.StreamWriter(fs)
        sw.WriteLine("Profile Output Table - HEC-FDA")
        sw.WriteLine("2d grid set Plan: " & plan & "   River: " & GetRiverName & "   Reach: " & GetReachName)
        sw.WriteLine()
        sw.WriteLine("# Rivers            = " & 1)
        sw.WriteLine("# Hydraulic Reaches = " & 1)
        sw.WriteLine("# River Stations    = " & _Stations.Count)
        sw.WriteLine("# Plans             = " & 1)
        sw.WriteLine("# Profiles          = " & _Stations(0).Flows.Count)
        sw.WriteLine()
        sw.WriteLine(" Reach            River Sta        Profile            Q Total  Min Ch El  W.S. Elev ")
        sw.WriteLine("                                                        (cfs)       (ft)       (ft) ")
        sw.WriteLine()

        'now write out the data.
        Dim baseline As String = " " & GetReachName
        Dim pad As Char = Convert.ToChar(" ")
        baseline = baseline.PadRight(17, pad)
        Dim stationline As String = ""
        baseline = baseline & " "
        Dim profileline As String = ""
        Dim flowstring As String = ""
        Dim invertstring As String = ""
        Dim wsestring As String = ""
		For Each riverstat As RiverStation In _Stations
			stationline = baseline & riverstat.Station
			stationline = stationline.PadRight(35, pad)
			invertstring = riverstat.Invert
			invertstring = invertstring.PadLeft(10)
			For i = 0 To riverstat.Flows.Count - 1
				profileline = stationline & "PF " & i
				profileline = profileline.PadRight(51)
				flowstring = riverstat.Flows(i)
				flowstring = flowstring.PadLeft(10, pad)
				wsestring = riverstat.WaterSurfaceElevations(i)
				wsestring = wsestring.PadLeft(10, pad)
				sw.WriteLine(profileline & flowstring & " " & invertstring & " " & wsestring & " ")
			Next
			sw.WriteLine()

		Next
		sw.Close() : sw.Dispose()
        fs.Close() : fs.Dispose()

    End Sub
End Class
