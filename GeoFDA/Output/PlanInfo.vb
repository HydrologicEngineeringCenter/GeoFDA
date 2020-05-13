Public Class PlanInfo
    Private _PlanName As String
    Private _PlandDescription As String
    Sub New(ByVal PlanName As String, ByVal PlanDescription As String)
        _PlandDescription = PlanDescription
        _PlanName = PlanName
    End Sub
    Public ReadOnly Property GetPlanName As String
        Get
            Return _PlanName
        End Get
    End Property
    Public ReadOnly Property GetPlanDesc As String
        Get
            Return _PlandDescription
        End Get
    End Property
    Public Overrides Function ToString() As String
        Return _PlanName & vbTab & _PlandDescription
    End Function
End Class
