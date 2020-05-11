Public Class ZipWizard
    Implements System.ComponentModel.INotifyPropertyChanged
    Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    'Private _VM As ZipWizardVM
    Private _basedirectory As String
    Private _outputdest As String
    Private _maptreeview As OpenGLMapping.MapTreeView
    Private _projectFile As String
    Private _PropertiesItems As List(Of ZipWizardRowItems)
    Private _MapItems As List(Of ZipWizardRowItems)
    Private wasunsucsessful As Boolean = False
    'Private WithEvents _BW As System.ComponentModel.BackgroundWorker
    'Public Property VM As ZipWizardVM
    '    Get
    '        Return _VM
    '    End Get
    '    Set(value As ZipWizardVM)
    '        _VM = value
    '        NotifyPropertyChanged("VM")
    '    End Set
    'End Property
    Public ReadOnly Property PropertyItems
        Get
            Return _PropertiesItems
        End Get
    End Property
    Public ReadOnly Property MapItems
        Get
            Return _MapItems
        End Get
    End Property
    Sub New(ByVal projectfile As String, ByVal maptreeview As OpenGLMapping.MapTreeView, ByVal plan As OutputChildTreeNode, ByVal basedirectory As String, ByVal outputdest As String)

        ' This call is required by the designer.

        _outputdest = outputdest
        _projectFile = projectfile
        ' Add any initialization after the InitializeComponent() call.
        _basedirectory = basedirectory
        _PropertiesItems = plan.GetPathsAndFileTypes
        _maptreeview = maptreeview
        'read all of the files in the project path?
        _MapItems = New List(Of ZipWizardRowItems)
        For Each fn As OpenGLMapping.FeatureNode In maptreeview.MapWindow.GISFeatures
            If (Not fn.Features.Features Is Nothing) Then
                _MapItems.Add(New GeoFDA.ZipWizardRowItems(fn.Features.Features.GetSource, fn.Features.GetType.Name.ToString, basedirectory))
            Else
                _MapItems.Add(New ZipWizardRowItems("", fn.Features.GetType.Name.ToString, basedirectory))
            End If

        Next
        InitializeComponent()
    End Sub
    Private Sub AutoGeneratingColumn(sender As Object, e As DataGridAutoGeneratingColumnEventArgs)

        If CType(sender, DataGrid).Name = "PlanPropertiesDGV" AndAlso e.PropertyName = "Include_In_ZipFile" Then e.Cancel = True
        If e.PropertyType = GetType(String) Then
            If e.PropertyName = "File_Path" Then e.Cancel = True
            e.Column.Width = New DataGridLength(1, DataGridLengthUnitType.Star)
            e.Column.Header = e.PropertyName.Replace("_", " ")
            If e.PropertyName = "File_Size" Then e.Column.Header = e.Column.Header & " (mb)"
        ElseIf e.PropertyType = GetType(Boolean) Then
            e.Column.Width = New DataGridLength(1, DataGridLengthUnitType.Star)
            e.Column.Header = e.PropertyName.Replace("_", " ")
        Else
            e.Cancel = True
        End If
    End Sub
    Private Sub NotifyPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(info))
    End Sub
    'Private Sub ReportProgress(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs)
    '    Progress.Value = e.ProgressPercentage
    'End Sub
    'Private Sub UpdateLabel(ByVal filename As String)
    '    ' = filename
    'End Sub
    'Private Sub ProgressComplete(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs)
    '    If e.Result = True Then
    '        Me.Close()
    '    Else
    '        MsgBox("Failure")
    '    End If
    'End Sub
    Private Sub Zipper_Click(sender As Object, e As RoutedEventArgs) Handles Zipper.Click
        'Progress.Value = 25
        MyBase.Cursor = Cursors.Wait
        ZipFiles()
        MyBase.Cursor = Nothing
        DialogResult = Not wasunsucsessful
        If wasunsucsessful Then
            MsgBox("Errors Occured")
        Else
            Me.Close()
        End If

        '_BW = New System.ComponentModel.BackgroundWorker
        '_BW.WorkerReportsProgress = True
        'AddHandler _BW.ProgressChanged, AddressOf ReportProgress
        'AddHandler _BW.DoWork, AddressOf _VM.ZipFiles
        'AddHandler _BW.RunWorkerCompleted, AddressOf ProgressComplete
        'AddHandler _VM.FileBeingZipped, AddressOf UpdateLabel
        '_BW.RunWorkerAsync(_outputdest)
        'If _VM.Zip(_outputdest, _BW) Then
        '    Me.Close()
        'Else
        '    MsgBox("Failure")
        'End If
    End Sub
    Public Sub ZipFiles() 'ByVal outputdest As String, worker As System.ComponentModel.BackgroundWorker) As Boolean

        'Dim outputdest As String = e.Argument.ToString
        If System.IO.File.Exists(_outputdest) Then
            Dim result As System.Windows.Forms.DialogResult
            result = MessageBox.Show("A zip file already exists for this FDA import file. Do you want to delete the old one and proceed?", "Zip File Already Exists", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes)
            If result = System.Windows.Forms.DialogResult.Yes Then
                System.IO.File.Delete(_outputdest)
            Else
                Exit Sub
            End If
        End If
        '_bw = CType(sender, System.ComponentModel.BackgroundWorker)
        Dim z As New Ionic.Zip.ZipFile(_outputdest)
        'AddHandler z.SaveProgress, AddressOf SaveProgress
        Dim containsMapdir As Boolean = False
        Dim properties As Boolean = False
        Dim totMbytes As Double = 0

        Try
            'add the project file
            z.AddFile(_projectFile, "")
            'load selected map files
            Dim count As Integer = 0
            Dim mapitemlocation As Integer = 0
            For i = 0 To _MapItems.Count - 1
                If _MapItems(i).Include_In_ZipFile Then

                    count += 1
                    If Not _MapItems(i).Is_In_Project_Directory Then mapitemlocation += 1
                End If

            Next
            Dim thefnlist(count - 1) As Integer
            Dim oringinalsources(mapitemlocation - 1) As SourceandIndex
            mapitemlocation = 0
            Dim milocindex As Integer = 0
            count = 0
            For i = 0 To _MapItems.Count - 1
                If _MapItems(i).Include_In_ZipFile Then 'if they dont include it, shouldnt i modify the xml file?
                    If _MapItems(i).Is_In_Project_Directory Then
                        For j = 0 To _MapItems(i).File_Paths.Count - 1
                            If (_MapItems(i).File_Paths(j) <> "") Then
                                z.AddFile(_MapItems(i).File_Paths(j), System.IO.Path.GetDirectoryName(_MapItems(i).File_Paths(j)).Replace(_basedirectory, ""))
                                totMbytes += CDbl(_MapItems(i).File_Size) 'in mb
                                'If j = 0 Then
                            End If
                            For Each fn As OpenGLMapping.FeatureNode In _maptreeview.MapWindow.GISFeatures
                                If fn.Features.Features Is Nothing Then
                                    If _MapItems(i).File_Paths(j) = "" Then
                                        thefnlist(count) = i
                                        mapitemlocation += 1
                                        count += 1
                                    End If
                                    Continue For
                                End If
                                If fn.Features.Features.GetSource = _MapItems(i).File_Paths(j) Then
                                    thefnlist(count) = i ''is this a deep copy? if not i may be changing the source for the items in the map tree..
                                    count += 1
                                End If
                            Next
                            'End If
                        Next
                    Else
                        If Not containsMapdir Then z.AddDirectoryByName("Maps") : containsMapdir = True
                        For j = 0 To _MapItems(i).File_Paths.Count - 1
                            If (_MapItems(i).File_Paths(j) <> "") Then
                                z.AddFile(_MapItems(i).File_Paths(j), "Maps")
                                totMbytes += CDbl(_MapItems(i).File_Size) 'in mb
                                'change the source file in the maptree
                                'If j = 0 Then

                            End If
                            mapitemlocation = 0
                            For Each fn As OpenGLMapping.FeatureNode In _maptreeview.MapWindow.GISFeatures
                                If fn.Features.Features Is Nothing Then
                                    If _MapItems(i).File_Paths(j) = "" Then
                                        thefnlist(count) = i
                                        mapitemlocation += 1
                                        count += 1
                                    End If
                                    Continue For
                                End If
                                If fn.Features.Features.GetSource = _MapItems(i).File_Paths(j) Then
                                    'only works if there is only one map at this location and name.. isnt that ok?
                                    oringinalsources(milocindex) = New SourceandIndex(mapitemlocation, fn.Features.Features.GetSource)
                                    thefnlist(count) = i ''is this a deep copy? if not i may be changing the source for the items in the map tree..
                                    fn.Features.Features.SetSource = ".\Maps\" & _MapItems(i).File_Name
                                    count += 1
                                End If
                                mapitemlocation += 1
                            Next
                            'End If
                        Next
                        milocindex += 1
                    End If
                Else
                    'dont remove it, use the fnlist to generate the specific map document.
                    ''remove it from the maptree. in order for this to work properly, the map tree needs to be cloneable.

                    'For Each fn As OpenGLMapping.FeatureNode In _maptreeview.MapWindow.GISFeatures
                    '    If fn.Features.Features.GetSource = _MapItems(i).File_Paths(0) Then
                    '        'only works if there is only one map at this location and name.. isnt that ok?
                    '        _maptreeview.Items.Remove(fn) : Exit For
                    '    End If
                    'Next
                End If
            Next
            'write out the mapxml files
            Dim mapdoc As New XDocument()
            mapdoc.Add(SpecialMapXMLforZipFile(thefnlist.ToList, _basedirectory)) ''because of the way feature nodes write. i cannot call write to xml via a background worker.
            For i = 0 To oringinalsources.Count - 1
                _maptreeview.MapWindow.GISFeatures(oringinalsources(i).Index).Features.Features.SetSource = oringinalsources(i).source
            Next

            Dim tmppath As String = System.IO.Path.GetTempPath & System.IO.Path.GetFileNameWithoutExtension(_projectFile) & "MapProperties.xml"
            'bw.ReportProgress(50)
            If System.IO.File.Exists(tmppath) Then Kill(tmppath)
            mapdoc.Save(tmppath)
            z.AddFile(tmppath, "")
            Dim finfo As System.IO.FileInfo = My.Computer.FileSystem.GetFileInfo(tmppath)
            totMbytes += CDbl(finfo.Length / 100000) 'in mb
            'load all plan files.
            For i = 0 To _PropertiesItems.Count - 1
                'If _PropertiesItems(i).Include_In_ZipFile Then 
                If _PropertiesItems(i).Is_In_Project_Directory Then
                    For j = 0 To _PropertiesItems(i).File_Paths.Count - 1
                        Dim ix As Integer = i
                        Dim jx As Integer = j
                        Dim tmpstr As String = _PropertiesItems(i).File_Paths(j).Replace(_basedirectory & "\", "").Replace("\", "/") 'System.IO.Path.GetDirectoryName(_PropertiesItems(ix).File_Paths(jx)).Replace(_basedirectory, "")
                        If z.EntryFileNames.Contains(tmpstr) Then
                            ' MsgBox("file already exists")
                        Else
                            z.AddFile(_PropertiesItems(i).File_Paths(j), System.IO.Path.GetDirectoryName(_PropertiesItems(i).File_Paths(j)).Replace(_basedirectory, ""))
                            totMbytes += CDbl(_PropertiesItems(i).File_Size) 'in mb
                        End If

                    Next

                Else
                    'this should never happen.
                    If Not properties Then z.AddDirectoryByName("Properties") : properties = True
                    For j = 0 To _PropertiesItems(i).File_Paths.Count - 1
                        z.AddFile(_PropertiesItems(i).File_Paths(j), "Properties")
                        totMbytes += CDbl(_PropertiesItems(i).File_Size) 'in mb
                    Next
                End If
                'End If
            Next
            '_totmb = totMbytes
            z.Save()
            z.Dispose()
            Kill(tmppath)
            wasunsucsessful = False
        Catch ex As Exception
            z.Dispose()
            wasunsucsessful = True
            MsgBox("Errors Occured.")
            'e.Result = False
            'Return False
        End Try
        'e.Result = True
    End Sub
    Private Class SourceandIndex
        Public Index As Integer
        Public source As String
        Public Sub New(indeex As Integer, sourcestring As String)
            Index = indeex
            source = sourcestring
        End Sub
    End Class
    Public Function SpecialMapXMLforZipFile(ByVal items As List(Of Integer), Optional ByVal BaseDirectory As String = "") As XElement
        Dim MapTreeElement As New XElement("Map_Layers")

        If IsNothing(_maptreeview.Projection) Then
            MapTreeElement.SetAttributeValue("Map_Projection", "")
        Else
            Try
                MapTreeElement.SetAttributeValue("Map_Projection", _maptreeview.Projection.GetWKT)
            Catch ex As Exception
                MapTreeElement.SetAttributeValue("Map_Projection", "")
            End Try
        End If
        With _maptreeview.MapWindow.CurrentViewDataExtent
            MapTreeElement.SetAttributeValue("View_MinX", .MinX.ToString)
            MapTreeElement.SetAttributeValue("View_MaxX", .MaxX.ToString)
            MapTreeElement.SetAttributeValue("View_MinY", .MinY.ToString)
            MapTreeElement.SetAttributeValue("View_MaxY", .MaxY.ToString)
        End With
        ' Dim SearchString As String
        Dim index As Integer = 0
        For Each node As OpenGLMapping.FeatureNode In _maptreeview.MapWindow.GISFeatures
            Try
                If items.Contains(index) Then MapTreeElement.Add(node.WriteToXElement(BaseDirectory))
            Catch ex As Exception
                MsgBox("yo")
                'Something went wrong writing this layer so skip it. I should probably show a warning message.
            End Try
            index += 1
        Next

        Return MapTreeElement
    End Function

    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs) Handles CloseButton.Click
        Me.Close()
    End Sub
End Class
