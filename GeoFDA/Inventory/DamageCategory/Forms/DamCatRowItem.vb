Public Class DamCatRowItem
    Private _Name As String
    Private _Description As String
    Private _indexFactor As String
	Public Event DamageCategoryRenamed(ByVal damagecategory As ComputableObjects.DamageCategory, ByVal newname As String)
	Public Property Name As String
        Get
            Return _Name
        End Get
        Set(value As String)
            If value = _Name Then
            Else
                If IsNothing(_Name) Then
                Else
                    RaiseEvent DamageCategoryRenamed(ToDamCat, value)
                End If
                _Name = value
            End If
        End Set
    End Property
    Public Property Description As String
        Get
            Return _Description
        End Get
        Set(value As String)
            _Description = value
        End Set
    End Property
    Public Property Index_Factor As String
        Get
            Return _indexFactor
        End Get
        Set(value As String)
            Dim int As Double
            If Double.TryParse(value, int) Then _indexFactor = value
        End Set
    End Property
    Sub New(ByVal Damcatname As String, ByVal desc As String, ByVal indexfactor As Double)
        _Name = Damcatname
        _Description = desc
        _indexFactor = indexfactor
    End Sub
    Sub New()
        _indexFactor = 1
    End Sub
	Public Function ToDamCat() As ComputableObjects.DamageCategory
		Return New ComputableObjects.DamageCategory(Name, Description, 365, _indexFactor)
	End Function
	Public Function Validate() As String
        If IsNothing(_Name) OrElse _Name = "" Then Return "Damage Category Name cannot be empty"
        If _Name.Length >= 32 Then Return "Damage Category Name must be less than or equal to 32 characters"
        Return ""
    End Function
End Class
