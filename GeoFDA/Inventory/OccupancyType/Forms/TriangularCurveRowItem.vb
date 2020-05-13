Namespace AutoGenerate
    Public Class TriangularCurveRowItem
        Inherits CurveUncertainRowItem
        Implements IUncertainType
        Private _x As String
        Private _y As String
        Private _min As String
        Private _max As String
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
                If Single.TryParse(value, s) Then _y = value : GetDistribution = New Statistics.Triangular(CDbl(_min), CDbl(_max), CDbl(s))
            End Set
        End Property
        Public Property Min As String
            Get
                Return _min
            End Get
            Set(value As String)
                Dim s As Single
                If Single.TryParse(value, s) Then _min = value : GetDistribution = New Statistics.Triangular(CDbl(_min), CDbl(_max), CDbl(_y))
            End Set
        End Property
        Public Property Max As String
            Get
                Return _max
            End Get
            Set(value As String)
                Dim s As Single
                If Single.TryParse(value, s) Then _max = value : GetDistribution = New Statistics.Triangular(CDbl(_min), CDbl(_max), CDbl(_y))
            End Set
        End Property
        Sub New()
            MyBase.New()
            GetDistribution = New Statistics.Triangular(0, 0, 0)
            X = ""
            Y = ""
            Min = ""
            Max = ""
        End Sub
        Sub New(ByVal xvalue As Single, ByVal dist As Statistics.Triangular)
            X = xvalue
            GetDistribution = dist
            Y = dist.GetCentralTendency
            Min = dist.getMin
            Max = dist.getMax
        End Sub
        Public Overrides Function ValidateRow(ByVal MinYvalue As Single, ByVal MaxYvalue As Single) As List(Of CellErrorReport)
            Dim ret As New List(Of CellErrorReport)
            Dim y As Single = GetYValue()
            If y > MaxYValue Then ret.Add(New CellErrorReport(RowNumber, 1, "Y", "The Y value was greater than the Maximum allowed Y value of " & MaxYValue.ToString, True))
            If y < MinYvalue Then ret.Add(New CellErrorReport(RowNumber, 1, "Y", "The Y value was less than the Minimum allowed Y value of " & MinYvalue.ToString, True))
            Dim max As Single = CSng(_max)
            If max > MaxYValue Then ret.Add(New CellErrorReport(RowNumber, 3, "Max", "The Max value is greater than the Maximum allowed value of " & MaxYValue.ToString, True))
            If max < MinYvalue Then ret.Add(New CellErrorReport(RowNumber, 3, "Max", "The Max value is less than the Minimum allowed value of " & MinYvalue.ToString, True))
            Dim min As Single = CSng(_min)
            If min > MaxYValue Then ret.Add(New CellErrorReport(RowNumber, 2, "Min", "The Min value is greater than the Maximum allowed value of " & MaxYValue.ToString, True))
            If min < MinYvalue Then ret.Add(New CellErrorReport(RowNumber, 2, "Min", "The Min value is less than the Minimum allowed value of " & MinYvalue.ToString, True))
            Dim yerror As CellErrorReport = Validate("Y")
            If Not IsNothing(yerror) Then ret.Add(yerror)
            Dim minerror As CellErrorReport = Validate("Min")
            If Not IsNothing(minerror) Then ret.Add(minerror)
            Dim maxerror As CellErrorReport = Validate("Max")
            If Not IsNothing(maxerror) Then ret.Add(maxerror)
            If ret.Count = 0 Then Return Nothing
            Return ret
        End Function
        Public Overrides Function Validate(PropertyName As String) As CellErrorReport
            Dim report As New CellErrorReport
            report.rownumber = RowNumber
            report.ParameterName = PropertyName
            Dim message As String = Nothing
            Select Case PropertyName
                Case "X"
                    report.IsValid = True
                    report.ColumnNumber = 0
                    Return Nothing
                Case "Y"
                    message = _dist.Validate()
                    If IsNothing(message) Then Return Nothing
                    report.tooltipMessage = message
                    report.ColumnNumber = 1
                    Return report
                Case "Min"
                    message = _dist.Validate()
                    If IsNothing(message) Then Return Nothing
                    report.tooltipMessage = message
                    report.ColumnNumber = 2
                    Return report
                Case "Max"
                    message = _dist.Validate()
                    If IsNothing(message) Then Return Nothing
                    report.tooltipMessage = message
                    report.ColumnNumber = 3
                    Return report
                Case Else
                    Return Nothing
            End Select

        End Function
        Public Overrides Function GetXvalue() As Single
            Return CSng(X)
        End Function
        Public Function GetDisplayName1() As String Implements IUncertainType.GetDisplayName
            Return "Triangular"
        End Function
        Public Overrides Function GetPlotNames() As List(Of String)
            Return {"Y", "Min", "Max"}.ToList
        End Function

        Public Overrides Function GetPlotData() As List(Of Single)
            Return {CSng(_x), CSng(_y), CSng(_min), CSng(_max)}.ToList
        End Function
    End Class
End Namespace

