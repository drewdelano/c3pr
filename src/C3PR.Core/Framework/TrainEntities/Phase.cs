using System;
using System.Collections.Generic;
using Autofac.Core.Lifetime;

namespace C3PR.Core.Framework
{
    public class Phase
    {
        string _phase;

        public Phase(string phase)
        {
            _phase = phase;

            _knownPhases = (_knownPhases ?? new List<Phase>());
            _knownPhases.Add(this);
        }

        public readonly static Phase Rollcall = new Phase("rollcall");
        public readonly static Phase Merging = new Phase("merging");
        public readonly static Phase Testing = new Phase("qa");
        public readonly static Phase Production = new Phase("prod");
        static List<Phase> _knownPhases;
        public static IReadOnlyList<Phase> KnownPhases => _knownPhases.AsReadOnly();

        public override string ToString()
        {
            return _phase;
        }

        public override int GetHashCode()
        {
            return _phase.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Phase objPhase))
            {
                return false;
            }

            return _phase.Equals(objPhase._phase);
        }

        public static bool operator ==(Phase obj1, Phase obj2)
        {
            return obj1?._phase == obj2?._phase;
        }

        public static bool operator !=(Phase obj1, Phase obj2)
        {
            return obj1?._phase != obj2?._phase;
        }

    }
}