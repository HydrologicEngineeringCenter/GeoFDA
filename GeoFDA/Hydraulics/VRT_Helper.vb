Public Class VRT_Helper
    Private _VRT_FilePath As String
    Private _Files As List(Of String)
    Sub New(ByVal VRT As LifeSimGIS.VRTReader)
        _Files = VRT.RasterFiles
        _VRT_FilePath = VRT.SourceFile
    End Sub
    Public Function SaveAs(ByVal NewVRTName As String, ByVal outputpath As String) As Boolean
        Dim ret As Boolean = False
        Dim outputvrt As String = outputpath ' & NewVRTName & ".vrt"
        outputpath = System.IO.Path.GetDirectoryName(outputpath)
        ''VRTDataset
        ''VRTRasterBand
        ''ComplexSource
        ''SourceFilename
        Dim oldroot As String = System.IO.Path.GetDirectoryName(_VRT_FilePath)
        If System.IO.File.Exists(_VRT_FilePath) Then
            Dim ba As Byte() = System.IO.File.ReadAllBytes(_VRT_FilePath)
            Dim ms As New System.IO.MemoryStream(ba)
            Dim xdoc As New XDocument
            xdoc = XDocument.Load(ms)
            ms.Dispose()
            Dim root As XElement = xdoc.Root

            'Dim vrtdataset As XElement = root
            'Dim vrtrasterband As XElement = vrtdataset
            'Dim complexsource As XElement = vrtrasterband
            'Dim sourcefiles As List(Of XElement) = complexsource.Elements("SourceFilename")
            Dim counter As Integer = 0
            For Each srcfile As XElement In xdoc.Element("VRTDataset").Element("VRTRasterBand").Element("ComplexSource").Elements("SourceFilename")
                System.IO.File.Copy(oldroot & "\" & srcfile.Value, outputpath & "\" & NewVRTName & "_" & counter & System.IO.Path.GetExtension(srcfile.Value))
                srcfile.SetValue(NewVRTName & "_" & counter & System.IO.Path.GetExtension(srcfile.Value))
                counter += 1
            Next
            xdoc.Save(outputvrt)
            ret = True
        Else
        End If

        Return ret
    End Function
    Public Function Rename(ByVal NewVRTName As String) As Boolean
        Dim ret As Boolean = False
        Dim oldroot As String = System.IO.Path.GetDirectoryName(_VRT_FilePath)
        Dim outputvrt As String = oldroot & "\" & NewVRTName & ".vrt"
        System.IO.File.Copy(_VRT_FilePath, outputvrt)
        System.IO.File.Delete(_VRT_FilePath)
        ret = True
        Return ret
    End Function
    Public Function Delete() As Boolean
        System.IO.File.Delete(_VRT_FilePath)
        For Each s As String In _Files
            System.IO.File.Delete(s)
        Next
        Return True
    End Function
End Class
