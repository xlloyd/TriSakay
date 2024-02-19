using Android.Gms.Location;
using Android.Util;
using System;

namespace TriSakay.Helpers
{
    public class LocationCallbackHelper : LocationCallback
    {
        public EventHandler<OnlocationCaptureEventArgs> MyLocation;

        public class OnlocationCaptureEventArgs : EventArgs
        {
            public Android.Locations.Location Location { get; set; }
        }
        public override void OnLocationAvailability(LocationAvailability locationAvailability)
        {
            Log.Debug("TriSakay", "IsLocationAvailable: {0}", locationAvailability.IsLocationAvailable);
        }
        public override void OnLocationResult(LocationResult result)
        {
            if (result.Locations.Count != 0)
            {
                MyLocation?.Invoke(this, new OnlocationCaptureEventArgs { Location = result.Locations[0] });
            }
        }
    }
}