Public Class BasicImpactAreaImporter
    Implements System.ComponentModel.INotifyPropertyChanged
    Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Private _InputImpactAreaPath As String
    Private _UniqueNameField As String
    Private _ImpactAreaAlias As String
    Private _ImpactAreaVM As NameVM
    Private _isvalidating As Boolean = False
    Private _isediting As Boolean = False
    Private _scrollbar As Primitives.ScrollBar
    Private _scrollviewer As ScrollViewer
    Private _manualcommitedit As Boolean = False
    Public ReadOnly Property GetImpactAreaPath As String
        Get
            Return _InputImpactAreaPath
        End Get
    End Property
    Public ReadOnly Property GetImpactAreaName As String
        Get
            Return _ImpactAreaAlias
        End Get
    End Property
    Public ReadOnly Property GetUniqueNameField As String
        Get
            Return _UniqueNameField
        End Get
    End Property
    Public ReadOnly Property GetImpactAreaNames As List(Of String)
        Get
            Return _ImpactAreaVM.GetImpactAreaNames
        End Get
    End Property
    Public ReadOnly Property GetPaddedImpactAreaNames As List(Of String)
        Get
            Return _ImpactAreaVM.GetPaddedImpactAreaNames
        End Get
    End Property
    Public ReadOnly Property GetImpactAreaIndexes As List(Of Double)
        Get
            Return _ImpactAreaVM.GetImpactAreaIndexes
        End Get
    End Property
    Public Property NameList As NameVM
        Get
            Return _ImpactAreaVM
        End Get
        Set(value As NameVM)
            _ImpactAreaVM = value
            NotifyPropertyChanged("NameList")
        End Set
    End Property
    Sub New(ByVal listofpolygonpaths As List(Of String))
        InitializeComponent()
        Dim validshapetypes As New List(Of LifeSimGIS.ShapefileReader.ShapeTypeEnumerable)
        validshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.Polygon)
        validshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PolygonM)
        validshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PolygonZM)
        PolygonBrowser.ValidShapeTypes = validshapetypes
        For i = 0 To listofpolygonpaths.Count - 1
            PolygonBrowser.AddItem(listofpolygonpaths(i))
        Next
    End Sub
    Private Sub CMDOk_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles CMDOk.Click

        'do vaidation here.
        If _InputImpactAreaPath = "" Then MsgBox("You have not defined a path to a Impact Area shapefile") : Exit Sub
        If _UniqueNameField = "" Then MsgBox("You have not defined a unique name field from the input shapefile") : Exit Sub
        If _ImpactAreaAlias = "" Then MsgBox("You have not given an alias name for this Impact Area set") : Exit Sub
        If _ImpactAreaVM.Items.Count = 0 Then MsgBox("There are no impact areas defined.") : Exit Sub
        For i = 0 To _ImpactAreaVM.Items.Count - 1
            Dim s As List(Of ImpactAreaError) = _ImpactAreaVM.Items(i).Validate
            If IsNothing(s) OrElse s.Count = 0 Then
            Else
                Dim errors As String = ""
                For j = 0 To s.Count - 1
                    errors = errors & vbNewLine & s(j).Message
                Next
                MsgBox("The impact area " & _ImpactAreaVM.Items(i).Name & " had the following error:" & vbNewLine & errors) : Exit Sub
            End If
        Next
        For Each badChar As Char In System.IO.Path.GetInvalidFileNameChars
            If _ImpactAreaAlias.Contains(badChar) Then MsgBox("Invalid character in file name.") : Exit Sub
        Next
        DialogResult = True
        Me.Close()
    End Sub
    Private Sub Close_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles CMDClose.Click
        Me.Close()
    End Sub

    Private Sub PolygonBrowser_SelectionMade(ByVal fullpath As String) Handles PolygonBrowser.CmbSelectionMade
        Dim dbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(fullpath, ".dbf"))
        _InputImpactAreaPath = fullpath
        UniqueName.Items.Clear()
        For i = 0 To dbf.ColumnNames.Count - 1
            UniqueName.Items.Add(dbf.ColumnNames(i))
        Next
    End Sub
    Private Sub TxtStreamName_TextChanged(sender As System.Object, e As System.Windows.Controls.TextChangedEventArgs) Handles TxtImpactAreaSetName.TextChanged
        _ImpactAreaAlias = TxtImpactAreaSetName.Text
    End Sub
    Private Sub UniqueName_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles UniqueName.SelectionChanged
        If IsLoaded Then
            If UniqueName.SelectedIndex = -1 Then
            Else
                If _InputImpactAreaPath = "" Then MsgBox("You have not selected a valid shapefile path") : Exit Sub
                Dim dbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(_InputImpactAreaPath, ".dbf"))
                Dim tmpvm As New NameVM
                For i = 0 To dbf.NumberOfRows - 1
                    tmpvm.Items.Add(New NameClass(dbf.GetCell(UniqueName.SelectedIndex, i), 0))
                Next
                _UniqueNameField = UniqueName.SelectedItem
                NameList = tmpvm
            End If
        End If

    End Sub
    Public Function GetRow(index As Integer) As DataGridRow
        Dim row As DataGridRow = DirectCast(ImpactAreas.ItemContainerGenerator.ContainerFromIndex(index), DataGridRow)
        If row Is Nothing Then
            ' May be virtualized, bring into view and try again.
            ImpactAreas.UpdateLayout()
            ImpactAreas.ScrollIntoView(ImpactAreas.Items(index))
            row = DirectCast(ImpactAreas.ItemContainerGenerator.ContainerFromIndex(index), DataGridRow)
        End If
        Return row
    End Function
    Public Function GetCell(rowindex As Integer, column As Integer) As DataGridCell
        Dim row As DataGridRow = GetRow(rowindex)
        If row IsNot Nothing Then
            Dim presenter As Primitives.DataGridCellsPresenter = GetTheVisualChild(Of Primitives.DataGridCellsPresenter)(row)
            If presenter Is Nothing Then
                ImpactAreas.ScrollIntoView(row, ImpactAreas.Columns(column))
                presenter = GetTheVisualChild(Of Primitives.DataGridCellsPresenter)(row)
            End If
            If presenter Is Nothing Then Return Nothing
            Dim cell As DataGridCell = DirectCast(presenter.ItemContainerGenerator.ContainerFromIndex(column), DataGridCell)
            Return cell
        End If
        Return Nothing
    End Function
    Public Function GetTheVisualChild(Of T As Visual)(parent As Visual) As T
        Dim child As T = Nothing
        Dim numVisuals As Integer = VisualTreeHelper.GetChildrenCount(parent)
        For i As Integer = 0 To numVisuals - 1
            Dim v As Visual = DirectCast(VisualTreeHelper.GetChild(parent, i), Visual)
            child = TryCast(v, T)
            If child Is Nothing Then
                child = GetTheVisualChild(Of T)(v)
            End If
            If child IsNot Nothing Then
                Exit For
            End If
        Next
        Return child
    End Function
    Private Sub StreamDGV_AutoGeneratingColumn(sender As Object, e As DataGridAutoGeneratingColumnEventArgs) Handles ImpactAreas.AutoGeneratingColumn
        If e.PropertyType = GetType(String) Then
            e.Column.Width = New DataGridLength(1, DataGridLengthUnitType.Star)
        ElseIf e.PropertyType = GetType(Double) Then
            e.Column.Header = e.PropertyName.Replace("_", " ")
            e.Column.Width = New DataGridLength(1, DataGridLengthUnitType.Star)
        Else
            e.Cancel = True
        End If
    End Sub
    Private Sub colorcells(ByVal errors As List(Of ImpactAreaError))
        _isvalidating = True
        If IsNothing(errors) Then _isvalidating = False : Exit Sub
        If _isediting = False Then
            ImpactAreas.Items.Refresh()
        End If

        Dim VertOffset As Double
        If IsNothing(_scrollviewer) Then
            VertOffset = 0
            _scrollviewer = GetTheVisualChild(Of ScrollViewer)(ImpactAreas)
        Else
            VertOffset = _scrollviewer.VerticalOffset
        End If

        Dim LastRow As Double
        If IsNothing(_scrollbar) Then
            LastRow = _ImpactAreaVM.Items.Count - 1
            If IsNothing(_scrollviewer) Then
            Else
                _scrollbar = CType(_scrollviewer.Template.FindName("PART_VerticalScrollBar", _scrollviewer), Primitives.ScrollBar)
            End If
        Else

            LastRow = VertOffset + _ImpactAreaVM.Items.Count - _scrollbar.Maximum
        End If

        Dim StartRowIndex As Integer = CInt(VertOffset)
        Dim EndRowIndex As Integer = CInt(LastRow)

        For i = 0 To errors.Count - 1
            If Not IsNothing(errors(i)) Then
                If errors(i).Row < StartRowIndex OrElse errors(i).Row > EndRowIndex Then Continue For
                Dim o As DataGridCell = GetCell(errors(i).Row, errors(i).Column)
                If IsNothing(o) Then Continue For
                o.BorderBrush = Brushes.Red
                o.BorderThickness = New Thickness(2)
                o.Margin = New Thickness(0, -1, 0, -1)

                If IsNothing(o.ToolTip) OrElse o.ToolTip = "" Then
                    ToolTipService.SetShowDuration(o, 10000)
                    o.ToolTip = errors(i).Message & " on row " & errors(i).Row
                Else
                    o.ToolTip = o.ToolTip & vbNewLine & errors(i).Message & " on row " & errors(i).Row
                End If
                Dim style = New Style(GetType(System.Windows.Controls.Primitives.DataGridRowHeader))
                style.Setters.Add(New Setter(ToolTipService.ToolTipProperty, "Row contains errors"))
                style.Setters.Add(New Setter() With {.Property = BorderBrushProperty, .Value = Brushes.DarkGoldenrod})
                style.Setters.Add(New Setter() With {.Property = BorderThicknessProperty, .Value = New Thickness(1)})
                style.Setters.Add(New Setter() With {.Property = ForegroundProperty, .Value = Brushes.Red})
                style.Setters.Add(New Setter() With {.Property = FontWeightProperty, .Value = FontWeights.Bold})
                style.Setters.Add(New Setter() With {.Property = ContentProperty, .Value = " !"})
                style.Setters.Add(New Setter() With {.Property = HorizontalContentAlignmentProperty, .Value = HorizontalAlignment.Center})

                Dim r As DataGridRow = GetRow(errors(i).Row)
                Dim h As New System.Windows.Controls.Primitives.DataGridRowHeader
                'r.Header = "!"
                r.HeaderStyle = style
            End If
        Next
        GetCell(CInt(VertOffset), 0)
        _isvalidating = False
    End Sub
    Private Sub AutoCurveDataGrid_CellEditEnding(sender As Object, e As DataGridCellEditEndingEventArgs) Handles ImpactAreas.CellEditEnding
        If Not _manualcommitedit Then
            _manualcommitedit = True
            ImpactAreas.CommitEdit(DataGridEditingUnit.Row, True)
            _manualcommitedit = False
            ImpactAreas.Items.Refresh()
            colorcells(_ImpactAreaVM.Validate)
        End If
        _isediting = False

    End Sub
    Private Sub NotifyPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(info))
    End Sub
End Class
