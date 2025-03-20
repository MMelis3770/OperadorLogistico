Option Explicit On
Imports SAPbouiCOM
Imports SEI.MonitorDeComandes.SEI_AddonEnum
Imports SEI.SEI_ADDON.SEI_AddonEnum
Public Class SEI_AlbaranCompras
    Inherits SEI_Form

#Region "CONSTANTES"
    Private Structure Items
        Const btnOk As String = "1"
        Const btnCancel As String = "2"
        Const etCardCode As String = "4"
    End Structure
    Private Structure UDF
        Const sUpgradeByRate As String = "U_SEIPVPorTar"
    End Structure
#End Region

#Region "ATRIBUTOS"
#End Region

#Region "CONSTRUCTOR"
    Public Sub New(ByRef ParentAddon As SEI_Addon, ByVal FormUID As String)
        'Afegir UDO
        MyBase.New(ParentAddon, enSBO_LoadFormTypes.LogicOnly, ParentAddon.SBO_Application.Forms.Item(FormUID).TypeEx, FormUID)
    End Sub
#End Region

#Region "FUNCIONES"

#End Region

#Region "FUNCIONES EVENTOS"
    Public Overrides Sub HANDLE_DATA_EVENT(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)
        Select Case BusinessObjectInfo.EventType
            Case SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD
                Try
                    'If Not BusinessObjectInfo.BeforeAction Then
                    '    'Checks if it isn't enabled the Upgrade by rate
                    '    If (Not HasUpgradeByRateEnabled()) Then
                    '        'Ask the user is it wants to update the rates
                    '        If Me.SBO_Application.MessageBox("¿Desea actualizar la tarifa del proveedor?", 1, "Sí", "No") = 1 Then
                    '            'If says yes prepare the new screen to reasign the rates
                    '            Dim xmlDoc As New XmlDocument()
                    '            xmlDoc.LoadXml(BusinessObjectInfo.ObjectKey)
                    '            Dim form = New SEI_ReasignacionDescuentos(Me.m_ParentAddon, Me.Form.Items.Item(Items.etCardCode).Specific.Value, xmlDoc.SelectSingleNode("//DocEntry").InnerText, DateTime.ParseExact(Me.Form.Items.Item("46").Specific.Value, "yyyyMMdd", CultureInfo.InvariantCulture))
                    '        End If
                    '    End If
                    'End If
                Catch ex As Exception
                    SBO_Application.StatusBar.SetText($"Error en la pantalla: {ex.Message}.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                End Try
        End Select
    End Sub
    Public Overrides Sub HANDLE_FORM_EVENTS(FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
    End Sub
    Public Overrides Sub HANDLE_LAYOUTKEY_EVENTS(ByRef EventInfo As SAPbouiCOM.LayoutKeyInfo, ByRef BubbleEvent As Boolean)
    End Sub
    Public Overrides Sub HANDLE_LUPA(ByRef CampoLupa As String, ByRef aRetorno As System.Collections.Generic.List(Of System.Collections.ArrayList))
    End Sub
    Public Overrides Sub HANDLE_MENU_EVENTS(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean)
    End Sub
    Public Overrides Sub HANDLE_PRINT_EVENT(ByRef eventInfo As SAPbouiCOM.PrintEventInfo, ByRef BubbleEvent As Boolean)
    End Sub
    Public Overrides Sub HANDLE_PROGRESSBAR_EVENTS(ByRef pVal As SAPbouiCOM.IProgressBarEvent, ByRef BubbleEvent As Boolean)
    End Sub
    Public Overrides Sub HANDLE_REPORT_DATA_EVENT(ByRef eventInfo As SAPbouiCOM.ReportDataInfo, ByRef BubbleEvent As Boolean)
    End Sub
    Public Overrides Sub HANDLE_RIGHTCLICK_EVENT(ByRef eventInfo As SAPbouiCOM.IContextMenuInfo, ByRef BubbleEvent As Boolean)
    End Sub
    Public Overrides Sub HANDLE_STATUSBAR_EVENTS(ByRef Text As String, ByRef MessageType As SAPbouiCOM.BoStatusBarMessageType)
    End Sub
    Public Overrides Sub HANDLE_WIDGET_EVENTS(ByRef pWidgetData As SAPbouiCOM.WidgetData, ByRef BubbleEvent As Boolean)
    End Sub
#End Region

#Region "FUNCIONES"
#End Region
End Class
