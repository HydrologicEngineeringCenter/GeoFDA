Imports Consequences_Assist.ComputableObjects
Imports System.Windows.Controls
Imports System.Windows

Public Class DDEC
    Implements System.ComponentModel.INotifyPropertyChanged
    Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
	'Public Shared ReadOnly SelectedOcctypeProperty As DependencyProperty = DependencyProperty.Register("SetSelectedOcctype", GetType(String), GetType(DDEC))
	Private _OccTypes As List(Of ComputableObjects.OccupancyType)
	Private _DamCats As List(Of ComputableObjects.DamageCategory)
	Public Property IsEditable As Boolean = False
    Public Property DisplayVehicleParameters As Boolean = True
    Private _HandleSelection As Boolean = True
	Public Event DamageCategoryAdded(ByVal DamCat As ComputableObjects.DamageCategory)
	Public Event DamageCategoryAssignmentChanged(ByVal occtype As ComputableObjects.OccupancyType, ByVal damagecategory As ComputableObjects.DamageCategory)
	Public Event OcctypeAdded(ByVal occtype As ComputableObjects.OccupancyType)
	Public Property OccTypes As List(Of ComputableObjects.OccupancyType)
		Get
			Return _OccTypes
		End Get
		Set(value As List(Of ComputableObjects.OccupancyType))
			_OccTypes = value
			NotifyPropertyChanged("Occtypes")
		End Set
	End Property
	Public Property SetSelectedOcctype As String
        Get
            Return OccTypeNameBox.SelectedItem
        End Get
        Set(value As String)
            If OccTypeNameBox.Items.Contains(value) Then
                OccTypeNameBox.SelectedItem = value
                TestAndSetCurrentSelection()
            Else
                Debug.Print("Occtype doesnt exist")
            End If
        End Set
    End Property
    Public Sub New()
		_OccTypes = New List(Of ComputableObjects.OccupancyType)
		_DamCats = New List(Of ComputableObjects.DamageCategory)
		' This call is required by the designer.
		InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Private Sub Editor_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        OccTypeNameBox.Items.Clear()
        CMBDamCat.Items.Clear()
        For i As Int32 = 0 To _OccTypes.Count - 1
            OccTypeNameBox.Items.Add(_OccTypes(i).Name)
        Next
        For i As Int32 = 0 To _DamCats.Count - 1
            CMBDamCat.Items.Add(_DamCats(i).Name)
        Next
        '
    End Sub
    Public Sub DefineSettings()
        If IsEditable Then

        Else
            StructureCheck.IsEnabled = False
            ContentCheck.IsEnabled = False
            OtherCheck.IsEnabled = False
            VehicleCheck.IsEnabled = False

            CMBDamCat.IsEnabled = False 'disallow changing damage categories, but allow changing occupancy types.
            StructureDDControl.IsReadonly = True
            ContentDDControl.IsReadonly = True
            OtherDDControl.IsReadonly = True
            VehicleDDControl.IsReadonly = True
            FoundationHeightUncertainty.IsEnabled = False
            StructureValueUncertainty.IsEnabled = False
            ContentValueUncertainty.IsEnabled = False
            OtherValueUncertainty.IsEnabled = False
            VehicleValueUncertainty.IsEnabled = False
            CreateNewButton.Visibility = Windows.Visibility.Hidden
            'NewOccTypeNameTextBox.Visibility = Windows.Visibility.Visible
            'OccTypeNameBox.Visibility = Windows.Visibility.Hidden

        End If
        If DisplayVehicleParameters Then
        Else
            VehicleCheck.Visibility = Windows.Visibility.Collapsed
            VehicleDDControl.Visibility = Windows.Visibility.Collapsed
            VehicleDDTabItem.Visibility = Windows.Visibility.Collapsed
            VehicleValueUncertainty.Visibility = Windows.Visibility.Collapsed
        End If



    End Sub
	'Public Sub LoadOccType(ByVal OccType As OccupancyType)
	'    If IsEditable = True Then

	'    End If
	'End Sub
	Public Sub LoadOccTypes(ByVal OccupancyTypes As List(Of ComputableObjects.OccupancyType), ByVal damcats As List(Of ComputableObjects.DamageCategory))
		'_OccTypes.Clear()
		Dim otypes As New List(Of ComputableObjects.OccupancyType)
		'otypes.Add(New OccupancyType)
		For Each occtype As ComputableObjects.OccupancyType In OccupancyTypes
			otypes.Add(occtype.Clone)
		Next
		_OccTypes = otypes
		_DamCats = damcats
		'_DamCats.Insert(0, New DamageCategory)
		OccTypeNameBox.Items.Clear()
		CMBDamCat.Items.Clear()
		For i As Int32 = 0 To _OccTypes.Count - 1
			OccTypeNameBox.Items.Add(_OccTypes(i).Name)
		Next
		For i As Int32 = 0 To _DamCats.Count - 1
			CMBDamCat.Items.Add(_DamCats(i).Name)
		Next
	End Sub
	Public Sub AddOccType(ByVal NewOccType As ComputableObjects.OccupancyType)
		_OccTypes.Add(NewOccType)
		OccTypeNameBox.Items.Add(NewOccType.Name)
		RaiseEvent OcctypeAdded(NewOccType)
		Dim damcatexists As Boolean = False
		For i = 0 To _DamCats.Count - 1
			If _DamCats(i).Name = NewOccType.DamageCategory.Name Then damcatexists = True : Exit For
		Next
		If damcatexists Then
			'it already exists.
		Else
			_DamCats.Add(NewOccType.DamageCategory)
			CMBDamCat.Items.Add(_DamCats.Last.Name)
			RaiseEvent DamageCategoryAdded(NewOccType.DamageCategory)
		End If
	End Sub
	Private Sub OccTypeNameBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If _HandleSelection = False Then
            _HandleSelection = True
            Exit Sub
        End If
        '
        If OccTypeNameBox.SelectedIndex = -1 Then
        Else
            If OccTypeNameBox.SelectedValue.ToString.Length > 32 Then
                OccTypeNameBox.Foreground = Media.Brushes.Red
                OccTypeNameBox.ToolTip = "This name is more than 32 characters, please rename this occupancy type."
            Else
                OccTypeNameBox.Foreground = Media.Brushes.Black
                OccTypeNameBox.ToolTip = Nothing
            End If
        End If
        Dim CurrentSelected As Int32
        If StructureDDTabItem.IsSelected Then CurrentSelected = 0
        If ContentDDTabItem.IsSelected Then CurrentSelected = 1
        If VehicleDDTabItem.IsSelected Then CurrentSelected = 2
        If OtherDDTabItem.IsSelected Then CurrentSelected = 3
        '
        If e.RemovedItems.Count > 0 And IsEditable = True Then
            Dim PreviousIndex As Int32 = OccTypeNameBox.Items.IndexOf(e.RemovedItems(0))
            If PreviousIndex > -1 Then
                Dim NewCurve As Statistics.MonotonicCurveUSingle, MessageOut As String = ""
                With _OccTypes(PreviousIndex)
                    '
                    'If FoundationHeightUncertainty.CheckDistributionValidity(MessageOut) = True Then
                    .FoundationHeightUncertainty = FoundationHeightUncertainty.ReturnDistribution
                    'Else
                    '    'MsgBox("Foundation Height Uncertainty: " & MessageOut)
                    '    '_HandleSelection = False
                    '    'CType(sender, ComboBox).SelectedItem = e.RemovedItems(0)
                    '    'Exit Sub
                    'End If
                    '
                    If CMBDamCat.SelectedIndex = -1 Then
                    Else
                        If .DamageCategory.Name = CMBDamCat.SelectedItem Then
                            'names are the same, damagecategory unchagnged.
                        Else
                            RaiseEvent DamageCategoryAssignmentChanged(_OccTypes(PreviousIndex), _DamCats(CMBDamCat.SelectedIndex))
                            .DamageCategory = _DamCats(CMBDamCat.SelectedIndex)
                        End If
                    End If

                    '
                    .CalcStructDamage = CBool(StructureCheck.IsChecked)
                    If StructureCheck.IsChecked Then
                        StructureDDTabItem.Focus()
                        StructureDDTabItem.UpdateLayout()
                        NewCurve = StructureDDControl.ExportToPairedData
                        'If MessageOut = "" Then
                        .SetStructurePercentDD = NewCurve
                        'Else
                        '    MsgBox("Structure Depth Damage Curve: " & MessageOut)
                        '    '_HandleSelection = False
                        '    'CType(sender, ComboBox).SelectedItem = e.RemovedItems(0)
                        '    'Exit Sub
                        'End If
                        'If StructureValueUncertainty.CheckDistributionValidity(MessageOut) = True Then
                        .StructureValueUncertainty = StructureValueUncertainty.ReturnDistribution
                        'Else
                        '    MsgBox("Structure Value Uncertainty: " & MessageOut)
                        '    '_HandleSelection = False
                        '    'CType(sender, ComboBox).SelectedItem = e.RemovedItems(0)
                        '    ' Exit Sub
                        'End If
                    End If
                    '
                    .CalcContentDamage = CBool(ContentCheck.IsChecked)
                    If ContentCheck.IsChecked Then
                        ContentDDTabItem.Focus()
                        ContentDDTabItem.UpdateLayout()
                        NewCurve = ContentDDControl.ExportToPairedData
                        'If MessageOut = "" Then
                        .SetContentPercentDD = NewCurve
                        'Else
                        '    MsgBox("Content Depth Damage Curve: " & MessageOut)
                        '    '_HandleSelection = False
                        '    'CType(sender, ComboBox).SelectedItem = e.RemovedItems(0)
                        '    'Exit Sub
                        'End If
                        'If ContentValueUncertainty.CheckDistributionValidity(MessageOut) = True Then
                        .ContentValueUncertainty = ContentValueUncertainty.ReturnDistribution
                        'Else
                        '    MsgBox("Content Value Uncertainty: " & MessageOut)
                        '    '_HandleSelection = False
                        '    'CType(sender, ComboBox).SelectedItem = e.RemovedItems(0)
                        '    'Exit Sub
                        'End If
                    End If
                    '
                    .CalcVehicleDamage = CBool(VehicleCheck.IsChecked)
                    If VehicleCheck.IsChecked Then
                        VehicleDDTabItem.Focus()
                        VehicleDDTabItem.UpdateLayout()
                        NewCurve = VehicleDDControl.ExportToPairedData
                        'If MessageOut = "" Then
                        .SetVehiclePercentDD = NewCurve
                        'Else
                        '    MsgBox("Vehicle Depth Damage Curve: " & MessageOut)
                        '    '_HandleSelection = False
                        '    'CType(sender, ComboBox).SelectedItem = e.RemovedItems(0)
                        '    'Exit Sub
                        'End If
                        'If VehicleValueUncertainty.CheckDistributionValidity(MessageOut) = True Then
                        .VehicleValueUncertainty = VehicleValueUncertainty.ReturnDistribution
                        'Else
                        'MsgBox("Vehicle Value Uncertainty: " & MessageOut)
                        '_HandleSelection = False
                        'CType(sender, ComboBox).SelectedItem = e.RemovedItems(0)
                        'Exit Sub
                        'End If
                    End If
                    '
                    .CalcOtherDamage = CBool(OtherCheck.IsChecked)
                    If OtherCheck.IsChecked Then
                        OtherDDTabItem.Focus()
                        OtherDDTabItem.UpdateLayout()
                        NewCurve = OtherDDControl.ExportToPairedData
                        'If MessageOut = "" Then
                        .SetOtherPercentDD = NewCurve
                        'Else
                        '    MsgBox("Other Depth Damage Curve: " & MessageOut)
                        '    '_HandleSelection = False
                        '    'CType(sender, ComboBox).SelectedItem = e.RemovedItems(0)
                        '    'Exit Sub
                        'End If
                        'If OtherValueUncertainty.CheckDistributionValidity(MessageOut) = True Then
                        .OtherValueUncertainty = OtherValueUncertainty.ReturnDistribution
                        'Else
                        '    MsgBox("Other Value Uncertainty: " & MessageOut)
                        '    '_HandleSelection = False
                        '    'CType(sender, ComboBox).SelectedItem = e.RemovedItems(0)
                        '    'Exit Sub
                        'End If
                    End If

                End With
            End If
        End If
        '
        If OccTypeNameBox.SelectedIndex = -1 Then
            StructureDDTabItem.IsEnabled = False
            StructureDDControl.IsEnabled = False
            ContentDDTabItem.IsEnabled = False
            ContentDDControl.IsEnabled = False
            OtherDDTabItem.IsEnabled = False
            OtherDDControl.IsEnabled = False
            VehicleDDTabItem.IsEnabled = False
            VehicleDDControl.IsEnabled = False
            UpdateInterface(-1)
            Exit Sub
        End If
        '
        With _OccTypes(OccTypeNameBox.SelectedIndex)
            FoundationHeightUncertainty.LoadOccTypeData(.FoundationHeightUncertainty)
            'DamageCategoryBox.SelectedIndex = _OccTypes(OccTypeNameBox.SelectedIndex).DamageCategoryIndex
            StructureDDControl.LoadCurveAndValidation(.GetStructurePercentDD, AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain)
            StructureCheck.IsChecked = .CalcStructDamage
            StructureValueUncertainty.LoadOccTypeData(.StructureValueUncertainty)
            '
            ContentDDControl.LoadCurveAndValidation(.GetContentPercentDD, AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain)
            ContentCheck.IsChecked = .CalcContentDamage
            ContentValueUncertainty.LoadOccTypeData(.ContentValueUncertainty)
            '
            OtherDDControl.LoadCurveAndValidation(.GetOtherPercentDD, AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain)
            OtherCheck.IsChecked = .CalcOtherDamage
            OtherValueUncertainty.LoadOccTypeData(.OtherValueUncertainty)
            '
            VehicleDDControl.LoadCurveAndValidation(.GetVehiclePercentDD, AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain)
            VehicleCheck.IsChecked = .CalcVehicleDamage
            VehicleValueUncertainty.LoadOccTypeData(.VehicleValueUncertainty)
            '
            CMBDamCat.SelectedItem = .DamageCategory.Name
        End With
        '
        If CurrentSelected = 0 Then StructureDDTabItem.IsSelected = True
        If CurrentSelected = 1 Then ContentDDTabItem.IsSelected = True
        If CurrentSelected = 2 Then VehicleDDTabItem.IsSelected = True
        If CurrentSelected = 3 Then OtherDDTabItem.IsSelected = True
        UpdateInterface(OccTypeNameBox.SelectedIndex)
    End Sub
    Public Function TestAndSetCurrentSelection() As Boolean
        Dim CurrentIndex As Int32 = OccTypeNameBox.SelectedIndex
        If CurrentIndex > -1 Then
            Dim NewCurve As Statistics.MonotonicCurveUSingle, MessageOut As String = ""
            With _OccTypes(CurrentIndex)
                '
                'If FoundationHeightUncertainty.CheckDistributionValidity(MessageOut) = True Then
                .FoundationHeightUncertainty = FoundationHeightUncertainty.ReturnDistribution
                'Else
                '    MsgBox("Foundation Height Uncertainty: " & MessageOut)
                '    'Return False
                'End If
                '.DamageCategoryIndex = DamageCategoryBox.SelectedIndex
                If CMBDamCat.SelectedIndex = -1 Then
                    'no damage category selected, this is not good.
                Else
                    If .DamageCategory.Name = CMBDamCat.SelectedItem Then
                        'names are the same, damagecategory unchagnged.
                    Else
                        RaiseEvent DamageCategoryAssignmentChanged(_OccTypes(CurrentIndex), _DamCats(CMBDamCat.SelectedIndex))
                        .DamageCategory = _DamCats(CMBDamCat.SelectedIndex)
                    End If

                End If
                .CalcStructDamage = CBool(StructureCheck.IsChecked)
                If StructureCheck.IsChecked Then
                    StructureDDTabItem.Focus()
                    StructureDDTabItem.UpdateLayout()
                    NewCurve = StructureDDControl.ExportToPairedData
                    'If MessageOut = "" Then
                    .SetStructurePercentDD = NewCurve
                    'Else
                    '    MsgBox("Structure Depth Damage Curve: " & MessageOut)
                    '    'Return False
                    'End If
                    'If StructureValueUncertainty.CheckDistributionValidity(MessageOut) = True Then
                    .StructureValueUncertainty = StructureValueUncertainty.ReturnDistribution
                    'Else
                    '    MsgBox("Structure Value Uncertainty: " & MessageOut)
                    '    'Return False
                    'End If
                End If
                '
                .CalcContentDamage = CBool(ContentCheck.IsChecked)
                If ContentCheck.IsChecked Then
                    ContentDDTabItem.Focus()
                    ContentDDTabItem.UpdateLayout()
                    NewCurve = ContentDDControl.ExportToPairedData
                    'If MessageOut = "" Then
                    .SetContentPercentDD = NewCurve
                    'Else
                    '    MsgBox("Content Depth Damage Curve: " & MessageOut)
                    '    'Return False
                    'End If
                    'If ContentValueUncertainty.CheckDistributionValidity(MessageOut) = True Then
                    .ContentValueUncertainty = ContentValueUncertainty.ReturnDistribution
                    'Else
                    '    MsgBox("Content Value Uncertainty: " & MessageOut)
                    '    'Return False
                    'End If
                End If
                '
                .CalcVehicleDamage = CBool(VehicleCheck.IsChecked)
                If VehicleCheck.IsChecked Then
                    VehicleDDTabItem.Focus()
                    VehicleDDTabItem.UpdateLayout()
                    NewCurve = VehicleDDControl.ExportToPairedData
                    'If MessageOut = "" Then
                    .SetVehiclePercentDD = NewCurve
                    'Else
                    '    MsgBox("Vehicle Depth Damage Curve: " & MessageOut)
                    '    'Return False
                    'End If
                    'If VehicleValueUncertainty.CheckDistributionValidity(MessageOut) = True Then
                    .VehicleValueUncertainty = VehicleValueUncertainty.ReturnDistribution
                    'Else
                    '    MsgBox("Vehicle Value Uncertainty: " & MessageOut)
                    '    'Return False
                    'End If
                End If
                '
                .CalcOtherDamage = CBool(OtherCheck.IsChecked)
                If OtherCheck.IsChecked Then
                    OtherDDTabItem.Focus()
                    OtherDDTabItem.UpdateLayout()
                    NewCurve = OtherDDControl.ExportToPairedData
                    ' If MessageOut = "" Then
                    .SetOtherPercentDD = NewCurve
                    'Else
                    '    MsgBox("Other Depth Damage Curve: " & MessageOut)
                    '    'Return False
                    'End If
                    'If OtherValueUncertainty.CheckDistributionValidity(MessageOut) = True Then
                    .OtherValueUncertainty = OtherValueUncertainty.ReturnDistribution
                    'Else
                    '    MsgBox("Other Value Uncertainty: " & MessageOut)
                    '    'Return False
                    'End If
                End If

            End With
        End If
        Return True
    End Function
    Private Sub UpdateInterface(ByVal OccTypteDataIndex As Int32) 'ByVal OccTypeData As OccupancyType)
        If OccTypteDataIndex = -1 Then
            OccTypeNameBox.SelectedIndex = -1
            StructureCheck.IsChecked = False
            ContentCheck.IsChecked = False
            OtherCheck.IsChecked = False
            VehicleCheck.IsChecked = False
            StructureDDTabItem.IsEnabled = False
            StructureDDControl.IsEnabled = False
            ContentDDTabItem.IsEnabled = False
            ContentDDControl.IsEnabled = False
            OtherDDTabItem.IsEnabled = False
            OtherDDControl.IsEnabled = False
            VehicleDDTabItem.IsEnabled = False
            VehicleDDControl.IsEnabled = False
        Else
			Dim OccTypeData As ComputableObjects.OccupancyType = _OccTypes(OccTypteDataIndex)
			With OccTypeData
                If .CalcStructDamage = True Then
                    StructureDDTabItem.IsEnabled = True
                    StructureDDTabItem.UpdateLayout()
                    StructureDDControl.LoadCurveAndValidation(.GetStructurePercentDD, AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain)
                    StructureDDControl.IsEnabled = IsEditable
                    StructureValueUncertainty.IsEnabled = IsEditable
                Else
                    StructureDDTabItem.IsEnabled = False
                    StructureDDControl.IsEnabled = False
                    StructureValueUncertainty.IsEnabled = False
                End If
                If .CalcContentDamage = True Then
                    ContentDDTabItem.IsEnabled = True

                    ContentValueUncertainty.IsEnabled = IsEditable
                    ContentDDTabItem.UpdateLayout()
                    ContentDDControl.LoadCurveAndValidation(.GetContentPercentDD, AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain)
                    ContentDDControl.IsEnabled = IsEditable
                Else
                    ContentDDTabItem.IsEnabled = False
                    ContentDDControl.IsEnabled = False
                    ContentValueUncertainty.IsEnabled = False
                End If
                If .CalcOtherDamage = True Then
                    OtherDDTabItem.IsEnabled = True
                    OtherValueUncertainty.IsEnabled = IsEditable
                    OtherDDTabItem.UpdateLayout()
                    OtherDDControl.LoadCurveAndValidation(.GetOtherPercentDD, AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain)
                    OtherDDControl.IsEnabled = IsEditable
                Else
                    OtherDDTabItem.IsEnabled = False
                    OtherDDControl.IsEnabled = False
                    OtherValueUncertainty.IsEnabled = False
                End If
                If .CalcVehicleDamage = True Then
                    VehicleDDTabItem.IsEnabled = True
                    VehicleValueUncertainty.IsEnabled = IsEditable
                    VehicleDDTabItem.UpdateLayout()
                    VehicleDDControl.LoadCurveAndValidation(.GetVehiclePercentDD, AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain)
                    VehicleDDControl.IsEnabled = IsEditable
                Else
                    VehicleDDTabItem.IsEnabled = False
                    VehicleDDControl.IsEnabled = False
                    VehicleValueUncertainty.IsEnabled = False
                End If
            End With
        End If
    End Sub
    Private Sub CheckBox_Checked(sender As Object, e As RoutedEventArgs)
        Dim GotChecked As CheckBox = CType(sender, CheckBox)
        If StructureCheck.Equals(GotChecked) Then
            StructureDDControl.IsEnabled = True
            StructureDDTabItem.IsEnabled = True
            StructureValueUncertainty.IsEnabled = True
        End If
        If ContentCheck.Equals(GotChecked) Then
            ContentDDControl.IsEnabled = True
            ContentDDTabItem.IsEnabled = True
            ContentValueUncertainty.IsEnabled = True
        End If
        If VehicleCheck.Equals(GotChecked) Then
            VehicleDDControl.IsEnabled = True
            VehicleDDTabItem.IsEnabled = True
            VehicleValueUncertainty.IsEnabled = True
        End If
        If OtherCheck.Equals(GotChecked) Then
            OtherDDControl.IsEnabled = True
            OtherDDTabItem.IsEnabled = True
            OtherValueUncertainty.IsEnabled = True
        End If
    End Sub
    Private Sub CheckBox_Unchecked(sender As Object, e As RoutedEventArgs)
        Dim GotUnChecked As CheckBox = CType(sender, CheckBox)
        If StructureCheck.Equals(GotUnChecked) Then
            StructureDDControl.IsEnabled = False
            StructureDDTabItem.IsEnabled = False
            StructureValueUncertainty.IsEnabled = False
        End If
        If ContentCheck.Equals(GotUnChecked) Then
            ContentDDControl.IsEnabled = False
            ContentDDTabItem.IsEnabled = False
            ContentValueUncertainty.IsEnabled = False
        End If
        If VehicleCheck.Equals(GotUnChecked) Then
            VehicleDDControl.IsEnabled = False
            VehicleDDTabItem.IsEnabled = False
            VehicleValueUncertainty.IsEnabled = False
        End If
        If OtherCheck.Equals(GotUnChecked) Then
            OtherDDControl.IsEnabled = False
            OtherDDTabItem.IsEnabled = False
            OtherValueUncertainty.IsEnabled = False
        End If
    End Sub
    Private Sub CaptureOccType()

    End Sub



    Private Sub NotifyPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(info))
    End Sub
End Class

