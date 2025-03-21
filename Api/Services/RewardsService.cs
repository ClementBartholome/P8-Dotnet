﻿using GpsUtil.Location;
using TourGuide.LibrairiesWrappers.Interfaces;
using TourGuide.Services.Interfaces;
using TourGuide.Users;

namespace TourGuide.Services;

public class RewardsService : IRewardsService
{
    private const double StatuteMilesPerNauticalMile = 1.15077945;
    private readonly int _defaultProximityBuffer = 10;
    private int _proximityBuffer;
    private readonly int _attractionProximityRange = 200;
    private readonly IGpsUtil _gpsUtil;
    private readonly IRewardCentral _rewardsCentral;
    private static int count = 0;

    public RewardsService(IGpsUtil gpsUtil, IRewardCentral rewardCentral)
    {
        _gpsUtil = gpsUtil;
        _rewardsCentral = rewardCentral;
        _proximityBuffer = _defaultProximityBuffer;
    }

    public void SetProximityBuffer(int proximityBuffer)
    {
        _proximityBuffer = proximityBuffer;
    }

    public void SetDefaultProximityBuffer()
    {
        _proximityBuffer = _defaultProximityBuffer;
    }

    /// <summary>
    /// Calculate rewards for a user based on their visited locations
    /// </summary>
    /// <param name="user"></param>
    public void CalculateRewards(User user)
    {
        count++;

        // Create immutable copies of collections to prevent modification during enumeration
        // This avoids the "Collection was modified" exception
        var userLocations = user.VisitedLocations.ToList();
        var attractions = _gpsUtil.GetAttractions();
        
        // Create a HashSet of existing reward attraction names to quickly check for duplicates
        var existingRewards = user.UserRewards.Select(r => r.Attraction.AttractionName).ToHashSet();

        // Temporary collection to store all new rewards before adding them to user
        // This separates the calculation phase from the modification phase
        var newRewards = new List<UserReward>();

        // Process each location-attraction pair to find new rewards
        foreach (var visitedLocation in userLocations)
        {
            foreach (var attraction in attractions)
            {
                // Check if the attraction has not been rewarded yet and is near the visited location
                if (!existingRewards.Contains(attraction.AttractionName) &&
                    NearAttraction(visitedLocation, attraction))
                {
                    newRewards.Add(new UserReward(
                        visitedLocation,
                        attraction,
                        GetRewardPoints(attraction, user)
                    ));

                    // Add to existingRewards to prevent duplicates within the same calculation
                    // This is important when a user has multiple visits near the same attraction
                    existingRewards.Add(attraction.AttractionName);
                }
            }
        }

        // Add all calculated rewards to the user's rewards collection
        // This is done after all calculations to prevent concurrent modification issues
        foreach (var reward in newRewards)
        {
            user.AddUserReward(reward);
        }
    }
    
    public async Task<List<UserReward>> CalculateRewardsParallel(User user, VisitedLocation newLocation, List<Attraction> attractions)
    {
        // Get existing rewards to avoid duplicates
        var existingRewards = user.UserRewards.Select(r => r.Attraction.AttractionName).ToHashSet();
    
        // List to hold all tasks to be executed in parallel
        var tasks = new List<Task<UserReward>>();

        foreach (var attraction in attractions)
        {
            // Check if the attraction has not been rewarded yet and the user is nearby
            if (!existingRewards.Contains(attraction.AttractionName) && 
                NearAttraction(newLocation, attraction))
            {
                // Task.Run launches each reward calculation on a different thread from the thread pool
                // This allows multiple calculations to run concurrently
                tasks.Add(Task.Run(async () => {
                    // GetRewardPointsAsync invokes RewardCentral which contains a random delay up to 1000ms
                    var points = await GetRewardPointsAsync(attraction, user);
                    return new UserReward(newLocation, attraction, points);
                }));
            }
        }

        // Task.WhenAll returns a task that completes when all tasks in the list have completed
        // This allows the method to await the completion of all reward calculations    
        return (await Task.WhenAll(tasks)).ToList();
    }

    private Task<int> GetRewardPointsAsync(Attraction attraction, User user)
    {
        return Task.FromResult(_rewardsCentral.GetAttractionRewardPoints(attraction.AttractionId, user.UserId));
    }

    public bool IsWithinAttractionProximity(Attraction attraction, Locations location)
    {
        Console.WriteLine(GetDistance(attraction, location));
        return GetDistance(attraction, location) <= _attractionProximityRange;
    }

    private bool NearAttraction(VisitedLocation visitedLocation, Attraction attraction)
    {
        return GetDistance(attraction, visitedLocation.Location) <= _proximityBuffer;
    }

    public int GetRewardPoints(Attraction attraction, User user)
    {
        return _rewardsCentral.GetAttractionRewardPoints(attraction.AttractionId, user.UserId);
    }

    public double GetDistance(Locations loc1, Locations loc2)
    {
        double lat1 = Math.PI * loc1.Latitude / 180.0;
        double lon1 = Math.PI * loc1.Longitude / 180.0;
        double lat2 = Math.PI * loc2.Latitude / 180.0;
        double lon2 = Math.PI * loc2.Longitude / 180.0;

        double angle = Math.Acos(Math.Sin(lat1) * Math.Sin(lat2)
                                 + Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon1 - lon2));

        double nauticalMiles = 60.0 * angle * 180.0 / Math.PI;
        return StatuteMilesPerNauticalMile * nauticalMiles;
    }
}