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
    public class FragmentDemoActivity : YouTubeFailureRecoveryActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.fragments_demo);

            YouTubePlayerFragment youTubePlayerFragment = FragmentManager.FindFragmentById<YouTubePlayerFragment>(Resource.Id.youtube_fragment);
            youTubePlayerFragment.Initialize(DeveloperKey.DEVELOPER_KEY, this);
        }

        public override void OnInitializationSuccess(IYouTubePlayerProvider provider, IYouTubePlayer player, bool wasRestored)
        {
            if (!wasRestored)
            {
                player.CueVideo("nCgQDjiotG0");
            }
        }
        protected override IYouTubePlayerProvider GetYouTubePlayerProvider()
        {
            return FragmentManager.FindFragmentById<YouTubePlayerFragment>(Resource.Id.youtube_fragment);
        }
    }
}