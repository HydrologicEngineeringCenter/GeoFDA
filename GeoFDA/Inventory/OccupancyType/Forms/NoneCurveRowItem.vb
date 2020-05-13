Namespace AutoGenerate
    Public Class NoneCurveRowItem
        Inherits CurveUncertainRowItem
        Implements IUncertainType

        Private _x As String
        Private _y As String
        Public Property X As String
            Get
                Return _x
            End Get
            Set(value As String)
                Dim s As Single
                If Single.TryParse(value, s) Then _x = value
            End Set
        End Property
        Public Property Y As String
            Get
                Return _y
            End Get
            Set(value As String)
                Dim s As Single
                If Single.TryParse(value, s) Then _y = value : GetDistribution = New Statistics.None(CDbl(s))
            End Set
        End Property
        Sub New()
            MyBase.New()
            GetDistribution = New Statistics.None(0)
            X = ""
            Y = ""
        End Sub
        Sub New(ByVal xvalue As Single, ByVal dist As Statistics.None)
            X = xvalue
            GetDistribution = dist
            Y = dist.GetCentralTendency
        End Sub
        Public Overrides Function ValidateRow(ByVal MinYvalue As Single, ByVal MaxYvalue As Single) As List(Of CellErrorReport)
            Dim y As Single = GetYValue()
            Dim errors As New List(Of CellErrorReport)
            If y > MaxYValue Then errors.Add(New CellErrorReport(RowNumber, 1, "Y", "The Y value was greater than the Maximum allowed Y value of " & MaxYValue.ToString, True))
            If y < MinYvalue Then errors.Add(New CellErrorReport(RowNumber, 1, "Y", "The Y value was less than the Minimum allowed Y value of " & MinYvalue.ToString, True))
            If errors.Count = 0 Then Return Nothing
            Return errors
        End Function
        Public Overrides Function GetXvalue() As Single
            Return CSng(X)
        End Function
        Public Function GetDisplayName1() As String Implements IUncertainType.GetDisplayName
            Return "None"
        End Function
        Public Overrides Function GetPlotNames() As List(Of String)
            Return {"Y"}.ToList
        End Function
        Public Overrides Function GetPlotData() As List(Of Single)
            Return {CSng(_x), CSng(_y)}.ToList
        End Function
    End Class
End Namespace

