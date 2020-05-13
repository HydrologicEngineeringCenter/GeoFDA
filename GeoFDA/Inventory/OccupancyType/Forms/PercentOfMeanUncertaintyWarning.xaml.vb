Imports Statistics
Imports System.Windows.Controls
Imports System.Windows
Namespace Controls.SupportingObjects
    Public Class PercentOfMeanUncertaintyWarning
        Public Shared ParameterNameProp As DependencyProperty = DependencyProperty.Register("ParameterName", GetType(String), GetType(PercentOfMeanUncertaintyWarning), New FrameworkPropertyMetadata("ParameterName"))
        Public Property ParameterName As String
            Get
                Return DirectCast(Me.GetValue(ParameterNameProp), String)
            End Get
            Set(value As String)
                Me.SetValue(ParameterNameProp, value)
            End Set
        End Property
        Public Property HasNormal As Boolean = True
        Public Property HasTriangular As Boolean = True
        Public Property HasUniform As Boolean = True
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            If HasNormal = False Then
                'StDevColumn.Width = New GridLength(0)
                UncertaintyComboBox.Items.Remove(NormalComboItem)
            End If
            If HasTriangular = False Then
                'TriangularMaximumColumn.Width = New GridLength(0)
                'TriangularMinimumColumn.Width = New GridLength(0)
                'TriangleMaxTextBox.Visibility = Windows.Visibility.Collapsed
                'TriangleMinTextBox.Visibility = Windows.Visibility.Collapsed
                UncertaintyComboBox.Items.Remove(TriangularComboItem)
            End If
            If HasUniform = False Then
                'UniformMaximumColumn.Width = New GridLength(0)
                'UniformMinimumColumn.Width = New GridLength(0)
                UncertaintyComboBox.Items.Remove(UniformComboItem)
            End If
            UncertaintyComboBox.SelectedIndex = 0
        End Sub

        Private Sub UncertaintyComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If UncertaintyComboBox.SelectedIndex = -1 Then Exit Sub
            MostLikelyPanel.Visibility = Windows.Visibility.Collapsed
            TriangularMinPanel.Visibility = Windows.Visibility.Collapsed
            TriangularMaxPanel.Visibility = Windows.Visibility.Collapsed
            StandardDeviationPanel.Visibility = Windows.Visibility.Collapsed
            UniformMinimumPanel.Visibility = Windows.Visibility.Collapsed
            UniformMaximumPanel.Visibility = Windows.Visibility.Collapsed
            'TriangularMaximumColumn.Width = New GridLength(0)
            'TriangularMinimumColumn.Width = New GridLength(0)
            '
            'UniformMaximumColumn.Width = New GridLength(0)
            'UniformMinimumColumn.Width = New GridLength(0)
            '
            'StDevColumn.Width = New GridLength(0)
            '
            'MostLikelyColumn.Width = New GridLength(0)
            '
            Select Case CType(UncertaintyComboBox.Items(UncertaintyComboBox.SelectedIndex), ComboBoxItem).Name
                Case NoneComboItem.Name
                    'MostLikelyColumn.Width = New GridLength(65)
                Case TriangularComboItem.Name
                    'MostLikelyColumn.Width = New GridLength(65)
                    Grid.SetColumn(TriangularMinPanel, 2)
                    Grid.SetColumn(MostLikelyPanel, 3)
                    Grid.SetColumn(TriangularMaxPanel, 4)
                    TriangularMinPanel.Visibility = Windows.Visibility.Visible
                    TriangularMaxPanel.Visibility = Windows.Visibility.Visible
                    MostLikelyPanel.Visibility = Windows.Visibility.Visible
                    'TriangularMaximumColumn.Width = New GridLength(65)
                    'TriangularMinimumColumn.Width = New GridLength(65)
                Case UniformComboItem.Name
                    Grid.SetColumn(UniformMinimumPanel, 2)
                    Grid.SetColumn(UniformMaximumPanel, 3)
                    UniformMinimumPanel.Visibility = Windows.Visibility.Visible
                    UniformMaximumPanel.Visibility = Windows.Visibility.Visible
                    'UniformMaximumColumn.Width = New GridLength(65)
                    'UniformMinimumColumn.Width = New GridLength(65)
                Case NormalComboItem.Name
                    'MostLikelyColumn.Width = New GridLength(65)
                    'StDevColumn.Width = New GridLength(65)
                    Grid.SetColumn(MostLikelyPanel, 2)
                    Grid.SetColumn(StandardDeviationPanel, 3)
                    MostLikelyPanel.Visibility = Windows.Visibility.Visible
                    StandardDeviationPanel.Visibility = Windows.Visibility.Visible
            End Select
        End Sub
        Private Sub TextBox_PreviewTextInput(sender As Object, e As System.Windows.Input.TextCompositionEventArgs)
            Dim Text As TextBox = CType(sender, TextBox)
            If Not Char.IsDigit(CChar(e.Text)) Then e.Handled = True 'numeric only
            If e.Text = Chr(8) Then e.Handled = False 'allow Backspace
            If e.Text = " " Then e.Handled = True 'don't allow spaces
            If e.Text = "-" And Text.SelectionStart = 0 And Text.Text.IndexOf("-") = -1 Then e.Handled = False 'allow negative
            If e.Text = "." And Text.Text.IndexOf(".") = -1 Then e.Handled = False 'allow one decimal
            'If e.KeyChar = Chr(13) Then OK.Focus() 'enter to move to next
        End Sub
        Public Sub LoadOccTypeData(ByVal TheData As ContinuousDistribution)
            StdDevTextBox.Text = ""
            TriangleMinTextBox.Text = ""
            TriangleMaxTextBox.Text = ""
            UniformMinTextBox.Text = ""
            UniformMaxTextBox.Text = ""
            If IsNothing(TheData) Then Exit Sub
            Select Case TheData.GetType
                Case GetType(None)
                    UncertaintyComboBox.SelectedItem = NoneComboItem
                Case GetType(Normal)
                    UncertaintyComboBox.SelectedItem = NormalComboItem
                    StdDevTextBox.Text = String.Format("{0:0.#####}", CType(TheData, Normal).GetStDev.ToString)
                    StdDevTextBox_LostFocus(Nothing, Nothing)
                Case GetType(Triangular)
                    UncertaintyComboBox.SelectedItem = TriangularComboItem
                    TriangleMinTextBox.Text = String.Format("{0:0.#####}", CType(TheData, Triangular).getMin.ToString)
                    TriangleMaxTextBox.Text = String.Format("{0:0.#####}", CType(TheData, Triangular).getMax.ToString)
                    TriangleMaxTextBox_LostFocus(Nothing, Nothing)
                    TriangleMinTextBox_LostFocus(Nothing, Nothing)
                Case GetType(Uniform)
                    UncertaintyComboBox.SelectedItem = UniformComboItem
                    UniformMinTextBox.Text = String.Format("{0:0.#####}", CType(TheData, Uniform).GetMin.ToString)
                    UniformMaxTextBox.Text = String.Format("{0:0.#####}", CType(TheData, Uniform).GetMax.ToString)
                    UniformMaxTextBox_LostFocus(Nothing, Nothing)
                    UniformMinTextBox_LostFocus(Nothing, Nothing)
            End Select
        End Sub
        Public Function CheckDistributionValidity(Optional ByRef MessageOut As String = "") As Boolean
            MessageOut = ""
            Dim DoubleChecker As Double
            Select Case CType(UncertaintyComboBox.SelectedItem, ComboBoxItem).Name
                Case NoneComboItem.Name
                    Return True
                Case TriangularComboItem.Name
                    If Double.TryParse(TriangleMinTextBox.Text, DoubleChecker) = False Then
                        MessageOut = "Triangular minimum value is not a valid entry."
                        Return False
                    End If
                    If Double.TryParse(TriangleMaxTextBox.Text, DoubleChecker) = False Then
                        MessageOut = "Triangular maximum value is not a valid entry."
                        Return False
                    End If
                Case NormalComboItem.Name
                    If Double.TryParse(StdDevTextBox.Text, DoubleChecker) = False Then
                        MessageOut = "Standard deviation value is not a valid entry."
                        Return False
                    End If
                Case UniformComboItem.Name
                    If Double.TryParse(UniformMinTextBox.Text, DoubleChecker) = False Then
                        MessageOut = "Uniform minimum value is not a valid entry."
                        Return False
                    End If
                    If Double.TryParse(UniformMaxTextBox.Text, DoubleChecker) = False Then
                        MessageOut = "Uniform maximum value is not a valid entry."
                        Return False
                    End If
            End Select
            Return True
        End Function
        Public Function ReturnDistribution() As ContinuousDistribution
            'If CheckDistributionValidity() = False Then Return Nothing

            Select Case CType(UncertaintyComboBox.SelectedItem, ComboBoxItem).Name
                Case NoneComboItem.Name
                    Return New None()
                Case TriangularComboItem.Name
                    Dim mindbl As Double
                    Dim maxdbl As Double
                    Double.TryParse(TriangleMinTextBox.Text, mindbl)
                    Double.TryParse(TriangleMaxTextBox.Text, maxdbl)
                    Return New Triangular(mindbl, maxdbl, 0)
                Case NormalComboItem.Name
                    Dim stdevdbl As Double
                    Double.TryParse(StdDevTextBox.Text, stdevdbl)
                    Return New Normal(0, stdevdbl)
                Case UniformComboItem.Name
                    Dim mindbl As Double
                    Dim maxdbl As Double
                    Double.TryParse(UniformMinTextBox.Text, mindbl)
                    Double.TryParse(UniformMaxTextBox.Text, maxdbl)
                    Return New Uniform(mindbl, maxdbl)
                Case Else
                    Return New None()
            End Select
        End Function

        Private Sub TriangleMinTextBox_LostFocus(sender As Object, e As RoutedEventArgs)
            If TriangleMinTextBox.Text = "" Then
                TriangleMinTextBox.BorderBrush = System.Windows.Media.Brushes.Red
                TriangleMinTextBox.ToolTip = "The Trangular distribution Min Value must be defined"
                Exit Sub
            End If
            Dim DoubleValue As Double
            If Double.TryParse(TriangleMinTextBox.Text, DoubleValue) Then
                If DoubleValue < (-100) Then ' TriangleMinTextBox.Text = "-100"
                    TriangleMinTextBox.BorderBrush = System.Windows.Media.Brushes.Red
                    TriangleMinTextBox.ToolTip = "The min of the triangular distribution for a parameter with uncertainty defined as a percentage of the mean cannot be less than -100"
                ElseIf DoubleValue > 0 Then ' TriangleMinTextBox.Text = "0"
                    TriangleMinTextBox.BorderBrush = System.Windows.Media.Brushes.Red
                    TriangleMinTextBox.ToolTip = "The min of the triangular distribution for a parameter with uncertainty defined as a percentage of the mean cannot be greater than 0"
                Else
                    TriangleMinTextBox.BorderBrush = System.Windows.Media.Brushes.Black
                    TriangleMinTextBox.ToolTip = Nothing
                End If
            Else
                'TriangleMinTextBox.Text = ""
                TriangleMinTextBox.BorderBrush = System.Windows.Media.Brushes.Red
                TriangleMinTextBox.ToolTip = "The value " & TriangleMinTextBox.Text & " could not be converted to double"
            End If
        End Sub

        Private Sub TriangleMaxTextBox_LostFocus(sender As Object, e As RoutedEventArgs)
            If TriangleMaxTextBox.Text = "" Then
                TriangleMaxTextBox.BorderBrush = System.Windows.Media.Brushes.Red
                TriangleMaxTextBox.ToolTip = "The Trangular distribution Max Value must be defined"
                Exit Sub
            End If
            Dim DoubleValue As Double
            If Double.TryParse(TriangleMaxTextBox.Text, DoubleValue) Then
                If DoubleValue < 0 Then ' TriangleMaxTextBox.Text = "0"
                    TriangleMaxTextBox.BorderBrush = System.Windows.Media.Brushes.Red
                    TriangleMaxTextBox.ToolTip = "The max of the triangular distribution for a parameter with uncertainty defined as a percentage of the mean cannot be less than 0"
                Else
                    TriangleMaxTextBox.BorderBrush = System.Windows.Media.Brushes.Black
                    TriangleMaxTextBox.ToolTip = Nothing
                End If
            Else
                'TriangleMaxTextBox.Text = ""
                TriangleMaxTextBox.BorderBrush = System.Windows.Media.Brushes.Red
                TriangleMaxTextBox.ToolTip = "The value " & TriangleMaxTextBox.Text & " could not be converted to double"
            End If
        End Sub

        Private Sub StdDevTextBox_LostFocus(sender As Object, e As RoutedEventArgs)
            If StdDevTextBox.Text = "" Then
                StdDevTextBox.BorderBrush = System.Windows.Media.Brushes.Red
                StdDevTextBox.ToolTip = "The Normal distribution Standard Deviation as a percent of the mean Value must be defined"
                Exit Sub
            End If
            Dim DoubleValue As Double
            If Double.TryParse(StdDevTextBox.Text, DoubleValue) Then
                If DoubleValue < 0 Then ' StdDevTextBox.Text = "0"
                    StdDevTextBox.BorderBrush = System.Windows.Media.Brushes.Red
                    StdDevTextBox.ToolTip = "The Standard Deviation of the Normal distribution for a parameter with uncertainty defined as percentage of the mean cannot be less than zero"
                ElseIf DoubleValue > 15 Then ' StdDevTextBox.Text = "15"
                    StdDevTextBox.BorderBrush = System.Windows.Media.Brushes.Red
                    StdDevTextBox.ToolTip = "The Standard Deviation of the Normal distribution for a parameter with uncertainty defined as percentage of the mean cannot be greater than 15"
                Else
                    StdDevTextBox.BorderBrush = System.Windows.Media.Brushes.Black
                    StdDevTextBox.ToolTip = Nothing
                End If
            Else
                StdDevTextBox.BorderBrush = System.Windows.Media.Brushes.Red
                StdDevTextBox.ToolTip = "The value " & StdDevTextBox.Text & " could not be converted to a double"
            End If
        End Sub

        Private Sub UniformMinTextBox_LostFocus(sender As Object, e As RoutedEventArgs)
            If UniformMinTextBox.Text = "" Then
                UniformMinTextBox.BorderBrush = System.Windows.Media.Brushes.Red
                UniformMinTextBox.ToolTip = "The Uniform Min Value must be defined"
                Exit Sub
            End If

            Dim DoubleValue As Double
            If Double.TryParse(UniformMinTextBox.Text, DoubleValue) Then
                If DoubleValue < (-100) Then ' UniformMinTextBox.Text = "-100"
                    UniformMinTextBox.BorderBrush = System.Windows.Media.Brushes.Red
                    UniformMinTextBox.ToolTip = "The min of the Uniform distribution for a parameter with uncertainty defined as a percentage of the mean cannot be less than -100"
                ElseIf DoubleValue > 0 Then ' UniformMinTextBox.Text = "0"
                    UniformMinTextBox.BorderBrush = System.Windows.Media.Brushes.Red
                    UniformMinTextBox.ToolTip = "The min of the Uniform distribution for a parameter with uncertainty defined as a percentage of the mean cannot be greater than 0"
                Else

                    UniformMinTextBox.BorderBrush = System.Windows.Media.Brushes.Black
                    UniformMinTextBox.ToolTip = Nothing
                End If
            Else
                'UniformMinTextBox.Text = ""
                UniformMinTextBox.BorderBrush = System.Windows.Media.Brushes.Red
                UniformMinTextBox.ToolTip = "The value " & UniformMinTextBox.Text & " could not be converted to double"
            End If


        End Sub

        Private Sub UniformMaxTextBox_LostFocus(sender As Object, e As RoutedEventArgs)
            If UniformMaxTextBox.Text = "" Then
                UniformMaxTextBox.BorderBrush = System.Windows.Media.Brushes.Red
                UniformMaxTextBox.ToolTip = "The Uniform Max Value must be defined"
                Exit Sub
            End If
            Dim DoubleValue As Double
            If Double.TryParse(UniformMaxTextBox.Text, DoubleValue) Then
                If DoubleValue < 0 Then ' TriangleMaxTextBox.Text = "0"
                    UniformMaxTextBox.BorderBrush = System.Windows.Media.Brushes.Red
                    UniformMaxTextBox.ToolTip = "The max of the Uniform distribution for a parameter with uncertainty defined as a percentage of the mean cannot be less than 0"
                Else
                    UniformMaxTextBox.BorderBrush = System.Windows.Media.Brushes.Black
                    UniformMaxTextBox.ToolTip = Nothing
                End If
            Else
                'TriangleMaxTextBox.Text = ""
                UniformMaxTextBox.BorderBrush = System.Windows.Media.Brushes.Red
                UniformMaxTextBox.ToolTip = "The value " & UniformMaxTextBox.Text & " could not be converted to double"
            End If
        End Sub
    End Class
End Namespace
