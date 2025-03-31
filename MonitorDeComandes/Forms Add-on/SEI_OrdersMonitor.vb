Imports System.Collections.Generic
Imports System.Data
Imports System.IO
Imports System.Linq
Imports System.Net.Http
Imports System.Threading.Tasks
Imports System.Xml.Linq
Imports CrystalDecisions.CrystalReports.Engine
Imports Newtonsoft.Json.Linq
Imports SAPbobsCOM
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
        Const btnCreateInvoice As String = "btInvoice"
        Const btnCreateDelivery As String = "btDelivery"
        Const btnFilter As String = "btFilter"
        Const cbOrderSt As String = "cbOrder"
        Const cbOperatorSt As String = "cbOper"
        Const etClient As String = "etClient"
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
                    Case FormControls.btnSend
                        HandleBtnSend(pVal, BubbleEvent)
                    Case FormControls.btnCreateInvoice
                        HandleBtnCreateInvoices(pVal, BubbleEvent)
                    Case FormControls.btnCreateDelivery
                        HandleBtnCreateDeliveries(pVal, BubbleEvent)
                    Case FormControls.btnFilter
                        HandleBtnFilter(pVal, BubbleEvent)
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

    Private Sub HandleBtnFilter(pVal As ItemEvent, ByRef BubbleEvent As Boolean)

        If pVal.EventType <> BoEventTypes.et_ITEM_PRESSED Then Exit Sub

        If Not pVal.BeforeAction Then

            Dim oDataTable As SAPbouiCOM.DataTable = Me.Form.DataSources.DataTables.Item(FormControls.DataTable)

            Dim cbOrderSt As ComboBox = Me.Form.Items.Item(FormControls.cbOrderSt).Specific
            Dim cbOperatorSt As ComboBox = Me.Form.Items.Item(FormControls.cbOperatorSt).Specific
            Dim etClient As EditText = Me.Form.Items.Item(FormControls.etClient).Specific


            Dim orderStatus As String = If(cbOrderSt.Selected IsNot Nothing, cbOrderSt.Selected.Value, "").Trim()
            Dim operatorStatus As String = If(cbOperatorSt.Selected IsNot Nothing, cbOperatorSt.Selected.Value, "").Trim()
            Dim client As String = If(etClient IsNot Nothing, etClient.Value.Trim(), "")


            Dim query As String = $"SELECT '' AS Send, T0.DocEntry,T0.DocNum as Document,T0.DocTotal AS DocTotal,T1.SlpName AS SalesEmployee,T0.DocDate As Date,T0.DocDueDate AS DueDate,T0.CardCode AS Client,T0.CardName AS ClientName,T2.DocEntry AS Invoice,T0.U_OrdersStatus AS OrderStatus,T0.U_OperatorStatus AS OperatorStatus
                                    From ORDR T0
                                    INNER Join OSLP T1 on T0.SlpCode = T1.SlpCode
                                    Left Join INV1 T3 on T0.DocEntry = T3.BaseEntry
                                    Left Join OINV T2 on T3.DocEntry = T2.DocEntry
                                    WHERE 1=1"


            If orderStatus <> "All" AndAlso orderStatus <> "" Then
                If orderStatus = "Pending" Then
                    query &= " AND T0.U_OrdersStatus IS NULL"
                Else
                    query &= " AND T0.U_OrdersStatus = '" & orderStatus & "'"
                End If
            End If

            If operatorStatus <> "" Then
                query &= " AND U_OperatorStatus = '" & operatorStatus & "'"
            End If

            If client <> "" Then
                query &= " AND T0.CardCode = '" & client & "'"
            End If

            query &= " GROUP BY T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, T2.DocEntry, T0.U_OrdersStatus, T0.U_OperatorStatus"


            oDataTable.ExecuteQuery(query)

            Dim grid As SAPbouiCOM.Grid = Me.Form.Items.Item(FormControls.grid).Specific
            grid.DataTable = oDataTable

            Dim rowHeaders As SAPbouiCOM.RowHeaders = grid.RowHeaders
            grid.RowHeaders.TitleObject.Caption = "#"

            For i As Integer = 0 To oDataTable.Rows.Count - 1

                rowHeaders.SetText(i, (i + 1).ToString())
            Next

            ConfigurateGridOfOrders(grid)


        End If
    End Sub

    Private Sub HandleBtnSend(pVal As ItemEvent, ByRef BubbleEvent As Boolean)
        If pVal.EventType <> BoEventTypes.et_ITEM_PRESSED Then Exit Sub
        If Not pVal.BeforeAction Then
            Try

                Dim grid As SAPbouiCOM.Grid = Me.Form.Items.Item(FormControls.grid).Specific
                Dim basePath As String = "C:\DataTXT\"
                Dim isOrderSelected As Boolean = False

                If Not Directory.Exists(basePath) Then
                    Directory.CreateDirectory(basePath)
                End If


                For i As Integer = 0 To grid.Rows.Count - 1
                    If grid.DataTable.GetValue("Send", i).ToString() = "Y" Then
                        isOrderSelected = True
                        Dim docEntry As Integer = grid.DataTable.GetValue("DocEntry", i)

                        Dim order As Order = GetOrder(docEntry)
                        If order Is Nothing Then
                            Throw New Exception("Order not found")
                        End If

                        Dim filePath As String = $"{basePath}Order_{docEntry}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.txt"
                        GenerateOrderTxt(order, filePath)
                        PatchOrder(docEntry, "Sent").Wait()

                    End If
                Next


                If Not isOrderSelected Then
                    SBO_Application.StatusBar.SetText("No order selected. Please select at least one order to send.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    Exit Sub
                End If

                LoadOrdersDataInGrid()


            Catch ex As Exception
                SBO_Application.StatusBar.SetText($"Error: {ex.Message}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                BubbleEvent = False
            End Try
        End If

    End Sub


    Private Sub HandleBtnCreateInvoices(pVal As ItemEvent, ByRef BubbleEvent As Boolean)
        If pVal.EventType <> BoEventTypes.et_ITEM_PRESSED Then Exit Sub
        If Not pVal.BeforeAction Then
            Try
                Dim grid As SAPbouiCOM.Grid = Me.Form.Items.Item(FormControls.grid).Specific
                Dim isOrderSelected As Boolean = False

                For i As Integer = 0 To grid.Rows.Count - 1

                    Dim sendChecked As String = grid.DataTable.GetValue("Send", i)
                    Dim docEntry As Integer = grid.DataTable.GetValue("DocEntry", i)
                    Dim orderStatus As Integer = grid.DataTable.GetValue("OrderStatus", i)

                    If sendChecked = "Y" And orderStatus = "Delivered" Then
                        isOrderSelected = True
                        'Do post invoice
                        PatchOrder(docEntry, "Invoiced").Wait()
                    ElseIf sendChecked = "Y" And orderStatus <> "Delivered" Then
                        SBO_Application.StatusBar.SetText("You cannot create an invoice, there is no delivery created.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    End If
                Next
                If Not isOrderSelected Then
                    SBO_Application.StatusBar.SetText("No order selected. Please select at least one order to create it's invoice.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    Exit Sub
                End If

                LoadOrdersDataInGrid()
            Catch ex As Exception
                SBO_Application.StatusBar.SetText($"{ex.Message}")
            End Try

        End If
    End Sub


    Private Sub HandleBtnCreateDeliveries(pVal As ItemEvent, ByRef BubbleEvent As Boolean)
        If pVal.EventType <> BoEventTypes.et_ITEM_PRESSED Then Exit Sub
        If Not pVal.BeforeAction Then
            Try
                Dim grid As SAPbouiCOM.Grid = Me.Form.Items.Item(FormControls.grid).Specific
                Dim isOrderSelected As Boolean = False
                For i As Integer = 0 To grid.Rows.Count - 1

                    Dim sendChecked As String = grid.DataTable.GetValue("Send", i)
                    Dim docEntry As Integer = grid.DataTable.GetValue("DocEntry", i)
                    Dim operatorStatus As Integer = grid.DataTable.GetValue("OperatorStatus", i)

                    If sendChecked = "Y" And operatorStatus = "C" Then
                        isOrderSelected = True
                        'Do post delivery
                        PatchOrder(docEntry, "Delivered").Wait()
                    ElseIf sendChecked = "Y" And operatorStatus <> "C" Then
                        SBO_Application.StatusBar.SetText("You cannot create a delivery, the order is not confirmed.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    End If

                Next

                If Not isOrderSelected Then
                    SBO_Application.StatusBar.SetText("No order selected. Please select at least one order to create it's delivery.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    Exit Sub
                End If
                LoadOrdersDataInGrid()
            Catch ex As Exception
                SBO_Application.StatusBar.SetText($"{ex.Message}")
            End Try

        End If
    End Sub
#End Region

#Region "FUINCIONES GENERALES"

    Private Async Function PatchOrder(DocEntry As Integer, OrderStatus As String) As Task
        Try
            Dim updatedData As New With {
                    Key .U_OrdersStatus = OrderStatus
                    }
            Await m_SBOAddon.oSLConnection.Request($"Orders({DocEntry})").PatchAsync(updatedData)

        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try

    End Function

    Private Sub LoadOrdersDataInGrid()
        Try
            Dim oCombo As SAPbouiCOM.ComboBox = CType(Me.Form.Items.Item("cbOrder").Specific, SAPbouiCOM.ComboBox)
            oCombo.Select("ALL", SAPbouiCOM.BoSearchKey.psk_ByValue)

            Dim oDataTable As SAPbouiCOM.DataTable = Me.Form.DataSources.DataTables.Item(FormControls.DataTable)


            Dim query As String = $"SELECT '' AS Send, T0.DocEntry,T0.DocNum as Document,T0.DocTotal AS DocTotal,T1.SlpName AS SalesEmployee,T0.DocDate As Date,T0.DocDueDate AS DueDate,T0.CardCode AS Client,T0.CardName AS ClientName,T2.DocEntry AS Invoice,T0.U_OrdersStatus AS OrderStatus,T0.U_OperatorStatus AS OperatorStatus
                                    FROM ORDR T0
                                    INNER JOIN OSLP T1 on T0.SlpCode = T1.SlpCode
                                    LEFT JOIN INV1 T3 on T0.DocEntry = T3.BaseEntry
                                    LEFT JOIN OINV T2 on T3.DocEntry = T2.DocEntry
                                    GROUP BY T0.DocEntry,T0.DocNum ,T0.DocTotal ,T1.SlpName ,T0.DocDate,T0.DocDueDate,T0.CardCode ,T0.CardName ,T2.DocEntry,T0.U_OrdersStatus,T0.U_OperatorStatus"


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


    Private Function GetOrder(docEntry As Integer) As Order
        Try
            Dim serviceLayer As SLConnection = m_SBOAddon.oSLConnection
            Dim response = serviceLayer.Request($"Orders({docEntry})").GetAsync(Of JObject)().Result()

            If response Is Nothing OrElse response Is Nothing Then
                Return Nothing
            End If

            Dim order = New Order With {
         .DocEntry = CInt(response("DocEntry")),
         .CardCode = response("CardCode")?.ToString(),
         .OrderDate = If(response("DocDate") IsNot Nothing,
                       DateTime.Parse(response("DocDate").ToString()),
                       DateTime.MinValue),
         .DocDueDate = If(response("DocDueDate") IsNot Nothing,
                        DateTime.Parse(response("DocDueDate").ToString()),
                        DateTime.MinValue),
         .Lines = New List(Of OrderLines)()
     }

            If response("DocumentLines") IsNot Nothing Then
                For Each line In response("DocumentLines")
                    order.Lines.Add(New OrderLines With {
                 .LineNum = CInt(line("LineNum")),
                 .ItemCode = line("ItemCode")?.ToString(),
                 .Quantity = CDbl(line("Quantity")),
                 .DueDate = If(line("ShipDate") IsNot Nothing,
                             DateTime.Parse(line("ShipDate").ToString()),
                             DateTime.MinValue)
             })
                Next
            End If

            Return order

        Catch ex As Exception
            SBO_Application.StatusBar.SetText($"Error fetching order {docEntry}: {ex.Message}",
                                     BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
            Return Nothing
        End Try
    End Function

    Private Sub GenerateOrderTxt(order As Order, filePath As String)
        Try
            Using writer As New StreamWriter(filePath)

                writer.WriteLine($"HEADER|{order.DocEntry}|{order.CardCode}|{order.OrderDate:yyyy-MM-dd}|{order.DocDueDate:yyyy-MM-dd}")

                For Each line In order.Lines
                    writer.WriteLine($"LINE|{order.DocEntry}|{line.LineNum}|{line.ItemCode}|{line.Quantity}")
                Next
            End Using
        Catch ex As Exception
            Throw New Exception($"Error generating TXT for order {order.DocEntry}: {ex.Message}")
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
            Dim orderStatus As String = grid.DataTable.GetValue("OrderStatus", i)
            Dim operatorStatus As String = grid.DataTable.GetValue("OperatorStatus", i)

            If orderStatus = "Sent" Then
                grid.CommonSetting.SetRowBackColor(i + 1, RGB(57, 255, 20))
            ElseIf operatorStatus = "P" Then
                grid.CommonSetting.SetRowBackColor(i + 1, RGB(255, 255, 0))
            ElseIf operatorStatus = "R" Then
                grid.CommonSetting.SetRowBackColor(i + 1, RGB(139, 0, 0))
            Else

                grid.CommonSetting.SetRowBackColor(i + 1, -1)
            End If


            'ASK ABOUT THE 15 INVOICES
        Next
        grid.AutoResizeColumns()
    End Sub

#End Region
#End Region

End Class