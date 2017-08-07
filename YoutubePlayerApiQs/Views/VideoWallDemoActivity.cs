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
using YoutubePlayerApiQs.Views.Ui;
using Com.Google.Android.Youtube.Player;
using Android.Util;

namespace YoutubePlayerApiQs.Views
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape,
        Theme = "@style/BlackNoBarsTheme",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.KeyboardHidden)]
    public class VideoWallDemoActivity : Activity,
        FlippingView.Listener,
        IYouTubePlayerOnInitializedListener,
        YouTubeThumbnailView.IOnInitializedListener
    {
        private int RECOVERY_DIALOG_REQUEST = 1;

        /** The player view cannot be smaller than 110 pixels high. */
        private float PLAYER_VIEW_MINIMUM_HEIGHT_DP = 110;
        private int MAX_NUMBER_OF_ROWS_WANTED = 4;

        // Example playlist from which videos are displayed on the video wall
        private String PLAYLIST_ID = "ECAE6B03CA849AD332";

        private int INTER_IMAGE_PADDING_DP = 5;

        // YouTube thumbnails have a 16 / 9 aspect ratio
        private double THUMBNAIL_ASPECT_RATIO = 16 / 9d;

        private int INITIAL_FLIP_DURATION_MILLIS = 100;
        private int FLIP_DURATION_MILLIS = 500;
        private int FLIP_PERIOD_MILLIS = 2000;

        private ImageWallView imageWallView;
        private Handler flipDelayHandler;

        private FlippingView flippingView;
        private YouTubeThumbnailView thumbnailView;
        private IYouTubeThumbnailLoader thumbnailLoader;

        private YouTubePlayerFragment playerFragment;
        private View playerView;
        private IYouTubePlayer player;

        private Dialog errorDialog;

        private int flippingCol;
        private int flippingRow;
        private int videoCol;
        private int videoRow;

        private bool nextThumbnailLoaded;
        private bool activityResumed;
        private State state;

        private enum State
        {
            UNINITIALIZED,
            LOADING_THUMBNAILS,
            VIDEO_FLIPPED_OUT,
            VIDEO_LOADING,
            VIDEO_CUED,
            VIDEO_PLAYING,
            VIDEO_ENDED,
            VIDEO_BEING_FLIPPED_OUT,
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            state = State.UNINITIALIZED;
            ViewGroup viewFrame = new FrameLayout(this);
            DisplayMetrics displayMetrics = Resources.DisplayMetrics;
            int maxAllowedNumberOfRows = (int)Math.Floor(
                (displayMetrics.HeightPixels / displayMetrics.Density) / PLAYER_VIEW_MINIMUM_HEIGHT_DP);
            int numberOfRows = Math.Min(maxAllowedNumberOfRows, MAX_NUMBER_OF_ROWS_WANTED);
            int interImagePaddingPx = (int)displayMetrics.Density * INTER_IMAGE_PADDING_DP;
            int imageHeight = (displayMetrics.HeightPixels / numberOfRows) - interImagePaddingPx;
            int imageWidth = (int)(imageHeight * THUMBNAIL_ASPECT_RATIO);

            imageWallView = new ImageWallView(this, imageWidth, imageHeight, interImagePaddingPx);
            viewFrame.AddView(imageWallView, ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

            thumbnailView = new YouTubeThumbnailView(this);
            thumbnailView.Initialize(DeveloperKey.DEVELOPER_KEY, this);

            flippingView = new FlippingView(this, this, imageWidth, imageHeight);
            flippingView.SetFlipDuration(INITIAL_FLIP_DURATION_MILLIS);
            viewFrame.AddView(flippingView, imageWidth, imageHeight);

            playerView = new FrameLayout(this);
            playerView.Id = Resource.Id.player_view;
            playerView.Visibility = ViewStates.Invisible;
            viewFrame.AddView(playerView, imageWidth, imageHeight);

            playerFragment = YouTubePlayerFragment.NewInstance();
            playerFragment.Initialize(DeveloperKey.DEVELOPER_KEY, this);
            FragmentManager.BeginTransaction().Add(Resource.Id.player_view, playerFragment).Commit();
            flipDelayHandler = new FlipDelayHandler()
            {
                HandleMessageAction = (msg) =>
                {
                    FlipNext();
                }
            };
            SetContentView(viewFrame);
        }

        public void OnInitializationFailure(YouTubeThumbnailView thumbnailView, YouTubeInitializationResult errorReason)
        {
            if (errorReason.IsUserRecoverableError)
            {
                if (errorDialog == null || !errorDialog.IsShowing)
                {
                    errorDialog = errorReason.GetErrorDialog(this, RECOVERY_DIALOG_REQUEST);
                    errorDialog.Show();
                }
            }
            else
            {
                string errorMessage =
                    string.Format(GetString(Resource.String.error_thumbnail_view), errorReason.ToString());
                Toast.MakeText(this, errorMessage, ToastLength.Short).Show();
            }
        }

        public void OnInitializationFailure(IYouTubePlayerProvider provider, YouTubeInitializationResult errorReason)
        {
            if (errorReason.IsUserRecoverableError)
            {
                if (errorDialog == null || !errorDialog.IsShowing)
                {
                    errorDialog = errorReason.GetErrorDialog(this, RECOVERY_DIALOG_REQUEST);
                    errorDialog.Show();
                }
            }
            else
            {
                string errorMessage = string.Format(GetString(Resource.String.error_player), errorReason.ToString());
                Toast.MakeText(this, errorMessage, ToastLength.Short).Show();
            }
        }

        public void OnInitializationSuccess(YouTubeThumbnailView thumbnailView, IYouTubeThumbnailLoader thumbnailLoader)
        {
            this.thumbnailLoader = thumbnailLoader;
            thumbnailLoader.SetOnThumbnailLoadedListener(new ThumbnailListener()
            {
                ThumbnailLoaded = (thumbnail, videoId) =>
                {
                    nextThumbnailLoaded = true;

                    if (activityResumed)
                    {
                        if (state.Equals(State.LOADING_THUMBNAILS))
                        {
                            FlipNext();
                        }
                        else if (state.Equals(State.VIDEO_FLIPPED_OUT))
                        {
                            // load player with the video of the next thumbnail being flipped in
                            state = State.VIDEO_LOADING;
                            player.CueVideo(videoId);
                        }
                    }
                },
                ThumbnailError = (thumbnail, reason) =>
                {
                    LoadNextThumbnail();
                }
            });
            MaybeStartDemo();
        }

        public void OnInitializationSuccess(IYouTubePlayerProvider provider, IYouTubePlayer player, bool wasResumed)
        {
            this.player = player;
            player.SetPlayerStyle(YouTubePlayerPlayerStyle.Chromeless);
            player.SetPlayerStateChangeListener(new VideoListener()
            {
                Loaded = () =>
                {
                    state = State.VIDEO_CUED;
                },
                VideoEnded = () =>
                {
                    state = State.VIDEO_ENDED;
                    imageWallView.ShowImage(videoCol, videoRow);
                    playerView.Visibility = ViewStates.Invisible;
                },
                Error = (errorReason) =>
                {
                    if (errorReason == YouTubePlayerErrorReason.UnexpectedServiceDisconnection)
                    {
                        // player has encountered an unrecoverable error - stop the demo
                        flipDelayHandler.RemoveCallbacksAndMessages(null);
                        state = State.UNINITIALIZED;
                        thumbnailLoader.Release();
                        thumbnailLoader = null;
                        player = null;
                    }
                    else
                    {
                        state = State.VIDEO_ENDED;
                    }
                }
            });
            MaybeStartDemo();
        }
        private void MaybeStartDemo()
        {
            if (activityResumed && player != null && thumbnailLoader != null
                && state.Equals(State.UNINITIALIZED))
            {
                thumbnailLoader.SetPlaylist(PLAYLIST_ID); // loading the first thumbnail will kick off demo
                state = State.LOADING_THUMBNAILS;
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == RECOVERY_DIALOG_REQUEST)
            {
                // Retry initialization if user performed a recovery action
                if (errorDialog != null && errorDialog.IsShowing)
                {
                    errorDialog.Dismiss();
                }
                errorDialog = null;
                playerFragment.Initialize(DeveloperKey.DEVELOPER_KEY, this);
                thumbnailView.Initialize(DeveloperKey.DEVELOPER_KEY, this);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            activityResumed = true;
            if (thumbnailLoader != null && player != null)
            {
                if (state.Equals(State.UNINITIALIZED))
                {
                    MaybeStartDemo();
                }
                else if (state.Equals(State.LOADING_THUMBNAILS))
                {
                    LoadNextThumbnail();
                }
                else
                {
                    if (state.Equals(State.VIDEO_PLAYING))
                    {
                        player.Play();
                    }
                    flipDelayHandler.SendEmptyMessageDelayed(0, FLIP_DURATION_MILLIS);
                }
            }
        }

        protected override void OnPause()
        {
            flipDelayHandler.RemoveCallbacksAndMessages(null);
            activityResumed = false;
            base.OnPause();
        }

        protected override void OnDestroy()
        {
            if (thumbnailLoader != null)
            {
                thumbnailLoader.Release();
            }
            base.OnDestroy();
        }

        private void FlipNext()
        {
            if (!nextThumbnailLoaded || state.Equals(State.VIDEO_LOADING))
            {
                return;
            }

            if (state.Equals(State.VIDEO_ENDED))
            {
                flippingCol = videoCol;
                flippingRow = videoRow;
                state = State.VIDEO_BEING_FLIPPED_OUT;
            }
            else
            {
                Pair nextTarget = imageWallView.GetNextLoadTarget();
                flippingCol = (int)nextTarget.First;
                flippingRow = (int)nextTarget.Second;
            }

            flippingView.SetX(imageWallView.GetXPosition(flippingCol, flippingRow));
            flippingView.SetY(imageWallView.GetYPosition(flippingCol, flippingRow));
            flippingView.SetFlipInDrawable(thumbnailView.Drawable);
            flippingView.SetFlipOutDrawable(imageWallView.GetImageDrawable(flippingCol, flippingRow));
            imageWallView.SetImageDrawable(flippingCol, flippingRow, thumbnailView.Drawable);
            imageWallView.HideImage(flippingCol, flippingRow);
            flippingView.Visibility = ViewStates.Visible;
            flippingView.Flip();
        }

        public void onFlipped(FlippingView view)
        {
            imageWallView.ShowImage(flippingCol, flippingRow);
            flippingView.Visibility = ViewStates.Invisible;

            if (activityResumed)
            {
                LoadNextThumbnail();

                if (state.Equals(State.VIDEO_BEING_FLIPPED_OUT))
                {
                    state = State.VIDEO_FLIPPED_OUT;
                }
                else if (state.Equals(State.VIDEO_CUED))
                {
                    videoCol = flippingCol;
                    videoRow = flippingRow;
                    playerView.SetX(imageWallView.GetXPosition(flippingCol, flippingRow));
                    playerView.SetY(imageWallView.GetYPosition(flippingCol, flippingRow));
                    imageWallView.HideImage(flippingCol, flippingRow);
                    playerView.Visibility = ViewStates.Visible;
                    player.Play();
                    state = State.VIDEO_PLAYING;
                }
                else if (state.Equals(State.LOADING_THUMBNAILS) && imageWallView.AllImagesLoaded())
                {
                    state = State.VIDEO_FLIPPED_OUT; // trigger flip in of an initial video
                    flippingView.SetFlipDuration(FLIP_DURATION_MILLIS);
                    flipDelayHandler.SendEmptyMessage(0);
                }
            }
        }

        private void LoadNextThumbnail()
        {
            nextThumbnailLoaded = false;
            if (thumbnailLoader.HasNext)
            {
                thumbnailLoader.Next();
            }
            else
            {
                thumbnailLoader.First();
            }
        }

        private class FlipDelayHandler : Handler
        {
            public Action<Message> HandleMessageAction { get; set; }
            public override void HandleMessage(Message msg)
            {
                HandleMessageAction?.Invoke(msg);
                SendEmptyMessageDelayed(0, 2000);//FLIP_PERIOD_MILLIS
            }
        }

        private class ThumbnailListener : Java.Lang.Object, IYouTubeThumbnailLoaderOnThumbnailLoadedListener
        {
            public Action<YouTubeThumbnailView, string> ThumbnailLoaded { get; set; }
            public Action<YouTubeThumbnailView, YouTubeThumbnailLoaderErrorReason> ThumbnailError { get; set; }
            public void OnThumbnailError(YouTubeThumbnailView thumbnail, YouTubeThumbnailLoaderErrorReason reason)
            {
                ThumbnailError?.Invoke(thumbnail, reason);
            }

            public void OnThumbnailLoaded(YouTubeThumbnailView thumbnail, string videoId)
            {
                ThumbnailLoaded?.Invoke(thumbnail, videoId);
            }
        }

        private class VideoListener : Java.Lang.Object, IYouTubePlayerPlayerStateChangeListener
        {
            public Action VideoEnded { get; set; }
            public Action<YouTubePlayerErrorReason> Error { get; set; }
            public Action Loaded { get; set; }
            public void OnAdStarted()
            {
            }

            public void OnError(YouTubePlayerErrorReason errorReason)
            {
                Error?.Invoke(errorReason);
            }

            public void OnLoaded(string videoId)
            {
                Loaded?.Invoke();
            }

            public void OnLoading()
            {
            }

            public void OnVideoEnded()
            {
                VideoEnded?.Invoke();
            }

            public void OnVideoStarted()
            {
            }
        }
    }
}