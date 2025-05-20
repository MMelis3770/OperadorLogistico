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


    End Structure
#End Region

#Region "CONSTRUCTOR"
    Public Sub New(ByRef parentaddon As SEI_Addon)
        MyBase.New(parentaddon, enSBO_LoadFormTypes.XmlFile, enAddonFormType.f_ConfOrders, enAddonMenus.ConfOrders, "CONFORDERS")
        Me.Initialize()
    End Sub

    Private Sub Initialize()
        Try
            ConfigurarMatrix()
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

    Private Sub ConfigurarMatrix()
        Try
            Dim oMatrix As SAPbouiCOM.Matrix = CType(Form.Items.Item("Item_5").Specific, SAPbouiCOM.Matrix)

            If oMatrix IsNot Nothing Then
                With oMatrix
                    .Columns.Item("#").Width = 22
                    .Columns.Item("C_0_3").Width = 130
                    .Columns.Item("C_0_1").Width = 130
                    .Columns.Item("C_0_2").Width = 130
                    .Columns.Item("C_0_4").Width = 130
                End With
            End If

        Catch ex As Exception
            SBO_Application.StatusBar.SetText($"Error al configurar Matrix: {ex.Message}",
            SAPbouiCOM.BoMessageTime.bmt_Short,
            SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub

#End Region

#Region "FUNCIONES GENERALES"


#End Region
End Class