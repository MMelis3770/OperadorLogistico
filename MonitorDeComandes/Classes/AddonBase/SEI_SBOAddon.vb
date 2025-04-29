Option Explicit On
Imports SAPbouiCOM

Public Class SEI_SBOAddon
    Inherits SEI_Addon

#Region "Contructor"
    Public Sub New(ByVal AddonName As String, ByRef pbo_RunApplication As Boolean)
        MyBase.New(AddonName)
        If IsNothing(Me.SBO_Application) Or IsNothing(Me.SBO_Company) Then
            'Starting the Application
            pbo_RunApplication = False
            Exit Sub
        Else
            pbo_RunApplication = True
        End If
        Me.Initialize()
    End Sub
    Private Sub Initialize()
        Me.BuildMenus()
        Me.SetEventFilters()
        Me.Get_AddonSettings(Me.SBO_Company)
        Me.SBO_Application.StatusBar.SetText("Add-on iniciado", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
    End Sub
#End Region

#Region "Handles Events"
    Public Overrides Sub Handle_SBO_AppEvent(ByVal EventType As SAPbouiCOM.BoAppEventTypes)
        Select Case EventType
            Case SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged
                SBO_Company.Disconnect()
                If Not MyBase.ConnectToSBO Then
                    System.Windows.Forms.Application.Exit()
                End If
                Me.Initialize()

            Case SAPbouiCOM.BoAppEventTypes.aet_LanguageChanged
                Me.Initialize()

            Case SAPbouiCOM.BoAppEventTypes.aet_ServerTerminition
                MsgBox("El servidor de UI se ha parado y es esencial para el funcionamiento del Add-On" & vbCrLf &
                ".El Add-on " & m_Name & " se ha cerrado." & vbCrLf &
                "Por favor reinicie SAP Business One.", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Warning")
                System.Windows.Forms.Application.Exit()

            Case SAPbouiCOM.BoAppEventTypes.aet_ShutDown
                System.Windows.Forms.Application.Exit()

        End Select
    End Sub
    Private Sub Iniciar_Configuracion()
        Dim ls As String = ""
        Dim oRecordset As SAPbobsCOM.Recordset = SBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        '
        ls = " SELECT ""Code"" FROM ""OUDO"" WHERE ""Code"" = 'SEICONFIG' " & vbCrLf
        oRecordset.DoQuery(ls)
        '
        If Not oRecordset.EoF Then
            LiberarObjCOM(oRecordset)

            ' SI EXISTE UDO ABRO FORMULARIO DE CONFIGURACIÓN:
            Dim oForm As SEI_Form
            oForm = New SEI_AddonSettings(Me)
        Else
            LiberarObjCOM(oRecordset)
            If SBO_Application.MessageBox("Se van a crear las tablas de configuración. Se recomienda reiniciar SAP después de que el proceso termine. ¿Continuar?", 1, "Si", "No") = 2 Then Exit Sub
            ' SI NO EXISTE UDO CREO CAMPOS DE CONFIGURACIÓN:
            Dim oCreateFields As New SEI_CreateTables(Me)
            oCreateFields.CreateSEICONFIG()
            '
            SBO_Application.MessageBox("Tabla configuración creada correctamente. Reinicie SAP.")
        End If
    End Sub
    Public Overrides Sub Handle_SBO_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        Dim oForm As SEI_Form
        '
        Try
            '-----------------------------------------------------
            ' Validar Formulario Modal 
            '-----------------------------------------------------
            '[Bloquejo Events a Formulari Origen]:
            For Each oForm In col_SBOFormsOpened
                If oForm.UniqueID = FormUID And oForm.FormTargetID <> "" Then
                    BubbleEvent = False
                    Exit Sub
                End If
            Next
            '
            If Not pVal.BeforeAction Then
                '// AFTER ACTION     
                Select Case pVal.EventType

                    Case SAPbouiCOM.BoEventTypes.et_FORM_LOAD
                        '[PROGRAMAR]
                        Select Case pVal.FormTypeEx
                            'Case SEI_AddonEnum.enSAPFormType.f_AlbaranCompras
                            '    oForm = New SEI_AlbaranCompras(Me, FormUID)
                        End Select
                        '[FI PROGRAMAR]
                        '
                    Case SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD
                        'Cancel·lar Form Modal:
                        For Each oForm In col_SBOFormsOpened
                            If oForm.UniqueID = FormUID Then
                                ' Actualitzo el form origen perquè es pugui tancar:
                                If oForm.FormOriginID <> "" Then
                                    For Each oForm1 In col_SBOFormsOpened
                                        If oForm1.UniqueID = oForm.FormOriginID Then
                                            oForm1.FormTargetID = ""
                                            Exit For
                                        End If
                                    Next
                                End If
                                ' Eliminar Formulari que es tanca de llista de formularis oberts:
                                col_SBOFormsOpened.Remove(FormUID)
                                Exit For
                            End If
                        Next
                    Case Else
                        ' Enviament d'events al formulari actiu:
                        For Each oForm In col_SBOFormsOpened
                            If oForm.UniqueID = FormUID Then
                                oForm.HANDLE_FORM_EVENTS(FormUID, pVal, BubbleEvent)
                            End If
                        Next

                End Select
            Else
                ' BEFORE ACTION
                ' Enviament d'events al formulari actiu:
                For Each oForm In col_SBOFormsOpened
                    If oForm.UniqueID = FormUID Then
                        oForm.HANDLE_FORM_EVENTS(FormUID, pVal, BubbleEvent)
                    End If
                Next

            End If
        Catch ExcE As Exception
            Me.SBO_Application.MessageBox(ExcE.Message)
        End Try

    End Sub
    Public Overrides Sub Handle_SBO_DataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)
        Dim oForm As SEI_Form
        '
        Try
            ' Enviament d'events de procés de dades al formulari actiu:
            For Each oForm In col_SBOFormsOpened
                If oForm.UniqueID = BusinessObjectInfo.FormUID Then
                    oForm.HANDLE_DATA_EVENT(BusinessObjectInfo, BubbleEvent)
                End If
            Next

        Catch ExcE As Exception
            Me.SBO_Application.MessageBox(ExcE.Message)
        End Try
        '
    End Sub
    Public Overrides Sub Handle_SBO_MenuEvent(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean)
        Try
            If pVal.BeforeAction Then
                ' BEFORE MENU ACTION
                If Me.col_SBOFormsOpened.Count <> 0 Then
                    Dim oForm As SEI_Form
                    For Each oForm In Me.col_SBOFormsOpened
                        If oForm.UniqueID = SBO_Application.Forms.ActiveForm.UniqueID Then
                            oForm.HANDLE_MENU_EVENTS(pVal, BubbleEvent)
                        End If
                    Next
                End If
            Else
                '----------------------------------------------
                '[PROGRAMAR] MENÚS y FORMULARIS D'USUARI
                '----------------------------------------------
                ' AFTER MENU ACTION     
                Select Case pVal.MenuUID.ToUpper
                    Case SEI_AddonEnum.enAddonMenus.ConfigurarAddon.ToUpper
                        Iniciar_Configuracion()
                    Case SEI_AddonEnum.enAddonMenus.OrdersMonitor.ToUpper
                        Dim oForm As SEI_Form
                        oForm = New SEI_OrdersMonitor(Me)
                    Case SEI_AddonEnum.enAddonMenus.ConfOrders.ToUpper
                        Dim oForm As SEI_Form
                        oForm = New SEI_ConfOrders(Me)


                    Case Else
                        ' AFTER MENU ACTION (Events)
                        If Me.col_SBOFormsOpened.Count <> 0 Then
                            Dim oForm As SEI_Form
                            For Each oForm In Me.col_SBOFormsOpened
                                If oForm.UniqueID = SBO_Application.Forms.ActiveForm.UniqueID Then
                                    oForm.HANDLE_MENU_EVENTS(pVal, BubbleEvent)
                                End If
                            Next
                        End If
                End Select
            End If

        Catch excE As Exception
            SBO_Application.MessageBox(excE.Message)
        End Try
    End Sub
    Public Overrides Sub Handle_ReportDataEvent(ByRef eventInfo As SAPbouiCOM.ReportDataInfo, ByRef BubbleEvent As Boolean)
        Dim oForm As SEI_Form
        '
        Try
            '  Enviar Manejador de Eventos   al formulario Activo
            For Each oForm In col_SBOFormsOpened
                If oForm.UniqueID = eventInfo.FormUID Then
                    oForm.HANDLE_REPORT_DATA_EVENT(eventInfo, BubbleEvent)
                End If
            Next

        Catch ExcE As Exception
            Me.SBO_Application.StatusBar.SetText(ExcE.ToString, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try

    End Sub
    Public Overrides Sub Handle_SBO_PrintEvent(ByRef eventInfo As SAPbouiCOM.PrintEventInfo, ByRef BubbleEvent As Boolean)
        Dim oForm As SEI_Form
        '
        Try
            For Each oForm In col_SBOFormsOpened
                If oForm.UniqueID = eventInfo.FormUID Then
                    oForm.HANDLE_PRINT_EVENT(eventInfo, BubbleEvent)
                End If
            Next

        Catch ExcE As Exception
            Me.SBO_Application.StatusBar.SetText(ExcE.ToString, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub
    Public Overrides Sub Handle_SBO_RightClickEvent(ByRef eventInfo As SAPbouiCOM.ContextMenuInfo, ByRef BubbleEvent As Boolean)
        Dim oForm As SEI_Form
        '
        Try
            For Each oForm In col_SBOFormsOpened
                If oForm.UniqueID = eventInfo.FormUID Then
                    oForm.HANDLE_RIGHTCLICK_EVENT(eventInfo, BubbleEvent)
                End If
            Next

        Catch ExcE As Exception
            Me.SBO_Application.StatusBar.SetText(ExcE.ToString, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub
    Public Overrides Sub Handle_SBO_Layoutkeyevent(ByRef EventInfo As SAPbouiCOM.LayoutKeyInfo, ByRef BubbleEvent As Boolean)
        Dim oForm As SEI_Form
        '
        Try
            '  Enviar Manejador de Eventos   al formulario Activo
            For Each oForm In col_SBOFormsOpened
                If oForm.UniqueID = EventInfo.FormUID Then
                    oForm.HANDLE_LAYOUTKEY_EVENTS(EventInfo, BubbleEvent)
                End If
            Next

        Catch ExcE As Exception
            Me.SBO_Application.StatusBar.SetText(ExcE.ToString, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub
#End Region

    Public Sub SeleccionarMenu(ByRef oAddon As SEI_Addon, ByVal sMenu As String)
        Dim i As Integer
        Dim oMenus As SAPbouiCOM.Menus
        oMenus = oAddon.SBO_Application.Menus.Item("51200").SubMenus
        For i = 0 To oMenus.Count - 1
            If oMenus.Item(i).String.ToUpper = sMenu.ToUpper Then
                oAddon.SBO_Application.ActivateMenuItem(oMenus.Item(i).UID)
                Exit For
            End If
        Next
    End Sub

    Public Function GetUIDUserMenu(ByVal table As String, Optional isUDT As Boolean = True) As String
        Dim oMenu As SAPbouiCOM.MenuItem = Nothing
        Dim sMenuUID As String = ""
        Dim s As String
        GetUIDUserMenu = ""
        Try
            oMenu = m_SBOAddon.SBO_Application.Menus.Item(If(isUDT, "51200", "47616"))
            For i As Integer = 0 To oMenu.SubMenus.Count - 1
                If oMenu.SubMenus.Item(i).String.Length < table.Length Then
                    s = oMenu.SubMenus.Item(i).String
                Else
                    s = oMenu.SubMenus.Item(i).String.Substring(0, table.Length)
                    If s.ToUpper() = table.ToUpper() Then
                        sMenuUID = oMenu.SubMenus.Item(i).UID
                        Exit For
                    End If
                End If
            Next
            If sMenuUID = "" Then Throw New Exception("No existe la tabla " & table)
            GetUIDUserMenu = sMenuUID
        Catch ex As Exception
            Throw New Exception("GetUIDUserMenu() -> " & ex.Message)
        Finally
            LiberarObjCOM(oMenu)
        End Try
    End Function
End Class
