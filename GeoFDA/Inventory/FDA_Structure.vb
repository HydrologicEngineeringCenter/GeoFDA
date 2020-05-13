Public Class FDA_Structure
    Private _name As String
    'Private _damagecategory As DamageCategory
    Private _occupancytype As Consequences_Assist.ComputableObjects.OccupancyType
    Private _Stream_Name As String
    Private _Station As Double
	Private _bank As ComputableObjects.BankEnum
	Private _Year As String
    Private _structVal As Double
    Private _contVal As Double
    Private _otherval As Double
    Private _FirstFloorStage As Double
    Private _GrndStage As Double
    Private _FoundationHeight As Double
    Private _Mod_name As String
    Private _Sid_Rch As String
    Private _location As LifeSimGIS.PointD
    ''' <summary>
    ''' created to allow for the stationing, ground elevation, to be assigned by a wsp.
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="occtype"></param>
    ''' <param name="structurevalue"></param>
    ''' <param name="foundationheight"></param>
    ''' <remarks></remarks>
    Sub New(ByVal name As String, ByVal occtype As Consequences_Assist.ComputableObjects.OccupancyType, ByVal structurevalue As String, ByVal foundationheight As String)
        _name = name
        _occupancytype = occtype
        _structVal = structurevalue
        _FoundationHeight = foundationheight
		_bank = ComputableObjects.BankEnum.Both
	End Sub
    Sub New(ByVal tabbedline() As String, ByVal headers() As String, ByVal occtype As Consequences_Assist.ComputableObjects.OccupancyType)
        _occupancytype = occtype
        Dim north As Double
        If Not Double.TryParse(tabbedline(Array.IndexOf(headers, "North")), north) Then Throw New Exception("the North value could not be converted to double for structure: " & tabbedline(Array.IndexOf(headers, "Struc_Name")))
        Dim east As Double
        If Not Double.TryParse(tabbedline(Array.IndexOf(headers, "East")), east) Then Throw New Exception("the East value could not be converted to double for structure: " & tabbedline(Array.IndexOf(headers, "Struc_Name")))
        If north = 0 AndAlso east = 0 Then Throw New Exception("The strucutre " & tabbedline(Array.IndexOf(headers, "Struc_Name")) & " does not have valid coordinates")
        _location = New LifeSimGIS.PointD(east, north)

        For i = 0 To headers.Count - 1
            Select Case headers(i)
                Case "Struc_Name"
                    If tabbedline(i) = "" OrElse tabbedline(i) = " " Then Throw New Exception("Name field was blank")
                    If tabbedline(i).Length > 16 Then Throw New Exception("The structure name " & tabbedline(i) & " is longer than 16 characters.")
                    'check for name conflicts
                    _name = tabbedline(i)
                Case "Stream_Name"
                    If tabbedline(i) = "" OrElse tabbedline(i) = " " Then Throw New Exception("Stream field was blank")
                    If tabbedline(i).Length > 16 Then Throw New Exception("The stream name " & tabbedline(i) & " is longer than 16 characters.")
                    _Stream_Name = tabbedline(i)
                Case "Station"
                    Double.TryParse(tabbedline(i), _Station)
                Case "Bank"
                    Select Case tabbedline(i)
                        Case "l", "L", "1", "Left", "left", "LEFT"
							_bank = ComputableObjects.BankEnum.Left
						Case "r", "R", "0", "Right", "right", "RIGHT"
							_bank = ComputableObjects.BankEnum.Right
						Case Else
							_bank = ComputableObjects.BankEnum.Both
					End Select
                Case "Year"
                    If tabbedline(i) = "" OrElse tabbedline(i) = " " Then
                        'year is blank.
                    Else
                        Integer.TryParse(tabbedline(i), _Year) ' how do i know what fields need to be output to shape?
                    End If
                Case "Struc_Val"
                    'assumed to be in thousands, check with bob.
                    If Double.TryParse(tabbedline(i), _structVal) Then _structVal *= 1000 ' should i complain if i dont get a structure value? it is not required for fda.
                Case "Cont_Val"
                    If Double.TryParse(tabbedline(i), _contVal) Then _contVal *= 1000
                Case "Other_Val"
                    If Double.TryParse(tabbedline(i), _otherval) Then _otherval *= 1000
                Case "1F_Stage"
                    Double.TryParse(tabbedline(i), _FirstFloorStage)
                Case "Grnd_Stage"
                    Double.TryParse(tabbedline(i), _GrndStage)
                Case "Found_Ht"
                    Double.TryParse(tabbedline(i), _FoundationHeight)
                Case "Begin_Damg"
                    '
                Case "Street"
                    '
                Case "City"
                    '
                Case "State"
                    '
                Case "Zip"
                    '
                Case "Zone"
                    '
                Case "Num_Struct"
                    '
                Case "Notes"
                Case "Mod_Name"
                    If tabbedline(i) = "" OrElse tabbedline(i) = " " Then _Mod_name = "Base"
                    _Mod_name = CStr(tabbedline(i))
                Case "SID_Rch"
                    _Sid_Rch = CStr(tabbedline(i))
                Case "SID_Reffld"
                Case Else

            End Select
        Next
    End Sub
    Public ReadOnly Property GetName As String
        Get
            Return _name
        End Get
    End Property
    Public ReadOnly Property GetOccupancyType As Consequences_Assist.ComputableObjects.OccupancyType
        Get
            Return _occupancytype
        End Get
    End Property
    Public WriteOnly Property SetSidReach As String
        Set(value As String)
            _Sid_Rch = value
        End Set
    End Property
    Public WriteOnly Property SetLocation As LifeSimGIS.PointD
        Set(value As LifeSimGIS.PointD)
            _location = value
        End Set
    End Property
    Public ReadOnly Property GetLocation As LifeSimGIS.PointD
        Get
            Return _location
        End Get
    End Property
    Public Property Stream As String
        Get
            Return _Stream_Name
        End Get
        Set(value As String)
            _Stream_Name = value
        End Set
    End Property
    Public WriteOnly Property SetGroundElevation As Double
        Set(value As Double)
            _GrndStage = value
        End Set
    End Property
    Public ReadOnly Property GetGroundElevation As Double
        Get
            Return _GrndStage
        End Get
    End Property
    Public Property Stationing As Double
        Get
            Return _Station
        End Get
        Set(value As Double)
            _Station = value
        End Set
    End Property
    Public ReadOnly Property GetStationing As Double
        Get
            Return _Station
        End Get
    End Property
    Public Property LeftBank As Boolean
        Get
			If _bank = ComputableObjects.BankEnum.Left Then
				Return True
			Else
				Return False
            End If
        End Get
        Set(value As Boolean)
            If value Then
				_bank = ComputableObjects.BankEnum.Left
			Else
                'it isnt left
            End If
        End Set
    End Property
    Public Property RightBank As Boolean
        Get
			If _bank = ComputableObjects.BankEnum.Right Then
				Return True
			Else
				Return False
            End If
        End Get
        Set(value As Boolean)
            If value Then
				_bank = ComputableObjects.BankEnum.Right
			Else
                'it isnt left
            End If
        End Set
    End Property

    Public Overrides Function ToString() As String
        Dim str As New System.Text.StringBuilder
        str.Append(_Name & vbTab)
        str.Append(_occupancytype.DamageCategory.Name & vbTab)
        str.Append(_Stream_Name & vbTab)
        str.Append(_occupancytype.Name & vbTab)
        str.Append(_Station & vbTab)
        str.Append(_bank.ToString & vbTab)
        str.Append(_Year & vbTab)
        str.Append(_structVal & vbTab)
        str.Append(_contVal & vbTab)
        str.Append(_otherval & vbTab)
        str.Append(_FirstFloorStage & vbTab)
        str.Append(_GrndStage & vbTab)
        str.Append(_FoundationHeight & vbTab)
        str.Append(_Sid_Rch & vbTab)
        str.Append(_location.Y & vbTab)
        str.Append(_location.X & vbTab)
        str.Append(_Mod_name & vbTab)
        Return str.ToString
    End Function

End Class
