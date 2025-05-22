Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Xml.Linq
Imports CrystalDecisions.Shared.Json
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
        Const cbColor As String = "cbColor"
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
            SBO_Application.StatusBar.SetText("Error loading the 'Order Monitor' screen", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
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
                    Case FormControls.btnCreateDelivery
                        HandleBtnCreateDeliveries(pVal, BubbleEvent)
                    Case FormControls.btnCreateInvoice
                        HandleBtnCreateInvoices(pVal, BubbleEvent)
                    Case FormControls.btnFilter
                        HandleBtnFilter(pVal, BubbleEvent)
                End Select
            End If
        Catch ex As Exception
            SBO_Application.StatusBar.SetText($"Error in the screen: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
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
            LoadOrdersDataInGridWithFiltration()
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
                    Dim sendChecked As String = grid.DataTable.GetValue("Send", i).ToString()
                    Dim docDueDate As DateTime = grid.DataTable.GetValue("DueDate", i)
                    Dim orderStatus As String = grid.DataTable.GetValue("OrderStatus", i).ToString()

                    If sendChecked = "Y" AndAlso docDueDate >= Date.Today And String.IsNullOrEmpty(orderStatus) Then
                        isOrderSelected = True
                        Dim docEntry As Integer = grid.DataTable.GetValue("DocEntry", i)

                        Dim order As Order = GetOrder(docEntry)
                        If order Is Nothing Then
                            Throw New Exception("Order not found")
                        End If

                        Dim filePath As String = $"{basePath}Order_{docEntry}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.txt"
                        GenerateOrderTxt(order, filePath)
                        PatchStatus(docEntry, "Sent").Wait()
                    ElseIf sendChecked = "Y" And docDueDate < Date.Today Then
                        isOrderSelected = True
                        SBO_Application.StatusBar.SetText("Cannot proceed: document due date is overdue.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    ElseIf sendChecked = "Y" And Not String.IsNullOrWhiteSpace(orderStatus) Then
                        isOrderSelected = True
                        SBO_Application.StatusBar.SetText("Cannot proceed: the order is not pending.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    End If
                Next


                If Not isOrderSelected Then
                    SBO_Application.StatusBar.SetText("No order selected. Please select at least one order to send.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    Exit Sub
                End If

                LoadOrdersDataInGridWithFiltration()

            Catch ex As Exception
                SBO_Application.StatusBar.SetText($"Error: {ex.Message}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                BubbleEvent = False
            End Try
        End If

    End Sub

    Private Sub HandleBtnCreateDeliveries(pVal As ItemEvent, ByRef BubbleEvent As Boolean)
        If pVal.EventType <> BoEventTypes.et_ITEM_PRESSED Then Exit Sub
        If Not pVal.BeforeAction Then
            Try
                Dim grid As SAPbouiCOM.Grid = Me.Form.Items.Item(FormControls.grid).Specific
                Dim isOrderSelected As Boolean = False
                Dim selectedOrders As New List(Of Order)

                For i As Integer = 0 To grid.Rows.Count - 1
                    Dim sendChecked As String = grid.DataTable.GetValue("Send", i)
                    Dim operatorStatus As String = grid.DataTable.GetValue("OperatorStatus", i)

                    If sendChecked = "Y" And operatorStatus = "C" Then
                        Dim docEntry As Integer = grid.DataTable.GetValue("DocEntry", i)
                        Dim cardCode As String = grid.DataTable.GetValue("Client", i).ToString()

                        Dim order As New Order With {
                            .DocEntry = docEntry,
                            .CardCode = cardCode
                        }

                        selectedOrders.Add(order)
                        isOrderSelected = True

                    ElseIf sendChecked = "Y" And operatorStatus <> "C" Then
                        SBO_Application.StatusBar.SetText("You cannot create a delivery, the order is not confirmed.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    End If
                Next

                If Not isOrderSelected Then
                    SBO_Application.StatusBar.SetText("No order selected. Please select at least one order to create its delivery.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    Exit Sub
                Else
                    PostDelivery(selectedOrders).Wait()
                    LoadOrdersDataInGridWithFiltration()
                End If
            Catch ex As Exception
                SBO_Application.StatusBar.SetText($"Error creating deliveries: {ex.Message}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
            End Try
        End If
    End Sub

    Private Sub HandleBtnCreateInvoices(pVal As ItemEvent, ByRef BubbleEvent As Boolean)
        If pVal.EventType <> BoEventTypes.et_ITEM_PRESSED Then Exit Sub
        If Not pVal.BeforeAction Then
            Try
                Dim grid As SAPbouiCOM.Grid = Me.Form.Items.Item(FormControls.grid).Specific
                Dim isOrderSelected As Boolean = False
                Dim selectedOrders As New List(Of Order)
                For i As Integer = 0 To grid.Rows.Count - 1

                    Dim sendChecked As String = grid.DataTable.GetValue("Send", i)
                    Dim orderStatus As String = grid.DataTable.GetValue("OrderStatus", i)

                    If sendChecked = "Y" And orderStatus = "Delivered" Then

                        Dim docEntry As Integer = grid.DataTable.GetValue("DocEntry", i)
                        Dim cardCode As String = grid.DataTable.GetValue("Client", i).ToString()
                        Dim DeliveryDocEntry As Integer = grid.DataTable.GetValue("Delivery", i)

                        Dim order As New Order With {
                            .DocEntry = docEntry,
                            .CardCode = cardCode,
                            .TrgetEntry = DeliveryDocEntry
                        }

                        selectedOrders.Add(order)
                        isOrderSelected = True

                    ElseIf sendChecked = "Y" And orderStatus <> "Delivered" Then
                        SBO_Application.StatusBar.SetText("You cannot create an invoice, there is no delivery created.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    End If
                Next
                If Not isOrderSelected Then
                    SBO_Application.StatusBar.SetText("No order selected. Please select at least one order to create it's invoice.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    Exit Sub
                Else
                    PostInvoice(selectedOrders).Wait()
                    LoadOrdersDataInGridWithFiltration()
                End If

            Catch ex As Exception
                SBO_Application.StatusBar.SetText($"{ex.Message}")
            End Try

        End If
    End Sub

#End Region

#Region "FUINCIONES GENERALES"

    Private Async Function PostDelivery(orders As List(Of Order)) As Task
        Try
            For Each orderBasic In orders
                Try
                    Dim completeOrder As Order = GetOrder(orderBasic.DocEntry)

                    If completeOrder Is Nothing OrElse completeOrder.DocumentLines Is Nothing OrElse completeOrder.DocumentLines.Count = 0 Then
                        Throw New Exception($"The full order could not be retrieved or it has no lines")
                    End If

                    Dim confirmedLines = GetConfirmedOrderLines(orderBasic.DocEntry)

                    If confirmedLines Is Nothing OrElse confirmedLines.Count = 0 Then
                        Throw New Exception($"No confirmed lines were found for order {orderBasic.DocEntry}")
                    End If

                    Dim delivery As New With {
                    .CardCode = completeOrder.CardCode,
                    .DocDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    .DocDueDate = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd"),
                    .Comments = $"Delivery created from order #{completeOrder.DocEntry}",
                    .DocumentLines = New List(Of Object)()
                    }

                    For Each orderLine In completeOrder.DocumentLines

                        Dim confirmedLinesForOrderLine As New List(Of Object)

                        For Each confirmedLine In confirmedLines
                            If CInt(confirmedLine.U_LineNum) = orderLine.LineNum Then
                                confirmedLinesForOrderLine.Add(confirmedLine)
                            End If
                        Next

                        If confirmedLinesForOrderLine.Count > 0 Then
                            Dim batchList As New List(Of Object)
                            Dim totalConfirmedQuantity As Double = 0

                            For Each confirmedLine In confirmedLinesForOrderLine
                                Dim batchNumber As String = confirmedLine.U_Batch?.ToString()
                                Dim quantity As Double = CDbl(confirmedLine.U_Quantity)

                                If Not String.IsNullOrEmpty(batchNumber) Then
                                    batchList.Add(New With {
                                    .BatchNumber = batchNumber,
                                    .Quantity = quantity
                                })
                                End If

                                totalConfirmedQuantity += quantity
                            Next

                            Dim deliveryLine = New With {
                            .BaseEntry = completeOrder.DocEntry,
                            .BaseLine = orderLine.LineNum,
                            .BaseType = 17,
                            .ItemCode = orderLine.ItemCode,
                            .Quantity = totalConfirmedQuantity,
                            .BatchNumbers = batchList
                        }

                            delivery.DocumentLines.Add(deliveryLine)
                        End If
                    Next

                    If delivery.DocumentLines.Count = 0 Then
                        Throw New Exception($"No confirmed lines were found to create the delivery for order {orderBasic.DocEntry}")
                    End If

                    Await m_SBOAddon.oSLConnection.Request("DeliveryNotes").PostAsync(delivery)

                    Await PatchStatus(completeOrder.DocEntry, "Delivered")

                    SBO_Application.StatusBar.SetText($"Delivery for order #{completeOrder.DocEntry} created successfully.", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success)

                Catch ex As Exception
                    SBO_Application.StatusBar.SetText($"Error creating delivery for order #{orderBasic.DocEntry}: {ex.Message}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    LogError(orderBasic.DocEntry, ex.Message)
                End Try
            Next
        Catch ex As Exception
            SBO_Application.StatusBar.SetText($"General error in PostDelivery: {ex.Message}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
        End Try
    End Function

    Private Function GetConfirmedOrderLines(orderId As Integer) As List(Of Object)
        Try
            Dim recordset As SAPbobsCOM.Recordset = CType(SBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset), SAPbobsCOM.Recordset)
            Dim query As String = $"SELECT U_LineNum, U_ItemCode, U_Quantity, U_Batch FROM [@CONFORDERLINES] WHERE U_OrderId = {orderId}"

            recordset.DoQuery(query)

            Dim confirmedLines As New List(Of Object)

            While Not recordset.EoF
                confirmedLines.Add(New With {
                .U_LineNum = CInt(recordset.Fields.Item("U_LineNum").Value),
                .U_ItemCode = recordset.Fields.Item("U_ItemCode").Value.ToString(),
                .U_Quantity = CDbl(recordset.Fields.Item("U_Quantity").Value),
                .U_Batch = recordset.Fields.Item("U_Batch").Value.ToString()
            })
                recordset.MoveNext()
            End While

            Return confirmedLines
        Catch ex As Exception
            Throw New Exception($"Error getting confirmed lines: {ex.Message}")
        End Try
    End Function

    Private Async Function PostInvoice(orders As List(Of Order)) As Task
        Try

            Dim groupedOrders = orders.GroupBy((Function(o) o.CardCode))
            For Each group In groupedOrders
                Dim deliveries As New List(Of Delivery)
                Dim cardCode = group.Key
                Try
                    Dim salesOrders = group.ToList()
                    For Each salesOrder In salesOrders
                        Dim delivery As Delivery = GetDelivery(salesOrder.TrgetEntry)
                        If delivery IsNot Nothing Then
                            deliveries.Add(delivery)
                        End If

                    Next

                    Dim invoice As New With {
                    .CardCode = cardCode,
                    .DocDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    .DocDueDate = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd"),
                    .Comments = $"Invoice created from deliveries",
                    .DocumentLines = New List(Of Object)()
                }
                    For Each delivery In deliveries
                        For Each line In delivery.DocumentLines
                            Dim invoiceLine = New With {
                            .BaseEntry = delivery.DocEntry,
                            .BaseLine = line.lineNum,
                            .BaseType = 15
                        }
                            invoice.DocumentLines.Add(invoiceLine)
                        Next
                    Next


                    Await m_SBOAddon.oSLConnection.Request("Invoices").PostAsync(invoice)

                    For Each delivery In deliveries
                        Dim baseEntry As Integer = delivery.DocumentLines.First().BaseEntry
                        Await PatchStatus(baseEntry, "Invoiced")
                    Next

                Catch ex As Exception
                    SBO_Application.StatusBar.SetText($"Error creating invoice for  : {ex.Message}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    LogError(cardCode, ex.Message)

                End Try
            Next

        Catch ex As Exception
            SBO_Application.StatusBar.SetText($"Error in PostInvoice: {ex.Message}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
        End Try
    End Function

    Private Sub LogError(cardCode As Integer, errorMessage As String)
        Try
            Dim userTable As SAPbobsCOM.UserTable = CType(SBO_Company.UserTables.Item("LogMonitorOrders"), SAPbobsCOM.UserTable)

            userTable.UserFields.Fields.Item("U_CardCode").Value = cardCode
            userTable.UserFields.Fields.Item("U_Error").Value = errorMessage

            Dim result As Integer = userTable.Add()
            If result <> 0 Then
                Dim errMsg As String = ""
                Dim errCode As Integer
                SBO_Company.GetLastError(errCode, errMsg)
                SBO_Application.StatusBar.SetText($"Failed to insert log: {errMsg}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
            End If
        Catch ex As Exception
            SBO_Application.StatusBar.SetText($"Exception logging error: {ex.Message}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
        End Try
    End Sub

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

    Private Sub LoadOrdersDataInGridWithFiltration()

        Try

            Dim oDataTable As SAPbouiCOM.DataTable = Me.Form.DataSources.DataTables.Item(FormControls.DataTable)

            Dim cbOrderSt As ComboBox = Me.Form.Items.Item(FormControls.cbOrderSt).Specific
            Dim cbOperatorSt As ComboBox = Me.Form.Items.Item(FormControls.cbOperatorSt).Specific
            Dim etClient As EditText = Me.Form.Items.Item(FormControls.etClient).Specific
            Dim cbColor As ComboBox = Me.Form.Items.Item(FormControls.cbColor).Specific

            Dim orderStatus As String = If(cbOrderSt.Selected IsNot Nothing, cbOrderSt.Selected.Value, "").Trim()
            Dim operatorStatus As String = If(cbOperatorSt.Selected IsNot Nothing, cbOperatorSt.Selected.Value, "").Trim()
            Dim client As String = If(etClient IsNot Nothing, etClient.Value.Trim(), "")
            Dim lineColor As String = If(cbColor.Selected IsNot Nothing, cbColor.Selected.Value, "").Trim()

            Dim query As String = $"SELECT * FROM (
                                                    SELECT 
                                                        '' AS Send,
                                                        SQ.DocEntry,
                                                        SQ.DocNum AS Document,
                                                        SQ.DocTotal AS DocTotal,
                                                        SQ.SlpName AS SalesEmployee,
                                                        SQ.DocDate AS DocumentDate,
                                                        SQ.DocDueDate AS DueDate,
										                SQ.Invoice,
										                SQ.InvoiceDocNum,
										                SQ.Delivery,
										                SQ.DeliveryDocNum,
                                                        SQ.CardCode AS Client,
                                                        SQ.CardName AS ClientName,
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
											                T2.DocNum AS InvoiceDocNum,
											                D.DocEntry AS Delivery,
											                D.DocNum AS DeliveryDocNum,
                                                            T0.U_OrdersStatus AS OrderStatus,
                                                            T4.U_Status AS OperatorStatus,
                                                            T2.DocDate AS InvoiceDocDate 
                                                        FROM ORDR T0
										                INNER JOIN RDR1 R ON T0.DocEntry = R.DocEntry
										                LEFT JOIN DLN1 D1 ON R.DocEntry = D1.BaseEntry AND R.LineNum = D1.BaseLine AND D1.BaseType = 17  
										                INNER JOIN ODLN D ON D1.DocEntry = D.DocEntry
										                LEFT JOIN INV1 T3 ON D.DocEntry = T3.BaseEntry AND D1.LineNum = T3.BaseLine AND T3.BaseType = 15  
										                INNER JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
                                                        LEFT JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
                                                        LEFT JOIN [@CONFORDERS] T4 ON T0.DocEntry = T4.U_OrderId
                                                        GROUP BY 
                                                            T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
                                                            T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, 
                                                            T2.DocEntry,T2.DocNum,D.DocEntry,
											                D.DocNum, T0.U_OrdersStatus, T4.U_Status, T2.DocDate
                                                        ORDER BY T2.DocDate DESC
                                                    ) AS SQ

                                                    UNION ALL

									                SELECT 
										                '' AS Send,
										                T0.DocEntry,
										                T0.DocNum AS Document,
										                T0.DocTotal AS DocTotal,
										                T1.SlpName AS SalesEmployee,
										                T0.DocDate AS DocumentDate,
										                T0.DocDueDate AS DueDate,
										                ISNULL(T2.DocEntry, 0) AS Invoice,
										                ISNULL(T2.DocNum, 0) AS InvoiceDocNum,
										                ISNULL(D.DocEntry, 0) AS Delivery,
										                ISNULL(D.DocNum, 0) AS DeliveryDocNum,
										                T0.CardCode AS Client,
										                T0.CardName AS ClientName,
										                T0.U_OrdersStatus AS OrderStatus,
										                T4.U_Status AS OperatorStatus,
										                CASE
											                WHEN T0.U_OrdersStatus = 'Sent' THEN 'Sent'
											                WHEN T2.DocEntry IS NOT NULL THEN 'Invoiced'
											                WHEN D.DocEntry IS NOT NULL THEN 'Delivered'
											                WHEN T4.U_Status = 'R' THEN 'Rejected'
											                WHEN T4.U_Status = 'C' THEN 'Confirmed'
										                END AS Status
									                FROM ORDR T0
									                INNER JOIN RDR1 R ON T0.DocEntry = R.DocEntry
									                LEFT JOIN DLN1 D1 ON R.DocEntry = D1.BaseEntry AND R.LineNum = D1.BaseLine AND D1.BaseType = 17  
									                LEFT JOIN ODLN D ON D1.DocEntry = D.DocEntry
									                LEFT JOIN INV1 T3 ON D.DocEntry = T3.BaseEntry AND D1.LineNum = T3.BaseLine AND T3.BaseType = 15  
									                LEFT JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
									                LEFT JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
									                LEFT JOIN [@CONFORDERS] T4 ON T0.DocEntry = T4.U_OrderId
									                WHERE (T0.U_OrdersStatus = 'Sent'
										                    OR T2.DocEntry IS NOT NULL 
											                OR D.DocEntry IS NOT NULL
											                OR T4.U_Status IN ('R', 'C'))
											                AND T0.DocEntry NOT IN (
												                SELECT TOP 15 T0.DocEntry
												                FROM ORDR T0
												                INNER JOIN RDR1 R ON T0.DocEntry = R.DocEntry
												                LEFT JOIN DLN1 D1 ON R.DocEntry = D1.BaseEntry AND R.LineNum = D1.BaseLine AND D1.BaseType = 17  
												                INNER JOIN ODLN D ON D1.DocEntry = D.DocEntry
												                LEFT JOIN INV1 T3 ON D.DocEntry = T3.BaseEntry AND D1.LineNum = T3.BaseLine AND T3.BaseType = 15  
												                INNER JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
												                GROUP BY T0.DocEntry, T2.DocDate
												                ORDER BY T2.DocDate DESC
									                )
									                GROUP BY 
										                T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
										                T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, 
										                T2.DocEntry,T2.DocNum,D.DocEntry,
										                D.DocNum, T0.U_OrdersStatus, T4.U_Status

                                                    UNION ALL

                                                    SELECT 
                                                        '' AS Send,
                                                        T0.DocEntry,
                                                        T0.DocNum AS Document,
                                                        T0.DocTotal AS DocTotal,
                                                        T1.SlpName AS SalesEmployee,
                                                        T0.DocDate AS DocumentDate,
                                                        T0.DocDueDate AS DueDate,
										                0 AS Invoice,
										                0 AS InvoiceDocNum,
										                0 AS Delivery,
										                0 AS DeliveryDocNum,
                                                        T0.CardCode AS Client,
                                                        T0.CardName AS ClientName,
                                                        NULL AS OrderStatus,
                                                        NULL AS OperatorStatus, 
                                                        'Pending' AS Status
                                                    FROM ORDR T0
                                                    LEFT JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
									                INNER JOIN RDR1 R ON T0.DocEntry = R.DocEntry
                                                    WHERE T0.DocStatus = 'O' and T0.U_OrdersStatus is null
                                                    GROUP BY 
                                                        T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
                                                        T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName) AS AllData 
                                                    
                                                    WHERE 1=1"

            If orderStatus <> "All" AndAlso orderStatus <> "" Then
                If orderStatus = "Pending" Then
                    query &= " AND Status = 'Pending'"
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

            If lineColor <> "" Then
                Select Case lineColor
                    Case "Yellow"
                        query &= " AND Status = 'Sent'"
                    Case "Red"
                        query &= " AND Status = 'Error'"
                    Case "Light Green"
                        query &= " AND Status = 'Confirmed'"
                    Case "Green"
                        query &= " AND Status = 'Delivered'"
                    Case "Dark Green"
                        query &= " AND Status = 'Invoiced'"
                    Case "Light Blue"
                        query &= " AND Status = 'Last 15 invoices'"
                    Case "Gray"
                        query &= " AND Status = 'Pending'
                                         AND NOT (DATEDIFF(HOUR, GETDATE(), AllData.DueDate) <= 48 AND DATEDIFF(HOUR, GETDATE(), AllData.DueDate) >= 0)
                                         AND NOT (AllData.DueDate < GETDATE())"
                    Case "Orange"
                        query &= " AND Status = 'Pending' AND DATEDIFF(HOUR, GETDATE(),AllData.DueDate) <= 48  AND DATEDIFF(HOUR,GETDATE(), AllData.DueDate) >= 0"
                    Case "Dark Gray"
                        query &= " AND Status = 'Pending' AND AllData.DueDate < GETDATE()"
                End Select
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

        Catch ex As Exception
            Throw New Exception("LoadOrdersDataInGrid() > " & ex.Message)
        End Try
    End Sub
    Private Sub LoadOrdersDataInGrid()

        Try
            Dim oCombo As SAPbouiCOM.ComboBox = CType(Me.Form.Items.Item("cbOrder").Specific, SAPbouiCOM.ComboBox)
            oCombo.Select("ALL", SAPbouiCOM.BoSearchKey.psk_ByValue)

            Dim oDataTable As SAPbouiCOM.DataTable = Me.Form.DataSources.DataTables.Item(FormControls.DataTable)


            Dim query As String = $"SELECT * FROM (
                                                    SELECT 
                                                        '' AS Send,
                                                        SQ.DocEntry,
                                                        SQ.DocNum AS Document,
                                                        SQ.DocTotal AS DocTotal,
                                                        SQ.SlpName AS SalesEmployee,
                                                        SQ.DocDate AS DocumentDate,
                                                        SQ.DocDueDate AS DueDate,
										                SQ.Invoice,
										                SQ.InvoiceDocNum,
										                SQ.Delivery,
										                SQ.DeliveryDocNum,
                                                        SQ.CardCode AS Client,
                                                        SQ.CardName AS ClientName,
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
											                T2.DocNum AS InvoiceDocNum,
											                D.DocEntry AS Delivery,
											                D.DocNum AS DeliveryDocNum,
                                                            T0.U_OrdersStatus AS OrderStatus,
                                                            T4.U_Status AS OperatorStatus,
                                                            T2.DocDate AS InvoiceDocDate 
                                                        FROM ORDR T0
										                INNER JOIN RDR1 R ON T0.DocEntry = R.DocEntry
										                LEFT JOIN DLN1 D1 ON R.DocEntry = D1.BaseEntry AND R.LineNum = D1.BaseLine AND D1.BaseType = 17  
										                INNER JOIN ODLN D ON D1.DocEntry = D.DocEntry
										                LEFT JOIN INV1 T3 ON D.DocEntry = T3.BaseEntry AND D1.LineNum = T3.BaseLine AND T3.BaseType = 15  
										                INNER JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
                                                        LEFT JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
                                                        LEFT JOIN [@CONFORDERS] T4 ON T0.DocEntry = T4.U_OrderId
                                                        GROUP BY 
                                                            T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
                                                            T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, 
                                                            T2.DocEntry,T2.DocNum,D.DocEntry,
											                D.DocNum, T0.U_OrdersStatus, T4.U_Status, T2.DocDate
                                                        ORDER BY T2.DocDate DESC
                                                    ) AS SQ

                                                    UNION ALL

									                SELECT 
										                '' AS Send,
										                T0.DocEntry,
										                T0.DocNum AS Document,
										                T0.DocTotal AS DocTotal,
										                T1.SlpName AS SalesEmployee,
										                T0.DocDate AS DocumentDate,
										                T0.DocDueDate AS DueDate,
										                ISNULL(T2.DocEntry, 0) AS Invoice,
										                ISNULL(T2.DocNum, 0) AS InvoiceDocNum,
										                ISNULL(D.DocEntry, 0) AS Delivery,
										                ISNULL(D.DocNum, 0) AS DeliveryDocNum,
										                T0.CardCode AS Client,
										                T0.CardName AS ClientName,
										                T0.U_OrdersStatus AS OrderStatus,
										                T4.U_Status AS OperatorStatus,
										                CASE
											                WHEN T0.U_OrdersStatus = 'Sent' THEN 'Sent'
											                WHEN T2.DocEntry IS NOT NULL THEN 'Invoiced'
											                WHEN D.DocEntry IS NOT NULL THEN 'Delivered'
											                WHEN T4.U_Status = 'R' THEN 'Rejected'
											                WHEN T4.U_Status = 'C' THEN 'Confirmed'
										                END AS Status
									                FROM ORDR T0
									                INNER JOIN RDR1 R ON T0.DocEntry = R.DocEntry
									                LEFT JOIN DLN1 D1 ON R.DocEntry = D1.BaseEntry AND R.LineNum = D1.BaseLine AND D1.BaseType = 17  
									                LEFT JOIN ODLN D ON D1.DocEntry = D.DocEntry
									                LEFT JOIN INV1 T3 ON D.DocEntry = T3.BaseEntry AND D1.LineNum = T3.BaseLine AND T3.BaseType = 15  
									                LEFT JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
									                LEFT JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
									                LEFT JOIN [@CONFORDERS] T4 ON T0.DocEntry = T4.U_OrderId
									                WHERE (T0.U_OrdersStatus = 'Sent'
										                    OR T2.DocEntry IS NOT NULL 
											                OR D.DocEntry IS NOT NULL
											                OR T4.U_Status IN ('R', 'C'))
											                AND T0.DocEntry NOT IN (
												                SELECT TOP 15 T0.DocEntry
												                FROM ORDR T0
												                INNER JOIN RDR1 R ON T0.DocEntry = R.DocEntry
												                LEFT JOIN DLN1 D1 ON R.DocEntry = D1.BaseEntry AND R.LineNum = D1.BaseLine AND D1.BaseType = 17  
												                INNER JOIN ODLN D ON D1.DocEntry = D.DocEntry
												                LEFT JOIN INV1 T3 ON D.DocEntry = T3.BaseEntry AND D1.LineNum = T3.BaseLine AND T3.BaseType = 15  
												                INNER JOIN OINV T2 ON T3.DocEntry = T2.DocEntry
												                GROUP BY T0.DocEntry, T2.DocDate
												                ORDER BY T2.DocDate DESC
									                )
									                GROUP BY 
										                T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
										                T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName, 
										                T2.DocEntry,T2.DocNum,D.DocEntry,
										                D.DocNum, T0.U_OrdersStatus, T4.U_Status

                                                    UNION ALL

                                                    SELECT 
                                                        '' AS Send,
                                                        T0.DocEntry,
                                                        T0.DocNum AS Document,
                                                        T0.DocTotal AS DocTotal,
                                                        T1.SlpName AS SalesEmployee,
                                                        T0.DocDate AS DocumentDate,
                                                        T0.DocDueDate AS DueDate,
										                0 AS Invoice,
										                0 AS InvoiceDocNum,
										                0 AS Delivery,
										                0 AS DeliveryDocNum,
                                                        T0.CardCode AS Client,
                                                        T0.CardName AS ClientName,
                                                        NULL AS OrderStatus,
                                                        NULL AS OperatorStatus, 
                                                        'Pending' AS Status
                                                    FROM ORDR T0
                                                    LEFT JOIN OSLP T1 ON T0.SlpCode = T1.SlpCode
									                INNER JOIN RDR1 R ON T0.DocEntry = R.DocEntry
                                                    WHERE T0.DocStatus = 'O' and T0.U_OrdersStatus is null
                                                    GROUP BY 
                                                        T0.DocEntry, T0.DocNum, T0.DocTotal, T1.SlpName, 
                                                        T0.DocDate, T0.DocDueDate, T0.CardCode, T0.CardName) AS AllData 
                                                    ORDER BY AllData.DueDate Asc"
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

        Catch ex As Exception
            Throw New Exception("LoadOrdersDataInGrid() > " & ex.Message)
        End Try
    End Sub

    Private Function GetDelivery(docEntry As Integer) As Delivery
        Try
            Dim serviceLayer As SLConnection = m_SBOAddon.oSLConnection
            Dim response = serviceLayer.Request($"DeliveryNotes({docEntry})").GetAsync(Of JObject)().Result()

            If response Is Nothing OrElse response Is Nothing Then
                Return Nothing
            End If

            Dim delivery As New Delivery With {
               .DocEntry = CInt(response("DocEntry")),
               .CardCode = response("CardCode")?.ToString(),
               .DocDate = If(response("DocDate") IsNot Nothing,
                       DateTime.Parse(response("DocDate").ToString()),
                       DateTime.MinValue),
               .DocDueDate = If(response("DocDueDate") IsNot Nothing,
                        DateTime.Parse(response("DocDueDate").ToString()),
                        DateTime.MinValue),
               .DocumentLines = New List(Of DeliveryLines)()
            }

            If response("DocumentLines") IsNot Nothing Then
                For Each line In response("DocumentLines")
                    delivery.DocumentLines.Add(New DeliveryLines With {
                 .lineNum = CInt(line("LineNum")),
                 .BaseType = line("BaseType"),
                 .BaseEntry = line("BaseEntry")
             })
                Next
            End If

            Return delivery

        Catch ex As Exception
            SBO_Application.StatusBar.SetText($"Error fetching delivery {docEntry}: {ex.Message}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
            Return Nothing
        End Try

    End Function

    Private Function GetOrder(docEntry As Integer) As Order
        Try
            Dim serviceLayer As SLConnection = m_SBOAddon.oSLConnection
            Dim response = serviceLayer.Request($"Orders({docEntry})?$expand=DocumentLines,DocumentLines/BatchNumbers").GetAsync(Of JObject)().Result()

            If response Is Nothing Then
                Return Nothing
            End If

            Dim order = New Order With {
            .DocEntry = CInt(response("DocEntry")),
            .CardCode = response("CardCode")?.ToString(),
            .OrderDate = If(response("DocDate") IsNot Nothing, DateTime.Parse(response("DocDate").ToString()), DateTime.MinValue),
            .DocDueDate = If(response("DocDueDate") IsNot Nothing, DateTime.Parse(response("DocDueDate").ToString()), DateTime.MinValue),
            .DocumentLines = New List(Of OrderLines)()
        }

            If response("DocumentLines") IsNot Nothing Then
                For Each line In response("DocumentLines")
                    Dim newLine As New OrderLines With {
                    .LineNum = CInt(line("LineNum")),
                    .ItemCode = line("ItemCode")?.ToString(),
                    .Quantity = CDbl(line("Quantity")),
                    .DueDate = If(line("ShipDate") IsNot Nothing, DateTime.Parse(line("ShipDate").ToString()), DateTime.MinValue),
                    .BatchNumbers = New List(Of BatchNumber)()
                }

                    If line("BatchNumbers") IsNot Nothing Then
                        For Each batch In line("BatchNumbers")
                            newLine.BatchNumbers.Add(New BatchNumber With {
                            .BatchNumber = batch("BatchNumber")?.ToString(),
                            .Quantity = CDbl(batch("Quantity"))
                        })
                        Next
                    End If

                    order.DocumentLines.Add(newLine)
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

            If (uid = "Date") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Document Date"
                grid.Columns.Item(uid).Editable = False
            End If

            If (uid = "DueDate") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Expected Delivery Date"
                grid.Columns.Item(uid).Editable = False
            End If

            If (uid = "Invoice") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Invoice"
                grid.Columns.Item(uid).LinkedObjectType = SAPbobsCOM.BoObjectTypes.oInvoices
                grid.Columns.Item(uid).Editable = False
            End If

            If (uid = "InvoiceDocNum") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Invoice DocNum"
                grid.Columns.Item(uid).Editable = False
            End If

            If (uid = "Delivery") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Delivery"
                grid.Columns.Item(uid).LinkedObjectType = SAPbobsCOM.BoObjectTypes.oDeliveryNotes
                grid.Columns.Item(uid).Editable = False
            End If

            If (uid = "DeliveryDocNum") Then
                grid.Columns.Item(uid).TitleObject.Caption = "Delivery DocNum"
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
            Dim dueDateObj As Object = grid.DataTable.GetValue("DueDate", i)

            If status = "Sent" Then
                grid.CommonSetting.SetRowBackColor(i + 1, RGB(255, 255, 0)) 'YELLOW
            ElseIf status = "Error" Then
                grid.CommonSetting.SetRowBackColor(i + 1, RGB(139, 0, 0)) 'RED
            ElseIf status = "Confirmed" Then
                grid.CommonSetting.SetRowBackColor(i + 1, RGB(144, 238, 144)) 'LIGHT GREEN
            ElseIf status = "Delivered" Then
                grid.CommonSetting.SetRowBackColor(i + 1, RGB(60, 179, 113)) 'GREEN
            ElseIf status = "Invoiced" Then
                grid.CommonSetting.SetRowBackColor(i + 1, RGB(0, 128, 0)) 'DARK GREEN
            ElseIf status = "Last 15 invoices" Then
                grid.CommonSetting.SetRowBackColor(i + 1, RGB(173, 216, 230)) ' LIGHT BLUE
            ElseIf status = "Pending" Then

                Dim dueDate As DateTime
                Dim isDueDateValid As Boolean = DateTime.TryParse(dueDateObj.ToString(), dueDate)

                If isDueDateValid Then
                    Dim timeUntilDue As TimeSpan = dueDate - DateTime.Now
                    If timeUntilDue.TotalHours <= 48 AndAlso timeUntilDue.TotalSeconds >= 0 Then
                        grid.CommonSetting.SetRowBackColor(i + 1, RGB(255, 165, 0))   ' ORANGE
                    ElseIf timeUntilDue.TotalSeconds < 0 Then
                        grid.CommonSetting.SetRowBackColor(i + 1, RGB(169, 169, 169))  'DARK GRAY
                    Else
                        grid.CommonSetting.SetRowBackColor(i + 1, -1)
                    End If
                End If
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