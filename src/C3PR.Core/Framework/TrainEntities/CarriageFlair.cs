using System;
using System.Diagnostics.CodeAnalysis;

namespace C3PR.Core.Framework
{
    public class CarriageFlair
    {
        string _flair;
        public CarriageFlair(string flair)
        {
            _flair = flair;
        }

        public override string ToString()
        {
            return _flair;
        }

        public override int GetHashCode()
        {
            return _flair.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CarriageFlair objCarriageFlair))
            {
                return false;
            }

            return _flair.Equals(objCarriageFlair._flair);
        }


        public string TrailingWhitespace { get; set; } = "";


        public static readonly CarriageFlair Lock = new CarriageFlair("l");
    }
}