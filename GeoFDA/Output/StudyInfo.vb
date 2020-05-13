Public Class StudyInfo
    Private _Plans As List(Of PlanInfo)
    Private _Streams As List(Of Stream)
    Private _Reaches As List(Of Reach)
    Private _BaseYear As Integer
    Private _futureMostLikelyYear As Integer
    Sub New(ByVal plans As List(Of PlanInfo), ByVal streams As List(Of Stream), ByVal reaches As List(Of Reach), ByVal baseyear As Integer, ByVal futureyear As Integer)
        _Plans = plans
        _Streams = streams
        _Reaches = reaches
        _BaseYear = baseyear
        _futureMostLikelyYear = futureyear
    End Sub
    Public ReadOnly Property getplans As List(Of PlanInfo)
        Get
            Return _Plans
        End Get
    End Property
    Public ReadOnly Property GetBaseyear As Integer
        Get
            Return _BaseYear
        End Get
    End Property
    Public ReadOnly Property GetFutureYear As Integer
        Get
            Return _futureMostLikelyYear
        End Get
    End Property
    Public Function GetPlanDt() As System.Data.DataTable
        Dim plansdt As New System.Data.DataTable
        Dim planname As New System.Data.DataColumn("Plan Name", GetType(String))
        Dim planDisc As New System.Data.DataColumn("Plan Description", GetType(String))
        plansdt.Columns.Add(planname)
        plansdt.Columns.Add(planDisc)
        For i = 0 To _Plans.Count - 1
            plansdt.Rows.Add({_Plans(i).GetPlanName, _Plans(i).GetPlanDesc}.ToArray)
        Next
        Return plansdt
    End Function
    Public Sub SetPlans(ByVal plandt As System.Data.DataTable)
        _Plans.Clear()
        Dim planname As String
        Dim plandesc As String
        Dim pi As PlanInfo
        For i = 0 To plandt.Rows.Count - 1
            planname = plandt.Rows(i).Item(0).ToString
            plandesc = plandt.Rows(i).Item(1).ToString
            pi = New PlanInfo(planname, plandesc)
            _Plans.Add(pi)
        Next
    End Sub
    Public Function GetStreamDt() As System.Data.DataTable
        Dim streamsdt As New System.Data.DataTable
        Dim streamnames As New System.Data.DataColumn("Stream Name", GetType(String))
        Dim streamdesc As New System.Data.DataColumn("Stream Description", GetType(String))
        streamsdt.Columns.Add(streamnames)
        streamsdt.Columns.Add(streamdesc)
        For i = 0 To _Streams.Count - 1
            streamsdt.Rows.Add({_Streams(i).GetStreamName, _Streams(i).GetStreamDescription}.ToArray)
        Next
        Return streamsdt
    End Function
    Public Function GetReachDT() As System.Data.DataTable
        Dim reachesdt As New System.Data.DataTable

        Dim reachnames As New System.Data.DataColumn("Reach Name", GetType(String))
        Dim reachDesc As New System.Data.DataColumn("Reach Description", GetType(String))
        Dim ReachStreamname As New System.Data.DataColumn("Stream Name", GetType(String))
        Dim beginstation As New System.Data.DataColumn("Beginning Station", GetType(Double))
        Dim endstation As New System.Data.DataColumn("Ending Station", GetType(Double))
        Dim bank As New System.Data.DataColumn("Bank", GetType(String))
        Dim IndexStation As New System.Data.DataColumn("Index Station", GetType(Double))
        reachesdt.Columns.Add(reachnames)
        reachesdt.Columns.Add(reachDesc)
        reachesdt.Columns.Add(ReachStreamname)
        reachesdt.Columns.Add(beginstation)
        reachesdt.Columns.Add(endstation)
        reachesdt.Columns.Add(bank)
        reachesdt.Columns.Add(IndexStation)

        For i = 0 To _Reaches.Count - 1
            reachesdt.Rows.Add({_Reaches(i).GetReachName, _Reaches(i).GetDescription, _Reaches(i).GetRiverName, _Reaches(i).GetUpstreamStation, _Reaches(i).GetDownstreamStation, _Reaches(i).GetBank, _Reaches(i).GetIndexLocation.GetStationing.Station}.ToArray)
        Next
        Return reachesdt
    End Function
    Public Overrides Function ToString() As String
        Dim str As New System.Text.StringBuilder
        str.Append("Plan_Name" & vbTab & "Plan_Desc" & vbNewLine)
        For i = 0 To _Plans.Count - 1
            str.AppendLine(_Plans(i).ToString)
        Next
        str.AppendLine("Year_Name")
        str.AppendLine(_BaseYear.ToString)
        str.AppendLine(_futureMostLikelyYear.ToString)
        str.AppendLine("Strm_Nme" & vbTab & "Strm_Desc")
        For i = 0 To _Streams.Count - 1
            str.AppendLine(_Streams(i).ToString)
        Next
        str.AppendLine()
        str.AppendLine("Rch_Name" & vbTab & "Rch_Desc" & vbTab & "Stream_Nme" & vbTab & "Beg_Sta" & vbTab & "End_Sta" & vbTab & "Bank" & vbTab & "Index_Sta" & vbTab & "SID_Rffld")
        For i = 0 To _Reaches.Count - 1
            str.AppendLine(_Reaches(i).ToString)
        Next

        Return str.ToString
    End Function
End Class
