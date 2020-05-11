Public Class SelectablePlan
    Private _Selected As Boolean = False
    Private _Plan As HydraulicsChildTreenode
    'Private _PlanName As String
    Sub New(ByVal theplan As HydraulicsChildTreenode)
        _Plan = theplan
        '_PlanName = _Plan.Header
    End Sub
    Public Property Selected As Boolean
        Get
            Return _Selected
        End Get
        Set(value As Boolean)
            _Selected = value
        End Set
    End Property
    Public ReadOnly Property PlanName As String
        Get
            Return _Plan.Header
        End Get
    End Property
    Public ReadOnly Property PlanYear As Integer
        Get
            Return _Plan.GetYear
        End Get
    End Property
    Public ReadOnly Property GetPlan As HydraulicsChildTreenode
        Get
            Return _Plan
        End Get
    End Property
End Class
