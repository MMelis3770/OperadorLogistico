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
        public async Task CreateAllObjects()
        {
            try
            {
                await CreateHeaderTable();
                await CreateLinesTable();

                await AddHeaderFields();
                await AddLineFields();

                await CreateConfirmationsUDO();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateAllObjects: {ex.Message}");

            }
        }

        private async Task CreateHeaderTable()
        {
            const string tableName = "CONF_ORDERS";
            const string description = "Confirmed Orders Header";

            var response = await _slConnection.Request("UserTablesMD").Filter($"TableName eq '{tableName}'").GetAllAsync<JObject>();

            if (response.Count == 0)
            {
                await _slConnection.Request("UserTablesMD").PostAsync(new JObject
                {
                    ["TableName"] = tableName,
                    ["TableDescription"] = description,
                    ["TableType"] = BoUTBTableType.bott_Document.ToString()
                });
            }
        }

        private async Task CreateLinesTable()
        {
            const string tableName = "CONF_ORDERLINES";
            const string description = "Confirmed Order Lines";

            var response = await _slConnection.Request("UserTablesMD")
                .Filter($"TableName eq '{tableName}'")
                .GetAllAsync<JObject>();

            if (response.Count == 0)
            {
                await _slConnection.Request("UserTablesMD").PostAsync(new JObject
                {
                    ["TableName"] = tableName,
                    ["TableDescription"] = description,
                    ["TableType"] = BoUTBTableType.bott_DocumentLines.ToString()
                });
            }
        }

        private async Task AddHeaderFields()
        {
            const string tableName = "@CONF_ORDERS";

            await CreateUserField(tableName, "DocEntry", "Order ID", BoFieldTypes.db_Numeric, 10,
                BoFldSubTypes.st_None, "ORDR");
            await CreateUserField(tableName, "DocNum", "Order Number", BoFieldTypes.db_Numeric, 10);
            await CreateUserField(tableName, "Status", "Overall Status", BoFieldTypes.db_Alpha, 20,
                validValues: new[] { "Pending", "Confirmed", "Error" });
            await CreateUserField(tableName, "ConfDate", "Confirmation Date", BoFieldTypes.db_Date);
            await CreateUserField(tableName, "ErrorMsg", "Error Message", BoFieldTypes.db_Alpha, 254);
        }

        private async Task AddLineFields()
        {
            const string tableName = "@CONF_ORDERLINES";

            await CreateUserField(tableName, "DocEntry", "Order ID", BoFieldTypes.db_Numeric, 10,
                BoFldSubTypes.st_None, "CONF_ORDERS");
            await CreateUserField(tableName, "LineNum", "Line Number", BoFieldTypes.db_Numeric, 3);
            await CreateUserField(tableName, "ItemCode", "Item Code", BoFieldTypes.db_Alpha, 30,
                BoFldSubTypes.st_None, "OITM");
            await CreateUserField(tableName, "Quantity", "Quantity", BoFieldTypes.db_Float, 10,
                BoFldSubTypes.st_Quantity);
            await CreateUserField(tableName, "LotNumber", "Lot Number", BoFieldTypes.db_Alpha, 50);
            await CreateUserField(tableName, "LineStatus", "Line Status", BoFieldTypes.db_Alpha, 20,
                validValues: new[] { "Pending", "Confirmed", "Error" });
            await CreateUserField(tableName, "ErrorMsg", "Line Error", BoFieldTypes.db_Alpha, 254);
        }

        private async Task CreateConfirmationsUDO()
        {
            const string udoCode = "CONFIRMACIONS";
            const string udoName = "Confirmed Orders";

            var response = await _slConnection.Request("UserObjectsMD")
                .Filter($"Code eq '{udoCode}'")
                .GetAllAsync<JObject>();

            if (response.Count == 0)
            {
                var udoPayload = new JObject
                {
                    ["Code"] = udoCode,
                    ["Name"] = udoName,
                    ["TableName"] = "CONF_ORDERS",
                    ["ObjectType"] = BoUDOObjType.boud_Document.ToString(),
                    ["CanCancel"] = BoYesNoEnum.tYES.ToString(),
                    ["CanClose"] = BoYesNoEnum.tYES.ToString(),
                    ["CanCreateDefaultForm"] = BoYesNoEnum.tYES.ToString(),
                    ["CanDelete"] = BoYesNoEnum.tYES.ToString(),
                    ["CanFind"] = BoYesNoEnum.tYES.ToString(),
                    ["CanLog"] = BoYesNoEnum.tYES.ToString(),
                    ["MenuUID"] = udoCode,
                    ["MenuCaption"] = udoName,
                    ["FatherMenuID"] = "43520", // Menú Finanzas, shaura de canviar pel menu que creem ??
                    ["Position"] = 1
                };

                var childTables = new JArray { "CONF_ORDERLINES" };
                udoPayload["ChildTables"] = childTables;

                var formColumns = new JArray
            {
                JObject.FromObject(new { FormColumnAlias = "DocNum", SonNumber = 0 }),
                JObject.FromObject(new { FormColumnAlias = "CardCode", SonNumber = 1 }),
                JObject.FromObject(new { FormColumnAlias = "Status", SonNumber = 2 })
            };
                udoPayload["FormColumns"] = formColumns;

                await _slConnection.Request("UserObjectsMD").PostAsync(udoPayload);
            }
        }

        private async Task CreateUserField(
            string table,
            string field,
            string description,
            BoFieldTypes type,
            int size = 0,
            BoFldSubTypes subType = BoFldSubTypes.st_None,
            string linkedTable = "",
            string[] validValues = null)
        {
            try
            {
                string tableName = table.Replace("@", "");
                string filter = $"TableName eq '{tableName}' and Name eq '{field}'";

                var response = await _slConnection.Request("UserFieldsMD")
                    .Filter(filter)
                    .GetAllAsync<JObject>();

                if (response.Count == 0)
                {
                    var fieldPayload = new JObject
                    {
                        ["TableName"] = tableName,
                        ["Name"] = field,
                        ["Description"] = description,
                        ["Type"] = type.ToString()
                    };

                    if (size > 0) fieldPayload["EditSize"] = size;
                    if (subType != BoFldSubTypes.st_None) fieldPayload["SubType"] = subType.ToString();
                    if (!string.IsNullOrEmpty(linkedTable)) fieldPayload["LinkedTable"] = linkedTable;

                    if (validValues != null)
                    {
                        var validVals = new JArray();
                        foreach (var val in validValues)
                        {
                            validVals.Add(new JObject
                            {
                                ["Value"] = val,
                                ["Description"] = val
                            });
                        }
                        fieldPayload["ValidValuesMD"] = validVals;
                    }

                    await _slConnection.Request("UserFieldsMD").PostAsync(fieldPayload);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateAllObjects: {ex.Message}");

            }
        }
    }
}
