Public Class StudyTreeNode
    Inherits FrameworkTreeNode
    Private _ProjectName As String = ""
    Private _ProjectDescription As String = " "
    'Private _ProjectFile As String = " "
    Private _CreatedBy As String = " "
    Private _CreatedDate As DateTime
    Private _Projectnotes As String = " "
    Private _MonetaryUnit As String = "1,000$'s"
    Private _UnitSystem As String = "English"
    Private _SurveyedYear As Integer = 0
    Private _UpdatedYear As Integer = 0
    Private _UpdatedPriceIndex As Double = 0
    Private _BaseYear As Integer = 0
    Private _MLFYear As Integer = 0
    Private _TreeNodes As New List(Of FrameworkTreeNode)
    Public Event CloseStudy(studytreenode As StudyTreeNode)
    Public Event AddOSM(sender As System.Object, e As System.Windows.RoutedEventArgs)
    Public Event AddMQ(sender As System.Object, e As System.Windows.RoutedEventArgs)
    Private _logfile As String
    Sub New(ByVal studyname As String)
        MyBase.New(studyname)
        _ProjectName = studyname
        '_ProjectFile = GetRootDirectory & "\" & _ProjectName & ".GeoFDA"
        _CreatedBy = System.Environment.UserName
        _CreatedDate = DateTime.Now
        _MonetaryUnit = "1,000$'s" 'convert this to the fda_computation Enum some day.
        _SurveyedYear = Date.Today.Year - 1
        _UpdatedYear = Date.Today.Year
		_UpdatedPriceIndex = 1.0

		WriteToXML()
        initializeLogMenuItem()
    End Sub
    ''' <summary>
    ''' This constructor is intended to be utilized for openeing existing studies
    ''' </summary>
    ''' <param name="xele">an xml document that comes from an existing GeoFDA file (*.GeoFDA)</param>
    ''' <remarks></remarks>
    Sub New(ByVal xele As XElement)
        MyBase.New(xele.Element("StudyTree").Attribute("Header").Value)
        initializeLogMenuItem()
    End Sub
    Public Overrides ReadOnly Property GetSubDirectory As String
        Get
            Return _ProjectName
        End Get
    End Property
    Public ReadOnly Property GetBaseYear As Integer
        Get
            Return _BaseYear
        End Get
    End Property
    Public ReadOnly Property GetFutureYear As Integer
        Get
            Return _MLFYear
        End Get
    End Property
    Public Property MonetaryUnit As String
        Get
            Return _MonetaryUnit
        End Get
        Set(value As String)
            _MonetaryUnit = value
        End Set
    End Property
    Public ReadOnly Property GetProjectFile As String
        Get
            Return GetCurrentDirectory & ".GeoFDA"
        End Get
    End Property
    Public Overrides ReadOnly Property GetNodeName As String
        Get
            Return "StudyTree"
        End Get
    End Property
    Public Overrides Sub SetContextMenu()
        Dim c As New ContextMenu

        Dim PropertiesMenu As New MenuItem
		PropertiesMenu.Header = "Study Properties"
		AddHandler PropertiesMenu.Click, AddressOf DisplayProperties
        c.Items.Add(PropertiesMenu)

        Dim AnalysisYearsMenu As New MenuItem
        AnalysisYearsMenu.Header = "Analysis Years"
        AddHandler AnalysisYearsMenu.Click, AddressOf DisplayAnalysisYears
        c.Items.Add(AnalysisYearsMenu)

        Dim SaveMenu As New MenuItem
        SaveMenu.Header = "Save"
        AddHandler SaveMenu.Click, AddressOf SaveClick
        c.Items.Add(SaveMenu)

        Dim SaveAsMenu As New MenuItem
        SaveAsMenu.Header = "Save As"
        AddHandler SaveAsMenu.Click, AddressOf SaveAsClick
        c.Items.Add(SaveAsMenu)

        Dim CloseMenu As New MenuItem
        CloseMenu.Header = "Close"
        AddHandler CloseMenu.Click, AddressOf CloseClick
        c.Items.Add(CloseMenu)

        'Dim Log As New MenuItem
        'Log.Header = "View Log File"
        'AddHandler Log.Click, AddressOf ViewLogfile
        'c.Items.Add(Log)

        Dim PropertiesMainMenu As New MenuItem
		PropertiesMainMenu.Header = "Study _Properties"
		AddHandler PropertiesMainMenu.Click, AddressOf DisplayProperties

        Dim AnalysisYearsMainMenu As New MenuItem
        AnalysisYearsMainMenu.Header = "_Analysis Years"
        AddHandler AnalysisYearsMainMenu.Click, AddressOf DisplayAnalysisYears

        For Each mi As MenuItem In GetMainContextMenu.Items
            If mi.Header = "_Study" Then
                If Not mi.IsEnabled Then mi.IsEnabled = True
                mi.Items.Add(PropertiesMainMenu)
                mi.Items.Add(AnalysisYearsMainMenu)
            End If
        Next
        'CType(GetMainContextMenu.Items(2), MenuItem).Items.Add(c)
        'CType(GetMainContextMenu.Items(2), MenuItem).IsEnabled = True
        MyBase.ContextMenu = c
    End Sub
    Public Overrides Sub RemoveMainMenuContextOptions()
        For Each mi As MenuItem In GetMainContextMenu.Items
            If mi.Header = "_Study" Then
                Dim listtoremove As New List(Of MenuItem)
                For Each submi As MenuItem In mi.Items
                    'If submi.Header = "_Properties" Then RemoveHandler submi.Click, AddressOf DisplayProperties
                    'If submi.Header = "_Analysis Years" Then RemoveHandler submi.Click, AddressOf DisplayAnalysisYears
                    listtoremove.Add(submi)
                Next
                For i = 0 To listtoremove.Count - 1
                    mi.Items.Remove(listtoremove(i))
                Next
            End If
        Next
        'MyBase.RemoveMainMenuContextOptions()
    End Sub
    Public Overrides Sub AddFrameworkChildren()
        ' Dim w As New WatershedTreeNode()
        Dim t As New TerrainParentTreeNode()
        Dim h As New HydraulicsParentTreeNode()
        Dim i As New InventoryTreeNode()
        Dim ia As New ImpactAreaParentTreeNode()
        Dim O As New OutputParentTreeNode()
        'AddFrameworkTreeNode(w)
        AddFrameworkTreeNode(t)
        AddFrameworkTreeNode(h)
        AddFrameworkTreeNode(i)
        AddFrameworkTreeNode(ia)
        AddFrameworkTreeNode(O)
        'w.AddFrameworkChildren()
        t.AddFrameworkChildren()
        h.AddFrameworkChildren()
        i.AddFrameworkChildren()
        ia.AddFrameworkChildren()
        O.AddFrameworkChildren()
        WriteToXML()
        CheckForErrors()
    End Sub
    Public Overrides Sub AddFrameworkChildren(ele As System.Xml.Linq.XElement)
        'add all map layers first, and have each element check if its map is added to the map window.
        MyBase.Cursor = Cursors.Wait
        ReadFromXMlelement(ele)
        'Dim w As New WatershedTreeNode()
        Dim t As New TerrainParentTreeNode()
        Dim h As New HydraulicsParentTreeNode()
        Dim i As New InventoryTreeNode()
        Dim ia As New ImpactAreaParentTreeNode()
        Dim O As New OutputParentTreeNode()
        'AddFrameworkTreeNode(w)
        AddFrameworkTreeNode(t)
        AddFrameworkTreeNode(h)
        AddFrameworkTreeNode(i)
        AddFrameworkTreeNode(ia)
        AddFrameworkTreeNode(O)
        Dim s As XElement = ele.Element(Me.GetNodeName)
        t.AddFrameworkChildren(s.Element(t.GetNodeName))
        'w.AddFrameworkChildren(s.Element(w.GetNodeName))
        h.AddFrameworkChildren(s.Element(h.GetNodeName))
        i.AddFrameworkChildren(s.Element(i.GetNodeName))
        ia.AddFrameworkChildren(s.Element(ia.GetNodeName))
        O.AddFrameworkChildren(s.Element(O.GetNodeName))
        MyBase.Cursor = Nothing
        'Me.Height = 30
        Saved = True
    End Sub
    Public Overrides Sub AddFrameworkTreeNode(FTN As FrameworkTreeNode)
        MyBase.AddFrameworkTreeNode(FTN)
        _TreeNodes.Add(FTN) 'add yourself?
        Saved = False
    End Sub
    Public Overrides Sub Bubble(ftn As FrameworkTreeNode)
        _TreeNodes.Add(ftn)
        Saved = False
    End Sub
    Public Sub RemoveFrameworkTreeNode(FTN As FrameworkTreeNode)
        Dim ret As New List(Of FrameworkTreeNode)
        For i = 0 To _TreeNodes.Count - 1
            If _TreeNodes(i).GetType = FTN.GetType Then
                If _TreeNodes(i).Header = FTN.Header Then ret.Add(_TreeNodes(i))
            End If
        Next
        If ret.Count = 0 Then MsgBox("No nodes of that name and type were found!") : Exit Sub
        If ret.Count > 1 Then
            For i As Integer = 0 To ret.Count - 1 'there should only ever be two with the same name
                If (DirectCast(ret(i), HydraulicsChildTreenode).GetYear = DirectCast(FTN, HydraulicsChildTreenode).GetYear) Then _TreeNodes.Remove(FTN)
            Next
        End If
        If ret.Count = 1 Then _TreeNodes.Remove(ret(0))
    End Sub
    Protected Overrides Function CheckforNameConflicts(ftn As FrameworkTreeNode) As Boolean
        If IsNothing(_TreeNodes) Then Return False
        For i = 0 To _TreeNodes.Count - 1
            If _TreeNodes(i).GetType = ftn.GetType Then
                If _TreeNodes(i).Header = ftn.Header Then
                    Return True
                Else
                    Return False
                End If
            End If
        Next
        Return False
    End Function
    Protected Overrides Function GetAllFrameworkTreenodesOfType(NodeTypeToGetInfoFrom As Type) As List(Of FrameworkTreeNode)
        Dim ret As New List(Of FrameworkTreeNode)
        If NodeTypeToGetInfoFrom.Name = Me.GetType.Name Then
            ret.Add(Me)
        End If
        For i = 0 To _TreeNodes.Count - 1
            If _TreeNodes(i).GetType = NodeTypeToGetInfoFrom Then
                ret.Add(_TreeNodes(i))
            End If
        Next
        Return ret
    End Function
    Private Sub DisplayProperties(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim p As New Properties(GetProjectFile)
        AddHandler p.SendMessage, AddressOf ReportMessage
        p.ShowDialog()
        If p.DialogResult = True Then
            _ProjectDescription = p.ProjectDescription
            _Projectnotes = p.ProjectNotes
            If _MonetaryUnit = p.MonetaryUnits Then
            Else
                Dim snodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(StructureInventoryChildTreeNode))
                If snodes.Count > 0 Then
                    MsgBox("You changed monetary units with existing structures, this is not allowed.  Monetary units are still " & _MonetaryUnit)
                Else
                    _MonetaryUnit = p.MonetaryUnits
                End If
            End If

            _UnitSystem = p.UnitsSystem
            _SurveyedYear = p.SurveyedYear
            _UpdatedYear = p.UpdatedYear
            _UpdatedPriceIndex = p.UpdatedPriceIndex
            WriteToXML()
        Else
            'not true!
        End If
        CheckForErrors()
    End Sub
    Private Sub DisplayAnalysisYears(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim a As AnalysisYears
        If _BaseYear = 0 And _MLFYear = 0 Then
            a = New AnalysisYears
        Else
            a = New AnalysisYears(_BaseYear, _MLFYear)
        End If
        Dim hnodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(HydraulicsChildTreenode))
        Dim onodes As List(Of FrameworkTreeNode) = GetAllFrameworkTreenodesOfType(GetType(OutputChildTreeNode))
        If a.ShowDialog() Then
            'update information based on values
            Dim newBaseYear As Integer = CInt(a.BaseYear)
            Dim newMLFYear As Integer = Nothing
            If a.MLFYear = "" Then
                newMLFYear = Nothing
            Else
                newMLFYear = CInt(a.MLFYear)
            End If
            If _BaseYear = newBaseYear Then
            Else
                ''changes occured

                For Each Hydronode As HydraulicsChildTreenode In hnodes
                    If Hydronode.GetYear = _BaseYear Then
                        Hydronode.SetYear = newBaseYear
                        For Each outnode As OutputChildTreeNode In onodes
                            For i = 0 To outnode.GetPlans.Count - 1
                                If outnode.GetPlans(i).Header = Hydronode.Header AndAlso outnode.GetPlans(i).GetYear = Hydronode.GetYear Then
                                    outnode.AlertUserThatUpdatesAreNeeded("The base year changed and computations need to update.", True, False)
                                End If
                            Next
                        Next
                    End If
                Next

                _BaseYear = newBaseYear
            End If

            If IsNothing(_MLFYear) Then
                If IsNothing(newMLFYear) Then
                    _MLFYear = Nothing
                Else
                    '' they made a chage but no previous nodes could exist, because mlfyear was nothing...
                    _MLFYear = newMLFYear
                End If
            Else
                If IsNothing(newMLFYear) Then
                    ''they made a chage
                    For Each Hydronode As HydraulicsChildTreenode In hnodes
                        If Hydronode.GetYear = _MLFYear Then
                            MsgBox("This most likely future year is used by the hydraulics water surface profile named:" & Hydronode.Header & vbNewLine & "You cannot set it to nothing")
                            WriteToXML()
                            CheckForErrors()
                            Exit Sub
                        End If
                    Next
                    _MLFYear = Nothing
                Else
                    If _MLFYear = newMLFYear Then
                    Else
                        ''they made a chage
                        For Each Hydronode As HydraulicsChildTreenode In hnodes
                            If Hydronode.GetYear = _MLFYear Then
                                Hydronode.SetYear = newMLFYear
                                For Each outnode As OutputChildTreeNode In onodes
                                    For i = 0 To outnode.GetPlans.Count - 1
                                        If outnode.GetPlans(i).Header = Hydronode.Header AndAlso outnode.GetPlans(i).GetYear = Hydronode.GetYear Then
                                            outnode.AlertUserThatUpdatesAreNeeded("The most likely future year changed and computations need to update.", True, False)
                                        End If
                                    Next
                                Next
                            End If
                        Next
                        _MLFYear = newMLFYear
                    End If

                End If
            End If
            WriteToXML()
        Else
            'user did not press ok.
        End If
        CheckForErrors()
    End Sub

    Private Sub SaveAsClick(sender As Object, e As System.Windows.RoutedEventArgs)
        Dim cns As New CreateNewStudy("Study Save As", "Please select a new name and a new location to save your study", "SaveAsHelp.xml")
        cns.Owner = GetMainWindow
        cns.ShowDialog()
        If cns.UserPressedOk Then
            If MsgBox("All maps in the map window will be removed as a part of this save as action. Do you wish to continue?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then Exit Sub
            'copy all directories first.
            If cns.StudyPath.Contains(Left(GetRootDirectory, Len(GetRootDirectory) - 1)) Then MsgBox("The new directory cannot be inside the current directory") : Exit Sub
            Dim l As New List(Of String)
            Do Until GetMapTreeView.Items.Count = 0
                'l.Add(CType(GetMapTreeView.Items(GetMapTreeView.Items.Count - 1), OpenGLMapping.FeatureNode).Features.Features.GetSource())
                CType(GetMapTreeView.Items(GetMapTreeView.Items.Count - 1), OpenGLMapping.FeatureNode).RemoveLayer()
            Loop
            My.Computer.FileSystem.CopyDirectory(GetRootDirectory, cns.StudyPath & "\" & cns.StudyName)
                'kill the old map and geofda file
                Kill(cns.StudyPath & "\" & cns.StudyName & "\" & Header & ".GeoFDA")
                Kill(cns.StudyPath & "\" & cns.StudyName & "\" & Header & "MapProperties.xml")
                FrameworkTreeNode.SetRootDirectory = cns.StudyPath & "\" & cns.StudyName & "\"
                _ProjectName = cns.StudyName
                MyBase.Header = cns.StudyName
                'SetUserDefinedName = cns.StudyName
                _CreatedBy = System.Environment.UserName
                _CreatedDate = Date.Today


            WriteToXML()
            Else
                'its not ok!
            End If
    End Sub
    Private Sub SaveClick(sender As Object, e As System.Windows.RoutedEventArgs)
        'RaiseEvent Save()
        WriteToXML()
    End Sub
    Private Sub CloseClick(sender As Object, e As System.Windows.RoutedEventArgs)
        RaiseEvent CloseStudy(Me)
    End Sub
    Public Overrides Sub OnSaveEvent(sender As Object, e As System.Windows.RoutedEventArgs)
        SaveClick(Nothing, Nothing)
    End Sub
    Public Sub OnSaveAsEvent(sender As Object, e As System.Windows.RoutedEventArgs)
        SaveAsClick(Nothing, Nothing)
    End Sub
    Public Sub OnCloseStudyEvent(sender As Object, e As System.Windows.RoutedEventArgs)
        CloseClick(Nothing, Nothing)
    End Sub
    Public Function CheckForErrors() As Boolean
        Dim counter As Integer = 0
        Dim message As New System.Text.StringBuilder
        If GetBaseYear = 0 Then counter += 1 : message.Append("The Base Year is Zero")
        'If GetFutureYear = 0 Then counter += 1 : message.AppendLine("The Future Year is Zero")
        If GetFutureYear <> 0 AndAlso GetBaseYear >= GetFutureYear Then counter += 1 : message.AppendLine("The Future Year is before or Equal to the Base Year")
        If counter = 0 Then MyBase.Foreground = Brushes.Black : MyBase.ToolTip = "" : Return False
        MyBase.Foreground = Brushes.Red
        MyBase.ToolTip = message.ToString
        Return True
    End Function
    Public Overrides Sub ReadFromXMlelement(ele As XElement)
        Dim props As XElement = ele.Element("Properties")
        Dim analysisYears As XElement = ele.Element("Analysis_Years")
        _ProjectName = ele.Attribute("Name")
        _ProjectDescription = CType(props.Element("Description").Value, String)
        _CreatedBy = CType(props.Element("Created_By").Value, String)
        _CreatedDate = CType(props.Element("Created_On").Value, Date)
        _Projectnotes = CType(props.Element("Notes").Value, String)
        _MonetaryUnit = CType(props.Element("Monetary_Unit").Value, String)
        _SurveyedYear = CType(props.Element("Surveyed_Year").Value, Integer)
        _UpdatedYear = CType(props.Element("Updated_Year").Value, Integer)
        _UpdatedPriceIndex = CType(props.Element("Updated_Price_Index").Value, Double)

        _BaseYear = CType(analysisYears.Element("Base_Year").Value, Integer)
        _MLFYear = CType(analysisYears.Element("Most_Likely_Future_Year").Value, Integer)
        CheckForErrors()
    End Sub

    Public Overrides Sub WriteToXML()
        Dim doc As New XDocument
        Dim Project As New XElement("Project")
        Project.SetAttributeValue("Name", _ProjectName)
        'Project.SetAttributeValue("Path", GetProjectFile)
        Dim Properties As New XElement("Properties")

        Dim Desc As New XElement("Description")
        Desc.Value = _ProjectDescription
        Properties.Add(Desc)

        Dim Creator As New XElement("Created_By")
        Creator.Value = _CreatedBy
        Properties.Add(Creator)

        Dim CreationDate As New XElement("Created_On")
        CreationDate.Value = _CreatedDate.ToString
        Properties.Add(CreationDate)

        Dim ProjectNotes As New XElement("Notes")
        ProjectNotes.Value = _Projectnotes
        Properties.Add(ProjectNotes)

        Dim MonetaryUnits As New XElement("Monetary_Unit")
        MonetaryUnits.Value = _MonetaryUnit
        Properties.Add(MonetaryUnits)


        Dim MeasurementUnits As New XElement("Measurement_Unit")
        MeasurementUnits.Value = _UnitSystem 'need to add measurment unit (feet, meters)
        Properties.Add(MeasurementUnits)

        Dim SurveyedYear As New XElement("Surveyed_Year")
        SurveyedYear.Value = _SurveyedYear
        Properties.Add(SurveyedYear)

        Dim UpdatedYear As New XElement("Updated_Year")
        UpdatedYear.Value = _UpdatedYear
        Properties.Add(UpdatedYear)

        Dim UPI As New XElement("Updated_Price_Index")
        UPI.Value = _UpdatedPriceIndex
        Properties.Add(UPI)

        Dim AnalysisYears As New XElement("Analysis_Years")

        Dim baseyear As New XElement("Base_Year")
        baseyear.Value = _BaseYear

        Dim MLFYear As New XElement("Most_Likely_Future_Year")
        MLFYear.Value = _MLFYear

        AnalysisYears.Add(baseyear)
        AnalysisYears.Add(MLFYear)

        Project.Add(Properties)
        Project.Add(AnalysisYears)
        Project.Add(WriteToXMLElement)

        doc.Add(Project)
        doc.Save(GetProjectFile)

        Dim mapdoc As New XDocument()
        mapdoc.Add(GetMapTreeView.WriteToXElement(Left(GetRootDirectory, GetRootDirectory.Length - 1)))
        mapdoc.Save(GetCurrentDirectory & "MapProperties.xml")
        
        Saved = True
    End Sub
    Public Overrides Function WriteToXMLElement() As System.Xml.Linq.XElement
        Dim ele As New XElement(GetNodeName)
        ele.SetAttributeValue("Header", MyBase.Header.ToString)
        For i = 0 To FirstLevelSubNodes.Count - 1
            ele.Add(FirstLevelSubNodes(i).WriteToXMLElement)
        Next
        Return ele
    End Function
    Public Overrides Sub ReadFromXMl(path As String)
    End Sub
    Public Sub ViewLogFile(sender As Object, e As System.Windows.RoutedEventArgs)
        If IsNothing(_logfile) OrElse _logfile = "" Then
            MsgBox("There have been no messages reported to the log file yet.")
        Else
            Dim lv As New Logger(_logfile)
            AddHandler lv.LogCleared, AddressOf DisableLogViewer
            lv.Owner = GetMainWindow
            lv.Show()
        End If
    End Sub
    Public Overrides Sub ReportMessage(message As String)
        WriteToLogFile(message)
    End Sub
    Private Sub DisableLogViewer()
        For Each mi As MenuItem In GetMainContextMenu.Items
            If mi.Header = "_Log" Then mi.IsEnabled = False
        Next
    End Sub
    Private Sub initializeLogMenuItem()
        If IsNothing(_logfile) OrElse _logfile = "" Then
            'get a temp file
            _logfile = System.IO.Path.GetTempPath & MyBase.Header & "_LogFile.txt"
        Else
        End If
        If System.IO.File.Exists(_logfile) Then
            Dim fs As New System.IO.FileStream(_logfile, IO.FileMode.Open)
            Dim sr As New System.IO.StreamReader(fs)
            Dim s As String = sr.ReadToEnd
            If s.Equals("") Then
            Else
                For Each mi As MenuItem In GetMainContextMenu.Items
                    If mi.Header = "_Log" Then mi.IsEnabled = True
                Next
            End If
            sr.Close() : sr.Dispose()
            fs.Close() : fs.Dispose()
        End If

    End Sub

    Public Sub WriteToLogFile(ByVal messagetoadd As String)
        initializeLogMenuItem()
        For Each mi As MenuItem In GetMainContextMenu.Items
            If mi.Header = "_Log" Then mi.IsEnabled = True
        Next
        Dim fs As System.IO.FileStream
        If Not System.IO.File.Exists(_logfile) Then
            'create it
            fs = New System.IO.FileStream(_logfile, IO.FileMode.Create)
        Else
            fs = New System.IO.FileStream(_logfile, IO.FileMode.Append)
        End If
        Dim sr As New System.IO.StreamWriter(fs)
        sr.WriteLine(messagetoadd)
        sr.Close() : sr.Dispose()
        fs.Close() : fs.Dispose()
        '
    End Sub
End Class
