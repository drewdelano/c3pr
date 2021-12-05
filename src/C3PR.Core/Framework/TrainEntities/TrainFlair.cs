using System;
using System.Collections.Generic;
using System.Text;

namespace C3PR.Core.Framework
{
    public class TrainFlair
    {
        string _flair;

        public TrainFlair(string flair)
        {
            _flair = flair;
        }

        public static readonly TrainFlair Hold = new TrainFlair("hold");
        public static readonly TrainFlair Train = new TrainFlair("choo");


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
            if (!(obj is TrainFlair objTrainFlair))
            {
                return false;
            }

            return _flair.Equals(objTrainFlair._flair);
        }
    }
}
