using System.Collections.Generic;

namespace C3PR.Core.Framework
{
    public class Rider
    {
        public RiderFlairContainer Flairs { get; set; } = new RiderFlairContainer();
        public string Name { get; set; } = "";
        public string TrailingWhitespace { get; set; } = "";
    }
}