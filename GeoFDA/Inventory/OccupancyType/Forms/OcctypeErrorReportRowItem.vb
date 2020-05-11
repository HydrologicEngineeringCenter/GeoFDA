Public Class OcctypeErrorReportRowItem
    Private _OcctypeName As String
    Private _Message As String
    Public ReadOnly Property OccupancyType As String
        Get
            Return _OcctypeName
        End Get
    End Property
    Public ReadOnly Property Error_Message As String
        Get
            Return _Message
        End Get
    End Property
    Sub New(ByVal occtypename As String, ByVal message As String)
        _OcctypeName = occtypename
        _Message = message
    End Sub
End Class
