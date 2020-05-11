Public Class ImportFromHAZUS
    Private _outputDirectory As String
    Private _monetaryUnit As String
    Public Event ReportMessage(ByVal message As String)
    Private _IsInComputeMode As Boolean = False
    Private _BW As System.ComponentModel.BackgroundWorker
    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        '_outputDirectory = outputdirectory
        Dim validshapetypes As New List(Of LifeSimGIS.ShapefileReader.ShapeTypeEnumerable)
        validshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.Polygon)
        validshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PolygonM)
        validshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PolygonZM)
        CBTextBox.ValidShapeTypes = validshapetypes
    End Sub
    Public Sub SetValidShapes(ByVal validshapes As List(Of String))
        For i = 0 To validshapes.Count - 1
            CBTextBox.AddItem(validshapes(i))
        Next
    End Sub
    Public Sub SetOutputDirectory(ByVal outputdirectory As String)
        _outputDirectory = outputdirectory
    End Sub
    Sub New(ByVal outputdirectory As String, ByVal validshapes As List(Of String), ByVal outputmonetaryunit As String)
        InitializeComponent()
        _outputDirectory = outputdirectory
        _monetaryUnit = outputmonetaryunit
        Dim validshapetypes As New List(Of LifeSimGIS.ShapefileReader.ShapeTypeEnumerable)
        validshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.Polygon)
        validshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PolygonM)
        validshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PolygonZM)
        CBTextBox.ValidShapeTypes = validshapetypes
        For i = 0 To validshapes.Count - 1
            CBTextBox.AddItem(validshapes(i))
        Next
    End Sub
    Public ReadOnly Property GetStructurePath As String
        Get
            Return _outputDirectory & "\" & NameTextBox.Text & ".shp"
        End Get
    End Property
    Private Sub BndryGbsButton_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        Dim OpenfileDialog As New Microsoft.Win32.OpenFileDialog
        OpenfileDialog.Filter = "bndrygbs (*.mdb) |*.mdb"
        Dim Inputfile As String = ""
        If OpenfileDialog.ShowDialog() = True Then Inputfile = OpenfileDialog.FileName.ToString
        If Inputfile = "" Then MsgBox("No file was selected") : Exit Sub
        If System.IO.Path.GetFileNameWithoutExtension(Inputfile) <> "bndrygbs" Then MsgBox("The selected mdb was not titled bndrygrbs") : Exit Sub
        BndryGbsTextBox.Text = Inputfile
        If System.IO.File.Exists(System.IO.Path.GetDirectoryName(Inputfile) & "/flVeh.mdb") Then flVehTextBox.Text = System.IO.Path.GetDirectoryName(Inputfile) & "/flVeh.mdb"
        If System.IO.File.Exists(System.IO.Path.GetDirectoryName(Inputfile) & "/MSH.mdb") Then MSHTextBox.Text = System.IO.Path.GetDirectoryName(Inputfile) & "/MSH.mdb"
    End Sub

    Private Sub flVehButton_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        Dim OpenfileDialog As New Microsoft.Win32.OpenFileDialog
        OpenfileDialog.Filter = "flVeh (*.mdb) |*.mdb"
        Dim Inputfile As String = ""
        If OpenfileDialog.ShowDialog() = True Then Inputfile = OpenfileDialog.FileName.ToString
        If Inputfile = "" Then MsgBox("No file was selected") : Exit Sub
        If System.IO.Path.GetFileNameWithoutExtension(Inputfile) <> "flVeh" Then MsgBox("The selected mdb was not titled flVeh") : Exit Sub
        flVehTextBox.Text = Inputfile
    End Sub

    Private Sub MSHButton_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        Dim OpenfileDialog As New Microsoft.Win32.OpenFileDialog
        OpenfileDialog.Filter = "MSH (*.mdb) |*.mdb"
        Dim Inputfile As String = ""
        If OpenfileDialog.ShowDialog() Then Inputfile = OpenfileDialog.FileName.ToString
        If Inputfile = "" Then MsgBox("No file was selected") : Exit Sub
        If System.IO.Path.GetFileNameWithoutExtension(Inputfile) <> "MSH" Then MsgBox("The selected mdb was not titled MSH") : Exit Sub
        MSHTextBox.Text = Inputfile
    End Sub
    Private Sub CancelButton_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        'figure out how to cancel the background worker?
        Me.Close()
    End Sub
    'Private Sub FoundationHeightTextBox_PreviewTextInput(sender As System.Object, e As System.Windows.Input.TextCompositionEventArgs) Handles FoundationHeightTextBox.PreviewTextInput
    '    Dim Text As TextBox = CType(sender, TextBox)
    '    If Not Char.IsDigit(CChar(e.Text)) Then e.Handled = True 'numeric only
    '    If e.Text = Chr(8) Then e.Handled = False 'allow Backspace
    '    If e.Text = " " Then e.Handled = True 'don't allow spaces
    '    If e.Text = "-" And Text.SelectionStart = 0 And Text.Text.IndexOf("-") = -1 Then e.Handled = False 'allow negative
    '    If e.Text = "." And Text.Text.IndexOf(".") = -1 Then e.Handled = False 'allow one decimal
    '    'If e.KeyChar = Chr(13) Then OK.Focus() 'enter to move to next
    'End Sub
    Private Sub OKButton_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        If NameTextBox.Text = "" Then
            MsgBox("No name was entered.")
            Exit Sub
        ElseIf CBTextBox.GetSelectedItemPath = "" Then
            MsgBox("No Study Area Shapefile was defined.")
            Exit Sub
        ElseIf BndryGbsTextBox.Text = "" Then
            MsgBox("No HAZUS population database was defined.")
            Exit Sub
        ElseIf flVehTextBox.Text = "" Then
            MsgBox("No HAZUS vehicle database was defined.")
            Exit Sub
        ElseIf MSHTextBox.Text = "" Then
            MsgBox("No HAZUS construction database was defined.")
            Exit Sub
            'ElseIf FoundationHeightTextBox.Text = "" Then
            '    MsgBox("No foundation height was entered.")
            '    Exit Sub
        End If

        For Each badChar As Char In System.IO.Path.GetInvalidFileNameChars
            If NameTextBox.Text.Contains(badChar) Then MsgBox("Invalid character in file name.") : Exit Sub
        Next

        If System.IO.Path.GetFileNameWithoutExtension(BndryGbsTextBox.Text) <> "bndrygbs" Then
            MsgBox("HAZUS population data file must be named bndrygbs.mdb.", MsgBoxStyle.Information, "Incorrect file name")
            Exit Sub
        End If

        If System.IO.Path.GetFileNameWithoutExtension(flVehTextBox.Text) <> "flVeh" Then
            MsgBox("HAZUS vehicle data file must be named flVeh.mdb.", MsgBoxStyle.Information, "Incorrect file name")
            Exit Sub
        End If

        If System.IO.Path.GetFileNameWithoutExtension(MSHTextBox.Text) <> "MSH" Then
            MsgBox("HAZUS construction type data file must be named MSH.mdb.", MsgBoxStyle.Information, "Incorrect file name")
            Exit Sub
        End If

        'Dim FoundationHeight As Single
        'If Single.TryParse(FoundationHeightTextBox.Text, FoundationHeight) = False Then
        '    MsgBox("Foundation height entered is not a valid number.")
        '    Exit Sub
        'End If

        'SI Directory
        If System.IO.Directory.Exists(_outputDirectory) = False Then
            System.IO.Directory.CreateDirectory(_outputDirectory)
        End If

        'Check to see if the output file already exists.

        If System.IO.File.Exists(GetStructurePath) Then
            MsgBox("A file already exists named: " & NameTextBox.Text & " in the directory: " & _outputDirectory & ", please rename this inventory, or delete the previous inventory.") : Exit Sub
        End If

        Dim hz As New Consequences_Assist.HAZUSToStructures()
        Dim c As New Consequences_Assist.CreateSIArgs
        c.StudyAreaShapefile = CBTextBox.GetSelectedItemPath
        c.BndryGrbsMDBPath = BndryGbsTextBox.Text
        c.MSHMDBPath = MSHTextBox.Text
        c.VEHMDBPath = flVehTextBox.Text
        c.OutputDest = GetStructurePath

        _BW = New System.ComponentModel.BackgroundWorker
        _BW.WorkerReportsProgress = True
        _BW.WorkerSupportsCancellation = True
        AddHandler _BW.ProgressChanged, AddressOf ReportProgress
        AddHandler _BW.DoWork, AddressOf hz.CreateSI
        'AddHandler cancelBW, AddressOf BW.CancelAsync
        AddHandler _BW.RunWorkerCompleted, AddressOf ProgressComplete
        AddHandler hz.ReportErrorMessage, AddressOf LogFile
        'AddHandler hz.ReportProgressMessage, AddressOf ProgressMessage
        MyBase.Cursor = Cursors.Wait
        _IsInComputeMode = True
        _BW.RunWorkerAsync(c)
        OKButton.IsEnabled = False
        CancelButton.IsEnabled = False
        ' hz.CreateSI(CBTextBox.GetSelectedItemPath, BndryGbsTextBox.Text, MSHTextBox.Text, flVehTextBox.Text, GetStructurePath)

        'DialogResult = True
        'MsgBox("Complete")
        'Me.Close()

    End Sub
    Private Sub LogFile(ByVal messagetoadd As String, ByVal structureinventoryname As String)
        ' RaiseEvent ReportMessage(messagetoadd)
        Dim log As String = _outputDirectory & "\" & structureinventoryname & "_" & "HazusGenerator_LogFile.txt"

        Dim fs As System.IO.FileStream
        If Not System.IO.File.Exists(log) Then
            'create it
            fs = New System.IO.FileStream(log, IO.FileMode.Create)
        Else
            fs = New System.IO.FileStream(log, IO.FileMode.Append)
        End If
        Dim sr As New System.IO.StreamWriter(fs)
        sr.WriteLine(messagetoadd)
        sr.Close() : sr.Dispose()
        fs.Close() : fs.Dispose()
    End Sub

    Private Sub ReportProgress(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs)
        Progress.Value = e.ProgressPercentage
        ProgressLabel.Text = CType(e.UserState, String)
    End Sub
    Private Sub UpdateLabel(ByVal filename As String)
        ' = filename
    End Sub
    Private Sub ProgressComplete(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs)
        MyBase.Cursor = Nothing
        If e.Cancelled = False Then

            If _monetaryUnit <> "$'s" Then 'hazus is always in dollars.
                Dim dbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(GetStructurePath, ".dbf"))
                Dim sval As Double() = Array.ConvertAll(dbf.GetColumn("Val_Struct"), AddressOf Double.Parse)
                Dim cval As Double() = Array.ConvertAll(dbf.GetColumn("Val_Cont"), AddressOf Double.Parse)
                Dim oval As Double() = Array.ConvertAll(dbf.GetColumn("Val_Other"), AddressOf Double.Parse)
                If _monetaryUnit = "1,000$'s" Then
                    For i = 0 To sval.Count - 1
                        sval(i) /= 1000
                        cval(i) /= 1000
                        oval(i) /= 1000
                    Next
                Else 'mustbe millions
                    For i = 0 To sval.Count - 1
                        sval(i) /= 1000000
                        cval(i) /= 1000000
                        oval(i) /= 1000000
                    Next
                End If
                dbf.EditColumn("Val_Struct", sval)
                dbf.EditColumn("Val_Cont", cval)
                dbf.EditColumn("Val_Other", oval)
            End If
            MsgBox("complete")
            _IsInComputeMode = False
            DialogResult = True
            Me.Close()
        Else
            _IsInComputeMode = False
            MsgBox("Failure")
            DialogResult = False

        End If
    End Sub


    Private Sub ImportFromHAZUS_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles Me.Closing
        MyBase.Cursor = Nothing
        If _IsInComputeMode Then
            If MsgBox("A process is underway, would you like to wait for it to finish?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                e.Cancel = True
            Else
                _BW.CancelAsync()
            End If
        End If
    End Sub
End Class
