Option Explicit On
Imports System.Linq
Imports SAPbobsCOM
Imports SAPbobsCOM.BoFieldTypes
Imports SAPbobsCOM.BoFldSubTypes
Imports SAPbobsCOM.BoObjectTypes
Imports SAPbobsCOM.BoYesNoEnum

Public Class SEI_CreateTables

#Region "Attributes"
    Protected m_ParentAddon As SEI_Addon
#End Region

#Region "Constructor"
    Public Sub New(ByRef ParentAddon As SEI_Addon)
        m_ParentAddon = ParentAddon
    End Sub
#End Region

#Region "Initialize Function"
    Public Sub AddUserTables()
        Try
            Me.m_ParentAddon.SBO_Application.RemoveWindowsMessage(SAPbouiCOM.BoWindowsMessageType.bo_WM_TIMER, True)
            '
            SubMain.m_SBOAddon.SBO_Application.StatusBar.SetText("Creando tablas y campos de usuario...", SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
            '

        Catch ex As Exception
            SubMain.m_SBOAddon.SBO_Application.StatusBar.SetText("Error al crear tablas de usuario: " & ex.Message)
            SubMain.m_SBOAddon.SBO_Application.MessageBox(ex.Message)
        Finally
            Me.m_ParentAddon.SBO_Application.MetadataAutoRefresh = True
        End Try
    End Sub
#End Region

#Region "Tables Fields and Objects"
    Public Sub CreateSEICONFIG()
        Const sTable As String = "@SEICONFIG"
        Const sDescritpion As String = "Add-on Configuration"
        '
        'TABLE:
        Me.AddUserTable(sTable, sDescritpion, SAPbobsCOM.BoUTBTableType.bott_MasterData)
        '
        ' FIELDS:
        Me.AddUserField(sTable, "SEIUSERDB", "Database User", db_Alpha, 50)
        Me.AddUserField(sTable, "SEIPASSWD", "Database Password", db_Alpha, 50)
        Me.AddUserField(sTable, "SEISLADDR", "Service layer address", db_Alpha, 100)

        Me.AddUserUDO(sTable,
                      sDescritpion,
                      "A" & sTable,
                      BoUDOObjType.boud_MasterData,
                      BoYesNoEnum.tNO,
                      BoYesNoEnum.tNO,
                      BoYesNoEnum.tNO,
                      BoYesNoEnum.tNO,
                      BoYesNoEnum.tNO,
                      BoYesNoEnum.tYES,
                      BoYesNoEnum.tNO,
                      BoYesNoEnum.tNO,
                      "",
                      -1)
    End Sub

#End Region

#Region "Aux Functions"
    Private Sub AddUserTable(ByVal Name As String, ByVal Description As String, ByVal iType As SAPbobsCOM.BoUTBTableType)
        Dim oUserTablesMD As SAPbobsCOM.UserTablesMD = Nothing
        '
        Name = Replace(Name, "@", "")
        '
        oUserTablesMD = Me.m_ParentAddon.SBO_Company.GetBusinessObject(oUserTables)
        If Not oUserTablesMD.GetByKey(Name) Then
            With oUserTablesMD
                .TableName = Name
                .TableDescription = Description
                .TableType = iType
                If .Add <> 0 Then
                    Me.m_ParentAddon.SBO_Application.MessageBox(Me.m_ParentAddon.SBO_Company.GetLastErrorDescription)
                End If
            End With
        End If
        '
        mGlobals.LiberarObjCOM(oUserTablesMD, True)
    End Sub
    Public Sub AddUserField(ByVal Table As String,
                              ByVal Field As String,
                              ByVal Description As String,
                              ByVal Type As SAPbobsCOM.BoFieldTypes,
                              ByVal Size As Integer,
                              Optional ByVal SubType As SAPbobsCOM.BoFldSubTypes = st_None,
                              Optional ByVal DefaultValue As String = "",
                              Optional ByVal CodesValidValues() As String = Nothing,
                              Optional ByVal NamesValidValues() As String = Nothing,
                              Optional ByVal LinkedTable As String = "",
                              Optional ByVal LinkedUDO As String = "",
                              Optional ByVal LinkedSystemObject As SAPbobsCOM.BoObjectTypes = Nothing,
                              Optional ByVal Mandatory As SAPbobsCOM.BoYesNoEnum = tNO)
        '
        Dim oUserFieldsMD As SAPbobsCOM.UserFieldsMD = Nothing
        Dim i As Long
        Dim iFieldID As Integer
        '
        iFieldID = GetUserFieldID(Me.m_ParentAddon.SBO_Company, Table, Field)
        oUserFieldsMD = Me.m_ParentAddon.SBO_Company.GetBusinessObject(oUserFields)
        '
        If Not oUserFieldsMD.GetByKey(Table, iFieldID) Then
            With oUserFieldsMD
                .TableName = Table
                .Name = Field
                .Description = Description
                .Type = Type
                If SubType <> st_None Then .SubType = SubType
                If Mandatory = tYES Then
                    If DefaultValue = "" Then
                        Me.m_ParentAddon.SBO_Application.MessageBox("Debe definirse un valor por defecto para el campo '" & Field & "-" & Description & "'")
                        Exit Sub
                    Else
                        .Mandatory = Mandatory
                    End If
                End If
                If Size <> 0 Then .EditSize = Size
                If LinkedTable <> "" Then .LinkedTable = LinkedTable
                If LinkedUDO <> "" Then .LinkedUDO = LinkedUDO
                If LinkedSystemObject <> 0 Then
                    If LinkedSystemObject <> SAPbobsCOM.BoObjectTypes.BoBridge And LinkedSystemObject <> SAPbobsCOM.BoObjectTypes.BoRecordset Then .LinkedSystemObject = LinkedSystemObject
                End If
                If DefaultValue <> "" Then .DefaultValue = DefaultValue
                If Not IsNothing(CodesValidValues) Then
                    With .ValidValues
                        For i = LBound(CodesValidValues) To UBound(CodesValidValues)
                            If i <> LBound(CodesValidValues) Then .Add()
                            .Value = CodesValidValues(i)
                            .Description = NamesValidValues(i)
                        Next
                    End With
                End If
                If .Add <> 0 Then Me.m_ParentAddon.SBO_Application.MessageBox(Me.m_ParentAddon.SBO_Company.GetLastErrorDescription)
            End With
            '
        Else
            If Not IsNothing(CodesValidValues) Then
                ' RECORRO LOS VALORES VÁLIDOS Y COMPRUEBO SI EXISTEN O NO. SI NO EXISTEN LOS CREARÉ:
                For i = 0 To CodesValidValues.Count - 1
                    If Not Me.UserFieldsListExist(Table, iFieldID, CodesValidValues(i)) Then
                        oUserFieldsMD.ValidValues.Add()
                        oUserFieldsMD.ValidValues.Value = CodesValidValues(i)
                        oUserFieldsMD.ValidValues.Description = NamesValidValues(i)
                    End If
                Next
                '
                If oUserFieldsMD.Update <> 0 Then Me.m_ParentAddon.SBO_Application.MessageBox(Me.m_ParentAddon.SBO_Company.GetLastErrorDescription)
            End If
        End If
        '
        mGlobals.LiberarObjCOM(oUserFieldsMD, True)
    End Sub
    Private Sub AddUserUDO(ByVal Name As String,
                          ByVal Description As String,
                          ByVal LogTable As String,
                          ByVal Type As SAPbobsCOM.BoUDOObjType,
                          ByVal CanCancel As SAPbobsCOM.BoYesNoEnum,
                          ByVal CanClose As SAPbobsCOM.BoYesNoEnum,
                          ByVal DefaultForm As SAPbobsCOM.BoYesNoEnum,
                          ByVal CanDelete As SAPbobsCOM.BoYesNoEnum,
                          ByVal CanFind As SAPbobsCOM.BoYesNoEnum,
                          ByVal CanLog As SAPbobsCOM.BoYesNoEnum,
                          ByVal CanYearTransfer As SAPbobsCOM.BoYesNoEnum,
                          ByVal ManageSeries As SAPbobsCOM.BoYesNoEnum,
                          ByVal FatherMenu As String,
                          ByVal Position As Integer,
                          Optional ByVal Array_ChildTables() As String = Nothing,
                          Optional ByVal Array_FindFields() As String = Nothing,
                          Optional ByVal Array_FormFields() As (String, Integer) = Nothing)
        '
        Dim oUserObject As SAPbobsCOM.UserObjectsMD
        Dim iChild As Integer
        '
        Name = Replace(Name, "@", "")
        '
        oUserObject = Me.m_ParentAddon.SBO_Company.GetBusinessObject(oUserObjectsMD)
        If Not oUserObject.GetByKey(Name) Then
            With oUserObject
                .CanCancel = CanCancel
                .CanClose = CanClose
                .CanCreateDefaultForm = DefaultForm
                .CanDelete = CanDelete
                .CanFind = CanFind
                .CanLog = CanLog
                '.LogTableName = LogTable
                .CanYearTransfer = CanYearTransfer
                .Code = Name
                .TableName = Name
                .ObjectType = Type
                .Name = Description
                .ManageSeries = ManageSeries
                .UseUniqueFormType = tYES
                If Not IsNothing(Array_ChildTables) Then
                    With .ChildTables
                        For iChild = LBound(Array_ChildTables) To UBound(Array_ChildTables)
                            If iChild <> LBound(Array_ChildTables) Then .Add()
                            .TableName = Array_ChildTables(iChild)
                        Next
                    End With
                End If
                If CanFind = tYES Then
                    If Not IsNothing(Array_FindFields) Then
                        With .FindColumns
                            For iChild = LBound(Array_FindFields) To UBound(Array_FindFields)
                                If iChild <> LBound(Array_FindFields) Then .Add()
                                .ColumnAlias = Array_FindFields(iChild)
                            Next
                        End With
                    End If
                End If
                If DefaultForm = tYES Then
                    If Not IsNothing(Array_FormFields) Then
                        With .FormColumns
                            .SonNumber = 0
                            .FormColumnAlias = "Code" 'El Code és obligatori
                            For iChild = LBound(Array_FormFields) To UBound(Array_FormFields)
                                Dim item = Array_FormFields(iChild)
                                If item.Item1.Trim.ToUpper <> "CODE" Then
                                    .Add()
                                    .FormColumnAlias = item.Item1
                                    .SonNumber = item.Item2
                                End If
                            Next
                        End With
                    End If
                    .EnableEnhancedForm = tNO
                    .MenuItem = tYES
                    .MenuCaption = Description
                    .MenuUID = Name
                    .FatherMenuID = FatherMenu
                    .Position = Position
                    .UseUniqueFormType = tYES
                End If
                If .Add <> 0 Then Me.m_ParentAddon.SBO_Application.MessageBox(Me.m_ParentAddon.SBO_Company.GetLastErrorDescription)
            End With
        End If
        '
        mGlobals.LiberarObjCOM(oUserObject, True)
        '
    End Sub
    Private Sub AddUserKey(ByVal TableName As String,
                         ByVal KeyName As String,
                         ByVal Unique As SAPbobsCOM.BoYesNoEnum,
                         ByVal Array_Fields() As String)
        '
        Dim oUserKeysMD As SAPbobsCOM.UserKeysMD = Nothing
        Dim iField As Integer
        '
        TableName = Replace(TableName, "@", "")
        '
        If Not Me.KeyExist(TableName, KeyName) Then
            oUserKeysMD = Me.m_ParentAddon.SBO_Company.GetBusinessObject(oUserKeys)
            With oUserKeysMD
                .KeyName = KeyName
                .TableName = TableName
                .Unique = Unique
                With .Elements
                    For iField = LBound(Array_Fields) To UBound(Array_Fields)
                        If iField <> LBound(Array_Fields) Then .Add()
                        .ColumnAlias = Replace(Array_Fields(iField), "U_", "")
                    Next
                End With
                If .Add <> 0 Then Me.m_ParentAddon.SBO_Application.MessageBox(m_ParentAddon.SBO_Company.GetLastErrorDescription)
            End With
        End If
        '
        mGlobals.LiberarObjCOM(oUserKeysMD, True)
        '
    End Sub
    Private Function KeyExist(ByVal TableName As String, ByVal KeyName As String) As Boolean
        '
        Dim ls As String
        Dim oRecordset As SAPbobsCOM.Recordset = Nothing
        '
        If Left(TableName, 1) <> "@" And Len(TableName) > 4 Then TableName = "@" & TableName
        '
        ls = ""
        ls &= " SELECT" & vbCrLf
        ls &= " TOP 1 ""KeyId""" & vbCrLf
        ls &= " FROM ""OUKD""" & vbCrLf
        ls &= " WHERE (1 = 1)" & vbCrLf
        ls &= " AND (""TableName"" = '" & TableName & "')" & vbCrLf
        ls &= " AND (""KeyName"" = '" & KeyName & "')" & vbCrLf
        '
        oRecordset = Me.m_ParentAddon.SBO_Company.GetBusinessObject(BoRecordset)
        oRecordset.DoQuery(ls)
        If oRecordset.EoF Then
            KeyExist = False
        Else
            KeyExist = True
        End If
        '
        mGlobals.LiberarObjCOM(oRecordset, True)
        '
    End Function

    Private Function UserFieldsListExist(ByVal sTableName As String,
                                       ByVal iFieldID As Integer,
                                       ByVal sFldValue As String) As Boolean

        Dim oRecordset As SAPbobsCOM.Recordset
        Dim ls As String

        oRecordset = Me.m_ParentAddon.SBO_Company.GetBusinessObject(BoRecordset)
        ls = " SELECT ""IndexID"" " & vbCrLf
        ls &= " FROM ""UFD1"" " & vbCrLf
        ls &= " WHERE ""TableID"" = '" & sTableName & "'" & vbCrLf
        ls &= " AND ""FieldID"" = " & iFieldID.ToString & vbCrLf
        ls &= " AND ""FldValue"" = '" & sFldValue & "'" & vbCrLf
        oRecordset.DoQuery(ls)
        '
        If Not oRecordset.EoF Then UserFieldsListExist = True Else UserFieldsListExist = False
        '
        mGlobals.LiberarObjCOM(oRecordset, True)
    End Function

#End Region

#Region "Modify/Delete UserFields"
    Public Sub RemoveUserField(ByVal Table As String,
                           ByVal Field As String)
        '
        Dim oUserFieldsMD As SAPbobsCOM.UserFieldsMD = Nothing
        Dim iFieldID As Integer
        '
        iFieldID = GetUserFieldID(Me.m_ParentAddon.SBO_Company, Table, Field)
        oUserFieldsMD = Me.m_ParentAddon.SBO_Company.GetBusinessObject(oUserFields)
        '
        If oUserFieldsMD.GetByKey(Table, iFieldID) Then

            If oUserFieldsMD.Remove <> 0 Then Me.m_ParentAddon.SBO_Application.MessageBox(Me.m_ParentAddon.SBO_Company.GetLastErrorDescription)

        End If
        '
        mGlobals.LiberarObjCOM(oUserFieldsMD, True)
    End Sub
#End Region

End Class
