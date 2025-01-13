using GpsUtil.Location;
using Microsoft.AspNetCore.Mvc;
using TourGuide.Services.Interfaces;
using TourGuide.Users;
using TripPricer;

namespace TourGuide.Controllers;

[ApiController]
[Route("[controller]")]
public class TourGuideController : ControllerBase
{
    private readonly ITourGuideService _tourGuideService;
    private readonly IRewardsService _rewardsService;

    public TourGuideController(ITourGuideService tourGuideService, IRewardsService rewardsService)
    {
        _tourGuideService = tourGuideService;
        _rewardsService = rewardsService;
    }

    [HttpGet("getLocation")]
    public ActionResult<VisitedLocation> GetLocation([FromQuery] string userName)
    {
        var location = _tourGuideService.GetUserLocation(GetUser(userName));
        return Ok(location);
    }

    // TODO: Change this method to no longer return a List of Attractions.
    // Instead: Get the closest five tourist attractions to the user - no matter how far away they are.
    // Return a new JSON object that contains:
    // Name of Tourist attraction, 
    // Tourist attractions lat/long, 
    // The user's location lat/long, 
    // The distance in miles between the user's location and each of the attractions.
    // The reward points for visiting each Attraction.
    //    Note: Attraction reward points can be gathered from RewardsCentral
    [HttpGet("getNearbyAttractions")]
    public ActionResult<List<Attraction>> GetNearbyAttractions([FromQuery] string userName)
    {
        var user = GetUser(userName);

        var userLastVisitedLocation = _tourGuideService.GetUserLocation(GetUser(userName));
        var closestAttractions = _tourGuideService.GetNearByAttractions(userLastVisitedLocation)
            .Select(attraction => new
            {
                Attraction = attraction,
                // Distance between the attraction and the user's location
                Distance = _rewardsService.GetDistance(attraction, userLastVisitedLocation.Location),
                // Retrieve the reward points for visiting the attraction
                RewardPoints = _rewardsService.GetRewardPoints(attraction, user)
            });

        // JSON object with the requested information
        var result = closestAttractions
            .Select(attractionInfo => new
            {
                attractionInfo.Attraction.AttractionName,
                AttractionLocation = new
                {
                    attractionInfo.Attraction.Latitude, attractionInfo.Attraction.Longitude
                },
                UserLocation = new
                {
                    userLastVisitedLocation.Location.Latitude, userLastVisitedLocation.Location.Longitude
                },
                attractionInfo.Distance,
                attractionInfo.RewardPoints
            });

        return Ok(result);
    }

    [HttpGet("getRewards")]
    public ActionResult<List<UserReward>> GetRewards([FromQuery] string userName)
    {
        var rewards = _tourGuideService.GetUserRewards(GetUser(userName));
        return Ok(rewards);
    }

    [HttpGet("getTripDeals")]
    public ActionResult<List<Provider>> GetTripDeals([FromQuery] string userName)
    {
        var deals = _tourGuideService.GetTripDeals(GetUser(userName));
        return Ok(deals);
    }

    private User GetUser(string userName)
    {
        return _tourGuideService.GetUser(userName);
    }
}