Public Class RiverStation
    Private _station As Single
    Private _invert As Single
    Private _probabilities As Single()
    Private _flows As Single()
    Private _WaterSurfaceElevations As Single()
    Sub New(ByVal station As Single, ByVal invert As Single, ByVal probabilities As Single(), ByVal flows As Single(), ByVal stages() As Single)
        _station = station
        _invert = invert
        ReDim _probabilities(probabilities.Count - 1)
        probabilities.CopyTo(_probabilities, 0)
        ReDim _flows(probabilities.Count - 1)
        flows.CopyTo(_flows, 0)
        ReDim _WaterSurfaceElevations(stages.Count - 1)
        stages.CopyTo(_WaterSurfaceElevations, 0)

    End Sub
    Public Property Station As Single
        Get
            Return _station
        End Get
        Set(value As Single)
            _station = value
        End Set
    End Property
    Public Property Invert As Single
        Get
            Return _invert
        End Get
        Set(value As Single)
            _invert = value
        End Set
    End Property
    Public Property Probabilities As Single()
        Get
            Return _probabilities
        End Get
        Set(value As Single())
            _probabilities = value
        End Set
    End Property
    Public Property Flows As Single()
        Get
            Return _flows
        End Get
        Set(value As Single())
            _flows = value
        End Set
    End Property
    Public Property WaterSurfaceElevations As Single()
        Get
            Return _WaterSurfaceElevations
        End Get
        Set(value As Single())
            _WaterSurfaceElevations = value
        End Set
    End Property
	Public Overrides Function ToString() As String
		Dim t As New System.Text.StringBuilder

		t.Append(_station & vbTab & _invert & vbTab)
		For i = 0 To _WaterSurfaceElevations.Count - 1
			t.Append(_WaterSurfaceElevations(i) & vbTab)
		Next
		t.Append(vbTab)
		For i = 0 To _flows.Count - 1
			t.Append(_flows(i) & vbTab)
		Next
		Return t.ToString
	End Function
End Class
