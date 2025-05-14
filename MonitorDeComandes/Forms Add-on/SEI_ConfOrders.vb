Imports System.Collections
Imports System.Threading.Tasks
Imports Newtonsoft.Json.Linq
Imports SAPbobsCOM
Imports SAPbouiCOM
Imports SEI.MonitorDeComandes.SEI_AddonEnum
Imports SEIDOR_SLayer

Public Class SEI_ConfOrders
    Inherits SEI_Form

#Region "ATRIBUTOS"

#End Region

#Region "CONSTANTES"
    Private Structure FormControls
        Const btnAdd As String = "1"
        Const btnCancel As String = "2"

        Const et_U_DocEntry As String = "1_U_E"
        Const CFL_U_DocEntry As String = "CFL_0"

        Const et_DocEntry As String = "Item_1"
        Const CFL_DocEntry As String = "CFL_2"

    End Structure
#End Region

#Region "CONSTRUCTOR"
    Public Sub New(ByRef parentaddon As SEI_Addon)
        MyBase.New(parentaddon, enSBO_LoadFormTypes.XmlFile, enAddonFormType.f_ConfOrders, enAddonMenus.ConfOrders, "CONFORDERS")
        Me.Initialize()
    End Sub

    Private Sub Initialize()
        Try
            'ConfigurateChooseFromList()

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
    End Sub
#End Region

#Region "FUNCIONES FORM"

    Private Sub ConfigurateChooseFromList()

        'Dim oParams As SAPbouiCOM.ChooseFromListCreationParams = Nothing
        'Dim oEdit As SAPbouiCOM.EditText = Nothing

        'Try

        '    ' U_DocEntry
        '    oParams = SBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_ChooseFromListCreationParams)
        '    With oParams
        '        .MultiSelection = False
        '        .ObjectType = "17"
        '        .UniqueID = "CFL_0"
        '    End With
        '    Me.Form.ChooseFromLists.Add(oParams)

        '    oEdit = Me.Form.Items.Item(FormControls.et_U_DocEntry).Specific
        '    oEdit.ChooseFromListUID = "CFL_0"
        '    oEdit.ChooseFromListAlias = "DocEntry"

        '    ' DocEntry
        '    oParams = SBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_ChooseFromListCreationParams)
        '    With oParams
        '        .MultiSelection = False
        '        .ObjectType = "CONF_ORDERS"
        '        .UniqueID = "CFL_2"
        '    End With
        '    Me.Form.ChooseFromLists.Add(oParams)

        '    oEdit = Me.Form.Items.Item(FormControls.et_DocEntry).Specific
        '    oEdit.ChooseFromListUID = "CFL_2"
        '    oEdit.ChooseFromListAlias = "DocEntry"



        'Catch ex As Exception
        '    Throw New Exception("CrearChooseFromList() > " & ex.Message)
        'Finally
        '    mGlobals.LiberarObjCOM(oEdit)
        '    mGlobals.LiberarObjCOM(oParams)
        'End Try
    End Sub
#End Region

#Region "FUNCIONES GENERALES"


#End Region
End Class