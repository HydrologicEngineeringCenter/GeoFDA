Public Class OutputParentTreeNode
    Inherits FrameworkTreeNode
    Private Const _Subdirectory As String = "FDA Import File"
    Private Const _NodeName As String = "FDAImportFile"
    Sub New()
        MyBase.New(_Subdirectory)
    End Sub
    Public Overrides ReadOnly Property GetSubDirectory As String
        Get
            Return _Subdirectory
        End Get
    End Property
    Public Overrides ReadOnly Property GetNodeName As String
        Get
            Return _NodeName
        End Get
    End Property
    Public Overrides Sub SetContextMenu()
        Dim c As New ContextMenu

        Dim AddOutputData As New MenuItem()
        AddOutputData.Header = "Create New Econ Import file"
        AddHandler AddOutputData.Click, AddressOf AddOutput

        Dim AddOutputDatamain As New MenuItem()
        AddOutputDatamain.Header = "Create New Econ Import file"
        AddHandler AddOutputDatamain.Click, AddressOf AddOutput

        For Each mi As MenuItem In GetMainContextMenu.Items
            If mi.Header = "_Study" Then
                Dim tm As New MenuItem()
                tm.Header = "FDA Import File"
                tm.Items.Add(AddOutputDatamain)
                mi.Items.Add(tm)
            End If
        Next

        c.Items.Add(AddOutputData)
        MyBase.ContextMenu = c
    End Sub
    Private Sub AddOutput(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim list As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(HydraulicsChildTreenode))
        Dim gridnodes As New System.Collections.ObjectModel.ObservableCollection(Of HydraulicsChildTreenode)
        For i As Integer = 0 To list.Count - 1
            gridnodes.Add(list(i))
        Next
        If gridnodes.Count = 0 Then MsgBox("You have no hydraulic plans defined") : Exit Sub
        Dim ilist As New System.Collections.ObjectModel.ObservableCollection(Of StructureInventoryChildTreeNode)
        list = GetAllFrameworkTreenodesOfType(GetType(StructureInventoryChildTreeNode))
        For i As Integer = 0 To list.Count - 1
            ilist.Add(CType(list(i), StructureInventoryChildTreeNode))
        Next
        If ilist.Count = 0 Then MsgBox("You have no inventories defined") : Exit Sub
        Dim dlist As New System.Collections.ObjectModel.ObservableCollection(Of ImpactAreaChildTreeNode)
        list = GetAllFrameworkTreenodesOfType(GetType(ImpactAreaChildTreeNode))
        For i As Integer = 0 To list.Count - 1
            dlist.Add(CType(list(i), ImpactAreaChildTreeNode))
        Next
        If dlist.Count = 0 Then MsgBox("You have no Impact Areas defined") : Exit Sub
        Dim Econ As New EconFileSpec(gridnodes, ilist, dlist)
        If Econ.ShowDialog Then
            'true
            Dim o As New OutputChildTreeNode(Econ.TBPlanName.Text, Econ.TBPlanDesc.DescriptionWindow.Description, Econ.CmbStructureInventory.SelectedItem, CType(GetAllFrameworkTreenodesOfType(GetType(OccupancyTypeTreeNode))(0), OccupancyTypeTreeNode), Econ.CmbDamageReach.SelectedItem, Econ.GetSelectedPlans)
            AddFrameworkTreeNode(o)
            o.AddFrameworkChildren()
            CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
        Else
            'not true!
        End If
    End Sub
    Public Sub CleanDirectory(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim masterlist As New List(Of String)
        Dim firstlevelheaders As New List(Of String)
        For Each ftn As FrameworkTreeNode In FirstLevelSubNodes
            masterlist.AddRange(DirectCast(ftn, OutputChildTreeNode).CheckForUnecessaryFiles)
            firstlevelheaders.Add(ftn.Header)
        Next
        Dim dinfo As New System.IO.DirectoryInfo(GetCurrentDirectory)
        For Each Dir As System.IO.DirectoryInfo In dinfo.GetDirectories
            If firstlevelheaders.Contains(Dir.Name) Then
            Else
                masterlist.Add(Dir.FullName)
            End If
        Next
        If masterlist.Count > 0 Then
            Dim msg As String = ""
            For i = 0 To masterlist.Count - 1
                msg = msg & masterlist(i) & vbNewLine
            Next
            If MsgBox("Files that should be deleted were found:" & vbNewLine & vbNewLine & msg & vbNewLine & "Would you like to delete?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                For i = 0 To masterlist.Count - 1
                    If System.IO.File.Exists(masterlist(i)) Then
                        For j = 0 To GetMapTreeView.GetAllFeatureNodes.Count - 1
                            If GetMapTreeView.GetAllFeatureNodes(j).Features.Features.GetSource = masterlist(i) Then
                                ''If GetMapTreeView.GetAllFeatureNodes(j).EditingMode Then
                                ''MsgBox("The Feature " & GetMapTreeView.GetAllFeatureNodes(j).FeatureNodeHeader & " is in edit mode would you like to abort?")
                                '' End If
                                GetMapTreeView.GetAllFeatureNodes(j).RemoveLayer(True)
                            End If
                        Next
                        System.IO.File.Delete(masterlist(i))
                    Else
                        If System.IO.Directory.Exists(masterlist(i)) Then
                            System.IO.Directory.Delete(masterlist(i), True)
                        End If
                    End If

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
    Public Overrides Sub AddFrameworkChildren(ele As System.Xml.Linq.XElement)
        ReadfromXMLElement(ele)
        Dim masterlist As New List(Of String)
        Dim firstlevelheaders As New List(Of String)
        For Each ftn As FrameworkTreeNode In FirstLevelSubNodes
            masterlist.AddRange(DirectCast(ftn, OutputChildTreeNode).CheckForUnecessaryFiles)
            firstlevelheaders.Add(ftn.Header)
        Next
        Dim dinfo As New System.IO.DirectoryInfo(GetCurrentDirectory)
        For Each Dir As System.IO.DirectoryInfo In dinfo.GetDirectories
            If firstlevelheaders.Contains(Dir.Name) Then
            Else
                masterlist.Add(Dir.FullName)
            End If
        Next
        If masterlist.Count > 0 Then
            Dim clean As New MenuItem
            clean.Header = "Clean Directory"
            AddHandler clean.Click, AddressOf CleanDirectory
            MyBase.ContextMenu.Items.Add(clean)
            MyBase.Foreground = System.Windows.Media.Brushes.Red
            MyBase.ToolTip = "Unnecessary files exist in this directory."
        End If
    End Sub
    Public Overrides Sub OnSaveEvent(sender As Object, e As System.Windows.RoutedEventArgs)

    End Sub
    Public Overrides Sub ReadFromXMl(path As String)

    End Sub
    Public Overrides Sub ReadfromXMLElement(ele As XElement)
        'do some stuff
        For Each element As XElement In ele.Elements

            Dim o As New OutputChildTreeNode(element)
            AddFrameworkTreeNode(o)
            o.AddFrameworkChildren(element)
        Next
    End Sub
    'Public Overrides Function WriteToXMLElement() As System.Xml.Linq.XElement
    '    Dim ret As New XElement(GetNodeName)

    'End Function
    Public Overrides Sub WriteToXML()

    End Sub
End Class
