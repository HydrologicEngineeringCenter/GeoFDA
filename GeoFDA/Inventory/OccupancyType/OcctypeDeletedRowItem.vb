Public Class OcctypeDeletedRowItem
    Private _StructureInventoryName As String
    Public ReadOnly Property StructureInventoryName As String
        Get
            Return _StructureInventoryName
        End Get
    End Property
    Public Property NumberOfStructuresImpacted As Integer
    Sub New(ByVal structureinvname As String)
        _StructureInventoryName = structureinvname
    End Sub
End Class
