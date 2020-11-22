namespace C3PR.Core.Framework
{
    public class RiderFlair
    {
        string _flair;
        public RiderFlair(string flair)
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
            if (!(obj is RiderFlair objCarriageFlair))
            {
                return false;
            }

            return _flair.Equals(objCarriageFlair._flair);
        }



        public string FlairText => _flair;
        public string WhitespaceAfter { get; set; } = "";


        public static readonly RiderFlair Ready = new RiderFlair("r")
        {
            WhitespaceAfter = " "
        };

        public static readonly RiderFlair EverReady = new RiderFlair("er")
        {
            WhitespaceAfter = " "
        };
    }
}