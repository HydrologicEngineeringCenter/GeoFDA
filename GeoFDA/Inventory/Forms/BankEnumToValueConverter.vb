Public Class BankEnumToValueConverterRight
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert
        If IsNothing(value) Then
            'no clue what to do
            Return False
        Else
            If value.ToString = Consequences_Assist.ComputableObjects.BankEnum.Right.ToString Then Return True
            'Dim p As String = parameter.ToString

            Return False
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        If IsNothing(value) Then
            Return Consequences_Assist.ComputableObjects.BankEnum.Right
        Else
            If value = True Then
                Return Consequences_Assist.ComputableObjects.BankEnum.Right
            Else
                Return Consequences_Assist.ComputableObjects.BankEnum.Left
            End If
        End If
    End Function
End Class
Public Class BankEnumToValueConverterLeft
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert
        If IsNothing(value) Then
            'no clue what to do
            Return False
        Else
            If value.ToString = Consequences_Assist.ComputableObjects.BankEnum.Left.ToString Then Return True
            'Dim p As String = parameter.ToString

            Return False
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        If IsNothing(value) Then
            Return Consequences_Assist.ComputableObjects.BankEnum.Right
        Else
            If value = True Then
                Return Consequences_Assist.ComputableObjects.BankEnum.Left
            Else
                Return Consequences_Assist.ComputableObjects.BankEnum.Right
            End If
        End If
    End Function
End Class
Public Class BankEnumToValueConverterBoth
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert
        If IsNothing(value) Then
            'no clue what to do
            Return False
        Else
            If value.ToString = Consequences_Assist.ComputableObjects.BankEnum.Both.ToString Then Return True
            'Dim p As String = parameter.ToString

            Return False
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        If IsNothing(value) Then
            Return Consequences_Assist.ComputableObjects.BankEnum.Right
        Else
            If value = True Then
                Return Consequences_Assist.ComputableObjects.BankEnum.Both
            Else
                Return Consequences_Assist.ComputableObjects.BankEnum.Right
            End If
        End If
    End Function
End Class
