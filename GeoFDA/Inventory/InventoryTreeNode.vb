Public Class InventoryTreeNode
    Inherits FrameworkTreeNode
    Private Const _Subdirectory As String = "Inventory"
    Sub New()
        MyBase.New(_Subdirectory)
        '' add sub treenodes 
        'SetContextMenu()
    End Sub
    'Sub New(ByVal directorypath As String, ByVal headername As String, ByVal mapwindow As OpenGLMapping.MapTreeView)
    '    MyBase.New(headername)
    'End Sub
    Public Overrides ReadOnly Property GetSubDirectory As String
        Get
            Return _Subdirectory
        End Get
    End Property
    Public Overrides ReadOnly Property GetNodeName As String
        Get
            Return _Subdirectory
        End Get
    End Property
    Public Overrides Sub SetContextMenu()
        Dim importfromFda As New MenuItem
		importfromFda.Header = "Import From FDA ASCII file"
		AddHandler importfromFda.Click, AddressOf ImportFromTxtFile
        Dim c As New ContextMenu
        c.Items.Add(importfromFda)

        Dim importfromFdaMAIN As New MenuItem
		importfromFdaMAIN.Header = "Import From _FDA ASCII file"
		AddHandler importfromFdaMAIN.Click, AddressOf ImportFromTxtFile

        For Each mi As MenuItem In GetMainContextMenu.Items
            If mi.Header = "_Study" Then
                Dim tm As New MenuItem()
                tm.Header = "_Inventory"
                tm.Items.Add(importfromFdaMAIN)
                mi.Items.Add(tm)
            End If
        Next

        MyBase.ContextMenu = c
    End Sub
    'Public Overrides Sub RemoveMainMenuContextOptions()
    '    MyBase.RemoveMainMenuContextOptions()
    '    For Each mi As MenuItem In GetMainContextMenu.Items
    '        If mi.Header = "_Study" Then
    '            For Each submi As MenuItem In mi.Items
    '                If submi.Header = "_Inventory" Then RemoveHandler CType(submi.Items(0), MenuItem).Click, AddressOf ImportFromTxtFile
    '                submi.Items.RemoveAt(0)
    '                mi.Items.Remove(submi)
    '            Next
    '        End If
    '    Next
    'End Sub
    Private Sub ImportFromTxtFile(ByVal sender As Object, e As RoutedEventArgs)
        Dim dc As DamageCategoryTreeNode = CType(GetFrameworkTreenodeByNameAndType("Damage Categories", GetType(DamageCategoryTreeNode)), DamageCategoryTreeNode)
        Dim ot As OccupancyTypeTreeNode = CType(GetFrameworkTreenodeByNameAndType("Occupancy Types", GetType(OccupancyTypeTreeNode)), OccupancyTypeTreeNode)
        Dim ofd As New Microsoft.Win32.OpenFileDialog
        With ofd
			.Title = "Please select an existing ASCII text file"
			.Filter = "text files (*.txt)|*.txt|FDA import files (GeoFDA) (*.fdai)|*.fdai|All Files (*.*)|*.*"
            .CheckFileExists = True
            .Multiselect = False
            '.InitialDirectory = GetCurrentDirectory
        End With

        If ofd.ShowDialog Then
            'copy to study directory
            'select case on the file.
            Select Case System.IO.Path.GetExtension(ofd.FileName).ToLower
                Case ".txt", ".fdai"
                    'load damcats
                    If (MsgBox("If damage categories and occupancy types are detected this proccess will overwrite those existing files. Would you like to continue?", MsgBoxStyle.YesNo) = MsgBoxResult.No) Then Exit Sub
                    Dim tmpdcs As Consequences_Assist.ComputableObjects.DamageCategories = dc.CreateXMLFromTxt(ofd.FileName)
                    If IsNothing(tmpdcs) Then
                    Else
                        If System.IO.File.Exists(dc.GetDamageCategoryPath) Then
                            'something already exists, and they have browsed to something new.
                            ReportMessage("Deleting old damcat file")
                            Kill(dc.GetDamageCategoryPath)
                        End If
                        tmpdcs.WriteToXML(dc.GetDamageCategoryPath)
                        CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
                    End If
                        'they browsed to a text file. Read the file and check for errors
                        'write out to xml in the study directory the corrected file.

                    'load occtypes
                    Dim tmpoccs As Consequences_Assist.ComputableObjects.OccupancyTypes = ot.CreateXMLFromTxt(ofd.FileName)
                    If IsNothing(tmpoccs) Then
                    Else
                        If System.IO.File.Exists(ot.getOcctypeFilepath) Then
                            'something already exists, and they have browsed to something new.
                            ReportMessage("Deleting old occtype file")
                            Kill(ot.getOcctypeFilepath)
                        End If
                        tmpoccs.WriteToXML(ot.getOcctypeFilepath)
                        ot.checkforerrors()
                        CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
                    End If


                        'check if files exist
                    If System.IO.File.Exists(ot.getOcctypeFilepath) And System.IO.File.Exists(dc.GetDamageCategoryPath) Then
                        Dim tmpdc As New Consequences_Assist.ComputableObjects.DamageCategories(dc.GetDamageCategoryPath)
                        If tmpdc.GetDamageCategories.Count = 0 Then MsgBox("No damage categories were found, so structures cannot be created") : Exit Sub
                        Dim occtypes As New Consequences_Assist.ComputableObjects.OccupancyTypes(ot.getOcctypeFilepath)
                        If occtypes.OccupancyTypes.Count = 0 Then MsgBox("No occupancy types were found, so structures cannot be created") : Exit Sub
                        Try
                            Dim snode As StudyTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode)
                            Dim s As New Consequences_Assist.ComputableObjects.Consequences_Inventory
                            AddHandler s.ReportMessage, AddressOf ReportMessage
                            'load structures if they are geospatial.
                            Dim importer As New ImportStructuresFromFDAFile(snode.MonetaryUnit, GetCurrentDirectory) 'it would be nice if they didnt have to do this step if the structure inventory isnt there.
                            importer.Owner = GetMainWindow
                            If importer.ShowDialog() Then
                                'success
                                'check for name conflicts first
                                Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(StructureInventoryChildTreeNode))
                                If nodes.Count = 0 Then
                                Else
                                    Dim existingstructurenames As New List(Of String)
                                    For i = 0 To nodes.Count - 1
                                        existingstructurenames.Add(CType(nodes(i), StructureInventoryChildTreeNode).Header)

                                    Next
                                    Dim nonameconflicts As Boolean = False
                                    Do Until nonameconflicts
                                        If existingstructurenames.Contains(importer.TxtName.Text) Then
                                            MsgBox("Name already exists, please select a different name")
                                            importer = New ImportStructuresFromFDAFile(snode.MonetaryUnit, GetCurrentDirectory)
                                            importer.TxtName.Text &= "_1"
                                            If importer.ShowDialog Then
                                            Else
                                                MsgBox("User aborted") : Exit Sub
                                            End If
                                        Else
                                            nonameconflicts = True
                                        End If
                                    Loop
                                End If

                                If snode.MonetaryUnit = importer.CMBMonetaryUnits.SelectedItem.ToString Then
                                    'all is well
                                    s.LoadFromFDATxtFile(ofd.FileName, occtypes, snode.MonetaryUnit, snode.MonetaryUnit)
                                Else
                                    'convert to new unit
                                    s.LoadFromFDATxtFile(ofd.FileName, occtypes, importer.CMBMonetaryUnits.SelectedItem.ToString, snode.MonetaryUnit)
                                End If
                                If s.Structures.Count = 0 Then
                                    'not successful abort.
                                    MsgBox("import of structures yeilded no structures, either something happened, or you had no structures in the import file.")
                                Else
                                    Dim sp As StructureInventoryParentNode = CType(GetFrameworkTreenodeByNameAndType("Structure Inventories", GetType(StructureInventoryParentNode)), StructureInventoryParentNode)
                                    If importer.HasProjection Then
                                        Dim proj As New GDALAssist.ESRIProjection(importer.TxtProjection.Text)
                                        s.WriteToFDAShapefile(sp.GetCurrentDirectory & importer.TxtName.Text & ".shp", proj)
                                        Dim c As New StructureInventoryChildTreeNode(importer.TxtName.Text, StructureGenerationMethod.FDAAsciiFile)
                                        sp.AddFrameworkTreeNode(c)
                                        c.AddFrameworkChildren()
                                        CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
                                    Else
                                        'should i use the maps projection?

                                        'check for no projection...

                                        s.WriteToFDAShapefile(sp.GetCurrentDirectory & "\" & importer.TxtName.Text & ".shp", GetMapTreeView.Projection)
                                        Dim c As New StructureInventoryChildTreeNode(importer.TxtName.Text, StructureGenerationMethod.FDAAsciiFile)
                                        sp.AddFrameworkTreeNode(c)
                                        c.AddFrameworkChildren()
                                        CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
                                    End If
                                End If

                            Else
                                'failure
                                MsgBox("You did not provide a name for the structure inventory, no inventory can be added to the GeoFDA project. However, Damage Categories and Occupancy Types were added.")
                                Kill(dc.GetDamageCategoryPath)
                                Kill(ot.getOcctypeFilepath)
                            End If

                        Catch ex As Exception
                            ReportMessage(ex.Message)
                        End Try
                    Else
                        MsgBox("Either the Occupancy Type File or the Damage Category File is missing.")
                    End If


                Case Else
                        MsgBox("File type not supported") : Exit Sub
            End Select
        Else
            MsgBox("You did not select any file.")
        End If





    End Sub
    Public Overrides Sub AddFrameworkChildren()
        MyBase.AddFrameworkTreeNode(New DamageCategoryTreeNode())
        MyBase.AddFrameworkTreeNode(New OccupancyTypeTreeNode())
        MyBase.AddFrameworkTreeNode(New StructureInventoryParentNode())
    End Sub
    Public Overrides Sub AddFrameworkChildren(ele As System.Xml.Linq.XElement)
        ReadFromXMLElement(ele)
        Dim dinfo As New System.IO.DirectoryInfo(GetCurrentDirectory)
        Dim cleandir As Boolean = False
        For Each file As System.IO.FileInfo In dinfo.GetFiles
            Dim keepfile As Boolean = False
            For Each Header As FrameworkTreeNode In FirstLevelSubNodes
                If System.IO.Path.GetFileNameWithoutExtension(file.FullName) = Header.Header Then
                    keepfile = True
                End If
            Next
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
            For Each Header As FrameworkTreeNode In FirstLevelSubNodes
                If System.IO.Path.GetFileNameWithoutExtension(file.FullName) = Header.Header Then
                    keepfile = True
                End If
            Next
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
    Public Overrides Sub OnSaveEvent(sender As Object, e As System.Windows.RoutedEventArgs)

    End Sub

    Public Overrides Sub ReadFromXMl(path As String)

    End Sub

    Public Overrides Sub WriteToXML()

    End Sub
    Public Overrides Sub ReadFromXMLElement(xele As System.Xml.Linq.XElement)
        Dim d As New DamageCategoryTreeNode
        Dim o As New OccupancyTypeTreeNode
        Dim s As New StructureInventoryParentNode
        AddFrameworkTreeNode(d)
        AddFrameworkTreeNode(o)
        AddFrameworkTreeNode(s)
        d.AddFrameworkChildren(xele.Element(d.GetNodeName))
        o.AddFrameworkChildren(xele.Element(o.GetNodeName))
        s.AddFrameworkChildren(xele.Element(s.GetNodeName))
    End Sub
End Class
