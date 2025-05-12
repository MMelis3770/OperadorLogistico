Imports System.Threading.Tasks
Imports Newtonsoft.Json.Linq
Imports SAPbobsCOM
Imports SAPbouiCOM
Imports SEI.MonitorDeComandes.SEI_AddonEnum
Imports SEIDOR_SLayer

Public Class SEI_ConfOrders
    Inherits SEI_Form

#Region "ATRIBUTOS"
    Private Const UDO_FormType As String = "UDO_FT_CONF_ORDERS"
    Private Const UDO_ObjectName As String = "CONF_ORDERS"
    Private Const UDO_DetailObjectName As String = "CONF_ORDERLINES"
#End Region

#Region "CONSTANTES"
    Private Structure FormControls
        Const btnAdd As String = "1"
        Const btnCancel As String = "2"

        Const et_DocEntry As String = "1_U_E"
        Const et_CardCode As String = "0_U_E"
        Const CFL_DocEntry As String = "CFL_0"
        Const CFL_CardCode As String = "CFL_1"

    End Structure
#End Region

#Region "CONSTRUCTOR"
    Public Sub New(ByRef parentaddon As SEI_Addon)
        MyBase.New(parentaddon, enSBO_LoadFormTypes.XmlFile, enAddonFormType.f_ConfOrders, enAddonMenus.ConfOrders)
        Me.Initialize()
    End Sub

    Private Sub Initialize()
        Try
            Me.Form.Visible = True
        Catch ex As Exception
            SBO_Application.StatusBar.SetText("Error al cargar la pantalla 'Confirmation Orders'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Me.Form.Close()
        End Try
    End Sub



#End Region

#Region "FUNCIONES EVENTOS"
    Public Overrides Sub HANDLE_DATA_EVENT(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)
    End Sub

    Public Overrides Sub HANDLE_FORM_EVENTS(FormUID As String, ByRef pVal As ItemEvent, ByRef BubbleEvent As Boolean)


        If Trim(pVal.ItemUID) <> "" Then
            Select Case pVal.ItemUID
                Case FormControls.et_DocEntry
                    Select Case pVal.EventType
                        Case SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST
                            Me.HandleChooseFromListEvent(pVal, BubbleEvent)
                    End Select
                Case FormControls.et_CardCode
                    Select Case pVal.EventType
                        Case SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST
                            Me.HandleChooseFromListEvent(pVal, BubbleEvent)
                    End Select
            End Select

        Else
            '
            'Eventos de Formulario
            '
            Select Case pVal.EventType
                Case SAPbouiCOM.BoEventTypes.et_FORM_ACTIVATE
                    '
                Case SAPbouiCOM.BoEventTypes.et_FORM_LOAD
                    ' AFTER
                    ' Se captura al inicializar la clase Initialize()
                Case SAPbouiCOM.BoEventTypes.et_FORM_CLOSE

                Case SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD

                Case SAPbouiCOM.BoEventTypes.et_FORM_RESIZE

            End Select
        End If
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
    Private Sub HandleChooseFromListEvent(ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        Dim oCFLEvento As SAPbouiCOM.IChooseFromListEvent = Nothing
        Dim oDataTable As SAPbouiCOM.DataTable = Nothing
        Dim oEditText As SAPbouiCOM.EditText = Nothing
        Dim stNartText As SAPbouiCOM.StaticText = Nothing

        Try
            If pVal.EventType = SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST Then
                oCFLEvento = DirectCast(pVal, SAPbouiCOM.IChooseFromListEvent)
                oDataTable = oCFLEvento.SelectedObjects

                If Not oDataTable Is Nothing AndAlso oDataTable.Rows.Count > 0 Then
                    Select Case pVal.ItemUID
                        Case FormControls.et_DocEntry
                            Dim docEntry As String = oDataTable.GetValue("DocEntry", 0).ToString()
                            oEditText = DirectCast(Me.Form.Items.Item(FormControls.et_DocEntry).Specific, SAPbouiCOM.EditText)
                            oEditText.Value = docEntry

                            Dim docNum As String = oDataTable.GetValue("DocNum", 0).ToString()
                            SBO_Application.StatusBar.SetText("Pedido seleccionado: " & docNum, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)

                        Case FormControls.et_CardCode
                            Dim cardCode As String = oDataTable.GetValue("CardCode", 0).ToString()
                            Dim cardName As String = oDataTable.GetValue("CardName", 0).ToString()

                            oEditText = DirectCast(Me.Form.Items.Item(FormControls.et_CardCode).Specific, SAPbouiCOM.EditText)
                            oEditText.Value = cardCode

                            SBO_Application.StatusBar.SetText("Cliente seleccionado: " & cardName, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
                    End Select
                End If
            End If
        Catch ex As Exception
            Me.SBO_Application.SetStatusBarMessage("ERROR HandleChooseFromListEvent: " & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, True)
        Finally
            LiberarObjCOM(oCFLEvento)
            LiberarObjCOM(oDataTable)
            LiberarObjCOM(oEditText)
            LiberarObjCOM(stNartText)
        End Try
    End Sub
#End Region

#Region "FUNCIONES FORM"

    Public Sub FilterOrdersByCustomer(ByVal cardCode As String)
        Try
            If String.IsNullOrEmpty(cardCode) Then
                Return
            End If

            Dim oConditions As SAPbouiCOM.Conditions = SBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_Conditions)
            Dim oCondition As SAPbouiCOM.Condition = oConditions.Add()

            oCondition.Alias = "CardCode"
            oCondition.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL
            oCondition.CondVal = cardCode

            SetCFLCondition(FormControls.CFL_DocEntry, oConditions)

            SBO_Application.StatusBar.SetText("Filtro aplicado: Mostrando pedidos del cliente " & cardCode, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
        Catch ex As Exception
            SBO_Application.StatusBar.SetText("Error al filtrar pedidos: " & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub

    Public Sub SetCFLCondition(ByVal cflUID As String, ByVal conditions As SAPbouiCOM.Conditions)
        Try
            Dim oCFL As SAPbouiCOM.ChooseFromList = Me.Form.ChooseFromLists.Item(cflUID)
            oCFL.SetConditions(conditions)
        Catch ex As Exception
            SBO_Application.StatusBar.SetText("Error al establecer condiciones del CFL: " & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub
#End Region

#Region "FUNCIONES GENERALES"


#End Region
End Class