Namespace ComputableObjects
    Public Class DamageCategories
        Private _DamCats As List(Of DamageCategory)
        Sub New()
            _DamCats = New List(Of DamageCategory)
        End Sub
        Sub New(ByVal damcats As List(Of DamageCategory))
            _DamCats = damcats
        End Sub
        Sub New(ByVal path As String)
            'need to differentiate between xml and txt.
            _DamCats = New List(Of DamageCategory)
            Select Case System.IO.Path.GetExtension(path).ToLower
                Case ".txt"
                    Dim fs As New System.IO.FileStream(path, IO.FileMode.Open, IO.FileAccess.Read)
                    Dim sr As New System.IO.StreamReader(fs)
                    Dim tabbedline() As String
                    tabbedline = Split(sr.ReadLine, vbTab)
                    Do Until tabbedline(0) = "Cat_Name"
                        If Not sr.EndOfStream Then
                            tabbedline = Split(sr.ReadLine, vbTab)
                        Else
                            Throw New Exception("This file did not contain the correct format for FDA Damage Categories")
                        End If
                    Loop
                    If Not sr.EndOfStream Then
                        tabbedline = Split(sr.ReadLine, vbTab)
                    Else
                        Throw New Exception("This file did not contain the correct format for FDA Damage Categories")
                    End If
                    Do Until tabbedline(0) = "Occ_Name" Or sr.EndOfStream
                        If Not sr.EndOfStream Then
                            _DamCats.Add(New DamageCategory(tabbedline(0), tabbedline(1), 365, tabbedline(2)))
                            tabbedline = Split(sr.ReadLine, vbTab)
                        Else
                            Throw New Exception("This file did not contain the correct format for FDA Damage Categories")
                        End If
                    Loop
                Case ".xml"
                    ReadFromXML(path)
            End Select
        End Sub
        Public ReadOnly Property GetDamageCategories As List(Of DamageCategory)
            Get
                Return _DamCats
            End Get
        End Property
        Public Function GetDamageCategoryByName(ByVal damcatname As String) As DamageCategory
            For i = 0 To _DamCats.Count - 1
                If _DamCats(i).Name = damcatname Then Return _DamCats(i)
            Next
            Return Nothing
        End Function
        Private Sub ReadFromXML(ByVal path As String)
            Dim ba As Byte() = System.IO.File.ReadAllBytes(path)
            Dim ms As New System.IO.MemoryStream(ba)
            Dim xdoc As New XDocument
            xdoc = XDocument.Load(ms)
            ms.Dispose()
            Dim xel As XElement = xdoc.Root
            For Each ele As XElement In xel.Elements
                _DamCats.Add(New DamageCategory(ele))
            Next
        End Sub
        Public Sub WriteToXML(ByVal outputpath As String)
            Dim doc As New XDocument
            Dim damcats As New XElement("DamageCategories")
            For i As Integer = 0 To _DamCats.Count - 1
                damcats.Add(_DamCats(i).writetoXMlElement)
            Next
            doc.Add(damcats)
            doc.Save(outputpath)
        End Sub
        Public Function WriteToFDAString() As String
            Dim s As New System.Text.StringBuilder
            s.AppendLine("Cat_Name" & vbTab & "Cat_Description" & vbTab & "Cost_Factor" & vbTab)
            For i As Integer = 0 To _DamCats.Count - 1
                If i = _DamCats.Count - 1 Then
                    s.Append(_DamCats(i).WriteToFDAString)
                Else
                    s.AppendLine(_DamCats(i).WriteToFDAString)
                End If

            Next
            Return s.ToString
        End Function
    End Class
End Namespace

