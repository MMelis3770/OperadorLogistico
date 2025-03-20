Option Explicit On

Imports System.Collections.Generic
Imports System.Linq
Imports System.Net.Http
Imports System.Xml.Linq
Imports SAPbouiCOM
Imports SEI.MonitorDeComandes.SEI_AddonEnum
Imports SEIDOR_SLayer

Public Class SEI_MonitorComandes
    Inherits SEI_Form

#Region "ATRIBUTOS"
    Dim AllSelected As Boolean = True
    Dim LotesBloqueados As Boolean = False
#End Region

#Region "CONSTANTES"
    Private Structure FormControls

    End Structure


#End Region

#Region "CONSTRUCTOR"
    Public Sub New(ByRef parentaddon As SEI_Addon)

        MyBase.New(parentaddon, enSBO_LoadFormTypes.XmlFile, enAddonFormType.f_MonitorComandes, enAddonMenus.MonitorComandes)

        Me.Initialize()

    End Sub

    Private Sub Initialize()
        Try


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
                    'Case FormControls.btnCargar
                    '    HandleBtnCargar(pVal, BubbleEvent)
                    'Case FormControls.comboProveedor
                    '    ComboProveedor_COMBO_SELECT(pVal, BubbleEvent)
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




#End Region
#End Region
End Class