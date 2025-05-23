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
            // Ensure the directory exists
            if (!Directory.Exists(_directoryPath))
            {
                Console.WriteLine($"The directory {_directoryPath} does not exist.");
                return orders;
            }

            string[] files = Directory.GetFiles(_directoryPath, "*.txt");
            Console.WriteLine($"Found {files.Length} .txt files to process");

            foreach (string file in files)
            {
                Console.WriteLine($"Processing file: {file}");
                var fileOrders = ProcessFile(file);
                Console.WriteLine($"Orders extracted from file: {fileOrders.Count}");

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

        Console.WriteLine($"Total orders processed: {orders.Count}");
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
            Console.WriteLine($"Lines read from file: {lines.Length}");

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split('|');

                if (parts.Length < 2)
                {
                    Console.WriteLine($"Line ignored (incorrect format): {line}");
                    continue;
                }

                if (parts[0] == "HEADER")
                {
                    // Add the previous order if it exists
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

                            Console.WriteLine($"New order: ID={currentOrder.DocEntry}, Client={currentOrder.CardCode}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing header: {ex.Message}");
                            currentOrder = null;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Incomplete HEADER: {line}");
                        currentOrder = null;
                    }
                }
                else if (parts[0] == "LINE" && currentOrder != null)
                {
                    if (parts.Length >= 5)
                    {
                        try
                        {
                            Console.WriteLine($"Parsing LINE: {line}");
                            Console.WriteLine($"Parts[1] (DocEntry): '{int.Parse(parts[1].Trim())}'");
                            Console.WriteLine($"Parts[2] (LineNum): '{int.Parse(parts[2].Trim())}'");
                            Console.WriteLine($"Parts[3] (ItemCode): '{parts[3].Trim()}'");
                            Console.WriteLine($"Parts[4] (Quantity): '{int.Parse(parts[4].Trim())}'");

                            var lineItem = new LineItem
                            {
                                DocEntry = int.Parse(parts[1].Trim()),
                                LineNum = int.Parse(parts[2].Trim()),
                                ItemCode = parts[3].Trim(),
                                Quantity = int.Parse(parts[4].Trim()),
                            };

                            // Verify that the line corresponds to the current order
                            if (lineItem.DocEntry == currentOrder.DocEntry)
                            {
                                currentOrder.LineItems.Add(lineItem);
                                Console.WriteLine($"Line added: Order={lineItem.DocEntry}, Line={lineItem.LineNum}, Item={lineItem.ItemCode}");
                            }
                            else
                            {
                                Console.WriteLine($"Incorrect order ID in line: {line}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing line: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Incomplete LINE: {line}");
                    }
                }
            }

            // Add the last order if it exists
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