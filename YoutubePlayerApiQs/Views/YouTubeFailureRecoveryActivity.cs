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

namespace YoutubePlayerApiQs.Views
{
    public abstract class YouTubeFailureRecoveryActivity : YouTubeBaseActivity, IYouTubePlayerOnInitializedListener
    {
        private static int RECOVERY_DIALOG_REQUEST = 1;
        public void OnInitializationFailure(IYouTubePlayerProvider p0, YouTubeInitializationResult p1)
        {
            if (p1.IsUserRecoverableError)
            {
                p1.GetErrorDialog(this, RECOVERY_DIALOG_REQUEST).Show();
            }
            else
            {
                String errorMessage = String.Format(GetString(Resource.String.error_player), p1.ToString());
                Toast.MakeText(this, errorMessage, ToastLength.Short).Show();
            }
        }

        public virtual void OnInitializationSuccess(IYouTubePlayerProvider p01, IYouTubePlayer p02, bool p1)
        {
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == RECOVERY_DIALOG_REQUEST)
            {
                // Retry initialization if user performed a recovery action
                GetYouTubePlayerProvider().Initialize(DeveloperKey.DEVELOPER_KEY, this);
            }
        }

        protected abstract IYouTubePlayerProvider GetYouTubePlayerProvider();
        
    }
}