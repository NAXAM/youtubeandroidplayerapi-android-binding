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
using Com.Google.Android.Youtube.Player;

//finish
namespace YoutubePlayerApiQs.Views
{
    [Activity]
    public class PlayerViewDemoActivity : YouTubeFailureRecoveryActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.playerview_demo);
            YouTubePlayerView youTubeView = FindViewById<YouTubePlayerView>(Resource.Id.youtube_view);
            youTubeView.Initialize(DeveloperKey.DEVELOPER_KEY, this);
        }

        public override void OnInitializationSuccess(IYouTubePlayerProvider provider, IYouTubePlayer player, bool wasRestored)
        {
            if (!wasRestored)
            {
                player.CueVideo("wKJ9KzGQq0w");
            }
        }
        protected override IYouTubePlayerProvider GetYouTubePlayerProvider()
        {
            return FindViewById<YouTubePlayerView>(Resource.Id.youtube_view);
        }
    }
}