Public Class OccupancyTypeTreeNode
    Inherits FrameworkTreeNode
    Private Const _ParentFolder As String = "Inventory"
    Private Const _FolderName As String = "OccupancyTypes"
    Private Const _header As String = "Occupancy Types"
    Private Const _OcctypeFileName As String = "Occtypes.xml"
    Private _UpdateComputeFiles As Boolean = False
    Private _Loaded As Boolean = False
    Sub New()
        MyBase.New(_header)
    End Sub
    Public Overrides ReadOnly Property GetSubDirectory As String
        Get
            Return _ParentFolder & "\" & _FolderName
        End Get
    End Property
    Public Overrides ReadOnly Property GetNodeName As String
        Get
            Return _FolderName
        End Get
    End Property
    Public ReadOnly Property getOcctypeFilepath As String
        Get
            Return GetCurrentDirectory & "\" & _OcctypeFileName
        End Get
    End Property
    Public Overrides Sub OnSaveEvent(sender As Object, e As System.Windows.RoutedEventArgs)

    End Sub
    Public Overrides Sub SetContextMenu()
        Dim c As New ContextMenu

        Dim BrowseToExisting As New MenuItem()
        BrowseToExisting.Header = "Import " & _header
        AddHandler BrowseToExisting.Click, AddressOf BrowseToExistingFile
        c.Items.Add(BrowseToExisting)

        Dim BrowseToDefaults As New MenuItem()
        BrowseToDefaults.Header = "Import default " & _header
        AddHandler BrowseToDefaults.Click, AddressOf BrowseToDefaultFile
        c.Items.Add(BrowseToDefaults)

        Dim Edit As New MenuItem()
        Edit.Header = "Edit " & _header
        Edit.IsEnabled = False
        AddHandler Edit.Click, AddressOf EditExisting
        c.Items.Add(Edit)

        Dim Export As New MenuItem()
        Export.Header = "Export " & _header
        Export.IsEnabled = False
        AddHandler Export.Click, AddressOf WriteToAscii
        c.Items.Add(Export)

        Dim BrowseToExistingMain As New MenuItem()
        BrowseToExistingMain.Header = "_Import " & _header
        AddHandler BrowseToExistingMain.Click, AddressOf BrowseToExistingFile

        For Each mi As MenuItem In GetMainContextMenu.Items
            If mi.Header = "_Study" Then
                For Each submi As MenuItem In mi.Items
                    If submi.Header = "_Inventory" Then
                        submi.Items.Add(BrowseToExistingMain)
                    End If
                Next
            End If
        Next

        MyBase.ContextMenu = c
    End Sub
    Sub BrowseToExistingFile(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim ofd As New Microsoft.Win32.OpenFileDialog
        With ofd
            .Title = "Please select an existing occupancy type text file"
            .Filter = "text files (*.txt)|*.txt|xml files (*.xml)|*.xml|All Files (*.*)|*.*"
            .CheckFileExists = True
            .Multiselect = False
            '.InitialDirectory = GetCurrentDirectory
        End With

        If ofd.ShowDialog Then
            'copy to study directory
            'select case on the file.
            Select Case System.IO.Path.GetExtension(ofd.FileName).ToLower
                Case ".txt"

                    Dim ot As Consequences_Assist.ComputableObjects.OccupancyTypes = CreateXMLFromTxt(ofd.FileName)
                    If IsNothing(ot) Then
                        Exit Sub
                    Else
                        If System.IO.File.Exists(getOcctypeFilepath) Then
                            'something already exists, and they have browsed to something new.
                            MsgBox("Deleting old occtype file")
                            Kill(getOcctypeFilepath)
                            _UpdateComputeFiles = True
                        End If
                        'they browsed to a text file. Read the file and check for errors
                        'write out to xml in the study directory the corrected file.
                        ot.WriteToXML(getOcctypeFilepath)
                        UpdateDamCatsBasedOnOcctypes()
                    End If



                Case ".xml"
                    'they browsed to an xml file, make sure the contents are correct?
                    'copy to the study directory
                    If getOcctypeFilepath = ofd.FileName Then
                        'they browsed to their own file
                    Else
                        If System.IO.File.Exists(getOcctypeFilepath) Then
                            'something already exists, and they have browsed to something new.
                            MsgBox("Deleting old occtype file")
                            Kill(getOcctypeFilepath)
                            _UpdateComputeFiles = True
                        End If
                        System.IO.File.Copy(ofd.FileName, getOcctypeFilepath)
                        UpdateDamCatsBasedOnOcctypes()
                    End If
                Case Else
                    MsgBox("File type not supported") : Exit Sub
            End Select
            IsEditable()
            checkforerrors()
            DirectCast(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
        Else
            MsgBox("You did not select an occupancy type file.")
        End If
    End Sub
    Public Sub BrowseToDefaultFile(sender As Object, e As System.Windows.RoutedEventArgs)
        If System.IO.File.Exists(getOcctypeFilepath) Then
            'something already exists, and they have browsed to something new.
            MsgBox("Deleting old occtype file")
            Kill(getOcctypeFilepath)
            _UpdateComputeFiles = True
        End If
        Dim assmb As System.Reflection.Assembly = System.Reflection.Assembly.Load(Me.GetType.Assembly.GetName)
        Dim strm As System.IO.Stream = assmb.GetManifestResourceStream(Me.GetType().Namespace & "." & "Occtypes.xml")
        Using OutputFile As System.IO.Stream = System.IO.File.Create(getOcctypeFilepath)
            strm.CopyTo(OutputFile)
        End Using
        UpdateDamCatsBasedOnOcctypes()
        IsEditable()
        checkforerrors()
        DirectCast(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
    End Sub
    Private Sub UpdateDamCatsBasedOnOcctypes()
        Dim ot As New Consequences_Assist.ComputableObjects.OccupancyTypes()
        AddHandler ot.ReportMessage, AddressOf ReportMessage
        ot.LoadFromFile(getOcctypeFilepath)
        Dim dtypes As DamageCategoryTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(DamageCategoryTreeNode))(0), DamageCategoryTreeNode)

        Dim damcatnamelist As New List(Of String)
        Dim damcatlist As New List(Of Consequences_Assist.ComputableObjects.DamageCategory)
        Dim d As Consequences_Assist.ComputableObjects.DamageCategories
        If System.IO.File.Exists(dtypes.GetDamageCategoryPath) Then
            d = New Consequences_Assist.ComputableObjects.DamageCategories(dtypes.GetDamageCategoryPath)
            For i = 0 To d.GetDamageCategories.Count - 1
                damcatnamelist.Add(d.GetDamageCategories(i).Name)
                damcatlist.Add(d.GetDamageCategories(i))
            Next
        Else

        End If

        For i = 0 To ot.OccupancyTypes.Count - 1
            If damcatnamelist.Contains(ot.OccupancyTypes(i).DamageCategory.Name) Then
            Else
                damcatnamelist.Add(ot.OccupancyTypes(i).DamageCategory.Name)
                damcatlist.Add(ot.OccupancyTypes(i).DamageCategory)
                ReportMessage("Damage category added: " & damcatnamelist(damcatnamelist.Count - 1))
            End If
        Next
        d = New Consequences_Assist.ComputableObjects.DamageCategories(damcatlist)
        d.WriteToXML(dtypes.GetDamageCategoryPath)
    End Sub
    Public Function CreateXMLFromTxt(ByVal textfilepath As String) As Consequences_Assist.ComputableObjects.OccupancyTypes
        Try
            Dim ot As New Consequences_Assist.ComputableObjects.OccupancyTypes
            AddHandler ot.ReportMessage, AddressOf ReportMessage
            ot.LoadFromFile(textfilepath)
            Return ot
        Catch ex As Exception
            ReportMessage("Error importing occupancy types during Import" & vbNewLine & vbNewLine & ex.Message)
            MsgBox("Error importing occupancy types during Import" & vbNewLine & vbNewLine & ex.Message)
            Return Nothing
        End Try
    End Function
    Sub EditExisting(sender As Object, e As System.Windows.RoutedEventArgs)
        If Not System.IO.File.Exists(getOcctypeFilepath) Then MsgBox("Occtype File Not Found") : Exit Sub
        Dim o As New Consequences_Assist.ComputableObjects.OccupancyTypes(getOcctypeFilepath)
        Dim dtypes As DamageCategoryTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(DamageCategoryTreeNode))(0), DamageCategoryTreeNode)
        'check to see if the damage categories are defined.
        If System.IO.File.Exists(dtypes.GetDamageCategoryPath) Then
            Dim d As New Consequences_Assist.ComputableObjects.DamageCategories(dtypes.GetDamageCategoryPath)
            Dim oeditor As New Consequences_Assist.OTE(o.OccupancyTypes, d.GetDamageCategories, GetCurrentDirectory & "\" & "Occtypes.xml")
            'AddHandler oeditor.OcctypeAdded, AddressOf OnOcctypeAdded 'do i really care?

            AddHandler oeditor.OcctypeRenamed, AddressOf OnOcctypeRename 'update structures
            AddHandler oeditor.OcctypeDeleted, AddressOf OnOcctypeDelete 'updatestructures and ask user if there are structures assigned which occupancy to reassign them to...
            AddHandler oeditor.OcctypeDamcatReassigned, AddressOf OnOcctypeDamCatReassigned 'update structures
            AddHandler oeditor.DamcatAdded, AddressOf OnDamCatAdded 'update damcatfile.
            AddHandler oeditor.Closing, AddressOf CloseAndCheckForErrors
            oeditor.UseVehicleDamages = False
            oeditor.Owner = GetMainWindow
            oeditor.Show()
        Else
            ReportMessage("Damage category file not found, please import damage categories.")
            MsgBox("Damage category file not found, please import damage categories.")
            Exit Sub
        End If

        checkforerrors()
        'get the new data.
        'check it for errors.
    End Sub
    Private Sub CloseAndCheckForErrors(sender As Object, e As System.ComponentModel.CancelEventArgs)
        checkforerrors()
    End Sub
    Private Sub OnDamCatAdded(ByVal damcat As Consequences_Assist.ComputableObjects.DamageCategory)
        Dim dtypes As DamageCategoryTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(DamageCategoryTreeNode))(0), DamageCategoryTreeNode)
        Dim d As New Consequences_Assist.ComputableObjects.DamageCategories(dtypes.GetDamageCategoryPath)
        d.GetDamageCategories.Add(damcat)
        d.WriteToXML(dtypes.GetDamageCategoryPath)
        _UpdateComputeFiles = True
    End Sub
    Private Sub OnOcctypeRename(ByVal oldocctype As Consequences_Assist.ComputableObjects.OccupancyType, ByVal newname As String)
        'check if structures exist.
        Dim fnodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(StructureInventoryChildTreeNode))
        If IsNothing(fnodes) OrElse fnodes.Count = 0 Then Exit Sub
        Dim snodes As New List(Of StructureInventoryChildTreeNode)
        For i = 0 To fnodes.Count - 1
            snodes.Add(CType(fnodes(i), StructureInventoryChildTreeNode))
        Next
        For i = 0 To snodes.Count - 1
            'get the structure dbf
            Dim dbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(snodes(i).GetStructurePath, ".dbf"))
            'get the list of occupancy type names?
            Dim oindex As Integer = Array.IndexOf(dbf.ColumnNames, "OccType")
            For j = 0 To dbf.NumberOfRows - 1
                If dbf.GetCell(oindex, j) = oldocctype.Name Then
                    dbf.EditCell(oindex, j, newname)
                    _UpdateComputeFiles = True
                End If
            Next
            dbf.Close()
        Next
    End Sub
    Private Sub OnOcctypeDamCatReassigned(ByVal occtype As Consequences_Assist.ComputableObjects.OccupancyType, ByVal damcat As Consequences_Assist.ComputableObjects.DamageCategory)
        'check if structures exist.
        Dim fnodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(StructureInventoryChildTreeNode))
        If IsNothing(fnodes) OrElse fnodes.Count = 0 Then Exit Sub
        Dim snodes As New List(Of StructureInventoryChildTreeNode)
        For i = 0 To fnodes.Count - 1
            snodes.Add(CType(fnodes(i), StructureInventoryChildTreeNode))
        Next
        For i = 0 To snodes.Count - 1
            'get the structure dbf
            Dim dbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(snodes(i).GetStructurePath, ".dbf"))
            'get the list of occupancy type names?
            Dim oindex As Integer = Array.IndexOf(dbf.ColumnNames, "OccType")
            Dim dindex As Integer = Array.IndexOf(dbf.ColumnNames, "DamCat")
            For j = 0 To dbf.NumberOfRows - 1
                If dbf.GetCell(oindex, j) = occtype.Name AndAlso dbf.GetCell(dindex, j) = occtype.DamageCategory.Name Then
                    dbf.EditCell(dindex, j, damcat.Name)
                    _UpdateComputeFiles = True
                End If
            Next
            dbf.Close()
        Next
    End Sub
    Private Sub OnOcctypeDelete(ByVal occtype As Consequences_Assist.ComputableObjects.OccupancyType, ByRef cancel As Boolean)
        Dim fnodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(StructureInventoryChildTreeNode))
        If IsNothing(fnodes) OrElse fnodes.Count = 0 Then Exit Sub
        Dim snodes As New List(Of StructureInventoryChildTreeNode)
        For i = 0 To fnodes.Count - 1
            snodes.Add(CType(fnodes(i), StructureInventoryChildTreeNode))
        Next
        Dim s As New List(Of OcctypeDeletedRowItem)
        For i = 0 To snodes.Count - 1
            'get the structure dbf
            s.Add(New OcctypeDeletedRowItem(snodes(i).Header))
            Dim dbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(snodes(i).GetStructurePath, ".dbf")) ''make sure that this dbf is not open in another location...
            'get the list of occupancy type names?
            Dim oindex As Integer = Array.IndexOf(dbf.ColumnNames, "OccType")
            Dim structurecounter As Integer = 0
            For j = 0 To dbf.NumberOfRows - 1
                If dbf.GetCell(oindex, j) = occtype.Name Then
                    'dbf.EditCell(oindex, i, newname)
                    s.Last.NumberOfStructuresImpacted += 1
                End If
            Next
            dbf.Close()
        Next
        Dim structuresimpacted As Boolean = False
        Dim message As New System.Text.StringBuilder
        For i = 0 To s.Count - 1
            If s(i).NumberOfStructuresImpacted <> 0 Then
                structuresimpacted = True
                message.Append(s(i).StructureInventoryName & " has " & s(i).NumberOfStructuresImpacted & " structures that have the occupancy type " & occtype.Name & vbNewLine)
            End If
        Next
        If structuresimpacted Then
            Dim occtypes As New Consequences_Assist.ComputableObjects.OccupancyTypes(getOcctypeFilepath)
            Dim lst As New List(Of Consequences_Assist.ComputableObjects.OccupancyType)
            For i = 0 To occtypes.OccupancyTypes.Count - 1
                If occtypes.OccupancyTypes(i).Name = occtype.Name AndAlso occtypes.OccupancyTypes(i).DamageCategory.Name = occtype.DamageCategory.Name Then
                Else
                    lst.Add(occtypes.OccupancyTypes(i))
                End If
            Next
            If lst.Count = 0 Then
                'dont allow because then there are no occtypes.
                MsgBox(message.ToString & "This leaves no occupancy types to assign to the structures in your inventory, action must be aborted.")
                cancel = True
            Else
                Dim delete As New OccupancyTypeDeleted(message.ToString, lst)
                If delete.ShowDialog Then
                    'delete safely
                    Dim selectedocctype As Consequences_Assist.ComputableObjects.OccupancyType = delete.CmbRemainingDamCats.SelectedItem
                    For i = 0 To occtypes.OccupancyTypes.Count - 1
                        If occtypes.OccupancyTypes(i).Name = occtype.Name AndAlso occtypes.OccupancyTypes(i).DamageCategory.Name = occtype.DamageCategory.Name Then
                            occtypes.OccupancyTypes.RemoveAt(i) : Exit For 'will cause index issues if you dont exit.
                        End If
                    Next
                    'structures.
                    For i = 0 To snodes.Count - 1
                        'get the structure dbf
                        Dim dbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(snodes(i).GetStructurePath, ".dbf"))
                        Dim oindex As Integer = Array.IndexOf(dbf.ColumnNames, "OccType")
                        Dim dindex As Integer = Array.IndexOf(dbf.ColumnNames, "DamCat")
                        For j = 0 To dbf.NumberOfRows - 1
                            If dbf.GetCell(oindex, j) = occtype.Name Then
                                dbf.EditCell(oindex, j, selectedocctype.Name)
                                dbf.EditCell(dindex, j, selectedocctype.DamageCategory.Name)
                            End If
                        Next
                        dbf.Close()
                    Next
                    occtypes.WriteToXML(getOcctypeFilepath) 'edit occurs.
                    _UpdateComputeFiles = True
                Else
                    cancel = True
                End If
            End If
        Else
            MsgBox("No structures are impacted by this action. Deleting safely.") 'this works because we are not setting cancel to true.
        End If


    End Sub
    Public Sub SetEditableAndUpdateDamcats()
        IsEditable()
        UpdateDamCatsBasedOnOcctypes()
    End Sub
    Private Sub IsEditable()
        For Each mi As MenuItem In MyBase.ContextMenu.Items
            If mi.Header = "Edit " & _header Then
                mi.IsEnabled = True
            End If
            If mi.Header = "Export " & _header Then
                mi.IsEnabled = True
            End If
        Next
    End Sub
    Public Function checkforerrors() As Boolean
        Dim counter As Integer = 0
        Dim statsreport As List(Of Consequences_Assist.AutoGenerate.CellErrorReport)
        Dim c As Consequences_Assist.AutoGenerate.Curve
        If System.IO.File.Exists(getOcctypeFilepath) Then
            Dim o As New Consequences_Assist.ComputableObjects.OccupancyTypes(getOcctypeFilepath)
            If o.OccupancyTypes.Count > 0 Then _Loaded = True : IsEditable()
            For i = 0 To o.OccupancyTypes.Count - 1
                Dim curvecount As Integer = 0
                If o.OccupancyTypes(i).Name.Length > 32 Then counter += 1
                If o.OccupancyTypes(i).CalcStructDamage Then
                    curvecount += 1
                    If Not IsNothing(o.OccupancyTypes(i).StructureValueUncertainty) Then
                        Select Case o.OccupancyTypes(i).StructureValueUncertainty.GetType
                            Case GetType(Statistics.Normal)
                                If CType(o.OccupancyTypes(i).StructureValueUncertainty, Statistics.Normal).GetStDev > 15 OrElse CType(o.OccupancyTypes(i).StructureValueUncertainty, Statistics.Normal).GetStDev <= 0 Then counter += 1
                            Case GetType(Statistics.Triangular)
                                If CType(o.OccupancyTypes(i).StructureValueUncertainty, Statistics.Triangular).getMin > 0 OrElse CType(o.OccupancyTypes(i).StructureValueUncertainty, Statistics.Triangular).getMin < -100 Then counter += 1
                                If CType(o.OccupancyTypes(i).StructureValueUncertainty, Statistics.Triangular).getMax < 0 Then counter += 1
                            Case GetType(Statistics.Uniform)
                                If CType(o.OccupancyTypes(i).StructureValueUncertainty, Statistics.Uniform).GetMin > 0 OrElse CType(o.OccupancyTypes(i).StructureValueUncertainty, Statistics.Uniform).GetMin < -100 Then counter += 1
                                If CType(o.OccupancyTypes(i).StructureValueUncertainty, Statistics.Triangular).getMax < 0 Then counter += 1
                            Case Else
                                'no errors.
                        End Select
                    End If

                    c = New Consequences_Assist.AutoGenerate.Curve(o.OccupancyTypes(i).GetStructurePercentDD, Consequences_Assist.AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain)
                    If c.Curve.Count = 0 Then
                        counter += 1
                    Else
                        statsreport = c.Validate(0, 100)
                        If Not IsNothing(statsreport) AndAlso statsreport.Count > 0 Then counter += 1
                    End If
                End If
                If o.OccupancyTypes(i).CalcContentDamage Then
                    curvecount += 1
                    If Not IsNothing(o.OccupancyTypes(i).ContentValueUncertainty) Then
                        Select Case o.OccupancyTypes(i).ContentValueUncertainty.GetType
                            Case GetType(Statistics.Normal)
                                If CType(o.OccupancyTypes(i).ContentValueUncertainty, Statistics.Normal).GetStDev > 15 OrElse CType(o.OccupancyTypes(i).ContentValueUncertainty, Statistics.Normal).GetStDev <= 0 Then counter += 1
                            Case GetType(Statistics.Triangular)
                                If CType(o.OccupancyTypes(i).ContentValueUncertainty, Statistics.Triangular).getMin > 0 OrElse CType(o.OccupancyTypes(i).ContentValueUncertainty, Statistics.Triangular).getMin < -100 Then counter += 1
                                If CType(o.OccupancyTypes(i).ContentValueUncertainty, Statistics.Triangular).getMax < 0 Then counter += 1
                            Case GetType(Statistics.Uniform)
                                If CType(o.OccupancyTypes(i).ContentValueUncertainty, Statistics.Uniform).GetMin > 0 OrElse CType(o.OccupancyTypes(i).ContentValueUncertainty, Statistics.Uniform).GetMin < -100 Then counter += 1
                                If CType(o.OccupancyTypes(i).ContentValueUncertainty, Statistics.Triangular).getMax < 0 Then counter += 1
                            Case Else
                                'no errors.
                        End Select
                    End If

                    c = New Consequences_Assist.AutoGenerate.Curve(o.OccupancyTypes(i).GetContentPercentDD, Consequences_Assist.AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain)
                    If c.Curve.Count = 0 Then
                        counter += 1
                    Else
                        statsreport = c.Validate(0, 100)
                        If Not IsNothing(statsreport) AndAlso statsreport.Count > 0 Then counter += 1
                    End If
                End If
                If o.OccupancyTypes(i).CalcOtherDamage Then
                    curvecount += 1
                    If Not IsNothing(o.OccupancyTypes(i).OtherValueUncertainty) Then
                        Select Case o.OccupancyTypes(i).OtherValueUncertainty.GetType
                            Case GetType(Statistics.Normal)
                                If CType(o.OccupancyTypes(i).OtherValueUncertainty, Statistics.Normal).GetStDev > 15 OrElse CType(o.OccupancyTypes(i).OtherValueUncertainty, Statistics.Normal).GetStDev <= 0 Then counter += 1
                            Case GetType(Statistics.Triangular)
                                If CType(o.OccupancyTypes(i).OtherValueUncertainty, Statistics.Triangular).getMin > 0 OrElse CType(o.OccupancyTypes(i).OtherValueUncertainty, Statistics.Triangular).getMin < -100 Then counter += 1
                                If CType(o.OccupancyTypes(i).OtherValueUncertainty, Statistics.Triangular).getMax < 0 Then counter += 1
                            Case GetType(Statistics.Uniform)
                                If CType(o.OccupancyTypes(i).OtherValueUncertainty, Statistics.Uniform).GetMin > 0 OrElse CType(o.OccupancyTypes(i).OtherValueUncertainty, Statistics.Uniform).GetMin < -100 Then counter += 1
                                If CType(o.OccupancyTypes(i).OtherValueUncertainty, Statistics.Triangular).getMax < 0 Then counter += 1
                            Case Else
                                'no errors.
                        End Select
                    End If

                    c = New Consequences_Assist.AutoGenerate.Curve(o.OccupancyTypes(i).GetOtherPercentDD, Consequences_Assist.AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain)
                    If c.Curve.Count = 0 Then
                        counter += 1
                    Else
                        statsreport = c.Validate(0, 100)
                        If Not IsNothing(statsreport) AndAlso statsreport.Count > 0 Then counter += 1
                    End If
                End If
                If curvecount = 0 Then counter += 1
            Next
        Else
            counter += 1
        End If

        If counter = 0 Then
            For Each mi As MenuItem In MyBase.ContextMenu.Items
                If mi.Header = "View errors" Then
                    MyBase.ContextMenu.Items.Remove(mi)
                    MyBase.Foreground = Brushes.Black
                    MyBase.ToolTip = ""
                    Exit For
                End If
            Next
            If _UpdateComputeFiles Then
                UpdateOutputTreeNodes(True, False)
            End If
            Return False
        Else
            MyBase.ToolTip = "there are " & counter.ToString & " errors"
            Dim loaded As Boolean = False
            For Each mi As MenuItem In MyBase.ContextMenu.Items
                If mi.Header = "View errors" Then
                    loaded = True
                End If
            Next
            If Not loaded Then
                Dim errorreport As New MenuItem()
                errorreport.Header = "View errors"
                AddHandler errorreport.Click, AddressOf CreateErrorMessage
                MyBase.ContextMenu.Items.Add(errorreport)
                MyBase.Foreground = Brushes.Red
            End If
            ''update compute
            If _UpdateComputeFiles Then
                _UpdateComputeFiles = False
                UpdateOutputTreeNodes(False, True)
            End If
            Return True
        End If
    End Function
    Private Sub UpdateOutputTreeNodes(askforrecompute As Boolean, haserrors As Boolean)
        Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
        For Each o As OutputChildTreeNode In nodes
            o.AlertUserThatUpdatesAreNeeded("Occupancy Types have changed", askforrecompute, haserrors)
        Next
    End Sub
    Private Sub CreateErrorMessage()
        Dim report As New List(Of OcctypeErrorReportRowItem)
        Dim statsreport As List(Of Consequences_Assist.AutoGenerate.CellErrorReport)
        Dim c As Consequences_Assist.AutoGenerate.Curve
        If System.IO.File.Exists(getOcctypeFilepath) Then
            Dim o As New Consequences_Assist.ComputableObjects.OccupancyTypes(getOcctypeFilepath)
            For i = 0 To o.OccupancyTypes.Count - 1
                Dim curvecount As Integer = 0
                If o.OccupancyTypes(i).Name.Length > 32 Then report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Name is greater than 32 characters"))
                If o.OccupancyTypes(i).CalcStructDamage Then
                    curvecount += 1
                    If Not IsNothing(o.OccupancyTypes(i).StructureValueUncertainty) Then
                        Select Case o.OccupancyTypes(i).StructureValueUncertainty.GetType
                            Case GetType(Statistics.Normal)
                                If CType(o.OccupancyTypes(i).StructureValueUncertainty, Statistics.Normal).GetStDev > 15 Then
                                    report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Structure Value Uncertianty is normal with a standard deviation greater than 15"))
                                ElseIf CType(o.OccupancyTypes(i).StructureValueUncertainty, Statistics.Normal).GetStDev <= 0 Then
                                    report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Structure Value Uncertianty is normal with a standard deviation less than or equal to 0"))
                                End If
                            Case GetType(Statistics.Triangular)
                                If CType(o.OccupancyTypes(i).StructureValueUncertainty, Statistics.Triangular).getMin > 0 Then
                                    report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Structure Value Uncertianty is triangular with a min greater than 0"))
                                ElseIf CType(o.OccupancyTypes(i).StructureValueUncertainty, Statistics.Triangular).getMin < -100 Then
                                    report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Structure Value Uncertianty is triangular with a min less than -100"))
                                End If
                                If CType(o.OccupancyTypes(i).StructureValueUncertainty, Statistics.Triangular).getMax < 0 Then report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Structure Value Uncertianty is triangular with a max less than 0"))
                            Case GetType(Statistics.Uniform)
                                If CType(o.OccupancyTypes(i).StructureValueUncertainty, Statistics.Uniform).GetMin > 0 Then
                                    report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Structure Value Uncertianty is uniform with a min greater than 0"))
                                ElseIf CType(o.OccupancyTypes(i).StructureValueUncertainty, Statistics.Uniform).GetMin < -100 Then
                                    report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Structure Value Uncertianty is uniform with a min less than -100"))
                                End If
                                If CType(o.OccupancyTypes(i).StructureValueUncertainty, Statistics.Triangular).getMax < 0 Then report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Structure Value Uncertianty is uniform with a max less than 0"))
                            Case Else
                                'no errors.
                        End Select
                    End If

                    c = New Consequences_Assist.AutoGenerate.Curve(o.OccupancyTypes(i).GetStructurePercentDD, Consequences_Assist.AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain)
                    If c.Curve.Count = 0 Then
                        report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Structure Damage Curve has no ordinates"))
                    Else
                        statsreport = c.Validate(0, 100)
                        If Not IsNothing(statsreport) AndAlso statsreport.Count > 0 Then report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Error on Structure Damage Curve"))
                    End If
                End If
                If o.OccupancyTypes(i).CalcContentDamage Then
                    curvecount += 1
                    If Not IsNothing(o.OccupancyTypes(i).ContentValueUncertainty) Then
                        Select Case o.OccupancyTypes(i).ContentValueUncertainty.GetType
                            Case GetType(Statistics.Normal)
                                If CType(o.OccupancyTypes(i).ContentValueUncertainty, Statistics.Normal).GetStDev > 15 Then
                                    report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Content Value Uncertianty is normal with a standard deviation greater than 15"))
                                ElseIf CType(o.OccupancyTypes(i).ContentValueUncertainty, Statistics.Normal).GetStDev <= 0 Then
                                    report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Content Value Uncertianty is normal with a standard deviation less than or equal to 0"))
                                End If
                            Case GetType(Statistics.Triangular)
                                If CType(o.OccupancyTypes(i).ContentValueUncertainty, Statistics.Triangular).getMin > 0 Then
                                    report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Content Value Uncertianty is triangular with a min greater than 0"))
                                ElseIf CType(o.OccupancyTypes(i).ContentValueUncertainty, Statistics.Triangular).getMin < -100 Then
                                    report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Content Value Uncertianty is triangular with a min less than -100"))
                                End If
                                If CType(o.OccupancyTypes(i).ContentValueUncertainty, Statistics.Triangular).getMax < 0 Then report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Content Value Uncertianty is triangular with a max less than 0"))
                            Case GetType(Statistics.Uniform)
                                If CType(o.OccupancyTypes(i).ContentValueUncertainty, Statistics.Uniform).GetMin > 0 Then
                                    report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Content Value Uncertianty is uniform with a min greater than 0"))
                                ElseIf CType(o.OccupancyTypes(i).ContentValueUncertainty, Statistics.Uniform).GetMin < -100 Then
                                    report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Content Value Uncertianty is uniform with a min less than -100"))
                                End If
                                If CType(o.OccupancyTypes(i).ContentValueUncertainty, Statistics.Triangular).getMax < 0 Then report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Content Value Uncertianty is uniform with a max less than 0"))
                            Case Else
                                'no errors.
                        End Select
                    End If

                    c = New Consequences_Assist.AutoGenerate.Curve(o.OccupancyTypes(i).GetContentPercentDD, Consequences_Assist.AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain)
                    If c.Curve.Count = 0 Then
                        report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Structure Damage Curve has no ordinates"))
                    Else
                        statsreport = c.Validate(0, 100)
                        If Not IsNothing(statsreport) AndAlso statsreport.Count > 0 Then report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Error on Content Damage Curve"))
                    End If
                End If
                If o.OccupancyTypes(i).CalcOtherDamage Then
                    curvecount += 1
                    If Not IsNothing(o.OccupancyTypes(i).OtherValueUncertainty) Then
                        Select Case o.OccupancyTypes(i).OtherValueUncertainty.GetType
                            Case GetType(Statistics.Normal)
                                If CType(o.OccupancyTypes(i).OtherValueUncertainty, Statistics.Normal).GetStDev > 15 Then
                                    report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Other Value Uncertianty is normal with a standard deviation greater than 15"))
                                ElseIf CType(o.OccupancyTypes(i).ContentValueUncertainty, Statistics.Normal).GetStDev <= 0 Then
                                    report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Other Value Uncertianty is normal with a standard deviation less than or equal to 0"))
                                End If
                            Case GetType(Statistics.Triangular)
                                If CType(o.OccupancyTypes(i).OtherValueUncertainty, Statistics.Triangular).getMin > 0 Then
                                    report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Other Value Uncertianty is triangular with a min greater than 0"))
                                ElseIf CType(o.OccupancyTypes(i).OtherValueUncertainty, Statistics.Triangular).getMin < -100 Then
                                    report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Other Value Uncertianty is triangular with a min less than -100"))
                                End If
                                If CType(o.OccupancyTypes(i).OtherValueUncertainty, Statistics.Triangular).getMax < 0 Then report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Other Value Uncertianty is triangular with a max less than 0"))
                            Case GetType(Statistics.Uniform)
                                If CType(o.OccupancyTypes(i).OtherValueUncertainty, Statistics.Uniform).GetMin > 0 Then
                                    report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Other Value Uncertianty is uniform with a min greater than 0"))
                                ElseIf CType(o.OccupancyTypes(i).OtherValueUncertainty, Statistics.Uniform).GetMin < -100 Then
                                    report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Other Value Uncertianty is uniform with a min less than -100"))
                                End If
                                If CType(o.OccupancyTypes(i).OtherValueUncertainty, Statistics.Triangular).getMax < 0 Then report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Other Value Uncertianty is uniform with a max less than 0"))
                            Case Else
                                'no errors.
                        End Select
                    End If

                    c = New Consequences_Assist.AutoGenerate.Curve(o.OccupancyTypes(i).GetOtherPercentDD, Consequences_Assist.AutoGenerate.ValidationTypes.MonotonicIncreasingUncertain)
                    If c.Curve.Count = 0 Then
                        report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Structure Damage Curve has no ordinates"))
                    Else
                        statsreport = c.Validate(0, 100)
                        If Not IsNothing(statsreport) AndAlso statsreport.Count > 0 Then report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "Error on Other Damage Curve"))
                    End If
                End If
                If curvecount = 0 Then report.Add(New OcctypeErrorReportRowItem(o.OccupancyTypes(i).Name, "No Depth Damage Relationships are Defined."))
            Next
        Else
            report.Add(New OcctypeErrorReportRowItem("All Occtypes", "No occtype file exists"))
        End If

        If report.Count = 0 Then
            'there is nothing to report
        Else
            'there is something to report
            'would be nice if this was displayed in a grid with the option to right click and edit the specific occupancy type.
            Dim view As New OcctypeErrorReport(report)
            view.Owner = GetMainWindow
            view.Show()
        End If
    End Sub
    Public Overrides Sub AddFrameworkChildren(ele As XElement)
        ReadFromXMLElement(ele)
        Dim dinfo As New System.IO.DirectoryInfo(GetCurrentDirectory)
        Dim cleandir As Boolean = False
        For Each file As System.IO.FileInfo In dinfo.GetFiles
            Dim keepfile As Boolean = False
            If file.FullName = getOcctypeFilepath Then
                keepfile = True
            End If
            If Not keepfile And Not file.Attributes = 38 Then
                cleandir = True
            End If
        Next
        If cleandir Then
            Dim cleandirectory As New MenuItem
            cleandirectory.Header = "Clean Directory"
            AddHandler cleandirectory.Click, AddressOf Clean
            MyBase.ContextMenu.Items.Add(cleandirectory)
            MyBase.Foreground = System.Windows.Media.Brushes.Red
            MyBase.ToolTip = "Unnecessary files exist in this directory."
        End If
    End Sub
    Private Sub Clean(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim filelist As New List(Of String)
        Dim dinfo As New System.IO.DirectoryInfo(GetCurrentDirectory)
        For Each file As System.IO.FileInfo In dinfo.GetFiles
            Dim keepfile As Boolean = False
            If file.FullName = getOcctypeFilepath Then
                keepfile = True
            End If
            If Not keepfile And Not file.Attributes = 38 Then
                filelist.Add(file.FullName)
            End If
        Next
        If filelist.Count > 0 Then
            Dim msg As String = ""
            For i = 0 To filelist.Count - 1
                msg = msg & filelist(i) & vbNewLine
            Next
            If MsgBox("Files that should be deleted were found:" & vbNewLine & vbNewLine & msg & vbNewLine & "Would you like to delete?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                For i = 0 To filelist.Count - 1
                    For j = 0 To GetMapTreeView.GetAllFeatureNodes.Count - 1
                        If GetMapTreeView.GetAllFeatureNodes(j).Features.Features.GetSource = filelist(i) Then
                            ''If GetMapTreeView.GetAllFeatureNodes(j).EditingMode Then
                            ''MsgBox("The Feature " & GetMapTreeView.GetAllFeatureNodes(j).FeatureNodeHeader & " is in edit mode would you like to abort?")
                            '' End If
                            GetMapTreeView.GetAllFeatureNodes(j).RemoveLayer(True)
                        End If
                    Next
                    Kill(filelist(i))
                Next
                'remove the menu item.
                MyBase.Foreground = System.Windows.Media.Brushes.Black
                MyBase.ToolTip = ""
                For Each mi As MenuItem In MyBase.ContextMenu.Items
                    If mi.Header = "Clean Directory" Then
                        MyBase.ContextMenu.Items.Remove(mi) : Exit For
                    End If
                Next
            End If
        End If
    End Sub
    Public Overrides Sub ReadFromXMLElement(xele As XElement)
        If xele.Name = GetNodeName Then
            '_OcctypeFileName = System.IO.Path.GetFileName(xele.Attribute("Path").Value)

            If Not IsNothing(xele.Attribute("Editable")) Then _Loaded = CBool(xele.Attribute("Editable").Value)
            If _Loaded Then
                If System.IO.File.Exists(getOcctypeFilepath) Then
                    IsEditable()
                    checkforerrors()
                Else
                    MsgBox("Your study file suggests that occtypes should exist, but the file " & getOcctypeFilepath & " could not be found.")
                    checkforerrors()
                End If

            End If
        Else
            Throw New ArgumentException("Passed an incorrect occupancy type xml element")
        End If
    End Sub
    Public Overrides Function WriteToXMLElement() As XElement
        Dim ret As New XElement(GetNodeName)
        ret.SetAttributeValue("Path", ConvertToRelativePath(getOcctypeFilepath))
        ret.SetAttributeValue("Editable", _Loaded)
        Return ret
    End Function
    Public Overrides Sub ReadFromXMl(path As String)

    End Sub
    Public Overrides Sub WriteToXML()

    End Sub
    Private Sub WriteToAscii(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim node As DamageCategoryTreeNode = DirectCast(GetAllFrameworkTreenodesOfType(GetType(DamageCategoryTreeNode))(0), DamageCategoryTreeNode)

        If System.IO.File.Exists(node.GetDamageCategoryPath) Then
            If System.IO.File.Exists(getOcctypeFilepath) Then

                Dim ots As New Consequences_Assist.ComputableObjects.OccupancyTypes(getOcctypeFilepath)
                Dim dcs As New Consequences_Assist.ComputableObjects.DamageCategories(node.GetDamageCategoryPath)
                If dcs.GetDamageCategories.Count = 0 Then
                    MsgBox("there are no damage categories defined") : Exit Sub
                ElseIf ots.OccupancyTypes.Count = 0 Then
                    MsgBox("there are no occupancy types defined") : Exit Sub
                Else
                    Dim sfd As New Microsoft.Win32.SaveFileDialog
                    With sfd
                        .Title = "Please select a location to export"
                        .Filter = "text files (*.txt)|*.txt"
                        .InitialDirectory = GetCurrentDirectory
                    End With
                    If sfd.ShowDialog Then
                        Dim fs As New System.IO.FileStream(sfd.FileName, IO.FileMode.Create)
                        Dim sw As New System.IO.StreamWriter(fs)
                        sw.Write(dcs.WriteToFDAString())
                        sw.Write(ots.WriteToFDAString)
                        sw.Close() : sw.Dispose()
                        fs.Close() : fs.Dispose()
                    End If
                End If
            Else
                MsgBox("No Occtype file exists.") : Exit Sub
            End If
        Else
            MsgBox("No damage category file exists.") : Exit Sub
        End If
    End Sub
End Class
