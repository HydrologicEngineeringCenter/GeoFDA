Public Class DamageCategory
    Private _Cat_Name As String
    Private _Cat_Description As String
    Private _Cost_factor As Double
    'Private _occupancies As List(Of OccupancyType)
    Sub New(ByVal CatName As String, Optional ByVal CatDesc As String = " ", Optional ByVal costfactor As Double = 1)
        _Cat_Name = CatName
        _Cat_Description = CatDesc
        _Cost_factor = costfactor
    End Sub

    Public ReadOnly Property GetName As String
        Get
            Return _Cat_Name
        End Get
    End Property
    Public Overrides Function ToString() As String
        Return _Cat_Name & vbTab & _Cat_Description & vbTab & _Cost_factor & vbTab 'why the extra vb tab?
    End Function
End Class
