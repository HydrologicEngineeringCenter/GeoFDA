Imports System.Collections.ObjectModel
Public Class EconFileSpec
    Implements System.ComponentModel.INotifyPropertyChanged
    Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Private _AvailablePlans As ObservableCollection(Of SelectablePlan)
    Private _DamageReaches As ObservableCollection(Of ImpactAreaChildTreeNode)
    Private _Inventories As ObservableCollection(Of StructureInventoryChildTreeNode)
    Sub New(ByVal GriddedPlans As ObservableCollection(Of HydraulicsChildTreenode), ByVal ilist As ObservableCollection(Of StructureInventoryChildTreeNode), ByVal dlist As ObservableCollection(Of ImpactAreaChildTreeNode))
        InitializeComponent()
        Inventories = ilist
        DamageReaches = dlist
        Dim tmplist As New ObservableCollection(Of SelectablePlan)
        For i = 0 To GriddedPlans.Count - 1
            tmplist.Add(New SelectablePlan(GriddedPlans(i)))
        Next
        AvailablePlans = tmplist
    End Sub
#Region "Properties"
    Public Property AvailablePlans As ObservableCollection(Of SelectablePlan)
        Get
            Return _AvailablePlans
        End Get
        Set(value As ObservableCollection(Of SelectablePlan))
            _AvailablePlans = value
            NotifyPropertyChanged("AvailablePlans")
        End Set
    End Property
    Public Property DamageReaches As ObservableCollection(Of ImpactAreaChildTreeNode)
        Get
            Return _DamageReaches
        End Get
        Set(value As ObservableCollection(Of ImpactAreaChildTreeNode))
            _DamageReaches = value
            NotifyPropertyChanged("DamageReaches")
        End Set
    End Property
    Public Property Inventories As ObservableCollection(Of StructureInventoryChildTreeNode)
        Get
            Return _Inventories
        End Get
        Set(value As ObservableCollection(Of StructureInventoryChildTreeNode))
            _Inventories = value
            NotifyPropertyChanged("Inventories")
        End Set
    End Property
    Public ReadOnly Property GetSelectedPlans As List(Of HydraulicsChildTreenode)
        Get
            Dim list As New List(Of HydraulicsChildTreenode)
            For i = 0 To AvailablePlans.Count - 1
                If AvailablePlans(i).Selected Then list.Add(AvailablePlans(i).GetPlan)
            Next
            Return list
        End Get
    End Property
#End Region

    Private Sub NotifyPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(info))
    End Sub
    Private Sub OKButton_Click(sender As Object, e As RoutedEventArgs) Handles OKButton.Click
        If TBPlanName.Text = "" Then MsgBox("You did not define a plan name.") : Exit Sub
        If TBPlanName.Text.Length > 32 Then MsgBox("Your plan name cannot be greater than 32 characters") : Exit Sub
        For Each badChar As Char In System.IO.Path.GetInvalidFileNameChars
            If TBPlanName.Text.Contains(badChar) Then MsgBox("Invalid character in file name.") : Exit Sub
        Next
        'If CmbWatershed.SelectedIndex = -1 Then MsgBox("You did not select a watershed") : Exit Sub
        If CmbDamageReach.SelectedIndex = -1 Then MsgBox("You did not select an impact area set") : Exit Sub
        If CmbStructureInventory.SelectedIndex = -1 Then MsgBox("You did not select a structure inventory") : Exit Sub
        If IsNothing(GetSelectedPlans) OrElse GetSelectedPlans.Count = 0 Then MsgBox("You did not select a hydraulic plan") : Exit Sub
        DialogResult = True
        Me.Close()
    End Sub
    Private Sub CancelButton_Click(sender As Object, e As RoutedEventArgs) Handles CancelButton.Click
        DialogResult = False
        Me.Close()
    End Sub
    Private Sub TxtName_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles TBPlanName.PreviewKeyDown
        Dim tb As TextBox = TryCast(sender, TextBox)

        If tb.Text.Length >= 33 Then
            If e.Key = Key.Back OrElse e.Key = Key.Delete Then
                If tb.Text.Length - tb.SelectedText.Length <= 32 Then
                    tb.Foreground = Brushes.Black
                    tb.ToolTip = ""
                ElseIf tb.Text.Length = 33 AndAlso tb.SelectedText.Length = 0 Then
                    tb.Foreground = Brushes.Black
                    tb.ToolTip = ""
                End If

            End If
        Else

        End If
    End Sub

    Private Sub TxtName_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TBPlanName.PreviewTextInput
        Dim tb As TextBox = TryCast(sender, TextBox)
        Dim newtext As String = tb.Text & e.Text
        If newtext.Length >= 33 Then
            'e.Handled = True
            tb.Foreground = Brushes.Red
            tb.ToolTip = "Name must be less than or equal to 32 characters"
        Else
            tb.Foreground = Brushes.Black
            tb.ToolTip = ""
        End If
    End Sub
End Class
