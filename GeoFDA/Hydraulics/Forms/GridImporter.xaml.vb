Public Class GridImporter
    Implements System.ComponentModel.INotifyPropertyChanged
    Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Private _AvailableFiles As System.Collections.ObjectModel.ObservableCollection(Of FilePathAndName)
    Private _SelectedFiles As System.Collections.ObjectModel.ObservableCollection(Of FilePathAndName)
    Private _Terrains As System.Collections.ObjectModel.ObservableCollection(Of TerrainChildTreeNode)
    Private _Years As System.Collections.ObjectModel.ObservableCollection(Of Integer)
    Sub New(ByVal StartupName As String, ByVal terrainlist As System.Collections.ObjectModel.ObservableCollection(Of TerrainChildTreeNode), ByVal baseyear As Integer, ByVal futureyear As Integer)

        ' This call is required by the designer.
        InitializeComponent()
        TxtName.Text = StartupName
        'If watershedlist.Count = 0 Then Throw New ArgumentException("There are no watersheds in this study")
        Terrains = terrainlist
        ' Add any initialization after the InitializeComponent() call.
        setcontextmenu()
        Dim tmpyears As New System.Collections.ObjectModel.ObservableCollection(Of Integer)
        tmpyears.Add(baseyear)
        If Not futureyear = 0 Then tmpyears.Add(futureyear)
        Years = tmpyears
    End Sub
#Region "properties"
    Public ReadOnly Property GetUniqueName As String
        Get
            Return TxtName.Text
        End Get
    End Property
    Public ReadOnly Property GetGrids As String()
        Get
            '' ugg
            Dim grids(7) As String
            For i = 0 To SelectedFiles.Count - 1
                grids(i) = SelectedFiles(i).FilePath
            Next
            Return grids
        End Get
    End Property
    Public ReadOnly Property GetProbabilities As Single()
        Get
            '' ugg
            Dim probs(7) As Single
            For i = 0 To SelectedFiles.Count - 1
                probs(i) = CSng(SelectedFiles(i).Probability)
            Next
            Return probs
        End Get
    End Property
    Public Property AvailableFiles As System.Collections.ObjectModel.ObservableCollection(Of FilePathAndName)
        Get
            Return _AvailableFiles
        End Get
        Set(value As System.Collections.ObjectModel.ObservableCollection(Of FilePathAndName))
            _AvailableFiles = value
            NotifyPropertyChanged("AvailableFiles")
        End Set
    End Property
    Public Property SelectedFiles As System.Collections.ObjectModel.ObservableCollection(Of FilePathAndName)
        Get
            If Not IsNothing(_SelectedFiles) Then
                Return New System.Collections.ObjectModel.ObservableCollection(Of FilePathAndName)(_SelectedFiles.OrderByDescending(Function(fpn) fpn.Probability).ThenBy(Function(fpn) fpn.FileName))
            Else
                Return Nothing
            End If
        End Get
        Set(value As System.Collections.ObjectModel.ObservableCollection(Of FilePathAndName))
            _SelectedFiles = value
            NotifyPropertyChanged("SelectedFiles")
        End Set
    End Property
    Public Property Terrains As System.Collections.ObjectModel.ObservableCollection(Of TerrainChildTreeNode)
        Get
            Return _Terrains
        End Get
        Set(value As System.Collections.ObjectModel.ObservableCollection(Of TerrainChildTreeNode))
            _Terrains = value
            NotifyPropertyChanged("Terrains")
        End Set
    End Property
    Public Property Years As System.Collections.ObjectModel.ObservableCollection(Of Integer)
        Get
            Return _Years
        End Get
        Set(value As System.Collections.ObjectModel.ObservableCollection(Of Integer))
            _Years = value
            NotifyPropertyChanged("Years")
        End Set
    End Property
#End Region

    Private Sub setcontextmenu()
        AddHandler TxtDirectory.SelectionMade, AddressOf DirectorySelected
    End Sub
    Private Sub DirectorySelected(ByVal fullpath As String)
        '_AvailableFiles = New System.Collections.ObjectModel.ObservableCollection(Of FilePathAndName)
        Dim thefiles As New System.Collections.ObjectModel.ObservableCollection(Of FilePathAndName)
        Dim theFltFiles As New System.Collections.ObjectModel.ObservableCollection(Of String)
        Dim theVrtFiles As New System.Collections.ObjectModel.ObservableCollection(Of String)
        Dim filelist() As String = System.IO.Directory.GetFiles(fullpath)
        For i = 0 To filelist.Count - 1
            If System.IO.Path.GetExtension(filelist(i)) = ".tif" Then thefiles.Add(New FilePathAndName(filelist(i), ""))
            If System.IO.Path.GetExtension(filelist(i)) = ".flt" Then theFltFiles.Add(filelist(i))
            If System.IO.Path.GetExtension(filelist(i)) = ".vrt" Then theVrtFiles.Add(filelist(i))
        Next
        Dim vrtreaders As New List(Of LifeSimGIS.VRTReader)
        Dim usingvrt As Boolean = False
        If theVrtFiles.Count > 0 Then
            If theVrtFiles.Count > 8 Then
                thefiles = New System.Collections.ObjectModel.ObservableCollection(Of FilePathAndName)
                theFltFiles = New System.Collections.ObjectModel.ObservableCollection(Of String)
                For Each s As String In theVrtFiles
                    thefiles.Add(New FilePathAndName(s, ""))
                    Dim tmpvrt As New LifeSimGIS.VRTReader(s, False)
                    vrtreaders.Add(tmpvrt)
                    For Each str As String In tmpvrt.RasterFiles
                        If System.IO.Path.GetExtension(str) = ".tif" Then
                            'no conversion necessary?
                        End If
                        If System.IO.Path.GetExtension(str) = ".flt" Then
                            theFltFiles.Add(str)
                        End If

                    Next
                Next
                If theFltFiles.Count > 0 Then MsgBox("You are using .vrt files with .flt files inside them, please convert those to .tif prior to importing into GeoFDA.") : Exit Sub
                usingvrt = True
            Else
                MsgBox("There are .vrt files present, but not 8, and it is too difficult to sort out which grids you want to use, so we respectfully request that you provide either 8 .tif files, or 8 .vrt files which point to .tif files...")
            End If
        Else
            If (thefiles.Count + theFltFiles.Count) < 8 Then
                MsgBox("There are not 8 files with the extension of *.tif or *.flt in this directory")
                Exit Sub
            End If
        End If


        If theFltFiles.Count > 0 Then

            If MsgBox("There are " & theFltFiles.Count.ToString & " files in this directory with the extension of *.flt. GeoFDA requires *.tif files. Would you like to convert them to .tif?", _
                                  MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                'convert the flt files
                For i = 0 To theFltFiles.Count - 1
                    LifeSimGIS.GDALUtils.ConvertToTilePyramidCompressedTiff(theFltFiles(i), System.IO.Path.ChangeExtension(theFltFiles(i), "tif"), True, True, True)
                Next
                'now that we have converted the .tif files, collect all .tif files

                filelist = System.IO.Directory.GetFiles(fullpath)
                If Not usingvrt Then
                    thefiles.Clear()
                    For i = 0 To filelist.Count - 1
                        If System.IO.Path.GetExtension(filelist(i)) = ".tif" Then thefiles.Add(New FilePathAndName(filelist(i), ""))
                    Next
                Else
                    ''currently this should never happen
                    Exit Sub
                End If
            End If
        End If

        If thefiles.Count < 8 Then
            MsgBox("There are not 8 files with the extention *.tif in this directory")
            '_AvailableFiles.Clear()
            Exit Sub
        End If
        AvailableFiles = thefiles
    End Sub
    Private Sub NotifyPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(info))
    End Sub
#Region "SelectAndDeSelect"
    Private Sub CmdDeSelectOne_Click(sender As Object, e As RoutedEventArgs) Handles CmdDeSelectOne.Click
        Dim tmplist As New System.Collections.ObjectModel.ObservableCollection(Of FilePathAndName)
        If Not IsNothing(_AvailableFiles) Then
            For i = 0 To _AvailableFiles.Count - 1
                tmplist.Add(_AvailableFiles(i))
            Next

            If Not MyWPFDGV.SelectedIndex = -1 Then tmplist.Add(SelectedFiles.Item(MyWPFDGV.SelectedIndex))
        End If

        AvailableFiles = tmplist
        Dim tmplist2 As New System.Collections.ObjectModel.ObservableCollection(Of FilePathAndName)
        tmplist2 = SelectedFiles
        If IsNothing(tmplist2) OrElse tmplist2.Count = 0 Then
            MsgBox("There are no files to remove.")

        ElseIf tmplist2.Count > 0 And MyWPFDGV.SelectedIndex = -1 Then
            tmplist.Add(tmplist2.Last)
            tmplist2.RemoveAt(tmplist2.Count - 1)
            AvailableFiles = tmplist
        ElseIf tmplist2.Count > 0 And MyWPFDGV.SelectedIndex <> -1 Then
            tmplist2.RemoveAt(MyWPFDGV.SelectedIndex)
        End If

        SelectedFiles = tmplist2
        'UpdateProbabilitiesBasedOnSelections()
    End Sub
    Private Sub CmdSelectOne_Click(sender As Object, e As RoutedEventArgs) Handles CmdSelectOne.Click
        Dim tmplist As New System.Collections.ObjectModel.ObservableCollection(Of FilePathAndName)
        Dim tmplist2 As New System.Collections.ObjectModel.ObservableCollection(Of FilePathAndName)
        tmplist2 = AvailableFiles

        If Not IsNothing(_SelectedFiles) Then
            tmplist = SelectedFiles
        End If

        If IsNothing(AvailableFiles) OrElse AvailableFiles.Count = 0 Then  'no available files to add
            MsgBox("There are no files to add.")
        ElseIf AvailableGrids.SelectedIndex = -1 Then                     'there are available files but no selection was made. add the first file
            tmplist.Add(AvailableFiles.Item(0))
            'SelectedFiles = tmplist
            tmplist2.RemoveAt(0)
        ElseIf AvailableGrids.SelectedIndex > -1 Then                        'there are available files and a selection was made
            tmplist.Add(AvailableFiles.Item(AvailableGrids.SelectedIndex))
            tmplist2.RemoveAt(AvailableGrids.SelectedIndex)
        End If

        SelectedFiles = tmplist
        AvailableFiles = tmplist2

        'UpdateProbabilitiesBasedOnSelections()
    End Sub
    Private Sub CmdSelectAll_Click(sender As Object, e As RoutedEventArgs) Handles CmdSelectAll.Click
        If IsNothing(AvailableFiles) Then
            MsgBox("There are no files to add.")
        ElseIf Not IsNothing(SelectedFiles) AndAlso SelectedFiles.Count <> 0 Then
            Dim tmplist As New System.Collections.ObjectModel.ObservableCollection(Of FilePathAndName)
            For i = 0 To _SelectedFiles.Count - 1
                tmplist.Add(_SelectedFiles(i))
            Next
            For i = 0 To _AvailableFiles.Count - 1
                tmplist.Add(_AvailableFiles(i))
            Next
            SelectedFiles = tmplist
            AvailableFiles = Nothing
        Else
            SelectedFiles = AvailableFiles
            AvailableFiles = Nothing
        End If
        'UpdateProbabilitiesBasedOnSelections()
    End Sub
    Private Sub CmdDeSelectAll_Click(sender As Object, e As RoutedEventArgs) Handles CmdDeSelectAll.Click
        If IsNothing(SelectedFiles) Then
            MsgBox("There are no files to remove.")
        ElseIf Not IsNothing(AvailableFiles) AndAlso AvailableFiles.Count <> 0 Then
            Dim tmplist As New System.Collections.ObjectModel.ObservableCollection(Of FilePathAndName)
            For i = 0 To _AvailableFiles.Count - 1
                tmplist.Add(_AvailableFiles(i))
            Next
            For i = 0 To _SelectedFiles.Count - 1
                tmplist.Add(_SelectedFiles(i))
            Next
            AvailableFiles = tmplist
            SelectedFiles = Nothing
        Else
            AvailableFiles = SelectedFiles
            SelectedFiles = Nothing

        End If

        'UpdateProbabilitiesBasedOnSelections()
    End Sub
#End Region
    Private Sub CMDOk_Click(sender As Object, e As RoutedEventArgs) Handles CMDOk.Click
        'If SelectedGrids.Items.Count <> 8 Then MsgBox("You have not selected 8 grid paths, please select 8 paths before you click OK.") : Exit Sub
        If MyWPFDGV.Items.Count <> 8 Then MsgBox("You have not selected 8 grid paths, please select 8 paths before you click OK.") : Exit Sub
        If _SelectedFiles.Where(Function(a) a.IsValid = False).Count > 0 Then MsgBox("One or more of your probability values are not valid") : Exit Sub
        If CmbTerrain.SelectedIndex = -1 Then MsgBox("You have not selected a terrain, please select a terrain before you click OK.") : Exit Sub
        If RDBDepth.IsChecked = RDBWSE.IsChecked Then MsgBox("You must select either Depth Grids or Water Surface Elevations") : Exit Sub
        If TxtName.Text = "" Then MsgBox("You have not defined a name for this grid set, please define a name before you click OK.") : Exit Sub
        If TxtName.Text.Length > 32 Then MsgBox("Your plan name must be equal to or less than 32 characters") : Exit Sub
        For Each badChar As Char In System.IO.Path.GetInvalidFileNameChars
            If TxtName.Text.Contains(badChar) Then MsgBox("Invalid character in file name.") : Exit Sub
        Next
        DialogResult = True
        Me.Close()
    End Sub
    Private Sub ValidateProbability(sender As Object, e As System.Windows.Controls.TextChangedEventArgs)
        Dim TBox As TextBox = CType(sender, TextBox)
        Dim SingleValue As Single
        If Single.TryParse(TBox.Text, SingleValue) = False Then
            If TBox.Text = "." Then Exit Sub
        Else
            'If SingleValue >= 0 AndAlso SingleValue <= 1 Then Exit Sub
        End If

    End Sub
    Private Sub ValidateProbabilityInput(sender As Object, e As System.Windows.Input.TextCompositionEventArgs)
        Dim TBox As TextBox = CType(sender, TextBox)
        'MsgBox(e.Handled.ToString)
        Dim SingleValue As Single
        If Single.TryParse(TBox.Text & e.Text, SingleValue) = False Then
            If e.Text = "." AndAlso TBox.Text.Contains(".") = False Then Exit Sub 'allow one decimal
            e.Handled = True
        Else
            If e.Text = "," Then e.Handled = True
            'If SingleValue >= 0 AndAlso SingleValue <= 1 Then Exit Sub
            'e.Handled = True
        End If
    End Sub
    Private Sub UpdateCaretView(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim TBox As TextBox = CType(sender, TextBox)
        If TBox.CaretBrush Is System.Windows.Media.Brushes.White Then TBox.CaretBrush = System.Windows.Media.Brushes.Black
    End Sub
    Private Sub CMDClose_Click(sender As Object, e As RoutedEventArgs) Handles CMDClose.Click
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
