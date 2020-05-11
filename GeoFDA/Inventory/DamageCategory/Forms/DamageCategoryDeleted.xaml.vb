Public Class DamageCategoryDeleted
    Private _Message As String
    Private _RemainingDamageCategories As List(Of Consequences_Assist.ComputableObjects.DamageCategory)
    Public ReadOnly Property Message As String
        Get
            Return _Message
        End Get
    End Property
    Public Property RemainingDamageCategories As List(Of Consequences_Assist.ComputableObjects.DamageCategory)
        Get
            Return _RemainingDamageCategories
        End Get
        Set(value As List(Of Consequences_Assist.ComputableObjects.DamageCategory))
            _RemainingDamageCategories = value
        End Set
    End Property
    Sub New(ByVal msg As String, ByVal remaindamcats As List(Of Consequences_Assist.ComputableObjects.DamageCategory))
        _Message = msg
        RemainingDamageCategories = remaindamcats
        InitializeComponent()
        If CmbRemainingDamCats.HasItems Then CmbRemainingDamCats.SelectedIndex = 0
    End Sub
    Private Sub CMDOk_Click(sender As Object, e As RoutedEventArgs) Handles CMDOk.Click
        'ok
        If CmbRemainingDamCats.SelectedIndex = -1 Then MsgBox("you have not made a selection for a damage category to replace the one being deleted.")
        DialogResult = True
    End Sub

    Private Sub CMDCancel_Click(sender As Object, e As RoutedEventArgs) Handles CMDCancel.Click
        'not ok.
        DialogResult = False
    End Sub
End Class
