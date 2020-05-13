Namespace ComputableObjects
    Public MustInherit Class Consequences_Structure
        Protected _Name As String
        Protected _DamageCategory As DamageCategory
        Protected _OccupancyType As OccupancyType
        Protected _FH As Double
        Protected _GroundEle As Double
        Protected _location As LifeSimGIS.PointD
        Sub New(ByVal name As String, ByVal damagecategory As DamageCategory, ByVal occtype As OccupancyType, ByVal foundationheight As Double, ByVal groundelevation As Double, ByVal loc As LifeSimGIS.PointD)
            _Name = name
            _DamageCategory = damagecategory
            _OccupancyType = occtype
            _FH = foundationheight
            _GroundEle = groundelevation
            _location = loc
        End Sub
        Public Property Occtype() As OccupancyType
            Get
                Return _OccupancyType
            End Get
            Set(value As OccupancyType)
                _OccupancyType = value
            End Set
        End Property
        Public Property DamageCategory() As DamageCategory
            Get
                Return _DamageCategory
            End Get
            Set(value As DamageCategory)
                _DamageCategory = value
            End Set
        End Property
        Public Property Name As String
            Get
                Return _Name
            End Get
            Set(value As String)
                _Name = value
            End Set
        End Property
        Public Property FH As Double
            Get
                Return _FH
            End Get
            Set(value As Double)
                _FH = value
            End Set
        End Property
        Public Property GroundEle As Double
            Get
                Return _GroundEle
            End Get
            Set(value As Double)
                _GroundEle = value
            End Set
        End Property
        Public Property Northing As Double
            Get
                Return _location.Y
            End Get
            Set(value As Double)
                _location.Y = value
            End Set
        End Property
        Public Property Easting As Double
            Get
                Return _location.X
            End Get
            Set(value As Double)
                _location.X = value
            End Set
        End Property
        Public Property Location As LifeSimGIS.PointD
            Get
                Return _location
            End Get
            Set(value As LifeSimGIS.PointD)
                _location = value
            End Set
        End Property
        Public Overridable Function WriteToFDAString() As String
            Return Nothing
        End Function
    End Class
End Namespace