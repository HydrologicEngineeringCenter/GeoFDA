Public Class HydraulicsParentTreeNode
    Inherits FrameworkTreeNode
    Private Const _Subdirectory As String = "Hydraulics"
    Private Const _Header As String = "Hydraulics"
    Sub New()
        MyBase.New(_Header)
    End Sub
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
        Dim c As New ContextMenu

        Dim AddHydraulicsData As New MenuItem()
        AddHydraulicsData.Header = "Import Hydraulic Data"
        AddHandler AddHydraulicsData.Click, AddressOf ImportFromGrids

        'Dim ImportGriddedData As New MenuItem
        'ImportGriddedData.Header = "From Gridded Data"
        'AddHandler ImportGriddedData.Click, AddressOf ImportFromGrids
        'AddHydraulicsData.Items.Add(ImportGriddedData)

        'Dim SteadyState As New MenuItem
        'SteadyState.Header = "From Steady State Data"
        'AddHandler SteadyState.Click, AddressOf ImportFromSteadyState
        'SteadyState.IsEnabled = False
        'AddHydraulicsData.Items.Add(SteadyState)

        'Dim UnsteadyState As New MenuItem
        'UnsteadyState.Header = "From Unsteady State Data"
        'AddHandler UnsteadyState.Click, AddressOf ImportFromUnsteadyState
        ''UnsteadyState.IsEnabled = False
        'AddHydraulicsData.Items.Add(UnsteadyState)
        Dim AddHydraulicsDataMain As New MenuItem()
        AddHydraulicsDataMain.Header = "_Import Hydraulic Data"
        AddHandler AddHydraulicsDataMain.Click, AddressOf ImportFromGrids

        For Each mi As MenuItem In GetMainContextMenu.Items
            If mi.Header = "_Study" Then
                Dim tm As New MenuItem()
                tm.Header = "_Hydraulics"
                tm.Items.Add(AddHydraulicsDataMain)
                mi.Items.Add(tm)
            End If
        Next

        c.Items.Add(AddHydraulicsData)
        MyBase.ContextMenu = c
    End Sub
    'Public Overrides Sub RemoveMainMenuContextOptions()
    '    For Each mi As MenuItem In GetMainContextMenu.Items
    '        If mi.Header = "_Study" Then
    '            For Each submi As MenuItem In mi.Items
    '                If submi.Header = "_Hydraulics" Then RemoveHandler CType(submi.Items(0), MenuItem).Click, AddressOf ImportFromGrids
    '                submi.Items.RemoveAt(0)
    '                mi.Items.Remove(submi)
    '                Exit For
    '            Next
    '        End If
    '    Next
    '    MyBase.RemoveMainMenuContextOptions()
    'End Sub
    Private Sub ImportFromGrids(sender As Object, e As System.Windows.RoutedEventArgs)
        'Dim g As New GriddedDataForm
        'Dim tmplist As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(WatershedChildTreeNode))
        'Dim wlist As New System.Collections.ObjectModel.ObservableCollection(Of WatershedChildTreeNode)
        'For i = 0 To tmplist.Count - 1
        '    wlist.Add(CType(tmplist(i), WatershedChildTreeNode))
        'Next
        'If wlist.Count = 0 Then ReportMessage("There are no watersheds in this study") : Exit Sub



        Dim tmplist As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(TerrainChildTreeNode))
        Dim tlist As New System.Collections.ObjectModel.ObservableCollection(Of TerrainChildTreeNode)
        If tmplist.Count = 0 Then ReportMessage("No Terrain files have been imported") : MsgBox("No terrain files have been imported") : Exit Sub
        For i = 0 To tmplist.Count - 1
            tlist.Add(CType(tmplist(i), TerrainChildTreeNode))
        Next
        tmplist = GetAllFrameworkTreenodesOfType(GetType(HydraulicsChildTreenode))
        Dim startupName As String = "Without"
        For i = 0 To tmplist.Count - 1
            If tmplist(i).Header = "Without" Then
                startupName = ""
            End If
        Next
        Dim fnlist As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))
        Dim s As StudyTreeNode = CType(fnlist(0), StudyTreeNode)
        If s.GetBaseYear = 0 Then ReportMessage("A Base Year is required") : MsgBox("A Base Year is required") : Exit Sub
        Try
            Dim g As New GridImporter(startupName, tlist, s.GetBaseYear, s.GetFutureYear)
            g.Owner = GetMainWindow
            g.ShowDialog()
            If g.DialogResult Then 'user clicked ok

                Dim nameAndYearConflict As Boolean = False
                Do
                    For Each tmpItem As FrameworkTreeNode In tmplist
                        If tmpItem.Header = g.GetUniqueName AndAlso DirectCast(tmpItem, HydraulicsChildTreenode).GetYear = g.CmbYear.SelectedItem Then
                            nameAndYearConflict = True
                            MsgBox("There is already a hydraulics node with this name and year")
                            g = New GridImporter(g.GetUniqueName & "_1", tlist, s.GetBaseYear, s.GetFutureYear)
                            g.Owner = GetMainWindow
                            If g.ShowDialog() Then
                            Else
                                'user aborted
                                Exit Sub
                            End If
                        Else
                            nameAndYearConflict = False
                        End If
                    Next
                Loop Until nameAndYearConflict = False

                If startupName = "Without" And startupName <> g.TxtName.Text Then
                    MsgBox("You did not define your plan to be " & startupName & " this name is required to create Net Benefit Calculations in FDA.")
                End If
                Dim c As New HydraulicsChildTreenode(g.GetUniqueName, g.TxtDesc.Text, g.GetGrids, g.GetProbabilities, g.CmbTerrain.SelectedItem, g.RDBDepth.IsChecked, g.CmbYear.SelectedItem)
                AddFrameworkTreeNode(c)
                c.AddFrameworkChildren()
                CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
            Else
                'not ok
            End If
        Catch ex As Exception
            ReportMessage(ex.Message)
        End Try

    End Sub
    'Private Sub ImportFromSteadyState(sender As Object, e As System.Windows.RoutedEventArgs)

    'End Sub
    'Private Sub ImportFromUnsteadyState(sender As Object, e As System.Windows.RoutedEventArgs)
    '    Dim tmplist As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(WatershedChildTreeNode))
    '    Dim wlist As New System.Collections.ObjectModel.ObservableCollection(Of WatershedChildTreeNode)

    '    Dim fnlist As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))
    '    Dim s As StudyTreeNode = CType(fnlist(0), StudyTreeNode)
    '    If s.GetBaseYear = 0 OrElse s.GetFutureYear = 0 Then ReportMessage("Base Year and Future Year Have Not been Properly Set") : Exit Sub
    '    For i = 0 To tmplist.Count - 1
    '        wlist.Add(CType(tmplist(i), WatershedChildTreeNode))
    '    Next
    '    If wlist.Count = 0 Then ReportMessage("There are no watersheds in this study") : Exit Sub
    '    Try
    '        Dim importfromdss As New UnsteadyImporter(wlist, s.GetBaseYear, s.GetFutureYear)
    '        importfromdss.Owner = GetMainWindow
    '        importfromdss.ShowDialog()
    '        If importfromdss.DialogResult Then
    '            'ok
    '        Else
    '            'not ok
    '        End If
    '    Catch ex As Exception
    '        ReportMessage(ex.Message)
    '    End Try

    'End Sub
    Public Overrides Sub AddFrameworkChildren()

    End Sub
    Public Overrides Sub AddFrameworkChildren(ele As System.Xml.Linq.XElement)
        ReadfromXMLElement(ele)
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
    Public Overrides Sub OnSaveEvent(sender As Object, e As System.Windows.RoutedEventArgs)

    End Sub
    Public Overrides Sub ReadFromXMl(path As String)

    End Sub
    Public Overrides Sub ReadfromXMLElement(ele As XElement)
        'do some stuff
        For Each element As XElement In ele.Elements
            'check all paths.
            'Dim grids As New XElement("Grids")
            'For i = 0 To _GridNames.Count - 1
            '    Dim x As New XElement("GridPath")
            '    x.SetAttributeValue("Path", ConvertToRelativePath(GetGridPaths(i)))
            '    x.SetAttributeValue("Prob", _Probs(i))
            '    grids.Add(x)
            'Next
            If IsNothing(element.Element("Grids")) Then
            Else
                Dim msg As String = ""
                For Each path As XElement In element.Element("Grids").Elements
                    If System.IO.File.Exists(ConvertFromRelativePath(path.Attribute("Path"))) Then
                    Else
                        msg = msg & ConvertFromRelativePath(path.Attribute("Path")) & vbNewLine
                    End If
                Next
                If msg = "" Then

                    Dim c As New HydraulicsChildTreenode(element)
                    AddFrameworkTreeNode(c)
                    c.AddFrameworkChildren(element)
                Else
                    MsgBox(msg & "Files Not Found")
                End If
            End If
        Next
    End Sub
    Public Overrides Sub WriteToXML()

    End Sub
End Class
