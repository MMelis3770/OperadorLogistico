Option Explicit On

Imports System.Drawing
Imports System.IO
Imports System.Reflection
Imports System.Text

Public Class SEI_Resources
#Region "Embeddeds"
    Public Shared Function GetEmbeddedResource(ByVal p_objTypeForNameSpace As Type, ByVal p_strScriptFileName As String) As String
        Dim s As StringBuilder = New StringBuilder
        Dim ass As [Assembly] = [Assembly].GetAssembly(p_objTypeForNameSpace)
        Dim sr As StreamReader
        '
        sr = New StreamReader(ass.GetManifestResourceStream(p_objTypeForNameSpace, p_strScriptFileName))
        s.Append(sr.ReadToEnd())
        '
        Return s.ToString()
    End Function
    Public Shared Sub GetEmbeddedResourceToStream(ByVal p_objTypeForNameSpace As Type, ByVal p_strFileName As String, ByRef p_objOutputStream As Stream)

        Dim l_objAssemble As [Assembly] = [Assembly].GetAssembly(p_objTypeForNameSpace)
        Dim l_objStreamReader As BinaryReader

        Dim l_strAllResources As String() = l_objAssemble.GetManifestResourceNames()

        l_objStreamReader = New BinaryReader(l_objAssemble.GetManifestResourceStream(p_objTypeForNameSpace, p_strFileName))

        Dim p_objInputStream As Stream = l_objAssemble.GetManifestResourceStream(p_objTypeForNameSpace, p_strFileName)
        p_objInputStream.Position = 0

        Const BUFFER_SIZE As Integer = 2048
        Dim l_objByteBuffer(BUFFER_SIZE) As Byte
        Dim l_intLength As Integer = p_objInputStream.Read(l_objByteBuffer, 0, BUFFER_SIZE)
        While l_intLength > 0
            p_objOutputStream.Write(l_objByteBuffer, 0, l_intLength)
            l_intLength = p_objInputStream.Read(l_objByteBuffer, 0, BUFFER_SIZE)
        End While

    End Sub
    Public Shared Function GetEmbeddedIcon(ByVal strName As String) As Icon
        'If you don't use the name of a resource it can find, 
        'you will get an error saying something about a Null.
        Return New Icon(System.Reflection.Assembly.GetExecutingAssembly.GetManifestResourceStream(strName))
    End Function
#End Region
End Class
