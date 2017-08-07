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
using Android.Graphics.Drawables;

using Java.Util;
using Android.Util;

namespace YoutubePlayerApiQs.Views.Ui
{
    public class ImageWallView : ViewGroup
    {
        private Context context;
        private System.Random random;

        private int imageHeight;
        private int imageWidth;
        private int interImagePadding;

        private ImageView[] images;
        private List<int> unInitializedImages;

        private int numberOfColumns;
        private int numberOfRows;


        public ImageWallView(Context context, int imageWidth, int imageHeight, int interImagePadding) : base(context)
        {
            this.context = context;
            random = new System.Random();

            this.imageWidth = imageWidth;
            this.imageHeight = imageHeight;
            this.interImagePadding = interImagePadding;
            this.images = new ImageView[0];
            this.unInitializedImages = new List<int>();
        }

        protected override void OnSizeChanged(int width, int height, int oldWidth, int oldHeight)
        {
            // create enough columns to fill view's width, plus an extra column at either side to allow
            // images to have diagonal offset across the screen.
            numberOfColumns = width / (imageWidth + interImagePadding) + 2;
            // create enough rows to fill the view's height (adding an extra row at bottom if necessary).
            numberOfRows = height / (imageHeight + interImagePadding);
            numberOfRows += (height % (imageHeight + interImagePadding) == 0) ? 0 : 1;

            if ((numberOfRows <= 0) || (numberOfColumns <= 0))
            {
                throw new Exception("Error creating an ImageWallView with " + numberOfRows
                    + " rows and " + numberOfColumns + " columns. Both values must be greater than zero.");
            }

            if (images.Length < (numberOfColumns * numberOfRows))
            {
                // khong tin tuong lam
                var tem = new ImageView[numberOfRows * numberOfColumns];
                for (int i = 0; i < images.Length; i++)
                {
                    if (i == numberOfRows * numberOfColumns)
                        break;
                    tem[i] = images[i];
                }
                images = tem;
                //images = Arrays.CopyOf((ImageView)images, numberOfRows * numberOfColumns);
            }

            RemoveAllViews();
            for (int col = 0; col < numberOfColumns; col++)
            {
                for (int row = 0; row < numberOfRows; row++)
                {
                    int elementIdx = GetElementIdx(col, row);
                    if (images[elementIdx] == null)
                    {
                        ImageView thumbnail = new ImageView(context);
                        thumbnail.LayoutParameters = new LayoutParams(imageWidth, imageHeight);
                        images[elementIdx] = thumbnail;
                        unInitializedImages.Add(elementIdx);
                    }
                    AddView(images[elementIdx]);
                }
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            DisplayMetrics displayMetrics = Resources.DisplayMetrics;
            int width = GetDefaultSize(displayMetrics.WidthPixels, widthMeasureSpec);
            int height = GetDefaultSize(displayMetrics.HeightPixels, heightMeasureSpec);
            SetMeasuredDimension(width, height);
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {

            for (int col = 0; col < numberOfColumns; col++)
            {
                for (int row = 0; row < numberOfRows; row++)
                {
                    int x = (col - 1) * (imageWidth + interImagePadding) + (row * (imageWidth / numberOfRows));
                    int y = row * (imageHeight + interImagePadding);
                    images[col * numberOfRows + row].Layout(x, y, x + imageWidth, y + imageHeight);
                }
            }
        }

        public int GetXPosition(int col, int row)
        {
            return images[GetElementIdx(col, row)].Left;
        }

        public int GetYPosition(int col, int row)
        {
            return images[GetElementIdx(col, row)].Top;
        }

        private int GetElementIdx(int col, int row)
        {
            return (col * numberOfRows) + row;
        }

        public void HideImage(int col, int row)
        {
            images[GetElementIdx(col, row)].Visibility = ViewStates.Invisible;
        }

        public void ShowImage(int col, int row)
        {
            images[GetElementIdx(col, row)].Visibility = ViewStates.Invisible;
        }

        public void SetImageDrawable(int col, int row, Android.Graphics.Drawables.Drawable drawable)
        {
            int elementIdx = GetElementIdx(col, row);
            // manually boxing elementIdx to avoid calling List.remove(int position) method overload
            unInitializedImages.Remove(elementIdx);
            images[elementIdx].SetImageDrawable(drawable);
        }

        public Android.Graphics.Drawables.Drawable GetImageDrawable(int col, int row)
        {
            int elementIdx = GetElementIdx(col, row);
            return images[elementIdx].Drawable;
        }

        public Pair GetNextLoadTarget()
        {
            int nextElement;
            do
            {
                if (unInitializedImages.Count == 0)
                {
                    // Don't choose the first or last columns (since they are partly hidden)
                    nextElement = random.Next((numberOfColumns - 2) * numberOfRows) + numberOfRows;
                }
                else
                {
                    nextElement = unInitializedImages[random.Next(unInitializedImages.Count)];
                }
            } while (images[nextElement].Visibility != ViewStates.Visible);

            int col = nextElement / numberOfRows;
            int row = nextElement % numberOfRows;
            return new Pair(col, row);
        }

        public bool AllImagesLoaded()
        {

            return unInitializedImages.Count == 0;
        }
    }
}