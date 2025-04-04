using GpsUtil.Location;
using Microsoft.Extensions.Caching.Memory;
using TourGuide.LibrairiesWrappers.Interfaces;

namespace TourGuide.LibrairiesWrappers;

public class GpsUtilWrapper : IGpsUtil
{
    private readonly GpsUtil.GpsUtil _gpsUtil;

    public GpsUtilWrapper(IMemoryCache memoryCache)
    {
        _gpsUtil = new GpsUtil.GpsUtil(memoryCache);
    }

    public VisitedLocation GetUserLocation(Guid userId)
    {
        return _gpsUtil.GetUserLocation(userId);
    }

    public List<Attraction> GetAttractions()
    {
        return _gpsUtil.GetAttractions();
    }
    
    public async Task<VisitedLocation> GetUserLocationAsync(Guid userId)
    {
        return await _gpsUtil.GetUserLocationAsync(userId);
    }
}