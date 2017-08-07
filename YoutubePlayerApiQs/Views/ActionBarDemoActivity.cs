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
using Android.Util;
using Com.Google.Android.Youtube.Player;
using Android.Graphics.Drawables;
namespace YoutubePlayerApiQs.Views
{
    [Activity]
    public class ActionBarDemoActivity : YouTubeFailureRecoveryActivity, IYouTubePlayerOnFullscreenListener
    {
        private ActionBarPaddedFrameLayout viewContainer;
        private YouTubePlayerFragment playerFragment;
        private View tutorialTextView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.action_bar_demo);

            viewContainer = FindViewById<ActionBarPaddedFrameLayout>(Resource.Id.view_container);
            playerFragment = (YouTubePlayerFragment)FragmentManager.FindFragmentById(Resource.Id.player_fragment);
            tutorialTextView = FindViewById<TextView>(Resource.Id.tutorial_text);
            playerFragment.Initialize(DeveloperKey.DEVELOPER_KEY, this);
            viewContainer.SetActionBar(ActionBar);

            // Action bar background is transparent by default.
            ActionBar.SetBackgroundDrawable(new ColorDrawable(Android.Graphics.Color.ParseColor("0xAA000000")));
        }

        public override void OnInitializationSuccess(IYouTubePlayerProvider provider, IYouTubePlayer player, bool wasRestored)
        {
            player.AddFullscreenControlFlag(YouTubePlayer.FullscreenFlagCustomLayout);
            player.SetOnFullscreenListener(this);

            if (!wasRestored)
            {
                player.CueVideo("9c6W4CCU9M4");
            }
        }


        public void OnFullscreen(bool fullscreen)
        {
            viewContainer.SetEnablePadding(!fullscreen);

            ViewGroup.LayoutParams playerParams = playerFragment.View.LayoutParameters;
            if (fullscreen)
            {
                tutorialTextView.Visibility = ViewStates.Gone;
                playerParams.Width = ViewGroup.LayoutParams.MatchParent;
                playerParams.Height = ViewGroup.LayoutParams.MatchParent;
            }
            else
            {
                tutorialTextView.Visibility = ViewStates.Visible;
                playerParams.Width = 0;
                playerParams.Height = ViewGroup.LayoutParams.WrapContent;
            }
        }

        protected override IYouTubePlayerProvider GetYouTubePlayerProvider()
        {
            return FragmentManager.FindFragmentById<YouTubePlayerFragment>(Resource.Id.player_fragment);
        }
    }

    public class ActionBarPaddedFrameLayout : FrameLayout
    {

        private ActionBar actionBar;
        private bool paddingEnabled;

        public ActionBarPaddedFrameLayout(Context context) : base(context, null)
        {
        }

        public ActionBarPaddedFrameLayout(Context context, IAttributeSet attrs) : base(context, attrs, 0)
        {
        }

        public ActionBarPaddedFrameLayout(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            paddingEnabled = true;
        }

        public void SetActionBar(ActionBar actionBar)
        {
            this.actionBar = actionBar;
            RequestLayout();
        }

        public void SetEnablePadding(bool enable)
        {
            paddingEnabled = enable;
            RequestLayout();
        }


        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            int topPadding =
                paddingEnabled && actionBar != null && actionBar.IsShowing ? actionBar.Height : 0;
            SetPadding(0, topPadding, 0, 0);

            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }

    }
}