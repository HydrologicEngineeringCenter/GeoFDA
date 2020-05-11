Public Class ImpactAreaError
    Private _message As String
    Private _Row As Integer
    Private _col As Integer
    Public ReadOnly Property Message As String
        Get
            Return _message
        End Get
    End Property
    Public Property Row As Integer
        Set(value As Integer)
            _Row = value
        End Set
        Get
            Return _Row
        End Get
    End Property
    Public ReadOnly Property Column As Integer
        Get
            Return _col
        End Get
    End Property
    Sub New(ByVal msg As String, ByVal r As Integer, c As Integer)
        _message = msg
        _Row = r
        _col = c
    End Sub
End Class
