using GpsUtil.Location;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourGuide.LibrairiesWrappers.Interfaces;
using TourGuide.Services.Interfaces;
using TourGuide.Users;
using TourGuide.Utilities;
using Xunit.Abstractions;

namespace TourGuideTest
{
    public class PerformanceTest : IClassFixture<DependencyFixture>
    {
        /*
         * Note on performance improvements:
         *
         * The number of generated users for high-volume tests can be easily adjusted using this method:
         *
         *_fixture.Initialize(100000); (for example)
         *
         *
         * These tests can be modified to fit new solutions, as long as the performance metrics at the end of the tests remain consistent.
         *
         * These are the performance metrics we aim to achieve:
         *
         * highVolumeTrackLocation: 100,000 users within 15 minutes:
         * Assert.True(TimeSpan.FromMinutes(15).TotalSeconds >= stopWatch.Elapsed.TotalSeconds);
         *
         * highVolumeGetRewards: 100,000 users within 20 minutes:
         * Assert.True(TimeSpan.FromMinutes(20).TotalSeconds >= stopWatch.Elapsed.TotalSeconds);
         */

        private readonly DependencyFixture _fixture;

        private readonly ITestOutputHelper _output;

        public PerformanceTest(DependencyFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task HighVolumeTrackLocation()
        {
            // Adjust the number of users here to test performance
            _fixture.Initialize(100000);

            List<User> allUsers = _fixture.TourGuideService.GetAllUsers();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            
            // Create a list that will store all asynchronous tasks to execute
            var tasks = new List<Task>();
            
            // Semaphor limiting to 1000 the number of simultaneous operations
            // to avoid overloading the system with 100000 requests at the same time
            SemaphoreSlim semaphore = new SemaphoreSlim(1000);
            
            foreach (var user in allUsers)
            {
                // Wait for obtaining a "token" from the semaphore
                // If the 100 tokens are already used, this line blocks the execution until a token is released
                await semaphore.WaitAsync();
                // Creation and addition of a new asynchronous task to the list
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        // Async call to the location tracking method for the current user
                        await _fixture.TourGuideService.TrackUserLocationAsync(user);
                    }
                    finally
                    {
                        // Release the semaphore token once the processing is complete
                        // The finally block ensures execution even in case of error
                        semaphore.Release();
                    }
                }));
            }
            
            // Wait for all tasks to complete before continuing
            await Task.WhenAll(tasks);

            stopWatch.Stop();
            _fixture.TourGuideService.Tracker.StopTracking();

            _output.WriteLine($"highVolumeTrackLocation: Time Elapsed: {stopWatch.Elapsed.TotalSeconds} seconds.");

            Assert.True(TimeSpan.FromMinutes(15).TotalSeconds >= stopWatch.Elapsed.TotalSeconds);
        }

        [Fact]
        public void HighVolumeGetRewards()
        {
            // Adjust the number of users here to test performance
            _fixture.Initialize(10);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Attraction attraction = _fixture.GpsUtil.GetAttractions()[0];
            List<User> allUsers = _fixture.TourGuideService.GetAllUsers();
            allUsers.ForEach(u => u.AddToVisitedLocations(new VisitedLocation(u.UserId, attraction, DateTime.Now)));

            allUsers.ForEach(u => _fixture.RewardsService.CalculateRewards(u));

            foreach (var user in allUsers)
            {
                Assert.True(user.UserRewards.Count > 0);
            }

            stopWatch.Stop();
            _fixture.TourGuideService.Tracker.StopTracking();

            _output.WriteLine($"highVolumeGetRewards: Time Elapsed: {stopWatch.Elapsed.TotalSeconds} seconds.");
            Assert.True(TimeSpan.FromMinutes(20).TotalSeconds >= stopWatch.Elapsed.TotalSeconds);
        }
    }
}