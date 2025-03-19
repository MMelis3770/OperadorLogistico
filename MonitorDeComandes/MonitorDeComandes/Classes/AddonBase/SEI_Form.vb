Option Explicit On
'
Imports SEI.SEI_ADDON.SEI_AddOnEnum

Public MustInherit Class SEI_Form

    Inherits System.Object

#Region "Variables"
    Private Shared m_intFormCount As Integer
    Protected m_ParentAddon As SEI_Addon
    Protected m_FormUID As String
    Protected m_SBO_Form As SAPbouiCOM.Form
    Protected m_SBO_LoadFormType As enSBO_LoadFormTypes
    Protected m_Formtype As String
    Protected m_InitializeForm As Boolean
    Protected UniqueIdentifier As String
    Protected m_ClosedForm As Boolean
    Protected m_Mensaje As String
    Protected m_FormOriginID As String
    Protected m_FormTargetID As String
#End Region

#Region "Constructor"
    Public Sub New(ByRef ParentAddon As SEI_Addon, _
               ByVal SBO_LoadFormType As SEI_AddOnEnum.enSBO_LoadFormTypes, _
               ByVal pst_FormType As String, _
               Optional ByVal FormUID As String = "", _
               Optional ByVal UDOName As String = "", _
               Optional ByVal FormLoadVisible As Boolean = False)
        Try
            m_FormUID = FormUID
            m_ParentAddon = ParentAddon
            m_intFormCount += 1

            m_SBO_LoadFormType = SBO_LoadFormType
            m_Formtype = pst_FormType

            Select Case m_SBO_LoadFormType
                Case enSBO_LoadFormTypes.LogicOnly
                    m_SBO_Form = m_ParentAddon.SBO_Application.Forms.Item(m_FormUID)

                Case enSBO_LoadFormTypes.XmlFile
                    m_SBO_Form = m_ParentAddon.CreateSBO_Form(FormUID, Me.FormType, UDOName)
                    m_SBO_Form.Visible = FormLoadVisible
                    m_FormUID = m_SBO_Form.UniqueID

            End Select

            m_ParentAddon.col_SBOFormsOpened.Add(Me, m_FormUID)

        Catch excE As Exception
            m_ParentAddon.SBO_Application.MessageBox(excE.Message)
        End Try
    End Sub
    Public Sub New(ByRef ParentAddon As SEI_Addon)
        '
        m_ParentAddon = ParentAddon
        '
    End Sub
#End Region

#Region "Propiedades"
    Public ReadOnly Property FormType() As String
        Get
            Return m_Formtype
        End Get
    End Property
    Public ReadOnly Property SBO_LoadFormType() As enSBO_LoadFormTypes
        Get
            Return m_SBO_LoadFormType
        End Get
    End Property
    Public Shared ReadOnly Property Count() As Integer
        Get
            Return m_intFormCount
        End Get
    End Property
    Property Form() As SAPbouiCOM.Form
        Get
            Form = m_SBO_Form
        End Get
        Set(ByVal value As SAPbouiCOM.Form)
            m_SBO_Form = value
        End Set
    End Property
    ReadOnly Property UniqueID() As String
        Get
            Return m_FormUID
        End Get
    End Property
    Property UID() As String
        Get
            UID = UniqueIdentifier
        End Get
        Set(ByVal value As String)
            UniqueIdentifier = value
        End Set
    End Property
    Protected ReadOnly Property SBO_Application() As SAPbouiCOM.Application
        Get
            Return m_ParentAddon.SBO_Application
        End Get
    End Property
    Protected ReadOnly Property SBO_Company() As SAPbobsCOM.Company
        Get
            Return m_ParentAddon.SBO_Company
        End Get
    End Property
    Public Property InitializeForm() As Boolean
        Get
            InitializeForm = m_InitializeForm
        End Get
        Set(ByVal value As Boolean)
            m_InitializeForm = value
        End Set
    End Property
    Public Property ClosedForm() As Boolean
        Get
            ClosedForm = m_ClosedForm
        End Get
        Set(ByVal value As Boolean)
            m_ClosedForm = value
        End Set
    End Property
    Property Mensaje() As String
        Get
            Mensaje = m_Mensaje
        End Get
        Set(ByVal value As String)
            m_Mensaje = value
        End Set
    End Property
    Property FormOriginID() As String
        Get
            FormOriginID = m_FormOriginID
        End Get
        Set(value As String)
            m_FormOriginID = value
        End Set
    End Property
    Property FormTargetID() As String
        Get
            FormTargetID = m_FormTargetID
        End Get
        Set(value As String)
            m_FormTargetID = value
        End Set
    End Property
#End Region

#Region "MustOverride"
    Public MustOverride Sub HANDLE_RIGHTCLICK_EVENT(ByRef eventInfo As SAPbouiCOM.IContextMenuInfo, ByRef BubbleEvent As Boolean)
    Public MustOverride Sub HANDLE_REPORT_DATA_EVENT(ByRef eventInfo As SAPbouiCOM.ReportDataInfo, ByRef BubbleEvent As Boolean)
    Public MustOverride Sub HANDLE_DATA_EVENT(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)
    Public MustOverride Sub HANDLE_FORM_EVENTS(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
    Public MustOverride Sub HANDLE_MENU_EVENTS(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean)
    Public MustOverride Sub HANDLE_PRINT_EVENT(ByRef eventInfo As SAPbouiCOM.PrintEventInfo, ByRef BubbleEvent As Boolean)
    Public MustOverride Sub HANDLE_LUPA(ByRef CampoLupa As String, ByRef aRetorno As System.Collections.Generic.List(Of System.Collections.ArrayList))
    Public MustOverride Sub HANDLE_STATUSBAR_EVENTS(ByRef Text As String, ByRef MessageType As SAPbouiCOM.BoStatusBarMessageType)
    Public MustOverride Sub HANDLE_PROGRESSBAR_EVENTS(ByRef pVal As SAPbouiCOM.IProgressBarEvent, ByRef BubbleEvent As Boolean)
    Public MustOverride Sub HANDLE_LAYOUTKEY_EVENTS(ByRef EventInfo As SAPbouiCOM.LayoutKeyInfo, ByRef BubbleEvent As Boolean)
    Public MustOverride Sub HANDLE_WIDGET_EVENTS(ByRef pWidgetData As SAPbouiCOM.WidgetData, ByRef BubbleEvent As Boolean)
#End Region

#Region "Overrides"
    Public Overrides Function ToString() As String
        Dim aName() As String
        aName = Split(MyBase.ToString(), ".")
        Return aName(aName.Length - 1)
    End Function

    Protected Overrides Sub Finalize()
        'clean up class
        GC.WaitForPendingFinalizers()
        GC.Collect()
        MyBase.Finalize()
    End Sub
#End Region

End Class

