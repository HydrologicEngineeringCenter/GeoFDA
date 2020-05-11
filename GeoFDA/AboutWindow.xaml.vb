Public Class AboutWindow
    Private Sub MoveMe(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseLeftButtonDown
        DragMove()
    End Sub
    Private Sub CloseMe(sender As Object, e As RoutedEventArgs) Handles WhiteX.MouseDown
        Me.Close()
    End Sub
End Class
