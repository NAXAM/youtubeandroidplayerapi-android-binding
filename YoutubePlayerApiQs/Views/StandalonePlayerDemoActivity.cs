using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Android.Text;
using static Android.Resource;
using Java.Lang;
using Com.Google.Android.Youtube.Player;

//finish
namespace YoutubePlayerApiQs.Views
{
    [Activity]
    public class StandalonePlayerDemoActivity : Activity, View.IOnClickListener
    {
        private static int REQ_START_STANDALONE_PLAYER = 1;
        private static int REQ_RESOLVE_SERVICE_MISSING = 2;

        private static string VIDEO_ID = "cdgQpa1pUUE";
        private static string PLAYLIST_ID = "7E952A67F31C58A3";
        private static List<string> VIDEO_IDS = new List<string> { "cdgQpa1pUUE", "8aCYZ3gXfy8", "zMabEyrtPRg" };

        private Button playVideoButton;
        private Button playPlaylistButton;
        private Button playVideoListButton;

        private EditText startIndexEditText;
        private EditText startTimeEditText;
        private CheckBox autoplayCheckBox;
        private CheckBox lightboxModeCheckBox;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.standalone_player_demo);

            InitInterface();
        }
        public void InitInterface()
        {
            playVideoButton = FindViewById<Button>(Resource.Id.start_video_button);
            playPlaylistButton = FindViewById<Button>(Resource.Id.start_playlist_button);
            playVideoListButton = FindViewById<Button>(Resource.Id.start_video_list_button);
            startIndexEditText = FindViewById<EditText>(Resource.Id.start_index_text);
            startTimeEditText = FindViewById<EditText>(Resource.Id.start_time_text);
            autoplayCheckBox = FindViewById<CheckBox>(Resource.Id.autoplay_checkbox);
            lightboxModeCheckBox = FindViewById<CheckBox>(Resource.Id.lightbox_checkbox);

            playVideoButton.SetOnClickListener(this);
            playPlaylistButton.SetOnClickListener(this);
            playVideoListButton.SetOnClickListener(this);
        }
        public void OnClick(View v)
        {
            int startIndex = 0;
            Int32.TryParse(startIndexEditText.Text.ToString(), out startIndex);
            int startTimeMillis = 0;
            Int32.TryParse(startTimeEditText.Text.ToString(), out startTimeMillis);
            startTimeMillis *= 1000;
            bool autoplay = autoplayCheckBox.Checked;
            bool lightboxMode = lightboxModeCheckBox.Checked;

            Intent intent = null;
            if (v == playVideoButton)
            {
                intent = YouTubeStandalonePlayer.CreateVideoIntent(
                    this, DeveloperKey.DEVELOPER_KEY, VIDEO_ID, startTimeMillis, autoplay, lightboxMode);
            }
            else if (v == playPlaylistButton)
            {
                intent = YouTubeStandalonePlayer.CreatePlaylistIntent(this, DeveloperKey.DEVELOPER_KEY,
                    PLAYLIST_ID, startIndex, startTimeMillis, autoplay, lightboxMode);
            }
            else if (v == playVideoListButton)
            {
                intent = YouTubeStandalonePlayer.CreateVideosIntent(this, DeveloperKey.DEVELOPER_KEY,
                    VIDEO_IDS, startIndex, startTimeMillis, autoplay, lightboxMode);
            }

            if (intent != null)
            {
                if (CanResolveIntent(intent))
                {
                    StartActivityForResult(intent, REQ_START_STANDALONE_PLAYER);
                }
                else
                {
                    // Could not resolve the intent - must need to install or update the YouTube API service.
                    YouTubeInitializationResult.ServiceMissing
                        .GetErrorDialog(this, REQ_RESOLVE_SERVICE_MISSING).Show();
                }
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == REQ_START_STANDALONE_PLAYER && resultCode != Result.Ok)
            {
                YouTubeInitializationResult errorReason =
                    YouTubeStandalonePlayer.GetReturnedInitializationResult(data);
                if (errorReason.IsUserRecoverableError)
                {
                    errorReason.GetErrorDialog(this, 0).Show();
                }
                else
                {
                    string errorMessage =
                        string.Format(GetString(Resource.String.error_player), errorReason.ToString());
                    Toast.MakeText(this, errorMessage, ToastLength.Short).Show();
                }
            }
        }

        private bool CanResolveIntent(Intent intent)
        {
            IList<ResolveInfo> resolveInfo = PackageManager.QueryIntentActivities(intent, 0);
            return resolveInfo != null && !(resolveInfo.Count == 0);
        }

        private int parseInt(string text, int defaultValue)
        {
            if (!TextUtils.IsEmpty(text))
            {
                try
                {
                    int x = 0;
                    Int32.TryParse(text, out x);
                    return x;
                }
                catch (NumberFormatException e)
                {
                    // fall through
                }
            }
            return defaultValue;
        }
    }
}