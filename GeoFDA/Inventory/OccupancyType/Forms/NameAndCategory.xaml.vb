Public Class NameAndCategory
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Private Sub OKButton_Click(sender As Object, e As System.Windows.RoutedEventArgs)
        If NameTextBox.Text = "" Then MsgBox("You did not define an Occupancy Type name.") : Exit Sub
        If DamCatTextBox.Text = "" Then MsgBox("You did not define a Damage Category name.") : Exit Sub
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
