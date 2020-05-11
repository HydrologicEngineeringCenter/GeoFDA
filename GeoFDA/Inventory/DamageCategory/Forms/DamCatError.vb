Public Class DamCatError
    Private _rownum As Integer
    Private _message As String
    Public ReadOnly Property Message As String
        Get
            Return _message
        End Get
    End Property
    Public ReadOnly Property Row As Integer
        Get
            Return _rownum
        End Get
    End Property
    Sub New(ByVal rownum As Integer, ByVal msg As String)
        _rownum = rownum
        _message = msg
    End Sub
End Class
