namespace CryptoOculus
{
    public interface IDnsUpdate
    {
        public string[] Hosts { get; }
        public string[] Ips { get; set; }
    }
}
