Option Explicit On
'
'
Public Class SEI_AddingPermissions
    '
    '-------------
    'Important
    '-------------
    'Todos los c�digos de permisos tienen que comenzar por una letra
    'el primer caracter no puede ser n�merico
    '
    '
#Region "Variables"
    '
    Const cADDON As String = "ADDON"
    '
    Private oParentAddon As SEI_Addon
    Private sErrNumber As String
    Private iErrMsg As Integer
    '
#End Region

#Region "Contructor"
    Public Sub New(ByRef o_ParentAddon As SEI_Addon)
        oParentAddon = o_ParentAddon
    End Sub
#End Region

#Region "Funcions P�bliques"
    Public Sub AddPermissions()
        '
        Me.A�adirPermiso_Addon()
        '
    End Sub

#End Region
    '
#Region "Funcions Privades"
    Private Sub A�adirPermiso_Addon()
        '
        'Dim oPermission As SAPbobsCOM.UserPermissionTree
        '
        'Carpeta Add-on
        'oPermission = oParentAddon.SBO_Company.GetBusinessObject(oUserPermissionTree)
        'If Not oPermission.GetByKey(cADDON) Then
        '    With oPermission
        '        .IsItem = tNO
        '        .Name = "Add-on SEI_ADDONBASE"
        '        .Options = bou_FullNone
        '        '.ParentID = ""
        '        .PermissionID = cADDON
        '        .UserSignature = oParentAddon.SBO_Company.UserSignature
        '        .Add()
        '    End With
        'End If
        '
        '[PROGRAMAR]
        'Cambio de N� de OF
        'oPermission = oParentAddon.SBO_Company.GetBusinessObject(oUserPermissionTree)
        'If Not oPermission.GetByKey("P" & f_CambiarOF) Then
        '    With oPermission
        '        .IsItem = tNO
        '        .Name = "Cambiar N� de OF"
        '        .Options = bou_FullNone
        '        .ParentID = cADDON
        '        .PermissionID = "P" & f_CambiarOF
        '        With .UserPermissionForms
        '            .FormType = f_CambiarOF
        '        End With
        '        .Add()
        '    End With
        'End If
        '
    End Sub
#End Region

End Class
