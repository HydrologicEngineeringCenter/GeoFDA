Namespace AutoGenerate
    Public Class NormalCurveRowItem
        Inherits CurveUncertainRowItem
        Implements IUncertainType
        Private _x As String
        Private _y As String
        Private _stdev As String
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
                If Single.TryParse(value, s) Then _y = value : GetDistribution = New Statistics.Normal(CSng(s), CType(_dist, Statistics.Normal).GetStDev)
            End Set
        End Property
        Public Property Standard_Deviation As String
            Get
                Return _stdev
            End Get
            Set(value As String)
                Dim s As Single
                If Single.TryParse(value, s) Then _stdev = value : GetDistribution = New Statistics.Normal(CType(_dist, Statistics.Normal).GetCentralTendency, CSng(s))
            End Set
        End Property
        Sub New()
            MyBase.New()
            GetDistribution = New Statistics.Normal(0, 0)
            X = ""
            Y = ""
            Standard_Deviation = ""
        End Sub
        Sub New(ByVal xvalue As Single, ByVal Norm As Statistics.Normal)
            X = xvalue
            _dist = Norm
            Y = Norm.GetCentralTendency
            Standard_Deviation = Norm.GetStDev

        End Sub
        Public Overrides Function ValidateRow(ByVal MinYvalue As Single, ByVal MaxYvalue As Single) As List(Of CellErrorReport)
            Dim ret As New List(Of CellErrorReport)
            Dim y As Single = GetYValue()
            If y > MaxYValue Then ret.Add(New CellErrorReport(RowNumber, 1, "Y", "The Y value was greater than the Maximum allowed Y value of " & MaxYValue.ToString, True))
            If y < MinYvalue Then ret.Add(New CellErrorReport(RowNumber, 1, "Y", "The Y value was less than the Minimum allowed Y value of " & MinYvalue.ToString, True))
            Dim max As Single = _dist.getDistributedVariable(1)
            If max > MaxYValue Then ret.Add(New CellErrorReport(RowNumber, 2, "Standard Deviation", "The Standard Deviation yeilds a max value greater than the Maximum allowed value of " & MaxYValue.ToString, True))
            If max < MinYvalue Then ret.Add(New CellErrorReport(RowNumber, 2, "Standard Deviation", "The Standard Deviation yeilds a min value less than the Minimum allowed value of " & MinYvalue.ToString, True))
            Dim min As Single = _dist.getDistributedVariable(0)
            If min > MaxYValue Then ret.Add(New CellErrorReport(RowNumber, 2, "Standard Deviation", "The Standard Deviation yeilds a min value greater than the Maximum allowed value of " & MaxYValue.ToString, True))
            If min < MinYvalue Then ret.Add(New CellErrorReport(RowNumber, 2, "Standard Deviation", "The Standard Deviation yeilds a min value less than the Minimum allowed value of " & MinYvalue.ToString, True))
            Dim Yerror As CellErrorReport = Validate("Y")
            If Not IsNothing(Yerror) Then ret.Add(Yerror)
            Dim stderror As CellErrorReport = Validate("Standard_Deviation")
            If Not IsNothing(stderror) Then ret.Add(stderror)
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
                    report.IsValid = True
                    report.ColumnNumber = 1
                    Return Nothing
                Case "Standard_Deviation"
                    message = _dist.Validate()
                    If IsNothing(message) Then Return Nothing
                    report.tooltipMessage = message
                    report.ColumnNumber = 2
                    Return report
                Case Else
                    Return Nothing
            End Select

        End Function
        Public Overrides Function GetXValue() As Single
            Return CSng(X)
        End Function
        Public Function GetDisplayName1() As String Implements IUncertainType.GetDisplayName
            Return "Normal"
        End Function
        Public Overrides Function GetPlotNames() As List(Of String)
            Return {"Y", "Min", "Max"}.ToList
        End Function

        Public Overrides Function GetPlotData() As List(Of Single)
            Return {CSng(_x), CSng(_dist.GetCentralTendency), CSng(_dist.getDistributedVariable(0)), CSng(_dist.getDistributedVariable(1))}.ToList
        End Function
    End Class
End Namespace

