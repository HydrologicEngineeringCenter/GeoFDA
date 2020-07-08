Public Class ImportFromShapefile
    Implements System.ComponentModel.INotifyPropertyChanged
    Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Private _AttributeNames As Collections.ObjectModel.ObservableCollection(Of String)
    Private _NumericAttributeNames As Collections.ObjectModel.ObservableCollection(Of String)
    Private _dt As System.Data.DataTable
    Private _monetaryUnits As String
    Public Event RaiseError(ByVal message As String)
    Public Sub New()
        InitializeComponent()
    End Sub
    Public Sub New(ByVal validshapes As List(Of String), ByVal studymonetaryunits As String)
        InitializeComponent()
        Dim validshapetypes As New List(Of LifeSimGIS.ShapefileReader.ShapeTypeEnumerable)
        validshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.Point)
        validshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PointM)
        validshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PointZM)
        validshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.Polygon)
        validshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PolygonM)
        validshapetypes.Add(LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PolygonZM)
        CmbStructureInventoryPaths.ValidShapeTypes = validshapetypes
        For i = 0 To validshapes.Count - 1
            CmbStructureInventoryPaths.AddItem(validshapes(i))
        Next
        initializeCmbMonetaryUnits()
        _monetaryUnits = studymonetaryunits
        CmbMonetaryUnits.SelectedItem = studymonetaryunits
    End Sub
    Public Property AttributeNames As Collections.ObjectModel.ObservableCollection(Of String)
        Get
            Return _AttributeNames
        End Get
        Set(value As Collections.ObjectModel.ObservableCollection(Of String))
            _AttributeNames = value
            NotifyPropertyChanged("AttributeNames")
        End Set
    End Property
    Public Property NumericAttributeNames As Collections.ObjectModel.ObservableCollection(Of String)
        Get
            Return _NumericAttributeNames
        End Get
        Set(value As Collections.ObjectModel.ObservableCollection(Of String))
            _NumericAttributeNames = value
            NotifyPropertyChanged("NumericAttributeNames")
        End Set
    End Property
    Public ReadOnly Property GetDataTable As System.Data.DataTable
        Get
            Return _dt
        End Get
    End Property
    Public ReadOnly Property GetUniqueName As String
        Get
            Return NameTextBox.Text
        End Get
    End Property
    Public ReadOnly Property GetOriginalPath As String
        Get
            Return CmbStructureInventoryPaths.GetSelectedItemPath
        End Get
    End Property
    Sub initializeCmbMonetaryUnits()
        CmbMonetaryUnits.Items.Add("$'s")
        CmbMonetaryUnits.Items.Add("1,000$'s")
        CmbMonetaryUnits.Items.Add("1,000,000$'s")
    End Sub
    Private Sub NotifyPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(info))
    End Sub
    Private Sub SelectionMade(ByVal path As String) Handles CmbStructureInventoryPaths.CmbSelectionMade
        Dim ShpReader As New LifeSimGIS.ShapefileReader(path)
        If CheckShapeType(ShpReader) Then
            Dim tmpAttributeNames As New Collections.ObjectModel.ObservableCollection(Of String)
            Dim tmpNumAttributeNames As New Collections.ObjectModel.ObservableCollection(Of String)
            _AttributeNames = New Collections.ObjectModel.ObservableCollection(Of String)
            Dim DBReader As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(path, ".dbf"))
            Dim ColumnNames() As String = DBReader.ColumnNames
            Dim numericcols As List(Of String) = DBReader.GetNumericColumns
            tmpAttributeNames.Add("")
            For i As Int32 = 0 To ColumnNames.Count - 1
                tmpAttributeNames.Add(ColumnNames(i))
            Next
            AttributeNames = tmpAttributeNames
            tmpNumAttributeNames.Add("")
            For i As Int32 = 0 To numericcols.Count - 1
                tmpNumAttributeNames.Add(numericcols(i))
            Next
            NumericAttributeNames = tmpNumAttributeNames
            If DBReader.DataBaseOpen Then DBReader.Close()
        Else
            MsgBox("Selected shapefile is not a simple point shapefile, or a polygon shapefile.", MsgBoxStyle.Information, "Incorrect shapefile type")
        End If
    End Sub
    Private Function CheckShapeType(ByVal ShpReader As LifeSimGIS.ShapefileReader) As Boolean
        If ShpReader.ShapeType = LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.Point Then Return True
        If ShpReader.ShapeType = LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PointM Then Return True
        If ShpReader.ShapeType = LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PointZM Then Return True
        If ShpReader.ShapeType = LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.Polygon Then Return True
        If ShpReader.ShapeType = LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PolygonM Then Return True
        If ShpReader.ShapeType = LifeSimGIS.ShapefileReader.ShapeTypeEnumerable.PolygonZM Then Return True
        Return False
    End Function
    Private Sub CancelButton_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        DialogResult = False
        Me.Close()
    End Sub

    Private Sub OKButton_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'Verify Inputs
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        If NameTextBox.Text = "" Then
            MsgBox("A name for the structure inventory dataset was not entered.", MsgBoxStyle.Critical, "No Name Entered")
            Exit Sub
        End If
        For Each badChar As Char In System.IO.Path.GetInvalidFileNameChars
            If NameTextBox.Text.Contains(badChar) Then MsgBox("Invalid character in file name.") : Exit Sub
        Next

        If CmbStructureInventoryPaths.GetSelectedItemPath = "" Then MsgBox("You have not selected a path to a shapefile") : Exit Sub
        'get the dbf file.
        Dim dbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(CmbStructureInventoryPaths.GetSelectedItemPath, ".dbf"))

        'Radio Buttons
        Dim UseFFE(dbf.NumberOfRows - 1) As Boolean
        Dim UseDBFGE(dbf.NumberOfRows - 1) As Boolean

        '
        Dim Names(dbf.NumberOfRows - 1) As String
        Dim DamCats(dbf.NumberOfRows - 1) As String
        Dim Occtypes(dbf.NumberOfRows - 1) As String
        Dim GE(dbf.NumberOfRows - 1) As Single
        Dim FH(dbf.NumberOfRows - 1) As Single
        Dim FFE(dbf.NumberOfRows - 1) As Single
        Dim StructureValues(dbf.NumberOfRows - 1) As Double

        Dim ContVal(dbf.NumberOfRows - 1) As Double
        Dim OtherVal(dbf.NumberOfRows - 1) As Double
        Dim Year(dbf.NumberOfRows - 1) As Integer
        Dim ModName(dbf.NumberOfRows - 1) As String

        Dim BeginDamage(dbf.NumberOfRows - 1) As Single
        Dim NumStructs(dbf.NumberOfRows - 1) As Integer

        If CmbName.SelectedIndex = -1 Then
            MsgBox("You did not define a name column") : Exit Sub
        Else
            'check names for duplicates.
            Dim tmpnames() As Object = dbf.GetColumn(CmbName.SelectedIndex - 1)
            For i = 0 To tmpnames.Count - 1
                If Names.Contains(tmpnames(i).ToString) Then
                    'build a report of the non unique names.
                    MsgBox("Structure names are not all unique") : Exit Sub
                Else
                    Names(i) = tmpnames(i)
                End If
            Next
        End If

        If CmbOccType.SelectedIndex = -1 OrElse CmbOccType.SelectedIndex = 0 Then MsgBox("you did not define an occupancy type") : Exit Sub
        If CmbDamCat.SelectedIndex = -1 OrElse CmbDamCat.SelectedIndex = 0 Then MsgBox("You did not define a damage category") : Exit Sub
        Dim tmpocctypes As Object() = dbf.GetColumn(CmbOccType.SelectedIndex - 1)
        Dim tmpdamcats As Object() = dbf.GetColumn(CmbDamCat.SelectedIndex - 1)
        'keep track of occupancy type names and damage categories, do not allow occtype/damcat combos where occtypeA  = occtypeA and damcatA<>DamcatB.
        Dim damagecategorydict As New Dictionary(Of String, String)
        Dim tmpocctype As String
        Dim tmpdamcat As String
        For i = 0 To tmpocctypes.Count - 1
            Occtypes(i) = tmpocctypes(i).ToString
            DamCats(i) = tmpdamcats(i).ToString
            tmpdamcat = DamCats(i)
            tmpocctype = Occtypes(i)
            If damagecategorydict.ContainsKey(tmpocctype) Then
                'already exits.
                If damagecategorydict.Item(tmpocctype) = tmpdamcat Then
                    'all good
                Else
                    'oh snap...
                    Debug.Print("The occtype: " & tmpocctype & " is contained in two damage categories " & tmpdamcat & " and " & damagecategorydict.Item(tmpocctype))
                    'MsgBox("The occtype: " & tmpocctype & " is contained in two damage categories " & tmpdamcat & " and " & damagecategorydict.Item(tmpocctype)) : Exit Sub
                End If
            Else
                damagecategorydict.Add(tmpocctype, tmpdamcat)
            End If
        Next
        If CmbStructureValue.SelectedIndex = -1 OrElse CmbStructureValue.SelectedIndex = 0 Then MsgBox("You did not define a structure value") : Exit Sub

        StructureValues = Array.ConvertAll(dbf.GetColumn(CmbStructureValue.SelectedItem.ToString), AddressOf Double.Parse)

        If UseFirstFloorElevation.IsChecked Then
            If CmbFFE.SelectedIndex = -1 OrElse CmbFFE.SelectedIndex = 0 Then
                MsgBox("You did not define a First Floor Elevation, and you did elect to define first floor elevation from the dbf.") : Exit Sub
            Else
                'retrieve from the dbf
                FFE = Array.ConvertAll(dbf.GetColumn(CmbFFE.SelectedItem.ToString), AddressOf Single.Parse)
            End If
        End If


        If UseDbfForTerrain.IsChecked Then
            'Make sure they've selected the attribute if they've clicked the define elevation from dbf checkbox
            If CmbGroundElevation.SelectedIndex = -1 OrElse CmbGroundElevation.SelectedIndex = 0 Then MsgBox("You did not define a Ground Elevation, and you did elect to define ground elevation from the dbf.") : Exit Sub

            'Save the selected ground elevations
            GE = Array.ConvertAll(dbf.GetColumn(CmbGroundElevation.SelectedItem.ToString), AddressOf Single.Parse)

            'Make sure that if foundation height is checked, they've selected the attribute for foundation height
            If UseFoundationHeight.IsChecked Then

                If CmbFoundationHeight.SelectedIndex = -1 OrElse CmbFoundationHeight.SelectedIndex = 0 Then
                    MsgBox("You did not define a foundation height, and you did elect to define elevation from the dbf.") : Exit Sub
                Else
                    'Save the selected foundation heights - set use dbf for terrain and use foundation height for each structure
                    FH = Array.ConvertAll(dbf.GetColumn(CmbFoundationHeight.SelectedItem.ToString), AddressOf Single.Parse)
                End If

            Else
                If CmbFFE.SelectedIndex = -1 OrElse CmbFFE.SelectedIndex = 0 Then
                    MsgBox("You did not define a first floor elevation, and you did elect to define elevation from the dbf") : Exit Sub
                Else
                    FFE = Array.ConvertAll(dbf.GetColumn(CmbFFE.SelectedItem.ToString), AddressOf Single.Parse)
                End If
            End If
        Else
            'dont use dbf for terrain..
            If UseFoundationHeight.IsChecked Then
                If CmbFoundationHeight.SelectedIndex = -1 OrElse CmbFoundationHeight.SelectedIndex = 0 Then
                    MsgBox("You did not define a foundation height.") : Exit Sub
                Else
                    FH = Array.ConvertAll(dbf.GetColumn(CmbFoundationHeight.SelectedItem.ToString), AddressOf Single.Parse)
                End If
            Else
                If CmbFFE.SelectedIndex = -1 OrElse CmbFFE.SelectedIndex = 0 Then
                    MsgBox("You did not define a first floor elevation") : Exit Sub
                Else
                    FFE = Array.ConvertAll(dbf.GetColumn(CmbFFE.SelectedItem.ToString), AddressOf Single.Parse)
                End If
            End If
        End If

        SaveUseDBFGE(StructureValues.Count)
        SaveUseFFE(StructureValues.Count)

        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'Get Optional attributes
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        If Not CmbContentValue.SelectedIndex = -1 OrElse CmbContentValue.SelectedIndex = 0 Then ContVal = Array.ConvertAll(dbf.GetColumn(CmbContentValue.SelectedItem.ToString), AddressOf Double.Parse)
        If Not CmbOtherValue.SelectedIndex = -1 OrElse CmbOtherValue.SelectedIndex = 0 Then OtherVal = Array.ConvertAll(dbf.GetColumn(CmbOtherValue.SelectedItem.ToString), AddressOf Double.Parse)
        If Not CmbYear.SelectedIndex = -1 OrElse CmbYear.SelectedIndex = 0 Then Year = Array.ConvertAll(dbf.GetColumn(CmbYear.SelectedItem.ToString), AddressOf Integer.Parse)
        If Not CmbModule.SelectedIndex = -1 OrElse CmbModule.SelectedIndex = 0 Then
            For i = 0 To dbf.NumberOfRows - 1
                ModName(i) = dbf.GetCell(CmbModule.SelectedIndex - 1, i)
            Next
        Else
            For i = 0 To dbf.NumberOfRows - 1
                ModName(i) = "Base"
            Next
        End If
        'If Not cmbbegindamage.selectedindex = -1 Then BeginDamage = Array.ConvertAll(dbf.GetColumn(cmbbegindamage.selectedindex), AddressOf Single.Parse)
        'If Not cmbNumstructs.selectedindex = -1 Then NumStructs = Array.ConvertAll(dbf.GetColumn(cmbNumStructs.selectedindex), AddressOf Integer.Parse)
        If _monetaryUnits = CmbMonetaryUnits.SelectedItem.ToString Then
        Else
            For i = 0 To StructureValues.Count - 1
                Select Case _monetaryUnits
                    Case "$'s"
                        Select Case CmbMonetaryUnits.SelectedItem.ToString
                            Case "1,000$'s"
                                StructureValues(i) /= 1000
                                ContVal(i) /= 1000
                                OtherVal(i) /= 1000
                            Case "1,000,000$'s"
                                StructureValues(i) /= 1000000
                                ContVal(i) /= 1000000
                                OtherVal(i) /= 1000000
                            Case Else
                                'RaiseEvent ReportMessage("Somthing bad happened")
                        End Select
                    Case "1,000$'s"
                        Select Case CmbMonetaryUnits.SelectedItem.ToString
                            Case "$'s"
                                StructureValues(i) *= 1000
                                OtherVal(i) *= 1000
                                ContVal(i) *= 1000
                            Case "1,000,000$'s"
                                StructureValues(i) /= 1000
                                OtherVal(i) /= 1000
                                ContVal(i) /= 1000
                            Case Else
                                'RaiseEvent ReportMessage("Somthing bad happened")
                        End Select
                    Case "1,000,000$'s"
                        Select Case CmbMonetaryUnits.SelectedItem.ToString
                            Case "$'s"
                                StructureValues(i) *= 1000000
                                ContVal(i) *= 1000000
                                OtherVal(i) *= 1000000
                            Case "1,000$'s"
                                StructureValues(i) *= 1000
                                ContVal(i) *= 1000
                                OtherVal(i) *= 1000
                            Case Else
                                'RaiseEvent ReportMessage("Somthing bad happened")
                        End Select
                End Select
            Next
        End If



        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'Convert input shapefile to Structure Inventory
        '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Dim dt As New System.Data.DataTable
        'Radio Buttons


        dt.Columns.Add("St_Name", GetType(String))
        dt.Columns.Add("DamCat", GetType(String))
        dt.Columns.Add("OccType", GetType(String))
        dt.Columns.Add("Found_Ht", GetType(Single))
        dt.Columns.Add("Ground_Ht", GetType(Single))
        dt.Columns.Add("FFE", GetType(Single))
        dt.Columns.Add("UseFFE", GetType(Boolean))
        dt.Columns.Add("UseDBF_GE", GetType(Boolean))
        dt.Columns.Add("Val_Struct", GetType(Double))
        dt.Columns.Add("Val_Cont", GetType(Double))
        dt.Columns.Add("Val_Other", GetType(Double))
        dt.Columns.Add("Yr_Built", GetType(Integer))
        dt.Columns.Add("Begin_Dmg", GetType(Single))
        dt.Columns.Add("Num_Struct", GetType(Integer))
        dt.Columns.Add("Mod_Name", GetType(String))

        For i = 0 To dbf.NumberOfRows - 1
            dt.Rows.Add({Names(i), DamCats(i), Occtypes(i), FH(i), GE(i), FFE(i), UseFFE(i), UseDBFGE(i), StructureValues(i), ContVal(i), OtherVal(i), Year(i), BeginDamage(i), NumStructs(i), ModName(i)})
        Next
        _dt = dt
        DialogResult = True
        Me.Close()
    End Sub

    Private Function SaveUseDBFGE(strucCount As Integer)
        Dim UseDBFGE(strucCount) As Boolean
        For i = 0 To strucCount - 1
            UseDBFGE(i) = UseDbfForTerrain.IsChecked
        Next
        Return UseDBFGE
    End Function

    Private Function SaveUseFFE(strucCount As Integer)
        Dim UseFFE(strucCount) As Boolean
        For i = 0 To strucCount - 1
            UseFFE(i) = UseDbfForTerrain.IsChecked
        Next
        Return UseFFE
    End Function

End Class
