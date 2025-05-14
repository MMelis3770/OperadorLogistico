Imports System.Linq
Imports System.Threading.Tasks
Imports Newtonsoft.Json.Linq
Imports SAPbobsCOM
Imports SEIDOR_SLayer

Public Class SEI_CreateTablesSL

    Public Async Function AddUserTables() As Task

        Await CreateLogErrorUserTable()

        'Await CreateUserDataDefinedField()

        Await CreateUDO()

    End Function

    Private Async Function CreateLogErrorUserTable() As Threading.Tasks.Task

        Const tableName As String = "LogMonitorOrders"

        Const tableDescription As String = "Log Monitor Orders"

        Await CreateUserTables(tableName, tableDescription, BoUTBTableType.bott_NoObjectAutoIncrement)

        Await CreateUserDataField("@" & tableName, "CardCode", "Client ID", BoFieldTypes.db_Numeric, 10)

        Await CreateUserDataField("@" & tableName, "Error", "Error", BoFieldTypes.db_Alpha, 254)

    End Function

    Private Async Function CreateUserDataDefinedField() As Threading.Tasks.Task

        Await CreateUserDataField("ORDR", "OrdersStatus", "OrdersStatus", BoFieldTypes.db_Alpha, 50)

    End Function

    Private Async Function CreateUDO() As Task
        Const tableHeader As String = "CONFORDERS"
        Const headerDescription As String = "Confirmed Orders Header"
        Const tableDetail As String = "CONFORDERLINES"
        Const detailDescription As String = "Confirmed Order Lines"
        Try
            Await CreateUserTables(tableHeader, headerDescription, BoUTBTableType.bott_Document)
            Await CreateUserTables(tableDetail, detailDescription, BoUTBTableType.bott_DocumentLines)

            Await CreateUserDataField("@" & tableHeader, "OrderId", "Order ID (DocEntry)", BoFieldTypes.db_Numeric, 10, linkedSystemObject:=BoObjectTypes.oOrders)
            Await CreateUserDataField("@" & tableHeader, "CardCode", "Customer Code", BoFieldTypes.db_Alpha, 10, linkedSystemObject:=BoObjectTypes.oBusinessPartners)
            Await CreateUserDataField("@" & tableHeader, "DocDate", "Document Date", BoFieldTypes.db_Date)
            Await CreateUserDataField("@" & tableHeader, "DocDueDate", "Delivery Date", BoFieldTypes.db_Date)

            Dim statusCodes As String() = {"C", "E"}
            Dim statusNames As String() = {"Confirmed", "Error"}
            Await CreateUserDataField("@" & tableHeader, "Status", "Status", BoFieldTypes.db_Alpha, 1, codesValidValues:=statusCodes, namesValidValues:=statusNames)
            Await CreateUserDataField("@" & tableHeader, "ErrorMsg", "Error Message", BoFieldTypes.db_Alpha, 254)

            Await CreateUserDataField("@" & tableDetail, "OrderId", "Order ID (DocEntry)", BoFieldTypes.db_Numeric, 10, linkedSystemObject:=BoObjectTypes.oOrders)
            Await CreateUserDataField("@" & tableDetail, "LineNum", "Line Number", BoFieldTypes.db_Numeric, 3)
            Await CreateUserDataField("@" & tableDetail, "ItemCode", "Item Code", BoFieldTypes.db_Alpha, 20)
            Await CreateUserDataField("@" & tableDetail, "Quantity", "Quantity", BoFieldTypes.db_Float, 10, BoFldSubTypes.st_Quantity)
            Await CreateUserDataField("@" & tableDetail, "Batch", "Batch Code", BoFieldTypes.db_Alpha, 50)

            Await CreateUserUDO(
                name:=tableHeader,
                description:="Confirmation Orders",
                logTable:="A" & tableHeader,
                type:=BoUDOObjType.boud_Document,
                canCancel:=BoYesNoEnum.tYES,
                canClose:=BoYesNoEnum.tYES,
                defaultForm:=BoYesNoEnum.tNO,
                canDelete:=BoYesNoEnum.tNO,
                canFind:=BoYesNoEnum.tYES,
                canLog:=BoYesNoEnum.tYES,
                canYearTransfer:=BoYesNoEnum.tNO,
                manageSeries:=BoYesNoEnum.tYES,
                fatherMenu:=Nothing,
                position:=Nothing,                    '
                array_ChildTables:={tableDetail},
                array_FindFields:={"U_OrderId", "U_CardCode"},
                array_FormFields:=Nothing,
                formSRFPath:=Nothing
            )

            Console.WriteLine("UDO creado exitosamente")
        Catch ex As Exception
            Console.WriteLine($"Error creando UDO: {ex.Message}")
            Throw
        End Try
    End Function



    Private Async Function CreateUserTables(name As String, description As String, iType As BoUTBTableType) As Task
        Try
            Dim response = Await m_SBOAddon.oSLConnection.Request("UserTablesMD").Filter($"TableName eq '{name.Replace("@", "")}'").GetAllAsync(Of JObject)()
            If response.Count = 0 Then
                Await m_SBOAddon.oSLConnection.Request("UserTablesMD").PostAsync(New With {
                    Key .TableName = name.Replace("@", ""),
                    Key .TableDescription = description,
                    Key .TableType = iType.ToString()
                })
            End If
        Catch ex As Exception
            Console.WriteLine($"Error creating user table: {ex.Message}")
        End Try

    End Function

    Private Async Function CreateUserDataField(
    table As String,
    field As String,
    description As String,
    type As BoFieldTypes,
    Optional size As Integer = 0,
    Optional subtype As BoFldSubTypes = BoFldSubTypes.st_None,
    Optional defaultValue As String = "",
    Optional codesValidValues As String() = Nothing,
    Optional namesValidValues As String() = Nothing,
    Optional linkedTable As String = "",
    Optional linkedUDO As String = "",
    Optional linkedSystemObject As BoObjectTypes? = Nothing,
    Optional mandatory As BoYesNoEnum = BoYesNoEnum.tNO
) As Task
        Try
            Dim response = Await m_SBOAddon.oSLConnection.Request("UserFieldsMD").Filter($"TableName eq '{table}' and Name eq '{field}'").GetAllAsync(Of Object)()
            If response.Count = 0 Then
                If mandatory = BoYesNoEnum.tYES AndAlso String.IsNullOrEmpty(defaultValue) Then
                    Throw New Exception($"Debe definirse un valor por defecto para el campo '{field}-{description}'")
                End If

                Dim basicField = New With {
                .TableName = table,
                .Name = field,
                .Description = description,
                .Type = type.ToString(),
                .SubType = If(subtype <> BoFldSubTypes.st_None, subtype.ToString(), Nothing),
                .Mandatory = mandatory.ToString(),
                .EditSize = If(size <> 0, size, Nothing),
                .LinkedTable = If(Not String.IsNullOrEmpty(linkedTable), linkedTable, Nothing),
                .LinkedUDO = If(Not String.IsNullOrEmpty(linkedUDO), linkedUDO, Nothing),
                .LinkedSystemObject = If(linkedSystemObject.HasValue AndAlso linkedSystemObject.Value <> BoObjectTypes.BoBridge AndAlso linkedSystemObject.Value <> BoObjectTypes.BoRecordset, linkedSystemObject.ToString(), Nothing),
                .DefaultValue = If(Not String.IsNullOrEmpty(defaultValue), defaultValue, Nothing)
            }

                Console.WriteLine($"Creating field {field} in table {table}")
                Await m_SBOAddon.oSLConnection.Request("UserFieldsMD").PostAsync(basicField)

                If codesValidValues IsNot Nothing AndAlso namesValidValues IsNot Nothing AndAlso codesValidValues.Length > 0 Then
                    Console.WriteLine($"Adding valid values for field {field}")

                    Dim fieldInfo = Await m_SBOAddon.oSLConnection.Request("UserFieldsMD").Filter($"TableName eq '{table}' and Name eq '{field}'").GetAllAsync(Of Object)()
                    If fieldInfo.Count > 0 Then
                        Dim fieldId As String = field

                        For i As Integer = 0 To codesValidValues.Length - 1
                            Try
                                Dim url = $"UserFieldsMD('{table}', '{fieldId}')"

                                Dim validValue = New With {
                                .ValidValues = New Object() {
                                    New With {
                                        .Value = codesValidValues(i),
                                        .Description = namesValidValues(i)
                                    }
                                }
                            }

                                Console.WriteLine($"  Adding value {codesValidValues(i)} - {namesValidValues(i)}")
                                Await m_SBOAddon.oSLConnection.Request(url).PatchAsync(validValue)
                                Console.WriteLine($"  Value added successfully")
                            Catch vvEx As Exception
                                Console.WriteLine($"  Error adding valid value: {vvEx.Message}")
                            End Try
                        Next
                    End If
                End If

                Console.WriteLine($"Field {field} setup completed")
            Else
                Console.WriteLine($"Field {field} already exists in table {table}")
            End If
        Catch ex As Exception
            Console.WriteLine($"Error creating field {field}: {ex.Message}")
            Throw
        End Try
    End Function

    Private Async Function CreateUserUDO(
    name As String,
    description As String,
    logTable As String,
    Optional type As BoUDOObjType = BoUDOObjType.boud_MasterData,
    Optional canCancel As BoYesNoEnum = BoYesNoEnum.tNO,
    Optional canClose As BoYesNoEnum = BoYesNoEnum.tNO,
    Optional defaultForm As BoYesNoEnum = BoYesNoEnum.tNO,
    Optional canDelete As BoYesNoEnum = BoYesNoEnum.tNO,
    Optional canFind As BoYesNoEnum = BoYesNoEnum.tNO,
    Optional canLog As BoYesNoEnum = BoYesNoEnum.tNO,
    Optional canYearTransfer As BoYesNoEnum = BoYesNoEnum.tNO,
    Optional manageSeries As BoYesNoEnum = BoYesNoEnum.tNO,
    Optional fatherMenu As String = Nothing,
    Optional position As Integer = -1,
    Optional array_ChildTables As String() = Nothing,
    Optional array_FindFields As String() = Nothing,
    Optional array_FormFields As String() = Nothing,
    Optional formSRFPath As String = Nothing
) As Task
        name = name.Replace("@", "").ToUpper()
        Dim response = Await m_SBOAddon.oSLConnection.Request("UserObjectsMD").Filter($"Code eq '{name}'").GetAllAsync(Of Object)()
        If response.Count = 0 Then
            Dim findColumnsArray As Object = Nothing
            If canFind = BoYesNoEnum.tYES AndAlso array_FindFields IsNot Nothing Then
                findColumnsArray = array_FindFields.Select(Function(f, i) New With {
                .ColumnNumber = i + 1,
                .ColumnAlias = f,
                .ColumnDescription = f,
                .Code = name
            }).ToList()
            End If

            Dim userObject = New With {
            .CanCancel = canCancel.ToString(),
            .CanClose = canClose.ToString(),
            .CanCreateDefaultForm = defaultForm.ToString(),
            .CanDelete = canDelete.ToString(),
            .CanFind = canFind.ToString(),
            .CanLog = canLog.ToString(),
            .CanYearTransfer = canYearTransfer.ToString(),
            .Code = name,
            .TableName = name,
            .LogTableName = logTable,
            .ObjectType = type.ToString(),
            .Name = description,
            .ManageSeries = manageSeries.ToString(),
            .UseUniqueFormType = BoYesNoEnum.tYES.ToString(),
            .UserObjectMD_ChildTables = If(array_ChildTables IsNot Nothing, array_ChildTables.Select(Function(t) New With {.TableName = t}).ToList(), Nothing),
            .UserObjectMD_FindColumns = findColumnsArray,
            .FormColumns = If(defaultForm = BoYesNoEnum.tYES AndAlso array_FormFields IsNot Nothing, array_FormFields.Where(Function(f) Not f.Trim().ToUpper().Equals("CODE")).Select(Function(f) New With {.FormColumnAlias = f}).Prepend(New With {.FormColumnAlias = "Code"}).ToList(), Nothing),
            .FormSRF = If(Not String.IsNullOrEmpty(formSRFPath), formSRFPath.Replace("\", "/"), Nothing),
            .EnableEnhancedForm = BoYesNoEnum.tNO.ToString(),
            .MenuItem = BoYesNoEnum.tYES.ToString(),
            .MenuCaption = description,
            .MenuUID = name,
            .FatherMenuID = fatherMenu,
            .Position = position
        }

            Await m_SBOAddon.oSLConnection.Request("UserObjectsMD").PostAsync(userObject)
            Console.WriteLine($"UDO {name} created successfully")
        Else
            Console.WriteLine($"UDO {name} already exists")
        End If
    End Function

End Class