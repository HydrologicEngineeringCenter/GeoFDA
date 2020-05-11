Public Class AnalysisYears
    Implements System.ComponentModel.IDataErrorInfo
    Private _baseyear As String
    Private _Mlfyear As String

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Sub New(ByVal by As Integer, ByVal mlfy As Integer)
        BaseYear = by
        If mlfy = 0 Then
        Else
            MLFYear = mlfy
        End If

        InitializeComponent()
    End Sub
    Public Property BaseYear As String
        Get
            Return _baseyear
        End Get
        Set(value As String)
            _baseyear = value
        End Set
    End Property
    Public Property MLFYear As String
        Get
            Return _Mlfyear
        End Get
        Set(value As String)
            _Mlfyear = value
        End Set
    End Property
    Private Sub Cmdok_Click(sender As Object, e As System.Windows.RoutedEventArgs) Handles Cmdok.Click
        If TxtBaseYear.Text = "" Then MsgBox("You have not defined a base year") : Exit Sub
        'If TxtMLFYear.Text = "" Then MsgBox("You have not defined a most likely future year") : Exit Sub
        Dim BYint As Integer
        If Not Integer.TryParse(TxtBaseYear.Text, BYint) Then MsgBox("your base year cannot be converted to an integer, please try again") : Exit Sub
        Dim MLFYINT As Integer
        If TxtMLFYear.Text = "" Then
        Else

            If TxtMLFYear.Text <> "" AndAlso Not Integer.TryParse(TxtMLFYear.Text, MLFYINT) Then MsgBox("Your most likely future year can not be converted to an integer, please try again") : Exit Sub
        End If

        If BYint < 1900 Then MsgBox("Your base year cannot be less than 1900") : Exit Sub
        If TxtMLFYear.Text <> "" AndAlso BYint >= MLFYINT Then MsgBox("Your base year is equal to or greater than your most likely future year, please try again") : Exit Sub
        'according to bob, MLFYear is not necessary.
        DialogResult = True
        Me.Close()
    End Sub
    Private Sub CmdClose_Click(sender As Object, e As System.Windows.RoutedEventArgs) Handles CmdClose.Click
        DialogResult = False
        Me.Close()
    End Sub

    Public ReadOnly Property [Error] As String Implements ComponentModel.IDataErrorInfo.Error
        Get
            Throw New NotImplementedException
        End Get
    End Property

    Default Public ReadOnly Property Item(columnName As String) As String Implements ComponentModel.IDataErrorInfo.Item
        Get
            If columnName = "MLFYear" Then
                If TxtMLFYear.Text = "" Then Return Nothing
                Dim int As Integer
                If Not Integer.TryParse(TxtMLFYear.Text, int) Then
                    Return "Could Not Convert to Integer"
                End If

            End If
            If columnName = "BaseYear" Then
                If Not IsLoaded Then Return Nothing
                If TxtBaseYear.Text = "" Then Return "Base Year cannot be empty"
                Dim Bint As Integer
                If Not Integer.TryParse(TxtBaseYear.Text, Bint) Then
                    Return "Could not convert to integer"
                Else
                    If Bint < 1900 Then Return "Base Year must be after 1900"
                End If
            End If
            Return Nothing
        End Get
    End Property
End Class

