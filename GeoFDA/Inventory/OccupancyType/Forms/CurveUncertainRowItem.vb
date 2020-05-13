Namespace AutoGenerate
    Public MustInherit Class CurveUncertainRowItem
        Inherits CurveRowItem
        Public Property GetDistribution As Statistics.ContinuousDistribution
            Get
                Return _dist
            End Get
            Set(value As Statistics.ContinuousDistribution)
                _dist = value
            End Set
        End Property
        Public Sub New()
            MyBase.New()
        End Sub
        Public Overrides Function GetYValue() As Single
            Return _dist.GetCentralTendency
        End Function
        Public Overridable Function GetMaxValue() As Single
            Return _dist.getDistributedVariable(1)
        End Function
        Public Overridable Function GetMinValue() As Single
            Return _dist.getDistributedVariable(0)
        End Function
        Public Overridable Function Get95CI() As Single
            Return _dist.getDistributedVariable(1)
        End Function
        Public Overridable Function Get05CI() As Single
            Return _dist.getDistributedVariable(0)
        End Function
        Protected _dist As Statistics.ContinuousDistribution
    End Class
End Namespace

