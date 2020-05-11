Public Class ImpactAreaParentTreeNode
    Inherits FrameworkTreeNode
    Private _directorypath As String
    Private Const _FolderName As String = "ImpactAreas"
    Private Const _header As String = "Impact Areas"
    Sub New()
        MyBase.New(_header)
    End Sub
    Sub New(ByVal header As String)
        MyBase.New(header)
    End Sub
    Sub New(ByVal xele As XElement)
        MyBase.New(_header)
        'check the xelement to determine if there are any existing impact areas.
    End Sub
    Public Overrides ReadOnly Property GetSubDirectory As String
        Get
            Return _FolderName
        End Get
    End Property
    Public Overrides ReadOnly Property GetNodeName As String
        Get
            Return _FolderName
        End Get
    End Property
    Public Overrides Sub SetContextMenu()
        Dim c As New ContextMenu

        Dim AddImpactAreaData As New MenuItem()
        AddImpactAreaData.Header = "Import Impact Areas"
        AddHandler AddImpactAreaData.Click, AddressOf AddImpactArea
        c.Items.Add(AddImpactAreaData)

        Dim AddImpactAreaDataMain As New MenuItem()
        AddImpactAreaDataMain.Header = "_Import Impact Areas"
        AddHandler AddImpactAreaDataMain.Click, AddressOf AddImpactArea

        For Each mi As MenuItem In GetMainContextMenu.Items
            If mi.Header = "_Study" Then
                Dim tm As New MenuItem()
                tm.Header = "I_mpact Areas"
                tm.Items.Add(AddImpactAreaDataMain)
                mi.Items.Add(tm)
            End If
        Next

        MyBase.ContextMenu = c
    End Sub
    'Public Overrides Sub RemoveMainMenuContextOptions()
    '    For Each mi As MenuItem In GetMainContextMenu.Items
    '        If mi.Header = "_Study" Then
    '            For Each submi As MenuItem In mi.Items
    '                If submi.Header = "I_mpact Areas" Then RemoveHandler CType(submi.Items(0), MenuItem).Click, AddressOf AddImpactArea
    '                submi.Items.RemoveAt(0)
    '                mi.Items.Remove(submi)
    '            Next
    '        End If
    '    Next
    '    MyBase.RemoveMainMenuContextOptions()
    'End Sub
    Sub AddImpactArea(sender As Object, e As System.Windows.RoutedEventArgs)

        'Dim StreamAlignments As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(StreamAlignmentChildTreeNode))
        'Dim CrossSectionSets As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(CrossSectionChildTreeNode))
        Dim IAfiles As New List(Of String)
        For Each fn As OpenGLMapping.FeatureNode In GetMapTreeView.MapWindow.GISFeatures
            Select Case fn.Features.GetType
                Case GetType(OpenGLMapping.MapPolygons)
                    IAfiles.Add(fn.Features.Features.GetSource)
                Case Else
                    'not a polygon 
            End Select
        Next
        'Dim IAimporter As New ImpactAreaImporter(IAfiles, StreamAlignments, CrossSectionSets)
        Dim Iaimporter As New BasicImpactAreaImporter(IAfiles)
        Iaimporter.ShowDialog()
        Dim impactareaname As String = ""
        If Iaimporter.DialogResult = True Then
            'check for name conflicts.
            impactareaname = Iaimporter.TxtImpactAreaSetName.Text
            Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(ImpactAreaChildTreeNode))
            If nodes.Count > 0 Then
                'potential for name conflicts
                Dim nameconflict As Boolean = False
                Do
                    nameconflict = False
                    For j = 0 To nodes.Count - 1
                        If nodes(j).Header = impactareaname Then
                            nameconflict = True
                        End If
                    Next
                    If nameconflict Then
                        MsgBox("Name conflict, please rename")
                        Dim r As New Rename(impactareaname & "_1")
                        If r.ShowDialog() Then
                            'loop
                            impactareaname = r.TxtName.Text
                        Else
                            'user aborted
                            Exit Sub
                        End If
                    End If
                Loop Until nameconflict = False


            End If

            Dim i As New ImpactAreaChildTreeNode(Iaimporter.GetImpactAreaPath, impactareaname, Iaimporter.GetPaddedImpactAreaNames, Iaimporter.GetImpactAreaNames, Iaimporter.GetImpactAreaIndexes)
            AddFrameworkTreeNode(i)
            i.AddFrameworkChildren()
            'save the study.
            CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
        Else
            'its not true
        End If
    End Sub

    Public Overrides Sub OnSaveEvent(sender As Object, e As System.Windows.RoutedEventArgs)

    End Sub
    Public Overrides Sub ReadFromXMl(path As String)

    End Sub
    Public Overrides Sub WriteToXML()

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
                    Dim remove As Boolean = False
                    Dim loc As Int16 = 0
                    For j = 0 To GetMapTreeView.GetAllFeatureNodes.Count - 1
                        If (GetMapTreeView.GetAllFeatureNodes(j).Features.Features Is Nothing) Then Continue For

                        If GetMapTreeView.GetAllFeatureNodes(j).Features.Features.GetSource = filelist(i) Then
                            ''If GetMapTreeView.GetAllFeatureNodes(j).EditingMode Then
                            ''MsgBox("The Feature " & GetMapTreeView.GetAllFeatureNodes(j).FeatureNodeHeader & " is in edit mode would you like to abort?")
                            '' End If
                            loc = j
                            remove = True

                        End If
                    Next
                    If remove Then GetMapTreeView.GetAllFeatureNodes(loc).RemoveLayer(True)
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
    Public Overrides Sub ReadFromXMLElement(xele As System.Xml.Linq.XElement)
        For Each element As XElement In xele.Elements
            'check if the destination exists...

            'iap.SetAttributeValue("Path", ConvertToRelativePath(GetImpactAreaPath))
            If System.IO.File.Exists(ConvertFromRelativePath(element.Attribute("Path").Value.ToString)) Then
                Dim c As New ImpactAreaChildTreeNode(element)
                AddFrameworkTreeNode(c)
                c.AddFrameworkChildren(element)
            Else
                MsgBox(ConvertFromRelativePath(element.Attribute("Path").Value.ToString) & vbNewLine & "File Not found")
            End If

        Next
    End Sub

End Class
