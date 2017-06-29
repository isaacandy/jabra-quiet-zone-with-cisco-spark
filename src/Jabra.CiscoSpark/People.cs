using System;

namespace Jabra.CiscoSpark
{
    public class People
    {
        public string id { get; set; }
        public string[] emails { get; set; }
        public string displayName { get; set; }
        public string nickName { get; set; }
        public string orgId { get; set; }
        public DateTime created { get; set; }
        public DateTime lastActivity { get; set; }
        public string status { get; set; }
        public string type { get; set; }
    }
}