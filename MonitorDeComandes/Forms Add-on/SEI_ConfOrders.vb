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
        Public Const btnAdd As String = "1"
        Public Const btnUpdate As String = "2"
        Public Const btnRemove As String = "3"
        Public Const btnCancel As String = "4"
        Public Const btnRefresh As String = "5"
    End Structure
#End Region

#Region "CONSTRUCTOR"
    Public Sub New(ByRef parentaddon As SEI_Addon)
        MyBase.New(parentaddon, enSBO_LoadFormTypes.XmlFile, enAddonFormType.f_ConfOrders, enAddonMenus.ConfOrders)
        Me.Initialize()
    End Sub

    Private Sub Initialize()
        Try
            SetFormReadOnly()

            AddHandler SBO_Application.FormDataEvent, AddressOf InterceptUDOFormDataEvent
            AddHandler SBO_Application.ItemEvent, AddressOf InterceptUDOItemEvent
            AddHandler SBO_Application.MenuEvent, AddressOf InterceptUDOMenuEvent

            Me.Form.Visible = True
        Catch ex As Exception
            SBO_Application.StatusBar.SetText("Error al cargar la pantalla 'Confirmation Orders'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Me.Form.Close()
        End Try
    End Sub

    Protected Overrides Sub Finalize()
        Try
            RemoveHandler SBO_Application.FormDataEvent, AddressOf InterceptUDOFormDataEvent
            RemoveHandler SBO_Application.ItemEvent, AddressOf InterceptUDOItemEvent
            RemoveHandler SBO_Application.MenuEvent, AddressOf InterceptUDOMenuEvent
        Catch ex As Exception
            SBO_Application.StatusBar.SetText("Error al finalizar la pantalla 'Confirmation Orders'", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
        MyBase.Finalize()
    End Sub
#End Region

#Region "FUNCIONES EVENTOS"
    Public Overrides Sub HANDLE_DATA_EVENT(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)
        If BusinessObjectInfo.FormTypeEx = UDO_FormType Then
            If BusinessObjectInfo.EventType = BoEventTypes.et_FORM_DATA_ADD Or
               BusinessObjectInfo.EventType = BoEventTypes.et_FORM_DATA_UPDATE Then

                BubbleEvent = False
                SBO_Application.StatusBar.SetText("No está permitido añadir o modificar registros en este objeto", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
            End If
        End If
    End Sub

    Public Overrides Sub HANDLE_FORM_EVENTS(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        Try
            If Me.Form.UniqueID = FormUID Then
                If Trim(pVal.ItemUID) <> "" Then
                    Select Case pVal.ItemUID
                    End Select
                End If

                If pVal.EventType = BoEventTypes.et_CLICK And pVal.BeforeAction Then
                    If pVal.ItemUID = FormControls.btnAdd Or
                       pVal.ItemUID = FormControls.btnUpdate Or
                       pVal.ItemUID = FormControls.btnRemove Then

                        BubbleEvent = False
                        SBO_Application.StatusBar.SetText("No está permitido añadir o modificar registros", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    End If
                End If
            End If
        Catch ex As Exception
            SBO_Application.StatusBar.SetText($"Error en la pantalla: {ex.Message}", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub

    Public Overrides Sub HANDLE_MENU_EVENTS(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean)
        If pVal.BeforeAction Then
            If pVal.MenuUID = "1282" Or
               pVal.MenuUID = "1281" Then

                Dim oForm As SAPbouiCOM.Form = SBO_Application.Forms.ActiveForm

                If oForm.TypeEx = UDO_FormType Then
                    BubbleEvent = False
                    SBO_Application.StatusBar.SetText("No está permitido añadir o modificar registros en este objeto", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                End If
            End If
        End If
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

    Private Sub SetFormReadOnly()
        Try
            If Me.Form IsNot Nothing Then
                Me.Form.Mode = BoFormMode.fm_VIEW_MODE

                If Me.Form.Items.Item(FormControls.btnAdd) IsNot Nothing Then
                    Me.Form.Items.Item(FormControls.btnAdd).Enabled = False
                End If

                If Me.Form.Items.Item(FormControls.btnUpdate) IsNot Nothing Then
                    Me.Form.Items.Item(FormControls.btnUpdate).Enabled = False
                End If

            End If
        Catch ex As Exception
            SBO_Application.StatusBar.SetText("El botón/acción no existe", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
        End Try
    End Sub
#End Region

#Region "INTERCEPTORES DE EVENTOS UDO"

    Private Sub InterceptUDOFormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)
        Try
            If BusinessObjectInfo.FormTypeEx = UDO_FormType Or
               BusinessObjectInfo.Type = UDO_ObjectName Then

                If BusinessObjectInfo.EventType = BoEventTypes.et_FORM_DATA_ADD Or
                   BusinessObjectInfo.EventType = BoEventTypes.et_FORM_DATA_UPDATE Or
                   BusinessObjectInfo.EventType = BoEventTypes.et_FORM_DATA_DELETE Then

                    If BusinessObjectInfo.BeforeAction Then
                        BubbleEvent = False
                        SBO_Application.StatusBar.SetText("No está permitido modificar este objeto de negocio", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    End If
                End If
            End If
        Catch ex As Exception
            SBO_Application.StatusBar.SetText("Error interceptando add, updato o remove", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
        End Try
    End Sub

    Private Sub InterceptUDOItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        Try
            Dim oForm As SAPbouiCOM.Form = Nothing

            Try
                oForm = SBO_Application.Forms.Item(FormUID)
                If oForm.TypeEx <> UDO_FormType Then
                    Return
                End If
            Catch
                Return
            End Try

            If pVal.BeforeAction Then
                If pVal.EventType = BoEventTypes.et_CLICK Then
                    If pVal.ItemUID = "1" Or
                       pVal.ItemUID = "2" Or
                       pVal.ItemUID = "3" Then

                        BubbleEvent = False
                        SBO_Application.StatusBar.SetText("No está permitido modificar este objeto de negocio", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    End If
                End If

                If pVal.EventType = BoEventTypes.et_COMBO_SELECT And pVal.ItemUID = "10002" Then
                    BubbleEvent = False
                    SBO_Application.StatusBar.SetText("No está permitido cambiar el modo de este formulario", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                End If
            End If

            If pVal.EventType = BoEventTypes.et_FORM_LOAD And pVal.After Then
                ConfigureUDOFormReadOnly(oForm)
            End If
        Catch ex As Exception
            SBO_Application.StatusBar.SetText("Error interceptando add, updato o remove", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)

        End Try
    End Sub

    Private Sub InterceptUDOMenuEvent(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean)
        Try
            If SBO_Application.Forms.ActiveForm.TypeEx = UDO_FormType Then
                If pVal.BeforeAction Then
                    If pVal.MenuUID = "1282" Or
                       pVal.MenuUID = "1281" Or
                       pVal.MenuUID = "1283" Or
                       pVal.MenuUID = "1284" Or
                       pVal.MenuUID = "1285" Or
                       pVal.MenuUID = "1290" Or
                       pVal.MenuUID = "1291" Then

                        BubbleEvent = False
                        SBO_Application.StatusBar.SetText("No está permitido modificar este objeto de negocio", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                    End If
                End If
            End If
        Catch ex As Exception
            SBO_Application.StatusBar.SetText("Error interceptando add, updato o remove", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
        End Try
    End Sub

    Private Sub ConfigureUDOFormReadOnly(ByRef oForm As SAPbouiCOM.Form)
        Try
            oForm.Mode = BoFormMode.fm_VIEW_MODE

            Dim standardButtons As String() = {"1", "2", "3"}

            For Each btnID As String In standardButtons
                Try
                    Dim oItem As SAPbouiCOM.Item = oForm.Items.Item(btnID)
                    oItem.Enabled = False
                Catch
                    SBO_Application.StatusBar.SetText("El botón no existe", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)

                End Try
            Next

            For i As Integer = 0 To oForm.Items.Count - 1
                Try
                    Dim oItem As SAPbouiCOM.Item = oForm.Items.Item(i)

                    If oItem.Type = BoFormItemTypes.it_EDIT Or
                       oItem.Type = BoFormItemTypes.it_COMBO_BOX Or
                       oItem.Type = BoFormItemTypes.it_CHECK_BOX Or
                       oItem.Type = BoFormItemTypes.it_LINKED_BUTTON Then

                        oItem.Enabled = False
                    End If
                Catch
                    SBO_Application.StatusBar.SetText("Error", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                End Try
            Next

            For i As Integer = 0 To oForm.Items.Count - 1
                Try
                    Dim oItem As SAPbouiCOM.Item = oForm.Items.Item(i)

                    If oItem.Type = BoFormItemTypes.it_MATRIX Or
                       oItem.Type = BoFormItemTypes.it_GRID Then

                        Dim oMatrix As SAPbouiCOM.Matrix = CType(oItem.Specific, SAPbouiCOM.Matrix)
                        oMatrix.Item.Enabled = False
                    End If
                Catch
                    SBO_Application.StatusBar.SetText("Error", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                End Try
            Next

            SBO_Application.StatusBar.SetText("Formulario abierto en modo de solo lectura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning)
        Catch ex As Exception
            SBO_Application.StatusBar.SetText($"Error al configurar el formulario como solo lectura: {ex.Message}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
        End Try
    End Sub
#End Region

#Region "FUNCIONES GENERALES"
    Public Sub RevokeUDOPermissions()
        Try
            Dim company As SAPbobsCOM.Company = m_ParentAddon.SBO_Company
            If company Is Nothing OrElse company.Connected = False Then
                Return
            End If

            Dim authorizationService As SAPbobsCOM.IUserPermissionTree = company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserPermissionTree)

            Dim permissionRecordset As SAPbobsCOM.Recordset = company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            permissionRecordset.DoQuery("SELECT INTERNAL_K, USER_CODE FROM OUSR WHERE INTERNAL_K > 0")

            While Not permissionRecordset.EoF
                Dim userId As Integer = permissionRecordset.Fields.Item("INTERNAL_K").Value

                If authorizationService.GetByKey(userId) Then
                    For i As Integer = 0 To authorizationService.Permissions.Count - 1
                        authorizationService.Permissions.SetCurrentLine(i)

                        If authorizationService.Permissions.PermissionID.Contains(UDO_ObjectName) Then
                            authorizationService.Permissions.UserPermissions = SAPbobsCOM.BoUPTOptions.bou_FullReadNone

                            Dim result As Integer = authorizationService.Update()
                            If result <> 0 Then
                                Dim errMsg As String = company.GetLastErrorDescription()
                                SBO_Application.StatusBar.SetText($"Error al actualizar permisos: {errMsg}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
                            End If

                            Exit For
                        End If
                    Next
                End If

                permissionRecordset.MoveNext()
            End While

            System.Runtime.InteropServices.Marshal.ReleaseComObject(permissionRecordset)
            System.Runtime.InteropServices.Marshal.ReleaseComObject(authorizationService)

            SBO_Application.StatusBar.SetText("Permisos de UDO actualizados correctamente", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success)
        Catch ex As Exception
            SBO_Application.StatusBar.SetText($"Error al revocar permisos del UDO: {ex.Message}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error)
        End Try
    End Sub

#End Region
End Class