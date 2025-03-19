Option Explicit On
Imports System.Collections.Generic
Imports System.Threading
Imports CrystalDecisions.Shared

Public Class SEI_CrystalReport

#Region "Attributes"
    Private _CrystalName As String
    Private _ListParameters As List(Of Parameter)
    Private _ListFormulas As List(Of Formula)
    Private _ListSubReports As List(Of SubReport)
    Private _DirectoryReport As String
    Private _BringToFront As Boolean
#End Region

#Region "Structures"
    Public Structure Parameter
        Public Code As String
        Public Value As String
    End Structure
    Public Structure Formula
        Public Name As String
        Public Value As String
    End Structure
    Public Structure SubReport
        Public SubReportName As String
        Public ListParams As List(Of Parameter)
        Public ListFormulas As List(Of Formula)
    End Structure
#End Region

#Region "Constructor"
    Public Sub New(ByVal CrystalName As String,
                   ByVal ListParameters As List(Of Parameter),
                   Optional ByVal ListFormulas As List(Of Formula) = Nothing,
                   Optional ByVal ListSubReports As List(Of SubReport) = Nothing,
                   Optional ByVal DirectoryReport As String = "")
        Me._CrystalName = CrystalName
        Me._ListParameters = ListParameters
        Me._ListFormulas = ListFormulas
        Me._ListSubReports = ListSubReports
        Me._DirectoryReport = DirectoryReport
    End Sub
#End Region

#Region "Public Functions"
    Public Sub Print_Crystal(Optional ByVal Printer As String = "", Optional ByVal NumberCopies As Integer = 1)
        Dim oReport As CrystalDecisions.CrystalReports.Engine.ReportDocument
        '
        oReport = Me.LoadReport()
        '
        'COMPROVAR QUE AGAFA LA PER DEFECTE:
        If Printer <> "" Then oReport.PrintOptions.PrinterName = Printer
        '
        oReport.PrintToPrinter(NumberCopies, True, 0, 0)
        '
        m_SBOAddon.SBO_Application.StatusBar.SetText("Document printed correctly.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
        '
        If Not oReport Is Nothing Then oReport.Close()
        '
    End Sub
    Public Sub Create_PDF(ByVal DirectoryNameFile As String)
        Dim oReport As CrystalDecisions.CrystalReports.Engine.ReportDocument
        '

        oReport = Me.LoadReport()
        '
        oReport.ExportOptions.ExportDestinationType = CrystalDecisions.Shared.ExportDestinationType.DiskFile
        oReport.ExportOptions.ExportFormatType = CrystalDecisions.Shared.ExportFormatType.PortableDocFormat
        oReport.ExportToDisk(ExportFormatType.PortableDocFormat, DirectoryNameFile)
        '
        m_SBOAddon.SBO_Application.StatusBar.SetText("Document created correctly.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
        '
        If Not oReport Is Nothing Then oReport.Close()
        '
    End Sub
    Public Sub Preview_Crystal(Optional ByVal bBringToFront As Boolean = True)
        Try
            Me._BringToFront = bBringToFront
            '
            'INITIALIZE WITH A THREAD IN ORDER TO AVOID BLOCKS:
            Dim myThread As New Thread(AddressOf OpenVisorCrystal)
            myThread.TrySetApartmentState(ApartmentState.STA)
            myThread.Start()
            '
        Catch ex As Exception
            m_SBOAddon.SBO_Application.MessageBox("Error at previewing Crystal Report: " & ex.Message)
        End Try
    End Sub
#End Region

#Region "Private Functions"
    Private Function LoadReport() As Object
        Dim oReport As CrystalDecisions.CrystalReports.Engine.ReportDocument
        Dim myLogin As New CrystalDecisions.Shared.TableLogOnInfo
        Dim sTempRPT As String = ""
        '
        m_SBOAddon.SBO_Application.StatusBar.SetText("Loading Report...", SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
        '
        oReport = New CrystalDecisions.CrystalReports.Engine.ReportDocument
        '
        ' ORIGIN
        If Me._DirectoryReport = "" Then
            sTempRPT = Me.GetRPTFromSBO(Me._CrystalName)
            oReport.Load(sTempRPT)
        Else
            oReport = New CrystalDecisions.CrystalReports.Engine.ReportDocument
            oReport.Load(Me._DirectoryReport)
        End If
        '
        ' CONNECTION
        If m_SBOAddon.SBO_Company.DbServerType <> SAPbobsCOM.BoDataServerTypes.dst_HANADB Then
            ' SQL:
            oReport.SetDatabaseLogon(m_SBOAddon.SBO_Company.DbUserName, m_SBOAddon.oAddonSettings.PasswordServer, m_SBOAddon.SBO_Company.Server, m_SBOAddon.SBO_Company.CompanyDB)
            '
        Else
            'HANA:
            Dim strConnection As String = "DRIVER= {B1CRHProxy};UID=" + m_SBOAddon.SBO_Company.DbUserName +
           ";PWD=" + m_SBOAddon.oAddonSettings.PasswordServer + ";SERVERNODE=" + m_SBOAddon.SBO_Company.Server +
           ";DATABASE=" + m_SBOAddon.SBO_Company.CompanyDB + ";"
            '
            Dim logonProps2 As CrystalDecisions.Shared.NameValuePairs2 = oReport.DataSourceConnections(0).LogonProperties
            logonProps2.Set("Provider", "B1CRHProxy")
            logonProps2.Set("Server Type", "B1CRHProxy")
            logonProps2.Set("Connection String", strConnection)
            logonProps2.Set("Locale Identifier", "1033")
            oReport.DataSourceConnections(0).SetLogonProperties(logonProps2)
            oReport.DataSourceConnections(0).SetConnection(m_SBOAddon.SBO_Company.Server, m_SBOAddon.SBO_Company.CompanyDB, m_SBOAddon.SBO_Company.DbUserName, m_SBOAddon.oAddonSettings.PasswordServer)
        End If
        '
        myLogin.ConnectionInfo.ServerName = m_SBOAddon.SBO_Company.Server
        myLogin.ConnectionInfo.UserID = m_SBOAddon.SBO_Company.DbUserName
        myLogin.ConnectionInfo.Password = m_SBOAddon.oAddonSettings.PasswordServer
        myLogin.ConnectionInfo.DatabaseName = m_SBOAddon.SBO_Company.CompanyDB
        '
        ' TABLE CONNECTION
        For i = 0 To oReport.Database.Tables.Count - 1
            oReport.Database.Tables.Item(i).ApplyLogOnInfo(myLogin)
        Next
        '
        ' PARAMETERS
        If Not IsNothing(Me._ListParameters) Then
            For i = 0 To Me._ListParameters.Count - 1
                oReport.SetParameterValue(Me._ListParameters.Item(i).Code, Me._ListParameters.Item(i).Value)
            Next
        End If
        '
        ' FORMULAS
        If Not IsNothing(Me._ListFormulas) Then
            For i = 0 To Me._ListFormulas.Count - 1
                oReport.DataDefinition.FormulaFields.Item(Me._ListFormulas.Item(i).Name).Text = "'" & Me._ListFormulas.Item(i).Value & "'"
            Next
        End If
        '
        ' SUBREPORTS
        If Not IsNothing(Me._ListSubReports) Then
            Dim mySubRepDoc As New CrystalDecisions.CrystalReports.Engine.ReportDocument
            For Each mySubRepDoc In oReport.Subreports
                ' TABLE CONNECTIONS:
                For i = 0 To mySubRepDoc.Database.Tables.Count - 1
                    mySubRepDoc.Database.Tables.Item(i).ApplyLogOnInfo(myLogin)
                Next
                'PARAMS AND FORMULAS:
                For i = 0 To Me._ListSubReports.Count - 1
                    If Me._ListSubReports.Item(i).SubReportName = mySubRepDoc.Name Then
                        ' PARAMETERS:
                        If Not IsNothing(Me._ListSubReports.Item(i).ListParams) Then
                            For j = 0 To Me._ListSubReports.Item(i).ListParams.Count - 1
                                mySubRepDoc.SetParameterValue(Me._ListSubReports.Item(i).ListParams.Item(j).Code, Me._ListSubReports.Item(i).ListParams.Item(j).Value)
                            Next
                        End If
                        ' FORMULAS:
                        If Not IsNothing(Me._ListSubReports.Item(i).ListFormulas) Then
                            For j = 0 To Me._ListSubReports.Item(i).ListFormulas.Count - 1
                                mySubRepDoc.DataDefinition.FormulaFields.Item(Me._ListSubReports.Item(i).ListFormulas.Item(j).Name).Text = "'" & Me._ListSubReports.Item(i).ListFormulas.Item(j).Value & "'"
                            Next
                        End If
                    End If
                Next
            Next
        End If
        '
        ' DELETE TEMPORARY FILE:
        If IO.File.Exists(sTempRPT) Then IO.File.Delete(sTempRPT)
        '
        Return oReport
    End Function
    Public Function GetRPTFromSBO(ByVal sValue As String, Optional ByVal sCampName As String = "DocName") As String
        Dim oCompanyService As SAPbobsCOM.CompanyService = Nothing
        Dim oBlobParams As SAPbobsCOM.BlobParams = Nothing
        Dim blobNewFilePath As String
        Dim oKeySegment As SAPbobsCOM.BlobTableKeySegment = Nothing
        '
        GetRPTFromSBO = ""
        '
        Try
            'get company service
            oCompanyService = GetM_SBOAddon.SBO_Company.GetCompanyService

            ' Specify the table and blob field 
            oBlobParams = oCompanyService.GetDataInterface(SAPbobsCOM.CompanyServiceDataInterfaces.csdiBlobParams)
            oBlobParams.Table = "RDOC"
            oBlobParams.Field = "Template"

            '// Specify the file name to which to write the blob 
            blobNewFilePath = Application.CommonAppDataPath & "\" & GetM_SBOAddon.SBO_Company.UserSignature & Now.ToString("yyyyMMddhhmmss") & ".rpt"
            '
            oBlobParams.FileName = blobNewFilePath
            '// Specify the key field and value of the row from which to get the blob 
            oKeySegment = oBlobParams.BlobTableKeySegments.Add()
            oKeySegment.Name = sCampName   ' DocCode - DocName
            oKeySegment.Value = sValue     ' Sample ->"RCRI0001"

            '// Save the blob to the file 
            oCompanyService.SaveBlobToFile(oBlobParams)
            '
            GetRPTFromSBO = blobNewFilePath

        Catch ex As Exception
            Throw New Exception(ex.Message)
        Finally
            LiberarObjCOM(oCompanyService)
            LiberarObjCOM(oBlobParams)
            LiberarObjCOM(oKeySegment)
        End Try
    End Function
    Private Sub OpenVisorCrystal()
        Dim oReport As CrystalDecisions.CrystalReports.Engine.ReportDocument = Nothing
        Dim oFormVisor As SEI_VisorCrystal
        '
        Try
            oReport = Me.LoadReport()

            oFormVisor = New SEI_VisorCrystal(oReport)
            If Me._BringToFront Then
                Dim oWindowsSbo As SEI_WindowsSbo
                oWindowsSbo = New SEI_WindowsSbo(m_SBOAddon)
                oFormVisor.ShowDialog(oWindowsSbo)
            Else
                oFormVisor.ShowDialog()
                oFormVisor.Dispose()
                oFormVisor = Nothing
                GC.Collect()
                GC.WaitForPendingFinalizers()
            End If

            m_SBOAddon.SBO_Application.StatusBar.SetText("Crystal previewed correctly.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
        Catch ex As Exception
            m_SBOAddon.SBO_Application.StatusBar.SetText(ex.Message)
            m_SBOAddon.SBO_Application.MessageBox("Error at previewing Crystal Report: " & ex.Message)
        Finally
            If Not oReport Is Nothing Then oReport.Close()
        End Try
    End Sub
#End Region

End Class
