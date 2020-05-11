Public Class PlanChildTreeNode
    Inherits FrameworkTreeNode
    Private Const _ParentFolder As String = "FDA Import File"
    Private _damagereaches As ImpactAreaChildTreeNode
    Private _plan As HydraulicsChildTreenode
    Private _IndexlocationsAreReady As Boolean = False
    Public Event IndexLocationsAreReadyEvent(sender As Object, e As RoutedEventArgs)
    Sub New(ByVal DamageReaches As ImpactAreaChildTreeNode, ByVal selectedplan As HydraulicsChildTreenode)
        MyBase.New(selectedplan.Header)
        _damagereaches = DamageReaches
        _plan = selectedplan
    End Sub
    Sub New(ByVal xele As XElement)
        MyBase.New(xele.Attribute("Header").Value)
    End Sub
    Public ReadOnly Property HydraulicChildTreeNode As HydraulicsChildTreenode
        Get
            Return _plan
        End Get
    End Property
    Public Overrides ReadOnly Property GetNodeName As String
        Get
            Return Me.GetType.Name
        End Get
    End Property
    Public ReadOnly Property IndexLocationsAreReady As Boolean
        Get
            Return _IndexlocationsAreReady
        End Get
    End Property
    Public Overrides ReadOnly Property GetSubDirectory As String
        Get
            Return _ParentFolder '& "\" & CType(Me.Parent, OutputChildTreeNode).Header & "\" & Me.Header
        End Get
    End Property

    Public Overrides Sub OnSaveEvent(sender As Object, e As System.Windows.RoutedEventArgs)

    End Sub

    Public Overrides Sub ReadFromXMl(path As String)

    End Sub

    Public Overrides Sub SetContextMenu()
        Dim c As New ContextMenu

        Dim EditDamageReaches As New MenuItem()
        EditDamageReaches.Header = "Edit Damage Reach Index Points"
        AddHandler EditDamageReaches.Click, AddressOf EditDamageReachIndexes
        c.Items.Add(EditDamageReaches)

        Dim CopyDamageReaches As New MenuItem()
        CopyDamageReaches.Header = "Copy from Existing"
        AddHandler CopyDamageReaches.Click, AddressOf CopyDamageReachIndexes
        c.Items.Add(CopyDamageReaches)

        MyBase.ContextMenu = c
    End Sub
    Sub EditExisting(sender As Object, e As System.Windows.RoutedEventArgs)

    End Sub
    Sub EditDamageReachIndexes(sender As Object, e As System.Windows.RoutedEventArgs)
        'if all of the damage reach indexes are fully qualified enale compute econ import file.
        Dim dbr As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(_damagereaches.GetImpactAreaPath, ".dbf"))
        Dim impactareas As String() = Array.ConvertAll(dbr.GetColumn("ImpactArea"), New Converter(Of Object, String)(Function(x) Convert.ToString(x)))
        Dim stations As Double() = Array.ConvertAll(dbr.GetColumn("Index"), New Converter(Of Object, Double)(Function(x) Double.Parse(x.ToString)))
        dbr.Close()
        Dim i As New IndexLocations(impactareas, stations, GetCurrentDirectory & "\" & CType(Me.Parent, OutputChildTreeNode).Header & "\" & Me.Header)
        i.Owner = GetMainWindow
        AddHandler i.ChangesHaveOccured, AddressOf UpdateParent
        i.ShowDialog()
        If i.DialogResult = True Then
            'true

        Else
            'not true
        End If
        CheckIfIndexLocationsAreReady()
    End Sub
    Private Sub UpdateParent()
        DirectCast(MyBase.Parent, OutputChildTreeNode).AlertUserThatUpdatesAreNeeded("Index locations have changed.", False, False)
    End Sub
    Private Function CheckIfIndexLocationsAreReady() As Boolean
        Dim dbr As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(_damagereaches.GetImpactAreaPath, ".dbf"))
        Dim impactareas As String() = Array.ConvertAll(dbr.GetColumn("ImpactArea"), New Converter(Of Object, String)(Function(x) Convert.ToString(x)))
        dbr.Close()
        Dim count As Integer = 0
        For j = 0 To impactareas.Count - 1
            If System.IO.File.Exists(GetCurrentDirectory & "\" & CType(Me.Parent, OutputChildTreeNode).Header & "\" & Me.Header & "\" & impactareas(j) & ".xml") Then count += 1
        Next
        If count = impactareas.Count Then
            _IndexlocationsAreReady = True
            RaiseEvent IndexLocationsAreReadyEvent(Me, Nothing)
            Return IndexLocationsAreReady
        Else
            _IndexlocationsAreReady = False
            Return False
        End If
    End Function
    Public Overrides Sub AddFrameworkChildren(ele As System.Xml.Linq.XElement)
        ReadFromXMLElement(ele)
    End Sub
    Public Function CheckForUnecessaryFiles() As List(Of String)
        Dim filestodelete As New List(Of String)
        If System.IO.Directory.Exists(GetCurrentDirectory & "\" & CType(Me.Parent, OutputChildTreeNode).Header & "\" & Me.Header & "\") Then
            Dim dinfo As New System.IO.DirectoryInfo(GetCurrentDirectory & "\" & CType(Me.Parent, OutputChildTreeNode).Header & "\" & Me.Header & "\")
            Dim finfo As System.IO.FileInfo() = dinfo.GetFiles
            Dim dbr As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(_damagereaches.GetImpactAreaPath, ".dbf"))
            Dim impactareas As String() = Array.ConvertAll(dbr.GetColumn("ImpactArea"), New Converter(Of Object, String)(Function(x) Convert.ToString(x)))
            For Each file As System.IO.FileInfo In finfo
                If impactareas.Contains(System.IO.Path.GetFileNameWithoutExtension(file.FullName)) Then
                Else
                    If impactareas.Contains(System.IO.Path.GetFileNameWithoutExtension(file.FullName).Split("_")(0)) Then
                    Else
                        filestodelete.Add(file.FullName)
                    End If
                End If
            Next
            Return filestodelete
        Else
            Return filestodelete
        End If
    End Function
    Public Overrides Sub WriteToXML()

    End Sub
    Public Overrides Sub ReadFromXMLElement(xele As System.Xml.Linq.XElement)
        '_description = xele.Attribute("Description").Value
        'refer to the watershed readfromxmelement
        _damagereaches = CType(GetFrameworkTreenodeByNameAndType(xele.Element("DamageReach").Value, GetType(ImpactAreaChildTreeNode)), ImpactAreaChildTreeNode)
        Dim x As XElement = xele.Element("Plan")
        Dim ats As IEnumerable(Of XAttribute) = x.Attributes
        Dim yearvalue As Integer = Nothing
        For i = 0 To ats.Count - 1
            If ats(i).Name = "PlanYear" Then
                yearvalue = CInt(x.Attribute("PlanYear"))
            End If
        Next
        Select Case x.Attribute("Type").Value
            'Case "CrossSectionHydraulicsChildTreenode"
            '    _plan = (CType(GetFrameworkTreenodeByNameAndType(x.Value, GetType(CrossSectionChildTreeNode)), HydraulicsChildTreenode))
            Case "HydraulicsChildTreenode"
                Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(HydraulicsChildTreenode))
                For Each h As HydraulicsChildTreenode In nodes
                    If IsNothing(yearvalue) OrElse yearvalue = 0 Then
                        If h.Header = x.Value Then
                            _plan = h
                            Exit For
                        End If
                    Else
                        If h.Header = x.Value AndAlso h.GetYear = yearvalue Then
                            _plan = h
                        End If
                    End If
                Next
        End Select
        _IndexlocationsAreReady = CBool(x.Attribute("IndexLocationsAreDefined").Value)
        CheckIfIndexLocationsAreReady()
        If _IndexlocationsAreReady Then
            RaiseEvent IndexLocationsAreReadyEvent(Me, Nothing)
        End If

    End Sub
    Public Overrides Function WriteToXMLElement() As System.Xml.Linq.XElement
        CheckIfIndexLocationsAreReady()
        Dim ret As New XElement(GetNodeName)
        ret.SetAttributeValue("Header", _plan.Header)
        'ret.SetAttributeValue("Description", _description)
        Dim reach As New XElement("DamageReach", _damagereaches.Header)
        ret.Add(reach)
        Dim plans As New XElement("Plan", _plan.Header)
        plans.SetAttributeValue("Type", _plan.GetType.Name)
        plans.SetAttributeValue("IndexLocationsAreDefined", _IndexlocationsAreReady.ToString)
        plans.SetAttributeValue("PlanYear", _plan.GetYear)
        ret.Add(plans)
        'add all the selected elements
        Return ret
    End Function

    Private Sub CopyDamageReachIndexes(sender As Object, e As RoutedEventArgs)
        ''check for other plans in the same output child tree node.
        Dim dbr As New DataBase_Reader.DBFReader(System.IO.Path.ChangeExtension(_damagereaches.GetImpactAreaPath, ".dbf"))
        Dim impactareas As String() = Array.ConvertAll(dbr.GetColumn("ImpactArea"), New Converter(Of Object, String)(Function(x) Convert.ToString(x)))
        Dim possiblematches As New List(Of String)
        Dim parent As OutputChildTreeNode = DirectCast(Me.Parent, OutputChildTreeNode)
        If parent.GetPlans.Count = 1 Then
            '' no other possible matches.
        Else
            '' possible matches
            For Each otherplan As HydraulicsChildTreenode In parent.GetPlans
                If otherplan.Header = _plan.Header Then
                    'its me.
                Else
                    'check if files exist in the folder.
                    Dim theyallexist As Boolean = True
                    For Each Str As String In impactareas
                        If System.IO.File.Exists(GetCurrentDirectory & "\" & CType(Me.Parent, OutputChildTreeNode).Header & "\" & otherplan.Header & "\" & Str & ".xml") Then
                        Else
                            theyallexist = False
                        End If
                    Next
                    If theyallexist Then possiblematches.Add(parent.Header & "-" & otherplan.Header & "-" & otherplan.GetYear)
                End If
            Next
        End If
        ''check for other output child tree nodes with the same impact area (that arent me)
        Dim nodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
        For Each outputnode As OutputChildTreeNode In nodes
            If outputnode.Header = parent.Header Then
                'its me. dont do that again
            Else
                If outputnode.GetImpactAreas.GetImpactAreaPath = _damagereaches.GetImpactAreaPath Then
                    'possible match
                    For Each otherplan As HydraulicsChildTreenode In outputnode.GetPlans
                        'check if files exist in the folder.
                        Dim theyallexist As Boolean = True
                        For Each Str As String In impactareas
                            If System.IO.File.Exists(GetCurrentDirectory & "\" & outputnode.Header & "\" & otherplan.Header & "\" & Str & ".xml") Then
                            Else
                                theyallexist = False
                            End If
                        Next
                        If theyallexist Then possiblematches.Add(outputnode.Header & "-" & otherplan.Header & "-" & otherplan.GetYear)
                    Next
                Else
                    'ignore, not the same names.
                End If
            End If
        Next
        If possiblematches.Count = 0 Then
            MsgBox("There are no options to copy from.")
        Else
            Dim copyfrom As New CopyIndexInfoFromExisting(possiblematches)
            copyfrom.Owner = GetMainWindow

            If copyfrom.ShowDialog Then
                If System.IO.Directory.Exists(GetCurrentDirectory & "\" & parent.Header & "\" & Header & "\") Then
                Else
                    System.IO.Directory.CreateDirectory(GetCurrentDirectory & "\" & parent.Header & "\" & Header & "\")
                End If
                For Each Str As String In impactareas
                    Dim src As String = GetCurrentDirectory & "\" & copyfrom.AvailableOptions.SelectedItem.ToString.Split("-")(0) & "\" & copyfrom.AvailableOptions.SelectedItem.ToString.Split("-")(1) & "\" & Str & ".xml"
                    If System.IO.File.Exists(GetCurrentDirectory & "\" & copyfrom.AvailableOptions.SelectedItem.ToString.Split("-")(0) & "\" & copyfrom.AvailableOptions.SelectedItem.ToString.Split("-")(1) & "\" & Str & ".xml") Then
                        Dim dest As String = GetCurrentDirectory & "\" & parent.Header & "\" & Header & "\" & Str & ".xml"
                        If System.IO.File.Exists(dest) Then
                            ReportMessage("Deleting " & GetCurrentDirectory & "\" & parent.Header & "\" & Header & "\" & Str & ".xml")
                            System.IO.File.Delete(dest)
                        End If
                        System.IO.File.Copy(src, dest)
                    End If
                Next
                CheckIfIndexLocationsAreReady()
                If _IndexlocationsAreReady Then
                    RaiseEvent IndexLocationsAreReadyEvent(Me, Nothing)
                End If
                MsgBox("Files copied.")
            Else
                ''they said no
            End If
        End If
    End Sub

End Class
