Public Class NameClass
    Private _Name As String
    Private _Index As Double
    Sub New(ByVal namestring As String, ByVal index As Double)
        Name = namestring
    End Sub
    Public Property Name As String
        Get
            Return _Name
        End Get
        Set(value As String)
            _Name = value
        End Set
    End Property
    Public Property Index_Station As Double
        Get
            Return _Index
        End Get
        Set(value As Double)
            _Index = value
        End Set
    End Property
    Public Function Validate() As List(Of ImpactAreaError)
        Dim errors As New List(Of ImpactAreaError)
        If _Name.Length > 32 Then errors.Add(New ImpactAreaError("Name must be less than or equal to 32 caracters", 0, 0))
        If _Index = 0 Then errors.Add(New ImpactAreaError("Index station cannot be zero", 0, 1))
        If _Index < 0 Then errors.Add(New ImpactAreaError("Index station cannot be negative", 0, 1))
        If _Index = CInt(_Index) Then errors.Add(New ImpactAreaError("Index station cannot be an integer number", 0, 1))
        Return errors
    End Function
End Class
