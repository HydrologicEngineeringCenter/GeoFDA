Namespace ComputableObjects
    Public Class OccupancyTypes
        Private _Occtypes As List(Of OccupancyType)
        Public Event ReportMessage(message As String)
        Public Property OccupancyTypes As List(Of OccupancyType)
            Get
                Return _Occtypes
            End Get
            Set(value As List(Of OccupancyType))
                _Occtypes = value
            End Set
        End Property
        Public Function DamageCategories() As List(Of DamageCategory)
            Dim ret As New List(Of DamageCategory)
            For Each ot As OccupancyType In _Occtypes
                If ret.Contains(ot.DamageCategory) Then
                Else
                    ret.Add(ot.DamageCategory)
                End If
            Next
            Return ret
        End Function
        Public Function GetOcctypesByName(ByVal name As String) As List(Of OccupancyType)
            If IsNothing(_Occtypes) Then Return Nothing
            If _Occtypes.Count = 0 Then Return Nothing
            Dim ret As New List(Of OccupancyType)
            For i As Integer = 0 To _Occtypes.Count - 1
                If _Occtypes(i).Name = name Then ret.Add(_Occtypes(i))
            Next
            Return ret
        End Function
        Public Function GetOcctypeByNameAndDamCat(ByVal name As String, ByVal damcatname As String) As OccupancyType
            If IsNothing(_Occtypes) Then Return Nothing
            If _Occtypes.Count = 0 Then Return Nothing
            Dim ret As New List(Of OccupancyType)
            For i As Integer = 0 To _Occtypes.Count - 1
                If _Occtypes(i).Name = name AndAlso _Occtypes(i).DamageCategory.Name = damcatname Then ret.Add(_Occtypes(i))
            Next
            If ret.Count = 0 Then Return Nothing
            If ret.Count > 1 Then Throw New Exception("More than one occtype exists with that name and damage category")
            Return ret(0)
        End Function
        Sub New()
            _Occtypes = New List(Of OccupancyType)
        End Sub
        ''' <summary>
        ''' Will load the occupancy types from a file, the format can be either .txt (FDA format see users guide appendix G) or xml (our new standard output)
        ''' </summary>
        ''' <param name="inputfile"></param>
        ''' <remarks></remarks>
        Sub New(ByVal inputfile As String)
            LoadFromFile(inputfile)
        End Sub
        Public Function GetOccTypeIndex(ByVal Name As String) As Int32
            If IsNothing(_Occtypes) Then Return -1
            Return _Occtypes.FindIndex(Function(c) c.Name = Name)
        End Function
        ''' <summary>
        ''' Will load the occupancy types from a file, the format can be either .txt (FDA format see users guide appendix G) or xml (our new standard output)
        ''' </summary>
        ''' <param name="inputfile"></param>
        ''' <remarks></remarks>
        Public Sub LoadFromFile(ByVal inputfile As String)
            If IsNothing(_Occtypes) Then _Occtypes = New List(Of OccupancyType)
            Select Case System.IO.Path.GetExtension(inputfile).ToLower
                Case ".txt"
                    'fda text reader
                    Dim fs As New System.IO.FileStream(inputfile, IO.FileMode.Open, IO.FileAccess.Read)
                    Dim sr As New System.IO.StreamReader(fs)
                    Dim occtypestring As System.Text.StringBuilder
                    Dim tabbedline() As String
                    Dim tmpline As String = ""
                    Dim startdata As Integer = 0
                    Dim parameter As Integer = 0
                    tabbedline = Split(sr.ReadLine, vbTab)
                    Do Until tabbedline(0) = "Occ_Name"
                        If Not sr.EndOfStream Then
                            tabbedline = Split(sr.ReadLine, vbTab)
                        Else
                            Throw New Exception("This file did not contain the correct format for FDA occupancy types")
                        End If
                    Loop
                    startdata = Array.IndexOf(tabbedline, "Start_Data")
                    parameter = Array.IndexOf(tabbedline, "Parameter")
                    Do Until sr.EndOfStream
                        occtypestring = New System.Text.StringBuilder
                        tmpline = sr.ReadLine
                        If tmpline = "" OrElse tmpline = vbNewLine OrElse tmpline.Substring(0, 7) = "Struc_N" OrElse tmpline.Substring(0, 8) = "Mod_Name" Then Exit Do ' what if the last line is a return or a blank line...
                        occtypestring.AppendLine(tmpline)
                        tabbedline = Split(tmpline, vbTab)
                        Do Until tabbedline.Count - 1 < parameter OrElse tabbedline(parameter) = "Struct"
                            If sr.EndOfStream Then
                                Throw New Exception("Encountered an occupancy type without a 'Struct' line")
                            Else
                                tmpline = sr.ReadLine
                                occtypestring.AppendLine(tmpline)
                                tabbedline = Split(tmpline, vbTab)
                            End If
                        Loop
                        If tabbedline.Count - 1 < parameter Then
                        Else
                            Dim ot As New OccupancyType
                            AddHandler ot.ReportMessage, AddressOf OnReportMessage
                            ot.LoadFromFDAInformation(occtypestring, startdata, parameter)
                            _Occtypes.Add(ot)
                        End If

                    Loop
                    sr.Close() : sr.Dispose()
                    fs.Close() : fs.Dispose()
                Case ".xml"
                    'create a reader for xml.
                    Dim ba As Byte() = System.IO.File.ReadAllBytes(inputfile)
                    Dim ms As New System.IO.MemoryStream(ba)
                    Dim xdoc As New XDocument
                    xdoc = XDocument.Load(ms)
                    ms.Dispose()
                    Dim xel As XElement = xdoc.Root
                    For Each ele As XElement In xel.Elements
                        _Occtypes.Add(New OccupancyType(ele))
                    Next
            End Select
        End Sub
        Public Sub OnReportMessage(ByVal message As String)
            RaiseEvent ReportMessage(message)
        End Sub
        Public Sub WriteToXML(ByVal outputpath As String)
            Dim doc As New XDocument
            Dim occtypes As New XElement("OccTypes")
            For i As Integer = 0 To _Occtypes.Count - 1
                occtypes.Add(_Occtypes(i).WriteToXElement)
            Next
            doc.Add(occtypes)
            doc.Save(outputpath)
        End Sub
        Public Function WriteToFDAString() As String
            Dim str As New System.Text.StringBuilder
            str.AppendLine("Occ_Name" & vbTab & "Occ_Description" & vbTab & "Cat_Name" & vbTab & "Parameter" & vbTab & "Start_Data" & vbTab)
            For i = 0 To _Occtypes.Count - 1
                str.AppendLine(_Occtypes(i).WriteToFDAString)
            Next
            Return str.ToString
        End Function
    End Class

End Namespace

