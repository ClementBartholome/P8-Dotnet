using RewardCentral.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RewardCentral;

public class RewardCentral
{
    public int GetAttractionRewardPoints(Guid attractionId, Guid userId)
    {
        int randomDelay = RandomNumberGenerator.GetInt32(1, 1001);
        Thread.Sleep(randomDelay);

        int randomInt = RandomNumberGenerator.GetInt32(1, 1001);
        return randomInt;
    }
    
    public async Task<int> GetAttractionRewardPointsAsync(Guid attractionId, Guid userId)
    {
        await Task.Delay(RandomNumberGenerator.GetInt32(1, 1001)); // simulate async I/O
        return RandomNumberGenerator.GetInt32(1, 1001);
    }
}
