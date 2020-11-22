using System.Threading.Tasks;
using C3PR.Core.Framework;

namespace C3PR.Core.Services
{
    public interface ICarriageManipulationService
    {
        void AddRiderInNewCarriage(Train train, string userName);
        Task AdvanceCarriageIfNecessary(Train train, string channelName);
        void LockCarriage(Carriage carriage);
        void UnlockCarriage(Carriage carriage);
        void AddRiderToCarriageByIndex(Train train, string userName, int specificCarriageNumber);
        void RemoveRiderFromCarriage(Train train, Rider rider, Carriage carriage);
        bool IsPhaseUnknown(Phase phase);
        bool IsNotHeld(Train train, Rider rider);
    }
}