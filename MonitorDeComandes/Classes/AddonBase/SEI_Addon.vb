Option Explicit On
Imports System.Drawing
Imports System.Timers
'
Imports System.Xml
Imports DatabaseConnection
Imports SEIDOR_SLayer
'
Public MustInherit Class SEI_Addon

#Region "ATRIBUTOS"
    Const DEVELOPERSCONNECTIONSTRING As String = "0030002C0030002C00530041005000420044005F00440061007400650076002C0050004C006F006D0056004900490056"
    Protected WithEvents m_SBO_Application As SAPbouiCOM.Application
    Protected m_SBO_Company As SAPbobsCOM.Company
    Protected m_Name As String
    Protected m_Connected As Boolean
    Protected BubbleEvent As Boolean
    Public col_SBOFormsOpened As Collection
    Protected sPaper As String
    Protected sType As String
    Public oAddonSettings As clsAddonSettings
    ' SELECCION FICHEROS WINDOWS
    Friend TipoFichero As String
    Friend DirectorioInicial As String
    Friend RutaFichero As String
    Friend NombreFichero As String
    ' CONNECTIONS
    Friend oDatabaseConnection As IDatabaseConnection
    Friend oSLConnection As SLConnection
#End Region

#Region "CONSTRUCTOR"
    Public Sub New(ByVal AddOnName As String)
        m_Name = AddOnName
        bMostrarIconoEncendido = My.Settings.MostrarIcono
        Me.Initialize()
    End Sub
    Private Sub Initialize()
        col_SBOFormsOpened = New Collection
        If Not ConnectToSBO() Then Application.Exit()
    End Sub
#End Region

#Region "FUNCIÓN FILTRAR EVENTOS"
    Public Function SetEventFilters() As Long
        Dim oFilters As New SAPbouiCOM.EventFilters
        Dim oFilter As SAPbouiCOM.EventFilter
        '        
        '[PROGRAMAR]
        oFilter = oFilters.Add(SAPbouiCOM.BoEventTypes.et_ALL_EVENTS)
        oFilter.AddEx(SEI_AddonEnum.enAddonFormType.f_AddonSettings)
        oFilter.AddEx(SEI_AddonEnum.enAddonFormType.f_OrdersMonitor)
        oFilter.AddEx(SEI_AddonEnum.enAddonFormType.f_ConfOrders)

        'SAP FORMS
        'oFilter.AddEx(SEI_AddonEnum.enSAPFormType.f_InterlocutoresComerciales)


        oFilter.AddEx("2018000001")
        '
        Me.SBO_Application.SetFilter(oFilters)
        '
    End Function
#End Region

#Region "INICIAR SBO"
    Public Function ConnectToSBO() As Boolean
        Try
            m_Connected = False
            'Try to connect to SAP Business One UIAPI
            If ConnectToUIAPI() Then
                Me.SBO_Application.StatusBar.SetText("Iniciando add-on", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
                If ConnectToDIAPI() Then m_Connected = True
                If bMostrarIconoEncendido Then Me.MostrarIconoSap()
            End If
            '
        Catch ex As Exception
            Throw ex
        End Try
        Return m_Connected
    End Function
    Private Function ConnectToUIAPI() As Boolean
        Try
            Dim SboGuiApi As SAPbouiCOM.SboGuiApi
            Dim sConnectionString As String
            '
            SboGuiApi = New SAPbouiCOM.SboGuiApi
            '
            'PCT MODE DEBUG / RELEASE:
            If Environment.GetCommandLineArgs().GetUpperBound(0) > 0 Then
                'MODE RELEASE:
                sConnectionString = Environment.GetCommandLineArgs().GetValue(1).ToString
            Else
                'MODE DEBUG:
                sConnectionString = DEVELOPERSCONNECTIONSTRING
            End If

            ' Connect to a running SBO Application
            SboGuiApi.Connect(sConnectionString)

            ' Get an initialized application object
            m_SBO_Application = SboGuiApi.GetApplication()
            '
            Return True

        Catch excE As Exception
            MsgBox("El Addon " & m_Name & " no puede conectar con SAP Business One." _
                   & vbCrLf & "Reinicie SAP Business One o" & vbCrLf &
                   "póngase en contacto con su Administrador.", MsgBoxStyle.OkOnly Or MsgBoxStyle.Critical, "Addon " & m_Name)
            Application.Exit()
        End Try
    End Function
    Private Function ConnectToDIAPI() As Boolean
        Dim ErrCode As Integer = 0
        Dim ErrMessage As String = ""
        Dim bConnected As Boolean = False
        '
        Try
            m_SBO_Company = m_SBO_Application.Company.GetDICompany()
            bConnected = True
            '
        Catch excE As Exception
            m_SBO_Application.MessageBox(excE.Message)
            Application.Exit()
        End Try
        Return bConnected
    End Function
    Public Sub BuildMenus()
        Const cModulos As String = "43520"
        '
        Me.AddMenuItem("SEI_ConfigurarAddon", "8192", False, True, "", 11, "Configurar Add-on", SAPbouiCOM.BoMenuType.mt_STRING)
        Me.AddMenuItem("8765", "43520", False, True, "C:\Users\nraoui\source\repos\OperadorLogistico\MonitorDeComandes\Images\IntegrationLogistic.bmp", -1, "Integración Logística", SAPbouiCOM.BoMenuType.mt_POPUP)
        Me.AddMenuItem("SEI_OrdersMonitor", "8765", False, True, "", -1, "Order Monitor", SAPbouiCOM.BoMenuType.mt_STRING)
        Me.AddMenuItem("SEI_ConfOrders", "8765", False, True, "", 1, "Confirmation orders", SAPbouiCOM.BoMenuType.mt_STRING)
        '
        'Menú Modelo:

        'Colocar monitorcomandes a on correspongui
    End Sub
    Private Sub AddMenuItem(ByVal sIDMenu As String, ByVal sFatherMenu As String, ByVal bChecked As Boolean, ByVal bEnabled As Boolean, ByVal sImage As String,
                            ByVal iPosition As Integer, ByVal sNombre As String, ByVal iType As SAPbouiCOM.BoMenuType)
        Dim oMenus As SAPbouiCOM.Menus = Nothing
        Dim oParams As SAPbouiCOM.MenuCreationParams = Nothing
        '
        Try
            If Not SBO_Application.Menus.Exists(sIDMenu) Then
                oMenus = SBO_Application.Menus.Item(sFatherMenu).SubMenus
                oParams = SBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams)
                With oParams
                    .Checked = bChecked
                    .Enabled = bEnabled
                    .Image = sImage
                    If iPosition = -1 Then
                        .Position = oMenus.Count
                    Else
                        .Position = iPosition
                    End If
                    .String = sNombre
                    .Type = iType
                    .UniqueID = sIDMenu
                End With
                oMenus.AddEx(oParams)
            End If
        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        Finally
            mGlobals.LiberarObjCOM(oMenus)
            mGlobals.LiberarObjCOM(oParams)
        End Try
    End Sub
    Public Sub Get_AddonSettings(ByRef oCompany As SAPbobsCOM.Company)
        Me.oAddonSettings = New clsAddonSettings
        '
        Dim ls As String = ""
        Dim sTable As String = "SEICONFIG"
        Dim oCreateFields As New SEI_CreateTables(Me)
        Dim oUserTablesMD As SAPbobsCOM.UserTablesMD = Nothing
        Dim oRecordset As SAPbobsCOM.Recordset = Nothing
        '
        Try
            ' CREO CAMPOS Y TABLA DE CONFIGURACIÓN DE ADD-ON:

            If Table_Exist(sTable, SBO_Company) Then

                oUserTablesMD = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserTables)
                '
                If oUserTablesMD.GetByKey(sTable.Replace("@", "")) Then
                    oRecordset = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                    ls = Me.Query_AddonSettings()
                    oRecordset.DoQuery(ls)
                    '
                    If Not oRecordset.EoF Then
                        With oAddonSettings
                            If mGlobals.GetUserFieldID(oCompany, sTable, "SEIUSERDB") <> -1 Then .UserServer = oRecordset.Fields.Item("U_SEIUSERDB").Value
                            If mGlobals.GetUserFieldID(oCompany, sTable, "SEIPASSWD") <> -1 Then .PasswordServer = oRecordset.Fields.Item("U_SEIPASSWD").Value
                            If mGlobals.GetUserFieldID(oCompany, sTable, "SEISLADDR") <> -1 Then .SLAddress = oRecordset.Fields.Item("U_SEISLADDR").Value

                        End With
                    End If
                End If
                ConfigureConnectionServices()

                'Create the connection to the DB
            End If
            '

        Catch ex As Exception
            SBO_Application.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            SBO_Application.MessageBox(ex.Message)
        Finally
            LiberarObjCOM(oUserTablesMD, True)
            LiberarObjCOM(oRecordset, True)
        End Try
    End Sub
    Private Function Table_Exist(ByVal sTable As String, ByRef oCompany As SAPbobsCOM.Company) As Boolean
        Dim ls As String = ""
        Dim oRecordset As SAPbobsCOM.Recordset = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        Dim bExist As Boolean = False

        ls = " SELECT ""TableName"" FROM ""OUTB"" WHERE ""TableName"" = '" & sTable & "' " & vbCrLf
        oRecordset.DoQuery(ls)

        If Not oRecordset.EoF Then
            bExist = True
        End If

        LiberarObjCOM(oRecordset, True)

        Return bExist
    End Function
    Private Function Query_AddonSettings()
        Dim ls As String = ""
        Dim sTable As String = "@SEICONFIG"
        '
        ls = " SELECT * " & vbCrLf
        ls &= " FROM """ & sTable & """ " & vbCrLf
        ls &= " WHERE ""Code"" = '1' " & vbCrLf
        ls = mGlobals.ConvertToHANA(Me.SBO_Company, ls)
        '
        Return ls
    End Function
    Private Function GetNewFormUID() As String

        Return Me.Name.ToString & SEI_Form.Count

    End Function
    Public Function CreateSBO_Form(ByVal pst_XMLDocumentName As String, ByVal pst_FormType As String, Optional ByVal pst_UDOCode As String = "") As SAPbouiCOM.Form
        'create a new form via xml
        '-----------------------------------------------------------------------------
        ' DESCRIPTION  : Create a form
        ' Entry       : XMLDocumentName (string)   : name of the XML File (without extension)
        '                FormType (string)          : Form Type (MRO_ENUM.enAeroOneFormType.cst_Counter As String = "MRO_0001"
        '                UDOName (string)           : Name of the UDO if there is one attached   
        '                   = ""  : Create a normal form
        '                   <> "" : Create a UDO type form
        ' Exit       : object SAPbouiCOM.Form
        ' MODIFICATION : 
        '-----------------------------------------------------------------------------
        Dim dsa_CreationPackage As SAPbouiCOM.FormCreationParams
        Dim dxl_XLMDocument As System.Xml.XmlDocument
        Try
            dsa_CreationPackage = SBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_FormCreationParams)

            dsa_CreationPackage.UniqueID = GetNewFormUID()
            dsa_CreationPackage.FormType = pst_FormType
            If pst_UDOCode <> "" Then
                dsa_CreationPackage.ObjectType = pst_UDOCode
            End If
            If pst_XMLDocumentName.Contains("<?xml") Then
                dxl_XLMDocument = GetFormDefinitions(pst_XMLDocumentName, "")
            Else
                dxl_XLMDocument = GetFormDefinitions(pst_XMLDocumentName)
            End If

            'set form position (X and Y)
            dsa_CreationPackage.XmlData = dxl_XLMDocument.InnerXml
            Return SBO_Application.Forms.AddEx(dsa_CreationPackage)

        Catch ex As Exception
            Throw ex
        End Try

    End Function
    Private Function GetFormDefinitions(ByVal NomFormulari As String) As XmlDocument
        Dim oXMLDocument As XmlDocument = New XmlDocument
        Dim oFilename As String
        '
        oFilename = NomFormulari & ".srf"
        oXMLDocument.LoadXml(SEI_Resources.GetEmbeddedResource(Me.GetType, oFilename))
        '
        Return oXMLDocument
    End Function
    Private Function GetFormDefinitions(ByVal NomFormulari As String, ByVal Nom As String) As XmlDocument
        Dim oXMLDocument As XmlDocument = New XmlDocument
        Dim oFilename As String = ""
        '
        oXMLDocument.LoadXml(NomFormulari)
        '
        Return oXMLDocument
    End Function

    Private Sub ConfigureConnectionServices()
        'Database connection
        If (Me.m_SBO_Company.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_HANADB) Then
            oDatabaseConnection = New HANADatabaseConnection(HANADatabaseConnection.DBVersion.HANA64,
                                                            oAddonSettings.UserServer,
                                                            oAddonSettings.PasswordServer,
                                                            Me.SBO_Company.CompanyDB,
                                                            Me.SBO_Company.Server)
        Else
            oDatabaseConnection = New SQLDatabaseConnection(Me.SBO_Company.Server,
                                                            Me.SBO_Company.CompanyDB,
                                                            oAddonSettings.UserServer,
                                                            oAddonSettings.PasswordServer)
        End If

        'oDatabaseConnection = New HANADatabaseConnection(HANADatabaseConnection.DBVersion.HANA64,
        '                                                "B1ADMIN",
        '                                                "W8srCR0aZnmL0f",
        '                                                Me.SBO_Company.CompanyDB,
        '                                                Me.SBO_Company.Server)

        'Service layer connection
        Dim sServiceLayerAddress = oAddonSettings.SLAddress
        Dim fun = Function(input As String) As String
                      Return m_SBOAddon.SBO_Application.Company.GetServiceLayerConnectionContext(input)
                  End Function
        If (Not String.IsNullOrEmpty(sServiceLayerAddress)) Then
            oSLConnection = New SLConnection(sServiceLayerAddress, fun)
        Else
            oSLConnection = Nothing
        End If
    End Sub
#End Region

#Region "PROPIEDADES"
    Public ReadOnly Property Connected() As Boolean
        Get
            Return m_Connected
        End Get
    End Property
    Public ReadOnly Property Name() As String
        Get
            Return m_Name
        End Get
    End Property
    Public ReadOnly Property SBO_Application() As SAPbouiCOM.Application
        Get
            Return m_SBO_Application
        End Get
    End Property
    Public ReadOnly Property SBO_Company() As SAPbobsCOM.Company
        Get
            Return m_SBO_Company
        End Get
    End Property
    Public ReadOnly Property SBO_Forms() As Collection
        Get
            Return col_SBOFormsOpened
        End Get
    End Property
    Public ReadOnly Property BitMapPath() As String
        Get
            BitMapPath = SBO_Company.BitMapPath & "ADDON\"
        End Get
    End Property
#End Region

#Region "MUSTOVERRIDES"
    Public MustOverride Sub Handle_SBO_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles m_SBO_Application.ItemEvent
    Public MustOverride Sub Handle_SBO_AppEvent(ByVal EventType As SAPbouiCOM.BoAppEventTypes) Handles m_SBO_Application.AppEvent
    Public MustOverride Sub Handle_SBO_MenuEvent(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean) Handles m_SBO_Application.MenuEvent
    Public MustOverride Sub Handle_SBO_DataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean) Handles m_SBO_Application.FormDataEvent
    Public MustOverride Sub Handle_SBO_PrintEvent(ByRef eventInfo As SAPbouiCOM.PrintEventInfo, ByRef BubbleEvent As Boolean) Handles m_SBO_Application.PrintEvent
    Public MustOverride Sub Handle_ReportDataEvent(ByRef eventInfo As SAPbouiCOM.ReportDataInfo, ByRef BubbleEvent As Boolean) Handles m_SBO_Application.ReportDataEvent
    Public MustOverride Sub Handle_SBO_RightClickEvent(ByRef eventInfo As SAPbouiCOM.ContextMenuInfo, ByRef BubbleEvent As Boolean) Handles m_SBO_Application.RightClickEvent
    Public MustOverride Sub Handle_SBO_Layoutkeyevent(ByRef EventInfo As SAPbouiCOM.LayoutKeyInfo, ByRef BubbleEvent As Boolean) Handles m_SBO_Application.LayoutKeyEvent
#End Region

#Region "ICONO ADD-ON ENCENDIDO"
    Public bMostrarIconoEncendido As Boolean
    Private NotifyIcon1 As NotifyIcon
    Private oTimer As System.Timers.Timer
    Private bSemafor As Boolean
    Private oIconRed As Icon
    Private oIconBlue As Icon
    '
    Private Sub MostrarIconoSap()
        '----------------------------------------------------------------
        ' Definir Iconos
        oIconRed = New Icon(Me.GetType(), "sap48Rojo.ico")
        oIconBlue = New Icon(Me.GetType(), "sap48.ico")
        '
        NotifyIcon1 = New NotifyIcon
        NotifyIcon1.Icon = oIconRed
        NotifyIcon1.Visible = True
        NotifyIcon1.Text = Me.Name
        '
        '----------------------------------------------------------------
        ' Definir Temporizador 
        oTimer = New System.Timers.Timer
        '
        AddHandler oTimer.Elapsed, AddressOf Me.OnTimer
        ' 
        oTimer.Interval = 200
        oTimer.Enabled = True
        '---------------------------------------------------------------
    End Sub
    Public Sub OnTimer(ByVal source As Object, ByVal e As ElapsedEventArgs)
        If bSemafor Then
            NotifyIcon1.Icon = oIconBlue
            bSemafor = False
        Else
            NotifyIcon1.Icon = oIconRed
            bSemafor = True
        End If
    End Sub
#End Region

End Class

