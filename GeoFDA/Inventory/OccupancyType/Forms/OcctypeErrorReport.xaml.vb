Public Class OcctypeErrorReport
    Private _Items As List(Of OcctypeErrorReportRowItem)
    Public Property Items As List(Of OcctypeErrorReportRowItem)
        Get
            Return _Items
        End Get
        Set(value As List(Of OcctypeErrorReportRowItem))
            _Items = value
        End Set
    End Property
    Sub New(ByVal errorlist As List(Of OcctypeErrorReportRowItem))
        Items = errorlist
        InitializeComponent()
    End Sub
    Private Sub CmdOk_Click(sender As Object, e As RoutedEventArgs) Handles CmdOk.Click
        Me.Close()
    End Sub

    Private Sub Errors_AutoGeneratingColumn(sender As Object, e As DataGridAutoGeneratingColumnEventArgs) Handles Errors.AutoGeneratingColumn
        If e.PropertyType = GetType(String) Then
            'go forth and do.
            e.Column.Header = e.PropertyName.Replace("_", " ")
            e.Column.Width = New DataGridLength(1, DataGridLengthUnitType.Star)
        Else
            e.Cancel = True
        End If
    End Sub
End Class
