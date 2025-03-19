Imports System.IO
Public Class clsLog
    Public Shared Log As clsLog = New clsLog()
    Private iErrors As Integer
    Public Path As String = ""
    Public PathCompleto As String = ""
    Public FicheroProceso As String = ""
    Public LogBox As Object = Nothing
    Public Sub New()
        Me.iErrors = 0
        Me.FicheroProceso = "LOG" & "_" & Now.ToString("dd_HH_mm_ss")
    End Sub
    Public Sub Info(ByVal sText As String)
        Me.WriteLog(sText, enTypeMessage.sINFO)
    End Sub
    Public Sub Warning(ByVal sText As String)
        Me.WriteLog(sText, enTypeMessage.sWARNING)
    End Sub
    Public Sub ErrorMsg(ByVal sText As String, Optional ByVal bPrint As Boolean = True)
        Me.WriteLog(sText, enTypeMessage.sERROR)
        Me.iErrors += 1
    End Sub
    Private Sub WriteLog(ByVal sMessage As String, ByVal sTypeMessage As String)
        Dim sLine As String = ""
        Dim oFile As StreamWriter = Nothing
        Dim iYear As Integer = DateTime.Now.Year
        Dim iMonth As Integer = DateTime.Now.Month
        '
        If Me.Path = "" Then Exit Sub
        ' If LOG folder doesn't exist we create it
        If Not Directory.Exists(Me.Path) Then Directory.CreateDirectory(Me.Path)
        ' If Year folder doesn't exist we create it 
        If Not Directory.Exists(Me.Path & "\" & iYear.ToString) Then Directory.CreateDirectory(Me.Path & "\" & iYear.ToString)
        ' If the month folder doesn't exist we create it
        If Not Directory.Exists(Me.Path & "\" & iYear.ToString & "\" & iMonth.ToString) Then Directory.CreateDirectory(Me.Path & "\" & iYear.ToString & "\" & iMonth.ToString)
        '
        Me.PathCompleto = Me.Path & "\" & iYear.ToString & "\" & iMonth.ToString & "\" & Me.FicheroProceso & ".txt"
        oFile = File.AppendText(Me.PathCompleto)
        Select Case sTypeMessage
            Case enTypeMessage.sINFO
                sLine = DateTime.Now.ToString("HH:mm:ss") & ": " & sMessage
            Case enTypeMessage.sWARNING
                sLine = DateTime.Now.ToString("HH:mm:ss") & ": [WARNING] - " & sMessage
            Case enTypeMessage.sERROR
                sLine = DateTime.Now.ToString("HH:mm:ss") & ": [ERROR] - " & sMessage
        End Select

        oFile.WriteLine(sLine)
        oFile.Flush()
        oFile.Close()
    End Sub
    Private Structure enTypeMessage
        Const sINFO As String = "INFO"
        Const sWARNING As String = "WARNING"
        Const sERROR As String = "ERROR"
    End Structure
    Public Function HasErrors() As Boolean
        Dim bHasErrors As Boolean = False
        If iErrors <> 0 Then
            bHasErrors = True
        End If
        Return bHasErrors
    End Function
    Public Sub SetLogBox(ByVal LogBox As Object)
        If Not LogBox Is Nothing Then
            Me.LogBox = LogBox
        End If
    End Sub
    Public Sub OpenLogFile()
        Try
            If File.Exists(Me.Path & ".txt") Then
                Shell("Notepad.exe """ & Me.Path & ".txt" & "", AppWinStyle.NormalFocus)
            End If
        Catch ex As Exception
            Throw New Exception("OpenLogFile: " & ex.Message)
        End Try
    End Sub
End Class
