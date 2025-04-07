using Newtonsoft.Json.Linq;
using SAPbobsCOM;
using SEIDOR_SLayer;

namespace SeidorSmallWebApi.Initialize
{
    public class SEI_CreateTablesSL
    {
        private readonly SLConnection _slConnection;

        public SEI_CreateTablesSL(SLConnection slConnection)
        {
            _slConnection = slConnection;
        }

        public async Task AddUserTables()
        {
            try
            {
                Console.WriteLine("Creating user tables and fields...");

                await CreateUDO();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user tables: {ex.Message}");
            }
        }
        public async Task CreateUDO()
        {
            const string tableHeader = "CONF_ORDERS";
            const string headerDescription = "Confirmed Orders Header";

            const string tableDetail = "CONF_ORDERLINES";
            const string detailDescription = "Confirmed Order Lines";

            // Create tables
            await AddUserTable(tableHeader, headerDescription, BoUTBTableType.bott_Document);
            await AddUserTable(tableDetail, detailDescription, BoUTBTableType.bott_DocumentLines);

            // Add header fields
            await AddUserField(tableHeader, "DocEntry", "Order ID", BoFieldTypes.db_Date);
            await AddUserField(tableHeader, "DocNum", "Order Number", BoFieldTypes.db_Numeric, 10);

            string[] statusCodes = { "P", "C", "E" };
            string[] statusNames = { "Pending", "Confirmed", "Error" };
            await AddUserField(tableHeader, "Status", "Overall Status", BoFieldTypes.db_Alpha, 10,
                defaultValue: "P", codesValidValues: statusCodes, namesValidValues: statusNames);

            await AddUserField(tableHeader, "ConfDate", "Confirmation Date", BoFieldTypes.db_Date);
            await AddUserField(tableHeader, "ErrorMsg", "Error Message", BoFieldTypes.db_Alpha, 254);

            // Add detail fields
            await AddUserField(tableDetail, "DocEntry", "Order ID", BoFieldTypes.db_Numeric, 10);
            await AddUserField(tableDetail, "LineNum", "Line Number", BoFieldTypes.db_Numeric, 3);
            await AddUserField(tableDetail, "ItemCode", "Item Code", BoFieldTypes.db_Alpha, 8, linkedTable: "OITM");
            await AddUserField(tableDetail, "Quantity", "Quantity", BoFieldTypes.db_Float, 8,
                subType: BoFldSubTypes.st_Quantity, defaultValue: "0");
            await AddUserField(tableDetail, "LotNumber", "Lot Number", BoFieldTypes.db_Alpha, 50);
            await AddUserField(tableDetail, "LineStatus", "Line Status", BoFieldTypes.db_Alpha, 20,
                defaultValue: "P", codesValidValues: statusCodes, namesValidValues: statusNames);
            await AddUserField(tableDetail, "ErrorMsg", "Line Error", BoFieldTypes.db_Alpha, 254);

            // Create form fields for UDO
            var formFields = new List<(string FieldName, int SonNumber)>
            {
                ("U_DocEntry", 0),
                ("U_DocNum", 0),
                ("U_Status", 0),
                ("U_ConfDate", 0),
                ("U_ErrorMsg", 0),
                ("U_DocEntry", 1),
                ("U_LineNum", 1),
                ("U_Quantity", 1),
                ("U_LotNumber", 1),
                ("U_LineStatus", 1),
                ("U_ErrorMsg", 1)
            };

            // Create find fields for UDO
            string[] findFields =
            {
                "U_DocEntry",
                "U_DocNum",
                "U_Status"
            };

            // Create UDO
            await AddUserUDO(
                tableHeader,
                "Confirmation Orders",
                "A" + tableHeader,
                BoUDOObjType.boud_Document,
                BoYesNoEnum.tYES,
                BoYesNoEnum.tYES,
                BoYesNoEnum.tYES,
                BoYesNoEnum.tYES,
                BoYesNoEnum.tYES,
                BoYesNoEnum.tYES,
                BoYesNoEnum.tNO,
                BoYesNoEnum.tNO,
                "8765",
                -1,
                new string[] { tableDetail },
                findFields,
                formFields,
                "C:\\Users\\mcorder\\OneDrive - SEIDOR SOLUTIONS S.L\\PFG\\OperadorLogistico\\SeidorSmallWebApi\\Initialize\\SEI_ConfOrders.srf" // Add your SRF file path here
            );
        }

        private async Task AddUserTable(string name, string description, BoUTBTableType tableType)
        {
            name = name.Replace("@", "");

            var response = await _slConnection.Request("UserTablesMD")
                .Filter($"TableName eq '{name}'")
                .GetAllAsync<JObject>();

            if (response.Count == 0)
            {
                await _slConnection.Request("UserTablesMD").PostAsync(new JObject
                {
                    ["TableName"] = name,
                    ["TableDescription"] = description,
                    ["TableType"] = tableType.ToString()
                });
            }
        }

        private async Task AddUserField(
            string tableName,
            string fieldName,
            string description,
            BoFieldTypes type,
            int size = 0,
            BoFldSubTypes subType = BoFldSubTypes.st_None,
            string defaultValue = "",
            string[] codesValidValues = null,
            string[] namesValidValues = null,
            string linkedTable = "",
            string linkedUDO = "",
            BoObjectTypes linkedSystemObject = 0,
            BoYesNoEnum mandatory = BoYesNoEnum.tNO)
        {
            try
            {
                tableName = tableName.Replace("@", "");
                string filter = $"TableName eq '{tableName}' and Name eq '{fieldName}'";

                var response = await _slConnection.Request("UserFieldsMD")
                    .Filter(filter)
                    .GetAllAsync<JObject>();

                if (response.Count == 0)
                {
                    // Check if mandatory field requires default value
                    if (mandatory == BoYesNoEnum.tYES && string.IsNullOrEmpty(defaultValue))
                    {
                        Console.WriteLine($"Default value must be defined for mandatory field '{fieldName}-{description}'");
                        return;
                    }

                    var fieldPayload = new JObject
                    {
                        ["TableName"] = tableName,
                        ["Name"] = fieldName,
                        ["Description"] = description,
                        ["Type"] = type.ToString()
                    };

                    // Add optional properties
                    if (subType != BoFldSubTypes.st_None)
                        fieldPayload["SubType"] = subType.ToString();

                    if (mandatory == BoYesNoEnum.tYES)
                        fieldPayload["Mandatory"] = mandatory.ToString();

                    if (size > 0)
                        fieldPayload["EditSize"] = size;

                    if (!string.IsNullOrEmpty(linkedTable))
                        fieldPayload["LinkedTable"] = linkedTable;

                    if (!string.IsNullOrEmpty(linkedUDO))
                        fieldPayload["LinkedUDO"] = linkedUDO;

                    if (linkedSystemObject != 0 &&
                        linkedSystemObject != BoObjectTypes.BoBridge &&
                        linkedSystemObject != BoObjectTypes.BoRecordset)
                        fieldPayload["LinkedSystemObject"] = linkedSystemObject.ToString();

                    if (!string.IsNullOrEmpty(defaultValue))
                        fieldPayload["DefaultValue"] = defaultValue;

                    // Add valid values if provided
                    if (codesValidValues != null && namesValidValues != null)
                    {
                        var validVals = new JArray();
                        for (int i = 0; i < codesValidValues.Length; i++)
                        {
                            validVals.Add(new JObject
                            {
                                ["Value"] = codesValidValues[i],
                                ["Description"] = namesValidValues[i]
                            });
                        }
                        fieldPayload["ValidValuesMD"] = validVals;
                    }

                    // Create the field
                    await _slConnection.Request("UserFieldsMD").PostAsync(fieldPayload);
                }
                else if (codesValidValues != null && namesValidValues != null)
                {
                    // Check if we need to update valid values
                    var fieldId = response[0]["FieldID"].ToString();

                    // Get existing valid values
                    var existingValuesResponse = await _slConnection.Request("UserFieldsMD")
                        .Filter($"TableName eq '{tableName}' and FieldID eq {fieldId}")
                        .GetAsync<JObject>();

                    var existingValues = existingValuesResponse["ValidValuesMD"] as JArray;
                    var existingValuesDict = new Dictionary<string, bool>();

                    if (existingValues != null)
                    {
                        foreach (var val in existingValues)
                        {
                            existingValuesDict[val["Value"].ToString()] = true;
                        }
                    }

                    // Check if we need to add new values
                    bool needsUpdate = false;
                    for (int i = 0; i < codesValidValues.Length; i++)
                    {
                        if (!existingValuesDict.ContainsKey(codesValidValues[i]))
                        {
                            needsUpdate = true;
                            break;
                        }
                    }

                    if (needsUpdate)
                    {
                        // Create update payload with all valid values
                        var updatePayload = new JObject
                        {
                            ["TableName"] = tableName,
                            ["FieldID"] = int.Parse(fieldId)
                        };

                        var validVals = new JArray();
                        for (int i = 0; i < codesValidValues.Length; i++)
                        {
                            validVals.Add(new JObject
                            {
                                ["Value"] = codesValidValues[i],
                                ["Description"] = namesValidValues[i]
                            });
                        }
                        updatePayload["ValidValuesMD"] = validVals;

                        // Update the field
                        await _slConnection.Request("UserFieldsMD").PatchAsync(updatePayload);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding user field {fieldName} to {tableName}: {ex.Message}");
            }
        }

        private async Task AddUserUDO(
    string name,
    string description,
    string logTable,
    BoUDOObjType type,
    BoYesNoEnum canCancel,
    BoYesNoEnum canClose,
    BoYesNoEnum defaultForm,
    BoYesNoEnum canDelete,
    BoYesNoEnum canFind,
    BoYesNoEnum canLog,
    BoYesNoEnum canYearTransfer,
    BoYesNoEnum manageSeries,
    string fatherMenu,
    int position,
    string[] childTables = null,
    string[] findFields = null,
    List<(string FieldName, int SonNumber)> formFields = null,
    string customSrfPath = null)
        {
            name = name.Replace("@", "");

            var response = await _slConnection.Request("UserObjectsMD")
                .Filter($"Code eq '{name}'")
                .GetAllAsync<JObject>();

            if (response.Count == 0)
            {
                var udoPayload = new JObject
                {
                    ["Code"] = name,
                    ["Name"] = description,
                    ["TableName"] = name,
                    ["ObjectType"] = type.ToString(),
                    ["CanCancel"] = canCancel.ToString(),
                    ["CanClose"] = canClose.ToString(),
                    ["CanCreateDefaultForm"] = (string.IsNullOrEmpty(customSrfPath) ? defaultForm : BoYesNoEnum.tNO).ToString(),
                    ["CanDelete"] = canDelete.ToString(),
                    ["CanFind"] = canFind.ToString(),
                    ["CanLog"] = canLog.ToString(),
                    ["CanYearTransfer"] = canYearTransfer.ToString(),
                    ["ManageSeries"] = manageSeries.ToString(),
                    ["UseUniqueFormType"] = BoYesNoEnum.tYES.ToString()
                };

                // Add custom SRF form path if provided
                if (!string.IsNullOrEmpty(customSrfPath))
                {
                    udoPayload["FormSRF"] = customSrfPath;
                }

                // Add child tables if provided
                if (childTables != null && childTables.Length > 0)
                {
                    var childTablesArray = new JArray();
                    foreach (var childTable in childTables)
                    {
                        childTablesArray.Add(new JObject
                        {
                            ["TableName"] = childTable
                        });
                    }
                    udoPayload["ChildTables"] = childTablesArray;
                }

                // Add find fields if provided and can find is enabled
                if (canFind == BoYesNoEnum.tYES && findFields != null && findFields.Length > 0)
                {
                    var findColumnsArray = new JArray();
                    foreach (var findField in findFields)
                    {
                        findColumnsArray.Add(new JObject
                        {
                            ["ColumnAlias"] = findField
                        });
                    }
                    udoPayload["FindColumns"] = findColumnsArray;
                }

                // Add form fields if provided and either default form is enabled or custom SRF is provided
                if ((defaultForm == BoYesNoEnum.tYES || !string.IsNullOrEmpty(customSrfPath)) && formFields != null && formFields.Count > 0)
                {
                    var formColumnsArray = new JArray();

                    // Add DocEntry field first
                    formColumnsArray.Add(new JObject
                    {
                        ["FormColumnAlias"] = "DocEntry",
                        ["SonNumber"] = 0
                    });

                    // Add other form fields
                    foreach (var field in formFields)
                    {
                        if (field.FieldName.Trim().ToUpper() != "DOCENTRY")
                        {
                            formColumnsArray.Add(new JObject
                            {
                                ["FormColumnAlias"] = field.FieldName,
                                ["SonNumber"] = field.SonNumber
                            });
                        }
                    }
                    udoPayload["FormColumns"] = formColumnsArray;

                    // Add menu properties
                    udoPayload["EnableEnhancedForm"] = BoYesNoEnum.tNO.ToString();
                    udoPayload["MenuItem"] = BoYesNoEnum.tYES.ToString();
                    udoPayload["MenuCaption"] = description;
                    udoPayload["MenuUID"] = name;

                    if (!string.IsNullOrEmpty(fatherMenu))
                        udoPayload["FatherMenuID"] = fatherMenu;

                    if (position != -1)
                        udoPayload["Position"] = position;
                }

                await _slConnection.Request("UserObjectsMD").PostAsync(udoPayload);
            }
        }

        private async Task AddUserKey(
            string tableName,
            string keyName,
            BoYesNoEnum unique,
            string[] fields)
        {
            tableName = tableName.Replace("@", "");

            var response = await _slConnection.Request("UserKeysMD")
                .Filter($"TableName eq '{tableName}' and KeyName eq '{keyName}'")
                .GetAllAsync<JObject>();

            if (response.Count == 0)
            {
                var keyPayload = new JObject
                {
                    ["TableName"] = tableName,
                    ["KeyName"] = keyName,
                    ["Unique"] = unique.ToString()
                };

                var elements = new JArray();
                foreach (var field in fields)
                {
                    elements.Add(new JObject
                    {
                        ["ColumnAlias"] = field.Replace("U_", "")
                    });
                }
                keyPayload["Elements"] = elements;

                await _slConnection.Request("UserKeysMD").PostAsync(keyPayload);
            }
        }
    }
}