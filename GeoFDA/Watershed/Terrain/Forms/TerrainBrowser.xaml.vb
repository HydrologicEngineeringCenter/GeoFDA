Public Class TerrainBrowser
    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Sub New(ByVal name As String, ByVal path As String)

        ' This call is required by the designer.
        InitializeComponent()
        TxtName.Text = name & "_1"
        TerrainPathBrowser.SetPath = path
        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Private Sub Ok(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles CmdOk.Click
        'create any necessary directories, and save a study file
        If TxtName.Text = "" Then MsgBox("You did not define a name") : Exit Sub
        If TerrainPathBrowser.Path = "" Then MsgBox("You did not select an input file.") : Exit Sub
        For Each badChar As Char In System.IO.Path.GetInvalidFileNameChars
            If TxtName.Text.Contains(badChar) Then MsgBox("Invalid character in file name.") : Exit Sub
        Next
        Dim finfo As System.IO.FileInfo = My.Computer.FileSystem.GetFileInfo(TerrainPathBrowser.Path)
        If CDbl(finfo.Length / 100000) > 200.0 Then MsgBox("File size exceeds limit of 2 GB.") : Exit Sub
        DialogResult = True
        Me.Close()
    End Sub
    Private Sub CloseME(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles CmdClose.Click
        Me.Close()
    End Sub
End Class
