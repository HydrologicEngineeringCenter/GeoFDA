Public Class Rename
    Implements System.ComponentModel.IDataErrorInfo
    Private _NewName As String
    Private _charMax As Integer = -1
    Private _hasError As Boolean = False
    Public Property NewName As String
        Get
            Return _NewName
        End Get
        Set(value As String)
            _NewName = value
        End Set
    End Property
    Sub New(ByVal oldname As String)
        NewName = oldname
        InitializeComponent()
    End Sub
    Sub New(ByVal oldname As String, charMax As Integer)
        NewName = oldname
        _charMax = charMax
        InitializeComponent()
    End Sub

    Private Sub CMDOk_Click(sender As Object, e As RoutedEventArgs) Handles CMDOk.Click
        If NewName = "" Then MsgBox("You cannot have a blank name.") : Exit Sub
        For Each badChar As Char In System.IO.Path.GetInvalidFileNameChars
            If NewName.Contains(badChar) Then MsgBox("Invalid character in file name.") : Exit Sub
        Next

        If _hasError Then
            Exit Sub
        ElseIf _charMax <> -1 AndAlso TxtName.Text.Length > _charMax Then 'this case is only to handle the rare case that the user is renaming an impact area and is using a name that is 31 or 32 chars long and the name already exists. Clicking "Ok" will automatically append "_1" which will put it over the 32 limit 
            MsgBox("Name cannot be greater than " & _charMax & " characters. Currently: " & TxtName.Text.Length, MsgBoxStyle.Exclamation, "Name Too Long")                            'but will not turn the txtbox red because it is not an edit session. The user would have been allowed to click OK.
            Exit Sub
        End If
       
        DialogResult = True
        Me.Close()
    End Sub

    Private Sub CMDClose_Click(sender As Object, e As RoutedEventArgs) Handles CMDClose.Click
        DialogResult = False
        Me.Close()
    End Sub

    Public ReadOnly Property [Error] As String Implements ComponentModel.IDataErrorInfo.Error
        Get
            Throw New NotImplementedException
        End Get
    End Property

    Default Public ReadOnly Property Item(columnName As String) As String Implements ComponentModel.IDataErrorInfo.Item
        Get
            If columnName.Equals("NewName") Then
                If _charMax <> -1 AndAlso TxtName.Text.Length > _charMax Then
                    _hasError = True
                    Return "Name cannot be greater than " & _charMax & " characters. Currently: " & TxtName.Text.Length
                Else
                    _hasError = False
                    Return Nothing
                End If
            Else
                Return Nothing
            End If
        End Get
    End Property
End Class
