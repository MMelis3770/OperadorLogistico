Imports System.Collections.Generic

Public Class Delivery
    Public Property CardCode As String
    Public Property DocDate As String
    Public Property DocDueDate As String
    Public Property DocEntry As Integer
    Public Property Comments As String
    Public Property WarehouseCode As String
    Public Property DocumentLines As List(Of DeliveryLines)
End Class
