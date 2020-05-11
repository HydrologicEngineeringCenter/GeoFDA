Public Class DamageCategoryTreeNode
    Inherits FrameworkTreeNode
    Private Const _ParentFolder As String = "Inventory"
    Private Const _Foldername As String = "DamageCategories"
    Private Const _header As String = "Damage Categories"
    Private _UpdateOutputTreeNodes As Boolean = False
    'Private _damcats As Consequences_Assist.ComputableObjects.DamageCategories
    Sub New()
        MyBase.New(_header)

    End Sub
    Sub New(xel As XElement)
        MyBase.New(_header)

    End Sub
    Public ReadOnly Property GetDamageCategoryPath As String
        Get
            Return GetCurrentDirectory & "\DamCats.xml"
        End Get
    End Property
    Public Overrides Sub SetContextMenu()
        Dim c As New ContextMenu

        Dim ImportDamageCategories As New MenuItem
        ImportDamageCategories.Header = "Import Damage Categories"
        AddHandler ImportDamageCategories.Click, AddressOf ImportDamageCats
        c.Items.Add(ImportDamageCategories)

        Dim DamageCategoryEditor As New MenuItem
        DamageCategoryEditor.Header = "Edit Damage Categories"
        AddHandler DamageCategoryEditor.Click, AddressOf EditDamageCategories

        Dim DamageCategoryExporter As New MenuItem
        DamageCategoryExporter.Header = "Export Damage Categories to Ascii"
        AddHandler DamageCategoryExporter.Click, AddressOf ExportDamageCategories

        c.Items.Add(DamageCategoryEditor)

        Dim ImportDamageCategoriesMain As New MenuItem
        ImportDamageCategoriesMain.Header = "_Import Damage Categories"
        AddHandler ImportDamageCategoriesMain.Click, AddressOf ImportDamageCats

        For Each mi As MenuItem In GetMainContextMenu.Items
            If mi.Header = "_Study" Then
                For Each submi As MenuItem In mi.Items
                    If submi.Header = "_Inventory" Then
                        submi.Items.Add(ImportDamageCategoriesMain)
                    End If
                Next
            End If
        Next

        MyBase.ContextMenu = c
    End Sub
    Private Sub ImportDamageCats(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim ofd As New Microsoft.Win32.OpenFileDialog
        With ofd
            .Title = "Please select an existing FDA import file with Damage Categories"
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


                    If System.IO.File.Exists(GetDamageCategoryPath) Then
                        'something already exists, and they have browsed to something new.
                        MsgBox("Deleting old damcat file")
                        Kill(GetDamageCategoryPath)
                    End If
                    'they browsed to a text file. Read the file and check for errors
                    'write out to xml in the study directory the corrected file.
                    CreateXMLFromTxt(ofd.FileName)

                Case ".xml"
                    'they browsed to an xml file, make sure the contents are correct?
                    'copy to the study directory
                    If GetDamageCategoryPath = ofd.FileName Then
                        'they browsed to their own file
                    Else
                        If System.IO.File.Exists(GetDamageCategoryPath) Then
                            'something already exists, and they have browsed to something new.
                            MsgBox("Deleting old damcat file")
                            Kill(GetDamageCategoryPath)
                        End If
                        System.IO.File.Copy(ofd.FileName, GetDamageCategoryPath)
                    End If
                Case Else
                    MsgBox("File type not supported") : Exit Sub
            End Select
            '_Loaded = True
            'IsEditable()
        Else
            MsgBox("You did not select a damage category file.")
        End If
    End Sub
    Private Sub EditDamageCategories()
        'damage category name, description and price index
        _UpdateOutputTreeNodes = False
        Dim dce As EditDamageCategories
        If System.IO.File.Exists(GetDamageCategoryPath) Then
            Dim damcats As New Consequences_Assist.ComputableObjects.DamageCategories(GetDamageCategoryPath)
            dce = New EditDamageCategories(damcats)
        Else
            dce = New EditDamageCategories()
        End If
        dce.Owner = GetMainWindow
        AddHandler dce.DamageCategoryDeleted, AddressOf OnDamageCategoryDeleted
        AddHandler dce.DamageCategoryRenamed, AddressOf OnDamageCategoryRenamed
        dce.ShowDialog()
        If dce.DialogResult Then
            'ok
            dce.GetDamageCategories.WriteToXML(GetDamageCategoryPath)
        Else
            'not ok
        End If
        Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
        If CheckForErrors() Then
            If _UpdateOutputTreeNodes Then
                If nodes.Count = 0 Then
                    'no update
                Else
                    ''update and notify there are errors
                    For Each output As OutputChildTreeNode In nodes
                        output.AlertUserThatUpdatesAreNeeded("Damage Categories changed and contain errors, fix the errors and update output.", False, True)
                    Next
                End If
            End If
        Else
            If _UpdateOutputTreeNodes Then
                If nodes.Count = 0 Then
                    'no update
                Else
                    ''update and notify there arent errors
                    For Each output As OutputChildTreeNode In nodes
                        output.AlertUserThatUpdatesAreNeeded("Damage Categories changed please update output.", True, False)
                    Next
                End If
            End If
        End If
    End Sub
    Private Sub OnDamageCategoryRenamed(ByVal Damcat As Consequences_Assist.ComputableObjects.DamageCategory, ByVal newname As String)
        'update all occupancy types
        'update all structures

        Dim otnode As OccupancyTypeTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(OccupancyTypeTreeNode))(0), OccupancyTypeTreeNode)
        If System.IO.File.Exists(otnode.getOcctypeFilepath) Then
            Dim ot As New Consequences_Assist.ComputableObjects.OccupancyTypes(otnode.getOcctypeFilepath)
            For i = 0 To ot.OccupancyTypes.Count - 1
                If ot.OccupancyTypes(i).DamageCategory.Name = Damcat.Name Then
                    ot.OccupancyTypes(i).DamageCategory.Name = newname
                    _UpdateOutputTreeNodes = True
                Else
                    'do nothing
                End If
            Next
            'write occtypes back to file
            ot.WriteToXML(otnode.getOcctypeFilepath)
        End If

        'structures.
        Dim fnodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(StructureInventoryChildTreeNode))
        If IsNothing(fnodes) OrElse fnodes.Count = 0 Then Exit Sub
        Dim snodes As New List(Of StructureInventoryChildTreeNode)
        For i = 0 To fnodes.Count - 1
            snodes.Add(CType(fnodes(i), StructureInventoryChildTreeNode))
        Next
        For i = 0 To snodes.Count - 1
            'get the structure dbf
            If System.IO.File.Exists(snodes(i).GetStructurePath) Then
                Dim dbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(snodes(i).GetStructurePath, ".dbf"))
                Dim dindex As Integer = Array.IndexOf(dbf.ColumnNames, "DamCat")
                For j = 0 To dbf.NumberOfRows - 1
                    If dbf.GetCell(dindex, i) = Damcat.Name Then
                        dbf.EditCell(dindex, i, newname)
                        _UpdateOutputTreeNodes = True
                    End If
                Next
                dbf.Close()
            Else
            End If
        Next


    End Sub
    Private Sub OnDamageCategoryDeleted(ByVal damcats As List(Of Consequences_Assist.ComputableObjects.DamageCategory), ByRef cancel As Boolean)
        Dim message As New System.Text.StringBuilder

        Dim otnode As OccupancyTypeTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(OccupancyTypeTreeNode))(0), OccupancyTypeTreeNode)
        If System.IO.File.Exists(otnode.getOcctypeFilepath) Then
            Dim ot As New Consequences_Assist.ComputableObjects.OccupancyTypes(otnode.getOcctypeFilepath)
            For j = 0 To damcats.Count - 1

                Dim occtypecounter As Integer = 0
                For i = 0 To ot.OccupancyTypes.Count - 1
                    If ot.OccupancyTypes(i).DamageCategory.Name = damcats(j).Name Then
                        'increment a counter

                        occtypecounter += 1
                    Else
                        'do nothing
                    End If

                Next
                If occtypecounter > 0 Then
                    If occtypecounter = 1 Then
                        message.Append("The Damage Category " & damcats(j).Name & " impacts the occupancy types " & occtypecounter & " time" & vbNewLine)
                    Else
                        message.Append("The Damage Category " & damcats(j).Name & " impacts the occupancy types " & occtypecounter & " times" & vbNewLine)
                    End If
                End If
            Next
        Else
            'no occtype checks
        End If


        Dim fnodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(StructureInventoryChildTreeNode))

        If IsNothing(fnodes) OrElse fnodes.Count = 0 Then
        Else
            Dim snodes As New List(Of StructureInventoryChildTreeNode)
            For i = 0 To fnodes.Count - 1
                snodes.Add(CType(fnodes(i), StructureInventoryChildTreeNode))
            Next

            For j = 0 To damcats.Count - 1
                'structures.

                For i = 0 To snodes.Count - 1
                    'get the structure dbf
                    Dim structurecounter As Integer = 0
                    Dim dbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(snodes(i).GetStructurePath, ".dbf"))
                    Dim dindex As Integer = Array.IndexOf(dbf.ColumnNames, "DamCat")
                    For k = 0 To dbf.NumberOfRows - 1
                        If dbf.GetCell(dindex, k) = damcats(j).Name Then
                            'increment a counter
                            structurecounter += 1
                        End If
                    Next
                    If structurecounter > 0 Then
                        If structurecounter = 1 Then
                            message.Append("The Damage Category " & damcats(j).Name & " impacts the structure inventory " & snodes(i).Header & " " & structurecounter & " time" & vbNewLine)
                        Else
                            message.Append("The Damage Category " & damcats(j).Name & " impacts the structure inventory " & snodes(i).Header & " " & structurecounter & " times" & vbNewLine)
                        End If
                    Else

                    End If

                    dbf.Close()
                Next
            Next
        End If
        If message.ToString = "" Then
            'no prob bob.  delete happily
            'MsgBox("The selected damage categories were used by no structures or occtypes. They have been deleted.")


        Else
            'check if they really want to delete.
            If Not System.IO.File.Exists(GetDamageCategoryPath) Then Exit Sub
            Dim dcats As New Consequences_Assist.ComputableObjects.DamageCategories(GetDamageCategoryPath)
            Dim lst As New List(Of Consequences_Assist.ComputableObjects.DamageCategory)
            For i = 0 To dcats.GetDamageCategories.Count - 1
                Dim addit As Boolean = True
                For j = 0 To damcats.Count - 1
                    If dcats.GetDamageCategories(i).Name = damcats(j).Name Then
                        addit = False
                    Else
                        'dont change it back to true
                    End If
                Next
                If addit Then lst.Add(dcats.GetDamageCategories(i))
            Next
            If lst.Count = 0 Then
                MsgBox(message.ToString & "This leaves no damage categories to assign, this action must be aborted.")
                cancel = True
            Else
                Dim damcatdeleted As New DamageCategoryDeleted(message.ToString, lst)
                If damcatdeleted.ShowDialog Then
                    'delete them and switch to selected 
                    Dim selection As Consequences_Assist.ComputableObjects.DamageCategory = damcatdeleted.CmbRemainingDamCats.SelectedItem
                    If System.IO.File.Exists(otnode.getOcctypeFilepath) Then
                        Dim ot As New Consequences_Assist.ComputableObjects.OccupancyTypes(otnode.getOcctypeFilepath)
                        For j = 0 To damcats.Count - 1
                            For i = 0 To ot.OccupancyTypes.Count - 1
                                If ot.OccupancyTypes(i).DamageCategory.Name = damcats(j).Name Then
                                    ot.OccupancyTypes(i).DamageCategory = selection
                                Else
                                    'do nothing
                                End If

                            Next
                        Next
                        ot.WriteToXML(otnode.getOcctypeFilepath)
                    Else

                    End If

                    If IsNothing(fnodes) OrElse fnodes.Count = 0 Then
                    Else
                        Dim snodes As New List(Of StructureInventoryChildTreeNode)
                        For i = 0 To fnodes.Count - 1
                            snodes.Add(CType(fnodes(i), StructureInventoryChildTreeNode))
                        Next
                        For j = 0 To damcats.Count - 1
                            'structures.
                            For i = 0 To snodes.Count - 1
                                'get the structure dbf
                                Dim structurecounter As Integer = 0
                                Dim dbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(snodes(i).GetStructurePath, ".dbf"))
                                Dim dindex As Integer = Array.IndexOf(dbf.ColumnNames, "DamCat")
                                For k = 0 To dbf.NumberOfRows - 1
                                    If dbf.GetCell(dindex, k) = damcats(j).Name Then
                                        dbf.EditCell(dindex, k, selection.Name)
                                    End If
                                Next
                                dbf.Close()
                            Next
                        Next
                    End If
                    _UpdateOutputTreeNodes = True
                    ''invalidate all outputchild tree nodes.
                Else
                    cancel = True
                End If
                End If
            'cancel = True
        End If
    End Sub
    Public Function CreateXMLFromTxt(ByVal textfilepath As String) As Consequences_Assist.ComputableObjects.DamageCategories
        Try
            Dim dc As New Consequences_Assist.ComputableObjects.DamageCategories(textfilepath)
            'dc.WriteToXML(GetDamageCategoryPath)
            Return dc
        Catch ex As Exception
            MsgBox("Error importing Damage Categories during Import" & vbNewLine & vbNewLine & ex.Message)
            Return Nothing
        End Try
    End Function
    Public Function CheckForErrors() As Boolean
        Dim count As Integer = 0
        If Not System.IO.File.Exists(GetDamageCategoryPath) Then
            count += 1
        Else
            Dim dcats As New Consequences_Assist.ComputableObjects.DamageCategories(GetDamageCategoryPath)

            For i = 0 To dcats.GetDamageCategories.Count - 1
                If dcats.GetDamageCategories(i).Name.Length >= 32 Then
                    count += 1
                ElseIf IsNothing(dcats.GetDamageCategories(i).Name) OrElse dcats.GetDamageCategories(i).Name = "" Then
                    count += 1
                End If
            Next
        End If
        If count = 0 Then
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
            MyBase.ToolTip = "there are " & count.ToString & " errors"
            Dim loaded As Boolean = False
            For Each mi As MenuItem In MyBase.ContextMenu.Items
                If mi.Header = "View errors" Then
                    loaded = True
                End If
            Next
            If Not loaded Then
                Dim errorreport As New MenuItem()
                errorreport.Header = "View errors"
                AddHandler errorreport.Click, AddressOf CreateErrorReport
                MyBase.ContextMenu.Items.Add(errorreport)
                MyBase.Foreground = Brushes.Red
            End If
            Return True
        End If
    End Function
    Public Sub CreateErrorReport(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim msg As New System.Text.StringBuilder()
        If Not System.IO.File.Exists(GetDamageCategoryPath) Then
            msg.AppendLine("The damage category file path does not exist")
        Else
            Dim dcats As New Consequences_Assist.ComputableObjects.DamageCategories(GetDamageCategoryPath)

            For i = 0 To dcats.GetDamageCategories.Count - 1
                If dcats.GetDamageCategories(i).Name.Length >= 32 Then
                    msg.AppendLine("The damage category: " & dcats.GetDamageCategories(i).Name & " has a name longer than 32 characters.")
                ElseIf IsNothing(dcats.GetDamageCategories(i).Name) OrElse dcats.GetDamageCategories(i).Name = "" Then
                    msg.AppendLine("A damage category at row: " & i & " has no name.")
                End If
            Next
        End If
        If msg.Length > 0 Then
            MsgBox(msg.ToString)
        Else
            MsgBox("No errors were found.")
        End If
    End Sub
    Public Overrides Sub OnSaveEvent(sender As Object, e As System.Windows.RoutedEventArgs)
        'If Not IsNothing(_damcats) Then WriteToXML()
    End Sub
    Public Overrides Sub ReadFromXMl(path As String)
        '_damcats = New Consequences_Assist.ComputableObjects.DamageCategories(path)
    End Sub
    Public Overrides Function WriteToXMLElement() As XElement
        Dim xel As New XElement(GetNodeName)
        xel.SetAttributeValue("Path", ConvertToRelativePath(GetDamageCategoryPath))
        Return xel
    End Function
    Public Overrides Sub AddFrameworkChildren(ele As XElement)
        If System.IO.File.Exists(GetDamageCategoryPath) Then
            'read the file and load the damage category stuff
            'ReadFromXMl(GetDamageCategoryPath)

        End If
        Dim dinfo As New System.IO.DirectoryInfo(GetCurrentDirectory)
        Dim cleandir As Boolean = False
        For Each file As System.IO.FileInfo In dinfo.GetFiles
            Dim keepfile As Boolean = False
            If file.FullName = GetDamageCategoryPath Then
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
                If file.FullName = GetDamageCategoryPath Then
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
        If System.IO.File.Exists(GetDamageCategoryPath) Then
            'read the file and load the damage category stuff
            ReadFromXMl(GetDamageCategoryPath)
        End If
    End Sub
    Public Overrides Sub WriteToXML()
        '_damcats.WriteToXML(GetDamageCategoryPath)
    End Sub

    Public Overrides ReadOnly Property GetSubDirectory As String
        Get
            Return _ParentFolder & "\" & _Foldername
        End Get
    End Property
    Public Overrides ReadOnly Property GetNodeName As String
        Get
            Return _Foldername
        End Get
    End Property

    Private Sub ExportDamageCategories(sender As Object, e As RoutedEventArgs)
        If System.IO.File.Exists(GetDamageCategoryPath) Then
            Dim dcs As New Consequences_Assist.ComputableObjects.DamageCategories(GetDamageCategoryPath)
            If dcs.GetDamageCategories.Count = 0 Then
                MsgBox("there are no damage categories defined") : Exit Sub
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
                    sw.Close() : sw.Dispose()
                    fs.Close() : fs.Dispose()
                End If
            End If
        Else
            MsgBox("No damage category file exists.") : Exit Sub
        End If
    End Sub

End Class
