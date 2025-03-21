﻿using GpsUtil.Location;
using TourGuide.LibrairiesWrappers.Interfaces;

namespace TourGuide.LibrairiesWrappers;

public class GpsUtilWrapper : IGpsUtil
{
    private readonly GpsUtil.GpsUtil _gpsUtil = new();

    public VisitedLocation GetUserLocation(Guid userId)
    {
        return _gpsUtil.GetUserLocation(userId);
    }

    public List<Attraction> GetAttractions()
    {
        return _gpsUtil.GetAttractions();
    }
    
    public Task<List<Attraction>> GetAttractionsAsync()
    {
        return _gpsUtil.GetAttractionsAsync();
    }
    
    public async Task<VisitedLocation> GetUserLocationAsync(Guid userId)
    {
        return await _gpsUtil.GetUserLocationAsync(userId);
    }
}
