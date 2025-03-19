Option Explicit On
Imports System.Collections
Imports System.Data

Public Class SEI_SQL

#Region "Atributs"

    Dim oSqlConnection As New SqlClient.SqlConnection

    Dim lRetCode As Long
    Dim lErrCode As Long
    Dim sErrMsg As String

    Protected _SQLstring As String
    Protected _Form As SAPbouiCOM.Form
    Protected _Addon As SEI_Addon
    '
    Property SQLstring() As String
        Get
            SQLstring = _SQLstring
        End Get
        Set(ByVal value As String)
            _SQLstring = value
        End Set
    End Property
    '
    Property Form() As SAPbouiCOM.Form
        Get
            Form = _Form

        End Get
        Set(ByVal value As SAPbouiCOM.Form)
            _Form = value
        End Set
    End Property
    '
    Property Addon() As SEI_Addon
        Get
            Addon = _Addon
        End Get
        Set(ByVal value As SEI_Addon)
            _Addon = value
        End Set
    End Property
#End Region

#Region "Constructor"
    Public Sub New(ByRef ParentAddon As SEI_Addon)
        Me.Addon = ParentAddon
    End Sub

    Public Sub New(ByRef oParentAddon As SEI_Addon, ByRef oForm As SAPbouiCOM.Form)
        Me.Addon = oParentAddon
        Me.Form = oForm
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

#End Region

#Region "Funcions"
    Public Sub Crear()
        Dim lNum As Long = 2
        Dim oProgress As SAPbouiCOM.ProgressBar = Nothing
        '
        Try
            Me.AbrirConexionSQL()
            '
            oProgress = Nothing
            oProgress = SubMain.m_SBOAddon.SBO_Application.StatusBar.CreateProgressBar("Creando procedimientos", lNum, False)
            oProgress.Value = 1
            '
            '[PROGRAMAR] 
            'oProgress.Text = "Creando Procedimiento SEIPA"
            'Me.Fitxer_SQL("SEIPA.SQL")
            'oProgress.Value += 1
            '
            Me.CerrarConexionSQL()
            '
            oProgress.Value = lNum
            '
        Catch ex As Exception
            Me.Addon.SBO_Application.MessageBox(ex.Message)
        Finally
            LiberarObjCOM(oProgress)
        End Try
    End Sub
    Private Sub Fitxer_SQL(ByVal sFichero As String)
        Dim sSQL As String
        Dim sBufer As String
        Dim iIndex As Integer = 0
        Dim indant As Integer = 0
        Dim aLista As New ArrayList
        '
        ' Primer miro si existeix. Si existeix, l'esborro perquè es creï després:
        If Me.ExisteixElProcediment(sFichero) Then
            Me.DropProcediment(sFichero)
        End If
        ' LA PALABRA "GO"  no se puede ejecutar directament en la sentencia "oSqlCommand.ExecuteNonQuery()"
        ' es necesario substituirla por el caracter "[]" para ejecutar sentencias
        ' separadas , es decir , por cada final GO, hay que llamar a la rutina EjecutarSentencia()
        ' al inicio del fichero y al final es necesario poner "[]"
        '
        ' Ejemplo
        '
        'SET ANSI_NULLS ON
        'GO

        '
        'SET ANSI_NULLS ON
        '$
        '
        sBufer = SEI_Resources.GetEmbeddedResource(GetType(SEI_SQL), sFichero)
        '
        If sBufer.IndexOf("$") <> -1 Then
            aLista.Add(0)
            While iIndex < sBufer.LastIndexOf("$")
                iIndex = sBufer.IndexOf("$", iIndex + 1)
                aLista.Add(iIndex)
            End While

            For i As Integer = 1 To aLista.Count - 1
                sSQL = sBufer.Substring(aLista(i - 1) + 1, aLista(i) - aLista(i - 1) - 1)
                Me.EjecutaSentencia(sSQL)
            Next
        Else
            Me.EjecutaSentencia(sBufer)
        End If
        '
    End Sub
    Private Sub DropProcediment(ByVal sNomProcediment As String)
        Dim oSqlCommand As SqlClient.SqlCommand
        Dim sSql As String = ""
        '
        sNomProcediment = Trim(sNomProcediment).Replace(".SQL", "")
        sSql = "DROP PROC " & sNomProcediment
        '
        oSqlCommand = New SqlClient.SqlCommand(sSql, oSqlConnection)
        oSqlCommand.ExecuteNonQuery()
        oSqlCommand.Dispose()
    End Sub
    Private Function ExisteixElProcediment(ByVal sNomProcediment As String) As Boolean
        Dim oRecordset As SAPbobsCOM.Recordset
        Dim ls As String = ""
        Dim bExist As Boolean = False
        '
        sNomProcediment = Trim(sNomProcediment).Replace(".SQL", "")
        oRecordset = m_SBOAddon.SBO_Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        '
        ls = " SELECT " & vbCrLf
        ls &= " name " & vbCrLf
        ls &= " FROM sysobjects " & vbCrLf
        ls &= " WHERE name = '" & sNomProcediment & "' " & vbCrLf
        ls &= " AND type = 'P' " & vbCrLf
        '
        oRecordset.DoQuery(ls)
        '
        If Not oRecordset.EoF Then
            bExist = True
        End If
        '
        LiberarObjCOM(oRecordset)
        '
        Return bExist
        '
    End Function
    Private Function EjecutaSentencia(ByVal sSql As String) As Boolean
        Dim oSqlCommand As SqlClient.SqlCommand
        '
        oSqlCommand = New SqlClient.SqlCommand(sSql, oSqlConnection)
        oSqlCommand.ExecuteNonQuery()
        oSqlCommand.Dispose()
        '
    End Function
    Private Sub AbrirConexionSQL()
        '
        Dim ls As String
        ls = "Server=" & Me.Addon.SBO_Company.Server & ";" &
             "Database=" & Me.Addon.SBO_Company.CompanyDB & ";" &
             "User id=" & Me.Addon.oAddonSettings.UserServer & ";" &
             "Password=" & Me.Addon.oAddonSettings.PasswordServer & ";"

        Me.SQLstring = ls

        oSqlConnection.ConnectionString = ls
        oSqlConnection.Open()

    End Sub
    Private Sub CerrarConexionSQL()
        '
        oSqlConnection.Close()
        oSqlConnection.Dispose()
        '
    End Sub
#End Region

End Class
