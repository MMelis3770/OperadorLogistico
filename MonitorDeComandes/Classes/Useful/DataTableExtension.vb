Imports SAPbouiCOM
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Xml.Linq
Imports System.Xml.XPath

Public Module DataTableExtension
    '<Extension()>
    'Public Sub SetValues(Of T)(dataTable As DataTable, values As List(Of T))

    '    ' Obtenemos propiedades del tipo de la lista, comprobamos que existan en el DataTable
    '    Dim properties = GetType(T).GetProperties().Where(Function(x) Contains(dataTable.Columns, x.Name)).ToList()
    '    Dim dataCells = CreateDataCells(values, properties)

    '    ' Obtenemos XML, actualizamos y volvemos a cargar
    '    Dim xdata As XDocument = XDocument.Parse(dataTable.SerializeAsXML(BoDataTableXmlSelect.dxs_DataOnly))
    '    Dim rows As XElement = xdata.XPathSelectElement("//Rows")
    '    If rows Is Nothing Then Return

    '    rows.ReplaceAll(dataCells)

    '    Dim xml As String = $"<?xml version=""1.0"" encoding=""UTF-16"" ?>{xdata.ToString(SaveOptions.DisableFormatting)}"
    '    dataTable.LoadSerializedXML(BoDataTableXmlSelect.dxs_DataOnly, xml)

    'End Sub

    'Private Function Contains(columns As DataColumns, name As String) As Boolean

    '    For i As Integer = 0 To columns.Count - 1
    '        If columns.Item(i).Name.Equals((name)) Then
    '            Return True
    '        End If
    '    Next

    '    Return False

    'End Function
    'Private Function CreateDataCells(Of T)(values As List(Of T), properties As IEnumerable(Of PropertyInfo)) As List(Of XElement)

    '    Dim lista As New List(Of XElement)
    '    For Each value As T In values

    '        Dim XElement As XElement = New XElement("Row",
    '               New XElement("Cells", CreateDataCellProperties(Of T)(value, properties)))

    '        lista.Add(XElement)

    '    Next

    '    Return lista

    'End Function
    'Private Function CreateDataCellProperties(Of T)(value As T, properties As List(Of PropertyInfo)) As List(Of XElement)

    '    Dim lista As New List(Of XElement)
    '    For Each prop As PropertyInfo In properties

    '        Dim XElement As XElement = New XElement("Cell",
    '                        New XElement("ColumnUid", prop.Name),
    '                        New XElement("Value", GetValue(prop, value)))
    '        lista.Add(XElement)

    '    Next

    '    Return lista

    'End Function
    'Private Function GetValue(Of T)(prop As PropertyInfo, value As T) As Object

    '    Dim propertyValue = prop.GetValue(value)

    '    If prop.PropertyType Is GetType(Date) Then
    '        If (propertyValue = Date.MinValue) Then
    '            Return ""
    '        Else
    '            Return Convert.ToDateTime(propertyValue).ToString("yyyyMMdd")
    '        End If
    '    End If

    '    Return Convert.ToString(propertyValue, CultureInfo.InvariantCulture)

    'End Function

    <Extension()>
    Public Sub SetValuesDynamic(Of T)(dataTable As DataTable, values As List(Of T))
        If (GetType(T).GetProperties().ToList().Any()) Then
            SetValuesOfT(dataTable, values)
        Else
            SetValuesDictionary(dataTable, values)
        End If
    End Sub

    Private Sub SetValuesOfT(Of T)(dataTable As DataTable, values As List(Of T))
        ' Obtenemos propiedades del tipo de la lista, comprobamos que existan en el DataTable
        Dim properties = GetType(T).GetProperties().ToList()
        Dim dataCells = CreateDataCells(values, properties.ToDictionary(Function(x) x.Name, Function(x) x.PropertyType))

        Dim xdata As XDocument = XDocument.Parse(dataTable.SerializeAsXML(BoDataTableXmlSelect.dxs_All))

        Dim columns As XElement = xdata.XPathSelectElement("//Columns")
        If columns Is Nothing Then Return
        columns.ReplaceWith(CreateColumns(properties))

        ' Obtenemos XML, actualizamos y volvemos a cargar
        Dim rows As XElement = xdata.XPathSelectElement("//Rows")
        If rows Is Nothing Then Return
        rows.ReplaceAll(dataCells)

        Dim xml As String = $"<?xml version=""1.0"" encoding=""UTF-16"" ?>{xdata.ToString(SaveOptions.DisableFormatting)}"
        dataTable.LoadSerializedXML(BoDataTableXmlSelect.dxs_All, xml)
    End Sub

    Private Sub SetValuesDictionary(Of T)(dataTable As DataTable, values As List(Of T))
        Dim entityDictonary As IDictionary(Of String, Object) = DirectCast(values.FirstOrDefault(), IDictionary(Of String, Object))
        Dim properties As Dictionary(Of String, Type) = entityDictonary.ToDictionary(Function(prop) prop.Key, Function(prop) If(prop.Value IsNot Nothing, prop.Value.GetType(), GetType(DBNull)))

        Dim dataCells = CreateDataCells(values, properties)

        ' Obtenemos XML, actualizamos y volvemos a cargar
        Dim xdata As XDocument = XDocument.Parse(dataTable.SerializeAsXML(BoDataTableXmlSelect.dxs_All))

        Dim columns As XElement = xdata.XPathSelectElement("//Columns")
        If columns Is Nothing Then Return
        columns.ReplaceAll(CreateColumns(properties))

        Dim rows As XElement = xdata.XPathSelectElement("//Rows")
        If rows Is Nothing Then Return
        rows.ReplaceAll(dataCells)

        Dim xml As String = $"<?xml version=""1.0"" encoding=""utf-8"" ?> {xdata.ToString(SaveOptions.DisableFormatting)}"
        dataTable.LoadSerializedXML(BoDataTableXmlSelect.dxs_All, xml)
    End Sub

    Private Function CreateColumns(properties As Dictionary(Of String, Type)) As XElement
        Dim columns As New XElement("Columns")

        For Each prop As KeyValuePair(Of String, Type) In properties
            Dim column As New XElement("Column")
            column.SetAttributeValue("Uid", prop.Key)
            column.SetAttributeValue("Type", GetColumnType(prop.Value))
            column.SetAttributeValue("MaxLength", "255")
            columns.Add(column)
        Next

        Return columns
    End Function

    Private Function CreateColumns(properties As List(Of PropertyInfo)) As XElement
        Dim columns As New XElement("Columns")

        For Each prop As PropertyInfo In properties
            Dim Column As New XElement("Column")
            Column.SetAttributeValue("Uid", prop.Name)
            Column.SetAttributeValue("Type", GetColumnType(prop.PropertyType))
            Column.SetAttributeValue("MaxLength", "255")
            columns.Add(Column)
        Next

        Return columns
    End Function

    Private Function GetColumnType(type As Type) As String
        ' Check if the type is nullable and get the underlying type
        Dim underlyingType As Type = If(Nullable.GetUnderlyingType(type), type)

        Select Case True
            Case GetType(Integer).IsAssignableFrom(underlyingType), GetType(Double).IsAssignableFrom(underlyingType), GetType(Decimal).IsAssignableFrom(underlyingType), GetType(Single).IsAssignableFrom(underlyingType), GetType(Long).IsAssignableFrom(underlyingType), GetType(Short).IsAssignableFrom(underlyingType)
                Return "8"
            Case GetType(String).IsAssignableFrom(underlyingType)
                Return "1"
            Case GetType(Date).IsAssignableFrom(underlyingType), GetType(DateTime).IsAssignableFrom(underlyingType)
                Return "4"
            Case Else
                Return "1"
        End Select
    End Function

    Private Function CreateDataCells(Of T)(values As List(Of T), properties As Dictionary(Of String, Type)) As List(Of XElement)

        Dim lista As New List(Of XElement)
        For Each value As T In values

            Dim XElement As XElement = New XElement("Row",
                   New XElement("Cells", CreateDataCellProperties(value, properties)))

            lista.Add(XElement)

        Next

        Return lista

    End Function
    Private Function CreateDataCellProperties(value As IDictionary(Of String, Object), properties As Dictionary(Of String, Type)) As List(Of XElement)

        Dim lista As New List(Of XElement)
        For Each prop As KeyValuePair(Of String, Type) In properties

            Dim XElement As XElement = New XElement("Cell",
                            New XElement("ColumnUid", prop.Key),
                            New XElement("Value", GetValue(prop, value)))
            lista.Add(XElement)

        Next

        Return lista

    End Function
    Private Function GetValue(prop As KeyValuePair(Of String, Type), value As IDictionary(Of String, Object)) As Object

        Dim propertyValue = value(prop.Key)

        If prop.Value Is GetType(Date) Then
            If (propertyValue = Date.MinValue) Then
                Return ""
            Else
                Return Convert.ToDateTime(propertyValue).ToString("yyyyMMdd")
            End If
        End If

        Return Convert.ToString(propertyValue, CultureInfo.InvariantCulture)

    End Function
End Module
