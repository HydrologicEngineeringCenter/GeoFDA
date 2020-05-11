Public Class XMLIndexLocation
    Implements System.ComponentModel.INotifyPropertyChanged
    Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Private _ItemsSource As System.Collections.ObjectModel.ObservableCollection(Of ProbFlowStage)
    Private _river As String
    Private _reach As String
    Private _riverstation As Single
    Private _invert As Single
    Private _reachname As String
    Private _HasChanges As Boolean = False
    Private _IsLoaded As Boolean = False
    Public Property ItemsSoucre As System.Collections.ObjectModel.ObservableCollection(Of ProbFlowStage)
        Get
            Return _ItemsSource
        End Get
        Set(value As System.Collections.ObjectModel.ObservableCollection(Of ProbFlowStage))
            _ItemsSource = value
            If _IsLoaded Then _HasChanges = True
            NotifyPropertyChanged("ItemsSoucre")
        End Set
    End Property
    Public ReadOnly Property HasChanges As Boolean
        Get
            Return _HasChanges
        End Get
    End Property
    Public Property River As String
        Get
            Return _river
        End Get
        Set(value As String)
            _river = value
            If _IsLoaded Then _HasChanges = True
            NotifyPropertyChanged("River")
        End Set
    End Property
    Public Property Reach As String
        Get
            Return _reach
        End Get
        Set(value As String)
            _reach = value
            If _IsLoaded Then _HasChanges = True
            NotifyPropertyChanged("Reach")
        End Set
    End Property
    Public Property ReachName As String
        Get
            Return _reachname
        End Get
        Set(value As String)
            _reachname = value
            If _IsLoaded Then _HasChanges = True
            NotifyPropertyChanged("ReachName")
        End Set
    End Property
    Public Property Invert As Single
        Get
            Return _invert
        End Get
        Set(value As Single)
            _invert = value
            If _IsLoaded Then _HasChanges = True
            NotifyPropertyChanged("Invert")
        End Set
    End Property
    Public Property Stationing As Single
        Get
            Return _riverstation
        End Get
        Set(value As Single)
            If value = _riverstation Then
            Else
                _riverstation = value
                If _IsLoaded Then _HasChanges = True
                NotifyPropertyChanged("Stationing")
            End If

        End Set
    End Property
    Sub WriteToXMLfile(ByVal directory As String)
        Dim root As New XElement("IndexLocation")
        root.SetAttributeValue("Name", _reachname)
        root.SetAttributeValue("River", _river)
        root.SetAttributeValue("Reach", _reach)
        root.SetAttributeValue("Station", _riverstation)
        root.SetAttributeValue("Invert", _invert)
        Dim values As New XElement("Values")
        For i = 0 To _ItemsSource.Count - 1
            Dim ord As New XElement("Ordinate")
            ord.SetAttributeValue("Probability", _ItemsSource(i).Probability)
            ord.SetAttributeValue("Flow", _ItemsSource(i).Flow)
            ord.SetAttributeValue("Stage", _ItemsSource(i).WaterSurfaceElevation)
            values.Add(ord)
        Next
        root.Add(values)
        Dim d As New XDocument()
        d.Add(root)
        d.Save(directory & "\" & _reachname & ".xml")
    End Sub
    'read from file
    Public Sub ReadFromXml(path As String)
        If System.IO.File.Exists(path) Then
            Dim ba As Byte() = System.IO.File.ReadAllBytes(path)
            Dim ms As New System.IO.MemoryStream(ba)
            Dim xdoc As New XDocument
            xdoc = XDocument.Load(ms)
            ms.Dispose()
            Dim root As XElement = xdoc.Root
            ReachName = root.Attribute("Name").Value
            River = root.Attribute("River").Value
            Reach = root.Attribute("Reach").Value
            Invert = Convert.ToSingle(root.Attribute("Invert").Value)
            Stationing = Convert.ToSingle(root.Attribute("Station").Value)
            Dim values As XElement = root.Element("Values")
            Dim tmpsrc As New ObjectModel.ObservableCollection(Of ProbFlowStage)
            For Each ele As XElement In values.Elements
                tmpsrc.Add(New ProbFlowStage(Convert.ToSingle(ele.Attribute("Probability").Value), Convert.ToSingle(ele.Attribute("Flow").Value), Convert.ToSingle(ele.Attribute("Stage").Value)))

            Next
            ItemsSoucre = tmpsrc
        Else
            If Not System.IO.Directory.Exists(path) Then System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path))
            Reach = System.IO.Path.GetFileNameWithoutExtension(path)
            ReachName = System.IO.Path.GetFileNameWithoutExtension(path)
            River = " "
            Invert = 0
            Stationing = 0
            Dim tmpsrc As New ObjectModel.ObservableCollection(Of ProbFlowStage)
            For i = 0 To 7
                tmpsrc.Add(New ProbFlowStage(0, 0, 0))
            Next
            ItemsSoucre = tmpsrc
        End If
        _IsLoaded = True
    End Sub
    Public Function ReadFromXMLToIndex(path As String) As FDA_Computation.IndexLocation
        ReadFromXml(path)
        Dim probs(7) As Single
        Dim flows(7) As Single
        Dim wses(7) As Single
        For i = 0 To _ItemsSource.Count - 1
            probs(i) = _ItemsSource(i).Probability
            flows(i) = _ItemsSource(i).Flow
            wses(i) = _ItemsSource(i).WaterSurfaceElevation
        Next
        Return New FDA_Computation.IndexLocation(_river, _reach, New FDA_Computation.RiverStation(_riverstation, _invert, probs, flows, wses))
    End Function
    Public Function Validate() As String
        Dim msg As New System.Text.StringBuilder
        If _river = "" Then msg.AppendLine("The river is not named")
        If Reach = "" Then msg.AppendLine("The reach is blank, that is not a valid input.")
        If Stationing = 0 Then msg.AppendLine("The stationing is zero, that is not a valid input.")
        For i = 1 To _ItemsSource.Count - 1
            If _ItemsSource(i).Probability >= _ItemsSource(i - 1).Probability Then msg.AppendLine("The probabilities are not monotonically decreasing at row " & i & ".")
            If _ItemsSource(i).Flow <= _ItemsSource(i - 1).Flow Then msg.AppendLine("The Flows are not monotonically increasing at row " & i & ".")
            If _ItemsSource(i).WaterSurfaceElevation <= _ItemsSource(i - 1).WaterSurfaceElevation Then msg.AppendLine("The Water Surface Elevations are not monotonically increasing at row " & i & ".")
        Next
        Return msg.ToString
    End Function
    Private Sub NotifyPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(info))
    End Sub
    Class ProbFlowStage
        Private _Prob As Single
        Private _Flow As Single
        Private _stage As Single
        Sub New(ByVal prob As Single, ByVal flow As Single, ByVal stage As Single)
            _Prob = prob
            _Flow = flow
            _stage = stage
        End Sub
        Public Property Probability As Single
            Get
                Return _Prob
            End Get
            Set(value As Single)
                _Prob = value
            End Set
        End Property
        Public Property Flow As Single
            Get
                Return _Flow
            End Get
            Set(value As Single)
                _Flow = value
            End Set
        End Property
        Public Property WaterSurfaceElevation As Single
            Get
                Return _stage
            End Get
            Set(value As Single)
                _stage = value
            End Set
        End Property
    End Class
End Class
