Public Class StructureInventoryChildTreeNode
    Inherits FrameworkTreeNode
    Implements System.ComponentModel.INotifyPropertyChanged
    Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Private Const _parentfolder As String = "Inventory"
    Private Const _FolderName As String = "Structures"
    Private _AddedToMapWindow As Boolean = False
    Private _generationMethod As StructureGenerationMethod = Nothing
    Private _FeatureNode As OpenGLMapping.FeatureNode
    Private _errors As List(Of StructureErorr)
    Public Event RemoveMapFeatureNode(ByVal updatemapwindow As Boolean)
    Sub New(ByVal uniquename As String, ByVal generationmethod As StructureGenerationMethod)
        MyBase.New(uniquename)
        _generationMethod = generationmethod
        _errors = New List(Of StructureErorr)
    End Sub
    Sub New(ByVal xele As XElement)
        MyBase.New(System.IO.Path.GetFileNameWithoutExtension(xele.Attribute("Path").Value))
        _errors = New List(Of StructureErorr)
    End Sub
    Public Property Errors As List(Of StructureErorr)
        Get
            Return _errors
        End Get
        Set(value As List(Of StructureErorr))
            _errors = value
            NotifyPropertyChanged("Errors")
        End Set
    End Property
    Public Overrides ReadOnly Property GetSubDirectory As String
        Get
            Return _parentfolder & "\" & _FolderName
        End Get
    End Property
    Public Overrides ReadOnly Property GetNodeName As String
        Get
            Return _FolderName
        End Get
    End Property
    Public ReadOnly Property GetStructurePath As String
        Get
            Return GetCurrentDirectory & "\" & Header & ".shp"
        End Get
    End Property
    Public Overrides Sub AddFrameworkChildren()
        Dim dbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(GetStructurePath, ".dbf"))
        Dim occtypenames(dbf.NumberOfRows - 1) As String
        Dim damcatnames(dbf.NumberOfRows - 1) As String
        Dim tmpocctypes As Object() = dbf.GetColumn("OccType")
        Dim tmpdamcats As Object() = dbf.GetColumn("DamCat")
        'keep track of occupancy type names and damage categories, do not allow occtype/damcat combos where occtypeA  = occtypeA and damcatA<>DamcatB.
        Dim damagecategorydict As New Dictionary(Of String, String)
        Dim tmpocctype As String
        Dim tmpdamcat As String
        For i = 0 To tmpocctypes.Count - 1
            occtypenames(i) = tmpocctypes(i).ToString
            damcatnames(i) = tmpdamcats(i).ToString
            tmpdamcat = damcatnames(i)
            tmpocctype = occtypenames(i)
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

        Dim otyps As OccupancyTypeTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(OccupancyTypeTreeNode))(0), OccupancyTypeTreeNode)
        Dim dtypes As DamageCategoryTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(DamageCategoryTreeNode))(0), DamageCategoryTreeNode)

        'check to see if occtypes exist
        If System.IO.File.Exists(dtypes.GetDamageCategoryPath) Then
            'check if current damcats include new damcats.
            Dim damcats As New Consequences_Assist.ComputableObjects.DamageCategories(dtypes.GetDamageCategoryPath)
            Dim tmpdamcatobj As Consequences_Assist.ComputableObjects.DamageCategory
            Dim damcatadded As Boolean = False
            For i = 0 To damcatnames.Count - 1
                tmpdamcatobj = damcats.GetDamageCategoryByName(damcatnames(i))
                If IsNothing(tmpdamcatobj) Then
                    damcatadded = True
                    ReportMessage("Damcat " & damcatnames(i) & " does not exist in the current damcat file a blank damcat has been added")
                    damcats.GetDamageCategories.Add(New Consequences_Assist.ComputableObjects.DamageCategory(damcatnames(i))) ' figure out a way to add one.
                End If

            Next
            If damcatadded Then damcats.WriteToXML(dtypes.GetDamageCategoryPath)
            If dtypes.CheckForErrors Then ReportMessage("Damage Categories contain errors")
        Else
            ReportMessage("No damcats currently exist")
            'create a bunch of empty damagecategories?
            Dim dcats As New Consequences_Assist.ComputableObjects.DamageCategories()
            Dim tmpdamcatobj As Consequences_Assist.ComputableObjects.DamageCategory
            For i = 0 To damcatnames.Count - 1
                tmpdamcatobj = dcats.GetDamageCategoryByName(damcatnames(i))
                If IsNothing(tmpdamcatobj) Then
                    ReportMessage("Damcat " & damcatnames(i) & " does not exist in the current damcat file a blank damcat has been added")
                    dcats.GetDamageCategories.Add(New Consequences_Assist.ComputableObjects.DamageCategory(damcatnames(i))) ' figure out a way to add one.
                End If
            Next
            dcats.WriteToXML(dtypes.GetDamageCategoryPath)
            If dtypes.CheckForErrors Then ReportMessage("DamCats contain errors")
        End If
        If System.IO.File.Exists(otyps.getOcctypeFilepath) Then
            Dim occs As New Consequences_Assist.ComputableObjects.OccupancyTypes(otyps.getOcctypeFilepath)
            Dim tmpocctypeobj As Consequences_Assist.ComputableObjects.OccupancyType
            Dim occsadded As Boolean = False
            For i = 0 To damagecategorydict.Count - 1
                tmpocctypeobj = occs.GetOcctypeByNameAndDamCat(damagecategorydict.Keys(i), damagecategorydict(damagecategorydict.Keys(i))) 'what if the occtype exists on a different damaagecategory..
                If IsNothing(tmpocctypeobj) Then
                    Dim testoccsexistunderdifferentname As List(Of Consequences_Assist.ComputableObjects.OccupancyType) = occs.GetOcctypesByName(damagecategorydict.Keys(i))
                    If Not IsNothing(testoccsexistunderdifferentname) AndAlso testoccsexistunderdifferentname.Count > 0 Then
                        'occtype already exists, but not with that damage category?
                        ReportMessage("Occtype " & damagecategorydict.Keys(i) & " does not exist in the current occtype file with damcat " & damagecategorydict(damagecategorydict.Keys(i)) & vbNewLine & "converting all structures with this pairing to match occtype file.")
                        tmpocctypeobj = occs.GetOcctypesByName(damagecategorydict.Keys(i))(0)
                        For j = 0 To occtypenames.Count - 1
                            If occtypenames(j) = damagecategorydict.Keys(i) Then
                                If damcatnames(j) <> tmpocctypeobj.DamageCategory.Name Then
                                    ReportMessage("The Damage Category: " & damcatnames(j) & " is inconsistent with the current occtype file. " & damcatnames(j) & " was changed to " & tmpocctypeobj.DamageCategory.Name & " for the structure at index: " & j)
                                    damcatnames(j) = tmpocctypeobj.DamageCategory.Name

                                End If
                            End If
                        Next
                        dbf.EditColumn("DamCat", damcatnames)
                        'find every structure that has the occtype with a different damage category and switch them to be consistent...
                    Else
                        occsadded = True
                        ReportMessage("Occtype " & damagecategorydict.Keys(i) & " does not exist in the current occtype file a blank occtype has been added")
                        occs.OccupancyTypes.Add(New Consequences_Assist.ComputableObjects.OccupancyType(damagecategorydict.Keys(i), damagecategorydict.Item(damagecategorydict.Keys(i)))) ' figure out a way to add one.
                    End If

                End If

            Next
            If occsadded Then occs.WriteToXML(otyps.getOcctypeFilepath)
            If otyps.checkforerrors Then ReportMessage("Occupancy Types contain errors")
        Else
            'add occtypes. and alert user.
            ReportMessage("No occtypes currently exist")
            'create a bunch of empty occtypes?
            Dim occs As New Consequences_Assist.ComputableObjects.OccupancyTypes()
            Dim tmpocc As Consequences_Assist.ComputableObjects.OccupancyType
            Dim tmpoccs As New List(Of Consequences_Assist.ComputableObjects.OccupancyType)
            For i = 0 To damagecategorydict.Count - 1
                tmpocc = occs.GetOcctypeByNameAndDamCat(damagecategorydict.Keys(i), damagecategorydict.Item(damagecategorydict.Keys(i)))
                If IsNothing(tmpocc) Then
                    tmpoccs = occs.GetOcctypesByName(damagecategorydict.Keys(i))
                    If Not IsNothing(tmpoccs) AndAlso tmpoccs.Count > 0 Then
                        'occtype already exists with a different damage category...
                        tmpocc = tmpoccs(0) ' what if there are more than one occtype??
                        ReportMessage("Occtype " & damagecategorydict.Keys(i) & " does not exist in the current occtype file with damcat " & damagecategorydict(damagecategorydict.Keys(i)) & vbNewLine & "converting all structures with this pairing to match occtype file.")
                        For j = 0 To occtypenames.Count - 1
                            If occtypenames(j) = damagecategorydict.Keys(i) Then
                                If damcatnames(j) <> tmpocc.DamageCategory.Name Then
                                    damcatnames(j) = tmpocc.DamageCategory.Name
                                End If
                            End If
                        Next
                        dbf.EditColumn("DamCat", damcatnames)
                        If dbf.DataBaseOpen Then dbf.Close()
                        'MsgBox("oh snap, tell will to fix this issue.")

                        'find every structure that has the occtype with a different damage category and switch them to be consistent...
                    Else
                        ReportMessage("Occtype " & damagecategorydict.Keys(i) & " does not exist in the current occtype file a blank occtype has been added")
                        occs.OccupancyTypes.Add(New Consequences_Assist.ComputableObjects.OccupancyType(damagecategorydict.Keys(i), damagecategorydict.Item(damagecategorydict.Keys(i)))) ' figure out a way to add one.
                    End If

                End If

            Next
            occs.WriteToXML(otyps.getOcctypeFilepath)
            If otyps.checkforerrors Then ReportMessage("Occupancy Types contain errors")
        End If
        'check to see if the structure occtype list is different than existing occtypes

    End Sub
    Public Function ContainsErrors() As Boolean
        'check for name matches
        Dim errs As New List(Of StructureErorr)
        If System.IO.File.Exists(System.IO.Path.ChangeExtension(GetStructurePath, ".dbf")) Then
            Dim dbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(GetStructurePath, ".dbf"))
            If Not dbf.ColumnNames.Contains("St_Name") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "St_Name")) <> GetType(String) Then errs.Add(New StructureErorr("The column St_Name does not exist, or it is not a String column", 0))
            If Not dbf.ColumnNames.Contains("DamCat") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "DamCat")) <> GetType(String) Then errs.Add(New StructureErorr("The column DamCat does not exist, or it is not a String column", 0))
            If Not dbf.ColumnNames.Contains("OccType") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "OccType")) <> GetType(String) Then errs.Add(New StructureErorr("The column OccType does not exist, or it is not a String column", 0))
            If Not dbf.ColumnNames.Contains("Found_Ht") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "Found_Ht")) <> GetType(Double) Then errs.Add(New StructureErorr("The column Found_Ht does not exist, or it is not a Double column", 0))
            If Not dbf.ColumnNames.Contains("Ground_Ht") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "Ground_Ht")) <> GetType(Double) Then errs.Add(New StructureErorr("The column Ground_Ht does not exist, or it is not a Double column", 0))
            If Not dbf.ColumnNames.Contains("FFE") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "FFE")) <> GetType(Double) Then errs.Add(New StructureErorr("The column FFE does not exist, or it is not a Double column", 0))
            If Not dbf.ColumnNames.Contains("Val_Struct") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "Val_Struct")) <> GetType(Double) Then errs.Add(New StructureErorr("The column Val_Struct does not exist, or it is not a Double column", 0))
            If Not dbf.ColumnNames.Contains("Val_Cont") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "Val_Cont")) <> GetType(Double) Then errs.Add(New StructureErorr("The column Val_Cont does not exist, or it is not a Double column", 0))
            If Not dbf.ColumnNames.Contains("Val_Other") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "Val_Other")) <> GetType(Double) Then errs.Add(New StructureErorr("The column Val_Other does not exist, or it is not a Double column", 0))
            If Not dbf.ColumnNames.Contains("Yr_Built") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "Yr_Built")) <> GetType(Integer) Then errs.Add(New StructureErorr("The column Yr_Built does not exist, or it is not an Integer column", 0))
            If Not dbf.ColumnNames.Contains("Begin_Dmg") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "Begin_Dmg")) <> GetType(Double) Then errs.Add(New StructureErorr("The column Begin_Dmg does not exist, or it is not a Double column", 0))
            If Not dbf.ColumnNames.Contains("Num_Struct") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "Num_Struct")) <> GetType(Integer) Then errs.Add(New StructureErorr("The column Num_Struct does not exist, or it is not an Integer column", 0))
            If Not dbf.ColumnNames.Contains("UseFFE") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "UseFFE")) <> GetType(Boolean) Then errs.Add(New StructureErorr("The column UseFFE does not exist, or it is not a Boolean column", 0))
            If Not dbf.ColumnNames.Contains("UseDBF_GE") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "UseDBF_GE")) <> GetType(Boolean) Then errs.Add(New StructureErorr("The column UseDBF_GE does not exist, or it is not a Boolean column", 0))
            If Not dbf.ColumnNames.Contains("Mod_Name") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "Mod_Name")) <> GetType(String) Then errs.Add(New StructureErorr("The column Mod_Name does not exist, or it is not a String column", 0))

            Dim names(dbf.NumberOfRows - 1) As String
            Dim tmpcolumn As Object() = dbf.GetColumn("St_Name")
            For i = 0 To tmpcolumn.Count - 1
                names(i) = tmpcolumn(i).ToString
            Next
            Dim sval As Double() = Array.ConvertAll(dbf.GetColumn("Val_Struct"), AddressOf Double.Parse)
            Dim cval As Double() = Array.ConvertAll(dbf.GetColumn("Val_Cont"), AddressOf Double.Parse)
            Dim oval As Double() = Array.ConvertAll(dbf.GetColumn("Val_Other"), AddressOf Double.Parse)
            Dim s As StudyTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode)
            Dim tmpnames As New List(Of String)
            For i = 0 To names.Count - 1
                'check for null names
                If names(i) = "" Then errs.Add(New StructureErorr("The structure at index " & i.ToString & " does not have a name defined", i))
                If tmpnames.Contains(names(i)) Then 'check for duplicates
                    errs.Add(New StructureErorr("The structure at index " & i.ToString & " is named " & names(i) & " is not uniquely named", i))
                Else
                    If names(i).Length > 32 Then errs.Add(New StructureErorr("The structure at index " & i.ToString & " is named " & names(i) & " which is more than 32 characters", i))
                    tmpnames.Add(names(i))
                End If
                'check for value issues
                If sval(i) > 99999999999 Then errs.Add(New StructureErorr("The Structure at index " & i.ToString & "has a structure value greater than $99,999,999,999.999 in " & s.MonetaryUnit.ToString, i)) '10.4 
                If cval(i) > 99999999999 Then errs.Add(New StructureErorr("The Structure at index " & i.ToString & "has a content value greater than $99,999,999,999.999 in " & s.MonetaryUnit.ToString, i))
                If oval(i) > 99999999999 Then errs.Add(New StructureErorr("The Structure at index " & i.ToString & "has an other value greater than $99,999,999,999.999 in " & s.MonetaryUnit.ToString, i))
            Next
            Dim otyps As OccupancyTypeTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(OccupancyTypeTreeNode))(0), OccupancyTypeTreeNode)
            Dim dtypes As DamageCategoryTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(DamageCategoryTreeNode))(0), DamageCategoryTreeNode)
            Dim occupancyTypes As New Consequences_Assist.ComputableObjects.OccupancyTypes(otyps.getOcctypeFilepath)
            Dim damagecategories As New Consequences_Assist.ComputableObjects.DamageCategories(dtypes.GetDamageCategoryPath)
            Dim occtypes(dbf.NumberOfRows - 1) As String
            tmpcolumn = dbf.GetColumn("OccType")
            For i = 0 To tmpcolumn.Count - 1
                occtypes(i) = tmpcolumn(i).ToString
            Next
            Dim damcats(dbf.NumberOfRows - 1) As String
            tmpcolumn = dbf.GetColumn("DamCat")
            If dbf.DataBaseOpen Then dbf.Close()
            For i = 0 To tmpcolumn.Count - 1
                damcats(i) = tmpcolumn(i).ToString
            Next
            Dim tmpocc As Consequences_Assist.ComputableObjects.OccupancyType
            Dim tmpocclist As List(Of Consequences_Assist.ComputableObjects.OccupancyType)
            Dim tmpdc As Consequences_Assist.ComputableObjects.DamageCategory
            For i = 0 To occtypes.Count - 1 'check for null occtypes'check for null damcats
                If occtypes(i) = "" Then errs.Add(New StructureErorr("The structure at index " & i.ToString & " does not have an occupancy type name defined", i))
                If damcats(i) = "" Then errs.Add(New StructureErorr("The structure at index " & i.ToString & " does not have a damage category name defined", i))
                tmpocc = occupancyTypes.GetOcctypeByNameAndDamCat(occtypes(i), damcats(i))
                tmpocclist = occupancyTypes.GetOcctypesByName(occtypes(i))
                tmpdc = damagecategories.GetDamageCategoryByName(damcats(i))
                If IsNothing(tmpocc) Then 'check for damcat octype pairs that dont work.
                    errs.Add(New StructureErorr("The structure at index " & i.ToString & " has an occupancy damage category pair that does not exist in the occupancy type list", i))
                End If
                If IsNothing(tmpocclist) AndAlso tmpocclist.Count = 0 Then 'check for occtypes that dont have an occtype in the occtype file
                    errs.Add(New StructureErorr("The structure at index " & i.ToString & " has an occupancy type that does not exist in the occupancy type list", i))
                End If
                If IsNothing(tmpdc) Then 'check for damcats that dont exist in the damcatfile
                    errs.Add(New StructureErorr("The structure at index " & i.ToString & " has a damage category that does not exist in the damage category list", i))
                End If
            Next
        Else
            errs.Add(New StructureErorr("Could not find the dbf file " & System.IO.Path.ChangeExtension(GetStructurePath, ".dbf"), 0))
        End If
        Errors = errs
        If Errors.Count = 0 Then
            For Each mi As MenuItem In MyBase.ContextMenu.Items
                If mi.Header = "View errors" Then
                    MyBase.ContextMenu.Items.Remove(mi)
                    MyBase.Foreground = Brushes.Black
                    MyBase.ToolTip = ""
                    Exit For
                End If
            Next
            Return False
        Else
            MyBase.ToolTip = "there are " & Errors.Count.ToString & " errors"
            Dim loaded As Boolean = False
            For Each mi As MenuItem In MyBase.ContextMenu.Items
                If mi.Header = "View errors" Then
                    loaded = True
                End If
            Next
            If Not loaded Then
                Dim errorreport As New MenuItem()
                errorreport.Header = "View errors"
                AddHandler errorreport.Click, AddressOf createerrormessage
                MyBase.ContextMenu.Items.Add(errorreport)
                MyBase.Foreground = Brushes.Red
            End If
            Return True
        End If
    End Function
    Private Sub createerrormessage(sender As Object, e As System.Windows.RoutedEventArgs)
        'how should i create the error report?
        If _errors.Count = 0 Then MsgBox("There are no errors.") : Exit Sub
        Dim errorreport As New StructureErrorForm(Me)
        errorreport.Owner = GetMainWindow
        errorreport.Show()
    End Sub
    Public Overrides Sub AddFrameworkChildren(ele As System.Xml.Linq.XElement)
        ReadFromXMLElement(ele)
    End Sub
    Public Overrides Sub OnSaveEvent(sender As Object, e As System.Windows.RoutedEventArgs)

    End Sub
    Public Overrides Sub ReadFromXMl(path As String)

    End Sub
    Public Overrides Sub SetContextMenu()
        Dim c As New ContextMenu

        Dim AddToMap As New MenuItem()
        AddToMap.Header = "Add to Map Window"
        AddHandler AddToMap.Click, AddressOf AddToMapWindow
        c.Items.Add(AddToMap)

        Dim FormEditor As New MenuItem()
        FormEditor.Header = "Open Attribute Table"
        AddHandler FormEditor.Click, AddressOf LaunchFormEditor
        c.Items.Add(FormEditor)

        Dim Rename As New MenuItem
        Rename.Header = "Rename"
        AddHandler Rename.Click, AddressOf ReNameStructures
        c.Items.Add(Rename)

        Dim SaveAs As New MenuItem
        SaveAs.Header = "Save As"
        AddHandler SaveAs.Click, AddressOf SaveStructuresAs
        c.Items.Add(SaveAs)

        Dim delete As New MenuItem()
        delete.Header = "Remove From Study"
        AddHandler delete.Click, AddressOf deletefromstudy
        c.Items.Add(delete)

        MyBase.ContextMenu = c
    End Sub
    Private Sub SaveStructuresAs(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim wasaddedtomapwindow As Boolean = False
        'rename the files
        Dim rename As New Rename(Header)
        rename.Title = "Save As"
        rename.Owner = GetMainWindow
        If rename.ShowDialog() Then
            'check for name conflicts.
            Dim ianodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(StructureInventoryChildTreeNode))
            If ianodes.Count > 0 Then
                'potential for name conflicts
                Dim nameconflict As Boolean = False
                Do
                    nameconflict = False
                    For j = 0 To ianodes.Count - 1
                        If ianodes(j).Header = rename.NewName Then
                            nameconflict = True
                        End If
                    Next
                    If nameconflict Then
                        rename = New Rename(rename.NewName & "_1")
                        rename.Title = "Save As"
                        rename.Owner = GetMainWindow
                        If rename.ShowDialog() Then
                            'loop
                        Else
                            'user aborted
                            Exit Sub
                        End If
                    End If
                Loop Until nameconflict = False
            End If
            Dim newname As String = GetCurrentDirectory & "\" & rename.NewName & ".shp"
            Dim parentfile As StructureInventoryParentNode = CType(Parent, StructureInventoryParentNode)
            'copy all the files.
            System.IO.File.Copy(GetStructurePath, newname)
            System.IO.File.Copy(System.IO.Path.ChangeExtension(GetStructurePath, ".shx"), System.IO.Path.ChangeExtension(newname, ".shx"))
            System.IO.File.Copy(System.IO.Path.ChangeExtension(GetStructurePath, ".dbf"), System.IO.Path.ChangeExtension(newname, ".dbf"))
            If System.IO.File.Exists(System.IO.Path.ChangeExtension(GetStructurePath, ".prj")) Then System.IO.File.Copy(System.IO.Path.ChangeExtension(GetStructurePath, ".prj"), System.IO.Path.ChangeExtension(newname, ".prj"))

            Dim childfile As New StructureInventoryChildTreeNode(rename.NewName, _generationMethod)
            parentfile.AddFrameworkTreeNode(childfile)
            childfile.AddFrameworkChildren()
            CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
        Else
            'user closed.
        End If


    End Sub
    Private Sub ReNameStructures(sender As Object, e As System.Windows.RoutedEventArgs)

        'check if any outputs exist that use these structures
        Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
        Dim msg As New System.Text.StringBuilder
        If nodes.Count = 0 Then
        Else
            Dim onodes As New List(Of OutputChildTreeNode)
            For i = 0 To nodes.Count - 1
                onodes.Add(CType(nodes(i), OutputChildTreeNode))
                For j = 0 To onodes.Last.GetPlans.Count - 1
                    If onodes.Last.GetStructureInventoryNode.Header = MyBase.Header Then
                        msg.AppendLine("Structures are used by " & onodes.Last.Header)
                    End If
                Next
            Next
        End If
        Dim dontkill As Boolean = False
        If msg.ToString = "" Then
        Else
            If MsgBox(msg.ToString & vbNewLine & "Would you still like to rename?", MsgBoxStyle.OkCancel, "Warning") = MsgBoxResult.Cancel Then dontkill = True
            'should i do anything for the output nodes? if they say ok?
        End If
        'rename the files
        If Not dontkill Then

            Dim rename As New Rename(Header)
            rename.Owner = GetMainWindow
            If rename.ShowDialog() Then
                Dim wasaddedtomapwindow As Boolean = False
                If _AddedToMapWindow Then
                    'remove it from the map window first.
                    wasaddedtomapwindow = True
                    RemoveFromMapWindow(Nothing, Nothing)
                End If
                'check for name conflicts.
                Dim ianodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(StructureInventoryChildTreeNode))
                If ianodes.Count > 0 Then
                    'potential for name conflicts
                    Dim nameconflict As Boolean = False
                    Do
                        nameconflict = False
                        For j = 0 To ianodes.Count - 1
                            If ianodes(j).Header = rename.NewName Then
                                nameconflict = True
                            End If
                        Next
                        If nameconflict Then
                            rename = New Rename(rename.NewName & "_1")
                            rename.Owner = GetMainWindow
                            If rename.ShowDialog() Then
                                'loop
                            Else
                                'user aborted
                                Exit Sub
                            End If
                        End If
                    Loop Until nameconflict = False
                End If
                Dim newname As String = GetCurrentDirectory & "\" & rename.NewName & ".shp"
                'copy files, kill old files, change header name.
                System.IO.File.Copy(GetStructurePath, newname)
                System.IO.File.Copy(System.IO.Path.ChangeExtension(GetStructurePath, ".shx"), System.IO.Path.ChangeExtension(newname, ".shx"))
                System.IO.File.Copy(System.IO.Path.ChangeExtension(GetStructurePath, ".dbf"), System.IO.Path.ChangeExtension(newname, ".dbf"))
                If System.IO.File.Exists(System.IO.Path.ChangeExtension(GetStructurePath, ".prj")) Then System.IO.File.Copy(System.IO.Path.ChangeExtension(GetStructurePath, ".prj"), System.IO.Path.ChangeExtension(newname, ".prj"))

                Kill(GetStructurePath)
                Kill(System.IO.Path.ChangeExtension(GetStructurePath, ".shx"))
                Kill(System.IO.Path.ChangeExtension(GetStructurePath, ".dbf"))
                If System.IO.File.Exists(System.IO.Path.ChangeExtension(GetStructurePath, ".prj")) Then Kill(System.IO.Path.ChangeExtension(GetStructurePath, ".prj"))
                Dim stn As StudyTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode)

                MyBase.Header = rename.NewName
                If wasaddedtomapwindow Then
                    AddToMapWindow(Nothing, Nothing)
                End If
                stn.WriteToXML()
            End If

        End If

    End Sub
    Sub deletefromstudy(sender As Object, e As System.Windows.RoutedEventArgs)

        'check if any outputs exist that use this inventory.
        Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
        Dim msg As New System.Text.StringBuilder
        If nodes.Count = 0 Then
        Else
            Dim onodes As New List(Of OutputChildTreeNode)
            For i = 0 To nodes.Count - 1
                onodes.Add(CType(nodes(i), OutputChildTreeNode))
                If onodes.Last.GetStructureInventoryNode.GetStructurePath = GetStructurePath Then
                    'we have a match
                    msg.AppendLine("structure inventory is used by " & onodes.Last.Header)
                End If
            Next
        End If
        Dim dontkill As Boolean = False
        If msg.ToString = "" Then
            'If there are no dependencies, then delete the structures
            DeleteMeAndAssociatedFiles()
        Else
            If MsgBox(msg.ToString & vbNewLine & "Would you still like to delete?", MsgBoxStyle.OkCancel, "Warning") = MsgBoxResult.Cancel Then
            Else
                DeleteMeAndAssociatedFiles()
            End If
        End If
    End Sub
    Public Sub DeleteMeAndAssociatedFiles()
        'check if any outputs exist that use this inventory.
        Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
        If nodes.Count = 0 Then
        Else
            For i = 0 To nodes.Count - 1
                If DirectCast(nodes(i), OutputChildTreeNode).GetStructureInventoryNode.GetStructurePath = GetStructurePath Then
                    DirectCast(nodes(i), OutputChildTreeNode).DeleteFromStudy()
                End If
            Next
        End If
        If _AddedToMapWindow Then
            'remove it from the map window first.
            RemoveFromMapWindow(Nothing, Nothing)
        End If
        Try
            Kill(GetStructurePath)
        Catch ex As Exception
            ReportMessage("Error deleting " & GetStructurePath & vbNewLine & ex.Message)
        End Try
        Try
            Kill(System.IO.Path.ChangeExtension(GetStructurePath, ".shx"))
        Catch ex As Exception
            ReportMessage("Error deleting " & System.IO.Path.ChangeExtension(GetStructurePath, ".shx") & vbNewLine & ex.Message)
        End Try
        Try
            Kill(System.IO.Path.ChangeExtension(GetStructurePath, ".dbf"))
        Catch ex As Exception
            ReportMessage("Error deleting " & System.IO.Path.ChangeExtension(GetStructurePath, ".dbf") & vbNewLine & ex.Message)
        End Try
        Try
            If System.IO.File.Exists(System.IO.Path.ChangeExtension(GetStructurePath, ".prj")) Then Kill(System.IO.Path.ChangeExtension(GetStructurePath, ".prj"))
        Catch ex As Exception
            ReportMessage("Error deleting " & System.IO.Path.ChangeExtension(GetStructurePath, ".prj") & vbNewLine & ex.Message)
        End Try
        Dim stn As StudyTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode)
        Dim sn As StructureInventoryParentNode = CType(Parent, StructureInventoryParentNode)
        sn.FirstLevelSubNodes.Remove(Me)
        sn.Items.Remove(Me)
        stn.RemoveFrameworkTreeNode(Me)
        stn.WriteToXML()
    End Sub
    Sub AddToMapWindow(sender As Object, e As System.Windows.RoutedEventArgs)
        If _AddedToMapWindow Then
            'it is already added.
        Else
            If IsNothing(_FeatureNode) Then
                _FeatureNode = New OpenGLMapping.FeatureNode(GetStructurePath, GetMapTreeView.MapWindow)
                _FeatureNode.Features.OpenGLFeatureData.FillColorByAttribute(0) = New OpenTK.Graphics.Color4(CByte(0), CByte(0), CByte(255), CByte(200))
                _FeatureNode.Features.OpenGLFeatureData.LineColorByAttribute(0) = New OpenTK.Graphics.Color4(CByte(0), CByte(0), CByte(255), CByte(255))
                _FeatureNode.Features.OpenGLFeatureData.GlyphSizeByAttribute(0) = 14
                _FeatureNode.Features.OpenGLFeatureData.GlyphTypeByAttribute(0) = OpenGLMapping.OpenGLFeatureInfo.GlyphType.House1
                _FeatureNode.Features.OpenGLFeatureData.BindBitmapSprite(_FeatureNode.Features.OpenGLFeatureData.CreateBitmapSource(0), 0)
            End If


            'add a handler to the remove event to handle the case that the map layer was removed by the user from the maps tab.
            GetMapTreeView.AddGISData(_FeatureNode, 0)
            AddHandler _FeatureNode.RemoveFromMapWindow, AddressOf FeatureRemoved
            AddHandler RemoveMapFeatureNode, AddressOf _FeatureNode.RemoveLayer
            'AddHandler _FeatureNode.AttributesOpening, AddressOf CancelAttributeOpen
            AddHandler _FeatureNode.AttributeEditsSaved, AddressOf DBFEditsSaved
            AddHandler _FeatureNode.FeaturesChanged, AddressOf OnFeaturesChanged
            AddHandler _FeatureNode.PreviewStartFeatureEdit, AddressOf CancelFeatureEdit
        End If

        For Each mi As MenuItem In MyBase.ContextMenu.Items
            If mi.Header = "Add to Map Window" Then
                mi.Header = "Remove From Map Window"
                RemoveHandler mi.Click, AddressOf AddToMapWindow
                AddHandler mi.Click, AddressOf RemoveFromMapWindow
            End If
        Next

        MyBase.FontWeight = System.Windows.FontWeights.Bold
        _AddedToMapWindow = True
    End Sub
    Private Sub DBFEditsSaved()
        Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))

        If ContainsErrors() Then
            If nodes.Count > 0 Then
                For i = 0 To nodes.Count - 1
                    If CType(nodes(i), OutputChildTreeNode).GetStructureInventoryNode.Header = MyBase.Header Then
                        CType(nodes(i), OutputChildTreeNode).AlertUserThatUpdatesAreNeeded("Structure Inventory data has changed (dbf changed), and contains errors.", False, True)
                    End If
                Next
            End If
        Else
            If nodes.Count > 0 Then
                For i = 0 To nodes.Count - 1
                    If CType(nodes(i), OutputChildTreeNode).GetStructureInventoryNode.Header = MyBase.Header Then
                        CType(nodes(i), OutputChildTreeNode).AlertUserThatUpdatesAreNeeded("Structure Inventory data has changed (dbf changed).", True, False)
                    End If
                Next
            End If
        End If
    End Sub
    Private Sub OnFeaturesChanged()
        Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
        If ContainsErrors() Then
            If nodes.Count > 0 Then
                For i = 0 To nodes.Count - 1
                    If CType(nodes(i), OutputChildTreeNode).GetStructureInventoryNode.Header = MyBase.Header Then
                        CType(nodes(i), OutputChildTreeNode).AlertUserThatUpdatesAreNeeded("Structure Inventory data has changed (features changed), and contains errors.", False, True)
                    End If
                Next
            End If
        Else
            If nodes.Count > 0 Then
                For i = 0 To nodes.Count - 1
                    If CType(nodes(i), OutputChildTreeNode).GetStructureInventoryNode.Header = MyBase.Header Then
                        CType(nodes(i), OutputChildTreeNode).AlertUserThatUpdatesAreNeeded("Structure Inventory data has changed (features changed).", True, False)
                    End If
                Next
            End If
        End If
    End Sub
    Private Sub CancelFeatureEdit(ByRef cancel As Boolean)
        If MsgBox("You are opening an edit session on the structure inventory shapefile, it is possible that the edits you do can significantly impact your data, would you like to continue?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
        Else
            cancel = True
        End If
    End Sub
    Private Sub LaunchFormEditor(ByVal sender As Object, e As System.Windows.RoutedEventArgs)
        If _AddedToMapWindow Then
            ' how do i get the correct feature node?
            'check if attribute table is open.
            If _FeatureNode.AttributesOpen Then
                'check if there are any edits.

                _FeatureNode.GetAttributeTable.BringIntoView() : Exit Sub

                Else
                'keep goin?
                _FeatureNode.OpenAttributes()
                End If
        Else
            'does it need to be in the map window? - yes, or the attribute table needs to be loaded.
            MsgBox("Structure inventory needs to be added to map window to edit. GeoFDA is loading the structure inventory into the map window.")
            AddToMapWindow(Nothing, Nothing)

            _FeatureNode.OpenAttributes()
        End If
        'Dim ste As New StructureTableEditor(_FeatureNode, Me)
        'ste.Owner = GetMainWindow
        'ste.ShowDialog()
        'ContainsErrors()
    End Sub
    Private Sub RemoveFromMapWindow(sender As Object, e As System.Windows.RoutedEventArgs)
        'how do i know which one to remove?
        RaiseEvent RemoveMapFeatureNode(True)
        _FeatureNode = Nothing
        For Each mi As MenuItem In MyBase.ContextMenu.Items
            If mi.Header = "Remove From Map Window" Then
                mi.Header = "Add to Map Window"
                AddHandler mi.Click, AddressOf AddToMapWindow
                RemoveHandler mi.Click, AddressOf RemoveFromMapWindow
            End If
        Next
        MyBase.FontWeight = System.Windows.FontWeights.Normal
        _AddedToMapWindow = False
    End Sub
    Private Sub FeatureRemoved(fn As OpenGLMapping.FeatureNode)
        RemoveHandler RemoveMapFeatureNode, AddressOf fn.RemoveLayer
        For Each mi As MenuItem In MyBase.ContextMenu.Items
            If mi.Header = "Remove From Map Window" Then
                mi.Header = "Add to Map Window"
                AddHandler mi.Click, AddressOf AddToMapWindow
                RemoveHandler mi.Click, AddressOf RemoveFromMapWindow
            End If
        Next
        MyBase.FontWeight = System.Windows.FontWeights.Normal
        _AddedToMapWindow = False
    End Sub
    Public Overrides Function WriteToXMLElement() As System.Xml.Linq.XElement
        Dim sasp As New XElement(GetNodeName)
        sasp.SetAttributeValue("Path", ConvertToRelativePath(GetStructurePath))
        Return sasp
    End Function
    Public Overrides Sub ReadFromXMLElement(xele As System.Xml.Linq.XElement)
        For Each fn As OpenGLMapping.FeatureNode In GetMapTreeView.MapWindow.GISFeatures
            If Not IsNothing(fn.Features.Features) AndAlso fn.Features.Features.GetSource = GetStructurePath Then
                _AddedToMapWindow = True
                _FeatureNode = fn
                AddHandler _FeatureNode.RemoveFromMapWindow, AddressOf FeatureRemoved
                AddHandler RemoveMapFeatureNode, AddressOf _FeatureNode.RemoveLayer
                'AddHandler _FeatureNode.AttributesOpening, AddressOf CancelAttributeOpen
                AddHandler _FeatureNode.AttributeEditsSaved, AddressOf DBFEditsSaved
                AddHandler _FeatureNode.FeaturesChanged, AddressOf OnFeaturesChanged
                AddHandler _FeatureNode.PreviewStartFeatureEdit, AddressOf CancelFeatureEdit
            End If
        Next
        If _AddedToMapWindow Then AddToMapWindow(Me, Nothing)
        ContainsErrors()
    End Sub
    Public Overrides Sub WriteToXML()

    End Sub
    Private Sub NotifyPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(info))
    End Sub
    Private Sub WriteToAscii(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim node As DamageCategoryTreeNode = DirectCast(GetAllFrameworkTreenodesOfType(GetType(DamageCategoryTreeNode))(0), DamageCategoryTreeNode)
        Dim otnode As OccupancyTypeTreeNode = DirectCast(GetAllFrameworkTreenodesOfType(GetType(OccupancyTypeTreeNode))(0), OccupancyTypeTreeNode)
        If System.IO.File.Exists(node.GetDamageCategoryPath) Then
            If System.IO.File.Exists(otnode.getOcctypeFilepath) Then
                Dim ots As New Consequences_Assist.ComputableObjects.OccupancyTypes(otnode.getOcctypeFilepath)
                Dim dcs As New Consequences_Assist.ComputableObjects.DamageCategories(node.GetDamageCategoryPath)
                'Dim structs As New Consequences_Assist.ComputableObjects
                ''need impact areas to define impact areas.

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
