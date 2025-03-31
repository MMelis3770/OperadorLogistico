Imports System.Threading.Tasks
Imports Newtonsoft.Json.Linq
Imports SAPbobsCOM
Imports SEIDOR_SLayer

Public Class SEI_CreateTablesSL


    Public Async Function AddUserTablesSL() As Threading.Tasks.Task
        Await CreateUserDefinedField()
    End Function

    Private Async Function CreateUserDefinedField() As Threading.Tasks.Task

        Await CreateUserDataField("ORDR", "OrdersStatus", "OrdersStatus", BoFieldTypes.db_Alpha, 50, "Pending")
        Await CreateUserDataField("ORDR", "OperatorStatus", "OrdersStatus", BoFieldTypes.db_Alpha, 50)
    End Function

    Private Async Function CreateUserDataField(table As String, field As String, description As String, type As BoFieldTypes, Optional size As Integer = 0, Optional defaultValue As String = "") As Task
        Try
            Dim response = Await m_SBOAddon.oSLConnection.Request("UserFieldsMD").Filter($"TableName eq '{table}' and Name eq '{field}'").GetAllAsync(Of JObject)()
            If response.Count = 0 Then
                Dim userField = New With {
                    Key .TableName = table,
                    Key .Name = field,
                    Key .Description = description,
                    Key .Type = type.ToString(),
                    Key .EditSize = If(size <> 0, CType(size, Integer?), CType(Nothing, Integer?)),
                    Key .DefaultValue = If(String.IsNullOrEmpty(defaultValue), CType(Nothing, String), defaultValue)
                }

                Await m_SBOAddon.oSLConnection.Request("UserFieldsMD").PostAsync(userField)
            End If
        Catch ex As Exception
            Console.WriteLine($"Error creating user table: {ex.Message}")
        End Try

    End Function
End Class
