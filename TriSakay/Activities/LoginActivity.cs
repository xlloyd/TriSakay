using Android.App;
using Android.OS;
using Android.Widget;
using Firebase.Auth;
using Firebase;
using Android.Support.V7.App;
using Android.Support.Design.Widget;
using TriSakay.EventListeners;
using Android.Views;
using System;
using Android.Support.V4.Widget;
using Snackbar = Android.Support.Design.Widget.Snackbar;

namespace TriSakay.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/TrisakayTheme", MainLauncher = false)]
    public class LoginActivity : AppCompatActivity
    {
        TextInputLayout emailText;
        TextInputLayout passwordText;
        TextView clickToRegisterText;
        Button loginButton;
        CoordinatorLayout rootView;
        FirebaseAuth Auth;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.login);

            emailText = FindViewById<TextInputLayout>(Resource.Id.emailText);
            passwordText = FindViewById<TextInputLayout>(Resource.Id.passwordText);
            rootView = FindViewById<CoordinatorLayout>(Resource.Id.rootView);
            loginButton = FindViewById<Button>(Resource.Id.loginButton);
            clickToRegisterText = FindViewById<TextView>(Resource.Id.clickToRegisterText);

            clickToRegisterText.Click += ClickToRegisterText_Click;
            loginButton.Click += LoginButton_Click;

            InitializeFirebase();
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            string email, password;
            email = emailText.EditText.Text;
            password = passwordText.EditText.Text;

            if (!email.Contains("@"))
            {
                Snackbar.Make(rootView, "Please Provide A Valid Email", Snackbar.LengthShort).Show();
                return;
            }
            else if (password.Length < 8)
            {
                Snackbar.Make(rootView, "Please Provide A Valid Password", Snackbar.LengthShort).Show();
                return;
            }

            TaskCompletionListener taskCompletionListener = new TaskCompletionListener();
            taskCompletionListener.Success += TaskCompletionListener_Success;
            taskCompletionListener.Failure += TaskCompletionListener_Failure;

            Auth.SignInWithEmailAndPassword(email, password)
                .AddOnSuccessListener(taskCompletionListener)
                .AddOnFailureListener(taskCompletionListener);
        }

        private void TaskCompletionListener_Failure(object sender, EventArgs e)
        {
            Snackbar.Make(rootView, "Login Failed", Snackbar.LengthShort).Show();
        }

        private void TaskCompletionListener_Success(object sender, EventArgs e)
        {
            StartActivity(typeof(MainActivity));
        }

        private void ClickToRegisterText_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(RegistrationActivity));
        }

        void InitializeFirebase()
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
                Auth = FirebaseAuth.Instance;
            }
            else
            {
                Auth = FirebaseAuth.Instance;
            }
        }
    }
}
