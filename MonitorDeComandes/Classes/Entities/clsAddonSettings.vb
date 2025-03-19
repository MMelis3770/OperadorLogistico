Option Strict Off
Option Explicit On

Public Class clsAddonSettings
    Public Property UserServer As String
    Public Property PasswordServer As String
    Public Property SLAddress As String
    'Public Property SLUser As String
    'Public Property SLPassword As String
    Public Property SMTPHOST As String
    Public Property SMTPUSER As String
    Public Property SMTPPASSWORD As String
    Public Property SMTPPORT As String
    Public Property SMTPSSL As String
    Public Property SMTPSUBJECT As String
    Public Property SMTPBODY As String
    Public Property SMTPSIGNATURE As String
    Public Sub New()
        Me.UserServer = ""
        Me.PasswordServer = ""
        Me.SLAddress = ""
        'Me.SLUser = ""
        'Me.SLPassword = ""
        Me.SMTPHOST = ""
        Me.SMTPUSER = ""
        Me.SMTPPASSWORD = ""
        Me.SMTPPORT = ""
        Me.SMTPSSL = ""
        Me.SMTPSUBJECT = ""
        Me.SMTPBODY = ""
        Me.SMTPSIGNATURE = ""
    End Sub
End Class
