Public Class Logger
    Private _logfile As String
    Public Event LogCleared()
    Sub New(ByVal file As String)
        InitializeComponent()
        _logfile = file

        Dim sr As New System.IO.StreamReader(_logfile)
        TheLog.Document.Blocks.Clear()
        Dim p As New System.Windows.Documents.Paragraph
        p.Inlines.Add(sr.ReadToEnd)
        'p.Inlines.Last.Foreground = New System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0))
        TheLog.Document.Blocks.Add(p)
        TheLog.ScrollToEnd()
        sr.Close() : sr.Dispose()
    End Sub
    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs) Handles CloseButton.Click
        Me.Close()
    End Sub

    Private Sub ClearButton_Click(sender As Object, e As RoutedEventArgs) Handles ClearButton.Click
        'delete the log file contents.
        Dim fs As New System.IO.FileStream(_logfile, IO.FileMode.Create)
        fs.Close() : fs.Dispose()
        RaiseEvent LogCleared()
        TheLog.Document.Blocks.Clear()
        'Dim sw As New System.IO.StreamWriter(fs)
        'sw.
    End Sub
End Class
