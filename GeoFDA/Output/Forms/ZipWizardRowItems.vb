Public Class ZipWizardRowItems
    Private _FilePaths As List(Of String)
    Private _FileType As String
    Private _FileSize As String
    Private _Store As Boolean = True
    Private _InProjectDirectory As Boolean = True
    Public ReadOnly Property File_Name As String
        Get
            Return System.IO.Path.GetFileName(_FilePaths(0))
        End Get
    End Property
    Public ReadOnly Property File_Paths As List(Of String)
        Get
            Return _FilePaths
        End Get
    End Property
    Public ReadOnly Property File_Type As String
        Get
            Return _FileType
        End Get
    End Property
    Public ReadOnly Property File_Size As String
        Get
            Return _FileSize
        End Get
    End Property
    Property Include_In_ZipFile As Boolean
        Get
            Return _Store
        End Get
        Set(value As Boolean)
            _Store = value
        End Set
    End Property
    Public ReadOnly Property Is_In_Project_Directory As Boolean
        Get
            Return _InProjectDirectory
        End Get
    End Property
    Sub New(ByVal file As String, ByVal filetype As String, ByVal basedirectory As String)

        _FilePaths = New List(Of String)
        _FilePaths.Add(file)
        _FileType = filetype
        Dim finfo As System.IO.FileInfo
        Dim fsize As Long = 0
        If file = "" Then
        Else
            finfo = My.Computer.FileSystem.GetFileInfo(_FilePaths(0))
            fsize = finfo.Length
            Select Case finfo.Extension
                Case ".shp"
                    'add the auxilary files.
                    If System.IO.File.Exists(System.IO.Path.ChangeExtension(_FilePaths(0), ".dbf")) Then
                        _FilePaths.Add(System.IO.Path.ChangeExtension(_FilePaths(0), ".dbf"))
                        fsize += My.Computer.FileSystem.GetFileInfo(_FilePaths.Last).Length
                    End If
                    If System.IO.File.Exists(System.IO.Path.ChangeExtension(_FilePaths(0), ".shx")) Then
                        _FilePaths.Add(System.IO.Path.ChangeExtension(_FilePaths(0), ".shx"))
                        fsize += My.Computer.FileSystem.GetFileInfo(_FilePaths.Last).Length
                    End If
                    If System.IO.File.Exists(System.IO.Path.ChangeExtension(_FilePaths(0), ".prj")) Then
                        _FilePaths.Add(System.IO.Path.ChangeExtension(_FilePaths(0), ".prj"))
                        fsize += My.Computer.FileSystem.GetFileInfo(_FilePaths.Last).Length
                    End If
                Case ".vrt"
                'find all files internal to the vrt and get their sizes too.
                Case ".flt"
                    'find the header file
                Case Else
                    'it should just be the one file.
                    'dss files have more than one piece.
            End Select
            Dim tmpstr As String = _FilePaths(0)
            _InProjectDirectory = Not tmpstr.Replace(basedirectory, ".") = _FilePaths(0)
        End If



        _FileSize = String.Format("{0,-10:0.###}", fsize / 1000000)
    End Sub
End Class
