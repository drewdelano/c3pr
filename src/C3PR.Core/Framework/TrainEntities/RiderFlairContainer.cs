using System.Collections.Generic;

namespace C3PR.Core.Framework
{
    public class RiderFlairContainer
    {
        public string PreflairWhitepace { get; set; } = "";
        public string PostflairWhitepace { get; set; } = "";
        public List<RiderFlair> Flairs { get; set; } = new List<RiderFlair>();
    }
}