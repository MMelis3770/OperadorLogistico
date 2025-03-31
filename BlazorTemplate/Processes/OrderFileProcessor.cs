using BlazorTemplate.Models;

namespace BlazorTemplate.Processes
{
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
                string[] files = Directory.GetFiles(_directoryPath, "*.txt");

                foreach (string file in files)
                {
                    var fileOrders = ProcessFile(file);
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

                foreach (string line in lines)
                {
                    string[] parts = line.Split('|');

                    if (parts[0] == "HEADER")
                    {
                        if (currentOrder != null)
                        {
                            orders.Add(currentOrder);
                        }

                        currentOrder = new OrderData
                        {
                            ID = int.Parse(parts[1]),
                            Client = parts[2],
                            OrderDate = DateTime.Parse(parts[3]),
                            DueDate = DateTime.Parse(parts[4])
                        };
                    }
                    else if (parts[0] == "LINE" && currentOrder != null)
                    {
                        var lineItem = new LineItem
                        {
                            OrderID = int.Parse(parts[1]),
                            LineNumber = int.Parse(parts[2]),
                            ItemCode = parts[3],
                            Quantity = int.Parse(parts[4]),
                        };

                        currentOrder.LineItems.Add(lineItem);
                    }
                }

                // Add the last order if there is one
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
}

