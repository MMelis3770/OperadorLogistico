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
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports SEI.MonitorDeComandes.SEI_AddonEnum

Public Class SEI_ExcelGasoil
    Inherits SEI_Form

#Region "ATRIBUTOS"
    Dim AllSelected As Boolean = True
    Dim LotesBloqueados As Boolean = False
#End Region

#Region "CONSTANTES"
    Private Structure FormControls
        Const btnCargar As String = "Item_0"
        Const comboProveedor As String = "Item_2"
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

        MyBase.New(parentaddon, enSBO_LoadFormTypes.XmlFile, enAddonFormType.f_ExcelGasoil, enAddonMenus.ExcelGasoil)

        Me.Initialize()

    End Sub

    Private Sub Initialize()
        Try

            RellenarComboProveedores()

            Me.Form.Visible = True

        Catch ex As Exception
            SBO_Application.StatusBar.SetText("Error al cargar la pantalla ‘Gestión documentos’", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Me.Form.Close()
        End Try
    End Sub

    Private Sub RellenarComboProveedores()

        Dim oCombo As SAPbouiCOM.ComboBox = Nothing

        Try

            oCombo = Me.Form.Items.Item(FormControls.comboProveedor).Specific

            oCombo.ValidValues.Add("", "")

            Dim rs As SAPbobsCOM.Recordset = Me.SBO_Company.GetBusinessObject(BoObjectTypes.BoRecordset)
            rs.DoQuery("SELECT TOP 3 CardCode, CardName FROM OCRD WHERE ISNULL(CardName,'') <> '' AND CardType = 'S'")

            While Not rs.EoF
                oCombo.ValidValues.Add(rs.Fields.Item("CardCode").Value, rs.Fields.Item("CardName").Value)
                rs.MoveNext()
            End While

            LiberarObjCOM(rs)

            'Me.Form.Items.Item(Cab.ComboModo).DisplayDesc = True

        Catch ex As Exception
            Throw New Exception("RellenarComboProveedores() > " & ex.Message)
        Finally
            LiberarObjCOM(oCombo)
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
                    Case FormControls.btnCargar
                        HandleBtnCargar(pVal, BubbleEvent)
                    Case FormControls.comboProveedor
                        ComboProveedor_COMBO_SELECT(pVal, BubbleEvent)
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

    Private Sub ComboProveedor_COMBO_SELECT(ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)

        If pVal.EventType <> BoEventTypes.et_COMBO_SELECT Then Exit Sub

        Dim combo As SAPbouiCOM.ComboBox = Nothing

        Try

            If Not pVal.BeforeAction Then

                combo = Me.Form.Items.Item(FormControls.comboProveedor).Specific
                Me.SBO_Application.MessageBox($"Valor seleccionado {combo.Selected.Description}")

            End If

        Catch ex As Exception
            Me.SBO_Application.MessageBox("ComboProveedor_COMBO_SELECT() > " & ex.Message)
            BubbleEvent = False
        Finally
            LiberarObjCOM(combo)
        End Try

    End Sub

    Public Sub txtProveedor_CHOOSE_FROM_LIST(ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)

        If pVal.EventType <> BoEventTypes.et_CHOOSE_FROM_LIST Then Exit Sub

        'Dim sCampo As String
        'Dim sDescripcion As String

        'Try

        '    If pVal.BeforeAction = False Then

        '        If pVal.SelectedObjects IsNot Nothing Then

        '            sCampo = Me.Obtener_Campo(pVal.ItemUID)
        '            sDescripcion = Me.Obtener_UDS_Descripcion(pVal.ItemUID)
        '            Try
        '                If Not pVal.SelectedObjects Is Nothing Then
        '                    Me.Form.DataSources.DBDataSources.Item(Tabla.Proyectos).SetValue(sCampo, 0, pVal.SelectedObjects.Columns.Item("CardCode").Cells.Item(0).Value)
        '                    Me.Form.DataSources.UserDataSources.Item(sDescripcion).Value = pVal.SelectedObjects.Columns.Item("CardName").Cells.Item(0).Value
        '                Else
        '                    Me.Form.Items.Item(pVal.ItemUID).Specific.value = ""
        '                End If

        '            Catch ex As Exception
        '            End Try

        '        End If

        '        If Me.Form.Mode = fm_OK_MODE Then Me.Form.Mode = fm_UPDATE_MODE

        '    End If

        'Catch ex As Exception
        '    Me.SBO_Application.MessageBox("txtProveedor_CHOOSE_FROM_LIST() > " & ex.Message)
        '    BubbleEvent = False
        'End Try

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

    Private Sub HandleBtnCargar(pVal As ItemEvent, ByRef BubbleEvent As Boolean)

        If pVal.EventType <> BoEventTypes.et_ITEM_PRESSED Then
            Exit Sub
        End If

        If pVal.BeforeAction Then
            Me.SBO_Application.MessageBox("BeforeAction")
            BubbleEvent = False
        Else
            Me.SBO_Application.MessageBox("AfterAction")
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