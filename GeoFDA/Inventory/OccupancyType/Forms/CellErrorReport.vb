Namespace AutoGenerate
    Public Class CellErrorReport
        Property rownumber As Integer
        Property ParameterName As String
        Property ColumnNumber As Integer
        Property tooltipMessage As String
        Property IsValid As Boolean = False
        Sub New()
        End Sub
        Sub New(ByVal row As Integer, ByVal col As Integer, ByVal paramname As String, ByVal message As String, ByVal iserror As Boolean)
            rownumber = row
            ColumnNumber = col
            ParameterName = paramname
            tooltipMessage = message
            IsValid = Not iserror
        End Sub
    End Class

End Namespace

