using TourGuide.LibrairiesWrappers.Interfaces;

namespace TourGuide.LibrairiesWrappers
{
    public class RewardCentralWrapper : IRewardCentral
    {
        private readonly RewardCentral.RewardCentral _rewardCentral;

        public RewardCentralWrapper()
        {
            _rewardCentral = new ();
        }

        public int GetAttractionRewardPoints(Guid attractionId, Guid userId)
        {
            return _rewardCentral.GetAttractionRewardPoints(attractionId, userId);
        }

        public Task<int> GetAttractionRewardPointsAsync(Guid attractionAttractionId, Guid userUserId)
        {
            return _rewardCentral.GetAttractionRewardPointsAsync(attractionAttractionId, userUserId);
        }
    }
}
