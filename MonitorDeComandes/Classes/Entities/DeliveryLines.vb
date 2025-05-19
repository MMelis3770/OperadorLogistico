Imports System.Collections.Generic

Public Class DeliveryLines
    Public Property BaseType As Integer = 17
    Public Property BaseEntry As Integer
    Public Property BaseLine As Integer
    Public Property ItemCode As String
    Public Property lineNum As Integer
    Public Property Quantity As Double
    Public Property BatchNumbers As List(Of BatchNumber)
End Class
