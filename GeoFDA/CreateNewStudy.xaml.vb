Public Class CreateNewStudy
    Private _path As String
    Private _name As String
    Private _UserPressedOK As Boolean = False
    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Sub New(ByVal MenuTitle As String, ByVal BrowserTitle As String, Optional ByVal xmlpath As String = Nothing)
        InitializeComponent()
        If Not IsNothing(xmlpath) Then hb.xmlPATH = xmlpath
        Me.Title = MenuTitle
        StudyPathBrowser.Title = BrowserTitle
    End Sub
    Private Sub closewindow(sender As System.Object, e As System.Windows.RoutedEventArgs)
        Me.Close()
    End Sub
    Public ReadOnly Property UserPressedOk As Boolean
        Get
            Return _UserPressedOK
        End Get
    End Property
    Public Property StudyName As String
        Set(value As String)
            _name = value
        End Set
        Get
            Return _name
        End Get
    End Property
    Public Property StudyPath As String
        Set(value As String)
            _path = value
        End Set
        Get
            Return _path
        End Get
    End Property
    Private Sub Ok(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles CmdOk.Click
        'create any necessary directories, and save a study file
        If StudyPathBrowser.Path = "" Then MsgBox("You did not select an output location") : Exit Sub
        If TxtName.Text = "" Then MsgBox("A name for the study was not entered.") : Exit Sub
        If TxtName.Text.Length > 32 Then MsgBox("Your study name must be equal to or less than 32 characters") : Exit Sub
        For Each badChar As Char In System.IO.Path.GetInvalidFileNameChars
            If TxtName.Text.Contains(badChar) Then MsgBox("Invalid character in file name.") : Exit Sub
        Next
        _name = TxtName.Text
        _path = StudyPathBrowser.Path
        _UserPressedOK = True
        Me.Close()
    End Sub
    Private Sub CloseME(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles CmdClose.Click
        Me.Close()
    End Sub
    Private Sub TxtName_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles TxtName.PreviewKeyDown
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

    Private Sub TxtName_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TxtName.PreviewTextInput
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
