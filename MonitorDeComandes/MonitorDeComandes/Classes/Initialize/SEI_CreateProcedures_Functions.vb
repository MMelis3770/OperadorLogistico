Imports System.Linq
Imports DatabaseConnection

Public Class SEI_CreateProcedures_Functions
    Private oDatabaseConnection As IDatabaseConnection
    Private oAddon As SEI_Addon

    Public Sub New(dbConnection As IDatabaseConnection, addon As SEI_Addon)
        oDatabaseConnection = dbConnection
        oAddon = addon
    End Sub

    Public Sub CreateProcedures()

    End Sub

#Region "Procedures"
    Private Sub CreateProcedure(name As String, procedure As String)
        Dim query As String
        If (oAddon.SBO_Company.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_HANADB) Then
            query = $"SELECT * FROM ""SYS"".""PROCEDURES"" WHERE ""SCHEMA_NAME"" = '{oAddon.SBO_Company.CompanyDB}' AND ""PROCEDURE_NAME"" = '{name}'"
        Else
            query = $"SELECT * FROM sys.objects WHERE type = 'P' AND name = '{name}'"
        End If
        Dim result = oDatabaseConnection.Query(Of Object)(query)
        If (result.Count = 0) Then
            oDatabaseConnection.Execute(procedure)
        End If
    End Sub
#End Region
#Region "Functions"
    Private Sub CreateFunction(name As String, sFunction As String)
        Dim query As String
        If (oAddon.SBO_Company.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_HANADB) Then
            query = $"SELECT * FROM ""SYS"".""FUNCTIONS"" WHERE ""DEFAULT_SCHEMA_NAME"" = '{oAddon.SBO_Company.CompanyDB}' AND ""FUNCTION_NAME"" = '{name.ToUpper()}'"
        Else
            query = $"SELECT * FROM sys.objects WHERE type = 'TF' AND name = '{name}'"
        End If
        Dim result = oDatabaseConnection.Query(Of Object)(query)
        If (result.Count = 0) Then
            oDatabaseConnection.Execute(sFunction)
        End If
    End Sub
#End Region
End Class
