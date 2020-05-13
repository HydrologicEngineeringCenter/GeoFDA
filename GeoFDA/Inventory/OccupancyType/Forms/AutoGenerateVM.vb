Public Class AutoGenerateVM
    Implements System.ComponentModel.INotifyPropertyChanged
    'Implements ICommand
    Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Private _curve As AutoGenerate.Curve
    Private _CurvePlotModel As OxyPlot.PlotModel
    Private _Validate As Boolean = True
    Private _isNotreadonly As Boolean = True
    Public Property IsUncertain As Boolean = False
    Private _DistributionTypes As List(Of String)
    Private _Curves As List(Of AutoGenerate.Curve)
    Private _selectedIndex As Int32
    Public Property ChartTitle As String = "Monotonic Curve"
    Public Property YValueTitle As String = "Y Values"
    Public Property XValueTitle As String = "X Values"
    Public Property SelectedIndex As Int32
        Get
            Return _selectedIndex
        End Get
        Set(value As Int32)
            _selectedIndex = value
            NotifyPropertyChanged("SelectedIndex")
            NotifyPropertyChanged("SelectedItem")
        End Set
    End Property
    Public Property SelectedItem As AutoGenerate.Curve
        Get
            Return GetCurves(_selectedIndex)
        End Get
        Set(value As AutoGenerate.Curve)
            _selectedIndex = GetCurves.IndexOf(value)
            NotifyPropertyChanged("SelectedIndex")
            NotifyPropertyChanged("SelectedItem")
            NotifyPropertyChanged("GetCurves")
        End Set
    End Property
    Public Property IsNotReadonly As Boolean
        Get
            Return _isNotreadonly
        End Get
        Set(value As Boolean)
            _isNotreadonly = value
            NotifyPropertyChanged("IsNotReadonly")
        End Set
    End Property
    Public Property GetCurves As List(Of AutoGenerate.Curve)
        Get
            Return _Curves
        End Get
        Set(value As List(Of AutoGenerate.Curve))
            _Curves = value
            NotifyPropertyChanged("GetCurves")
            NotifyPropertyChanged("CurvePlotModel")
        End Set
    End Property
    Public Property CurvePlotModel() As OxyPlot.PlotModel
        Get
            Return _CurvePlotModel
        End Get
        Set(value As OxyPlot.PlotModel)
            _CurvePlotModel = value
            NotifyPropertyChanged("CurvePlotModel")
        End Set
    End Property
    Public Property Validate As Boolean
        Get
            Return _validate
        End Get
        Set(value As Boolean)
            _Validate = value
            GetCurves(SelectedIndex).CanValidate = value
        End Set
    End Property
    Sub New()
        GetCurves = New List(Of AutoGenerate.Curve)
        'IsUncertain = True
        Dim xs As New List(Of Single)
        Dim dists As New List(Of Statistics.ContinuousDistribution)
        For i = 0 To 1
            xs.Add(0)
            dists.Add(New Statistics.None(0))
        Next
        '
        Dim nc As New Statistics.MonotonicCurveUSingle(xs.ToArray, dists.ToArray)
        LoadDefaultUncertianCurves(nc, AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain)
        SetUpPlotmodel()
        PlotGrid()
    End Sub
    Sub New(ByVal statisticscurve As Statistics.PairedData, Optional ByVal validationtype As AutoGenerate.ValidationTypes = AutoGenerate.ValidationTypes.GenericCurveSingle)
        GetCurves = New List(Of AutoGenerate.Curve)
        Select Case validationtype
            Case AutoGenerate.ValidationTypes.GenericCurveUncertian
                IsUncertain = True
                LoadDefaultUncertianCurves(statisticscurve, validationtype)
            Case AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain
                IsUncertain = True
                LoadDefaultUncertianCurves(statisticscurve, validationtype)
                SelectedItem = SelectedItem
            Case Else
                IsUncertain = False
                GetCurves.Add(New AutoGenerate.Curve(statisticscurve, validationtype))
                SelectedIndex = 0
        End Select
        SetUpPlotmodel()
        PlotGrid()
    End Sub
    Private Sub NotifyPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(info))
    End Sub
    Private Sub LoadDefaultUncertianCurves(ByVal curve As Statistics.PairedData, ByVal validation As AutoGenerate.ValidationTypes)
        GetCurves = New List(Of AutoGenerate.Curve)
        Dim xs As New List(Of Single)
        Dim ys As New List(Of Statistics.ContinuousDistribution)
        xs.Add(0)
        ys.Add(New Statistics.None(0))
        GetCurves.Add(New AutoGenerate.Curve(xs, ys, validation))
        ys(0) = New Statistics.Normal(0, 0)
        GetCurves.Add(New AutoGenerate.Curve(xs, ys, validation))
        ys(0) = New Statistics.Triangular(0, 0, 0)
        GetCurves.Add(New AutoGenerate.Curve(xs, ys, validation))
        ys(0) = New Statistics.Uniform(0, 0)
        GetCurves.Add(New AutoGenerate.Curve(xs, ys, validation))

        If IsNothing(curve) Then SelectedIndex = 0 : Exit Sub
        Select Case curve.GetType
            Case GetType(Statistics.MonotonicCurveUSingle)
                If CType(curve, Statistics.MonotonicCurveUSingle).Y.Count = 0 Then SelectedIndex = 0 : Exit Sub
                Select Case CType(curve, Statistics.MonotonicCurveUSingle).Y(0).GetType
                    Case GetType(Statistics.None)
                        GetCurves(0) = New AutoGenerate.Curve(curve, validation)
                        SelectedIndex = 0
                        xs.Clear()
                        ys.Clear()
                        For i = 0 To CType(curve, Statistics.MonotonicCurveUSingle).Y.Count - 1
                            xs.Add(CType(curve, Statistics.MonotonicCurveUSingle).X(i))
                            ys.Add(New Statistics.Normal(CType(curve, Statistics.MonotonicCurveUSingle).Y(i).GetCentralTendency, 0))
                        Next
                        GetCurves(1) = New AutoGenerate.Curve(xs, ys, validation)
                        ys.Clear()
                        For i = 0 To CType(curve, Statistics.MonotonicCurveUSingle).Y.Count - 1
                            ys.Add(New Statistics.Triangular(0, CType(curve, Statistics.MonotonicCurveUSingle).Y(i).GetCentralTendency, CType(curve, Statistics.MonotonicCurveUSingle).Y(i).GetCentralTendency))
                        Next
                        GetCurves(2) = New AutoGenerate.Curve(xs, ys, validation)
                        ys.Clear()
                        For i = 0 To CType(curve, Statistics.MonotonicCurveUSingle).Y.Count - 1
                            ys.Add(New Statistics.Uniform(CType(curve, Statistics.MonotonicCurveUSingle).Y(i).GetCentralTendency, CType(curve, Statistics.MonotonicCurveUSingle).Y(i).GetCentralTendency))
                        Next
                        GetCurves(3) = New AutoGenerate.Curve(xs, ys, validation)
                    Case GetType(Statistics.Normal)
                        GetCurves(1) = New AutoGenerate.Curve(curve, validation)
                        SelectedIndex = 1
                        xs.Clear()
                        ys.Clear()
                        For i = 0 To CType(curve, Statistics.MonotonicCurveUSingle).Y.Count - 1
                            xs.Add(CType(curve, Statistics.MonotonicCurveUSingle).X(i))
                            ys.Add(New Statistics.None(CType(curve, Statistics.MonotonicCurveUSingle).Y(i).GetCentralTendency))
                        Next
                        GetCurves(0) = New AutoGenerate.Curve(xs, ys, validation)
                        ys.Clear()
                        For i = 0 To CType(curve, Statistics.MonotonicCurveUSingle).Y.Count - 1
                            ys.Add(New Statistics.Triangular(0, CType(curve, Statistics.MonotonicCurveUSingle).Y(i).GetCentralTendency, CType(curve, Statistics.MonotonicCurveUSingle).Y(i).GetCentralTendency))
                        Next
                        GetCurves(2) = New AutoGenerate.Curve(xs, ys, validation)
                        ys.Clear()
                        For i = 0 To CType(curve, Statistics.MonotonicCurveUSingle).Y.Count - 1
                            ys.Add(New Statistics.Uniform(CType(curve, Statistics.MonotonicCurveUSingle).Y(i).GetCentralTendency, CType(curve, Statistics.MonotonicCurveUSingle).Y(i).GetCentralTendency))
                        Next
                        GetCurves(3) = New AutoGenerate.Curve(xs, ys, validation)
                    Case GetType(Statistics.Triangular)
                        GetCurves(2) = New AutoGenerate.Curve(curve, validation)
                        SelectedIndex = 2
                        xs.Clear()
                        ys.Clear()
                        For i = 0 To CType(curve, Statistics.MonotonicCurveUSingle).Y.Count - 1
                            xs.Add(CType(curve, Statistics.MonotonicCurveUSingle).X(i))
                            ys.Add(New Statistics.Normal(CType(curve, Statistics.MonotonicCurveUSingle).Y(i).GetCentralTendency, 0))
                        Next
                        GetCurves(1) = New AutoGenerate.Curve(xs, ys, validation)
                        ys.Clear()
                        For i = 0 To CType(curve, Statistics.MonotonicCurveUSingle).Y.Count - 1
                            ys.Add(New Statistics.None(CType(curve, Statistics.MonotonicCurveUSingle).Y(i).GetCentralTendency))
                        Next
                        GetCurves(0) = New AutoGenerate.Curve(xs, ys, validation)
                        ys.Clear()
                        For i = 0 To CType(curve, Statistics.MonotonicCurveUSingle).Y.Count - 1
                            ys.Add(New Statistics.Uniform(CType(curve, Statistics.MonotonicCurveUSingle).Y(i).GetCentralTendency, CType(curve, Statistics.MonotonicCurveUSingle).Y(i).GetCentralTendency))
                        Next
                        GetCurves(3) = New AutoGenerate.Curve(xs, ys, validation)
                    Case GetType(Statistics.Uniform)
                        GetCurves(3) = New AutoGenerate.Curve(curve, validation)
                        SelectedIndex = 3
                        xs.Clear()
                        ys.Clear()
                        For i = 0 To CType(curve, Statistics.MonotonicCurveUSingle).Y.Count - 1
                            xs.Add(CType(curve, Statistics.MonotonicCurveUSingle).X(i))
                            ys.Add(New Statistics.Normal(CType(curve, Statistics.MonotonicCurveUSingle).Y(i).GetCentralTendency, 0))
                        Next
                        GetCurves(1) = New AutoGenerate.Curve(xs, ys, validation)
                        ys.Clear()
                        For i = 0 To CType(curve, Statistics.MonotonicCurveUSingle).Y.Count - 1
                            ys.Add(New Statistics.Triangular(0, CType(curve, Statistics.MonotonicCurveUSingle).Y(i).GetCentralTendency, CType(curve, Statistics.MonotonicCurveUSingle).Y(i).GetCentralTendency))
                        Next
                        GetCurves(2) = New AutoGenerate.Curve(xs, ys, validation)
                        ys.Clear()
                        For i = 0 To CType(curve, Statistics.MonotonicCurveUSingle).Y.Count - 1
                            ys.Add(New Statistics.None(CType(curve, Statistics.MonotonicCurveUSingle).Y(i).GetCentralTendency))
                        Next
                        GetCurves(0) = New AutoGenerate.Curve(xs, ys, validation)
                    Case Else
                        'abort
                End Select
            Case Else
                'abort
        End Select
    End Sub
    Public Sub AddRows(ByVal insertatIndex As Integer, ByVal number As Integer)
        GetCurves(SelectedIndex).AddRows(insertatIndex, number)
    End Sub
    Public Sub RemoveRows(ByVal indices As List(Of Int32))
        GetCurves(SelectedIndex).RemoveRows(indices)
    End Sub
    Public Sub SetUpPlotmodel()
        Dim c As New OxyPlot.PlotModel
        c = New OxyPlot.PlotModel
        c.Title = ChartTitle
        Dim YAxis As OxyPlot.Axes.LinearAxis = New OxyPlot.Axes.LinearAxis
        YAxis.Position = OxyPlot.Axes.AxisPosition.Left
        YAxis.Title = YValueTitle
        YAxis.MajorGridlineStyle = OxyPlot.LineStyle.Solid
        YAxis.MinorGridlineStyle = OxyPlot.LineStyle.Dash
        c.Axes.Add(YAxis)
        Dim XAxis As OxyPlot.Axes.LinearAxis = New OxyPlot.Axes.LinearAxis
        XAxis.Position = OxyPlot.Axes.AxisPosition.Bottom
        XAxis.Title = XValueTitle
        XAxis.MajorGridlineStyle = OxyPlot.LineStyle.Solid
        XAxis.MinorGridlineStyle = OxyPlot.LineStyle.Dash
        c.Axes.Add(XAxis)
        c.LegendBackground = OxyPlot.OxyColors.White
        c.LegendBorder = OxyPlot.OxyColors.DarkGray
        c.LegendPosition = OxyPlot.LegendPosition.BottomRight
        CurvePlotModel = c
    End Sub
    Public Sub PlotGrid()
        If IsNothing(_CurvePlotModel) Then _CurvePlotModel = New OxyPlot.PlotModel
        _CurvePlotModel.Series.Clear()
        Dim hlist As List(Of String) = CType(GetCurves(SelectedIndex).Curve(0), AutoGenerate.CurveRowItem).GetPlotNames
        Dim colorstack As New Stack
        colorstack.Push(OxyPlot.OxyColors.Blue)
        colorstack.Push(OxyPlot.OxyColors.Red)
        For i = 0 To hlist.Count - 1
            Dim CentralSeries As New OxyPlot.Series.LineSeries()
            CentralSeries.Title = hlist(i)
            CentralSeries.MarkerType = OxyPlot.MarkerType.Circle
            If hlist(i) = "Y" Then
                CentralSeries.Color = OxyPlot.OxyColors.Black
            Else
                If colorstack.Count > 0 Then
                    CentralSeries.Color = colorstack.Pop
                Else
                    CentralSeries.Color = OxyPlot.OxyColors.Black
                End If
            End If
            _CurvePlotModel.Series.Add(CentralSeries)
            For j = 0 To GetCurves(SelectedIndex).Curve.Count - 1
                CentralSeries.Points.Add(New OxyPlot.DataPoint(CType(GetCurves(SelectedIndex).Curve(j), AutoGenerate.CurveRowItem).GetPlotData(0), CType(GetCurves(SelectedIndex).Curve(j), AutoGenerate.CurveRowItem).GetPlotData(i + 1)))
            Next
        Next
        _CurvePlotModel.InvalidatePlot(True)
    End Sub

End Class
