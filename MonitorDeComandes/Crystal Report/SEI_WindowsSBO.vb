Imports System.Diagnostics

Public Class SEI_WindowsSbo
    Implements System.Windows.Forms.IWin32Window
    Private _hwnd As IntPtr
    Public Sub New(ByVal handle As IntPtr)
        Me._hwnd = handle
    End Sub

    Public Sub New(ByRef oSEI_AddOn As SEI_Addon)
        Dim MyProcs() As Process
        Dim i As Integer
        MyProcs = Process.GetProcessesByName("SAP Business One")
        For i = 0 To UBound(MyProcs)
            If InStr(MyProcs(i).MainWindowTitle, oSEI_AddOn.SBO_Company.CompanyName) <> 0 Then
                Me._hwnd = MyProcs(i).MainWindowHandle
                Exit For
            End If
        Next
    End Sub
    Public ReadOnly Property Handle() As System.IntPtr Implements System.Windows.Forms.IWin32Window.Handle
        Get
            Return _hwnd
        End Get
    End Property
End Class
