Imports SAPbobsCOM

Public Class SEI_CreateChartOfAccount
#Region "Attributes"
    Protected m_ParentAddon As SEI_Addon
#End Region

#Region "Constructor"
    Public Sub New(ByRef ParentAddon As SEI_Addon)
        m_ParentAddon = ParentAddon
    End Sub
#End Region

    Public Sub CreateChartOfAccounts()

    End Sub

    Private Sub CreateChartOfAccount(code As String,
                                    name As String,
                                    fatherMenu As String,
                                    active As BoYesNoEnum)
        Dim cOA As ChartOfAccounts = m_ParentAddon.SBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oChartOfAccounts)
        If (Not cOA.GetByKey(code)) Then
            With cOA
                .Code = code
                .Name = name
                .FatherAccountKey = fatherMenu
                .ActiveAccount = active
                If .Add <> 0 Then
                    Me.m_ParentAddon.SBO_Application.MessageBox(Me.m_ParentAddon.SBO_Company.GetLastErrorDescription)
                End If
            End With
        End If
        LiberarObjCOM(cOA, True)
    End Sub
End Class
