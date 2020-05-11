Public Class FilePathAndName
    Implements System.ComponentModel.INotifyPropertyChanged
    Implements System.ComponentModel.IDataErrorInfo
    Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Private _FilePath As String
    Private _FileName As String
    Private _Probability As String
    Private _Valid As Boolean = False
    Dim counter As Integer = 0
    Sub New(ByVal path As String, ByVal prob As String)
        FilePath = path
        FileName = System.IO.Path.GetFileNameWithoutExtension(_FilePath)
        Probability = prob
    End Sub
    Sub New(ByVal path As String, ByVal fpart As String, ByVal prob As Single)
        FilePath = path
        FileName = fpart
        Probability = prob
    End Sub
    Public Property FilePath As String
        Get
            Return _FilePath
        End Get
        Set(value As String)
            _FilePath = value
            NotifyPropertyChanged("FilePath")
        End Set
    End Property
    Public ReadOnly Property IsValid As Boolean
        Get
            Return _Valid
        End Get
    End Property
    Public Property FileName As String
        Get
            Return _FileName
        End Get
        Set(value As String)
            _FileName = value
            NotifyPropertyChanged("FileName")
            NotifyPropertyChanged("ProbAndFileName")
        End Set
    End Property
    Public Property Probability As String
        Get
            Return CStr(_Probability)
        End Get
        Set(value As String)
            _Probability = value
                NotifyPropertyChanged("[Error]")
                NotifyPropertyChanged("Probability")
                NotifyPropertyChanged("ProbAndFileName")
                NotifyPropertyChanged("GetProbabilityAsChance")
            NotifyPropertyChanged("GetProbabilityAsReturnInterval")
        End Set
    End Property
    Public ReadOnly Property GetProbabilityAsReturnInterval As String
        Get
            Dim sng As Single
            If Single.TryParse(_Probability, sng) Then
                Return CSng(1 / sng).ToString("F0") & " Year"
            Else
                Return "Invalid Probability"
            End If

        End Get
    End Property
    Public ReadOnly Property GetProbabilityAsChance As String
        Get
            Dim sng As Single
            If Single.TryParse(_Probability, sng) Then
                Return "1 in " & CSng(1 / sng).ToString("F0")
            Else
                Return "Invalid Probability"
            End If
        End Get
    End Property
    Private ReadOnly Property GetProbabilityAsACE As String
        Get
            Return _Probability & " ACE"
        End Get
    End Property
    Public Property ProbAndFileName As String
        Get
            Return GetProbabilityAsACE & " , " & _FileName
        End Get
        Set(value As String)
            _FileName = value
            NotifyPropertyChanged("ProbAndFileName")
        End Set
    End Property
    Private Function ValidateProbability(value As String) As String
        counter += 1
        Dim SingleValue As Single
        _Valid = False
        If Single.TryParse(value, SingleValue) = False Then
            If value = "." Then Return ""
            Return "the value " & value & " is not a valid probability"
        Else
            If SingleValue >= 0 AndAlso SingleValue <= 1 Then
                _Valid = True
                Return ""
            Else
                Return "the value " & value & " is less than zero or greater than 1"
            End If
        End If
    End Function
    Private Sub NotifyPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(info))
    End Sub

    Public ReadOnly Property [Error] As String Implements ComponentModel.IDataErrorInfo.Error
        Get
            Dim errors As New System.Text.StringBuilder
            errors.Append(Item("Probability"))
            If errors.ToString = "" Then Return Nothing
            Return errors.ToString
        End Get
    End Property

    Default Public ReadOnly Property Item(columnName As String) As String Implements ComponentModel.IDataErrorInfo.Item
        Get
            If columnName = "Probability" Then
                Return ValidateProbability(_Probability)
            End If
            Return ""
        End Get
    End Property
End Class
