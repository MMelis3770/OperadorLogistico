namespace Console.Models
{
    public class Client
    {
        public string? CardCode;
        public string? CardName;

        public override string ToString()
        {
            return $"Client [CardCode={CardCode}, CardName={CardName}]";
        }
    }
}
