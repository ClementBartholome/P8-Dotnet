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
        Task.Delay(10);
        
        int randomInt = new Random().Next(1, 1000);
        return randomInt;
    }
}
