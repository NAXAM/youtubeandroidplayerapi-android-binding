using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Youtube.Player;
using Android.Content.Res;
using Orientation = Android.Content.Res.Orientation;
//finish
namespace YoutubePlayerApiQs.Views
{
    [Activity]
    public class FullscreenDemoActivity : YouTubeFailureRecoveryActivity, View.IOnClickListener,
    CompoundButton.IOnCheckedChangeListener, IYouTubePlayerOnFullscreenListener
    {
        private static int PORTRAIT_ORIENTATION = Build.VERSION.SdkInt < BuildVersionCodes.Gingerbread
              ? (int)Android.Content.Res.Orientation.Portrait
              : (int)Android.Content.Res.Orientation.Undefined;

        private LinearLayout baseLayout;
        private YouTubePlayerView playerView;
        private IYouTubePlayer player;
        private Button fullscreenButton;
        private CompoundButton checkbox;
        private View otherViews;

        private bool fullscreen;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.fullscreen_demo);

            baseLayout = FindViewById<LinearLayout>(Resource.Id.layout);
            playerView = FindViewById<YouTubePlayerView>(Resource.Id.player);
            fullscreenButton = FindViewById<Button>(Resource.Id.fullscreen_button);
            checkbox = FindViewById<CompoundButton>(Resource.Id.landscape_fullscreen_checkbox);
            otherViews = FindViewById(Resource.Id.other_views);

            checkbox.SetOnCheckedChangeListener(this);
            // You can use your own button to switch to fullscreen too
            fullscreenButton.SetOnClickListener(this);

            playerView.Initialize(DeveloperKey.DEVELOPER_KEY, this);

            DoLayout();
        }

        public void OnCheckedChanged(CompoundButton buttonView, bool isChecked)
        {
            int controlFlags = player.FullscreenControlFlags;
            if (isChecked)
            {
                // If you use the FULLSCREEN_FLAG_ALWAYS_FULLSCREEN_IN_LANDSCAPE, your activity's normal UI
                // should never be laid out in landscape mode (since the video will be fullscreen whenever the
                // activity is in landscape orientation). Therefore you should set the activity's requested
                // orientation to portrait. Typically you would do this in your AndroidManifest.xml, we do it
                // programmatically here since this activity demos fullscreen behavior both with and without
                // this flag).
                RequestedOrientation = ScreenOrientation.Portrait;
                controlFlags |= YouTubePlayer.FullscreenFlagAlwaysFullscreenInLandscape;
            }
            else
            {
                RequestedOrientation = ScreenOrientation.Sensor;
                controlFlags &= ~YouTubePlayer.FullscreenFlagAlwaysFullscreenInLandscape;
            }
            player.FullscreenControlFlags = controlFlags;
        }

        public void OnClick(View v)
        {
            player.SetFullscreen(!fullscreen);
        }

        protected override IYouTubePlayerProvider GetYouTubePlayerProvider()
        {
            return playerView;
        }

        private void DoLayout()
        {
            ViewGroup.LayoutParams playerParams = playerView.LayoutParameters;
            if (fullscreen)
            {
                // When in fullscreen, the visibility of all other views than the player should be set to
                // GONE and the player should be laid out across the whole screen.
                playerParams.Width = ViewGroup.LayoutParams.MatchParent;
                playerParams.Height = ViewGroup.LayoutParams.MatchParent;

                otherViews.Visibility = ViewStates.Gone;
            }
            else
            {
                // This layout is up to you - this is just a simple example (vertically stacked boxes in
                // portrait, horizontally stacked in landscape).
                otherViews.Visibility=ViewStates.Visible;
                ViewGroup.LayoutParams otherViewsParams = otherViews.LayoutParameters;
                if (Resources.Configuration.Orientation == Orientation.Landscape)
                {
                    playerParams.Width = otherViewsParams.Width = 0;
                    playerParams.Height = ViewGroup.LayoutParams.WrapContent;
                    otherViewsParams.Height = ViewGroup.LayoutParams.WrapContent;
                    //playerParams.Weight = 1;
                    baseLayout.Orientation=Android.Widget.Orientation.Horizontal;
                }
                else
                {
                    playerParams.Width = otherViewsParams.Width = ViewGroup.LayoutParams.MatchParent;
                    playerParams.Height = ViewGroup.LayoutParams.MatchParent;
                    //playerParams.Weight = 0;
                    otherViewsParams.Height = 0;
                    baseLayout.Orientation=Android.Widget.Orientation.Vertical;
                }
                SetControlsEnabled();
            }
        }

        private void SetControlsEnabled()
        {
            checkbox.Enabled = (player != null
                && Resources.Configuration.Orientation == Android.Content.Res.Orientation.Portrait);
            fullscreenButton.Enabled = (player != null);
        }
        public void OnFullscreen(bool p0)
        {
            fullscreen = p0;
            DoLayout();
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            DoLayout();
        }
    }
}