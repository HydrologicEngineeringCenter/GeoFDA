Public Class ImpactAreaErrorReport
    Private _SN As ImpactAreaChildTreeNode
    Public ReadOnly Property SICTN As ImpactAreaChildTreeNode
        Get
            Return _SN
        End Get
    End Property

    Sub New(ByRef sn As ImpactAreaChildTreeNode)
        _SN = sn
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
