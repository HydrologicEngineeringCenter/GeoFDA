Public Class ImpactAreaChildTreeNode
    Inherits FrameworkTreeNode
    Private _description As List(Of String)
    Private Const _FolderName As String = "ImpactAreas"
    Private _AddedToMapWindow As Boolean = False
    Private _FeatureNode As OpenGLMapping.FeatureNode
    Private _errors As List(Of StructureErorr)
    Public Event RemoveMapFeatureNode(ByVal removefrommap As Boolean)
    Sub New(ByVal ImpactAreaFilePath As String, ByVal UserDefinedName As String, ByVal paddedimpactareanames As List(Of String), ByVal impactareanames As List(Of String), ByVal stations As List(Of Double))
        MyBase.New(UserDefinedName)
        'copy to study directory
        If System.IO.File.Exists(GetImpactAreaPath) Then
            'file conflict.
            If ImpactAreaFilePath = GetImpactAreaPath Then
                ' save as with the new name?
            Else
                ' delete all the old stuff
                If System.IO.File.Exists(GetImpactAreaPath) Then Kill(GetImpactAreaPath)
                If System.IO.File.Exists(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".shx")) Then Kill(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".shx"))
                If System.IO.File.Exists(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".dbf")) Then Kill(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".dbf"))
                If System.IO.File.Exists(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".prj")) Then Kill(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".prj"))
            End If
        End If
        System.IO.File.Copy(ImpactAreaFilePath, GetCurrentDirectory & "\" & Header & ".shp")
        System.IO.File.Copy(System.IO.Path.ChangeExtension(ImpactAreaFilePath, "shx"), GetCurrentDirectory & "\" & Header & ".shx")
        System.IO.File.Copy(System.IO.Path.ChangeExtension(ImpactAreaFilePath, ".prj"), GetCurrentDirectory & "\" & Header & ".prj")
        Dim dt As New System.Data.DataTable
        dt.Columns.Add(New System.Data.DataColumn("ImpactArea", GetType(String)))
        'dt.Columns.Add(New System.Data.DataColumn("Stream", GetType(String)))
        dt.Columns.Add(New System.Data.DataColumn("Bank", GetType(String)))
        'dt.Columns.Add(New System.Data.DataColumn("Upstream", GetType(Double)))
        'dt.Columns.Add(New System.Data.DataColumn("Downstream", GetType(Double)))
        dt.Columns.Add(New System.Data.DataColumn("Index", GetType(Double)))
        For i As Integer = 0 To impactareanames.Count - 1
            dt.Rows.Add({paddedimpactareanames(i), "Right", stations(i)})
        Next
        Dim dw As New DataBase_Reader.DBFReader(GetCurrentDirectory & "\" & Header & ".dbf", dt)
        dw.EditColumn("ImpactArea", impactareanames.ToArray)
        If dw.DataBaseOpen Then dw.Close()

        _description = New List(Of String)

    End Sub
    Sub New(ByVal Ele As XElement)
        MyBase.New(Ele.Attribute("Header").Value)

    End Sub
    Public Property Errors As List(Of StructureErorr)
        Get
            Return _errors
        End Get
        Set(value As List(Of StructureErorr))
            _errors = value
        End Set
    End Property

    Public Overrides ReadOnly Property GetSubDirectory As String
        Get
            Return _FolderName
        End Get
    End Property
    Public Overrides ReadOnly Property GetNodeName As String
        Get
            Return Me.GetType.Name
        End Get
    End Property
    Public ReadOnly Property GetImpactAreaPath As String
        Get
            Return GetCurrentDirectory & "\" & MyBase.Header & ".shp"
        End Get
    End Property
    Public Overrides Sub SetContextMenu()
        Dim c As New ContextMenu

        Dim AddToMap As New MenuItem()
        AddToMap.Header = "Add to Map Window"
        AddHandler AddToMap.Click, AddressOf AddToMapWindow
        c.Items.Add(AddToMap)

        Dim Edit As New MenuItem()
        Edit.Header = "Edit Impact Areas"
        AddHandler Edit.Click, AddressOf EditImpactAreas
        c.Items.Add(Edit)

        Dim Rename As New MenuItem
        Rename.Header = "Rename"
        AddHandler Rename.Click, AddressOf RenameImpactAreas
        c.Items.Add(Rename)

        Dim SaveAs As New MenuItem
        SaveAs.Header = "Save As"
        AddHandler SaveAs.Click, AddressOf SaveImpactAreasAs
        c.Items.Add(SaveAs)

        Dim Remove As New MenuItem
        Remove.Header = "Remove From Study"
        AddHandler Remove.Click, AddressOf RemoveFromStudy
        c.Items.Add(Remove)

        MyBase.ContextMenu = c
    End Sub
    Sub EditImpactAreas(sender As Object, e As System.Windows.RoutedEventArgs)
        If Not System.IO.File.Exists(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".dbf")) Then MsgBox("Could not find the impact area dbf") : ContainsErrors() : Exit Sub
        Dim dbfreader As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".dbf"))
        Dim ias As New List(Of FDA_Computation.ImpactArea)
        For i = 0 To dbfreader.NumberOfRows - 1
            If _description.Count = i Then _description.Add(" ")
            Dim ia As New FDA_Computation.ImpactArea(dbfreader.GetCell("ImpactArea", i), [Enum].Parse(GetType(FDA_Computation.BankEnum), dbfreader.GetCell("Bank", i)), dbfreader.GetCell("Index", i))
            ias.Add(ia)
        Next
        'dbfreader.Close()
        If ias.Count < 1 Then MsgBox("There are no impact areas in the impact area shapefile. Please create a new feature using the feature editor tools in the map window.") : Exit Sub
        Dim iaeditor As New ImpactAreaEditor(ias)
        iaeditor.Owner = GetMainWindow
        iaeditor.ShowDialog()
        If iaeditor.DialogResult = True Then
            Dim EditedInfo As System.Collections.ObjectModel.ObservableCollection(Of FDA_Computation.ImpactArea) = iaeditor.ImpactAreas
            'now write it to the existing dbf file
            Dim haschanges As Boolean = False
            For i = 0 To EditedInfo.Count - 1
                If Not dbfreader.GetCell(Array.IndexOf(dbfreader.ColumnNames, "ImpactArea"), i) = EditedInfo(i).Name Then dbfreader.EditCell(Array.IndexOf(dbfreader.ColumnNames, "ImpactArea"), i, EditedInfo(i).Name) : haschanges = True
                If Not dbfreader.GetCell(Array.IndexOf(dbfreader.ColumnNames, "Bank"), i) = EditedInfo(i).Bank.ToString Then dbfreader.EditCell(Array.IndexOf(dbfreader.ColumnNames, "Bank"), i, EditedInfo(i).Bank.ToString) : haschanges = True
                If Not dbfreader.GetCell(Array.IndexOf(dbfreader.ColumnNames, "Index"), i) = EditedInfo(i).IndexLocation Then dbfreader.EditCell(Array.IndexOf(dbfreader.ColumnNames, "Index"), i, EditedInfo(i).IndexLocation) : haschanges = True
            Next
            If dbfreader.DataBaseOpen Then dbfreader.Close()
            If haschanges Then
                Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
                For Each outnode As OutputChildTreeNode In nodes
                    If outnode.GetImpactAreas.Header = Header Then
                        outnode.AlertUserThatUpdatesAreNeeded("Impact area dbf data has changed.", False, False)
                    End If
                Next
            End If
        Else
            'it not true!
        End If
        ContainsErrors()
    End Sub
    Sub AddToMapWindow(sender As Object, e As System.Windows.RoutedEventArgs)
        If _AddedToMapWindow Then
            'its already added
        Else
            If IsNothing(_FeatureNode) Then
                _FeatureNode = New OpenGLMapping.FeatureNode(GetImpactAreaPath, GetMapTreeView.MapWindow)
                _FeatureNode.Features.OpenGLFeatureData.LineColorByAttribute(0) = New OpenTK.Graphics.Color4(CByte(255), CByte(0), CByte(0), CByte(255)) 'red
                _FeatureNode.Features.OpenGLFeatureData.FillColorByAttribute(0) = New OpenTK.Graphics.Color4(CByte(0), CByte(255), CByte(0), CByte(200)) 'green a bit transparent
                _FeatureNode.Features.OpenGLFeatureData.LineWidthByAttribute(0) = 1
            End If

            AddHandler _FeatureNode.RemoveFromMapWindow, AddressOf FeatureRemoved
            AddHandler RemoveMapFeatureNode, AddressOf _FeatureNode.RemoveLayer
            AddHandler _FeatureNode.AttributesOpening, AddressOf CancelAttributeOpen
            AddHandler _FeatureNode.AttributeEditsSaved, AddressOf DBFEditsSaved
            AddHandler _FeatureNode.FeaturesChanged, AddressOf OnFeaturesChanged
            AddHandler _FeatureNode.PreviewStartFeatureEdit, AddressOf CancelFeatureEdit
            'add a handler to the remove event to handle the case that the map layer was removed by the user from the maps tab.
            GetMapTreeView.AddGISData(_FeatureNode, 0)

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
    Private Sub SaveImpactAreasAs(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim wasaddedtomapwindow As Boolean = False
        'rename the files
        Dim rename As New Rename(Header)
        rename.Title = "Save As"
        rename.Owner = GetMainWindow
        If rename.ShowDialog() Then
            'check for name conflicts.
            Dim ianodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(ImpactAreaChildTreeNode))
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
            Dim parentfile As ImpactAreaParentTreeNode = CType(Parent, ImpactAreaParentTreeNode)
            Dim dbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".dbf"))
            Dim names As New List(Of String)
            Dim paddednames As New List(Of String)
            Dim stations As New List(Of Double)
            For i = 0 To dbf.NumberOfRows - 1
                names.Add(dbf.GetCell("ImpactArea", i))
                paddednames.Add(names(i).PadRight(32))
                stations.Add(dbf.GetCell("Index", i))
            Next
            If dbf.DataBaseOpen Then dbf.Close()
            Dim childfile As New ImpactAreaChildTreeNode(GetImpactAreaPath, rename.NewName, paddednames, names, stations)
            parentfile.AddFrameworkTreeNode(childfile)
            childfile.AddFrameworkChildren()
            CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode).WriteToXML()
        Else
            'user closed.
        End If
    End Sub
    Private Sub ReNameImpactAreas(sender As Object, e As System.Windows.RoutedEventArgs)

        'check if any outputs exist that use these impact areas
        Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
        Dim msg As New System.Text.StringBuilder
        If nodes.Count = 0 Then
        Else
            Dim onodes As New List(Of OutputChildTreeNode)
            For i = 0 To nodes.Count - 1
                onodes.Add(CType(nodes(i), OutputChildTreeNode))
                If onodes.Last.GetImpactAreas.Header = MyBase.Header Then
                    msg.AppendLine("Impact areas are used by " & onodes.Last.Header)
                End If
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
                Dim ianodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(ImpactAreaChildTreeNode))
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
                System.IO.File.Copy(GetImpactAreaPath, newname)
                System.IO.File.Copy(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".shx"), System.IO.Path.ChangeExtension(newname, ".shx"))
                System.IO.File.Copy(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".dbf"), System.IO.Path.ChangeExtension(newname, ".dbf"))
                If System.IO.File.Exists(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".prj")) Then System.IO.File.Copy(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".prj"), System.IO.Path.ChangeExtension(newname, ".prj"))

                Kill(GetImpactAreaPath)
                Kill(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".shx"))
                Kill(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".dbf"))
                If System.IO.File.Exists(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".prj")) Then Kill(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".prj"))
                Dim stn As StudyTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode)

                MyBase.Header = rename.NewName
                'Dim sn As ImpactAreaParentTreeNode = CType(Parent, ImpactAreaParentTreeNode)
                'sn.FirstLevelSubNodes.Remove(Me)
                'sn.Items.Remove(Me)
                If wasaddedtomapwindow Then
                    AddToMapWindow(Nothing, Nothing)
                End If
                stn.WriteToXML()
            End If

        End If

    End Sub
    Private Sub RemoveFromStudy(sender As Object, e As System.Windows.RoutedEventArgs)

        'check if any outputs exist that use this inventory.
        Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
        Dim msg As New System.Text.StringBuilder
        If nodes.Count = 0 Then
        Else
            For i = 0 To nodes.Count - 1
                If DirectCast(nodes(i), OutputChildTreeNode).GetImpactAreas.Header = Header Then
                    msg.AppendLine("Impact areas are used by " & DirectCast(nodes(i), OutputChildTreeNode).Header)
                End If
            Next
        End If
        Dim dontkill As Boolean = False
        If msg.ToString = "" Then
            'if the impact is not used by an output, then delete
            DeleteFromStudy()
        Else
            If MsgBox(msg.ToString & vbNewLine & "Would you still like to delete?", MsgBoxStyle.OkCancel, "Warning") = MsgBoxResult.Cancel Then
            Else
                DeleteFromStudy()
            End If
        End If
    End Sub
    Public Sub DeleteFromStudy()
        Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
        If nodes.Count = 0 Then
        Else
            For i = 0 To nodes.Count - 1
                If DirectCast(nodes(i), OutputChildTreeNode).GetImpactAreas.Header = Header Then
                    DirectCast(nodes(i), OutputChildTreeNode).DeleteFromStudy()
                End If
            Next
        End If
        If _AddedToMapWindow Then
            'remove it from the map window first.
            RemoveFromMapWindow(Nothing, Nothing)
        End If
        Kill(GetImpactAreaPath)
        Kill(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".shx"))
        Kill(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".dbf"))
        If System.IO.File.Exists(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".prj")) Then Kill(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".prj"))
        Dim stn As StudyTreeNode = CType(GetAllFrameworkTreenodesOfType(GetType(StudyTreeNode))(0), StudyTreeNode)
        Dim sn As ImpactAreaParentTreeNode = CType(Parent, ImpactAreaParentTreeNode)
        sn.FirstLevelSubNodes.Remove(Me)
        sn.Items.Remove(Me)
        stn.RemoveFrameworkTreeNode(Me)
        stn.WriteToXML()
    End Sub
    Private Sub FeatureRemoved(fn As OpenGLMapping.FeatureNode)
        'RemoveHandler fn.RemoveFromMapWindow, AddressOf FeatureRemoved
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
    Public Function ContainsErrors() As Boolean
        'check for name matches
        Dim errs As New List(Of StructureErorr)
        If System.IO.File.Exists(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".dbf")) Then
            Dim dbf As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(GetImpactAreaPath, ".dbf"))

            If Not dbf.ColumnNames.Contains("ImpactArea") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "ImpactArea")) <> GetType(String) Then errs.Add(New StructureErorr("The column ImpactArea does not exist, or it is not a String column", 0))
            'If Not dbf.ColumnNames.Contains("Stream") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "Stream")) <> GetType(String) Then errs.Add(New StructureErorr("The column Stream does not exist, or it is not a String column", 0))
            If Not dbf.ColumnNames.Contains("Bank") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "Bank")) <> GetType(String) Then errs.Add(New StructureErorr("The column Bank does not exist, or it is not a String column", 0))
            'If Not dbf.ColumnNames.Contains("Upstream") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "Upstream")) <> GetType(Double) Then errs.Add(New StructureErorr("The column Upstream does not exist, or it is not a Double column", 0))
            'If Not dbf.ColumnNames.Contains("Downstream") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "Downstream")) <> GetType(Double) Then errs.Add(New StructureErorr("The column Downstream does not exist, or it is not a Double column", 0))
            If Not dbf.ColumnNames.Contains("Index") OrElse dbf.ColumnTypes(Array.IndexOf(dbf.ColumnNames, "Index")) <> GetType(Double) Then errs.Add(New StructureErorr("The column Index does not exist, or it is not a Double column", 0))

            Dim names(dbf.NumberOfRows - 1) As String
            Dim banks(dbf.NumberOfRows - 1) As String
            Dim stations(dbf.NumberOfRows - 1) As Double
            Dim tmpcolumn As Object() = dbf.GetColumn("ImpactArea")
            For i = 0 To tmpcolumn.Count - 1
                names(i) = tmpcolumn(i).ToString
            Next
            tmpcolumn = dbf.GetColumn("Bank")
            For i = 0 To tmpcolumn.Count - 1
                banks(i) = tmpcolumn(i).ToString
            Next
            tmpcolumn = dbf.GetColumn("Index")
            For i = 0 To tmpcolumn.Count - 1
                stations(i) = CDbl(tmpcolumn(i))
            Next
            If dbf.DataBaseOpen Then dbf.Close()
            Dim tmpnames As New List(Of String)
            For i = 0 To names.Count - 1
                'check for null names
                If names(i) = "" Then errs.Add(New StructureErorr("The Impact Area at index " & i.ToString & " does not have a name defined", i))
                If tmpnames.Contains(names(i)) Then 'check for duplicates
                    errs.Add(New StructureErorr("The Impact Area at index " & i.ToString & " is named " & names(i) & " is not uniquely named", i))
                Else
                    If names(i).Length > 32 Then errs.Add(New StructureErorr("The Impact Area at index " & i.ToString & " is named " & names(i) & " which is more than 32 characters", i))
                    tmpnames.Add(names(i))
                End If
            Next
            For i = 0 To banks.Count - 1
                'check for null banks
                If banks(i) = "" Then errs.Add(New StructureErorr("The Impact Area at index " & i.ToString & " does not have a bank defined", i))
                Select Case banks(i)
                    Case FDA_Computation.BankEnum.Both.ToString
                    Case FDA_Computation.BankEnum.Left.ToString
                    Case FDA_Computation.BankEnum.Right.ToString
                    Case Else
                        errs.Add(New StructureErorr("The Impact Area at index " & i.ToString & " does not have a proper bank definition", i))
                End Select
            Next
            ''check the stations are not less than or equal to zero, and not an integer.
            Dim tmpstations As New List(Of Double)
            For i = 0 To stations.Count - 1
                'check for null names
                If stations(i) <= 0 Then errs.Add(New StructureErorr("The Impact Area at index " & i.ToString & " does not have an index greater than zero", i))
                If stations(i) = CInt(stations(i)) Then errs.Add(New StructureErorr("The Impact Area at index " & i.ToString & " has a station that is an integer, this is not allowed.", i))
                If tmpstations.Contains(stations(i)) Then 'check for duplicates
                    errs.Add(New StructureErorr("The Impact Area at index " & i.ToString & " has the same station as another impact area.", i))
                Else

                    tmpstations.Add(stations(i))
                End If
            Next
        Else
            errs.Add(New StructureErorr("Could not find the dbf file " & System.IO.Path.ChangeExtension(GetImpactAreaPath, ".dbf"), 0))
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
        Dim errorreport As New ImpactAreaErrorReport(Me)
        errorreport.Owner = GetMainWindow
        errorreport.Show()
    End Sub
    Private Sub DBFEditsSaved()
        'MsgBox("Saved")
        '' check if any output child nodes exist with this imact area set.
        Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))

        If ContainsErrors() Then
            If nodes.Count > 0 Then
                For i = 0 To nodes.Count - 1
                    If CType(nodes(i), OutputChildTreeNode).GetImpactAreas.Header = MyBase.Header Then
                        CType(nodes(i), OutputChildTreeNode).AlertUserThatUpdatesAreNeeded("Impact area dbf data has changed, and contains errors.", False, True)
                    End If
                Next
            End If
        Else
            If nodes.Count > 0 Then
                For i = 0 To nodes.Count - 1
                    If CType(nodes(i), OutputChildTreeNode).GetImpactAreas.Header = MyBase.Header Then
                        CType(nodes(i), OutputChildTreeNode).AlertUserThatUpdatesAreNeeded("Impact area dbf data has changed.", True, False)
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
                    If CType(nodes(i), OutputChildTreeNode).GetImpactAreas.Header = MyBase.Header Then
                        CType(nodes(i), OutputChildTreeNode).AlertUserThatUpdatesAreNeeded("Impact area feature data has changed, and contains errors.", False, True)
                    End If
                Next
            End If
        Else
            ''determine if additional impact areas have been created?
            If nodes.Count > 0 Then
                For i = 0 To nodes.Count - 1
                    If CType(nodes(i), OutputChildTreeNode).GetImpactAreas.Header = MyBase.Header Then
                        CType(nodes(i), OutputChildTreeNode).AlertUserThatUpdatesAreNeeded("Impact area feature data has changed.", True, False)
                    End If
                Next
            End If
        End If
    End Sub
    Private Sub CancelFeatureEdit(ByRef cancel As Boolean)
        If MsgBox("You are opening an edit session on the impact area shapefile, it is possible that the edits you do can significantly impact your data, would you like to continue?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
        Else
            cancel = True
        End If
    End Sub
    Private Sub CancelAttributeOpen(ByRef cancel As Boolean)
        If MsgBox("You are opening an impact area DBF attribute table, it is possible that the edits you do can significantly impact your data, would you like to continue?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
        Else
            cancel = True
        End If
    End Sub
    Public Overrides Sub AddFrameworkChildren(ele As System.Xml.Linq.XElement)
        ReadFromXMLElement(ele)
    End Sub
    Public Overrides Sub OnSaveEvent(sender As Object, e As System.Windows.RoutedEventArgs)

    End Sub
    Public Overrides Sub ReadFromXMl(path As String)

    End Sub

    Public Overrides Sub WriteToXML()

    End Sub
    Public Overrides Function WriteToXMLElement() As System.Xml.Linq.XElement
        Dim iap As New XElement(GetNodeName)
        iap.SetAttributeValue("Header", Header)
        iap.SetAttributeValue("Path", ConvertToRelativePath(GetImpactAreaPath))
        Return iap
    End Function

    Public Overrides Sub ReadFromXMLElement(xele As System.Xml.Linq.XElement)
        For Each fn As OpenGLMapping.FeatureNode In GetMapTreeView.MapWindow.GISFeatures
            If Not IsNothing(fn.Features.Features) AndAlso fn.Features.Features.GetSource = GetImpactAreaPath Then
                _AddedToMapWindow = True
                _FeatureNode = fn
                AddHandler _FeatureNode.RemoveFromMapWindow, AddressOf FeatureRemoved
                AddHandler RemoveMapFeatureNode, AddressOf _FeatureNode.RemoveLayer
                AddHandler _FeatureNode.AttributesOpening, AddressOf CancelAttributeOpen
                AddHandler _FeatureNode.AttributeEditsSaved, AddressOf DBFEditsSaved
                AddHandler _FeatureNode.FeaturesChanged, AddressOf OnFeaturesChanged
                AddHandler _FeatureNode.PreviewStartFeatureEdit, AddressOf CancelFeatureEdit
            End If
        Next
        If _AddedToMapWindow Then AddToMapWindow(Me, Nothing)
        _description = New List(Of String)
    End Sub
End Class
