'
Public Class SEI_AddonEnum

#Region "CONSTANTES SAP"
    Public Structure enSAPFormType
        ' NUMERACIONES FORMULARIOS
        Const f_FormulariosSBO_0 As String = "0"
        Const f_CFL_AlbaranCompra As String = "10019"
        Const f_CFL_DevolucionCompra As String = "10020"
        Const f_CFL_PedidoCompras As String = "10021"
        Const f_CFL_Empleados As String = "10170"
        Const f_CFL_CuentasMayor As String = "10000"
        Const f_ConsultasFormateadas As String = "9999"
        Const f_Herramientas As String = "4864"
        Const f_VentanasDefUsuario As String = "51200"
        Const f_FormulariosEstandar As String = "47616"
        Const f_Form9999 As String = "9999"
        ' DEFINICIONES
        Const f_ClaseExpedicion As String = "899"
        Const f_Fabricantes As String = "897"
        Const f_GruposArticulos As String = "63"
        Const f_GruposClientes As String = "174"
        Const f_GruposProveedores As String = "247"
        Const f_PlanCuentasTratar As String = "750"
        Const f_PropiedadesIC As String = "422"
        Const f_Responsables As String = "8015"
        Const f_ViasPago As String = "505"
        Const f_ProyectosSAP As String = "711"
        Const f_ParametrizacionesGenerales As String = "138"
        ' FINANZAS
        Const f_InformeFiscal As String = "47"
        Const f_CentrosBeneficio As String = "810"
        Const f_Asientos As String = "392"
        Const f_AsientosBorrador As String = "229"
        ' INTERLOCUTORES COMERCIALES
        Const f_CondicionesPago As String = "177"
        Const f_InterlocutoresComerciales As String = "134"
        ' VENTAS        
        Const f_OfertaVentas As String = "149"
        Const f_PedidoVentas As String = "139"
        Const f_AlbaranVentas As String = "140"
        Const f_DevolucionVentas As String = "180"
        Const f_AbonoVentas As String = "179"
        Const f_FacturaVentas As String = "133"
        Const f_SeleccionLotes As String = "42"
        ' COMPRAS
        Const f_PedidoCompras As String = "142"
        Const f_AlbaranCompras As String = "143"
        Const f_DevolucionCompras As String = "182"
        Const f_FacturaCompras As String = "141"
        Const f_AbonoCompras As String = "181"
        Const f_SolicitudPedido As String = "540000988"
        Const f_SolicitudDevolucionMercancia As String = "234234568"
        ' INVENTARIO
        Const f_Articulos As String = "150"
        Const f_Traslados As String = "940"
        Const f_SolicitudTraslados As String = "1250000940"
        Const f_PreciosEspecialesIC As String = "668"
        ' SERVICIO
        Const f_LlamadaServicio As String = "60110"
        ' RECURSOS HUMANOS
        Const f_Empleados As String = "60100"
        ' GESTION PROYECTOS
        Const f_Proyecto As String = "234000045"
    End Structure
    Public Structure enMenuUID
        ' Menus Add-on
        Const MNU_Principal As String = "1030"
        Const MNU_F1 As String = "275"
        Const MNU_F2 As String = "6402"
        Const MNU_F3 As String = "6403"
        Const MNU_F4 As String = "6404"
        Const MNU_F5 As String = "6405"
        Const MNU_F6 As String = "6406"
        Const MNU_F7 As String = "6407"
        Const MNU_F8 As String = "6408"
        Const MNU_F9 As String = "6409"
        Const MNU_F10 As String = "6410"
        Const MNU_F11 As String = "6411"
        Const MNU_F12 As String = "6412"
        '
        Const MNU_Fichero As String = "512"
        Const MNU_Tratar As String = "768"
        Const MNU_Vista As String = "40960"
        Const MNU_Datos As String = "1280"
        Const MNU_Ir_a As String = "5888"
        Const MNU_Modulos As String = "43520"
        Const MNU_Herramientas As String = "4864"
        Const MNU_Ventana As String = "1024"
        Const MNU_Ayuda As String = "43564"
        '
        Const MNU_Siguiente As String = "1288"
        Const MNU_Anterior As String = "1289"
        Const MNU_Primero As String = "1290"
        Const MNU_Ultimo As String = "1291"
        Const MNU_ActualizarRegistro As String = "1304"
        '
        Const MNU_Copiar As String = "772"
        Const MNU_Pegar As String = "773"
        '
        Const MNU_Fichero_Cerrar As String = "514"
        Const MNU_Buscar As String = "1281"
        Const MNU_Crear As String = "1282"
        Const MNU_Eliminar As String = "1283"
        Const MNU_AñadirLinea As String = "1292"
        Const MNU_CerrarLinea As String = "1299"
        Const MNU_EliminarLinea As String = "1293"
        Const MNU_CopiarCeldaSuperior As String = "1295"
        Const MNU_CopiarCeldaInferior As String = "1296"
        '
        Const MNU_Duplicar As String = "1287"
        Const MNU_Cancelar As String = "1284"
        Const MNU_Cerrar As String = "1286"
        Const MNU_Restablecer As String = "1285"
        Const MNU_Proyectos As String = "8457"
        Const MNU_Catalogo_IC As String = "12545"
        Const MNU_Presentacion_Preliminar_Layout As String = "521"
        Const MNU_Imprimir As String = "520"
        Const MNU_Presentacion_Preliminar As String = "519"
        Const MNU_Buscar_SHIFT_F2 As String = "7425"
        Const MNU_Campos_Usuario As String = "6913"
        Const MNU_FondoClasico As String = "5633"
        Const MNU_FondoNaranja As String = "5639"
        Const MNU_ColoresFondo As String = "5632"
        Const MNU_ColorRojo As String = "254"
        Const MNU_ColorDefecto As String = "-1"
        Const MNU_ParametrizarFormulario As String = "5890"
        Const MNU_Filtrar As String = "4870"
        Const MNU_ProyectosSAP As String = "8457"
        '
        'Form SAP
        Const m_PedidoCompra As String = "2305"
        Const m_AlbaranCompra As String = "2306"
        Const m_DevolucionCompra As String = "2307"
        Const m_FacturaCompra As String = "2308"
        Const m_AbonoCompra As String = "2309"
        Const m_Traslado As String = "3080"
    End Structure
#End Region

#Region "CONSTANTES ADD-ON"
    Public Structure enAddonFormType
        ' FORMULARIOS USUARIO
        Const f_AddonSettings As String = "SEICONFIG"
        Const f_OrdersMonitor As String = "SEIORDERSMONITOR"
        Const f_ConfOrders As String = "SEICONFORDERS"
    End Structure
    Public Structure enAddonMenus
        ' MENUS ADD-ON:
        Const ConfigurarAddon As String = "SEI_ConfigurarAddon"
        Const OrdersMonitor As String = "SEI_OrdersMonitor"
        Const ConfOrders As String = "SEI_ConfOrders"

    End Structure
#End Region

#Region "FORM TYPES"
    Public Enum enSBO_LoadFormTypes
        XmlFile = 0       ' Formularios .srf (xml) 
        LogicOnly = 1     ' Formulario de Sap 
        GuiByCode = 3
    End Enum
#End Region

End Class
