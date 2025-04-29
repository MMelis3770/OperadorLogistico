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

        MyBase.New(parentaddon, enSBO_LoadFormTypes.XmlFile, enAddonFormType.f_ConfOrders, enAddonMenus.ConfOrders)

        Me.Initialize()

    End Sub

    Private Sub Initialize()
        Try

            Me.Form.Visible = True

        Catch ex As Exception
            SBO_Application.StatusBar.SetText("Error al cargar la pantalla ‘Confirmation Orders’", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
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
                    'Case FormControls.btnSend
                    '    HandleBtnSend(pVal, BubbleEvent)

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



#End Region
#End Region

End Class