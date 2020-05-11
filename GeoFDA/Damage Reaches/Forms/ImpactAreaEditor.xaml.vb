Public Class ImpactAreaEditor
    Implements System.ComponentModel.INotifyPropertyChanged
    Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Private _ImpactAreas As System.Collections.ObjectModel.ObservableCollection(Of FDA_Computation.ImpactArea)
    Private _Streams As System.Collections.ObjectModel.ObservableCollection(Of String)
    Private _Stat_types As System.Collections.ObjectModel.ObservableCollection(Of String)
    Sub New(ByVal impactareasinput As List(Of FDA_Computation.ImpactArea))

        ' This call is required by the designer.
        InitializeComponent()
        Dim testImpactAreas As New System.Collections.ObjectModel.ObservableCollection(Of FDA_Computation.ImpactArea)
        ' Add any initialization after the InitializeComponent() call.
        For i = 0 To impactareasinput.Count - 1
            testImpactAreas.Add(impactareasinput(i))
        Next
        ImpactAreas = testImpactAreas
        CmbImpactArea.SelectedIndex = 0
    End Sub
    Public Property ImpactAreas As System.Collections.ObjectModel.ObservableCollection(Of FDA_Computation.ImpactArea)
        Get
            Return _ImpactAreas
        End Get
        Set(value As System.Collections.ObjectModel.ObservableCollection(Of FDA_Computation.ImpactArea))
            _ImpactAreas = value
            NotifyPropertyChanged("ImpactAreas")
        End Set
    End Property
    Private Sub NotifyPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(info))
    End Sub
    Private Sub CMDOk_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles CMDOk.Click

        DialogResult = True
        Me.Close()
    End Sub
    Private Sub CMDClose_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles CMDClose.Click
        Me.Close()
    End Sub

    Private Sub RenameMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles RenameMenuItem.Click
        ''create a rename dialog.
        If CmbImpactArea.SelectedIndex = -1 Then MsgBox("Please select an impact area to rename") : Exit Sub
        Dim r As New Rename(ImpactAreas(CmbImpactArea.SelectedIndex).Name, 32)
        r.Owner = Me
        Dim nameconflicts As Boolean = True

        Do Until Not nameconflicts
            If r.ShowDialog Then
                nameconflicts = False
                For i = 0 To _ImpactAreas.Count - 1
                    If _ImpactAreas(i).Name = r.NewName Then
                        'name already exists
                        MsgBox("that name already exists, all names must be unique.")
                        r = New Rename(r.NewName & "_1", 32)
                        r.Owner = Me
                        nameconflicts = True : Exit For
                    End If
                Next
            Else
                ''user aborted. make no changes.
                Exit Sub
            End If
        Loop
        ImpactAreas(CmbImpactArea.SelectedIndex).Name = r.NewName
        Dim idx As Integer = CmbImpactArea.SelectedIndex
        CmbImpactArea.SelectedIndex = 0
        CmbImpactArea.SelectedIndex = idx

    End Sub
End Class
