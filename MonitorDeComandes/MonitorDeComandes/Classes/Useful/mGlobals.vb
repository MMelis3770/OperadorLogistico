Option Explicit On
Imports System.Reflection
'
Imports SAPbobsCOM.BoObjectTypes

Module mGlobals

#Region "FUNCIONES BASE"
    Public Sub LiberarObjCOM(ByRef oObjCOM As Object, Optional ByVal bCollect As Boolean = False)
        ' Alliberar i destruïr Objecte COM: 
        ' A UDO'S és necessari utilitzar GC.Collect per eliminar-los de la memòria:
        If Not IsNothing(oObjCOM) Then
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oObjCOM)
            oObjCOM = Nothing
            If bCollect Then GC.Collect()
        End If
    End Sub
    Public Function GetAssemblyTitle() As String
        GetAssemblyTitle = ""
        '
        Dim m_MyAssembly As Assembly
        '
        m_MyAssembly = Assembly.GetExecutingAssembly()
        '
        If m_MyAssembly.IsDefined(GetType(AssemblyTitleAttribute), True) Then
            Dim attr As Attribute = Attribute.GetCustomAttribute(m_MyAssembly, GetType(AssemblyTitleAttribute))
            Dim title_attr As AssemblyTitleAttribute = DirectCast(attr, AssemblyTitleAttribute)
            GetAssemblyTitle = title_attr.Title
        End If
    End Function
    Public Function GetUserFieldID(ByRef oCompany As SAPbobsCOM.Company, ByVal sTableName As String, ByVal sFieldName As String) As Long
        Dim oRecordset As SAPbobsCOM.Recordset
        Dim ls As String = ""
        '
        If Mid(sTableName, 1, 1) <> "@" And Len(sTableName) > 4 Then sTableName = "@" & sTableName
        '
        oRecordset = oCompany.GetBusinessObject(BoRecordset)
        ls = "Select ""FieldID"" FROM ""CUFD"" WHERE ""TableID"" = '" & sTableName & "' And ""AliasID"" = '" & sFieldName & "'"
        oRecordset.DoQuery(ls)
        '
        If Not oRecordset.EoF Then GetUserFieldID = oRecordset.Fields.Item("FieldID").Value Else GetUserFieldID = -1
        '
        mGlobals.LiberarObjCOM(oRecordset, True)
    End Function
    Public Function ObtenerBitmap(ByRef oME As Object, _
                              ByVal sBitmap As String, _
                              Optional ByVal NombreBitmap As String = "", _
                              Optional ByVal Extension As String = "bmp") As String
        '
        Dim oImage As System.Drawing.Bitmap
        Dim sFichero As String
        '
        oImage = New System.Drawing.Bitmap(oME.GetType(), sBitmap)
        '
        If NombreBitmap = "" Then
            sFichero = System.IO.Path.GetTempPath & "sbo.bmp"
            oImage.Save(sFichero)
        Else
            sFichero = System.IO.Path.GetTempPath & NombreBitmap & "." & Extension
            oImage.Save(sFichero)
        End If
        '
        ObtenerBitmap = sFichero
        '
    End Function
    Public Function GetDefaultReportMenuCode(ByVal sRootName As String, ByVal sFormType As String) As String
        GetDefaultReportMenuCode = ""
        Dim ls As String = ""
        Dim oRecordset As SAPbobsCOM.Recordset = SubMain.m_SBOAddon.SBO_Company.GetBusinessObject(BoRecordset)
        '
        ls = " SELECT" & vbCrLf
        ls &= " T0.CODE," & vbCrLf
        ls &= " T0.NAME," & vbCrLf
        ls &= " T0.DEFLT_REP," & vbCrLf
        ls &= " T0.ADD_NAME," & vbCrLf
        ls &= " T0.FRM_TYPE," & vbCrLf
        ls &= " T0.MNU_ID," & vbCrLf
        ls &= " FROM ""RTYP"" T0" & vbCrLf
        ls &= " WHERE (T0.NAME = '" & sRootName & "') -- Menú" & vbCrLf
        If sFormType <> "" Then
            ls &= " AND (T0.FRM_TYPE = '" & sFormType & "') -- Type del formulari" & vbCrLf
        End If
        oRecordset.DoQuery(ls)
        '
        If oRecordset.RecordCount > 0 Then
            GetDefaultReportMenuCode = oRecordset.Fields.Item("CODE").Value
        End If
        '
        LiberarObjCOM(oRecordset)
    End Function
#End Region

#Region "FUNCIONES CONVERSIONES TIPOS"
    ' NUMERICOS
    Function NullToDoble(ByVal Valor As Object)

        If IsDBNull(Valor) Or Trim(Valor) = "" Then
            Return 0
        Else
            Return CType(Valor, Double)
        End If

    End Function
    Public Function String_To_Double(ByVal sValue As String) As Double
        Dim dValue As Double = 0
        Dim oAdminInfo As SAPbobsCOM.AdminInfo
        Dim sNumericSeparatorWindows As String = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator
        Dim sDecimalSeparatorWindows As String = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator
        oAdminInfo = SubMain.m_SBOAddon.SBO_Company.GetCompanyService().GetAdminInfo()
        '
        ' Quito el separador numérico:
        sValue = sValue.Replace(oAdminInfo.ThousandsSeparator, "")
        '
        dValue = Replace(sValue, oAdminInfo.DecimalSeparator, sDecimalSeparatorWindows)
        '
        LiberarObjCOM(oAdminInfo)
        Return dValue
    End Function
    Public Function Double_To_String_SBO(ByVal dValue As Double) As String
        Dim sValue As String = ""
        Dim oAdminInfo As SAPbobsCOM.AdminInfo
        Dim sNumericSeparatorWindows As String = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator
        Dim sDecimalSeparatorWindows As String = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator
        oAdminInfo = SubMain.m_SBOAddon.SBO_Company.GetCompanyService().GetAdminInfo()
        '
        sValue = dValue.ToString.Replace(sDecimalSeparatorWindows, oAdminInfo.DecimalSeparator)
        '
        LiberarObjCOM(oAdminInfo)
        Return sValue
    End Function
    Function NullToLong(ByVal Valor As Object) As Long

        If IsDBNull(Valor) Or Trim(Valor) = "" Then
            Return 0
        Else
            Return CType(Valor, Long)
        End If

    End Function
    Function NullToInt(ByVal Valor As Object) As Integer

        If IsDBNull(Valor) Or Trim(Valor.GetType.ToString) = " " Or Valor.ToString.Trim = "" Then
            Return 0
        Else
            Return Convert.ToInt32(Valor.ToString)
        End If

    End Function
    Function CeroToNULL(ByVal Valor As Object) As String

        If IsDBNull(Valor) Or Valor.ToString.Trim = "" Or Valor.ToString.Trim = "0" Then
            Return "NULL"
        Else
            Return Valor.ToString
        End If

    End Function
    Function BlancsToNULL(ByVal Valor As Object) As String

        If IsDBNull(Valor) Or Valor.ToString.Trim = "" Then
            Return "NULL"
        Else
            Return "'" & Valor.ToString & "'"
        End If

    End Function
    ' TEXTO
    Function NullToText(ByVal Valor As Object) As String

        If IsNothing(Valor) Then
            Return ""
        Else
            If IsDBNull(Valor) Or Valor.GetType.ToString = "" Then
                Return ""
            Else
                Return Valor
            End If

        End If

    End Function
    ' FECHAS/HORAS
    Function NullToDate(ByVal Valor As Object) As String

        If IsNothing(Valor) Then
            Return ""
        End If

        If IsDBNull(Valor) Or Valor.ToString = "" Then
            Return ""
        Else
            If String.Format("{0:d}", Valor) <> "30/12/1899" Then
                Return String.Format("{0:d}", Valor)
            Else
                Return ""
            End If
        End If

    End Function
    Public Function NullToHour(ByVal Valor As Object) As String

        If IsDBNull(Valor) Or Valor.GetType.ToString = "" Then
            Return ""
        Else
            Return String.Format("{0:t}", Valor)
        End If

    End Function
    Function FechaToNULL(ByVal sValor As String) As String
        If IsDate(sValor) Then
            FechaToNULL = "'" & Convert.ToDateTime(sValor).ToString("yyyyMMdd") & "'"
        Else
            FechaToNULL = "NULL"
        End If
    End Function
    Public Function HoraToString(ByVal sHora As String) As String
        Dim sValor As String
        Dim iLongitud As Integer = 0
        '
        iLongitud = sHora.Length
        '
        sValor = "00:00"
        '
        Select Case iLongitud
            Case 1
                sValor = "00:0" & sHora
            Case 2
                sValor = "00:" & sHora
            Case 3
                sValor = "0" & sHora.Substring(0, 1) & ":" & sHora.Substring(1, 2)
            Case 4
                sValor = sHora.Substring(0, 2) & ":" & sHora.Substring(2, 2)
            Case 5
                sValor = sHora

        End Select
        '
        Return sValor
    End Function
    Public Function EsHora(ByVal sHora As String) As Boolean

        Dim iHoras As Integer
        Dim iMinutos As Integer

        EsHora = False

        sHora = sHora.Replace(":", "")

        If IsNumeric(sHora) Then
            If sHora.Length = 4 Then
                iHoras = sHora.Substring(0, 2)
                iMinutos = sHora.Substring(2, 2)
                If iHoras >= 0 And iHoras <= 23 Then
                    If iMinutos >= 0 And iMinutos <= 59 Then
                        EsHora = True
                    End If
                End If
            End If
        End If

    End Function
    Public Function HoraToInteger(ByVal sHora As String) As Integer
        Dim iTime As Integer = 0
        Dim iHour As Integer

        iTime = sHora.Length

        If iTime >= 4 And sHora.Contains(":") Then
            iHour = Convert.ToInt16(sHora.Replace(":", ""))
            If iHour >= 0 And iHour < 2400 Then
                HoraToInteger = iHour
            Else
                HoraToInteger = 0
            End If
        ElseIf iTime > 1 And Not sHora.Contains(":") Then
            iHour = Convert.ToInt32(sHora)
            If iHour >= 0 And iHour < 2400 Then
                HoraToInteger = iHour
            Else
                HoraToInteger = 0
            End If
        Else
            HoraToInteger = 0
        End If
    End Function
    Public Function DiaSemana(ByVal valor As Object) As String

        Dim iDiaSemana As Integer
        '
        DiaSemana = ""
        '
        If Not IsDate(valor) Then
            Return "null"
        End If
        '
        iDiaSemana = Weekday(valor, FirstDayOfWeek.Monday)
        '
        Select Case iDiaSemana
            Case DayOfWeek.Monday
                Return "Lunes".ToUpper
            Case DayOfWeek.Tuesday
                Return "Martes".ToUpper
            Case DayOfWeek.Wednesday
                Return "Miercoles".ToUpper
            Case DayOfWeek.Thursday
                Return "Jueves".ToUpper
            Case DayOfWeek.Friday
                Return "Viernes".ToUpper
            Case DayOfWeek.Saturday
                Return "Sabado".ToUpper
            Case DayOfWeek.Sunday
                Return "Domingo".ToUpper
            Case 7
                Return "Domingo".ToUpper
        End Select

    End Function
#End Region

#Region "FUNCIONES SELECCIONAR/GUARDAR FICHERO O CARPETA"
    Public Function Seleccionar_Fichero(Optional ByRef sNombreFichero As String = "",
                             Optional ByVal sTipoFichero As String = "",
                             Optional ByVal sDirectorioInicial As String = "") As String

        m_SBOAddon.TipoFichero = sTipoFichero

        If sDirectorioInicial = "" Then
            m_SBOAddon.DirectorioInicial = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        Else
            m_SBOAddon.DirectorioInicial = sDirectorioInicial
        End If

        Dim myThread As New System.Threading.Thread(AddressOf OpenFile)
        myThread.SetApartmentState(Threading.ApartmentState.STA)
        myThread.Start()
        myThread.Join()
        Seleccionar_Fichero = m_SBOAddon.RutaFichero
        sNombreFichero = m_SBOAddon.NombreFichero
        myThread.Abort()

    End Function
    Private Sub OpenFile()
        Try
            Dim dialog As New OpenFileDialog
            Dim owner As New Form With {
                .Width = 200,
                .Height = 200
            }

            owner.Activate()
            owner.BringToFront()
            owner.Visible = True
            owner.TopMost = True
            owner.Focus()
            owner.Visible = False
            dialog.ShowHelp = True
            dialog.InitialDirectory = m_SBOAddon.DirectorioInicial
            If (m_SBOAddon.TipoFichero <> "") Then
                dialog.Filter = m_SBOAddon.TipoFichero
            End If
            dialog.ShowDialog(owner)
            m_SBOAddon.RutaFichero = dialog.FileName
            m_SBOAddon.NombreFichero = mGlobals.ObtenerNombre(dialog.FileName)

        Catch ExcE As Exception
            m_SBOAddon.SBO_Application.MessageBox(ExcE.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub
    Public Function Seleccionar_Directorio()
        Dim sDirectorio As String = ""
        Dim myThread As New System.Threading.Thread(AddressOf SelectFolder)
        '
        myThread.SetApartmentState(Threading.ApartmentState.STA)
        myThread.Start()
        myThread.Join()
        sDirectorio = m_SBOAddon.RutaFichero
        myThread.Abort()
        '
        Return sDirectorio
    End Function
    Private Sub SelectFolder()
        Try
            Dim dialog As New FolderBrowserDialog
            Dim owner As New Form With {
                .Width = 200,
                .Height = 200
            }
            '
            owner.Activate()
            owner.BringToFront()
            owner.Visible = True
            owner.TopMost = True
            owner.Focus()
            owner.Visible = False
            '
            dialog.ShowDialog(owner)
            '
            m_SBOAddon.RutaFichero = dialog.SelectedPath
            '
        Catch ExcE As Exception
            m_SBOAddon.SBO_Application.MessageBox(ExcE.ToString, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub
    Private Function ObtenerNombre(ByVal sRuta As String) As String
        Dim sValor() As String
        sValor = sRuta.Split("\")
        ObtenerNombre = sValor(sValor.Length - 1)
    End Function
#End Region

#Region "FUNCIONES HANA"
    Public Function ConvertToHANA(ByRef oCompany As SAPbobsCOM.Company, ByVal sQuery As String) As String
        ConvertToHANA = sQuery
        If IsHanaDataBase(oCompany) Then
            ConvertToHANA = sQuery.Replace("ISNULL", "IFNULL")
            ConvertToHANA = sQuery.Replace("isnull", "IFNULL")
            ConvertToHANA = sQuery.Replace("Isnull", "IFNULL")
            ConvertToHANA = ConvertToHANA.Replace("dbo.", "")
            ConvertToHANA = ConvertToHANA.Replace("DBO.", "")
            ConvertToHANA = sQuery.Replace(" + ", " || ")
            ConvertToHANA = sQuery.Replace("LEN", "LENGTH")
        End If
    End Function
    Public Function IsHanaDataBase(ByVal oCompany As SAPbobsCOM.Company) As Boolean
        IsHanaDataBase = False
        If oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_HANADB Then
            IsHanaDataBase = True
        End If
    End Function
#End Region

End Module
