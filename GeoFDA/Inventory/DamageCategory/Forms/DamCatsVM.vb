Public Class DamCatsVM
    Private _items As System.Collections.ObjectModel.ObservableCollection(Of Object)
    Public Property CanValidate As Boolean = True
    Public Event DamageCategoryDeleted(ByVal damagecategories As List(Of Consequences_Assist.ComputableObjects.DamageCategory), ByRef cancel As Boolean)
    Public Event DamageCategoryRenamed(ByVal damagecategory As Consequences_Assist.ComputableObjects.DamageCategory, ByVal newname As String)
    Public Property Items As System.Collections.ObjectModel.ObservableCollection(Of Object)
        Get
            Return _items
        End Get
        Set(value As System.Collections.ObjectModel.ObservableCollection(Of Object))
            _items = value
        End Set
    End Property
    Sub New()
        _items = New System.Collections.ObjectModel.ObservableCollection(Of Object)
    End Sub
    Sub New(ByVal names As List(Of String), ByVal descriptions As List(Of String), ByVal indexfactors As List(Of Integer))
        Dim lst As New List(Of DamCatRowItem)
        For i = 0 To names.Count - 1
            Dim item As New DamCatRowItem(names(i), descriptions(i), indexfactors(i))
            AddHandler item.DamageCategoryRenamed, AddressOf OnDamageCategoryRenamed
            lst.Add(item)
        Next
    End Sub
    Sub RemoveRow(ByVal item As DamCatRowItem, ByRef cancel As Boolean)
        RaiseEvent DamageCategoryDeleted({item.ToDamCat}.ToList, cancel)
        If Not cancel Then _items.Remove(item)

    End Sub
    Sub Add(ByVal item As DamCatRowItem, Optional ByVal index As Integer = Nothing)
        AddHandler item.DamageCategoryRenamed, AddressOf OnDamageCategoryRenamed
        If IsNothing(index) Then
            _items.Add(item)
        Else
            _items.Insert(index, item)
        End If
    End Sub
    Private Sub OnDamageCategoryRenamed(ByVal damcat As Consequences_Assist.ComputableObjects.DamageCategory, ByVal newname As String)
        RaiseEvent DamageCategoryRenamed(damcat, newname)
    End Sub
    Public Function validate() As List(Of DamCatError)
        If Not CanValidate Then Return Nothing
        Dim report As New List(Of DamCatError)
        Dim msg As String = ""
        For i = 0 To _items.Count - 1
            msg = _items(i).Validate
            If IsNothing(msg) OrElse msg = "" Then
                'do nothing
            Else
                report.Add(New DamCatError(i, msg))
                msg = ""
            End If
        Next
        Return report
    End Function
End Class
