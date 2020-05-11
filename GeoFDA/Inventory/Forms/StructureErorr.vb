Public Class StructureErorr
    Private _error As String
    Private _Index As Integer
    Public ReadOnly Property Error_Message As String
        Get
            Return _error
        End Get
    End Property
    Public ReadOnly Property Index As Integer
        Get
            Return _index
        End Get
    End Property
    Public Sub New(ByVal errormsg As String, ByVal ind As Integer)
        _error = errormsg
        _index = ind
    End Sub
End Class
