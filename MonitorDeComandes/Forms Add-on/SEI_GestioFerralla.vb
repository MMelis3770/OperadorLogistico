Option Explicit On

Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Net.Http
Imports System.Reflection
Imports System.Security.Cryptography
Imports System.Xml.Linq
Imports System.Text.Json
Imports SAPbobsCOM
Imports SAPbouiCOM
Imports SEI.SEI_ADDON.SEI_AddonEnum
Imports SEIDOR_SLayer
Imports CrystalDecisions.CrystalReports.Engine

Public Class SEI_GestioFerralla
    Inherits SEI_Form

#Region "ATRIBUTOS"
    Dim AllSelected As Boolean = True
    Dim LotesBloqueados As Boolean = False
#End Region

#Region "CONSTANTES"
    Private Structure FormControls
        Const btnCancelar As String = "2"
        Const btnRecargar As String = "btnRecarg"
        Const btnProcesar As String = "Item_0"
        Const grid As String = "grid"
    End Structure
    Private Structure GridColumns
        Const colChkBox As String = "Seleccion"
        Const colCodArticulo As String = "ItemCode"
        Const colLote As String = "Lote"
        Const colStatusLote As String = "CardCode"
        Const colChatarra As String = "Chatarra"
        Const colCantidad As String = "Cantidad"
        Const colPeso As String = "Peso"
        Const colAbsEntry As String = "AbsEntry"
        Const colPesoTeorico As String = "PesoTeorico"
        Const colEntradaMcia As String = "EntradaMercancia"
        Const colSalidaMcia As String = "SalidaMercancia"
    End Structure


#End Region

#Region "CONSTRUCTOR"
    Public Sub New(ByRef parentaddon As SEI_Addon)

        'Afegir UDO
        MyBase.New(parentaddon, enSBO_LoadFormTypes.XmlFile, enAddonFormType.f_GestionChatarra, enAddonMenus.GestionChatarra)

        Me.Initialize()
    End Sub

    Private Sub Initialize()
        Try
            Me.Form.Freeze(True)
            If Not FillMatrix() Then
                Me.Form.Close()
                Exit Sub
            End If

            Me.Form.Visible = True
            Me.Form.Freeze(False)

        Catch ex As Exception
            SBO_Application.StatusBar.SetText("Error al cargar la pantalla ‘Gestión documentos’", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
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
                    Case FormControls.grid
                        HandleGridEvents(pVal, BubbleEvent)
                    Case FormControls.btnProcesar
                        HandleBtnProcesar(pVal, BubbleEvent)
                    Case FormControls.btnRecargar
                        HandleBtnRecargar(pVal, BubbleEvent)
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

#Region "FUNCIONES"
#Region "INICIALIZADORES"

#End Region
#Region "HANDLERS"
    Private Sub HandleGridEvents(ByRef pVal As SAPbouiCOM.IItemEvent, ByRef BubbleEvent As Boolean)
        If pVal.ColUID = GridColumns.colLote And pVal.EventType = BoEventTypes.et_MATRIX_LINK_PRESSED And pVal.BeforeAction And pVal.Row > -1 Then
            Dim oGrid As SAPbouiCOM.Grid = Me.Form.Items.Item(FormControls.grid).Specific
            Dim sDocEntry As String = oGrid.DataTable.GetValue("AbsEntry", oGrid.GetDataTableRowIndex(pVal.Row))
            '
            BubbleEvent = False
            SBO_Application.OpenForm(10000044, "", sDocEntry)
            '
            LiberarObjCOM(oGrid)
        End If
        Grid_Click(pVal)
    End Sub

    Private Sub HandleBtnRecargar(pVal As ItemEvent, ByRef BubbleEvent As Boolean)

        If pVal.EventType <> BoEventTypes.et_ITEM_PRESSED Then
            Exit Sub
        End If
        If pVal.BeforeAction Then
            If Me.SBO_Application.MessageBox($"¿Recargar pantalla?", 1, "Sí", "No") = 2 Then
                BubbleEvent = False
                Exit Sub
            End If

        Else
            FillMatrix()
            Me.Form.Items.Item(FormControls.btnProcesar).Enabled = True

        End If

    End Sub

    Private Sub HandleBtnProcesar(pVal As ItemEvent, ByRef BubbleEvent As Boolean)
        If pVal.EventType <> BoEventTypes.et_ITEM_PRESSED Then
            Exit Sub
        End If
        If pVal.BeforeAction Then
            If Me.SBO_Application.MessageBox($"¿Crear documentos de mercancías?", 1, "Sí", "No") = 2 Then
                BubbleEvent = False
                Exit Sub
            End If

        Else
            Dim oGrid As SAPbouiCOM.Grid
            Try
                Me.Form.Freeze(True)
                Dim errores As Boolean
                Me.Form.Items.Item(FormControls.btnProcesar).Enabled = False
                SBO_Application.StatusBar.SetSystemMessage($"Procesando lotes...",, BoStatusBarMessageType.smt_Warning)

                oGrid = Me.Form.Items.Item(FormControls.grid).Specific

                For i As Integer = 0 To oGrid.DataTable.Rows.Count - 1
                    If oGrid.DataTable.GetValue("Seleccion", i) = "Y" Then
                        If oGrid.DataTable.GetValue("Estado", i) <> "Liberado" Then

                            oGrid.CommonSetting.SetRowBackColor(i + 1, RGB(255, 0, 0))
                            oGrid.DataTable.SetValue("Errores", i, $"Lote en estado {oGrid.DataTable.GetValue("Estado", i)} ")
                            Continue For
                        End If
                        SBO_Application.StatusBar.SetSystemMessage($"Procesando lote " & oGrid.DataTable.GetValue("Lote", i) & "...",, BoStatusBarMessageType.smt_Warning)

                        Dim result = GenerarDocumentosAsync(i).Result
                        If result = False Then errores = True
                    End If
                Next

                If errores Then
                    SBO_Application.StatusBar.SetSystemMessage("Han habido errores al procesar los lotes.",, BoStatusBarMessageType.smt_Error)
                    SBO_Application.MessageBox("Han habido errores al procesar los lotes.")
                End If

                SBO_Application.StatusBar.SetSystemMessage("Proceso terminado",, BoStatusBarMessageType.smt_Success)
                SBO_Application.MessageBox("Proceso terminado")

            Catch ex As Exception
                SBO_Application.StatusBar.SetSystemMessage($"Errores al procesar lotes: " & ex.Message,, BoStatusBarMessageType.smt_Error)
                SBO_Application.MessageBox($"Errores al procesar lotes: " & ex.Message)

            Finally
                Me.Form.Freeze(False)
                LiberarObjCOM(oGrid)
            End Try
        End If

    End Sub
#End Region
#Region "FUINCIONES GENERALES"

    Private Function FillMatrix() As Boolean
        Dim grid As SAPbouiCOM.Grid = Nothing
        Try
            Me.Form.Freeze(True)
            Dim query = GetData()

            Dim result = Me.m_ParentAddon.oDatabaseConnection.Query(Of Object)(query)

            grid = Me.Form.Items.Item(FormControls.grid).Specific

            If (result.Count() = 0) Then
                grid.DataTable.Clear()

                Me.SBO_Application.StatusBar.SetText("No hay material pendiente.", BoMessageTime.bmt_Medium, BoStatusBarMessageType.smt_Warning)
                Me.SBO_Application.MessageBox("No hay material pendiente.")

                Return False
            End If

            grid.DataTable.SetValuesDynamic(Of Object)(result)

            ConfigurateGrid(grid)
            Return True
        Catch ex As Exception
            Throw
        Finally
            LiberarObjCOM(grid)
            Me.Form.Freeze(False)
        End Try
    End Function
    Private Sub Grid_Click(ByVal pVal As SAPbouiCOM.ItemEvent)

        If (pVal.ColUID = "Seleccion") And pVal.Row = -1 And pVal.EventType = BoEventTypes.et_DOUBLE_CLICK And Not pVal.BeforeAction Then
            ToggleCheckboxes(pVal.ColUID)
        End If
    End Sub
    Private Sub ToggleCheckboxes(columnID As String)
        Dim oGrid As SAPbouiCOM.Grid = Me.Form.Items.Item(FormControls.grid).Specific
        Dim checkValue As String = "Y"
        Try

            If Me.AllSelected Then Me.AllSelected = False Else Me.AllSelected = True

            Me.FillMatrix()

        Catch ex As Exception
            Throw ex
        Finally
            LiberarObjCOM(oGrid)
        End Try
    End Sub
    Private Sub ConfigurateGrid(grid As SAPbouiCOM.Grid)
        Dim xmlDoc = XDocument.Parse(grid.DataTable.SerializeAsXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_All))

        Dim xmlColumns = xmlDoc.Descendants("Column")

        For Each column In xmlColumns
            Dim uid As String = column.Attribute("Uid").Value
            If (uid = GridColumns.colChkBox) Then
                grid.Columns.Item(uid).TitleObject.Caption = " "
                grid.Columns.Item(uid).Width = 15
                grid.Columns.Item(uid).Type = SAPbouiCOM.BoGridColumnType.gct_CheckBox
            Else
                grid.Columns.Item(uid).Editable = False
            End If
            If (uid = GridColumns.colCodArticulo) Then
                grid.Columns.Item(uid).TitleObject.Caption = "Artículo"
                grid.Columns.Item(uid).LinkedObjectType = SAPbobsCOM.BoObjectTypes.oProductTrees

            End If
            If (uid = GridColumns.colEntradaMcia) Then
                grid.Columns.Item(uid).TitleObject.Caption = "Entrada mcía"
                grid.Columns.Item(uid).LinkedObjectType = SAPbobsCOM.BoObjectTypes.oInventoryGenEntry

            End If
            If (uid = GridColumns.colSalidaMcia) Then
                grid.Columns.Item(uid).TitleObject.Caption = "Salida mcía"
                grid.Columns.Item(uid).LinkedObjectType = SAPbobsCOM.BoObjectTypes.oInventoryGenExit

            End If
            If (uid = GridColumns.colAbsEntry) Then

                grid.Columns.Item(uid).Visible = False

            End If
            If (uid = GridColumns.colLote) Then
                grid.Columns.Item(uid).LinkedObjectType = 10000044

            End If
            If (uid = GridColumns.colChatarra) Then
                grid.Columns.Item(uid).LinkedObjectType = SAPbobsCOM.BoObjectTypes.oItems

            End If
            If (uid = GridColumns.colPeso) Then
                grid.Columns.Item(uid).TitleObject.Caption = "Peso (kg)"

            End If
            If (uid = GridColumns.colPesoTeorico) Then
                grid.Columns.Item(uid).TitleObject.Caption = "Peso teórico (Kg)"

            End If

        Next
        grid.DataTable.LoadSerializedXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_DataOnly, xmlDoc.ToString())
        For i = 0 To grid.Rows.Count - 1
            grid.CommonSetting.SetRowBackColor(i + 1, -1)
        Next
        grid.AutoResizeColumns()
    End Sub

    Private Function GetData()
        ' ---T2.""ItemName"", 
        Dim sSelected As String
        If Me.AllSelected Then sSelected = "Y" Else sSelected = "N"

        Dim query = $"SELECT
                         '{sSelected}' AS ""Seleccion"",
                         T0.""ItemCode"",
                           T0.""AbsEntry"",
	                     T0.""DistNumber"" as ""Lote"",
	                     CASE
	                     WHEN T0.""Status""= 0 THEN 'Liberado' 
	                     WHEN T0.""Status"" = 1 THEN 'Acceso denegado'
	                     WHEN T0.""Status"" = 2 THEN 'Bloqueado'
	                     END AS ""Estado"",
                         T4.""Code"" AS ""Chatarra"",
	                     ROUND(T1.""Quantity"",5) as ""Cantidad"",
	                     ROUND((T2.""SWeight1"" * t5.""WightInMG"" )/ 1000000,5) as ""Peso"" ,
	                     ROUND((((T2.""SWeight1"" * t5.""WightInMG"" )/ 1000000) * T1.""Quantity""),5)  as ""PesoTeorico"",
                         ' ' AS ""EntradaMercancia"",
                         ' ' AS ""SalidaMercancia"",
                         ' ' as ""Errores""

                    FROM
                         OBTN T0
                         inner join OBTQ T1 on T0.""ItemCode"" = T1.""ItemCode"" and T0.""SysNumber"" = T1.""SysNumber""
                         inner join OITM T2 on T0.""ItemCode"" = T2.""ItemCode""
	                     INNER JOIN OITT T3 ON T0.""ItemCode"" = T3.""Code""
	                     INNER JOIN ITT1 T4 ON T3.""Code"" = T4.""Father""
                         INNER JOIN OWGT T5 ON T5.""UnitCode"" = T2.""SWght1Unit""
                    WHERE
                         T1.""Quantity"" > 0 AND T1.""WhsCode"" = '39' and T4.""Code"" LIKE 'SC%'
                    ORDER BY
                         T1.""WhsCode"", T0.""DistNumber""
                    "
        query = ConvertToHANA(SBO_Company, query)
        Return query
    End Function

    Private Sub ValidateRows()

    End Sub
    Private Async Function GenerarDocumentosAsync(ByVal index As Integer) As Threading.Tasks.Task(Of Boolean)
        Dim oGrid As SAPbouiCOM.Grid = Me.Form.Items.Item(FormControls.grid).Specific
        Try
            Dim oSalidaMercancias As Object = CrearSalidaMercancias(index)
            Dim oEntradaMercancias As Object = CrearEntradaMercancias(index)

            Dim SLSalidaMercancias = New SLBatchRequest(
            HttpMethod.Post,' HTTP method
            "InventoryGenExits",' resource
            oSalidaMercancias)
            SLSalidaMercancias.ContentID = 1
            Dim SLEntradaMercancias = New SLBatchRequest(
            HttpMethod.Post,' HTTP method
            "InventoryGenEntries",' resource
            oEntradaMercancias)
            SLEntradaMercancias.ContentID = 2
            Dim batchResult As HttpResponseMessage() = Await m_ParentAddon.oSLConnection.PostBatchAsync(SLSalidaMercancias, SLEntradaMercancias)
            Dim InvExitNum = batchResult(0).Headers.Location.LocalPath.Split("(")(1).Replace(")", "")
            Dim InvEntryNum = batchResult(1).Headers.Location.LocalPath.Split("(")(1).Replace(")", "")
            oGrid.CommonSetting.SetRowBackColor(index + 1, RGB(0, 255, 0))
            oGrid.DataTable.SetValue("Errores", index, "Procesado")
            oGrid.DataTable.SetValue("EntradaMercancia", index, InvEntryNum)
            oGrid.DataTable.SetValue("SalidaMercancia", index, InvExitNum)
            Return True
        Catch ex As Exception
            oGrid.CommonSetting.SetRowBackColor(index + 1, RGB(255, 0, 0))
            oGrid.DataTable.SetValue("Errores", index, ex.Message)

            Return False
        Finally
            LiberarObjCOM(oGrid)
        End Try



    End Function
    Private Function CrearSalidaMercancias(ByVal index As Integer) As Object
        Dim oGrid As SAPbouiCOM.Grid = Me.Form.Items.Item(FormControls.grid).Specific


        Dim oSalidaMercancia As New DocMercancias With {
            .DocDate = DateTime.Now,
            .Comments = "Salida creada a partir del proceso de vaciado.",
            .DocumentLines = New List(Of LineasMercancias)
        }

        Dim oLinea As New LineasMercancias With {
            .ItemCode = oGrid.DataTable.GetValue("ItemCode", index),
            .Quantity = mGlobals.String_To_Double(oGrid.DataTable.GetValue("Cantidad", index)),
            .WarehouseCode = "39",
            .BatchNumbers = New List(Of BatchNumbers)
        }
        Dim oBatchNum As New BatchNumbers With {
            .BatchNumber = oGrid.DataTable.GetValue("Lote", index),
            .Quantity = mGlobals.String_To_Double(oGrid.DataTable.GetValue("Cantidad", index))
        }

        oLinea.BatchNumbers.Add(oBatchNum)
        oSalidaMercancia.DocumentLines.Add(oLinea)
        LiberarObjCOM(oGrid)

        Return oSalidaMercancia
    End Function
    Private Function CrearEntradaMercancias(ByVal index As Integer) As Object
        Dim oGrid As SAPbouiCOM.Grid = Me.Form.Items.Item(FormControls.grid).Specific

        Dim oEntradaMercancia As New DocMercancias With {
            .DocDate = DateTime.Now,
            .Comments = "Entrada creada a partir del proceso de vaciado.",
            .DocumentLines = New List(Of LineasMercancias)
        }

        Dim oLinea As New LineasMercancias With {
            .WarehouseCode = "40",
            .ItemCode = oGrid.DataTable.GetValue("Chatarra", index),
            .Quantity = mGlobals.String_To_Double(oGrid.DataTable.GetValue("PesoTeorico", index))
            }

        oEntradaMercancia.DocumentLines.Add(oLinea)
        LiberarObjCOM(oGrid)
        Return oEntradaMercancia
    End Function
#End Region
#End Region
End Class