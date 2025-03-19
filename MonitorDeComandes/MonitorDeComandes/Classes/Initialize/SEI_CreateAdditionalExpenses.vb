Imports SAPbobsCOM
Imports System.Linq
Imports System.Xml.Linq

Public Class SEI_CreateAdditionalExpenses
#Region "Attributes"
    Protected m_ParentAddon As SEI_Addon
#End Region

#Region "Constructor"
    Public Sub New(ByRef ParentAddon As SEI_Addon)
        m_ParentAddon = ParentAddon
    End Sub
#End Region

    Public Sub CreateAdditionalExpenses()

    End Sub

    Private Sub CreateAdditionalExpense(name As String,
                                        revenueAccount As String,
                                        expenseAccount As String,
                                        stock As SAPbobsCOM.BoYesNoEnum,
                                        Optional distributionMethod As BoAeDistMthd = BoAeDistMthd.aed_None)
        Dim cOA As AdditionalExpenses = m_ParentAddon.SBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oAdditionalExpenses)
        If (Not ExistsAdditionalExpense(name)) Then
            With cOA
                .Name = name
                .RevenuesAccount = revenueAccount
                .ExpenseAccount = expenseAccount
                .Stock = stock
                .DistributionMethod = distributionMethod
                If .Add <> 0 Then
                    Me.m_ParentAddon.SBO_Application.MessageBox(Me.m_ParentAddon.SBO_Company.GetLastErrorDescription)
                End If
            End With
        End If
        LiberarObjCOM(cOA, True)
    End Sub

    Private Function ExistsAdditionalExpense(name As String) As Boolean
        'Dim query = $"SELECT ""ExpnsName"" FROM ""OEXD"" WHERE ""ExpnsName"" = '{name}'"
        'Return m_ParentAddon.oDatabaseConnection.Query(Of Object)(query).Any()
    End Function
End Class
