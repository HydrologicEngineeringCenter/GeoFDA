Public Class NameVM
    Private _items As List(Of NameClass)
    Public Property Items As List(Of NameClass)
        Get
            Return _items
        End Get
        Set(value As List(Of NameClass))
            _items = value
        End Set
    End Property
    Public Function GetImpactAreaNames() As List(Of String)
        Dim ret As New List(Of String)
        For i = 0 To _items.Count - 1
            ret.Add(_items(i).Name)
        Next
        Return ret
    End Function
    Public Function GetPaddedImpactAreaNames() As List(Of String)
        Dim ret As New List(Of String)
        For i = 0 To _items.Count - 1
            ret.Add(_items(i).Name.PadRight(32))
        Next
        Return ret
    End Function
    Public Function GetImpactAreaIndexes() As List(Of Double)
        Dim ret As New List(Of Double)
        For i = 0 To _items.Count - 1
            ret.Add(_items(i).Index_Station)
        Next
        Return ret
    End Function
    Sub New()
        _items = New List(Of NameClass)
    End Sub
    Sub New(ByVal list As List(Of String))
        Dim tmpitems As New List(Of NameClass)
        For i = 0 To list.Count - 1
            tmpitems.Add(New NameClass(list(i), 0))
        Next
        Items = tmpitems
    End Sub
    Public Function Validate() As List(Of ImpactAreaError)
        Dim report As New List(Of ImpactAreaError)
        Dim rowreport As New List(Of ImpactAreaError)
        For i = 0 To _items.Count - 1
            rowreport = _items(i).Validate
            If IsNothing(rowreport) OrElse rowreport.Count = 0 Then
                'do nothing
            Else
                For j = 0 To rowreport.Count - 1
                    rowreport(j).Row = i
                    report.Add(rowreport(j))
                Next
            End If
        Next
        Return report
    End Function
End Class
