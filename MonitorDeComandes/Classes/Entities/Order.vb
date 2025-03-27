Imports System.Collections.Generic

Public Class Order
    Public Property DocEntry As Integer
    Public Property CardCode As String
    Public Property OrderDate As DateTime
    Public Property DocDueDate As DateTime
    Public Property Lines As List(Of OrderLines)
End Class