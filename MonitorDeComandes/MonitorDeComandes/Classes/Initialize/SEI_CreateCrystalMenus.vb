Option Strict Off
Option Explicit On
Imports System.IO
Imports SAPbobsCOM.BoObjectTypes

Public Class SEI_CreateCrystalMenus

#Region "CONSTRUCTOR"
    Public Sub New(ByRef ParentAddon As SEI_Addon)
    End Sub
#End Region

#Region "FUNCIONES"
    Public Sub CreateCRs()
        '
        ' ELIMINO MENÚS CRYSTALS:
        'Me.Eliminar_Menus()

        ' AÑADO LOS MENÚS:
        Me.Create_Crystal_Menus()
        '
        ' AÑADO LOS CRYSTALS:
        Me.Import_Crystals()
        '
    End Sub
    Private Sub Create_Crystal_Menus()
        ' CREO LOS MENÚS DE CRYSTALS:
        '
        'Me.Add_Crystal_Menu("GestionCaptacion", enAddonFormType.f_EnvioCertificado_Grid)
        '
    End Sub
    Private Sub Import_Crystals()
        ' IMPORTO LOS CRYSTALS A LOS MENÚS CORRESPONDIENTES:
        ' PARA MENÚS DE SAP INFORMAR DIRECTAMENTE EL CODE DE [RTYP]. PARA LOS DE USUARIO USAR FUNCIÓN GET_TYPECODE()
        '
        'Me.Import_Crystal(Application.StartupPath() & "\Crystal Report\Crystals\Certificado_Amics_Can_Ruti.rpt", Me.Get_TypeCode(enAddonFormType.f_EnvioCertificado_Grid), "Certificado Amics Can Ruti")
        '
    End Sub
#End Region

#Region "FUNCIONES VARIAS"
    Private Sub Add_Crystal_Menu(ByVal sMenuName As String, ByVal FormType As String)
        Dim sMenuCode As String = Me.Existe_Crystal_Menu(sMenuName, FormType)
        '
        If sMenuCode = "" Then
            Try
                Dim rptTypeService As SAPbobsCOM.ReportTypesService = Nothing
                Dim newType As SAPbobsCOM.ReportType = Nothing
                Dim newTypeParam As SAPbobsCOM.ReportTypeParams = Nothing
                rptTypeService = SubMain.m_SBOAddon.SBO_Company.GetCompanyService.GetBusinessService(SAPbobsCOM.ServiceTypes.ReportTypesService)
                newType = rptTypeService.GetDataInterface(SAPbobsCOM.ReportTypesServiceDataInterfaces.rtsReportType)
                newType.TypeName = sMenuName
                newType.AddonName = SubMain.m_SBOAddon.Name
                newType.AddonFormType = FormType
                newType.MenuID = "MNU_" & sMenuName
                '
                newTypeParam = rptTypeService.AddReportType(newType)
                '
            Catch ex As Exception
                SubMain.m_SBOAddon.SBO_Application.MessageBox(ex.Message)
            End Try
        End If
    End Sub
    Private Function Existe_Crystal_Menu(ByVal sMenuName As String, ByVal FormType As String) As String
        Dim sCode As String = ""
        Dim ls As String
        Dim oRecordset As SAPbobsCOM.Recordset = Nothing
        '
        ls = " SELECT T0.CODE " & vbCrLf
        ls &= " FROM ""RTYP"" T0" & vbCrLf
        ls &= " WHERE T0.NAME = '" & sMenuName & "' " & vbCrLf
        ls &= " AND T0.FRM_TYPE = '" & FormType & "' " & vbCrLf
        oRecordset = SubMain.m_SBOAddon.SBO_Company.GetBusinessObject(BoRecordset)
        oRecordset.DoQuery(ls)
        If Not oRecordset.EoF Then
            sCode = oRecordset.Fields.Item("CODE").Value
        End If
        '
        LiberarObjCOM(oRecordset, True)
        '
        Return sCode
    End Function
    Private Sub Import_Crystal(ByVal sPathCrystalFile As String, ByVal TypeCode As String, ByVal sName As String)
        Dim oLayoutService As SAPbobsCOM.ReportLayoutsService
        Dim oReport As SAPbobsCOM.ReportLayout
        Dim newReportCode As String = ""
        '
        oLayoutService = m_SBOAddon.SBO_Company.GetCompanyService.GetBusinessService(SAPbobsCOM.ServiceTypes.ReportLayoutsService)
        oReport = oLayoutService.GetDataInterface(SAPbobsCOM.ReportLayoutsServiceDataInterfaces.rlsdiReportLayout)
        '
        oReport.Name = sName
        oReport.TypeCode = TypeCode
        oReport.Author = m_SBOAddon.SBO_Company.UserName
        oReport.Category = SAPbobsCOM.ReportLayoutCategoryEnum.rlcCrystal
        '
        ' SI NO EXISTE EL CRYSTAL LO CREAMOS:
        If Not Existe_Crystal(sName, TypeCode) Then
            Try
                Dim oNewReportParams As SAPbobsCOM.ReportLayoutParams = oLayoutService.AddReportLayout(oReport)
                newReportCode = oNewReportParams.LayoutCode
            Catch ex As Exception
                m_SBOAddon.SBO_Application.MessageBox(ex.ToString)
            End Try
            '
            ' Upload .rpt file using setblob interface:
            Dim oCompanyService As SAPbobsCOM.CompanyService = m_SBOAddon.SBO_Company.GetCompanyService()
            Dim oBlobParams As SAPbobsCOM.BlobParams = oCompanyService.GetDataInterface(SAPbobsCOM.CompanyServiceDataInterfaces.csdiBlobParams)
            oBlobParams.Table = "RDOC"
            oBlobParams.Field = "Template"
            '
            ' Specify the record whose blob field is to be set:
            Dim oKeySegment As SAPbobsCOM.BlobTableKeySegment = oBlobParams.BlobTableKeySegments.Add()
            oKeySegment.Name = "DocCode"
            oKeySegment.Value = newReportCode
            '
            Dim oBlob As SAPbobsCOM.Blob = oCompanyService.GetDataInterface(SAPbobsCOM.CompanyServiceDataInterfaces.csdiBlob)
            '
            ' Put the rpt file into buffer:
            Dim oFile As FileStream = New FileStream(sPathCrystalFile, System.IO.FileMode.Open)
            Dim fileSize As Integer = oFile.Length

            Dim buf() As Byte = New Byte(fileSize) {}
            oFile.Read(buf, 0, fileSize)
            oFile.Close()
            '
            ' Convert memory buffer to Base64 String:
            oBlob.Content = Convert.ToBase64String(buf, 0, fileSize)
            '
            Try
                ' Upload blob to database:
                oCompanyService.SetBlob(oBlobParams, oBlob)
                '
            Catch ex As Exception
                m_SBOAddon.SBO_Application.MessageBox(ex.ToString)
            End Try
            '
            LiberarObjCOM(oCompanyService)
            LiberarObjCOM(oBlobParams)
            LiberarObjCOM(oBlob)
            LiberarObjCOM(oCompanyService)
        End If

        LiberarObjCOM(oReport)
        LiberarObjCOM(oLayoutService)
    End Sub
    Private Function Get_TypeCode(ByVal sFormType As String) As String
        Dim sTypeCode As String = ""
        Dim ls As String = ""
        Dim oRecordset As SAPbobsCOM.Recordset
        '
        oRecordset = SubMain.m_SBOAddon.SBO_Company.GetBusinessObject(BoRecordset)
        ls = " SELECT ""CODE"" FROM ""RTYP"" WHERE FRM_TYPE = '" & sFormType & "' " & vbCrLf
        oRecordset.DoQuery(ls)
        '
        If Not oRecordset.EoF Then
            sTypeCode = oRecordset.Fields.Item("CODE").Value
        End If
        '
        LiberarObjCOM(oRecordset)
        Return sTypeCode
    End Function
    Private Function Existe_Crystal(ByVal sCrystalName As String, ByVal FormType As String) As Boolean
        Dim bExiste As Boolean = False
        Dim ls As String
        Dim oRecordset As SAPbobsCOM.Recordset = Nothing
        '
        oRecordset = SubMain.m_SBOAddon.SBO_Company.GetBusinessObject(BoRecordset)
        ls = " SELECT " & vbCrLf
        ls &= " ""DocCode"" " & vbCrLf
        ls &= " FROM ""RDOC"" " & vbCrLf
        ls &= " WHERE ""DocName"" = '" & sCrystalName & "' " & vbCrLf
        ls &= " AND ""TypeCode"" = '" & FormType & "' " & vbCrLf
        oRecordset.DoQuery(ls)
        '
        If Not oRecordset.EoF Then bExiste = True
        '
        LiberarObjCOM(oRecordset, True)
        '
        Return bExiste
    End Function
#End Region

#Region "ELIMINAR MENU"
    Private Sub Eliminar_Menus()
        '
        'Me.Eliminar_Menu_Crystal("GestionCaptacion", SEI_AddOnEnum.enAddOnFormType.f_EnvioCertificado_Grid)
        '
    End Sub
    Private Sub Eliminar_Menu_Crystal(ByVal sMenuName As String, ByVal sFormType As String)
        Dim sMenuCode As String = Me.Existe_Crystal_Menu(sMenuName, sFormType)
        '
        If sMenuCode <> "" And Me.Menu_Sin_Crystals(sMenuCode) Then
            Dim oCmpSrv As SAPbobsCOM.CompanyService
            Dim oReportTypeService As SAPbobsCOM.ReportTypesService
            Dim oReportTypeParams As SAPbobsCOM.ReportTypeParams
            Dim oReportType As SAPbobsCOM.ReportType
            'Get report Type service   
            oCmpSrv = m_SBOAddon.SBO_Company.GetCompanyService
            oReportTypeService = oCmpSrv.GetBusinessService(SAPbobsCOM.ServiceTypes.ReportTypesService)
            oReportType = oReportTypeService.GetDataInterface(SAPbobsCOM.ReportTypesServiceDataInterfaces.rtsReportType)
            'Set parameters   
            oReportTypeParams = oReportTypeService.GetDataInterface(SAPbobsCOM.ReportTypesServiceDataInterfaces.rtsReportTypeParams)
            '
            oReportTypeParams.TypeCode = sMenuCode
            ' 
            'Get the report Type   
            oReportType = oReportTypeService.GetReportType(oReportTypeParams)
            'Update the ReportType  
            oReportTypeService.DeleteReportType(oReportType)

            LiberarObjCOM(oCmpSrv)
            LiberarObjCOM(oReportType)
            LiberarObjCOM(oReportTypeParams)
            LiberarObjCOM(oReportTypeService)
        End If
    End Sub
    Private Function Menu_Sin_Crystals(ByVal sMenuCode As String) As Boolean
        Dim bMenuSinCrystals As Boolean = True
        Dim ls As String = ""
        Dim oRecordset As SAPbobsCOM.Recordset = SubMain.m_SBOAddon.SBO_Company.GetBusinessObject(BoRecordset)
        '
        ls = " SELECT ""DocCode"" FROM ""RDOC"" WHERE ""TypeCode"" = '" & sMenuCode & "' " & vbCrLf
        oRecordset.DoQuery(ls)
        '
        If Not oRecordset.EoF Then
            bMenuSinCrystals = False
        End If
        '
        Return bMenuSinCrystals
    End Function
#End Region

End Class
