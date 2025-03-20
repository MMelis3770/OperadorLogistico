Option Explicit On
Option Strict Off

Imports System.Net
Imports SAPbouiCOM
Imports SAPbouiCOM.BoAutoFormMode
Imports SAPbouiCOM.BoAutoManagedAttr
Imports SAPbouiCOM.BoCellClickType
Imports SAPbouiCOM.BoEventTypes
Imports SAPbouiCOM.BoFormMode
Imports SAPbouiCOM.BoModeVisualBehavior
Imports SEI.MonitorDeComandes.SEI_AddonEnum
Imports SEI.SEI_ADDON.SEI_AddonEnum

Public Class SEI_AddonSettings
    Inherits SEI_Form

#Region "Contants"
    Public Structure Items
        ' Botons
        Const btnOk As String = "1"
        Const btnCancel As String = "2"
        Const ButtonCrearCampos As String = "Item_4"
        ' EditTexts
        Const EditCode As String = "Item_5"
        Const EditServerUser As String = "Item_0"
        Const EditServerPassword As String = "Item_2"
        'Other:
        Const PaneGeneral As String = "Item_7"

    End Structure
#End Region

#Region "Attributes"
    '
#End Region

#Region "Constructor"
    Public Sub New(ByRef parentaddon As SEI_Addon)
        MyBase.New(parentaddon, enSBO_LoadFormTypes.XmlFile, SEI_AddonEnum.enAddonFormType.f_AddonSettings, "SEI_AddonSettings", , False)

        If Not Me.ExisteRegistro() Then
            Me.Form.Mode = BoFormMode.fm_ADD_MODE
            Me.Form.Items.Item(Items.EditCode).Specific.Value = "1"
            Me.Form.Items.Item(Items.btnOk).Click(ct_Regular)
        End If

        'si se crean controles via codigo utilizar Initialize()
        Me.Form.Mode = fm_FIND_MODE
        Me.Form.Items.Item(Items.EditCode).Specific.Value = "1"
        Me.Form.Items.Item(Items.btnOk).Click(ct_Regular)

        Me.Form.Items.Item(Items.PaneGeneral).Click(ct_Regular)

        Me.Initialize()

        Me.Form.Visible = True
    End Sub
    Private Function ExisteRegistro() As Boolean
        Dim bExiste As Boolean = True
        Dim ls As String = ""
        Dim oRecordset As SAPbobsCOM.Recordset
        '
        oRecordset = SBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        ls = " SELECT ""Code"" FROM ""@SEICONFIG"" " & vbCrLf
        oRecordset.DoQuery(ls)
        '
        If oRecordset.EoF Then
            bExiste = False
        End If
        '
        LiberarObjCOM(oRecordset)
        Return bExiste
    End Function
#End Region

#Region "Funciones events"

    Public Overrides Sub HANDLE_DATA_EVENT(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)
        '
        If BusinessObjectInfo.BeforeAction Then
            Select Case BusinessObjectInfo.EventType

            End Select
        Else
            Select Case BusinessObjectInfo.EventType
                Case SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD

                Case SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE
                    If BusinessObjectInfo.ActionSuccess Then
                        ' Actualitzem l'objecte de parametritzacions d'add-on
                        m_SBOAddon.Get_AddonSettings(Me.SBO_Company)
                    End If

            End Select
        End If
        '
    End Sub
    Public Overrides Sub HANDLE_FORM_EVENTS(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        '
        If pVal.ItemUID <> "" Then
            Select Case pVal.ItemUID
                Case Items.ButtonCrearCampos
                    Me.ButtonCrearCampos_ITEM_PRESSED(pVal, BubbleEvent)



            End Select
        End If
        '
    End Sub

    Public Overrides Sub HANDLE_MENU_EVENTS(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean)
        '
        If pVal.BeforeAction Then
            Select Case pVal.MenuUID

            End Select
        Else
            Select Case pVal.MenuUID

            End Select
        End If
        '
    End Sub

    Public Overrides Sub HANDLE_PRINT_EVENT(ByRef eventInfo As SAPbouiCOM.PrintEventInfo, ByRef BubbleEvent As Boolean)

    End Sub

    Public Overrides Sub HANDLE_REPORT_DATA_EVENT(ByRef eventInfo As SAPbouiCOM.ReportDataInfo, ByRef BubbleEvent As Boolean)

    End Sub

    Public Overrides Sub HANDLE_RIGHTCLICK_EVENT(ByRef eventInfo As SAPbouiCOM.IContextMenuInfo, ByRef BubbleEvent As Boolean)

    End Sub

    Public Overrides Sub HANDLE_LAYOUTKEY_EVENTS(ByRef EventInfo As SAPbouiCOM.LayoutKeyInfo, ByRef BubbleEvent As Boolean)

    End Sub

    Public Overrides Sub HANDLE_LUPA(ByRef CampoLupa As String, ByRef aRetorno As System.Collections.Generic.List(Of System.Collections.ArrayList))

    End Sub

    Public Overrides Sub HANDLE_PROGRESSBAR_EVENTS(ByRef pVal As SAPbouiCOM.IProgressBarEvent, ByRef BubbleEvent As Boolean)

    End Sub

    Public Overrides Sub HANDLE_STATUSBAR_EVENTS(ByRef Text As String, ByRef MessageType As SAPbouiCOM.BoStatusBarMessageType)

    End Sub

    Public Overrides Sub HANDLE_WIDGET_EVENTS(ByRef pWidgetData As SAPbouiCOM.WidgetData, ByRef BubbleEvent As Boolean)

    End Sub
#End Region

#Region "Funcions"
    Private Sub Initialize()
        '
        Try
            If Me.InitializeForm Then Exit Sub
            '
            Me.InitializeForm = True
            '
            Me.Form.Freeze(True)
            Me.Form.Settings.Enabled = False
            Me.Form.Settings.EnableRowFormat = False
            Me.Form.DataBrowser.BrowseBy = Items.EditCode
            Me.PositionItems()
            Me.SetAutoManagedAttribute()
            Me.EnableMenus(False)

        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        Finally
            Me.Form.Freeze(False)
        End Try
    End Sub
    Private Sub SetAutoManagedAttribute()
        '
        With Me.Form.Items.Item(Items.EditCode)
            .SetAutoManagedAttribute(ama_Visible, afm_All, mvb_True)
            .SetAutoManagedAttribute(ama_Editable, afm_Ok, mvb_False)
            .SetAutoManagedAttribute(ama_Editable, afm_Add, mvb_False)
            .SetAutoManagedAttribute(ama_Editable, afm_Find, mvb_False)
            .SetAutoManagedAttribute(ama_Editable, afm_View, mvb_False)
        End With
        '
        With Me.Form.Items.Item(Items.EditServerUser)
            .SetAutoManagedAttribute(ama_Visible, afm_All, mvb_True)
            .SetAutoManagedAttribute(ama_Editable, afm_Ok, mvb_True)
            .SetAutoManagedAttribute(ama_Editable, afm_Add, mvb_False)
            .SetAutoManagedAttribute(ama_Editable, afm_Find, mvb_False)
            .SetAutoManagedAttribute(ama_Editable, afm_View, mvb_False)
        End With
        '
        With Me.Form.Items.Item(Items.EditServerPassword)
            .SetAutoManagedAttribute(ama_Visible, afm_All, mvb_True)
            .SetAutoManagedAttribute(ama_Editable, afm_Ok, mvb_True)
            .SetAutoManagedAttribute(ama_Editable, afm_Add, mvb_False)
            .SetAutoManagedAttribute(ama_Editable, afm_Find, mvb_False)
            .SetAutoManagedAttribute(ama_Editable, afm_View, mvb_False)
        End With
    End Sub
    Private Sub PositionItems()
        Me.Form.Items.Item(Items.EditCode).Top = -1000
    End Sub
    Private Sub EnableMenus(ByVal bValue As Boolean)
        Me.Form.EnableMenu(SEI_AddonEnum.enMenuUID.MNU_Buscar, bValue)
        Me.Form.EnableMenu(SEI_AddonEnum.enMenuUID.MNU_Crear, bValue)
        Me.Form.EnableMenu(SEI_AddonEnum.enMenuUID.MNU_Primero, bValue)
        Me.Form.EnableMenu(SEI_AddonEnum.enMenuUID.MNU_Anterior, bValue)
        Me.Form.EnableMenu(SEI_AddonEnum.enMenuUID.MNU_Siguiente, bValue)
        Me.Form.EnableMenu(SEI_AddonEnum.enMenuUID.MNU_Ultimo, bValue)
    End Sub
    Private Sub ButtonCrearCampos_ITEM_PRESSED(ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        If pVal.EventType = et_ITEM_PRESSED And Not pVal.BeforeAction Then
            Me.CreateTables()
        End If
    End Sub
    Public Sub CreateTables()
        Dim oCreateTables As SEI_CreateTables
        Dim oCreateFormatQuerys As SEI_AddingFormatedQueries
        Dim oPermissions As SEI_AddingPermissions
        'Dim oSQL As SEI_SQL
        'Dim oCreateCR As SEI_CreateCrystalMenus

        If SubMain.m_SBOAddon.SBO_Application.MessageBox("¿Desea ejecutar el proceso de creación de tablas y campos de usuario?", 1, "Sí", "No") = 2 Then
            Exit Sub
        End If
        '
        Me.m_ParentAddon.Get_AddonSettings(Me.SBO_Company)

        'oCreateTables = New SEI_CreateTables(SubMain.m_SBOAddon)
        'oCreateTables.AddUserTables()
        ''
        'oCreateFormatQuerys = New SEI_AddingFormatedQueries(SubMain.m_SBOAddon)
        'oCreateFormatQuerys.AddFormatedQueries()
        ''

        '' Creació de permisos:
        'oPermissions = New SEI_AddingPermissions(SubMain.m_SBOAddon)
        'oPermissions.AddPermissions()

        'Dim oChOfAc = New SEI_CreateChartOfAccount(SubMain.m_SBOAddon)
        'oChOfAc.CreateChartOfAccounts()

        'Dim oCreateAddExp = New SEI_CreateAdditionalExpenses(SubMain.m_SBOAddon)
        'oCreateAddExp.CreateAdditionalExpenses()

        ''Create the corresponding procedures
        'Dim createProcedure As New SEI_CreateProcedures_Functions(Me.m_ParentAddon.oDatabaseConnection, SubMain.m_SBOAddon)
        'createProcedure.CreateProcedures()
        '
        '' Creació de procediments emmagatzemats SQL:
        'oSQL = New SEI_SQL(SubMain.m_SBOAddon)
        'oSQL.Crear()
        ''
        '' Creació de Crystals i menús de crystals:
        'oCreateCR = New SEI_CreateCrystalMenus(SubMain.m_SBOAddon)
        'oCreateCR.CreateCRs()
        '
        SubMain.m_SBOAddon.SBO_Application.StatusBar.SetText("Operación finalizada.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
        SubMain.m_SBOAddon.SBO_Application.MessageBox("Operación finalizada.")
        '
    End Sub

#End Region

End Class