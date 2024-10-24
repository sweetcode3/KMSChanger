namespace KMSChanger.Models
{
    public class Config
    {
        public Dictionary<string, string> ProductKeys { get; set; } = new();
        public List<string> KmsServers { get; set; } = new();

        public bool IsValid()
        {
            return ProductKeys.Count > 0 && KmsServers.Count > 0;
        }

        public string GetProductKeyByEdition(string edition)
        {
            return ProductKeys.FirstOrDefault(x => edition.Contains(x.Key)).Value;
        }
    }
}
