using BlazorTemplate.Models;

public class OrderFileProcessor
{
    private readonly string _directoryPath;

    public OrderFileProcessor(string directoryPath)
    {
        _directoryPath = directoryPath;
    }

    // Process all txt files in the directory
    public List<OrderData> ProcessFiles()
    {
        List<OrderData> orders = new List<OrderData>();
        try
        {
            // Asegurarse de que el directorio existe
            if (!Directory.Exists(_directoryPath))
            {
                Console.WriteLine($"El directorio {_directoryPath} no existe.");
                return orders;
            }

            string[] files = Directory.GetFiles(_directoryPath, "*.txt");
            Console.WriteLine($"Encontrados {files.Length} archivos .txt para procesar");

            foreach (string file in files)
            {
                Console.WriteLine($"Procesando archivo: {file}");
                var fileOrders = ProcessFile(file);
                Console.WriteLine($"Órdenes extraídas del archivo: {fileOrders.Count}");

                if (fileOrders.Any())
                {
                    orders.AddRange(fileOrders);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing files: {ex.Message}");
        }

        Console.WriteLine($"Total de órdenes procesadas: {orders.Count}");
        return orders;
    }

    // Process txt files
    private List<OrderData> ProcessFile(string filePath)
    {
        List<OrderData> orders = new List<OrderData>();
        OrderData currentOrder = null;

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            Console.WriteLine($"Líneas leídas del archivo: {lines.Length}");

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split('|');

                if (parts.Length < 2)
                {
                    Console.WriteLine($"Línea ignorada (formato incorrecto): {line}");
                    continue;
                }

                if (parts[0] == "HEADER")
                {
                    // Añadir la orden anterior si existe
                    if (currentOrder != null)
                    {
                        orders.Add(currentOrder);
                    }

                    if (parts.Length >= 5)
                    {
                        try
                        {
                            currentOrder = new OrderData
                            {
                                DocEntry = int.Parse(parts[1].Trim()),
                                CardCode = parts[2].Trim(),
                                OrderDate = DateTime.Parse(parts[3].Trim()),
                                DocDueDate = DateTime.Parse(parts[4].Trim())
                            };

                            Console.WriteLine($"Nueva orden: ID={currentOrder.DocEntry}, Cliente={currentOrder.CardCode}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al parsear la cabecera: {ex.Message}");
                            currentOrder = null;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"HEADER incompleto: {line}");
                        currentOrder = null;
                    }
                }
                else if (parts[0] == "LINE" && currentOrder != null)
                {
                    if (parts.Length >= 5)
                    {
                        try
                        {
                            Console.WriteLine($"Parseando LINE: {line}");
                            Console.WriteLine($"Parts[1] (DocEntry): '{int.Parse(parts[1].Trim())}'");
                            Console.WriteLine($"Parts[2] (LineNum): '{int.Parse(parts[2].Trim())}'");
                            Console.WriteLine($"Parts[3] (LineNum): '{parts[3].Trim()}'");
                            Console.WriteLine($"Parts[4] (Quantity): '{int.Parse(parts[4].Trim())}'");

                            var lineItem = new LineItem
                            {
                                DocEntry = int.Parse(parts[1].Trim()),
                                LineNum = int.Parse(parts[2].Trim()),
                                ItemCode = parts[3].Trim(),
                                Quantity = int.Parse(parts[4].Trim()),
                            };

                            // Verificar que la línea corresponde a la orden actual
                            if (lineItem.DocEntry == currentOrder.DocEntry)
                            {
                                currentOrder.LineItems.Add(lineItem);
                                Console.WriteLine($"Línea añadida: Orden={lineItem.DocEntry}, Línea={lineItem.LineNum}, Item={lineItem.ItemCode}");
                            }
                            else
                            {
                                Console.WriteLine($"ID de orden incorrecto en la línea: {line}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al parsear la línea: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"LINE incompleta: {line}");
                    }
                }
            }

            // Añadir la última orden si existe
            if (currentOrder != null)
            {
                orders.Add(currentOrder);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
        }

        return orders;
    }
}