Imports System.ComponentModel

Public Class IndexLocations
    Implements System.ComponentModel.INotifyPropertyChanged
    Implements System.ComponentModel.IDataErrorInfo
    Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Private _directory As String
    Private _index As XMLIndexLocation
    Private _stations As Double()
    Public Event ChangesHaveOccured()
    Private _IsSelectionProgrammatic As Boolean = False
    Sub New(ByVal impactareas As String(), ByVal stations As Double(), ByVal outputdirectory As String)
        InitializeComponent()
        _directory = outputdirectory
        ReachName.ItemsSource = impactareas
        _stations = stations
    End Sub
    Public Property IndexLocation As XMLIndexLocation
        Get
            Return _index
        End Get
        Set(value As XMLIndexLocation)
            _index = value
            NotifyPropertyChanged("IndexLocation")
        End Set
    End Property

    Default Public ReadOnly Property Item(columnName As String) As String Implements IDataErrorInfo.Item
        Get
            If columnName = "Invert" Then
                If Not IsNumeric(TxtInvert.Text) Then Return "Invert must be numerical"
            End If
            Return ""
        End Get
    End Property

    Public ReadOnly Property [Error] As String Implements IDataErrorInfo.Error
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Private Sub Close_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles CMDClose.Click
        Me.Close()
    End Sub
    Private Sub CMDOk_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles CMDOk.Click

        If Not IsNothing(_index) Then
            Dim msg As String = _index.Validate
            If Not IsNumeric(TxtInvert.Text) Then msg &= vbNewLine & "The Invert must be numeric"
            If msg = "" Then
                If _index.HasChanges Then RaiseEvent ChangesHaveOccured()
                _index.WriteToXMLfile(_directory)
            Else
                MsgBox("For the reach named " & _index.ReachName & " the following errors exist" & vbNewLine & msg)
                Exit Sub
            End If

        End If
        DialogResult = True
        Me.Close()
    End Sub

    Private Sub ReachName_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ReachName.SelectionChanged
        If IsLoaded Then
            If _IsSelectionProgrammatic Then
            Else
                If ReachName.SelectedIndex = -1 Then Exit Sub
                Dim idx As Integer = 0
                If e.RemovedItems.Count > 0 Then
                    idx = ReachName.Items.IndexOf(e.RemovedItems(0))
                    '_stations(idx) = _index.Stationing ''if this changes. make sure to update the impact area shapefile.\
                    If _index.HasChanges Then RaiseEvent ChangesHaveOccured()
                    Dim msg As String = _index.Validate
                    If Not IsNumeric(TxtInvert.Text) Then msg &= vbNewLine & "The Invert must be numeric"
                    If msg = "" Then
                        _index.WriteToXMLfile(_directory)
                    Else
                        MsgBox("For the reach named " & _index.ReachName & " the following errors exist" & vbNewLine & msg)
                        _IsSelectionProgrammatic = True
                        ReachName.SelectedItem = e.RemovedItems(0)
                        Exit Sub
                    End If

                End If
                Dim i As New XMLIndexLocation
                i.ReadFromXml(_directory & "\" & ReachName.SelectedItem & ".xml")
                If i.Stationing = _stations(ReachName.SelectedIndex) Then
                Else
                    If _stations(ReachName.SelectedIndex) = 0 Then
                        '_stations(ReachName.SelectedIndex) = i.Stationing
                    Else
                        i.Stationing = _stations(ReachName.SelectedIndex)
                    End If
                End If
                IndexLocation = i
            End If

        Else
            'do nothing
        End If
        _IsSelectionProgrammatic = False
    End Sub
    Private Sub TextBox_TextChanged(sender As System.Object, e As System.Windows.Controls.TextChangedEventArgs)
        Dim TBox As TextBox = CType(sender, TextBox)
        Dim SingleValue As Single
        If Single.TryParse(TBox.Text, SingleValue) = False Then
            If TBox.Text = "-" Or TBox.Text = "-." Or TBox.Text = "." Then Exit Sub
        End If
    End Sub
    Private Sub NotifyPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(info))
    End Sub

End Class
