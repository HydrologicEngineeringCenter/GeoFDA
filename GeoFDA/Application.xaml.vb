Class Application

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.
    Private Sub AppStartup(sender As Object, e As StartupEventArgs)
        Dim mw As New MainWindow
        mw.Show()
        If e.Args.Length < 2 And e.Args.Length > 0 Then
            If System.IO.Path.GetExtension(e.Args(0)) = ".GeoFDA" Then
                mw.LoadFromFile(e.Args(0))
            Else
                MsgBox("File extention not recognized")
            End If
        ElseIf e.Args.Length = 0 Then

        Else
            Select Case e.Args(0)
                Case "Launch"
                    If System.IO.Path.GetExtension(e.Args(1)) = ".GeoFDA" Then
                        mw.LoadFromFile(e.Args(1))
                    Else
                        MsgBox("File extention not recognized")
                    End If
                Case "Recompute"

            End Select


        End If
    End Sub
End Class
