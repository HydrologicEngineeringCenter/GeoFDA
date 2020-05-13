Public Class EditDamageCategories
    Implements System.ComponentModel.INotifyPropertyChanged
    Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Private _DamCats As DamCatsVM
    Private _isvalidating As Boolean = False
    Private _isediting As Boolean = False
    Private _scrollbar As Primitives.ScrollBar
    Private _scrollviewer As ScrollViewer
    Private _manualcommitedit As Boolean = False
	Public Event DamageCategoryDeleted(ByVal damagecategories As List(Of ComputableObjects.DamageCategory), ByRef cancel As Boolean)
	Public Event DamageCategoryRenamed(ByVal Damagecategory As ComputableObjects.DamageCategory, ByVal newname As String)
	Public Property DamCats As DamCatsVM
        Get
            Return _DamCats
        End Get
        Set(value As DamCatsVM)
            _DamCats = value
            NotifyPropertyChanged("DamCats")
        End Set
    End Property
    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        DamCats = New DamCatsVM
        AddHandler DamCats.DamageCategoryDeleted, AddressOf OnDamageCategoryDeleted
        AddHandler DamCats.DamageCategoryRenamed, AddressOf OnDamageCategoryRenamed
        CMDRemove.IsEnabled = False
        NotifyPropertyChanged("DamCats")
    End Sub
	Sub New(ByVal dclist As ComputableObjects.DamageCategories)
		InitializeComponent()
		DamCats = New DamCatsVM
		AddHandler DamCats.DamageCategoryDeleted, AddressOf OnDamageCategoryDeleted
		AddHandler DamCats.DamageCategoryRenamed, AddressOf OnDamageCategoryRenamed
		For i = 0 To dclist.GetDamageCategories.Count - 1
			DamCats.Add(New DamCatRowItem(dclist.GetDamageCategories(i).Name, dclist.GetDamageCategories(i).GetDescription, dclist.GetDamageCategories(i).GetCostFactor))
		Next
		If DamCats.Items.Count > 0 Then CMDRemove.IsEnabled = True
		NotifyPropertyChanged("DamCats")
	End Sub
	Private Sub OnDamageCategoryDeleted(ByVal damagecategories As List(Of ComputableObjects.DamageCategory), ByRef cancel As Boolean)
		RaiseEvent DamageCategoryDeleted(damagecategories, cancel)
		If cancel Then
			Debug.Print("damage categories not deleted")
		Else
			Debug.Print("damage categories deleted")
		End If
	End Sub
	Private Sub OnDamageCategoryRenamed(ByVal damagecategory As ComputableObjects.DamageCategory, ByVal newname As String)
		RaiseEvent DamageCategoryRenamed(damagecategory, newname)
		Debug.Print(damagecategory.Name & " renamed " & newname)
	End Sub
	Public Function GetDamageCategories() As ComputableObjects.DamageCategories

		Dim damcats As New List(Of ComputableObjects.DamageCategory)
		For i = 0 To _DamCats.Items.Count - 1
			damcats.Add(New ComputableObjects.DamageCategory(CStr(_DamCats.Items(i).Name), CStr(_DamCats.Items(i).Description), 365, CDbl(_DamCats.Items(i).Index_Factor)))
		Next
		Return New ComputableObjects.DamageCategories(damcats)

	End Function
	Private Sub CMDCancel_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles CMDCancel.Click
        DialogResult = False
        Me.Close()
    End Sub
    Private Sub CMDOk_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles CMDOk.Click
        'do any validation
        DialogResult = True
        Me.Close()
    End Sub
    Private Sub StreamDGV_AutoGeneratingColumn(sender As Object, e As DataGridAutoGeneratingColumnEventArgs) Handles DamCatsDGV.AutoGeneratingColumn
        If e.PropertyName.Equals("Index_Factor") Then
            Dim dataGridTextColumn As DataGridTextColumn = TryCast(e.Column, DataGridTextColumn)
            dataGridTextColumn.Header = "Index Factor"
        End If
        If e.PropertyType = GetType(String) Then
            e.Column.Width = New DataGridLength(1, DataGridLengthUnitType.Star)
        Else
            e.Cancel = True
        End If
    End Sub
    Sub AddRows(ByVal insertat As Integer, ByVal number As Integer)
        '_DamCats.Add(insertat, number)
        colorcells(_DamCats.validate)
        If Not CMDRemove.IsEnabled Then CMDRemove.IsEnabled = True
        '_vm.PlotGrid()
    End Sub
    Sub RemoveRows(ByVal indices As List(Of Int32))
        '_DamCats.RemoveRows(indices)
        'For i = 0 To indices.Count - 1
        '    OnDamageCategoryDeleted(_DamCats.Items(indices(i)).ToDamCat)
        'Next
        colorcells(_DamCats.validate)
        If DamCats.Items.Count = 0 Then CMDRemove.IsEnabled = False
    End Sub
    Private Sub PreviewRemoveRows(ByVal indices As List(Of Int32), ByRef cancel As Boolean)
		Dim l As New List(Of ComputableObjects.DamageCategory)
		For i = 0 To indices.Count - 1
            l.Add(_DamCats.Items(indices(i)).ToDamCat)
        Next
        OnDamageCategoryDeleted(l, cancel)
    End Sub
    Sub StopValidation()
        _DamCats.CanValidate = False
    End Sub
    Sub Pasted()
        _DamCats.CanValidate = True
        colorcells(_DamCats.validate)
    End Sub
    Private Sub colorcells(ByVal errors As List(Of DamCatError))
        _isvalidating = True
        If IsNothing(errors) Then _isvalidating = False : Exit Sub
        If _isediting = False Then
            DamCatsDGV.Items.Refresh()
        End If

        Dim VertOffset As Double
        If IsNothing(_scrollviewer) Then
            VertOffset = 0
            _scrollviewer = DamCatsDGV.GetTheVisualChild(Of ScrollViewer)(DamCatsDGV)
        Else
            VertOffset = _scrollviewer.VerticalOffset
        End If

        Dim LastRow As Double
        If IsNothing(_scrollbar) Then
            LastRow = _DamCats.Items.Count - 1
            If IsNothing(_scrollviewer) Then
            Else
                _scrollbar = CType(_scrollviewer.Template.FindName("PART_VerticalScrollBar", _scrollviewer), Primitives.ScrollBar)
            End If
        Else

            LastRow = VertOffset + _DamCats.Items.Count - _scrollbar.Maximum
        End If

        Dim StartRowIndex As Integer = CInt(VertOffset)
        Dim EndRowIndex As Integer = CInt(LastRow)

        For i = 0 To errors.Count - 1
            If Not IsNothing(errors(i)) Then
                If errors(i).Row < StartRowIndex OrElse errors(i).Row > EndRowIndex Then Continue For
                Dim o As DataGridCell = DamCatsDGV.GetCell(errors(i).Row, 0)
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

                Dim r As DataGridRow = DamCatsDGV.GetRow(errors(i).Row)
                Dim h As New System.Windows.Controls.Primitives.DataGridRowHeader
                'r.Header = "!"
                r.HeaderStyle = style
            End If
        Next
        DamCatsDGV.GetCell(CInt(VertOffset), 0)
        _isvalidating = False
    End Sub
    Private Sub AutoCurveDataGrid_CellEditEnding(sender As Object, e As DataGridCellEditEndingEventArgs) Handles DamCatsDGV.CellEditEnding
        If Not _manualcommitedit Then
            _manualcommitedit = True
            DamCatsDGV.CommitEdit(DataGridEditingUnit.Row, True)
            _manualcommitedit = False
            DamCatsDGV.Items.Refresh()
            colorcells(_DamCats.validate)
        End If
        _isediting = False

    End Sub
    Private Sub NotifyPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(info))
    End Sub

    Private Sub EditDamageCategories_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        AddHandler DamCatsDGV.DataPasted, AddressOf Pasted
        AddHandler DamCatsDGV.PreviewPasteData, AddressOf StopValidation
        AddHandler DamCatsDGV.RowsAdded, AddressOf AddRows
        AddHandler DamCatsDGV.RowsDeleted, AddressOf RemoveRows
        AddHandler DamCatsDGV.PreviewDeleteRows, AddressOf PreviewRemoveRows
        _scrollviewer = DamCatsDGV.GetTheVisualChild(Of ScrollViewer)(DamCatsDGV)
        If IsNothing(_scrollviewer) Then
        Else
            _scrollbar = CType(_scrollviewer.Template.FindName("PART_VerticalScrollBar", _scrollviewer), Primitives.ScrollBar)
        End If
    End Sub

    Private Sub CMDAdd_Click(sender As Object, e As RoutedEventArgs) Handles CMDAdd.Click
        If IsNothing(DamCatsDGV.SelectedIndex) OrElse DamCatsDGV.SelectedIndex = -1 Then
            DamCats.Add(New DamCatRowItem())
        Else
            If DamCatsDGV.SelectedItems.Count > 1 Then
                'multiple
                Dim num As Integer = DamCatsDGV.SelectedItems.Count
                For i = 0 To num - 1
                    DamCats.Add(New DamCatRowItem(), DamCatsDGV.SelectedIndex + i)
                Next
            Else
                DamCats.Add(New DamCatRowItem(), DamCatsDGV.SelectedIndex)
            End If

        End If
        NotifyPropertyChanged("DamCats")
        If Not CMDRemove.IsEnabled Then CMDRemove.IsEnabled = True
        If Not DamCats.Items.Count = 0 Then colorcells(_DamCats.validate)
    End Sub

    Private Sub CMDRemove_Click(sender As Object, e As RoutedEventArgs) Handles CMDRemove.Click
        If DamCatsDGV.Items.Count = 0 Then Exit Sub
        If DamCatsDGV.SelectedItems.Count <= 1 Then
            If DamCatsDGV.SelectedIndex = -1 Then
                Dim cancel As Boolean = False
                'OnDamageCategoryDeleted(_DamCats.Items.Last.ToDamCat, cancel)
                DamCats.RemoveRow(_DamCats.Items.Last, cancel)
                If Not cancel Then NotifyPropertyChanged("DamCats")
            Else
                Dim cancel As Boolean = False
                'OnDamageCategoryDeleted(DamCatsDGV.SelectedItem.ToDamCat, cancel)
                DamCats.RemoveRow(DamCatsDGV.SelectedItem, cancel)
                If Not cancel Then NotifyPropertyChanged("DamCats")
            End If
        Else
            Dim num As Integer = DamCatsDGV.SelectedItems.Count
            For i = 0 To num - 1
                Dim cancel As Boolean = False
                'OnDamageCategoryDeleted(_DamCats.Items(DamCatsDGV.SelectedItems(0)).ToDamCat, cancel)
                DamCats.RemoveRow(DamCatsDGV.SelectedItems(0), cancel)
                'If Not cancel Then
            Next
            NotifyPropertyChanged("DamCats")
            End If

            If DamCats.Items.Count = 0 Then
                CMDRemove.IsEnabled = False
            Else
                colorcells(_DamCats.validate)
            End If

    End Sub
End Class
