Imports System.Xml
Imports System.Windows

Public Class OTE
    Private _OccTypes As List(Of ComputableObjects.OccupancyType)
    Private _damcats As List(Of ComputableObjects.DamageCategory)
    Private _OutputFile As String
    Public Event OcctypeAdded(ByVal Occtype As ComputableObjects.OccupancyType)
    Public Event OcctypeRenamed(ByVal Occtype As ComputableObjects.OccupancyType, ByVal NewName As String)
    Public Event OcctypeDeleted(ByVal Occtype As ComputableObjects.OccupancyType, ByRef cancel As Boolean)
    Public Event DamcatAdded(ByVal damcat As ComputableObjects.DamageCategory)
    Public Event OcctypeDamcatReassigned(ByVal occtype As ComputableObjects.OccupancyType, ByVal NewDamcat As ComputableObjects.DamageCategory) ' structures will need to be updated to have proper assignments.
    Property UseVehicleDamages As Boolean
        Get
            Return DepthDamageEditor.DisplayVehicleParameters
        End Get
        Set(value As Boolean)
            DepthDamageEditor.DisplayVehicleParameters = value
        End Set
    End Property
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Public Sub New(ByVal OccupancyTypes As List(Of ComputableObjects.OccupancyType), ByVal damacats As List(Of ComputableObjects.DamageCategory), ByVal OutputFile As String)

        _OccTypes = New List(Of ComputableObjects.OccupancyType)
        _damcats = damacats
        _OutputFile = OutputFile
        For Each occtype As ComputableObjects.OccupancyType In OccupancyTypes
            _OccTypes.Add(occtype.Clone)
        Next
        InitializeComponent()
    End Sub

    Private Sub OccTypeEditor_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles Me.Closing
        'SINode.OccTypeEditorOpen = False
    End Sub

    Private Sub OccTypeEditor_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        DepthDamageEditor.NewOccTypeNameTextBox.Visibility = Windows.Visibility.Hidden
        DepthDamageEditor.OccTypeNameBox.Visibility = Windows.Visibility.Visible
        DepthDamageEditor.CreateNewButton.Visibility = Windows.Visibility.Visible
        DepthDamageEditor.OccTypeNameBox.SelectedIndex = 0
    End Sub

    Private Sub OccTypeEditor_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        DepthDamageEditor.LoadOccTypes(_OccTypes, _damcats)
        DepthDamageEditor.DefineSettings()
        AddHandler DepthDamageEditor.CreateNewButton.Click, AddressOf AddNew_Click
        AddHandler DepthDamageEditor.DamageCategoryAdded, AddressOf OnDamageCategoryAdded
        AddHandler DepthDamageEditor.DamageCategoryAssignmentChanged, AddressOf OnDamageCategoryAssignmentChanged
        AddHandler DepthDamageEditor.OcctypeAdded, AddressOf OnOcctypeAdded
    End Sub
    Private Sub OnDamageCategoryAssignmentChanged(ByVal occtype As ComputableObjects.OccupancyType, ByVal damcat As ComputableObjects.DamageCategory)
        RaiseEvent OcctypeDamcatReassigned(occtype, damcat)
    End Sub
    Private Sub OnDamageCategoryAdded(ByVal DamageCategory As ComputableObjects.DamageCategory)
        RaiseEvent DamcatAdded(DamageCategory)
    End Sub
    Private Sub OnOcctypeAdded(ByVal Occtype As ComputableObjects.OccupancyType)
        RaiseEvent OcctypeAdded(Occtype) 'if i reroute the ddec event, i will potentially raise the event twice.
    End Sub
    Private Sub CancelButton_Click(sender As Object, e As RoutedEventArgs)
        Me.Close() ' cancel does not work on delete.
    End Sub
    Private Sub OKButton_Click(sender As Object, e As RoutedEventArgs)
        If DepthDamageEditor.TestAndSetCurrentSelection = True Then
            Dim XML_OutSettings As XmlWriterSettings = New XmlWriterSettings
            XML_OutSettings.Indent = True
            Using writer As XmlWriter = XmlWriter.Create(_OutputFile, XML_OutSettings)
                writer.WriteStartDocument()
                writer.WriteStartElement("OccTypes") ' Root.
                For Each occtype As ComputableObjects.OccupancyType In DepthDamageEditor.OccTypes
                    If occtype.Name = "" Then
                    Else
                        occtype.WriteToXElement.WriteTo(writer)
                    End If

                Next
            End Using
            'RaiseEvent OccTypesUpdated()
            Me.Close()
        End If
    End Sub

    Private Sub AddNew_Click(sender As Object, e As RoutedEventArgs)
        Dim NameDialog As New NameAndCategory()
        NameDialog.Owner = Me
        Dim Result As Boolean = CBool(NameDialog.ShowDialog)
        If Result = True Then
            If NameDialog.NameTextBox.Text <> "" Then
                For Each OccType As ComputableObjects.OccupancyType In DepthDamageEditor.OccTypes
                    If NameDialog.NameTextBox.Text = OccType.Name Then
                        MsgBox("Name entered already exists.")
                        AddNew_Click(Nothing, Nothing)
                        Exit Sub
                    End If
                Next
                Dim NewOccType As New ComputableObjects.OccupancyType(NameDialog.NameTextBox.Text, NameDialog.DamCatTextBox.Text)
                RaiseEvent OcctypeAdded(NewOccType) ' will get raised in the next line by the DDEC also...
                DepthDamageEditor.AddOccType(NewOccType)
                DepthDamageEditor.OccTypeNameBox.SelectedIndex = DepthDamageEditor.OccTypeNameBox.Items.Count - 1
            Else
                MsgBox("No name was entered, new data was not added.")
            End If
        End If
    End Sub

    Private Sub Delete_Click(sender As Object, e As RoutedEventArgs)
        Dim selectedIndex As Int32 = DepthDamageEditor.OccTypeNameBox.SelectedIndex
        'Select Case MessageBox.Show("Are you sure you want to delete '" & DepthDamageEditor.OccTypeNameBox.SelectedValue.ToString & "'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)
        '    Case MessageBoxResult.Yes
        Dim cancel As Boolean = False
        RaiseEvent OcctypeDeleted(DepthDamageEditor.OccTypes(selectedIndex), cancel)
        If Not cancel Then
            DepthDamageEditor.OccTypes.RemoveAt(selectedIndex)
            DepthDamageEditor.OccTypeNameBox.Items.RemoveAt(selectedIndex)
            If selectedIndex > 0 Then selectedIndex -= 1 ' what if the list is now empty?
            DepthDamageEditor.OccTypeNameBox.SelectedIndex = selectedIndex
        End If


        '    Case Else
        '        '
        'End Select
    End Sub

    Private Sub Rename_Click(sender As Object, e As RoutedEventArgs)
        Dim NameDialog As New NameDialog()
        NameDialog.NameTextBox.Text = DepthDamageEditor.OccTypeNameBox.SelectedValue.ToString
        'NameDialog.DamCatTextBox.Text = DepthDamageEditor.OccTypes(DepthDamageEditor.OccTypeNameBox.SelectedIndex).DamageCategory.Name
        NameDialog.Title = "Rename Occupancy Type"
        NameDialog.Owner = Me
        Dim Result As Boolean = CBool(NameDialog.ShowDialog)
        If Result = True Then
            If NameDialog.NameTextBox.Text <> "" Then
                For Each OccType As ComputableObjects.OccupancyType In DepthDamageEditor.OccTypes
                    If NameDialog.NameTextBox.Text = OccType.Name Then
                        MsgBox("Name entered already exists.")
                        Rename_Click(Nothing, Nothing)
                        Exit Sub
                    End If
                Next
                RaiseEvent OcctypeRenamed(DepthDamageEditor.OccTypes(DepthDamageEditor.OccTypeNameBox.SelectedIndex), NameDialog.NameTextBox.Text) 'allow structures to rename occtypes.
                DepthDamageEditor.OccTypes(DepthDamageEditor.OccTypeNameBox.SelectedIndex).Name = NameDialog.NameTextBox.Text
                Dim SelectedIndex As Int32 = DepthDamageEditor.OccTypeNameBox.SelectedIndex
                DepthDamageEditor.OccTypeNameBox.Items(DepthDamageEditor.OccTypeNameBox.SelectedIndex) = NameDialog.NameTextBox.Text
                DepthDamageEditor.OccTypeNameBox.SelectedIndex = SelectedIndex
            Else
                MsgBox("No name was entered, data was not renamed.")
            End If
        End If
    End Sub

    Private Sub SaveAs_Click(sender As Object, e As RoutedEventArgs)
        Dim NameDialog As New NameDialog()
        NameDialog.NameTextBox.Text = DepthDamageEditor.OccTypeNameBox.SelectedValue.ToString
        NameDialog.Title = "Save Occupancy Type As"
        NameDialog.Owner = Me
        Dim Result As Boolean = CBool(NameDialog.ShowDialog)
        If Result = True Then
            If NameDialog.NameTextBox.Text <> "" Then
                For Each OccType As ComputableObjects.OccupancyType In DepthDamageEditor.OccTypes
                    If NameDialog.NameTextBox.Text = OccType.Name Then
                        MsgBox("Name entered already exists.")
                        Rename_Click(Nothing, Nothing)
                        Exit Sub
                    End If
                Next
                Dim NewOccType As ComputableObjects.OccupancyType = DepthDamageEditor.OccTypes(DepthDamageEditor.OccTypeNameBox.SelectedIndex).Clone
                NewOccType.Name = NameDialog.NameTextBox.Text
                RaiseEvent OcctypeAdded(NewOccType) 'will get raised by the DDEC also
                DepthDamageEditor.AddOccType(NewOccType)
                DepthDamageEditor.OccTypeNameBox.SelectedItem = DepthDamageEditor.OccTypes(DepthDamageEditor.OccTypes.Count - 1)

            Else
                MsgBox("No name was entered, data was not saved.")
            End If
        End If
    End Sub
End Class
