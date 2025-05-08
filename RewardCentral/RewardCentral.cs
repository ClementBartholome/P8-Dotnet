using RewardCentral.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RewardCentral;

public class RewardCentral
{
    public int GetAttractionRewardPoints(Guid attractionId, Guid userId)
    {
        int randomDelay = new Random().Next(1, 1000);
        Thread.Sleep(randomDelay);

        int randomInt = new Random().Next(1, 1000);
        return randomInt;
    }
    
    public async Task<int> GetAttractionRewardPointsAsync(Guid attractionId, Guid userId)
    {
        await Task.Delay(new Random().Next(1, 1000)); // simulate async I/O
        return new Random().Next(1, 1000);
    }
}
