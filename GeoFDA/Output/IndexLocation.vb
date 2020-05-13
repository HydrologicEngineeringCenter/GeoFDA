Public Class IndexLocation
#Region "Location Info"
    Private _coordinates As Integer 'convert to pointd
    Private _river As String
    Private _reach As String
    Private _stationing As RiverStation
#End Region
#Region "Constructors"
	Sub New(ByVal river As String, ByVal reach As String, ByVal riverstation As RiverStation)
        _river = river
        _reach = reach
        _stationing = riverstation
    End Sub
#End Region
#Region "Properties"
    Public ReadOnly Property GetRiverName As String
        Get
            Return _river
        End Get
    End Property
    Public WriteOnly Property SetRiverName As String
        Set(value As String)
            _river = value
        End Set
    End Property
    Public ReadOnly Property GetReachName As String
        Get
            Return _reach
        End Get
    End Property
    Public WriteOnly Property SetReachName As String
        Set(value As String)
            _reach = value
        End Set
    End Property
    Public ReadOnly Property GetStationing As RiverStation
        Get
            Return _stationing
        End Get
    End Property
    Public WriteOnly Property SetStationing As RiverStation
        Set(value As RiverStation)
            _stationing = value
        End Set
    End Property

#End Region


End Class
