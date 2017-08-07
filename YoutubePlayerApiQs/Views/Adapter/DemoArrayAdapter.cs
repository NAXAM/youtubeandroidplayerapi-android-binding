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



namespace YoutubePlayerApiQs.Views.Adapter
{
    public class DemoArrayAdapter : ArrayAdapter<DemoListViewItem>
    {
        private LayoutInflater inflater;
        public DemoArrayAdapter(Context context, int textViewResourceId, List<DemoListViewItem> objects) : base(context, textViewResourceId, objects)
        {
            inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView == null)
            {
                convertView = inflater.Inflate(Resource.Layout.list_item, null);
            }
            TextView textView = convertView.FindViewById<TextView>(Resource.Id.list_item_text);
            textView.Text = GetItem(position).GetTitle();
            TextView disabledText = convertView.FindViewById<TextView>(Resource.Id.list_item_disabled_text);
            disabledText.Text = GetItem(position).GetDisabledText();

            if (IsEnabled(position))
            {
                disabledText.Visibility = ViewStates.Invisible;
                textView.SetTextColor(Android.Graphics.Color.White);
            }
            else
            {
                disabledText.Visibility = ViewStates.Visible;
                textView.SetTextColor(Android.Graphics.Color.Gray);
            }
            return convertView;
        }

        public override bool AreAllItemsEnabled()
        {
            return true;
        }

        public override bool IsEnabled(int position)
        {
            return GetItem(position).IsEnabled();
        }

        public bool AnyDisabled()
        {
            for (int i = 0; i < Count; i++)
            {
                if (!IsEnabled(i))
                {
                    return true;
                }
            }
            return false;
        }
    }
}