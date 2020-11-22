using System.Collections.Generic;

namespace C3PR.Core.Framework
{
    public class Carriage
    {
        public List<Rider> Riders { get; set; } = new List<Rider>();
        public CarriageFlairs Flairs { get; set; } = new CarriageFlairs();
    }
}