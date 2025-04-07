using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using CrystalDecisions.CrystalReports.Engine;
using Newtonsoft.Json.Linq;
using SAPbobsCOM;
using SAPbouiCOM;
using SEI.MonitorDeComandes.SEI_AddonEnum;
using SEIDOR_SLayer;

public class SEI_ConfOrders : SEI_Form
{
    #region ATTRIBUTES

    #endregion

    #region CONSTANTS
    private struct FormControls
    {
        // Define your control IDs here
    }
    #endregion

    #region CONSTRUCTOR
    public SEI_ConfOrders(SEI_Addon parentAddon)
        : base(parentAddon, enSBO_LoadFormTypes.XmlFile, enAddonFormType.f_ConfOrders, enAddonMenus.ConfOrders)
    {
        this.Initialize();
    }

    private void Initialize()
    {
        try
        {
            this.Form.Visible = true;
        }
        catch (Exception ex)
        {
            SBO_Application.StatusBar.SetText("Error loading 'Confirmation Orders' screen", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            this.Form.Close();
        }
    }
    #endregion

    #region EVENT FUNCTIONS
    public override void HANDLE_DATA_EVENT(ref BusinessObjectInfo BusinessObjectInfo, ref bool BubbleEvent)
    {
        // Handle business object data events
    }

    public override void HANDLE_FORM_EVENTS(string FormUID, ref ItemEvent pVal, ref bool BubbleEvent)
    {
        try
        {
            if (!string.IsNullOrEmpty(pVal.ItemUID.Trim()))
            {
                // Handle control or form item events
                switch (pVal.ItemUID)
                {
                    //case FormControls.btnSend:
                    //    HandleBtnSend(ref pVal, ref BubbleEvent);
                    //    break;
                }
            }
        }
        catch (Exception ex)
        {
            SBO_Application.StatusBar.SetText($"Screen error: {ex.Message}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
        }
    }

    public override void HANDLE_MENU_EVENTS(ref MenuEvent pVal, ref bool BubbleEvent)
    {
        // Handle menu events
    }

    public override void HANDLE_PRINT_EVENT(ref PrintEventInfo eventInfo, ref bool BubbleEvent)
    {
        // Handle print events
    }

    public override void HANDLE_REPORT_DATA_EVENT(ref ReportDataInfo eventInfo, ref bool BubbleEvent)
    {
        // Handle report data events
    }

    public override void HANDLE_RIGHTCLICK_EVENT(ref IContextMenuInfo eventInfo, ref bool BubbleEvent)
    {
        // Handle right-click events
    }

    public override void HANDLE_LUPA(ref string CampoLupa, ref List<ArrayList> aRetorno)
    {
        // Handle "lupa" (choose from list) events
    }

    public override void HANDLE_LAYOUTKEY_EVENTS(ref LayoutKeyInfo EventInfo, ref bool BubbleEvent)
    {
        // Handle layout key events
    }

    public override void HANDLE_PROGRESSBAR_EVENTS(ref IProgressBarEvent pVal, ref bool BubbleEvent)
    {
        // Handle progress bar events
    }

    public override void HANDLE_STATUSBAR_EVENTS(ref string Text, ref BoStatusBarMessageType MessageType)
    {
        // Handle status bar events
    }

    public override void HANDLE_WIDGET_EVENTS(ref WidgetData pWidgetData, ref bool BubbleEvent)
    {
        // Handle widget events
    }
    #endregion

    #region FORM FUNCTIONS

    #endregion

    #region FUNCTIONS
    #region INITIALIZERS
    // Add initializer methods here

    #endregion

    #region HANDLERS
    // Add event handler methods here

    #endregion

    #region GENERAL FUNCTIONS
    // Helper method to load UDO data
    public void LoadUDOData(string docEntry)
    {
        try
        {
            // Get UDO data using Service Layer connection
            var udo = SBO_Company.GetBusinessObject(BoObjectTypes.BoRecordset) as Recordset;
            string query = $@"
                SELECT * FROM [@CONF_ORDERS] 
                WHERE U_DocEntry = '{docEntry}'";

            udo.DoQuery(query);

            if (!udo.EoF)
            {
                // Populate header data
                string status = udo.Fields.Item("U_Status").Value.ToString();
                DateTime? confDate = null;
                if (udo.Fields.Item("U_ConfDate").Value != null)
                {
                    confDate = DateTime.Parse(udo.Fields.Item("U_ConfDate").Value.ToString());
                }

                // Update form fields
                // Form.Items.Item("txtDocEntry").Specific.Value = docEntry;
                // Form.Items.Item("txtStatus").Specific.Value = status;
                // if (confDate.HasValue)
                // {
                //     Form.Items.Item("dtConfDate").Specific.Value = confDate.Value;
                // }

                // Load detail records
                LoadDetailLines(docEntry);
            }
        }
        catch (Exception ex)
        {