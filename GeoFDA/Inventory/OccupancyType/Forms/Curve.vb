Namespace AutoGenerate
    Public Class Curve
        Private _RowItems As System.Collections.ObjectModel.ObservableCollection(Of Object)
        Public Property MonotonicCurveUSingle As Boolean = False
        Public Property MonotonicIncreasing As Boolean = False
        Public Property MonotonicDecreasing As Boolean = False
        Public Property GenericCurve As Boolean = False
        Public Property GenericCurveUncertain As Boolean = False
        Public Property CurveDisplayName As String
        Public Property CanValidate As Boolean = True
        Public Property Curve As System.Collections.ObjectModel.ObservableCollection(Of Object)
            Get
                Return _RowItems
            End Get
            Set(value As System.Collections.ObjectModel.ObservableCollection(Of Object))
                _RowItems = value
            End Set
        End Property
        Public Property Headers As List(Of String)
        Sub New(ByVal xvalues As List(Of Single), ByVal yvalues As List(Of Statistics.ContinuousDistribution), ByVal validation As ValidationTypes)
            loadFromListsUncertain(xvalues, yvalues)
            setvalidation(validation)
        End Sub
        Sub New(ByVal curve As Statistics.PairedData, ByVal validation As ValidationTypes)
            _RowItems = New System.Collections.ObjectModel.ObservableCollection(Of Object)
            Dim ri As CurveRowItem
            Select Case curve.GetType
                Case GetType(Statistics.GenericCurveSingle)
                    Dim gcs As Statistics.GenericCurveSingle = CType(curve, Statistics.GenericCurveSingle)
                    If gcs.X.Count = gcs.Y.Count Then
                        For i = 0 To gcs.X.Count - 1
                            ri = New CurveSingleRowItem(gcs.X(i), gcs.Y(i))
                            ri.RowNumber = i
                            _RowItems.Add(ri)
                            If i = 0 Then CurveDisplayName = "Single"
                        Next
                    End If
                Case GetType(Statistics.GenericCurveSingleUncertain)
                    Dim gcsu As Statistics.GenericCurveSingleUncertain = CType(curve, Statistics.GenericCurveSingleUncertain)
                    loadFromListsUncertain(gcsu.X, gcsu.Y)
                Case GetType(Statistics.MonotonicCurveUSingle)
                    Dim mcsu As Statistics.MonotonicCurveUSingle = CType(curve, Statistics.MonotonicCurveUSingle)
                    loadFromListsUncertain(mcsu.X, mcsu.Y)
                Case GetType(Statistics.MonotonicCurveIncreasing)
                    Dim gcs As Statistics.MonotonicCurveIncreasing = CType(curve, Statistics.MonotonicCurveIncreasing)
                    If gcs.X.Count = gcs.Y.Count Then
                        For i = 0 To gcs.X.Count - 1
                            ri = New CurveSingleRowItem(gcs.X(i), gcs.Y(i))
                            ri.RowNumber = i
                            _RowItems.Add(ri)
                            If i = 0 Then CurveDisplayName = "Single Increasing"
                        Next
                    End If
                Case GetType(Statistics.MonotonicCurveDecreasing)
                    Dim gcs As Statistics.MonotonicCurveDecreasing = CType(curve, Statistics.MonotonicCurveDecreasing)
                    If gcs.X.Count = gcs.Y.Count Then
                        For i = 0 To gcs.X.Count - 1
                            ri = New CurveSingleRowItem(gcs.X(i), gcs.Y(i))
                            ri.RowNumber = i
                            _RowItems.Add(ri)
                            If i = 0 Then CurveDisplayName = "Single Decreasing"
                        Next
                    End If
                Case Else
            End Select
            setvalidation(validation)
        End Sub
        Public Sub AddRows(ByVal InsertAtIndex As Integer, ByVal number As Integer)
            Dim btcri As CurveRowItem
            For i = InsertAtIndex To number + InsertAtIndex - 1
                btcri = _RowItems(i)
                btcri.RowNumber = i
            Next
            For i = InsertAtIndex To _RowItems.Count - 1
                btcri = _RowItems(i)
                btcri.RowNumber = i
            Next
        End Sub
        Public Sub RemoveRows(ByVal indices As List(Of Int32))
            Dim btcri As CurveRowItem
            For i = 0 To _RowItems.Count - 1
                btcri = _RowItems(i)
                btcri.RowNumber = i
            Next
        End Sub
        Private Sub setvalidation(ByVal validationrule As ValidationTypes)
            Select Case validationrule
                Case ValidationTypes.GenericCurveSingle
                    GenericCurve = True
                Case ValidationTypes.GenericCurveUncertian
                    GenericCurveUncertain = True
                Case ValidationTypes.MonotonicDecreasing
                    MonotonicDecreasing = True
                Case ValidationTypes.MonotonicIncreasing
                    MonotonicIncreasing = True
                Case ValidationTypes.MonotonicIncreasingUncertain
                    MonotonicCurveUSingle = True
                Case Else
            End Select
        End Sub
        Private Sub loadFromListsUncertain(ByVal xvalues As List(Of Single), ByVal yvalues As List(Of Statistics.ContinuousDistribution))
            _RowItems = New System.Collections.ObjectModel.ObservableCollection(Of Object)
            Dim ri As CurveRowItem
            If xvalues.Count = yvalues.Count Then
                For i = 0 To xvalues.Count - 1
                    Select Case yvalues(0).GetType
                        Case GetType(Statistics.None)
                            ri = New NoneCurveRowItem(xvalues(i), yvalues(i))
                            ri.RowNumber = i
                            _RowItems.Add(ri)
                            If i = 0 Then CurveDisplayName = "None"
                        Case GetType(Statistics.Triangular)
                            ri = New TriangularCurveRowItem(xvalues(i), yvalues(i))
                            ri.RowNumber = i
                            _RowItems.Add(ri)
                            If i = 0 Then CurveDisplayName = "Triangular"
                        Case GetType(Statistics.Normal)
                            ri = New NormalCurveRowItem(xvalues(i), yvalues(i))
                            ri.RowNumber = i
                            _RowItems.Add(ri)
                            If i = 0 Then CurveDisplayName = "Normal"
                        Case GetType(Statistics.Uniform)
                            ri = New UniformCurveRowItem(xvalues(i), yvalues(i))
                            ri.RowNumber = i
                            _RowItems.Add(ri)
                            If i = 0 Then CurveDisplayName = "Uniform"
                    End Select
                Next
            Else
                'mismatch
            End If
        End Sub
        Function Validate(ByVal minvalue As Single, ByVal maxvalue As Single) As List(Of CellErrorReport)
            If _RowItems.Count = 0 Then Return Nothing
            Dim report As List(Of AutoGenerate.CellErrorReport) = ValidateCurve()
            Dim rowreport As List(Of AutoGenerate.CellErrorReport) = ValidateRows(minvalue, maxvalue)
            If IsNothing(report) Then
                If IsNothing(rowreport) Then
                    Return Nothing
                Else
                    Return rowreport
                End If
            Else
                If IsNothing(rowreport) Then
                    Return report
                Else
                    report.AddRange(rowreport)
                    Return report
                End If
            End If
        End Function
        Private Function ValidateCurve() As List(Of CellErrorReport)
            Dim c As Statistics.PairedData
            If GenericCurve Then
                Return Nothing
            End If
            If GenericCurveUncertain Then
                Return Nothing
            End If
            If MonotonicCurveUSingle Then
                Dim xvalues(_RowItems.Count - 1) As Single
                Dim yvalues(_RowItems.Count - 1) As Statistics.ContinuousDistribution
                For i = 0 To _RowItems.Count - 1
                    xvalues(i) = _RowItems(i).GetXValue
                    yvalues(i) = _RowItems(i).GetDistribution
                Next
                c = New Statistics.MonotonicCurveUSingle(xvalues, yvalues)
                Return convertStatisticsReportToCellErrorReport(c.Verify)
            End If
            If MonotonicDecreasing Then
                Dim xvalues(_RowItems.Count - 1) As Single
                Dim yvalues(_RowItems.Count - 1) As Single
                For i = 0 To _RowItems.Count - 1
                    xvalues(i) = _RowItems(i).GetXValue
                    yvalues(i) = _RowItems(i).GetYvalue
                Next
                c = New Statistics.MonotonicCurveDecreasing(xvalues, yvalues)
                Return convertStatisticsReportToCellErrorReport(c.Verify)
            End If
            If MonotonicIncreasing Then
                Dim xvalues(_RowItems.Count - 1) As Single
                Dim yvalues(_RowItems.Count - 1) As Single
                For i = 0 To _RowItems.Count - 1
                    xvalues(i) = _RowItems(i).GetXValue
                    yvalues(i) = _RowItems(i).GetYvalue
                Next
                c = New Statistics.MonotonicCurveIncreasing(xvalues, yvalues)
                Return convertStatisticsReportToCellErrorReport(c.Verify)
            End If
            Return Nothing
        End Function
        Private Function ValidateRows(ByVal minvalue As Single, ByVal maxvalue As Single) As List(Of CellErrorReport)
            Dim ret As New List(Of CellErrorReport)
            Dim tmpret As New List(Of CellErrorReport)
            For i = 0 To _RowItems.Count - 1
                tmpret = CType(_RowItems(i), CurveRowItem).ValidateRow(minvalue, maxvalue)
                If Not IsNothing(tmpret) Then ret.AddRange(tmpret)
            Next
            If ret.Count = 0 Then Return Nothing
            Return ret
        End Function
        Private Function convertStatisticsReportToCellErrorReport(ByVal report As Statistics.ErrorReport) As List(Of CellErrorReport)
            If report.Errors.Count > 0 Then
                Dim ret As New List(Of CellErrorReport)
                For i = 0 To report.Errors.Count - 1
                    Dim cr As New CellErrorReport
                    cr.IsValid = False
                    cr.rownumber = report.Errors(i).Row
                    cr.ColumnNumber = report.Errors(i).Column
                    cr.tooltipMessage = report.Errors(i).GetMessage
                    ret.Add(cr)
                Next
                Return ret
            End If
            Return Nothing
        End Function
    End Class
End Namespace



