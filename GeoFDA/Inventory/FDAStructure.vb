Namespace ComputableObjects
    Public Class FDAStructure
		Inherits ComputableObjects.Consequences_Structure
		Private _value As Double
        Public Event ReportMessage(ByVal message As String)
        Sub New(ByVal name As String, ByVal occtype As OccupancyType, ByVal value As Double, ByVal foundationheight As Double, Optional ByVal loc As LifeSimGIS.PointD = Nothing)
            MyBase.New(name, occtype.DamageCategory, occtype, foundationheight, 0, loc)
            _value = value
        End Sub
		Sub New(ByVal tabbedline() As String, ByVal headers() As String, ByVal occtype As ComputableObjects.OccupancyType, ByVal inputmonetaryunit As String, ByVal outputMonetaryUnit As String)
			MyBase.New("", occtype.DamageCategory, occtype, 0, 0, New LifeSimGIS.PointD(0, 0))
			Dim north As Double
			If Not Double.TryParse(tabbedline(Array.IndexOf(headers, "North")), north) Then RaiseEvent ReportMessage("the North value could not be converted to double for structure: " & tabbedline(Array.IndexOf(headers, "Struc_Name")))
			Dim east As Double
			If Not Double.TryParse(tabbedline(Array.IndexOf(headers, "East")), east) Then RaiseEvent ReportMessage("the East value could not be converted to double for structure: " & tabbedline(Array.IndexOf(headers, "Struc_Name")))
			If north = 0 AndAlso east = 0 Then RaiseEvent ReportMessage("The strucutre " & tabbedline(Array.IndexOf(headers, "Struc_Name")) & " does not have valid coordinates")
			_location = New LifeSimGIS.PointD(east, north)

			For i = 0 To headers.Count - 1
				Select Case headers(i)
					Case "Struc_Name"
						If tabbedline(i) = "" OrElse tabbedline(i) = " " Then Throw New Exception("Name field was blank")
						If tabbedline(i).Length > 32 Then RaiseEvent ReportMessage("The structure name " & tabbedline(i) & " is longer than 32 characters.")
						'check for name conflicts
						_Name = tabbedline(i)
					Case "Stream_Name"
						If tabbedline(i) = "" OrElse tabbedline(i) = " " Then Throw New Exception("Stream field was blank")
						If tabbedline(i).Length > 32 Then RaiseEvent ReportMessage("The stream name " & tabbedline(i) & " is longer than 32 characters.")
						Stream = tabbedline(i)
					Case "Station"
						Double.TryParse(tabbedline(i), Stationing)
					Case "Bank"
						Select Case tabbedline(i)
							Case "l", "L", "1", "Left", "left", "LEFT"
								Bank = BankEnum.Left
							Case "r", "R", "0", "Right", "right", "RIGHT"
								Bank = BankEnum.Right
							Case Else
								Bank = BankEnum.Both
						End Select
					Case "Year"
						If tabbedline(i) = "" OrElse tabbedline(i) = " " Then
							'year is blank.
						Else
							Integer.TryParse(tabbedline(i), Year) ' how do i know what fields need to be output to shape?
						End If
					Case "Struc_Val"
						Double.TryParse(tabbedline(i), StructureValue)
						If inputmonetaryunit = outputMonetaryUnit Then
						Else
							Select Case inputmonetaryunit
								Case "$'s"
									Select Case outputMonetaryUnit
										Case "1,000$'s"
											StructureValue /= 1000
										Case "1,000,000$'s"
											StructureValue /= 1000000
										Case Else
											RaiseEvent ReportMessage("Somthing bad happened")
									End Select
								Case "1,000$'s"
									Select Case outputMonetaryUnit
										Case "$'s"
											StructureValue *= 1000
										Case "1,000,000$'s"
											StructureValue /= 1000
										Case Else
											RaiseEvent ReportMessage("Somthing bad happened")
									End Select
								Case "1,000,000$'s"
									Select Case outputMonetaryUnit
										Case "$'s"
											StructureValue *= 1000000
										Case "1,000$'s"
											StructureValue *= 1000
										Case Else
											RaiseEvent ReportMessage("Somthing bad happened")
									End Select
							End Select
						End If
					Case "Cont_Val"
						Double.TryParse(tabbedline(i), ContentValue)
						If inputmonetaryunit = outputMonetaryUnit Then
						Else
							Select Case inputmonetaryunit
								Case "$'s"
									Select Case outputMonetaryUnit
										Case "1,000$'s"
											ContentValue /= 1000
										Case "1,000,000$'s"
											ContentValue /= 1000000
										Case Else
											RaiseEvent ReportMessage("Somthing bad happened")
									End Select
								Case "1,000$'s"
									Select Case outputMonetaryUnit
										Case "$'s"
											ContentValue *= 1000
										Case "1,000,000$'s"
											ContentValue /= 1000
										Case Else
											RaiseEvent ReportMessage("Somthing bad happened")
									End Select
								Case "1,000,000$'s"
									Select Case outputMonetaryUnit
										Case "$'s"
											ContentValue *= 1000000
										Case "1,000$'s"
											ContentValue *= 1000
										Case Else
											RaiseEvent ReportMessage("Somthing bad happened")
									End Select
							End Select
						End If
					Case "Other_Val"
						Double.TryParse(tabbedline(i), OtherValue)
						If inputmonetaryunit = outputMonetaryUnit Then
						Else
							Select Case inputmonetaryunit
								Case "$'s"
									Select Case outputMonetaryUnit
										Case "1,000$'s"
											OtherValue /= 1000
										Case "1,000,000$'s"
											OtherValue /= 1000000
										Case Else
											RaiseEvent ReportMessage("Somthing bad happened")
									End Select
								Case "1,000$'s"
									Select Case outputMonetaryUnit
										Case "$'s"
											OtherValue *= 1000
										Case "1,000,000$'s"
											OtherValue /= 1000
										Case Else
											RaiseEvent ReportMessage("Somthing bad happened")
									End Select
								Case "1,000,000$'s"
									Select Case outputMonetaryUnit
										Case "$'s"
											OtherValue *= 1000000
										Case "1,000$'s"
											OtherValue *= 1000
										Case Else
											RaiseEvent ReportMessage("Somthing bad happened")
									End Select
							End Select
						End If
					Case "1F_Stage"
						Double.TryParse(tabbedline(i), FirstFloorElevation)
					Case "Grnd_Stage"
						Double.TryParse(tabbedline(i), GroundEle)
					Case "Found_Ht"
						Double.TryParse(tabbedline(i), FH)
					Case "Begin_Damg"
						Double.TryParse(tabbedline(i), BeginDamage)
					Case "Street"
						'
					Case "City"
						'
					Case "State"
						'
					Case "Zip"
						'
					Case "Zone"
						'
					Case "Num_Struct"
						Integer.TryParse(tabbedline(i), NumStructs)
					Case "Notes"
					Case "Mod_Name"
						If tabbedline(i) = "" OrElse tabbedline(i) = " " Then St_Module = "Base"
						St_Module = CStr(tabbedline(i))
					Case "SID_Rch"
						SidReach = CStr(tabbedline(i))
					Case "SID_Reffld"
					Case Else

				End Select
			Next
		End Sub
		Public Property StructureValue As Double
            Get
                Return _value
            End Get
            Set(value As Double)
                _value = value
            End Set
        End Property
        Public Property FirstFloorElevation As Double
        Public Property ContentValue As Double
        Public Property OtherValue As Double
        Public Property Stream As String
        Public Property Stationing As Double
        Public Property SidReach As String
        Public Property St_Module As String
        Public Property Year As Integer
        Public Property Bank As BankEnum
        Public Property BeginDamage As Double
        Public Property NumStructs As Integer
        Public Overrides Function WriteToFDAString() As String
            Dim str As New System.Text.StringBuilder
            str.Append(Name & vbTab)
            str.Append(Occtype.DamageCategory.Name & vbTab)
            str.Append(Stream & vbTab)
            str.Append(Occtype.Name & vbTab)
            str.Append(Stationing & vbTab)
            str.Append(Bank.ToString & vbTab)
            str.Append(Year & vbTab)
            str.Append(StructureValue & vbTab)
            str.Append(ContentValue & vbTab)
            str.Append(OtherValue & vbTab)
            str.Append(FirstFloorElevation & vbTab)
            str.Append(GroundEle & vbTab)
            str.Append(FH & vbTab)
            str.Append(SidReach & vbTab)
            str.Append(Location.Y & vbTab)
            str.Append(Location.X & vbTab)
            str.Append(St_Module & vbTab)
            Return str.ToString
        End Function
    End Class
End Namespace

