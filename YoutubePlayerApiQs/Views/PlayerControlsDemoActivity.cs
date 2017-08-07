using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Com.Google.Android.Youtube.Player;
using System.Diagnostics;
using Android.Text;

//finish
namespace YoutubePlayerApiQs.Views
{
    [Activity]
    public class PlayerControlsDemoActivity : YouTubeFailureRecoveryActivity,
        View.IOnClickListener,
        TextView.IOnEditorActionListener,
        CompoundButton.IOnCheckedChangeListener,
        AdapterView.IOnItemSelectedListener
    {
        private static ListEntry[] ENTRIES = {
      new ListEntry("Androidify App", "irH3OSOskcE", false),
      new ListEntry("Chrome Speed Tests", "nCgQDjiotG0", false),
      new ListEntry("Playlist: Google I/O 2012", "PL56D792A831D0C362", true)};

        private string KEY_CURRENTLY_SELECTED_ID = "currentlySelectedId";

        private YouTubePlayerView youTubePlayerView;
        private IYouTubePlayer player;
        private TextView stateText;
        private ArrayAdapter<ListEntry> videoAdapter;
        private Spinner videoChooser;
        private Button playButton;
        private Button pauseButton;
        private EditText skipTo;
        private TextView eventLog;
        private SpannableStringBuilder logString;
        private RadioGroup styleRadioGroup;

        private MyPlaylistEventListener playlistEventListener;
        private MyPlayerStateChangeListener playerStateChangeListener;
        private MyPlaybackEventListener playbackEventListener;

        private int currentlySelectedPosition;
        private string currentlySelectedId;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.player_controls_demo);

            InitInterface();
        }

        public void InitInterface()
        {
            youTubePlayerView = FindViewById<YouTubePlayerView>(Resource.Id.youtube_view);
            stateText = FindViewById<TextView>(Resource.Id.state_text);
            videoChooser = FindViewById<Spinner>(Resource.Id.video_chooser);
            playButton = FindViewById<Button>(Resource.Id.play_button);
            pauseButton = FindViewById<Button>(Resource.Id.pause_button);
            skipTo = FindViewById<EditText>(Resource.Id.skip_to_text);
            eventLog = FindViewById<TextView>(Resource.Id.event_log);

            styleRadioGroup = FindViewById<RadioGroup>(Resource.Id.style_radio_group);
            (FindViewById<RadioButton>(Resource.Id.style_default)).SetOnCheckedChangeListener(this);
            (FindViewById<RadioButton>(Resource.Id.style_minimal)).SetOnCheckedChangeListener(this);
            (FindViewById<RadioButton>(Resource.Id.style_chromeless)).SetOnCheckedChangeListener(this);
            logString = new SpannableStringBuilder();

            videoAdapter = new ArrayAdapter<ListEntry>(this, Android.Resource.Layout.SimpleSpinnerItem, ENTRIES);
            videoAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            videoChooser.OnItemSelectedListener = this;
            videoChooser.Adapter = videoAdapter;

            playButton.SetOnClickListener(this);
            pauseButton.SetOnClickListener(this);
            skipTo.SetOnEditorActionListener(this);

            youTubePlayerView.Initialize(DeveloperKey.DEVELOPER_KEY, this);


            playlistEventListener = new MyPlaylistEventListener()
            {
                OnNextAction = () =>
                {
                    Log("NEXT VIDEO");
                },
                OnPlaylistEndedAction = () =>
                {
                    Log("PLAYLIST ENDED");
                },
                OnPreviousAction = () =>
                {
                    Log("PREVIOUS VIDEO");
                }
            };
            playerStateChangeListener = new MyPlayerStateChangeListener()
            {
                OnAdStartedAction = () =>
                {
                    UpdateText();
                    Log(playerStateChangeListener.playerState);
                },
                OnErrorAction = (p0) =>
                {
                    if (p0 == YouTubePlayerErrorReason.UnexpectedServiceDisconnection)
                    {
                        // When this error occurs the player is released and can no longer be used.
                        player = null;
                        SetControlsEnabled(false);
                    }
                    UpdateText();
                    Log(playerStateChangeListener.playerState);
                },
                OnLoadedAction = (p0) =>
                {
                    UpdateText();
                    Log(playerStateChangeListener.playerState);
                },
                OnLoadingAction = () =>
                {
                    UpdateText();
                    Log(playerStateChangeListener.playerState);
                },
                OnVideoEndedAction = () =>
                {
                    UpdateText();
                    Log(playerStateChangeListener.playerState);
                },
                OnVideoStartedAction = () =>
                {
                    UpdateText();
                    Log(playerStateChangeListener.playerState);
                }
            };
            playbackEventListener = new MyPlaybackEventListener()
            {
                OnPlayingAction = () =>
                {
                    UpdateText();
                }
            };

            SetControlsEnabled(false);
        }

        public override void OnInitializationSuccess(IYouTubePlayerProvider provider, IYouTubePlayer player, bool wasRestored)
        {
            this.player = player;
            player.SetPlaylistEventListener(playlistEventListener);
            player.SetPlayerStateChangeListener(playerStateChangeListener);
            player.SetPlaybackEventListener(playbackEventListener);

            if (!wasRestored)
            {
                PlayVideoAtSelection();
            }
            SetControlsEnabled(true);
        }

        public void OnCheckedChanged(CompoundButton buttonView, bool isChecked)
        {
            if (isChecked && player != null)
            {
                switch (buttonView.Id)
                {
                    case Resource.Id.style_default:
                        player.SetPlayerStyle(YouTubePlayerPlayerStyle.Default);
                        break;
                    case Resource.Id.style_minimal:
                        player.SetPlayerStyle(YouTubePlayerPlayerStyle.Minimal);
                        break;
                    case Resource.Id.style_chromeless:
                        player.SetPlayerStyle(YouTubePlayerPlayerStyle.Chromeless);
                        break;
                }
            }
        }

        public void OnClick(View v)
        {
            if (v == playButton)
            {
                player.Play();
            }
            else if (v == pauseButton)
            {
                player.Pause();
            }
        }

        public bool OnEditorAction(TextView v, [GeneratedEnum] ImeAction actionId, KeyEvent e)
        {
            if (v == skipTo)
            {
                int skipToSecs = ParseInt(skipTo.Text.ToString(), 0);
                player.SeekToMillis(skipToSecs * 1000);
                InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
                imm.HideSoftInputFromWindow(skipTo.WindowToken, 0);
                return true;
            }
            return false;
        }

        public void OnItemSelected(AdapterView parent, View view, int position, long id)
        {
            currentlySelectedPosition = position;
            PlayVideoAtSelection();
        }

        public void OnNothingSelected(AdapterView parent)
        {
        }

        protected override IYouTubePlayerProvider GetYouTubePlayerProvider()
        {
            return youTubePlayerView;
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutString(KEY_CURRENTLY_SELECTED_ID, currentlySelectedId);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
            currentlySelectedId = savedInstanceState.GetString(KEY_CURRENTLY_SELECTED_ID);
        }
        private void PlayVideoAtSelection()
        {
            ListEntry selectedEntry = videoAdapter.GetItem(currentlySelectedPosition);
            if (selectedEntry.id != currentlySelectedId && player != null)
            {
                currentlySelectedId = selectedEntry.id;
                if (selectedEntry.isPlaylist)
                {
                    player.CuePlaylist(selectedEntry.id);
                }
                else
                {
                    player.CueVideo(selectedEntry.id);
                }
            }
        }

        public void UpdateText()
        {
            stateText.Text = string.Format("Current state: %s %s %s",
                playerStateChangeListener.playerState, playbackEventListener.playbackState,
                playbackEventListener.bufferingState);
        }

        private void Log(String message)
        {
            logString.Append(message + "\n");
            eventLog.SetText(logString, TextView.BufferType.Normal);
        }

        private void SetControlsEnabled(bool enabled)
        {
            playButton.Enabled = enabled;
            pauseButton.Enabled = enabled;
            skipTo.Enabled = enabled;
            videoChooser.Enabled = enabled;
            for (int i = 0; i < styleRadioGroup.ChildCount; i++)
            {
                styleRadioGroup.GetChildAt(i).Enabled = enabled;
            }
        }

        private int ParseInt(string intString, int defaultValue)
        {
            try
            {
                int x = 0;
                Int32.TryParse(intString, out x);
                return intString != null ? x : defaultValue;
            }
            catch (Java.Lang.NumberFormatException e)
            {
                return defaultValue;
            }
        }

        private string FormatTime(int millis)
        {
            int seconds = millis / 1000;
            int minutes = seconds / 60;
            int hours = minutes / 60;

            return (hours == 0 ? "" : hours + ":")
                + String.Format("%02d:%02d", minutes % 60, seconds % 60);
        }

        private string GetTimesText()
        {
            int currentTimeMillis = player.CurrentTimeMillis;
            int durationMillis = player.DurationMillis;
            return String.Format("(%s/%s)", FormatTime(currentTimeMillis), FormatTime(durationMillis));
        }

        private class MyPlaylistEventListener : Java.Lang.Object, IYouTubePlayerPlaylistEventListener
        {
            public Action OnNextAction { get; set; }
            public Action OnPlaylistEndedAction { get; set; }
            public Action OnPreviousAction { get; set; }

            public void OnNext()
            {
                OnNextAction?.Invoke();
            }

            public void OnPlaylistEnded()
            {
                OnPlaylistEndedAction?.Invoke();
            }

            public void OnPrevious()
            {
                OnPreviousAction?.Invoke();
            }
        }

        private class MyPlaybackEventListener : Java.Lang.Object, IYouTubePlayerPlaybackEventListener
        {
            public string playbackState = "NOT_PLAYING";
            public string bufferingState = "";
            public Action OnPlayingAction { get; set; }

            public void OnBuffering(bool p0)
            {
            }

            public void OnPaused()
            {
            }

            public void OnPlaying()
            {
                playbackState = "PLAYING";
                OnPlayingAction?.Invoke();
            }

            public void OnSeekTo(int p0)
            {
            }

            public void OnStopped()
            {
            }
        }

        private class MyPlayerStateChangeListener : Java.Lang.Object, IYouTubePlayerPlayerStateChangeListener
        {
            public string playerState = "UNINITIALIZED";
            public Action OnAdStartedAction { get; set; }
            public Action<YouTubePlayerErrorReason> OnErrorAction { get; set; }
            public Action<string> OnLoadedAction { get; set; }
            public Action OnLoadingAction { get; set; }
            public Action OnVideoEndedAction { get; set; }
            public Action OnVideoStartedAction { get; set; }

            public void OnAdStarted()
            {
                playerState = "AD_STARTED";
                OnAdStartedAction?.Invoke();
            }

            public void OnError(YouTubePlayerErrorReason p0)
            {
                playerState = "ERROR (" + p0 + ")";
                OnErrorAction?.Invoke(p0);
            }

            public void OnLoaded(string p0)
            {
                playerState = string.Format("LOADED %s", p0);
                OnLoadedAction?.Invoke(p0);
            }

            public void OnLoading()
            {
                playerState = "LOADING";
                OnLoadingAction?.Invoke();
            }

            public void OnVideoEnded()
            {
                playerState = "VIDEO_ENDED";
                OnVideoEndedAction?.Invoke();
            }

            public void OnVideoStarted()
            {
                playerState = "VIDEO_STARTED";
                OnVideoStartedAction?.Invoke();
            }
        }
        private class ListEntry
        {

            public string title;
            public string id;
            public bool isPlaylist;

            public ListEntry(string title, string videoId, bool isPlaylist)
            {
                this.title = title;
                this.id = videoId;
                this.isPlaylist = isPlaylist;
            }
            public override string ToString()
            {
                return title;
            }

        }
    }
}