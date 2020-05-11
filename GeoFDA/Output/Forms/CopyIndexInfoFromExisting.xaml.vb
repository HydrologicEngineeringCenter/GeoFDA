Public Class CopyIndexInfoFromExisting
    Private _options As List(Of String)
    Public Property Options As List(Of String)
        Get
            Return _options
        End Get
        Set(value As List(Of String))
            _options = value
        End Set
    End Property
    Sub New(ByVal opts As List(Of String))
        Options = opts
        InitializeComponent()
    End Sub
    Private Sub CMDOk_Click(sender As Object, e As RoutedEventArgs) Handles CMDOk.Click
        If AvailableOptions.SelectedIndex = -1 Then MsgBox("You have not made a selection.") : Exit Sub
        DialogResult = True
        Me.Close()
    End Sub

    Private Sub CMDCancel_Click(sender As Object, e As RoutedEventArgs) Handles CMDCancel.Click
        Me.Close()
    End Sub
End Class
