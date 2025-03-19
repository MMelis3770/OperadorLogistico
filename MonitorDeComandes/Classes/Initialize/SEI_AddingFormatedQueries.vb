Option Strict Off
Option Explicit On
Imports System.Collections.Generic
Imports SAPbobsCOM
Imports SAPbobsCOM.BoFormattedSearchActionEnum
'
Imports SAPbobsCOM.BoObjectTypes
Imports SAPbobsCOM.BoYesNoEnum

Public Class SEI_AddingFormatedQueries

#Region "Variables"

    Private _ParentAddon As SEI_Addon
    Private _Form As SAPbouiCOM.Form
    Private sErrNumber As String
    Private iErrMsg As Integer

    'Aquest mòdul afegeix les búsquedes formatejades necessàries per a l'Add-On
    '
    'Taules implicades
    'OQCN -> Es crearà una categoria amb el nom SDK on s'hi afegiran les consultes formatejades
    'OUQR -> Consultes necessàries per al Gestor de consultes
    'CSHS -> Taula on es relaciona la consulta formatejada amb el item
    'CUVV -> Si la consulta formatejada es basa amb valors existents, aquí és on es guarden aquests valors
    '
    Const c_Categoria As String = "SDK"

    Private Enum ACCIO
        AccioValors = 1
        AccioConsulta = 2
    End Enum

#End Region

#Region "Contructor"
    '
    Public Sub New(ByRef o_ParentAddon As SEI_Addon)
        _ParentAddon = o_ParentAddon
    End Sub
    '
    Public Sub New(ByRef o_ParentAddon As SEI_Addon, ByRef o_Form As SAPbouiCOM.Form)
        _ParentAddon = o_ParentAddon
        _Form = o_Form
    End Sub

#End Region

#Region "Funciones"
    Public Sub AddFormatedQueries()
        ''Afegeixo la categoria "SDK"
        'Me.AfegirCategoria_SDK()
        ''Afegeixo les consultes necessàries dintre la categoria "SDK"
        'Me.AfegirGestorConsultes()
        ''Afegeixo les consultes formatejades necessàries
        'Me.AfegirBusquedesFormatejades()
        '
    End Sub
    Private Sub AfegirGestorConsultes()
        Dim ls As String = ""
        Dim lCategoria As Long
        Dim sNomConsulta As String = ""

        lCategoria = ContadorCategoriaSDK()

        If lCategoria = -1 Then
            Me._ParentAddon.SBO_Application.MessageBox(("No existe la Categoría SDK"))
            Exit Sub
        End If

        ' [PROGRAMAR] --> Definició de les consultes formatejades
        Dim lNumQueries As Long = 4
        '
        Dim oProgress As SAPbouiCOM.ProgressBar = Nothing
        oProgress = Nothing
        oProgress = SubMain.m_SBOAddon.SBO_Application.StatusBar.CreateProgressBar("Creando Consultas Formateadas", lNumQueries, False)
        oProgress.Value = 0
        '
        ' ''-------------------------------------------------------------------------------------------
        ' '' Consulta de fabricants
        ' ''-------------------------------------------------------------------------------------------
        'sNomConsulta = "Lista de fabricantes"
        'ls = "SELECT FirmCode, FirmName from OMRC"
        'AddQuery(lCategoria, sNomConsulta, ls)
        'oProgress.Value += 1
        'oProgress.Text = "Creando consulta de" & " " & sNomConsulta
        'Application.DoEvents()
        'ls = "SELECT ""UomName"" FROM ""OUOM"" WHERE ""UomEntry"" = $[@SEIDCOMCI.U_SEIUM];"
        'sNomConsulta = "SEI_DC_GetNameUM"
        'AddQuery(lCategoria, sNomConsulta, ls)
        'oProgress.Value += 1
        'oProgress.Text = "Creando consulta de" & " " & sNomConsulta
        oProgress.Value = lNumQueries

        LiberarObjCOM(oProgress)
        '
    End Sub
    Private Sub AfegirBusquedesFormatejades()

        '-------------------------------------------------------------------------------------------
        ' Llista de Tarifes
        '-------------------------------------------------------------------------------------------
        'Me.AddFormQuery(enAddOnFormType.f_Articulos, "Lista de Precios", "grdCond", "U_SEIListNum")
        'Me.AddFormQuery("SEIDCMCI1",
        '                "3",
        '                FormattedSearchByFieldEnum.fsbfWhenColumnValueChanges,
        '                "SEI_DC_GetNameUM",
        '                "U_SEIUM",
        '                action:=BoFormattedSearchActionEnum.bofsaQuery,
        '                columnID:="U_SEIUMN",
        '                refresh:=BoYesNoEnum.tYES)
    End Sub

#End Region

#Region "Funciones Auxiliares"

    'CSHS, OUQR

    Public Sub EliminarConsultesFormatejades()
        '
        Me.EliminarConsultas()
        Me.EliminarBusquedas()
        '
    End Sub

    Private Sub EliminarConsultas()

        'me.EliminarConsulta (sconsulta)

    End Sub

    Private Sub EliminarBusquedas()

        'EliminarBusqueda(f_OF_Teknics, "grdlf", "Col9", ACCIO.AccioConsulta)

    End Sub

    Private Sub EliminarBusqueda(ByVal sForm As String, ByVal sItem As String, ByVal sCol As String, ByVal sAccio As ACCIO)
        '
        Dim ls As String
        Dim oRecordSet As SAPbobsCOM.Recordset
        '
        If sCol = "" Then sCol = "-1"
        '
        ls = ""
        ls = ls & " DELETE CSHS "
        ls = ls & " WHERE  FormID='" & sForm & "'"
        ls = ls & " AND    ItemID='" & sItem & "'"
        ls = ls & " AND    ColID ='" & sCol & "'"

        oRecordSet = Nothing
        oRecordSet = _ParentAddon.SBO_Company.GetBusinessObject(BoRecordset)
        oRecordSet.DoQuery(ls)
        '
        'Si és una búsqueda formatejada per valors definits, els valors estaran a la variables "sValors" separats per "|"
        If sAccio = ACCIO.AccioValors Then
            '
            ls = ""
            ls = ls & " DELETE CUVV "
            ls = ls & " WHERE  FormID='" & sForm & "'"
            ls = ls & " AND    ItemID='" & sItem & "'"
            ls = ls & " AND    ColID ='" & sCol & "'"
            '
            oRecordSet = Nothing
            oRecordSet = _ParentAddon.SBO_Company.GetBusinessObject(BoRecordset)
            oRecordSet.DoQuery(ls)
            '
        End If
        '
        oRecordSet = Nothing
        '
    End Sub

    Private Function ExistsBusqueda(ByVal sForm As String, ByVal sItem As String, ByVal sCol As String) As Boolean

        If sCol = "" Then sCol = "-1"

        Dim sSQL As String
        sSQL = "SELECT ""IndexID"" FROM ""CSHS"" " &
               " WHERE ""FormID"" = '" & sForm & "' " &
               " AND ""ItemID"" = '" & sItem & "' " &
               " AND ""ColID"" = '" & sCol & "'"

        Dim oRecordSet As SAPbobsCOM.Recordset
        oRecordSet = _ParentAddon.SBO_Company.GetBusinessObject(BoRecordset)
        oRecordSet.DoQuery(sSQL)

        If oRecordSet.EoF Then
            ExistsBusqueda = False
        Else
            ExistsBusqueda = True
        End If

    End Function
    '
    Private Sub InserirBusqueda(ByVal sForm As String,
                                ByVal sItem As String,
                                ByVal sCol As String,
                                ByVal lQueryID As Long)
        '
        Dim oBusqueda As SAPbobsCOM.FormattedSearches
        '
        If sCol = "" Then sCol = "-1"
        '
        oBusqueda = _ParentAddon.SBO_Company.GetBusinessObject(oFormattedSearches)
        With oBusqueda
            .Action = bofsaQuery
            .ByField = tNO
            .ColumnID = sCol
            .FieldID = ""
            .ForceRefresh = tNO
            .FormID = sForm
            .ItemID = sItem
            .QueryID = lQueryID
            .Refresh = tNO
        End With
        If oBusqueda.Add <> 0 Then

        End If
        '
    End Sub

    Private Sub InserirBusqueda(formID As String,
                                itemID As String,
                                byFieldEx As FormattedSearchByFieldEnum,
                                Optional lQueryID? As Long = Nothing,
                                Optional fieldID As String = "",
                                Optional columnID As String = "-1",
                                Optional action As BoFormattedSearchActionEnum = bofsaNone,
                                Optional byField As BoYesNoEnum = tNO,
                                Optional refresh As BoYesNoEnum = tNO,
                                Optional forceRefresh As BoYesNoEnum = tNO,
                                Optional fieldIDs As List(Of Object) = Nothing,
                                Optional userValidValues As List(Of Object) = Nothing)
        '
        Dim oBusqueda As SAPbobsCOM.FormattedSearches
        '
        If columnID = "" Then columnID = "-1"
        '
        oBusqueda = _ParentAddon.SBO_Company.GetBusinessObject(oFormattedSearches)
        With oBusqueda
            .FormID = formID
            .ItemID = itemID
            .ColumnID = columnID
            .Action = action
            If (lQueryID IsNot Nothing) Then .QueryID = lQueryID
            .Refresh = refresh
            .FieldID = fieldID
            .ForceRefresh = forceRefresh
            .ByField = byField
            .ByFieldEx = byFieldEx
            If (fieldIDs IsNot Nothing) Then
                Dim oFieldIDs = .FieldIDs
                For Each field In fieldIDs
                    oFieldIDs.FieldID = field
                    oFieldIDs.Add()
                Next
            End If
            If (userValidValues IsNot Nothing) Then
                Dim oUVV = .UserValidValues
                For Each field In userValidValues
                    oUVV.FieldValue = field
                    oUVV.Add()
                Next
            End If
        End With
        If oBusqueda.Add <> 0 Then
            Me._ParentAddon.SBO_Application.MessageBox(Me._ParentAddon.SBO_Company.GetLastErrorDescription)
        End If
        '
    End Sub

    Private Sub InserirConsulta(ByVal lCategoria As Long, ByVal sConsulta As String, ByVal sSQL As String)
        '
        Dim oConsulta As SAPbobsCOM.UserQueries
        '
        oConsulta = _ParentAddon.SBO_Company.GetBusinessObject(oUserQueries)
        With oConsulta
            .Query = sSQL
            .QueryCategory = lCategoria
            .QueryDescription = sConsulta
            .Add()
        End With
    End Sub
    '
    Private Function ExisteixConsulta(ByVal sConsulta As String) As Boolean

        Dim sSQL As String
        sSQL = "SELECT ""IntrnalKey"" FROM ""OUQR"" WHERE ""QName"" = '" & sConsulta & "'"

        Dim oRecordSet As SAPbobsCOM.Recordset
        oRecordSet = _ParentAddon.SBO_Company.GetBusinessObject(BoRecordset)
        oRecordSet.DoQuery(sSQL)

        If oRecordSet.EoF Then
            ExisteixConsulta = False
        Else
            ExisteixConsulta = True
        End If

    End Function
    '
    Private Function EliminarConsulta(ByVal sConsulta As String) As Boolean
        '
        Dim ls As String
        Dim oRecordSet As SAPbobsCOM.Recordset
        '
        ls = "DELETE OUQR WHERE QName = '" & sConsulta & "'"
        oRecordSet = Nothing
        oRecordSet = _ParentAddon.SBO_Company.GetBusinessObject(BoRecordset)
        oRecordSet.DoQuery(ls)
        '
    End Function
    '
    Private Function AfegirCategoria_SDK() As Long
        '
        Dim lCategoria As Long
        Dim oCategoria As SAPbobsCOM.QueryCategories
        Dim oRecordSet As SAPbobsCOM.Recordset
        Dim sKey As String
        Dim sSQL As String
        '
        sKey = ""
        'Comprovo que no existeixi la categoria
        sSQL = "SELECT ""CategoryId"" FROM ""OQCN"" WHERE ""CatName"" = '" & c_Categoria & "'"

        oRecordSet = _ParentAddon.SBO_Company.GetBusinessObject(BoRecordset)
        oRecordSet.DoQuery(sSQL)

        If oRecordSet.EoF Then
            'Afegeixo la categoria SDK
            oCategoria = _ParentAddon.SBO_Company.GetBusinessObject(oQueryCategories)
            oCategoria.Name = c_Categoria
            If oCategoria.Add = 0 Then
                _ParentAddon.SBO_Company.GetNewObjectCode(sKey)
                lCategoria = NullToLong(sKey)
            End If
            '
            oCategoria = Nothing
        Else
            AfegirCategoria_SDK = oRecordSet.Fields.Item("CategoryID").Value
        End If
        '
        oRecordSet = Nothing
        '
        AfegirCategoria_SDK = lCategoria
        '
    End Function

    Private Sub AddFormQuery(ByVal sFormulario As String,
                             ByVal sNomConsulta As String,
                             ByVal sItem As String,
                             ByVal sCol As String)

        Dim sSQL As String
        Dim oRecordSet As SAPbobsCOM.Recordset

        If Not ExistsBusqueda(sFormulario, sItem, sCol) Then

            'Busco l'identificador de la consulta---------- Nom Consulta
            sSQL = "SELECT IntrnalKey FROM OUQR WHERE QName = '" & sNomConsulta & "'"
            oRecordSet = _ParentAddon.SBO_Company.GetBusinessObject(BoRecordset)
            oRecordSet.DoQuery(sSQL)

            If Not oRecordSet.EoF Then
                InserirBusqueda(sFormulario, sItem, sCol, oRecordSet.Fields.Item("IntrnalKey").Value)
            End If
        End If

    End Sub

    Private Sub AddFormQuery(formID As String,
                            itemID As String,
                            byFieldEx As FormattedSearchByFieldEnum,
                            queryName As String,
                            Optional fieldID As String = "",
                            Optional columnID As String = "-1",
                            Optional action As BoFormattedSearchActionEnum = bofsaNone,
                            Optional byField As BoYesNoEnum = tNO,
                            Optional refresh As BoYesNoEnum = tNO,
                            Optional forceRefresh As BoYesNoEnum = tNO,
                            Optional fieldIDs As List(Of Object) = Nothing,
                            Optional userValidValues As List(Of Object) = Nothing)

        Dim sSQL As String
        Dim oRecordSet As SAPbobsCOM.Recordset

        If Not ExistsBusqueda(formID, itemID, columnID) Then

            'Busco l'identificador de la consulta---------- Nom Consulta
            sSQL = "SELECT ""IntrnalKey"" FROM ""OUQR"" WHERE ""QName"" = '" & queryName & "'"
            oRecordSet = _ParentAddon.SBO_Company.GetBusinessObject(BoRecordset)
            oRecordSet.DoQuery(sSQL)

            If Not oRecordSet.EoF Then
                InserirBusqueda(formID,
                                itemID,
                                byFieldEx,
                                CLng(oRecordSet.Fields.Item("IntrnalKey").Value),
                                fieldID,
                                columnID,
                                action,
                                byField,
                                refresh,
                                forceRefresh,
                                fieldIDs,
                                userValidValues)
            End If
        End If

    End Sub

    Private Function ContadorCategoriaSDK() As Long

        Dim sSQL As String
        Dim oRecordSet As SAPbobsCOM.Recordset

        ContadorCategoriaSDK = -1

        'Busco l'identificador de la categoria SDK
        sSQL = "SELECT ""CategoryId"" FROM ""OQCN"" WHERE ""CatName"" = '" & c_Categoria & "'"

        oRecordSet = _ParentAddon.SBO_Company.GetBusinessObject(BoRecordset)
        oRecordSet.DoQuery(sSQL)

        If Not oRecordSet.EoF Then
            ContadorCategoriaSDK = oRecordSet.Fields.Item("CategoryId").Value
        End If

    End Function
    '
    Private Sub AddQuery(ByVal lCategoria As Long, ByVal sNomConsulta As String, ByVal sSQL As String)

        ' lCategoria   -> Categoria SDK
        ' sNomConsulta -> Nombre de la Consulta
        ' sSQL         -> Sentencia SQL

        If Not ExisteixConsulta(sNomConsulta) Then
            InserirConsulta(lCategoria, sNomConsulta, sSQL)
        End If
    End Sub

#End Region

End Class