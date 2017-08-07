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
using static Android.Widget.AdapterView;
using YoutubePlayerApiQs.Views.Adapter;
using Com.Google.Android.Youtube.Player;

//finish
namespace YoutubePlayerApiQs.Views
{
    [Activity]
    public class IntentsDemoActivity : Activity, IOnItemClickListener
    {
        // This is the value of Intent.EXTRA_LOCAL_ONLY for API level 11 and above.
        private static string EXTRA_LOCAL_ONLY = "android.intent.extra.LOCAL_ONLY";
        private static string VIDEO_ID = "-Uwjt32NvVA";
        private static string PLAYLIST_ID = "PLF3DFB800F05F551A";
        private static string USER_ID = "Google";
        private static string CHANNEL_ID = "UCVHFbqXqoYvEWM1Ddxl0QDg";
        private static int SELECT_VIDEO_REQUEST = 1000;

        private List<DemoListViewItem> intentItems;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.intents_demo);

            InitInterface();
        }

        public void InitInterface()
        {
            intentItems = new List<DemoListViewItem>();
            intentItems.Add(new IntentItem() { Title = "Play Video", Type = IntentType.PLAY_VIDEO });

            intentItems.Add(new IntentItem() { Title = "Open Playlist", Type = IntentType.OPEN_PLAYLIST });
            intentItems.Add(new IntentItem() { Title = "Play Playlist", Type = IntentType.PLAY_PLAYLIST });
            intentItems.Add(new IntentItem() { Title = "Open User", Type = IntentType.OPEN_USER });
            intentItems.Add(new IntentItem() { Title = "Open Channel", Type = IntentType.OPEN_CHANNEL });
            intentItems.Add(new IntentItem() { Title = "Open Search Results", Type = IntentType.OPEN_SEARCH });
            intentItems.Add(new IntentItem() { Title = "Upload Video", Type = IntentType.UPLOAD_VIDEO });

            ListView listView = FindViewById<ListView>(Resource.Id.intent_list);
            DemoArrayAdapter adapter = new DemoArrayAdapter(this, Resource.Layout.list_item, intentItems);
            listView.Adapter = adapter;
            listView.OnItemClickListener = this;

            TextView youTubeVersionText = FindViewById<TextView>(Resource.Id.youtube_version_text);
            string version = YouTubeIntents.GetInstalledYouTubeVersionName(this);
            if (version != null)
            {
                string text = string.Format(GetString(Resource.String.youtube_currently_installed), version);
                youTubeVersionText.Text = text;
            }
            else
            {
                youTubeVersionText.Text = GetString(Resource.String.youtube_not_installed);
            }
        }
        public bool IsIntentTypeEnabled(IntentType type)
        {
            switch (type)
            {
                case IntentType.PLAY_VIDEO:
                    return YouTubeIntents.CanResolvePlayVideoIntent(this);
                case IntentType.OPEN_PLAYLIST:
                    return YouTubeIntents.CanResolveOpenPlaylistIntent(this);
                case IntentType.PLAY_PLAYLIST:
                    return YouTubeIntents.CanResolvePlayPlaylistIntent(this);
                case IntentType.OPEN_SEARCH:
                    return YouTubeIntents.CanResolveSearchIntent(this);
                case IntentType.OPEN_USER:
                    return YouTubeIntents.CanResolveUserIntent(this);
                case IntentType.OPEN_CHANNEL:
                    return YouTubeIntents.CanResolveChannelIntent(this);
                case IntentType.UPLOAD_VIDEO:
                    return YouTubeIntents.CanResolveUploadIntent(this);
            }
            return false;
        }

        public void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            IntentItem clickedIntentItem = (IntentItem)intentItems[position];

            Intent intent;
            switch (clickedIntentItem.Type)
            {
                case IntentType.PLAY_VIDEO:
                    intent = YouTubeIntents.CreatePlayVideoIntentWithOptions(this, VIDEO_ID, true, false);
                    StartActivity(intent);
                    break;
                case IntentType.OPEN_PLAYLIST:
                    intent = YouTubeIntents.CreateOpenPlaylistIntent(this, PLAYLIST_ID);
                    StartActivity(intent);
                    break;
                case IntentType.PLAY_PLAYLIST:
                    intent = YouTubeIntents.CreatePlayPlaylistIntent(this, PLAYLIST_ID);
                    StartActivity(intent);
                    break;
                case IntentType.OPEN_SEARCH:
                    intent = YouTubeIntents.CreateSearchIntent(this, USER_ID);
                    StartActivity(intent);
                    break;
                case IntentType.OPEN_USER:
                    intent = YouTubeIntents.CreateUserIntent(this, USER_ID);
                    StartActivity(intent);
                    break;
                case IntentType.OPEN_CHANNEL:
                    intent = YouTubeIntents.CreateChannelIntent(this, CHANNEL_ID);
                    StartActivity(intent);
                    break;
                case IntentType.UPLOAD_VIDEO:
                    // This will load a picker view in the users' gallery.
                    // The upload activity is started in the function onActivityResult.
                    intent = new Intent(Intent.ActionPick, null).SetType("video/*");
                    intent.PutExtra(EXTRA_LOCAL_ONLY, true);
                    StartActivityForResult(intent, SELECT_VIDEO_REQUEST);
                    break;
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case 1000:
                        Intent intent = YouTubeIntents.CreateUploadIntent(this, data.Data);
                        StartActivity(intent);
                        break;
                }
            }
            base.OnActivityResult(requestCode, resultCode, data);
        }

        public enum IntentType
        {
            PLAY_VIDEO,
            OPEN_PLAYLIST,
            PLAY_PLAYLIST,
            OPEN_USER,
            OPEN_CHANNEL,
            OPEN_SEARCH,
            UPLOAD_VIDEO
        }
        private class IntentItem : DemoListViewItem
        {

            public string Title { set; get; }
            public IntentType Type { set; get; }
            public Action IsEnabledAction { get; set; }


            public string GetDisabledText()
            {
                return "This intent is not supported with the currently installed version of YouTube.";
            }

            public string GetTitle()
            {
                return Title;
            }

            public bool IsEnabled()
            {
                return true;
                //IsIntentTypeEnabled(type);
            }

        }
    }
}