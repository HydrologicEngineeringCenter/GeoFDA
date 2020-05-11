Public Class Properties
    Private _ProjectName As String
    Private _ProjectDescription As String
    Private _ProjectFile As String
    Private _CreatedBy As String
    Private _CreatedDate As DateTime
    Private _Projectnotes As String
    Private _MonetaryUnit As String
    Private _UnitSystem As String
    Private _SurveyedYear As Integer
    Private _UpdatedYear As Integer
    Private _UpdatedPriceIndex As Double
    Public Event SendMessage(ByVal message As String)
    Sub New(ByVal Path As String)
        ' This call is required by the designer.
        ' Add any initialization after the InitializeComponent() call.
        Dim ba As Byte() = System.IO.File.ReadAllBytes(Path)
        Dim ms As New System.IO.MemoryStream(ba)
        Dim doc As New XDocument
        doc = XDocument.Load(ms)
        ms.Dispose()
        Dim root As XElement = doc.Root
        Dim props As XElement = root.Element("Properties")

        _ProjectName = doc.Element("Project").Attribute("Name").Value.ToString
        _ProjectFile = Path
        _CreatedBy = CType(props.Element("Created_By").Value, String)
        _CreatedDate = CType(props.Element("Created_On").Value, Date)
        ProjectDescription = CType(props.Element("Description").Value, String)

        ProjectNotes = CType(props.Element("Notes").Value, String)
        MonetaryUnits = CType(props.Element("Monetary_Unit").Value, String)
        UnitsSystem = CType(props.Element("Measurement_Unit").Value, String)
        SurveyedYear = CType(props.Element("Surveyed_Year").Value, Integer)
        UpdatedYear = CType(props.Element("Updated_Year").Value, Integer)
        UpdatedPriceIndex = CType(props.Element("Updated_Price_Index").Value, Double)

        InitializeComponent()
        initializeCmbMonetaryUnits()
        TxtProjectDescription.DescriptionWindow.Description = _ProjectDescription
        TxtProjectNotes.DescriptionWindow.Description = _Projectnotes
        CmbMonetaryUnits.SelectedItem = _MonetaryUnit
        TBUnitsSystem.Text = _UnitSystem
    End Sub
    Public ReadOnly Property ProjectName As String
        Get
            Return _ProjectName
        End Get
    End Property
    Public ReadOnly Property ProjectFile As String
        Get
            Return _ProjectFile
        End Get
    End Property
    Public ReadOnly Property CreatedBy As String
        Get
            Return _CreatedBy
        End Get
    End Property
    Public ReadOnly Property CreatedDate As DateTime
        Get
            Return _CreatedDate
        End Get
    End Property
    Public Property ProjectDescription As String
        Get
            Return _ProjectDescription
        End Get
        Set(value As String)
            _ProjectDescription = value
        End Set
    End Property
    Public Property ProjectNotes As String
        Get
            Return _Projectnotes
        End Get
        Set(value As String)
            _Projectnotes = value
        End Set
    End Property
    Public Property MonetaryUnits As String
        Get
            Return _MonetaryUnit
        End Get
        Set(value As String)
            _MonetaryUnit = value
        End Set
    End Property
    Public Property UnitsSystem As String
        Get
            Return _UnitSystem
        End Get
        Set(value As String)
            _UnitSystem = value
        End Set
    End Property
    Public Property SurveyedYear As Integer
        Get
            Return _SurveyedYear
        End Get
        Set(value As Integer)
            _SurveyedYear = value
        End Set
    End Property
    Public Property UpdatedYear As Integer
        Get
            Return _UpdatedYear
        End Get
        Set(value As Integer)
            _UpdatedYear = value
        End Set
    End Property
    Public Property UpdatedPriceIndex As Double
        Get
            Return _UpdatedPriceIndex
        End Get
        Set(value As Double)
            _UpdatedPriceIndex = value
        End Set
    End Property
    Sub initializeCmbMonetaryUnits()
        CmbMonetaryUnits.Items.Add("$'s")
        CmbMonetaryUnits.Items.Add("1,000$'s")
        CmbMonetaryUnits.Items.Add("1,000,000$'s")
    End Sub
    Private Sub Cmdok_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles Cmdok.Click
        'do validation here.
        If Not Integer.TryParse(TxtSurveyedYear.Text, _SurveyedYear) Then MsgBox("Your surveyed year could not be converted to integer") : Exit Sub
        If Not Integer.TryParse(TxtUpdatedYear.Text, _UpdatedYear) Then MsgBox("Your updated year could not be converted to integer") : Exit Sub
        If Not Double.TryParse(TxtUpdatedPriceIndex.Text, _UpdatedPriceIndex) Then MsgBox("Your updated price index could not be converted to a percentage value") : Exit Sub
        If _SurveyedYear > _UpdatedYear Then MsgBox("Your updated year is after your surveyed year, that shouldnt be... try again") : Exit Sub
        If _SurveyedYear < 1900 Then MsgBox("Your surveyed year is prior to 1900") : Exit Sub
        If _SurveyedYear > Date.Now.Year Then MsgBox("Your surveyed year is in the future, please define something from the recent past.") : Exit Sub
        If _UpdatedYear < 1900 Then MsgBox("Your updated year is prior to 1900") : Exit Sub
        If _UpdatedYear > Date.Now.Year Then MsgBox("Your updated year is in the future, please define something from the recent past.") : Exit Sub
        If Math.Abs(_UpdatedPriceIndex) > 1 Then MsgBox("Your updated price index is greater than 1, this number should be expressed as a percent and usually be between zero and 1") : Exit Sub 'check on this.
        _ProjectDescription = TxtProjectDescription.DescriptionWindow.Description
        _Projectnotes = TxtProjectNotes.DescriptionWindow.Description
        _MonetaryUnit = CmbMonetaryUnits.SelectedItem
        If _ProjectDescription = "" Then _ProjectDescription = " " 'cannot be blank for xml...
        If _Projectnotes = "" Then _Projectnotes = " "
        Me.DialogResult = True
        Me.Close()
    End Sub

    Private Sub CmdClose_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles CmdClose.Click
        Me.Close()
    End Sub
End Class
