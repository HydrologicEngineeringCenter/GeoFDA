Public Class FilePathManipulator
    Private _InvalidCharacters() = {"<", ">", ":", Chr(34), "/", "\", "|", "?", "*"}
    Public Function IsPathAbsolute(ByVal path As String) As Boolean
        If path.StartsWith("\\") Then Return True
        If path.Substring(1, 1) = ":" Then
            If path.Substring(2, 1) = "\" Then Return True
        Else
            If path.Substring(0, 1) = "\" Then Return True
        End If
        Return False
    End Function
    Public Function RelativeToAbsolute(ByVal path As String) As String
        If IsPathAbsolute(path) Then Return path
        If path.Substring(1, 1) = ":" Then
            'the path has a drive specification
            'System.IO.Directory.GetCurrentDirectory
            Return Nothing
        Else
            Return Nothing
        End If
    End Function
    ''' <summary>
    ''' Takes an input file and a file that is the base file, and produces a relative path
    ''' </summary>
    ''' <param name="filename">the file you wish to have a relative path to</param>
    ''' <param name="baseFilename">the file location you wish to make the other file relative</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function MakeRelative(ByVal filename As String, ByVal baseFilename As String) As String
        Dim relativePath As String = ""
        '
        '
        If filename = "" Then Return ""
        '
        '  Get the location of the file that will operate as the relative location
        Dim basePath As String = System.IO.Path.GetDirectoryName(baseFilename)
        Dim filePath As String = System.IO.Path.GetDirectoryName(filename)
        If basePath = filePath Then
            '
            ' File is at base location (shared path)
            relativePath = System.IO.Path.Combine(".", System.IO.Path.GetFileName(filename))
        Else
            '
            ' Gotta figure out how much they are the same
            Dim fileParts() As String = Split(filePath, System.IO.Path.DirectorySeparatorChar)
            Dim baseParts() As String = Split(basePath, System.IO.Path.DirectorySeparatorChar)

            Dim iAgree As Integer = -1
            For i As Integer = 0 To baseParts.Count - 1
                If i > fileParts.Count - 1 Then
                    ' continue, base path is longer
                ElseIf baseParts(i).ToUpper = fileParts(i).ToUpper Then
                    ' they agree
                    iAgree = i
                Else
                    Exit For
                End If
            Next

            If iAgree < 0 Then
                ' then paths don't match at all and we must use an absolute path
                relativePath = filename
            Else
                If iAgree = baseParts.Count - 1 Then
                    ' path match down  base path, we must add on fileParts
                    relativePath = "."
                Else
                    '  Go up to the common directory
                    For i As Integer = iAgree + 1 To baseParts.Count - 1
                        relativePath = System.IO.Path.Combine(relativePath, "..")
                    Next
                End If
                '
                '  Go down the singular path
                For i As Integer = iAgree + 1 To fileParts.Count - 1
                    relativePath = System.IO.Path.Combine(relativePath, fileParts(i))
                Next
                '
                ' Add the filename
                relativePath = System.IO.Path.Combine(relativePath, System.IO.Path.GetFileName(filename))
            End If
        End If
        '
        '
        Return relativePath
    End Function
    ''' <summary>
    ''' takes an input relative file path and converts it to an absolute path
    ''' </summary>
    ''' <param name="filename">the input file that is relative</param>
    ''' <param name="baseFilename">the location of the file it is relative to</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function MakeAbsolute(ByVal filename As String, ByVal baseFilename As String) As String
        '
        ' bail out if no relative filename is entered
        If filename = "" Then Return ""
        '
        ' make the absolute pathname from the relative pathname
        Dim absolutePath As String = ""
        '
        '  Get the location of the file that will operate as the relative location
        Dim basePath As String = System.IO.Path.GetDirectoryName(baseFilename)
        Dim baseParts() As String = Split(basePath, System.IO.Path.DirectorySeparatorChar)
        '
        Dim fileParts() As String = Split(filename, System.IO.Path.DirectorySeparatorChar) ' this includes the filename
        If fileParts(0) = "." Then
            '
            ' use basepath location and build path down to file
            absolutePath = basePath
            For i As Integer = 1 To fileParts.Count - 1
                absolutePath = System.IO.Path.Combine(absolutePath, fileParts(i))
            Next
        ElseIf fileParts(0) = ".." Then
            '
            ' Find the how many directories up we need to go
            Dim iStart As Integer = -1
            For i As Integer = 0 To fileParts.Count - 1
                If fileParts(i) = ".." Then
                    iStart = i
                Else
                    Exit For
                End If
            Next
            '
            '  Build the basepath
            absolutePath = System.IO.Path.GetPathRoot(basePath)                 'Start with the root directory
            iStart = iStart + 1
            Dim iEnd As Integer = baseParts.Count - 1 - iStart
            For i As Integer = 1 To iEnd                                        'Add the folders (root is in the i=0 place)
                absolutePath = System.IO.Path.Combine(absolutePath, baseParts(i))
            Next
            '
            '  Build the remaining file path
            For i As Integer = iStart To fileParts.Count - 1
                absolutePath = System.IO.Path.Combine(absolutePath, fileParts(i))
            Next
        Else
            '
            ' The path was already absolute
            absolutePath = filename
        End If
        '
        ' 
        Return absolutePath
    End Function
End Class
