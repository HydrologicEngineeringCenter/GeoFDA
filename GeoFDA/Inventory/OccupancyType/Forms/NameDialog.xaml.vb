Public Class NameDialog
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Private Sub OKButton_Click(sender As Object, e As System.Windows.RoutedEventArgs)
        Me.DialogResult = True
        Me.Close()
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As System.Windows.RoutedEventArgs)
        Me.DialogResult = False
        Me.Close()
    End Sub

    Private Sub NameDialog_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        NameTextBox.Focus()
        NameTextBox.CaretIndex = NameTextBox.Text.Length
    End Sub
End Class
