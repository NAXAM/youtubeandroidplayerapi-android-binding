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
using static Android.Net.Sip.SipAudioCall;
using Android.Animation;
using Android.Graphics.Drawables;
using Android.Views.Animations;

namespace YoutubePlayerApiQs.Views.Ui
{

    public class FlippingView : FrameLayout
    {
        public ImageView flipOutView;
        private ImageView flipInView;
        private AnimatorSet animations;
        private Listener listener;

        public interface Listener
        {
            void onFlipped(FlippingView view);
        }
        public FlippingView(Context context, Listener listener, int width, int height) : base(context)
        {
            this.listener = listener;
            this.flipOutView = new ImageView(context);
            this.flipInView = new ImageView(context);

            AddView(flipOutView, width, height);
            AddView(flipInView, width, height);

            flipInView.Rotation = -90;

            ObjectAnimator flipOutAnimator = ObjectAnimator.OfFloat(flipOutView, "rotationY", 0, 90);
            flipOutAnimator.SetInterpolator(new AccelerateInterpolator());
            Animator flipInAnimator = ObjectAnimator.OfFloat(flipInView, "rotationY", -90, 0);
            flipInAnimator.SetInterpolator(new DecelerateInterpolator());
            animations = new AnimatorSet();
            animations.PlaySequentially(flipOutAnimator, flipInAnimator);
            animations.SetDuration(1000);
            animations.AddListener(new AnimationListener()
            {
                AnimationEnd = (animation) =>
                {
                    flipOutView.Rotation = 0;
                    flipInView.Rotation = -90;
                    listener.onFlipped(this);
                }
            });
        }

        public void SetFlipInDrawable(Drawable drawable)
        {
            flipInView.SetImageDrawable(drawable);
        }

        public void SetFlipOutDrawable(Drawable drawable)
        {
            flipOutView.SetImageDrawable(drawable);
        }

        public void SetFlipDuration(int flipDuration)
        {
            animations.SetDuration(flipDuration);
        }

        public void Flip()
        {
            animations.Start();
        }

        public class AnimationListener : AnimatorListenerAdapter
        {
            public Action<Animator> AnimationEnd { get; set; }
            public override void OnAnimationEnd(Animator animation)
            {
                AnimationEnd?.Invoke(animation);
            }
        }

    }
}