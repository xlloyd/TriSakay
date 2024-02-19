using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using System;
using Firebase.Auth;
using Firebase.Database;
using Firebase;
using Android.Support.Design.Widget;
using TriSakay.EventListeners;
using Java.Util;

namespace TriSakay.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/TrisakayTheme", MainLauncher = false)]
    public class RegistrationActivity : AppCompatActivity
    {
        TextInputLayout fullNameText;
        TextInputLayout phoneText;
        TextInputLayout emailText;
        TextInputLayout passwordText;
        Button registerButton;
        CoordinatorLayout rootView;
        TextView clickToLoginText;

        FirebaseAuth Auth;
        FirebaseDatabase database;

        TaskCompletionListener TaskCompletionListener = new TaskCompletionListener();

        string fullname, phone, email, password;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.register);

            InitializeFirebase();
            Auth = FirebaseAuth.Instance;
            ConnectControl();

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
                database = FirebaseDatabase.GetInstance(app);
            }
            else
            {
                database = FirebaseDatabase.GetInstance(app);
            }


        }
        void ConnectControl()
        {
            fullNameText = (TextInputLayout)FindViewById(Resource.Id.fullNameText);
            phoneText = (TextInputLayout)FindViewById(Resource.Id.phoneText);
            emailText = (TextInputLayout)FindViewById(Resource.Id.emailText);
            passwordText = (TextInputLayout)FindViewById(Resource.Id.passwordText);
            rootView = (CoordinatorLayout)FindViewById(Resource.Id.rootView);
            registerButton = (Button)FindViewById(Resource.Id.registerButton);
            clickToLoginText = (TextView)FindViewById(Resource.Id.clickToLogin);

            clickToLoginText.Click += ClickToLoginText_Click;
            registerButton.Click += RegisterButton_Click;
        }

        private void ClickToLoginText_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(LoginActivity));
            Finish();
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {


            fullname = fullNameText.EditText.Text;
            phone = phoneText.EditText.Text;
            email = emailText.EditText.Text;
            password = passwordText.EditText.Text;

            if (fullname.Length < 3)
            {
                Snackbar.Make(rootView, "Please Enter a Valid Name", Snackbar.LengthShort).Show();
                return;
            }
            else if (phone.Length < 11)
            {
                Snackbar.Make(rootView, "Please Enter a Valid Number", Snackbar.LengthShort).Show();
                return;
            }
            else if (!email.Contains("@"))
            {
                Snackbar.Make(rootView, "Please Enter a Valid Email", Snackbar.LengthShort).Show();
                return;
            }
            else if (password.Length < 8)
            {
                Snackbar.Make(rootView, "Please Enter a Password upto 8 characters", Snackbar.LengthShort).Show();
                return;
            }

            RegisterUser(fullname, phone, email, password);
        }

        void RegisterUser(string name, string phone, string email, string password)
        {
            TaskCompletionListener.Success += TaskCompletionListener_Success;
            TaskCompletionListener.Failure += TaskCompletionListener_Failure;
            Auth.CreateUserWithEmailAndPassword(email, password)
                .AddOnSuccessListener(this, TaskCompletionListener)
                .AddOnFailureListener(this, TaskCompletionListener);
        }

        private void TaskCompletionListener_Failure(object sender, EventArgs e)
        {
            Snackbar.Make(rootView, "User Registration Was Unsuccessfull", Snackbar.LengthShort).Show();
        }

        private void TaskCompletionListener_Success(object sender, EventArgs e)
        {
            Snackbar.Make(rootView, "User Registration Was Successfull", Snackbar.LengthShort).Show();

            HashMap userMap = new HashMap();
            userMap.Put("email", email);
            userMap.Put("phone", phone);
            userMap.Put("fullname", fullname);

            DatabaseReference userReference = database.GetReference("users/" + Auth.CurrentUser.Uid);
            userReference.SetValue(userMap);

        }
    }
}