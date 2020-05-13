Namespace AutoGenerate
    Public MustInherit Class CurveRowItem
        Public MustOverride Function GetYValue() As Single
        Public MustOverride Function GetXvalue() As Single
        Public Property RowNumber As Integer
        Public Sub New()

        End Sub
        Public MustOverride Function GetPlotNames() As List(Of String)
        Public MustOverride Function GetPlotData() As List(Of Single)
        Public Overridable Function Validate(ByVal PropertyName As String) As CellErrorReport
            Return Nothing
        End Function
        Public Overridable Function ValidateRow(ByVal minvalue As Single, ByVal maxvalue As Single) As List(Of CellErrorReport)
            Return Nothing
        End Function
    End Class
End Namespace
