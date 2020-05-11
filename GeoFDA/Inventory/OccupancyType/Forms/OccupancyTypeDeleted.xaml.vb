Public Class OccupancyTypeDeleted
    Private _Message As String
    Private _RemainingOcctypes As List(Of Consequences_Assist.ComputableObjects.OccupancyType)
    Public ReadOnly Property Message As String
        Get
            Return _Message
        End Get
    End Property
    Public Property RemainingOccupancyTypes As List(Of Consequences_Assist.ComputableObjects.OccupancyType)
        Get
            Return _RemainingOcctypes
        End Get
        Set(value As List(Of Consequences_Assist.ComputableObjects.OccupancyType))
            _RemainingOcctypes = value
        End Set
    End Property
    Sub New(ByVal msg As String, ByVal remainocctypes As List(Of Consequences_Assist.ComputableObjects.OccupancyType))
        _Message = msg
        RemainingOccupancyTypes = remainocctypes
        InitializeComponent()
        If CmbRemainingDamCats.HasItems Then CmbRemainingDamCats.SelectedIndex = 0
    End Sub
    Private Sub CMDOk_Click(sender As Object, e As RoutedEventArgs) Handles CMDOk.Click
        'ok
        If CmbRemainingDamCats.SelectedIndex = -1 Then MsgBox("you have not made a selection for an occupancy type to replace the one being deleted.")
        DialogResult = True
    End Sub

    Private Sub CMDCancel_Click(sender As Object, e As RoutedEventArgs) Handles CMDCancel.Click
        'not ok.
        DialogResult = False
    End Sub
End Class
