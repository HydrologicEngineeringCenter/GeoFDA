Public Class EconInfo
    Private _DamCats As List(Of DamageCategory)
	Private _Occtypes As List(Of ComputableObjects.OccupancyType)
	Private _Structures As StructureInventory
    Private _FromFile As Boolean = False
    Private _String As String
	Sub New(ByVal Damcats As List(Of DamageCategory), ByVal Occtypes As List(Of ComputableObjects.OccupancyType), ByVal structures As StructureInventory)
		_DamCats = Damcats
		_Occtypes = Occtypes
		_Structures = structures
	End Sub
	Sub New(ByVal Filepath As String)
        _FromFile = True
        Dim fs As New System.IO.FileStream(Filepath, IO.FileMode.Open, IO.FileAccess.Read)
        Dim sr As New System.IO.StreamReader(fs)
        _String = sr.ReadToEnd
        sr.Dispose() : sr.Close()
        fs.Dispose() : fs.Close()
        ''read the DamCat stuff first.
        '_DamCats = New List(Of DamageCategory)
        'Dim line() As String = Split(sr.ReadLine, vbTab)
        'If line(0) = "Cat_Name" Then
        '    Do Until line(0) = "Occ_Name"
        '        line = Split(sr.ReadLine, vbTab)
        '        _DamCats.Add(New DamageCategory(line(0), line(1), line(2)))
        '    Loop
        'Else
        '    Throw New Exception("First line was not a header for a damage category")
        'End If
        'Dim currentOccType As String = ""
        'Dim occtypeStringBuilder As System.Text.StringBuilder
        '_Occtypes = New List(Of OccupancyType)
        'If line(0) = "Occ_Name" Then
        '    Dim startdata As Integer = Array.IndexOf(line, "Start_Data")
        '    line = Split(sr.ReadLine, vbTab)
        '    Do Until sr.EndOfStream
        '        currentOccType = line(0)
        '        Do Until line(0) <> currentOccType
        '            occtypeStringBuilder = New System.Text.StringBuilder
        '            For i = 0 To line.Count - 1
        '                occtypeStringBuilder.Append(line(i) & vbTab)
        '            Next
        '            occtypeStringBuilder.Append(vbNewLine)
        '            _Occtypes.Add(New OccupancyType(occtypeStringBuilder, startdata, startdata - 1))
        '            line = Split(sr.ReadLine, vbTab)
        '        Loop
        '    Loop

        'End If
    End Sub
    Public Overrides Function ToString() As String
        If _FromFile Then Return _String
        Dim str As New System.Text.StringBuilder
        str.AppendLine("Cat_Name" & vbTab & "Cat_Description" & vbTab & "Cost_Factor" & vbTab)
        For i = 0 To _DamCats.Count - 1
            str.AppendLine(_DamCats(i).ToString)
        Next
        str.AppendLine("Occ_Name" & vbTab & "Occ_Description" & vbTab & "Cat_Name" & vbTab & "Parameter" & vbTab & "Start_Data" & vbTab)
        For i = 0 To _Occtypes.Count - 1
            str.AppendLine(_Occtypes.ToString)
        Next
        'add the strucures 
        str.AppendLine(_Structures.ToString)
        Return str.ToString
    End Function
End Class
