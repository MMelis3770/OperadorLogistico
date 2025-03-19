Option Explicit On
Option Strict On

Module SubMain
    Public m_SBOAddon As SEI_Addon
    '
    Public Sub Main()
        Try
            Dim bRunApplication As Boolean
            m_SBOAddon = New SEI_SBOAddon(GetAssemblyTitle(), bRunApplication)
            '
            ' Starting the Application
            If bRunApplication = True Then
                System.Windows.Forms.Application.Run()
            Else
                System.Windows.Forms.Application.Exit()
            End If
            '
        Catch ex As Exception
            MsgBox(ex.ToString())
        End Try
    End Sub
    Public Function GetM_SBOAddon() As SEI_Addon
        GetM_SBOAddon = m_SBOAddon
    End Function

    'Move services

End Module