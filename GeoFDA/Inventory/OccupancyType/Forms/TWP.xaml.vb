Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Windows

Public Class TWP
    Implements System.ComponentModel.INotifyPropertyChanged
    Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Private _vm As AutoGenerateVM = New AutoGenerateVM
    Private _validation As AutoGenerate.ValidationTypes = AutoGenerate.ValidationTypes.GenericCurveSingle
    Private _ManualCommitEdit As Boolean = False
    Private _IsEditing As Boolean = False
    Private _isValidating As Boolean = False
    Private _isloadingNewCurve As Boolean = False
    'Private _scrollbar As Primitives.ScrollBar
    Private _Scrollviewer As ScrollViewer
    Private _Minvalue As Single = Single.MinValue
    Private _Maxvalue As Single = Single.MaxValue
    Public Property ChartTitle As String = "Monotonic Curve"
    Public Property YValueTitle As String = "Y Values"
    Public Property XValueTitle As String = "X Values"
    Public Property XModifier As String = ""
    Public Property YModifier As String = ""
    Public Property ViewModel As AutoGenerateVM
        Get
            Return _vm
        End Get
        Set(value As AutoGenerateVM)
            _vm = value
            NotifyPropertyChanged("ViewModel")
        End Set
    End Property
    Public Property IsReadonly As Boolean
        Get
            Return Not _vm.IsNotReadonly
        End Get
        Set(value As Boolean)
            ViewModel.IsNotReadonly = Not value
            Me.IsEnabled = Not value
        End Set
    End Property
    Public Property GenericCurveSingle As Boolean
        Get
            If _validation = AutoGenerate.ValidationTypes.GenericCurveSingle Then Return True
            Return False
        End Get
        Set(value As Boolean)
            If value Then
                _validation = AutoGenerate.ValidationTypes.GenericCurveSingle
            End If
        End Set
    End Property
    Public Property GenericCurveUncertain As Boolean
        Get
            If _validation = AutoGenerate.ValidationTypes.GenericCurveUncertian Then Return True
            Return False
        End Get
        Set(value As Boolean)
            If value Then
                _validation = AutoGenerate.ValidationTypes.GenericCurveUncertian
            End If
        End Set
    End Property
    Public Property MonotonicCurveIncreasing As Boolean
        Get
            If _validation = AutoGenerate.ValidationTypes.MonotonicIncreasing Then Return True
            Return False
        End Get
        Set(value As Boolean)
            If value Then
                _validation = AutoGenerate.ValidationTypes.MonotonicIncreasing
            End If
        End Set
    End Property
    Public Property MonotonicCurveDecreasing As Boolean
        Get
            If _validation = AutoGenerate.ValidationTypes.MonotonicDecreasing Then Return True
            Return False
        End Get
        Set(value As Boolean)
            If value Then
                _validation = AutoGenerate.ValidationTypes.MonotonicDecreasing
            End If
        End Set
    End Property
    Public Property MonotonicIncreasingUncertain As Boolean
        Get
            If _validation = AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain Then Return True
            Return False
        End Get
        Set(value As Boolean)
            If value Then
                _validation = AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain
            End If
        End Set
    End Property
    Public Property YMinValue As Single
        Get
            Return _Minvalue
        End Get
        Set(value As Single)
            _Minvalue = value
        End Set
    End Property
    Public Property YMaxValue As Single
        Get
            Return _Maxvalue
        End Get
        Set(value As Single)
            _Maxvalue = value
        End Set
    End Property
    Sub New()
        _vm.SetUpPlotmodel()
        InitializeComponent()

    End Sub

    Sub LoadCurveAndValidation(ByVal c As Statistics.PairedData, Optional validation As AutoGenerate.ValidationTypes = AutoGenerate.ValidationTypes.GenericCurveSingle)
        ViewModel = New AutoGenerateVM(c, validation)
        ViewModel.IsNotReadonly = Not IsReadonly
		Me.IsEnabled = Not IsReadonly
		ViewModel.ChartTitle = ChartTitle
        ViewModel.YValueTitle = YValueTitle
        ViewModel.XValueTitle = XValueTitle
        ViewModel.SetUpPlotmodel()
        ViewModel.PlotGrid()
        _validation = validation
        _Scrollviewer = AutoCurveDataGrid.GetTheVisualChild(Of ScrollViewer)(AutoCurveDataGrid)
        'If IsNothing(_Scrollviewer) Then
        'Else
        '    _scrollbar = CType(_Scrollviewer.Template.FindName("PART_VerticalScrollBar", _Scrollviewer), Primitives.ScrollBar)
        'End If
        'colorcells(_vm.GetCurves(_vm.SelectedIndex).Validate(YMinValue, YMaxValue))
    End Sub
    Private Sub MainWindow_Loaded(sender As Object, e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        AddHandler AutoCurveDataGrid.DataPasted, AddressOf Pasted
        AddHandler AutoCurveDataGrid.PreviewPasteData, AddressOf StopValidation
        AddHandler AutoCurveDataGrid.RowsAdded, AddressOf AddRows
        AddHandler AutoCurveDataGrid.RowsDeleted, AddressOf RemoveRows
        _Scrollviewer = AutoCurveDataGrid.GetTheVisualChild(Of ScrollViewer)(AutoCurveDataGrid)
        'If IsNothing(_Scrollviewer) Then
        'Else
        '    _scrollbar = CType(_Scrollviewer.Template.FindName("PART_VerticalScrollBar", _Scrollviewer), Primitives.ScrollBar)
        'End If
    End Sub
    Private Sub TextBox_TextChanged(sender As Object, e As System.Windows.Input.TextCompositionEventArgs)
        Dim TBox As TextBox = CType(sender, TextBox)
        Dim SingleValue As Single
        If Single.TryParse(TBox.Text & e.Text, SingleValue) = False Then
            If e.Text = "-" AndAlso TBox.CaretIndex = 0 Then Exit Sub 'allow negative
            If e.Text = "." AndAlso TBox.Text.Contains(".") = False Then Exit Sub 'allow one decimal
            e.Handled = True
        Else
            'If e.Text = "," Then e.Handled = True
        End If
    End Sub
    Private Sub colorcells(ByVal errors As List(Of AutoGenerate.CellErrorReport))
        _isValidating = True

        'If _IsEditing = False Then
        '    AutoCurveDataGrid.Items.Refresh()
        'End If
        AutoCurveDataGrid.UpdateLayout()
        Dim VertOffset As Double = 0
        Dim LastRow As Double = 0
        If IsNothing(_Scrollviewer) Then
            _Scrollviewer = AutoCurveDataGrid.GetTheVisualChild(Of ScrollViewer)(AutoCurveDataGrid)
            Exit Sub
        End If
        VertOffset = _Scrollviewer.VerticalOffset
        LastRow = VertOffset + _vm.GetCurves(_vm.SelectedIndex).Curve.Count - _Scrollviewer.ScrollableHeight

        Dim StartRowIndex As Integer = CInt(VertOffset)
        Dim EndRowIndex As Integer = CInt(LastRow)
        Dim Row As DataGridRow
        Dim presenter As System.Windows.Controls.Primitives.DataGridCellsPresenter
        Dim Cell As DataGridCell

        For i As Int32 = 0 To _vm.GetCurves(_vm.SelectedIndex).Curve.Count - 1
            Row = DirectCast(AutoCurveDataGrid.ItemContainerGenerator.ContainerFromIndex(i), DataGridRow)
            If IsNothing(Row) Then Continue For
            presenter = AutoCurveDataGrid.GetTheVisualChild(Of System.Windows.Controls.Primitives.DataGridCellsPresenter)(Row)
            If IsNothing(presenter) Then Continue For
            For j As Int32 = 0 To AutoCurveDataGrid.Columns.Count - 1
                Cell = DirectCast(presenter.ItemContainerGenerator.ContainerFromIndex(j), DataGridCell)

                '
                If IsNothing(Cell) Then Continue For
                Cell.BorderBrush = Nothing
                Cell.BorderThickness = Nothing
                Cell.Margin = Nothing
                Cell.ToolTip = Nothing
            Next
            '
            Row.HeaderStyle = Nothing
        Next


        If IsNothing(errors) Then _isValidating = False : Exit Sub
        For i = 0 To errors.Count - 1
            If Not IsNothing(errors(i)) Then
                If errors(i).IsValid Then
                Else
                    If errors(i).rownumber < StartRowIndex OrElse errors(i).rownumber > EndRowIndex Then Continue For

                    Row = DirectCast(AutoCurveDataGrid.ItemContainerGenerator.ContainerFromIndex(errors(i).rownumber), DataGridRow)
                    If IsNothing(Row) Then Continue For
                    presenter = AutoCurveDataGrid.GetTheVisualChild(Of System.Windows.Controls.Primitives.DataGridCellsPresenter)(Row)
                    If IsNothing(presenter) Then Continue For
                    Cell = DirectCast(presenter.ItemContainerGenerator.ContainerFromIndex(errors(i).ColumnNumber), DataGridCell)
                    '
                    If IsNothing(Cell) Then Continue For

                    Cell.BorderBrush = Brushes.Red
                    Cell.BorderThickness = New Thickness(2)
                    Cell.Margin = New Thickness(0, -1, 0, -1)

                    If IsNothing(Cell.ToolTip) OrElse Cell.ToolTip = "" Then
                        ToolTipService.SetShowDuration(Cell, 10000)
                        Cell.ToolTip = errors(i).tooltipMessage & " on row " & errors(i).rownumber
                    Else
                        Cell.ToolTip = Cell.ToolTip & vbNewLine & errors(i).tooltipMessage & " on row " & errors(i).rownumber
                    End If
                    Dim style = New Style(GetType(System.Windows.Controls.Primitives.DataGridRowHeader))
                    style.Setters.Add(New Setter(ToolTipService.ToolTipProperty, "Row contains errors"))
                    style.Setters.Add(New Setter() With {.Property = BorderBrushProperty, .Value = Brushes.DarkGoldenrod})
                    style.Setters.Add(New Setter() With {.Property = BorderThicknessProperty, .Value = New Thickness(1)})
                    style.Setters.Add(New Setter() With {.Property = ForegroundProperty, .Value = Brushes.Red})
                    style.Setters.Add(New Setter() With {.Property = FontWeightProperty, .Value = FontWeights.Bold})
                    style.Setters.Add(New Setter() With {.Property = ContentProperty, .Value = " !"})
                    style.Setters.Add(New Setter() With {.Property = HorizontalContentAlignmentProperty, .Value = HorizontalAlignment.Center})

                    Row.HeaderStyle = style
                End If
            End If
        Next
        'AutoCurveDataGrid.GetCell(CInt(VertOffset), 0)
        _isValidating = False
    End Sub
    Sub AddRows(ByVal insertat As Integer, ByVal number As Integer)
        _vm.AddRows(insertat, number)
        colorcells(_vm.GetCurves(_vm.SelectedIndex).Validate(YMinValue, YMaxValue))
        _vm.PlotGrid()
    End Sub
    Sub RemoveRows(ByVal indices As List(Of Int32))
        _vm.RemoveRows(indices)
        colorcells(_vm.GetCurves(_vm.SelectedIndex).Validate(YMinValue, YMaxValue))
        _vm.PlotGrid()
    End Sub
    Sub StopValidation()
        _vm.Validate = False
    End Sub
    Sub Pasted()
        _vm.Validate = True
        colorcells(_vm.GetCurves(_vm.SelectedIndex).Validate(YMinValue, YMaxValue))
        _vm.PlotGrid()
    End Sub
    Private Sub UpdateColumnNames(sender As Object, e As DataGridAutoGeneratingColumnEventArgs) Handles AutoCurveDataGrid.AutoGeneratingColumn
        If e.PropertyType = GetType(String) Then
            Dim dg As DataGrid = TryCast(sender, DataGrid)
            Dim c As AutoGenerateVM = TryCast(dg.DataContext, TWP).ViewModel
            Dim cr As AutoGenerate.CurveUncertainRowItem = TryCast(dg.Items(0), AutoGenerate.CurveUncertainRowItem)
            If IsNothing(c) OrElse IsNothing(c.GetCurves(c.SelectedIndex).Headers) OrElse c.GetCurves(c.SelectedIndex).Headers.Count = 0 Then
                If e.PropertyName.Contains("_") Then e.Column.Header = e.PropertyName.Replace("_", " ")
                If e.Column.Header.Equals("X") Then
                    If Not XModifier.Equals("") Then e.Column.Header = XModifier
                ElseIf e.Column.Header.Equals("Y") Then
                    If Not YModifier.Equals("") Then e.Column.Header = YModifier
                Else
                    If Not YModifier.Equals("") Then e.Column.Header &= " " & YModifier
                End If

            Else
                e.Column.Header = c.GetCurves(c.SelectedIndex).Headers(dg.Columns.Count)
            End If
            e.Column.Width = New DataGridLength(1, DataGridLengthUnitType.Star)
            e.Column.HeaderStyle = Me.Resources("WrappedColumnHeaderStyle")
            TryCast(e.Column, DataGridTextColumn).EditingElementStyle = Me.Resources("ColumnEditStyle")
            Debug.Print("Autogenerating coloumn")
        Else
            e.Cancel = True
        End If
    End Sub
    Public Function ExportToPairedData(ByRef messageout As String) As Statistics.PairedData
        Select Case _validation
            Case AutoGenerate.ValidationTypes.GenericCurveSingle
                Dim tmprowitem As AutoGenerate.CurveSingleRowItem
                Dim xs As New List(Of Single)
                Dim ys As New List(Of Single)
                For i = 0 To _vm.SelectedItem.Curve.Count - 1
                    tmprowitem = CType(_vm.SelectedItem.Curve(i), AutoGenerate.CurveSingleRowItem)
                    xs.Add(tmprowitem.GetXvalue)
                    ys.Add(tmprowitem.GetYValue)
                Next
                Dim c As New Statistics.GenericCurveSingle(xs.ToArray, ys.ToArray)
                Dim messages As Statistics.ErrorReport = c.Verify
                If messages.Errors.Count > 0 Then
                    For i = 0 To messages.Errors.Count - 1
                        messageout = messageout & vbNewLine & messages.Errors(i).GetMessage
                    Next
                    Return Nothing
                Else
                    Return c
                End If
            Case AutoGenerate.ValidationTypes.GenericCurveUncertian
                Dim tmprowitem As AutoGenerate.CurveUncertainRowItem
                Dim xs As New List(Of Single)
                Dim ys As New List(Of Statistics.ContinuousDistribution)
                For i = 0 To _vm.SelectedItem.Curve.Count - 1
                    tmprowitem = _vm.SelectedItem.Curve(i)
                    xs.Add(tmprowitem.GetXvalue)
                    ys.Add(tmprowitem.GetDistribution)
                Next
                Dim c As New Statistics.GenericCurveSingleUncertain(xs, ys)
                Dim messages As Statistics.ErrorReport = c.Verify
                If messages.Errors.Count > 0 Then
                    For i = 0 To messages.Errors.Count - 1
                        messageout = messageout & vbNewLine & messages.Errors(i).GetMessage
                    Next
                    Return Nothing
                Else
                    Return c
                End If
            Case AutoGenerate.ValidationTypes.MonotonicDecreasing
                Dim tmprowitem As AutoGenerate.CurveSingleRowItem
                Dim xs As New List(Of Single)
                Dim ys As New List(Of Single)
                For i = 0 To _vm.SelectedItem.Curve.Count - 1
                    tmprowitem = CType(_vm.SelectedItem.Curve(i), AutoGenerate.CurveSingleRowItem)
                    xs.Add(tmprowitem.GetXvalue)
                    ys.Add(tmprowitem.GetYValue)
                Next
                Dim c As New Statistics.MonotonicCurveDecreasing(xs.ToArray, ys.ToArray)
                Dim messages As Statistics.ErrorReport = c.Verify
                If messages.Errors.Count > 0 Then
                    For i = 0 To messages.Errors.Count - 1
                        messageout = messageout & vbNewLine & messages.Errors(i).GetMessage
                    Next
                    Return Nothing
                Else
                    Return c
                End If
            Case AutoGenerate.ValidationTypes.MonotonicIncreasing
                Dim tmprowitem As AutoGenerate.CurveSingleRowItem
                Dim xs As New List(Of Single)
                Dim ys As New List(Of Single)
                For i = 0 To _vm.SelectedItem.Curve.Count - 1
                    tmprowitem = CType(_vm.SelectedItem.Curve(i), AutoGenerate.CurveSingleRowItem)
                    xs.Add(tmprowitem.GetXvalue)
                    ys.Add(tmprowitem.GetYValue)
                Next
                Dim c As New Statistics.MonotonicCurveIncreasing(xs.ToArray, ys.ToArray)
                Dim messages As Statistics.ErrorReport = c.Verify
                If messages.Errors.Count > 0 Then
                    For i = 0 To messages.Errors.Count - 1
                        messageout = messageout & vbNewLine & messages.Errors(i).GetMessage
                    Next
                    Return Nothing
                Else
                    Return c
                End If
            Case AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain
                Dim tmprowitem As AutoGenerate.CurveUncertainRowItem
                Dim xs As New List(Of Single)
                Dim ys As New List(Of Statistics.ContinuousDistribution)
                For i = 0 To _vm.SelectedItem.Curve.Count - 1
                    tmprowitem = _vm.SelectedItem.Curve(i)
                    xs.Add(tmprowitem.GetXvalue)
                    ys.Add(tmprowitem.GetDistribution)
                Next
                Dim c As New Statistics.MonotonicCurveUSingle(xs.ToArray, ys.ToArray)
                Dim messages As Statistics.ErrorReport = c.Verify
                If messages.Errors.Count > 0 Then
                    For i = 0 To messages.Errors.Count - 1
                        messageout = messageout & vbNewLine & messages.Errors(i).GetMessage
                    Next
                    Return Nothing
                Else
                    Return c
                End If
            Case Else
                messageout = "No validation was set, unsure what type of curve to export..."
                Return Nothing
        End Select
    End Function
    Public Function ExportToPairedData() As Statistics.PairedData
        Select Case _validation
            Case AutoGenerate.ValidationTypes.GenericCurveSingle
                Dim tmprowitem As AutoGenerate.CurveSingleRowItem
                Dim xs As New List(Of Single)
                Dim ys As New List(Of Single)
                For i = 0 To _vm.SelectedItem.Curve.Count - 1
                    tmprowitem = CType(_vm.SelectedItem.Curve(i), AutoGenerate.CurveSingleRowItem)
                    xs.Add(tmprowitem.GetXvalue)
                    ys.Add(tmprowitem.GetYValue)
                Next
                Dim c As New Statistics.GenericCurveSingle(xs.ToArray, ys.ToArray)
                Return c
            Case AutoGenerate.ValidationTypes.GenericCurveUncertian
                Dim tmprowitem As AutoGenerate.CurveUncertainRowItem
                Dim xs As New List(Of Single)
                Dim ys As New List(Of Statistics.ContinuousDistribution)
                For i = 0 To _vm.SelectedItem.Curve.Count - 1
                    tmprowitem = _vm.SelectedItem.Curve(i)
                    xs.Add(tmprowitem.GetXvalue)
                    ys.Add(tmprowitem.GetDistribution)
                Next
                Dim c As New Statistics.GenericCurveSingleUncertain(xs, ys)
                Return c
            Case AutoGenerate.ValidationTypes.MonotonicDecreasing
                Dim tmprowitem As AutoGenerate.CurveSingleRowItem
                Dim xs As New List(Of Single)
                Dim ys As New List(Of Single)
                For i = 0 To _vm.SelectedItem.Curve.Count - 1
                    tmprowitem = CType(_vm.SelectedItem.Curve(i), AutoGenerate.CurveSingleRowItem)
                    xs.Add(tmprowitem.GetXvalue)
                    ys.Add(tmprowitem.GetYValue)
                Next
                Dim c As New Statistics.MonotonicCurveDecreasing(xs.ToArray, ys.ToArray)
                Return c
            Case AutoGenerate.ValidationTypes.MonotonicIncreasing
                Dim tmprowitem As AutoGenerate.CurveSingleRowItem
                Dim xs As New List(Of Single)
                Dim ys As New List(Of Single)
                For i = 0 To _vm.SelectedItem.Curve.Count - 1
                    tmprowitem = CType(_vm.SelectedItem.Curve(i), AutoGenerate.CurveSingleRowItem)
                    xs.Add(tmprowitem.GetXvalue)
                    ys.Add(tmprowitem.GetYValue)
                Next
                Dim c As New Statistics.MonotonicCurveIncreasing(xs.ToArray, ys.ToArray)
                Return c
            Case AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain
                Dim tmprowitem As AutoGenerate.CurveUncertainRowItem
                Dim xs As New List(Of Single)
                Dim ys As New List(Of Statistics.ContinuousDistribution)
                For i = 0 To _vm.SelectedItem.Curve.Count - 1
                    tmprowitem = _vm.SelectedItem.Curve(i)
                    xs.Add(tmprowitem.GetXvalue)
                    ys.Add(tmprowitem.GetDistribution)
                Next
                Dim c As New Statistics.MonotonicCurveUSingle(xs.ToArray, ys.ToArray)
                Return c
            Case Else
                Return Nothing
        End Select
    End Function
    Private Sub NotifyPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(info))
    End Sub
    Private Sub AutoCurveDataGrid_BeginningEdit(sender As Object, e As DataGridBeginningEditEventArgs) Handles AutoCurveDataGrid.BeginningEdit
        _IsEditing = True
    End Sub
    Private Sub AutoCurveDataGrid_CellEditEnding(sender As Object, e As DataGridCellEditEndingEventArgs) Handles AutoCurveDataGrid.CellEditEnding
        If Not _ManualCommitEdit Then
            _ManualCommitEdit = True
            AutoCurveDataGrid.CommitEdit(DataGridEditingUnit.Row, True)
            _ManualCommitEdit = False
            AutoCurveDataGrid.Items.Refresh()
            colorcells(_vm.SelectedItem.Validate(YMinValue, YMaxValue))
            _vm.PlotGrid()
        End If
        _IsEditing = False

    End Sub
    Private Sub DistributionTypeSelection_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles DistributionTypeSelection.SelectionChanged
        If IsLoaded Then
            _isloadingNewCurve = True
            AutoCurveDataGrid.Items.Refresh()
            If AutoCurveDataGrid.Items.Count = 0 Then _isloadingNewCurve = False : Exit Sub
            colorcells(_vm.SelectedItem.Validate(YMinValue, YMaxValue))
            _vm.PlotGrid()
            _isloadingNewCurve = False
        End If

    End Sub

    Private Sub AutoCurveDataGrid_ScrollChanged(sender As Object, e As ScrollChangedEventArgs)
        If Not IsLoaded Then Exit Sub
        If _isValidating Then Exit Sub
        'If _IsEditing Then Exit Sub
        If _isloadingNewCurve Then Exit Sub
        colorcells(_vm.SelectedItem.Validate(YMinValue, YMaxValue))
    End Sub
End Class
