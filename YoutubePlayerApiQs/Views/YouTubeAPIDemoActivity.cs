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

namespace YoutubePlayerApiQs.Views
{
    [Activity(Label = "YoutubePlayerApiQs", MainLauncher = true, Icon = "@drawable/icon")]
    public class YouTubeAPIDemoActivity : Activity
    {
        private Button btnVideoList;
        private Button btnVideoWall;
        private Button btnSimplePlayerView;
        private Button btnSimplePlayerFragment;
        private Button btnCustomPlayerControls;
        private Button btnCustomFullscreenHandling;
        private Button btnStandalonePlayer;
        private Button btnAppLauncherIntents;
        private Intent intent;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.demo_home);

            btnVideoList = FindViewById<Button>(Resource.Id.btn_video_list);
            btnVideoWall = FindViewById<Button>(Resource.Id.btn_video_wall);
            btnSimplePlayerView = FindViewById<Button>(Resource.Id.btn_simple_playerview);
            btnSimplePlayerFragment = FindViewById<Button>(Resource.Id.btn_simple_playerfragment);
            btnCustomPlayerControls = FindViewById<Button>(Resource.Id.btn_custom_player_control);
            btnCustomFullscreenHandling = FindViewById<Button>(Resource.Id.btn_custom_fullscreen_handling);
            btnStandalonePlayer = FindViewById<Button>(Resource.Id.btn_standalone_player);
            btnAppLauncherIntents = FindViewById<Button>(Resource.Id.btn_youtube_launcher_intent);

            btnVideoList.Click += (s, e) =>
            {
                intent = new Intent(this, typeof(VideoListDemoActivity));
                StartActivity(intent);
            };
            btnVideoWall.Click += (s, e) =>
            {
                intent = new Intent(this, typeof(VideoWallDemoActivity));
                StartActivity(intent);
            };
            btnSimplePlayerView.Click += (s, e) =>
            {
                intent = new Intent(this, typeof(PlayerViewDemoActivity));
                StartActivity(intent);
            };
            btnSimplePlayerFragment.Click += (s, e) =>
            {
                intent = new Intent(this, typeof(FragmentDemoActivity));
                StartActivity(intent);
            };
            btnCustomPlayerControls.Click += (s, e) =>
            {
                intent = new Intent(this, typeof(PlayerControlsDemoActivity));
                StartActivity(intent);
            };
            btnCustomFullscreenHandling.Click += (s, e) =>
            {
                intent = new Intent(this, typeof(FullscreenDemoActivity));
                StartActivity(intent);
            };
            btnStandalonePlayer.Click += (s, e) =>
            {
                intent = new Intent(this, typeof(StandalonePlayerDemoActivity));
                StartActivity(intent);
            };
            btnAppLauncherIntents.Click += (s, e) =>
            {
                intent = new Intent(this, typeof(IntentsDemoActivity));
                StartActivity(intent);
            };
        }
    }
}