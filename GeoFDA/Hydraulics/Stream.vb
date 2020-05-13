Public Class Stream
    Private _StreamName As String
    Private _StreamDescription As String
    Sub New(ByVal streamName As String, ByVal StreamDescription As String)
        _StreamDescription = StreamDescription
        _StreamName = streamName
    End Sub
    Public ReadOnly Property GetStreamName As String
        Get
            Return _StreamName
        End Get
    End Property
    Public ReadOnly Property GetStreamDescription As String
        Get
            Return _StreamDescription
        End Get
    End Property
    Public Overrides Function ToString() As String
        Return _StreamName & vbTab & _StreamDescription
    End Function
End Class
