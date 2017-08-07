using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Youtube.Player;
using Java.Lang;
using Java.Security;
using Java.Util;
using Android.Widget;
using YoutubePlayerApiQs.Views;
using System;
using static Android.Animation.Animator;
using Android.Animation;

namespace YoutubePlayerApiQs.Views
{
    [Activity]
    public class VideoListDemoActivity : Activity, IYouTubePlayerOnFullscreenListener
    {

        /** The duration of the animation sliding up the video in portrait. */
        private int ANIMATION_DURATION_MILLIS = 300;
        /** The pAdding between the video list and the video in landscape orientation. */
        private int LANDSCAPE_VIDEO_PADDING_DP = 5;

        /** The request code when calling startActivityForResult to recover from an API service error. */
        private int RECOVERY_DIALOG_REQUEST = 1;

        private VideoListFragment listFragment;
        private VideoFragment videoFragment;

        private View videoBox;
        private View closeButton;

        private bool isFullscreen;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.video_list_demo);

            listFragment = FragmentManager.FindFragmentById<VideoListFragment>(Resource.Id.list_fragment);
            videoFragment = FragmentManager.FindFragmentById<VideoFragment>(Resource.Id.video_fragment_container);

            videoBox = FindViewById(Resource.Id.video_box);
            closeButton = FindViewById(Resource.Id.close_button);
            closeButton.Click += (s, e) =>
            {
                OnClickClose(closeButton);
            };

            videoBox.Visibility = ViewStates.Invisible;
            DoLayout();
            CheckYouTubeApi();
        }

        private void CheckYouTubeApi()
        {
            YouTubeInitializationResult errorReason =
                YouTubeApiServiceUtil.IsYouTubeApiServiceAvailable(this);
            if (errorReason.IsUserRecoverableError)
            {
                errorReason.GetErrorDialog(this, RECOVERY_DIALOG_REQUEST).Show();
            }
            else if (errorReason != YouTubeInitializationResult.Success)
            {
                string errorMessage =
                    string.Format(Resources.GetString(Resource.String.error_player), errorReason.ToString());
                Toast.MakeText(this, errorMessage, ToastLength.Short).Show();
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == RECOVERY_DIALOG_REQUEST)
            {
                // Recreate the activity if user performed a recovery action
                Recreate();
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            DoLayout();
        }

        private void DoLayout()
        {
            bool isPortrait =
                Resources.Configuration.Orientation == Android.Content.Res.Orientation.Portrait;

            listFragment.View.Visibility = (isFullscreen ? ViewStates.Gone : ViewStates.Visible);
            listFragment.SetLabelVisibility(isPortrait);
            closeButton.Visibility = (isPortrait ? ViewStates.Visible : ViewStates.Gone);


            if (isFullscreen)
            {
                videoBox.TranslationY = (0); // ReSet any translation that was applied in portrait.

                SetLayoutSize(videoFragment.View, ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
                SetLayoutSizeAndGravity(videoBox, ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent, GravityFlags.Top | GravityFlags.Left);
            }
            else if (isPortrait)
            {
                SetLayoutSize(listFragment.View, ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
                SetLayoutSize(videoFragment.View, ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                SetLayoutSizeAndGravity(videoBox, ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, GravityFlags.Bottom);
            }
            else
            {
                videoBox.TranslationY = (0); // ReSet any translation that was applied in portrait.
                int screenWidth = DpToPx(Resources.Configuration.ScreenWidthDp);
                SetLayoutSize(listFragment.View, screenWidth / 4, ViewGroup.LayoutParams.MatchParent);
                int videoWidth = screenWidth - screenWidth / 4 - DpToPx(LANDSCAPE_VIDEO_PADDING_DP);
                SetLayoutSize(videoFragment.View, videoWidth, ViewGroup.LayoutParams.WrapContent);
                SetLayoutSizeAndGravity(videoBox, videoWidth, ViewGroup.LayoutParams.WrapContent,
                    GravityFlags.Right | GravityFlags.Center);
            }
        }
        private int DpToPx(int dp)
        {
            return (int)(dp * Resources.DisplayMetrics.Density + 0.5f);
        }
        private void SetLayoutSize(View view, int width, int height)
        {
            ViewGroup.LayoutParams param = view.LayoutParameters;
            param.Width = width;
            param.Height = height;
            view.LayoutParameters = param;
        }
        private void SetLayoutSizeAndGravity(View view, int width, int height, GravityFlags gravity)
        {
            FrameLayout.LayoutParams param = (FrameLayout.LayoutParams)view.LayoutParameters;
            param.Height = height;
            param.Width = width;
            param.Gravity = gravity;
            view.LayoutParameters = param;
        }

        public void OnFullscreen(bool p0)
        {
            this.isFullscreen = p0;
            DoLayout();
        }
        public void OnClickClose(View view)
        {
            listFragment.ListView.ClearChoices();
            listFragment.ListView.RequestLayout();
            videoFragment.Pause();
            ViewPropertyAnimator animator = videoBox.Animate().TranslationYBy(videoBox.Height).SetDuration(ANIMATION_DURATION_MILLIS);
            animator.SetListener(new AinimatorListener()
            {
                AnimatorEnd = (a) =>
                {
                    if (videoBox.Height < 0) videoBox.Visibility = ViewStates.Gone;
                }
            });
        }

    }

    class AinimatorListener : Java.Lang.Object, IAnimatorListener
    {
        public Action<Animator> AnimatorEnd { get; set; }
        public void OnAnimationCancel(Animator animation)
        {
        }

        public void OnAnimationEnd(Animator animation)
        {
            AnimatorEnd?.Invoke(animation);
        }

        public void OnAnimationRepeat(Animator animation)
        {
        }

        public void OnAnimationStart(Animator animation)
        {
        }
    }

    /**
     * A fragment that shows a static list of videos.
     */
    public class VideoListFragment : ListFragment
    {

        private static List<VideoEntry> VIDEO_LIST = new List<VideoEntry>();

        List<VideoEntry> lists = new List<VideoEntry>();


        private PageAdapter adapter;
        private View videoBox;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            lists.Add(new VideoEntry("YouTube Collection", "Y_UmWdcTrrc"));
            lists.Add(new VideoEntry("GMail Tap", "1KhZKNZO8mQ"));
            lists.Add(new VideoEntry("Chrome Multitask", "UiLSiqyDf4Y"));
            lists.Add(new VideoEntry("Google Fiber", "re0VRK6ouwI"));
            lists.Add(new VideoEntry("Autocompleter", "blB_X38YSxQ"));
            lists.Add(new VideoEntry("GMail Motion", "Bu927_ul_X0"));
            lists.Add(new VideoEntry("Translate for Animals", "3I24bSteJpw"));
            var temp = Collections.UnmodifiableList(lists);
            foreach (var item in temp)
            {
                VIDEO_LIST.Add((VideoEntry)item);
            }
            adapter = new PageAdapter(Activity, VIDEO_LIST);
        }


        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            videoBox = Activity.FindViewById(Resource.Id.video_box);
            ListView.ChoiceMode = ChoiceMode.Single;
            ListAdapter = (adapter);
        }


        public override void OnListItemClick(ListView l, View v, int position, long id)
        {
            string videoId = VIDEO_LIST[position].videoId;

            VideoFragment videoFragment =
                (VideoFragment)FragmentManager.FindFragmentById(Resource.Id.video_fragment_container);
            videoFragment.SetVideoId(videoId);

            // The videoBox is INVISIBLE if no video was previously selected, so we need to show it now.
            if (videoBox.Visibility != ViewStates.Visible)
            {
                if (Resources.Configuration.Orientation == Android.Content.Res.Orientation.Portrait)
                {
                    // Initially translate off the screen so that it can be animated in from below.
                    videoBox.TranslationY = (videoBox.Height);
                }
                videoBox.Visibility = ViewStates.Visible;
            }

            // If the fragment is off the screen, we animate it in.
            if (videoBox.TranslationY > 0)
            {
                videoBox.Animate().TranslationY(0).SetDuration(300);//ANIMATION_DURATION_MILLIS
            }
        }


        public override void OnDestroyView()
        {
            base.OnDestroyView();

            adapter.ReleaseLoaders();
        }

        public void SetLabelVisibility(bool visible)
        {
            adapter.SetLabelVisibility(visible);
        }
    }

    /**
     * Adapter for the video list. Manages a Set of YouTubeThumbnailViews, including initializing each
     * of them only once and keeping track of the loader of each one. When the ListFragment Gets
     * destroyed it Releases all the loaders.
     */
    public class PageAdapter : BaseAdapter
    {

        private List<VideoEntry> entries;
        private List<View> entryViews;
        private Dictionary<YouTubeThumbnailView, IYouTubeThumbnailLoader> thumbnailViewToLoaderMap;
        private LayoutInflater inflater;
        private ThumbnailListener thumbnailListener;

        private bool labelsVisible;
        Context context;
        public PageAdapter(Context context, List<VideoEntry> entries)
        {
            this.entries = entries;
            this.context = context;
            entryViews = new List<View>();
            thumbnailViewToLoaderMap = new Dictionary<YouTubeThumbnailView, IYouTubeThumbnailLoader>();
            inflater = LayoutInflater.From(context);
            thumbnailListener = new ThumbnailListener()
            {
                OnInitializationSuccessAction = (view, loader) =>
                {
                    thumbnailViewToLoaderMap.Add(view, loader);
                }
            };

            labelsVisible = true;
        }

        public void ReleaseLoaders()
        {
            foreach (var loader in thumbnailViewToLoaderMap.Values)
            {
                loader.Release();
            }
        }

        public void SetLabelVisibility(bool visible)
        {
            labelsVisible = visible;
            foreach (var view in entryViews)
            {
                view.FindViewById(Resource.Id.text).Visibility = (visible ? ViewStates.Visible : ViewStates.Gone);
            }
        }


        public override int Count
        {
            get { return entries.Count; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return entries[position];
        }



        public override long GetItemId(int position)
        {
            return 0;
        }


        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            VideoEntry entry = entries[position];

            // There are three cases here
            if (view == null)
            {
                // 1) The view has not yet been created - we need to initialize the YouTubeThumbnailView.
                view = inflater.Inflate(Resource.Layout.video_list_item, parent, false);
                YouTubeThumbnailView thumbnail = (YouTubeThumbnailView)view.FindViewById(Resource.Id.thumbnail);
                thumbnail.Tag = (entry.videoId);
                thumbnail.Initialize(DeveloperKey.DEVELOPER_KEY, thumbnailListener);
            }
            else
            {
                YouTubeThumbnailView thumbnail = (YouTubeThumbnailView)view.FindViewById(Resource.Id.thumbnail);
                IYouTubeThumbnailLoader loader = thumbnailViewToLoaderMap[thumbnail];
                if (loader == null)
                {
                    // 2) The view is already created, and is currently being initialized. We store the
                    //    current videoId in the tag.
                    thumbnail.Tag = (entry.videoId);
                }
                else
                {
                    // 3) The view is already created and already initialized. Simply Set the right videoId
                    //    on the loader.
                    thumbnail.SetImageResource(Resource.Drawable.loading_thumbnail);
                    loader.SetVideo(entry.videoId);
                }
            }
            TextView label = ((TextView)view.FindViewById(Resource.Id.text));
            label.Text = (entry.text);
            label.Visibility = (labelsVisible ? ViewStates.Visible : ViewStates.Gone);
            return view;
        }


    }
    public class ThumbnailListener : Java.Lang.Object, YouTubeThumbnailView.IOnInitializedListener, IYouTubeThumbnailLoaderOnThumbnailLoadedListener
    {
        public Action<YouTubeThumbnailView, IYouTubeThumbnailLoader> OnInitializationSuccessAction { get; set; }
        public void OnInitializationFailure(YouTubeThumbnailView view, YouTubeInitializationResult loader)
        {
            view.SetImageResource(Resource.Drawable.no_thumbnail);
        }
        public void OnInitializationSuccess(YouTubeThumbnailView view, IYouTubeThumbnailLoader loader)
        {
            loader.SetOnThumbnailLoadedListener(this);
            OnInitializationSuccessAction?.Invoke(view, loader);
            //thumbnailViewToLoaderMap.put(view, loader);
            view.SetImageResource(Resource.Drawable.loading_thumbnail);
            string videoId = (string)view.Tag;
            loader.SetVideo(videoId);
        }
        public void OnThumbnailError(YouTubeThumbnailView view, YouTubeThumbnailLoaderErrorReason p1)
        {
            view.SetImageResource(Resource.Drawable.no_thumbnail);
        }
        public void OnThumbnailLoaded(YouTubeThumbnailView p0, string p1)
        {
        }
    }

    public class VideoFragment : YouTubePlayerFragment,
            IYouTubePlayerOnInitializedListener
    {

        private IYouTubePlayer player;
        private string videoId;

        public VideoFragment NewInstance()
        {
            return new VideoFragment();
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Initialize(DeveloperKey.DEVELOPER_KEY, this);
        }

        public override void OnDestroy()
        {
            if (player != null)
            {
                player.Release();
            }
            base.OnDestroy();
        }
        public void SetVideoId(string videoId)
        {
            if (videoId != null && !videoId.Equals(this.videoId))
            {
                this.videoId = videoId;
                if (player != null)
                {
                    player.CueVideo(videoId);
                }
            }
        }
        public void Pause()
        {
            if (player != null)
            {
                player.Pause();
            }
        }

        public void OnInitializationFailure(IYouTubePlayerProvider p0, YouTubeInitializationResult p1)
        {
            this.player = null;
        }

        public void OnInitializationSuccess(IYouTubePlayerProvider provider, IYouTubePlayer player, bool restored)
        {
            this.player = player;
            player.AddFullscreenControlFlag(YouTubePlayer.FullscreenFlagCustomLayout);
            player.SetOnFullscreenListener((VideoListDemoActivity)Activity);
            if (!restored && videoId != null)
            {
                player.CueVideo(videoId);
            }
        }
    }

    public class VideoEntry : Java.Lang.Object
    {
        public string text;
        public string videoId;

        public VideoEntry(string text, string videoId)
        {
            this.text = text;
            this.videoId = videoId;
        }
    }


}