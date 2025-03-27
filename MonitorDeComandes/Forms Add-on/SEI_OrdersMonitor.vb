Option Explicit On

Imports System.Collections.Generic
Imports System.Linq
Imports System.Net.Http
Imports System.Xml.Linq
Imports SAPbouiCOM
Imports SEI.MonitorDeComandes.SEI_AddonEnum
Imports SEIDOR_SLayer

Public Class SEI_OrdersMonitor
    Inherits SEI_Form

#Region "ATRIBUTOS"

#End Region

#Region "CONSTANTES"
    Private Structure FormControls
        Const grid As String = "grid"
        Const DataTable As String = "DT_Orders"
        Const btnSend As String = "btOrders"
    End Structure


#End Region

#Region "CONSTRUCTOR"
    Public Sub New(ByRef parentaddon As SEI_Addon)

        MyBase.New(parentaddon, enSBO_LoadFormTypes.XmlFile, enAddonFormType.f_OrdersMonitor, enAddonMenus.OrdersMonitor)

        Me.Initialize()

    End Sub

    Private Sub Initialize()
        Try

            LoadOrdersDataInGrid()
            Me.Form.Visible = True

        Catch ex As Exception
            SBO_Application.StatusBar.SetText("Error al cargar la pantalla ‘Monitor de Comandes’", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Me.Form.Close()
        End Try
    End Sub


#End Region

#Region "FUNCIONES EVENTOS"
    Public Overrides Sub HANDLE_DATA_EVENT(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)
    End Sub
    Public Overrides Sub HANDLE_FORM_EVENTS(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        Try
            If Trim(pVal.ItemUID) <> "" Then
                'Eventos de Controles o Items de Formulario
                Select Case pVal.ItemUID
                    'Case FormControls.grid
                    '    HandleGridEvents(pVal, BubbleEvent)

                End Select
            End If
        Catch ex As Exception
            SBO_Application.StatusBar.SetText($"Error en la pantalla: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub
    Public Overrides Sub HANDLE_MENU_EVENTS(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean)
    End Sub
    Public Overrides Sub HANDLE_PRINT_EVENT(ByRef eventInfo As SAPbouiCOM.PrintEventInfo, ByRef BubbleEvent As Boolean)
    End Sub
    Public Overrides Sub HANDLE_REPORT_DATA_EVENT(ByRef eventInfo As SAPbouiCOM.ReportDataInfo, ByRef BubbleEvent As Boolean)
    End Sub
    Public Overrides Sub HANDLE_RIGHTCLICK_EVENT(ByRef eventInfo As SAPbouiCOM.IContextMenuInfo, ByRef BubbleEvent As Boolean)
    End Sub
    Public Overrides Sub HANDLE_LUPA(ByRef CampoLupa As String, ByRef aRetorno As System.Collections.Generic.List(Of System.Collections.ArrayList))
    End Sub
    Public Overrides Sub HANDLE_LAYOUTKEY_EVENTS(ByRef EventInfo As SAPbouiCOM.LayoutKeyInfo, ByRef BubbleEvent As Boolean)
    End Sub
    Public Overrides Sub HANDLE_PROGRESSBAR_EVENTS(ByRef pVal As SAPbouiCOM.IProgressBarEvent, ByRef BubbleEvent As Boolean)
    End Sub
    Public Overrides Sub HANDLE_STATUSBAR_EVENTS(ByRef Text As String, ByRef MessageType As SAPbouiCOM.BoStatusBarMessageType)
    End Sub
    Public Overrides Sub HANDLE_WIDGET_EVENTS(ByRef pWidgetData As SAPbouiCOM.WidgetData, ByRef BubbleEvent As Boolean)
    End Sub
#End Region

#Region "FUNCIONES FORM"




#End Region


#Region "FUNCIONES"
#Region "INICIALIZADORES"

#End Region
#Region "HANDLERS"


#End Region
#Region "FUINCIONES GENERALES"

    Private Sub LoadOrdersDataInGrid()
        Try
            Dim oCombo As SAPbouiCOM.ComboBox = CType(Me.Form.Items.Item("cbOrder").Specific, SAPbouiCOM.ComboBox)
            oCombo.Select("ALL", SAPbouiCOM.BoSearchKey.psk_ByValue)

            Dim oDataTable As SAPbouiCOM.DataTable = Me.Form.DataSources.DataTables.Item(FormControls.DataTable)


            Dim query As String = $"SELECT '' AS Send, T0.DocEntry,T0.DocNum as Document,T0.DocTotal AS DocTotal,T1.SlpName AS SalesEmployee,T0.DocDate As Date,T0.DocDueDate AS DueDate,T0.CardCode AS Client,T0.CardName AS ClientName,T2.DocEntry AS Invoice,'' AS OrderStatus,'' AS OperatorStatus
                                    FROM ORDR T0
                                    INNER JOIN OSLP T1 on T0.SlpCode = T1.SlpCode
                                    LEFT JOIN INV1 T3 on T0.DocEntry = T3.BaseEntry
                                    LEFT JOIN OINV T2 on T3.DocEntry = T2.DocEntry
                                    GROUP BY T0.DocEntry,T0.DocNum ,T0.DocTotal ,T1.SlpName ,T0.DocDate,T0.DocDueDate,T0.CardCode ,T0.CardName ,T2.DocEntry,T0.DocStatus"


            oDataTable.ExecuteQuery(query)

            Dim grid As SAPbouiCOM.Grid = Me.Form.Items.Item(FormControls.grid).Specific
            grid.DataTable = oDataTable

            Dim rowHeaders As SAPbouiCOM.RowHeaders = grid.RowHeaders
            grid.RowHeaders.TitleObject.Caption = "#"

            For i As Integer = 0 To oDataTable.Rows.Count - 1

                rowHeaders.SetText(i, (i + 1).ToString())
            Next

            ConfigurateGridOfOrders(grid)

        Catch ex As Exception
            Throw New Exception("LoadOrdersDataInGrid() > " & ex.Message)
        End Try
    End Sub

    Private Sub ConfigurateGridOfOrders(grid As SAPbouiCOM.Grid)

        Dim xmlDoc = XDocument.Parse(grid.DataTable.SerializeAsXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_All))

        Dim xmlColumns = xmlDoc.Descendants("Column")

        For Each column In xmlColumns
            Dim uid As String = column.Attribute("Uid").Value

            If (uid = "Send") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Send"
                grid.Columns.Item(uid).Type = SAPbouiCOM.BoGridColumnType.gct_CheckBox
                grid.Columns.Item(uid).Editable = True
            End If

            If (uid = "DocEntry") Then
                grid.Columns.Item(uid).TitleObject.Caption = "DocEntry"
                grid.Columns.Item(uid).LinkedObjectType = SAPbobsCOM.BoObjectTypes.oOrders
            End If

            If (uid = "Document") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Document"
            End If

            If (uid = "DocTotal") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Document Total"
            End If

            If (uid = "SalesEmployee") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Sales Employee"
            End If

            If (uid = "Invoice") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Invoice"
            End If

            If (uid = "Date") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Date"
            End If

            If (uid = "DueDate") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Due Date"
            End If

            If (uid = "Client") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Client"
            End If

            If (uid = "ClientName") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Client Name"
            End If

            If (uid = "OrderStatus") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Order Status"
            End If

            If (uid = "OperatorStatus") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Operator Status"
            End If
        Next
        grid.DataTable.LoadSerializedXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly, xmlDoc.ToString())
        For i = 0 To grid.Rows.Count - 1
            grid.CommonSetting.SetRowBackColor(i + 1, -1)
        Next
        grid.AutoResizeColumns()
    End Sub




#End Region
#End Region
End Class