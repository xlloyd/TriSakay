using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Firebase;
using Firebase.Database;
using Android.Views;
using Android.Gms.Maps;
using Android.Support.V4.App;
using Android.Gms.Maps.Model;
using Java.Interop;
using Android;
using Android.Content.PM;
using Android.Gms.Location;
using System;
using TriSakay.Helpers;
using Android.Content;
using Android.Gms.Location.Places.UI;
using Android.Gms.Location.Places;
using Google.Places;
using System.Collections.Generic;



namespace TriSakay
{
    [Activity(Label = "TriSakay", Icon = "@mipmap/icon", Theme = "@style/TrisakayTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback
    {
        FirebaseDatabase database;
        Android.Support.V7.Widget.Toolbar mainToolbar;
        Android.Support.V4.Widget.DrawerLayout drawerLayout;

        //TextViews
        TextView pickupLocationText;
        TextView destinationText;

        //Layouts
        RelativeLayout layoutPickup;
        RelativeLayout destinationPickUp;

        GoogleMap mainMap;

        readonly string[] permissionGroupLocation = { Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation };
        const int requestLocationId = 0;

        LocationRequest mLocationRequest;
        FusedLocationProviderClient locationClient;
        Android.Locations.Location mLastLocation;
        LocationCallbackHelper mLocationCallback;

        private const int DebounceInterval = 300; // milliseconds
        private Handler debounceHandler;


        static int UPDATE_INTERVAL = 5; // 5 SECONDS
        static int FASTEST_INTERVAL = 5;
        static int DISPLACEMENT = 3; // METERS

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            if (!PlacesApi.IsInitialized)
            {
                PlacesApi.Initialize(this, "AIzaSyDpvNVJByey1tWSz1oTev6iWUt7UHmdeBY");
            }

            ConnectControl();

            SupportMapFragment mapFragment = SupportFragmentManager.FindFragmentById(Resource.Id.map).JavaCast<SupportMapFragment>();
            mapFragment.GetMapAsync(this);

            CheckLocationPermission();
            CreateLocationRequest();
            GetMyLocation();
            StartLocationUpdates();

            debounceHandler = new Handler(Looper.MainLooper);
        }

        void ConnectControl()
        {
            // Initialize views and layout
            drawerLayout = FindViewById<Android.Support.V4.Widget.DrawerLayout>(Resource.Id.drawerLayout);
            mainToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.mainToolbar);
            SetSupportActionBar(mainToolbar);
            SupportActionBar.Title = "";
            SupportActionBar.SetHomeAsUpIndicator(Resource.Mipmap.ic_menu_action);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            pickupLocationText = FindViewById<TextView>(Resource.Id.pickupLocationText);
            destinationText = FindViewById<TextView>(Resource.Id.destinationText);

            layoutPickup = FindViewById<RelativeLayout>(Resource.Id.layoutPickUp);
            destinationPickUp = FindViewById<RelativeLayout>(Resource.Id.destinationPickUp);

            // Set click listeners
            layoutPickup.Click += LayoutPickup_Click;
            destinationPickUp.Click += DestinationPickUp_Click;
        }

        void LayoutPickup_Click(object sender, EventArgs e)
        {
            debounceHandler.RemoveCallbacksAndMessages(null);
            debounceHandler.PostDelayed(() =>
            {
                LaunchPlaceAutocompleteActivity(1);
            }, DebounceInterval);

        }

        void DestinationPickUp_Click(object sender, EventArgs e)
        {
            debounceHandler.RemoveCallbacksAndMessages(null);
            debounceHandler.PostDelayed(() =>
            {
                LaunchPlaceAutocompleteActivity(2);
            }, DebounceInterval);
        }

        void LaunchPlaceAutocompleteActivity(int requestCode)
        {

            List<Google.Places.Place.Field> fields = new List<Google.Places.Place.Field>
            {
                Google.Places.Place.Field.Id,
                Google.Places.Place.Field.Name,
                Google.Places.Place.Field.LatLng,
                Google.Places.Place.Field.Address
            };

            Intent intent = new Autocomplete.IntentBuilder(AutocompleteActivityMode.Overlay, fields)
                .SetCountry("PH")
                .Build(this);

            StartActivityForResult(intent, requestCode);
        }


        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    drawerLayout.OpenDrawer((int)GravityFlags.Left);
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        void Initializeddatabase()
        {
            var app = FirebaseApp.InitializeApp(this);

            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetApplicationId("trisakay-fcdea")
                    .SetApiKey("AIzaSyDLyoirrQJNFBSWsm-f_FXpSsZtfRYdUGw")
                    .SetDatabaseUrl("https://trisakay-fcdea-default-rtdb.asia-southeast1.firebasedatabase.app")
                    .SetStorageBucket("trisakay-fcdea.appspot.com")
                    .Build();

                app = FirebaseApp.InitializeApp(this, options);
                database = FirebaseDatabase.GetInstance(app);
            }
            else
            {
                database = FirebaseDatabase.GetInstance(app);
            }

            DatabaseReference dbref = database.GetReference("UserSupport");
            dbref.SetValue("Ticket");

            Toast.MakeText(this, "Completed", ToastLength.Short).Show();
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            try
            {
                bool success = googleMap.SetMapStyle(MapStyleOptions.LoadRawResourceStyle(this, Resource.Raw.mapstyle));
            }
            catch
            {
                // Handle exception
            }

            mainMap = googleMap;
        }


        bool CheckLocationPermission()
        {
            bool permissionGranted = false;

            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted &&
                ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                permissionGranted = false;
                RequestPermissions(permissionGroupLocation, requestLocationId);
            }
            else
            {
                permissionGranted = true;
            }
            return permissionGranted;
        }

        void CreateLocationRequest()
        {
            mLocationRequest = new LocationRequest();
            mLocationRequest.SetInterval(UPDATE_INTERVAL);
            mLocationRequest.SetFastestInterval(FASTEST_INTERVAL);
            mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            mLocationRequest.SetSmallestDisplacement(DISPLACEMENT);
            locationClient = new FusedLocationProviderClient(this);

            mLocationCallback = new LocationCallbackHelper();
            mLocationCallback.MyLocation += MLocationCallback_MyLocation;

        }

        void MLocationCallback_MyLocation(object sender, LocationCallbackHelper.OnlocationCaptureEventArgs e)
        {
            mLastLocation = e.Location;
            LatLng myposition = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);
            mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 18));
        }

        void StartLocationUpdates()
        {
            if (CheckLocationPermission())
            {
                locationClient.RequestLocationUpdates(mLocationRequest, mLocationCallback, null);
            }
        }

        void StopLocationUpdates()
        {
            if (locationClient != null && mLocationCallback != null)
            {
                locationClient.RemoveLocationUpdates(mLocationCallback);

            }
        }

        async void GetMyLocation()
        {
            if (!CheckLocationPermission())
            {
                return;
            }

            try
            {
                mLastLocation = await locationClient.GetLastLocationAsync();

                if (mLastLocation != null)
                {
                    LatLng myposition = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);
                    mainMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 18));
                }
                else
                {
                    // Handle case where location is null
                    Toast.MakeText(this, "Location not available", ToastLength.Short).Show();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Toast.MakeText(this, "Error getting location: " + ex.Message, ToastLength.Short).Show();
            }
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [Android.Runtime.GeneratedEnum] Permission[] grantResults)
        {
            if (grantResults[0] == (int)Permission.Granted)
            {
                Toast.MakeText(this, "Permission was Granted", ToastLength.Short).Show();
            }
            else
            {
                Toast.MakeText(this, "Permission was Denied", ToastLength.Short).Show();
            }
        }
        protected override void OnActivityResult(int requestCode, [Android.Runtime.GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                var place = Autocomplete.GetPlaceFromIntent(data);

                if (requestCode == 1)
                {
                    pickupLocationText.Text = $"Address: {place.Address}, Latitude: {place.LatLng.Latitude}, Longitude: {place.LatLng.Longitude}";
                    mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(place.LatLng, 15));
                }
                else if (requestCode == 2)
                {
                    destinationText.Text = $"Address: {place.Address}, Latitude: {place.LatLng.Latitude}, Longitude: {place.LatLng.Longitude}";
                    mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(place.LatLng, 15));
                }
            }
        }



    }
}
