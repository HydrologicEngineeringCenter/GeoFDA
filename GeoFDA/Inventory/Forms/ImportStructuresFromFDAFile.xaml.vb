Public Class ImportStructuresFromFDAFile
    Private _hasProjection As Boolean = False
    Private _CurrentDirectory As String
    Public ReadOnly Property HasProjection As Boolean
        Get
            Return _hasProjection
        End Get
    End Property
    Sub New(ByVal MonetaryUnit As String, ByVal currentDirectory As String)
        _CurrentDirectory = currentDirectory
        InitializeComponent()
        initializeCmbMonetaryUnits()

        CMBMonetaryUnits.SelectedItem = MonetaryUnit
    End Sub
    Private Sub CMDOk_Click(sender As Object, e As RoutedEventArgs) Handles CMDOk.Click
        If TxtName.Text = "" Then MsgBox("You did not provide a name for the strucutre invntory") : Exit Sub
        If TxtProjection.Text <> "" Then
            If System.IO.Path.GetExtension(TxtProjection.Text) <> ".prj" Then MsgBox("The file path does not end in the .prj file format") : Exit Sub
            _hasProjection = True
        End If
        DialogResult = True
        Me.Close()
    End Sub
    Sub initializeCmbMonetaryUnits()
        CmbMonetaryUnits.Items.Add("$'s")
        CmbMonetaryUnits.Items.Add("1,000$'s")
        CmbMonetaryUnits.Items.Add("1,000,000$'s")
    End Sub
    Private Sub CMDCancel_Click(sender As Object, e As RoutedEventArgs) Handles CMDCancel.Click
        DialogResult = False
        Me.Close()
    End Sub

    Private Sub Browse_Click(sender As Object, e As RoutedEventArgs) Handles Browse.Click
        Dim ofd As New Microsoft.Win32.OpenFileDialog
        With ofd
            .Title = "Please select an existing projection file"
            .Filter = "projection files (*.prj)|*.prj|All Files (*.*)|*.*"
            .CheckFileExists = True
            .Multiselect = False
            '.InitialDirectory = _CurrentDirectory
        End With

        If ofd.ShowDialog Then
            If Not ofd.FileName = "" Then
                If System.IO.Path.GetExtension(ofd.FileName) = ".prj" Then TxtProjection.Text = ofd.FileName
            Else
                MsgBox("No selection was made.")
            End If

        Else
            MsgBox("No Selection was made.")
        End If
    End Sub
End Class
