using Android.App;
using Android.Content;
using Android.OS;
using System;
using Android.Support.V7.App;
using Android.Content.PM;
using System.Threading.Tasks;


namespace TriSakay.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = false, ConfigurationChanges = ConfigChanges.ScreenSize)]
    public class SplashActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

        }

        protected override async void OnResume()
        {
            base.OnResume();
            await SimulateStartup();
        }



        async Task SimulateStartup()
        {
            await Task.Delay(TimeSpan.FromSeconds(2.5));
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }
    }
}

