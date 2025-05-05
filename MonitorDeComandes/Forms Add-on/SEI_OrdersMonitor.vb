Imports System.Collections.Generic
Imports System.IO
Imports System.Threading.Tasks
Imports System.Xml.Linq
Imports Newtonsoft.Json.Linq
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
    Public Sub SBO_Application_FormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)
        If BusinessObjectInfo.ActionSuccess And BusinessObjectInfo.BeforeAction = False Then
            If BusinessObjectInfo.FormTypeEx = "UDO_FormType" Then ' Change to your UDO Form Type
                Dim oForm As SAPbouiCOM.Form = SBO_Application.Forms.Item(BusinessObjectInfo.FormUID)
                Dim oMatrix As SAPbouiCOM.Matrix = oForm.Items.Item("YourMatrixID").Specific ' Change Matrix ID

                For i As Integer = 0 To oMatrix.RowCount - 1
                    Dim currentOperatorStatus As String = oMatrix.Columns.Item("Col_OperatorStatus").Cells.Item(i + 1).Specific.Value


                Next
            End If
        End If
    End Sub
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

            Dim query As String = $"SELECT * FROM (
                                    SELECT 
                                        '' AS Send,
                                        SQ.DocEntry,
                                        SQ.DocNum AS Document,
                                        SQ.DocTotal AS DocTotal,
                                        SQ.SlpName AS SalesEmployee,
                                        SQ.DocDate AS Date,
                                        SQ.DocDueDate AS DueDate,
                                        SQ.CardCode AS Client,
                                        SQ.CardName AS ClientName,
                                        SQ.Invoice AS Invoice,
                                        SQ.OrderStatus AS OrderStatus,
                                        SQ.OperatorStatus AS OperatorStatus,
                                        'Last 15 invoices' AS Status
                                    FROM (
                                        SELECT TOP 15 
                                            T0.DocEntry,
                                            T0.DocNum,
                                            T0.DocTotal,
                                            T1.SlpName,
                                            T0.DocDate,
                                            T0.DocDueDate,
                                            T0.CardCode,
                                            T0.CardName,
                                            T2.DocEntry AS Invoice,
                                            T0.U_OrdersStatus AS OrderStatus,
                                            T4.U_Status AS OperatorStatus,
                                            T2.DocDate AS InvoiceDocDate 
                                        FROM ORDR T0
                                        INNER JOIN INV1 T3 ON T0.DocEntry = T3.BaseEntry
                                        INNER JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
                                        INNER JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
                                        LEFT JOIN [@CONF_ORDERS] T4 ON T0.DocEntry = T4.U_DocEntry
                                        GROUP BY 
                                            T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
                                            T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, 
                                            T2.DocEntry, T0.U_OrdersStatus, T4.U_Status, T2.DocDate
                                        ORDER BY T2.DocDate DESC
                                    ) AS SQ

                                        UNION 

                                        SELECT 
                                            '' AS Send,
                                            T0.DocEntry,
                                            T0.DocNum AS Document,
                                            T0.DocTotal AS DocTotal,
                                            T1.SlpName AS SalesEmployee,
                                            T0.DocDate AS Date,
                                            T0.DocDueDate AS DueDate,
                                            T0.CardCode AS Client,
                                            T0.CardName AS ClientName,
                                            T2.DocEntry AS Invoice,
                                            T0.U_OrdersStatus AS OrderStatus,
                                            T4.U_Status AS OperatorStatus,
                                            'Sent' AS Status
                                        FROM ORDR T0
                                        LEFT JOIN INV1 T3 ON T0.DocEntry = T3.BaseEntry
                                        LEFT JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
                                        INNER JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
                                        LEFT JOIN [@CONF_ORDERS] T4 ON T0.DocEntry = T4.U_DocEntry
                                        WHERE T0.U_OrdersStatus = 'Sent'
                                        GROUP BY 
                                            T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
                                            T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, 
                                            T2.DocEntry, T0.U_OrdersStatus, T4.U_Status

                                        UNION

                                        SELECT 
                                            '' AS Send,
                                            T0.DocEntry,
                                            T0.DocNum AS Document,
                                            T0.DocTotal AS DocTotal,
                                            T1.SlpName AS SalesEmployee,
                                            T0.DocDate AS Date,
                                            T0.DocDueDate AS DueDate,
                                            T0.CardCode AS Client,
                                            T0.CardName AS ClientName,
                                            T2.DocEntry AS Invoice,
                                            T0.U_OrdersStatus AS OrderStatus,
                                            T4.U_Status AS OperatorStatus,
                                            'Error' AS Status
                                        FROM ORDR T0
                                        LEFT JOIN INV1 T3 ON T0.DocEntry = T3.BaseEntry
                                        LEFT JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
                                        INNER JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
                                        LEFT JOIN [@CONF_ORDERS] T4 ON T0.DocEntry = T4.U_DocEntry
                                        WHERE T4.U_Status = 'Error'
                                        GROUP BY 
                                            T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
                                            T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, 
                                            T2.DocEntry, T0.U_OrdersStatus, T4.U_Status

                                        UNION

                                        SELECT 
                                            '' AS Send,
                                            T0.DocEntry,
                                            T0.DocNum AS Document,
                                            T0.DocTotal AS DocTotal,
                                            T1.SlpName AS SalesEmployee,
                                            T0.DocDate AS Date,
                                            T0.DocDueDate AS DueDate,
                                            T0.CardCode AS Client,
                                            T0.CardName AS ClientName,
                                            T2.DocEntry AS Invoice,
                                            T0.U_OrdersStatus AS OrderStatus,
                                            T4.U_Status AS OperatorStatus,
                                            'Partially Confirmed' AS Status
                                        FROM ORDR T0
                                        LEFT JOIN INV1 T3 ON T0.DocEntry = T3.BaseEntry
                                        LEFT JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
                                        INNER JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
                                        LEFT JOIN [@CONF_ORDERS] T4 ON T0.DocEntry = T4.U_DocEntry
                                        WHERE T4.U_Status = 'Partially Confirmed'
                                        GROUP BY 
                                            T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
                                            T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, 
                                            T2.DocEntry, T0.U_OrdersStatus, T4.U_Status

                                        UNION

                                        SELECT 
                                            '' AS Send,
                                            T0.DocEntry,
                                            T0.DocNum AS Document,
                                            T0.DocTotal AS DocTotal,
                                            T1.SlpName AS SalesEmployee,
                                            T0.DocDate AS Date,
                                            T0.DocDueDate AS DueDate,
                                            T0.CardCode AS Client,
                                            T0.CardName AS ClientName,
                                            T2.DocEntry AS Invoice,
                                            T0.U_OrdersStatus AS OrderStatus,
                                            T4.U_Status AS OperatorStatus,
                                            'Confirmed' AS Status
                                        FROM ORDR T0
                                        LEFT JOIN INV1 T3 ON T0.DocEntry = T3.BaseEntry
                                        LEFT JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
                                        INNER JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
                                        LEFT JOIN [@CONF_ORDERS] T4 ON T0.DocEntry = T4.U_DocEntry
                                        WHERE T4.U_Status = 'Confirmed'
                                        GROUP BY 
                                            T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
                                            T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, 
                                            T2.DocEntry, T0.U_OrdersStatus, T4.U_Status

                                        UNION

                                        SELECT 
                                            '' AS Send,
                                            T0.DocEntry,
                                            T0.DocNum AS Document,
                                            T0.DocTotal AS DocTotal,
                                            T1.SlpName AS SalesEmployee,
                                            T0.DocDate AS Date,
                                            T0.DocDueDate AS DueDate,
                                            T0.CardCode AS Client,
                                            T0.CardName AS ClientName,
                                            T2.DocEntry AS Invoice,
                                            T0.U_OrdersStatus AS OrderStatus,
                                            T4.U_Status AS OperatorStatus,
                                            'Pending' AS Status
                                        FROM ORDR T0
                                        LEFT JOIN INV1 T3 ON T0.DocEntry = T3.BaseEntry
                                        LEFT JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
                                        INNER JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
                                        LEFT JOIN [@CONF_ORDERS] T4 ON T0.DocEntry = T4.U_DocEntry
                                        WHERE T0.U_OrdersStatus is null 
                                        AND T0.DocEntry NOT IN ( 
                                                SELECT TOP 15 
                                                    T0.DocEntry
                                                FROM ORDR T0
                                                INNER JOIN INV1 T3 ON T0.DocEntry = T3.BaseEntry
                                                INNER JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
                                                INNER JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
                                                LEFT JOIN [@CONF_ORDERS] T4 ON T0.DocEntry = T4.U_DocEntry
                                                GROUP BY 
                                                    T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
                                                    T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, 
                                                    T2.DocEntry, T0.U_OrdersStatus, T4.U_Status, T2.DocDate
                                                ORDER BY T2.DocDate DESC
                                            )
                                        GROUP BY 
                                            T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
                                            T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, 
                                            T2.DocEntry, T0.U_OrdersStatus, T4.U_Status) AS AllData 
                                    WHERE 1=1"

            If orderStatus <> "All" AndAlso orderStatus <> "" Then
                If orderStatus = "Pending" Then
                    query &= " AND OrderStatus IS NULL"
                Else
                    query &= " AND OrderStatus = '" & orderStatus & "'"
                End If
            End If

            If operatorStatus <> "" Then
                query &= " AND OperatorStatus = '" & operatorStatus & "'"
            End If

            If client <> "" Then
                query &= " AND Client = '" & client & "'"
            End If

            query &= " ORDER BY DueDate ASC"

            Me.Form.Freeze(True)
            oDataTable.ExecuteQuery(query)

            Dim grid As SAPbouiCOM.Grid = Me.Form.Items.Item(FormControls.grid).Specific
            grid.DataTable = oDataTable

            Dim rowHeaders As SAPbouiCOM.RowHeaders = grid.RowHeaders
            grid.RowHeaders.TitleObject.Caption = "#"

            For i As Integer = 0 To oDataTable.Rows.Count - 1

                rowHeaders.SetText(i, (i + 1).ToString())
            Next

            ConfigurateGridOfOrders(grid)
            Me.Form.Freeze(False)
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
                        PatchStatus(docEntry, "Sent").Wait()

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
                Dim docEntries As New List(Of Integer)
                For i As Integer = 0 To grid.Rows.Count - 1

                    Dim sendChecked As String = grid.DataTable.GetValue("Send", i)
                    Dim docEntry As Integer = grid.DataTable.GetValue("DocEntry", i)
                    Dim orderStatus As Integer = grid.DataTable.GetValue("OrderStatus", i)

                    docEntries.Add(docEntry)

                    If sendChecked = "Y" And orderStatus = "Delivered" Then
                        isOrderSelected = True
                        PatchStatus(docEntry, "Invoiced").Wait()
                    ElseIf sendChecked = "Y" And orderStatus <> "Delivered" Then
                        SBO_Application.StatusBar.SetText("You cannot create an invoice, there is no delivery created.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    End If
                Next
                If Not isOrderSelected Then
                    SBO_Application.StatusBar.SetText("No order selected. Please select at least one order to create it's invoice.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    Exit Sub
                End If

                PostInvoice(docEntries).Wait()
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
                        Dim delivery = CreateDeliveryObject(docEntry)
                        PostDelivery(delivery).Wait()
                        'Do post delivery. Fer el post de totes les deliveries marcades com a chek si                   
                        'Si ha anat be PatchStatus tal i com esta, sino cridem la funcio pero
                        'posatn no deliverered error delivering
                        Dim response = New DeliveryResponse()

                        If response.IsSuccess Then
                            PatchStatus(docEntry, "Delivered").Wait()
                        Else
                            PatchStatus(docEntry, "Error delivering").Wait()
                        End If

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

    Private Function CreateDeliveryObject(docEntry As Integer) As Delivery
        Try
            Dim response = Task.Run(Function() m_SBOAddon.oSLConnection.Request("CONF_ORDERS").Filter($"DocEntry eq {docEntry}").GetAsync(Of JObject)()).GetAwaiter().GetResult()
            If response Is Nothing Then
                Throw New Exception($"Order with DocEntry {docEntry} not found in CONF_ORDERS")
            End If

            Dim delivery As New Delivery()
            delivery.DocEntry = CInt(response("DocEntry"))
            delivery.CardCode = Task.Run(Function() GetOrderCardCode(CInt(response("DocNum")))).GetAwaiter().GetResult()
            delivery.DocDate = DateTime.Now.ToString("yyyy-MM-dd")
            delivery.DocDueDate = DateTime.Now.ToString("yyyy-MM-dd")
            delivery.Comments = $"Delivery created from confirmed order #{response("DocNum")}"

            Dim linesResponse = Task.Run(Function() m_SBOAddon.oSLConnection.Request("CONF_ORDERLINES").Filter($"DocEntry eq {docEntry}").GetAllAsync(Of JObject)()).GetAwaiter().GetResult()
            If linesResponse Is Nothing OrElse linesResponse.Count = 0 Then
                Throw New Exception($"No lines found for order {docEntry} in CONF_ORDERLINES")
            End If

            For Each lineData As JObject In linesResponse
                If lineData("LineStatus").ToString() = "C" Then
                    Dim line As New DeliveryLines()
                    line.BaseEntry = CInt(response("DocNum"))
                    line.BaseLine = CInt(lineData("LineNum"))
                    line.ItemCode = lineData("ItemCode").ToString()
                    line.Quantity = CDbl(lineData("Quantity"))
                    If Not String.IsNullOrEmpty(lineData("LotNumber")?.ToString()) Then
                        Dim batch As New BatchNumber()
                        batch.BatchNumber = lineData("LotNumber").ToString()
                        batch.Quantity = CDbl(lineData("Quantity"))
                        line.BatchNumbers.Add(batch)
                    End If
                    delivery.DocumentLines.Add(line)
                End If
            Next

            If delivery.DocumentLines.Count = 0 Then
                Throw New Exception("No confirmed lines were found for delivery creation")
            End If

            Return delivery
        Catch ex As Exception
            SBO_Application.StatusBar.SetText($"Error creating delivery object from UDO: {ex.Message}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
            Return Nothing
        End Try
    End Function

    Private Async Function GetOrderCardCode(docNum As Integer) As Task(Of String)
        Try
            Dim orderResponse = Await m_SBOAddon.oSLConnection.Request("Orders").Filter($"DocNum eq {docNum}").GetAsync(Of JObject)()

            If orderResponse Is Nothing Then
                Throw New Exception($"Original order with DocNum {docNum} not found")
            End If

            Return orderResponse("CardCode").ToString()
        Catch ex As Exception
            SBO_Application.StatusBar.SetText($"Error getting CardCode: {ex.Message}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
            Return String.Empty
        End Try
    End Function

    Private Async Function PostDelivery(delivery As Delivery) As Task(Of DeliveryResponse)
        Dim response As New DeliveryResponse()

        Try
            If delivery Is Nothing Then
                response.IsSuccess = False
                response.Message = "Delivery object is null"
                Return response
            End If

            Try
                Await m_SBOAddon.oSLConnection.Request("Orders").PostAsync(delivery)

                response.IsSuccess = True
                response.Message = "Delivery created successfully"
                SBO_Application.StatusBar.SetText($"Delivery for order #{delivery.DocEntry} created successfully.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success)
            Catch slEx As Exception
                response.IsSuccess = False
                response.Message = $"Error creating delivery: {slEx.Message}"
                SBO_Application.StatusBar.SetText(response.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
            End Try

            Return response

        Catch ex As Exception
            response.IsSuccess = False
            response.Message = "Exception when posting delivery: " & ex.Message
            SBO_Application.StatusBar.SetText(response.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
            Return response
        End Try
    End Function

    Private Async Function PostInvoice(docEntries As List(Of Integer)) As Task
        Try
            If docEntries Is Nothing OrElse docEntries.Count = 0 Then
                SBO_Application.StatusBar.SetText("No orders selected to create invoices.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                Return
            End If

            Dim successCount As Integer = 0
            Dim errorCount As Integer = 0

            For Each docEntry As Integer In docEntries
                Try
                    Dim order As Order = GetOrder(docEntry)
                    If order Is Nothing Then
                        SBO_Application.StatusBar.SetText($"Error: Could not retrieve order {docEntry}.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                        errorCount += 1
                        Continue For
                    End If

                    Dim invoice As New With {
                    .CardCode = order.CardCode,
                    .DocDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    .DocDueDate = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd"),
                    .Comments = $"Invoice created from order #{docEntry}",
                    .DocumentLines = New List(Of Object)()
                }

                    For Each line In order.DocumentLines
                        Dim invoiceLine As New With {
                        .BaseEntry = docEntry,
                        .BaseLine = line.LineNum,
                        .BaseType = 17,
                        .Quantity = line.Quantity
                    }
                        invoice.DocumentLines.Add(invoiceLine)
                    Next

                    Await m_SBOAddon.oSLConnection.Request("Invoices").PostAsync(invoice)

                    Await PatchStatus(docEntry, "Invoiced")

                    SBO_Application.StatusBar.SetText($"Invoice for order #{docEntry} created successfully.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success)
                    successCount += 1

                Catch ex As Exception
                    SBO_Application.StatusBar.SetText($"Error creating invoice for order #{docEntry}: {ex.Message}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    errorCount += 1
                End Try
            Next

            If successCount > 0 AndAlso errorCount = 0 Then
                SBO_Application.StatusBar.SetText($"All invoices ({successCount}) created successfully.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success)
            ElseIf successCount > 0 AndAlso errorCount > 0 Then
                SBO_Application.StatusBar.SetText($"Created {successCount} invoices with {errorCount} errors.", BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Warning)
            ElseIf successCount = 0 AndAlso errorCount > 0 Then
                SBO_Application.StatusBar.SetText($"Failed to create any invoices. {errorCount} errors occurred.", BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Error)
            End If

        Catch ex As Exception
            SBO_Application.StatusBar.SetText($"Error in PostInvoice: {ex.Message}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
        End Try
    End Function

    Private Async Function PatchStatus(DocEntry As Integer, OrderStatus As String) As Task
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


            'Dim query As String = $"SELECT '' AS Send, T0.DocEntry,T0.DocNum as Document,T0.DocTotal AS DocTotal,T1.SlpName AS SalesEmployee,T0.DocDate As Date,T0.DocDueDate AS DueDate,T0.CardCode AS Client,T0.CardName AS ClientName,T2.DocEntry AS Invoice,T0.U_OrdersStatus AS OrderStatus,T4.U_Status AS OperatorStatus,
            '                        CASE 
            '                            WHEN T2.DocEntry IN (
            '                                SELECT TOP 15 T2.DocEntry FROM ORDR T0
            '	JOIN INV1 T3 ON T0.DocEntry = T3.BaseEntry
            '	JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
            '	GROUP by T2.DocEntry,T2.DocDate  ORDER BY T2.DocDate DESC
            '                            ) THEN 'Y' ELSE 'N'
            '                        END AS IsLastFifteenInvoices
            '                        FROM ORDR T0
            '                        INNER JOIN OSLP T1 on T0.SlpCode = T1.SlpCode
            '                        LEFT JOIN INV1 T3 on T0.DocEntry = T3.BaseEntry
            '                        LEFT JOIN OINV T2 on T3.DocEntry = T2.DocEntry
            '                        LEFT JOIN [@CONF_ORDERS] T4 on T0.DocEntry = T4.U_DocEntry
            '                        GROUP BY T0.DocEntry,T0.DocNum ,T0.DocTotal ,T1.SlpName ,T0.DocDate ,T0.DocDueDate,T0.CardCode,T0.CardName,T2.DocEntry,T0.U_OrdersStatus ,T4.U_Status  Order by T0.DocEntry"

            Dim query As String = $"SELECT * FROM (
                                    SELECT 
                                        '' AS Send,
                                        SQ.DocEntry,
                                        SQ.DocNum AS Document,
                                        SQ.DocTotal AS DocTotal,
                                        SQ.SlpName AS SalesEmployee,
                                        SQ.DocDate AS Date,
                                        SQ.DocDueDate AS DueDate,
                                        SQ.CardCode AS Client,
                                        SQ.CardName AS ClientName,
                                        SQ.Invoice AS Invoice,
                                        SQ.OrderStatus AS OrderStatus,
                                        SQ.OperatorStatus AS OperatorStatus,
                                        'Last 15 invoices' AS Status
                                    FROM (
                                        SELECT TOP 15 
                                            T0.DocEntry,
                                            T0.DocNum,
                                            T0.DocTotal,
                                            T1.SlpName,
                                            T0.DocDate,
                                            T0.DocDueDate,
                                            T0.CardCode,
                                            T0.CardName,
                                            T2.DocEntry AS Invoice,
                                            T0.U_OrdersStatus AS OrderStatus,
                                            T4.U_Status AS OperatorStatus,
                                            T2.DocDate AS InvoiceDocDate 
                                        FROM ORDR T0
                                        INNER JOIN INV1 T3 ON T0.DocEntry = T3.BaseEntry
                                        INNER JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
                                        INNER JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
                                        LEFT JOIN [@CONF_ORDERS] T4 ON T0.DocEntry = T4.U_DocEntry
                                        GROUP BY 
                                            T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
                                            T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, 
                                            T2.DocEntry, T0.U_OrdersStatus, T4.U_Status, T2.DocDate
                                        ORDER BY T2.DocDate DESC
                                    ) AS SQ

                                        UNION 

                                        SELECT 
                                            '' AS Send,
                                            T0.DocEntry,
                                            T0.DocNum AS Document,
                                            T0.DocTotal AS DocTotal,
                                            T1.SlpName AS SalesEmployee,
                                            T0.DocDate AS Date,
                                            T0.DocDueDate AS DueDate,
                                            T0.CardCode AS Client,
                                            T0.CardName AS ClientName,
                                            T2.DocEntry AS Invoice,
                                            T0.U_OrdersStatus AS OrderStatus,
                                            T4.U_Status AS OperatorStatus,
                                            'Sent' AS Status
                                        FROM ORDR T0
                                        LEFT JOIN INV1 T3 ON T0.DocEntry = T3.BaseEntry
                                        LEFT JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
                                        INNER JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
                                        LEFT JOIN [@CONF_ORDERS] T4 ON T0.DocEntry = T4.U_DocEntry
                                        WHERE T0.U_OrdersStatus = 'Sent'
                                        GROUP BY 
                                            T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
                                            T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, 
                                            T2.DocEntry, T0.U_OrdersStatus, T4.U_Status

                                        UNION

                                        SELECT 
                                            '' AS Send,
                                            T0.DocEntry,
                                            T0.DocNum AS Document,
                                            T0.DocTotal AS DocTotal,
                                            T1.SlpName AS SalesEmployee,
                                            T0.DocDate AS Date,
                                            T0.DocDueDate AS DueDate,
                                            T0.CardCode AS Client,
                                            T0.CardName AS ClientName,
                                            T2.DocEntry AS Invoice,
                                            T0.U_OrdersStatus AS OrderStatus,
                                            T4.U_Status AS OperatorStatus,
                                            'Error' AS Status
                                        FROM ORDR T0
                                        LEFT JOIN INV1 T3 ON T0.DocEntry = T3.BaseEntry
                                        LEFT JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
                                        INNER JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
                                        LEFT JOIN [@CONF_ORDERS] T4 ON T0.DocEntry = T4.U_DocEntry
                                        WHERE T4.U_Status = 'Error'
                                        GROUP BY 
                                            T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
                                            T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, 
                                            T2.DocEntry, T0.U_OrdersStatus, T4.U_Status

                                        UNION

                                        SELECT 
                                            '' AS Send,
                                            T0.DocEntry,
                                            T0.DocNum AS Document,
                                            T0.DocTotal AS DocTotal,
                                            T1.SlpName AS SalesEmployee,
                                            T0.DocDate AS Date,
                                            T0.DocDueDate AS DueDate,
                                            T0.CardCode AS Client,
                                            T0.CardName AS ClientName,
                                            T2.DocEntry AS Invoice,
                                            T0.U_OrdersStatus AS OrderStatus,
                                            T4.U_Status AS OperatorStatus,
                                            'Partially Confirmed' AS Status
                                        FROM ORDR T0
                                        LEFT JOIN INV1 T3 ON T0.DocEntry = T3.BaseEntry
                                        LEFT JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
                                        INNER JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
                                        LEFT JOIN [@CONF_ORDERS] T4 ON T0.DocEntry = T4.U_DocEntry
                                        WHERE T4.U_Status = 'Partially Confirmed'
                                        GROUP BY 
                                            T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
                                            T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, 
                                            T2.DocEntry, T0.U_OrdersStatus, T4.U_Status

                                        UNION

                                        SELECT 
                                            '' AS Send,
                                            T0.DocEntry,
                                            T0.DocNum AS Document,
                                            T0.DocTotal AS DocTotal,
                                            T1.SlpName AS SalesEmployee,
                                            T0.DocDate AS Date,
                                            T0.DocDueDate AS DueDate,
                                            T0.CardCode AS Client,
                                            T0.CardName AS ClientName,
                                            T2.DocEntry AS Invoice,
                                            T0.U_OrdersStatus AS OrderStatus,
                                            T4.U_Status AS OperatorStatus,
                                            'Confirmed' AS Status
                                        FROM ORDR T0
                                        LEFT JOIN INV1 T3 ON T0.DocEntry = T3.BaseEntry
                                        LEFT JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
                                        INNER JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
                                        LEFT JOIN [@CONF_ORDERS] T4 ON T0.DocEntry = T4.U_DocEntry
                                        WHERE T4.U_Status = 'Confirmed'
                                        GROUP BY 
                                            T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
                                            T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, 
                                            T2.DocEntry, T0.U_OrdersStatus, T4.U_Status

                                        UNION

                                        SELECT 
                                            '' AS Send,
                                            T0.DocEntry,
                                            T0.DocNum AS Document,
                                            T0.DocTotal AS DocTotal,
                                            T1.SlpName AS SalesEmployee,
                                            T0.DocDate AS Date,
                                            T0.DocDueDate AS DueDate,
                                            T0.CardCode AS Client,
                                            T0.CardName AS ClientName,
                                            T2.DocEntry AS Invoice,
                                            T0.U_OrdersStatus AS OrderStatus,
                                            T4.U_Status AS OperatorStatus,
                                            'Pending' AS Status
                                        FROM ORDR T0
                                        LEFT JOIN INV1 T3 ON T0.DocEntry = T3.BaseEntry
                                        LEFT JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
                                        INNER JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
                                        LEFT JOIN [@CONF_ORDERS] T4 ON T0.DocEntry = T4.U_DocEntry
                                        WHERE T0.U_OrdersStatus is null 
                                        AND T0.DocEntry NOT IN ( 
                                                SELECT TOP 15 
                                                    T0.DocEntry
                                                FROM ORDR T0
                                                INNER JOIN INV1 T3 ON T0.DocEntry = T3.BaseEntry
                                                INNER JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
                                                INNER JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
                                                LEFT JOIN [@CONF_ORDERS] T4 ON T0.DocEntry = T4.U_DocEntry
                                                GROUP BY 
                                                    T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
                                                    T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, 
                                                    T2.DocEntry, T0.U_OrdersStatus, T4.U_Status, T2.DocDate
                                                ORDER BY T2.DocDate DESC
                                            )
                                        GROUP BY 
                                            T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
                                            T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, 
                                            T2.DocEntry, T0.U_OrdersStatus, T4.U_Status) AS AllData 
                                        ORDER BY AllData.DueDate Asc"

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
         .DocumentLines = New List(Of OrderLines)()
     }

            If response("DocumentLines") IsNot Nothing Then
                For Each line In response("DocumentLines")
                    order.DocumentLines.Add(New OrderLines With {
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

                For Each line In order.DocumentLines
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
                grid.Columns.Item(uid).Editable = False
            End If

            If (uid = "Document") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Document"
                grid.Columns.Item(uid).Editable = False
            End If

            If (uid = "DocTotal") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Document Total"
                grid.Columns.Item(uid).Editable = False
            End If

            If (uid = "SalesEmployee") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Sales Employee"
                grid.Columns.Item(uid).Editable = False
            End If

            If (uid = "Invoice") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Invoice"
                grid.Columns.Item(uid).Editable = False
            End If

            If (uid = "Date") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Date"
                grid.Columns.Item(uid).Editable = False
            End If

            If (uid = "DueDate") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Due Date"
                grid.Columns.Item(uid).Editable = False
            End If

            If (uid = "Client") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Client"
                grid.Columns.Item(uid).Editable = False
            End If

            If (uid = "ClientName") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Client Name"
                grid.Columns.Item(uid).Editable = False
            End If

            If (uid = "OrderStatus") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Order Status"
                grid.Columns.Item(uid).Editable = False
            End If

            If (uid = "OperatorStatus") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Operator Status"
                grid.Columns.Item(uid).Editable = False
            End If
        Next

        If grid.Columns.Count > 0 AndAlso grid.Columns.Item("Status") IsNot Nothing Then
            grid.Columns.Item("Status").Visible = False
        End If

        For i = 0 To grid.Rows.Count - 1
            Dim orderStatus As String = grid.DataTable.GetValue("OrderStatus", i)
            Dim operatorStatus As String = grid.DataTable.GetValue("OperatorStatus", i)
            Dim status As String = grid.DataTable.GetValue("Status", i)

            If status = "Sent" Then
                grid.CommonSetting.SetRowBackColor(i + 1, RGB(255, 255, 0)) 'YELLOW
            ElseIf status = "Partially Confirmed" Then
                grid.CommonSetting.SetRowBackColor(i + 1, RGB(255, 165, 0)) 'ORANGE
            ElseIf status = "Error" Then
                grid.CommonSetting.SetRowBackColor(i + 1, RGB(139, 0, 0)) 'RED
            ElseIf status = "Confirmed" Then
                grid.CommonSetting.SetRowBackColor(i + 1, RGB(57, 255, 20)) 'GREEN
            ElseIf status = "Last 15 invoices" Then
                grid.CommonSetting.SetRowBackColor(i + 1, RGB(173, 216, 230)) ' blue
            Else
                grid.CommonSetting.SetRowBackColor(i + 1, -1)
            End If
        Next

        grid.DataTable.LoadSerializedXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly, xmlDoc.ToString())
        grid.AutoResizeColumns()
    End Sub

#End Region
#End Region

End Class