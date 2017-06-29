namespace Jabra.CiscoSpark
{
    public class Token
    {
        public string access_token { get; set; }
        public double expires_in { get; set; }
        public string refresh_token { get; set; }
        public double refresh_token_expires_in { get; set; }
    }
}