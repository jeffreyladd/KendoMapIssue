using Bogus;
using KendoMapExample.Shared;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace KendoMapExample.Server.Hubs
{
    public class MarkerHub : Hub
    {
        readonly CacheService _cache;

        public MarkerHub(CacheService cache)
        {
            _cache = cache;
        }

        public async Task GetData()
        {
            await Clients.Caller.SendAsync("InitialData", this._cache.GetData());
        }
    }

    public class CacheService
    {
        ConcurrentDictionary<string, MyMarkers> _markers = new ConcurrentDictionary<string, MyMarkers>();
        IHubContext<MarkerHub> _hub;

        public CacheService(IHubContext<MarkerHub> hub)
        {
            this._hub = hub;
        }

        public MyMarkers[] GetData()
        {
            return this._markers.Values.ToArray();
        }

        public async Task NewMarker()
        {
            var f = new Faker().Address;
            var name = f.City();

            this._markers.TryAdd(name, new MyMarkers { Name = name, LatLng = new double[] { f.Latitude(), f.Longitude() }});

            await this._hub.Clients.All.SendAsync("UpdateMarkers", this._markers.Values);
        }
    }


    public class Worker : BackgroundService
    {
        readonly CacheService _cache;

        public Worker(CacheService cache)
        {
            _cache = cache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await this._cache.NewMarker();
                await Task.Delay(5000);
            }
        }
    }
}
