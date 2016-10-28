using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.Text;

namespace TideApp
{
    [Activity(Label = "Tidal Chart", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;

        private DatePicker datePicker;
        private Button refreshButton;
        private TextView highTideOne;
        private TextView highTideTwo;
        private TextView lowTideOne;
        private TextView lowTideTwo;
        private TextView sunTextView;
        private TextView highTideLabel;
        private Spinner spinner;
        private Core.TideData.Location location;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            WireObjects();
            this.datePicker.DateTime = DateTime.Now;
            location = Core.TideData.Location.BowleysBar;
            this.DoUpdate();
            
        }

        private void WireObjects()
        {
            this.refreshButton = FindViewById<Button>(Resource.Id.RefreshButton);
            refreshButton.Click += (object sender, EventArgs e) => this.DoUpdate();
            this.datePicker = FindViewById<DatePicker>(Resource.Id.datePicker1);
            this.highTideOne = FindViewById<TextView>(Resource.Id.highTide1); ;
            this.highTideTwo = FindViewById<TextView>(Resource.Id.highTide2);
            this.lowTideOne = FindViewById<TextView>(Resource.Id.lowTide1);
            this.lowTideTwo = FindViewById<TextView>(Resource.Id.lowTide2);
            this.sunTextView = FindViewById<TextView>(Resource.Id.sunTextView);
            this.highTideLabel = FindViewById<TextView>(Resource.Id.highTideLabel);
            spinner = FindViewById<Spinner>(Resource.Id.locations);

            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
            var adapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.location_array, Android.Resource.Layout.SimpleSpinnerItem);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;

        }

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var v = spinner.GetItemAtPosition(e.Position).ToString();
            location = Core.TideData.GetLocationId(v);
            string toast = string.Format("The location chaged to {0}", v);
            Toast.MakeText(this, toast, ToastLength.Short).Show();
            this.DoUpdate();
        }

        private async void DoUpdate()
        {
            var now = datePicker.DateTime;
            Task<Core.DailyData> loadTask = Core.TideData.LoadTideData(now, location);
            var results = await loadTask;
            highTideLabel.Text = $"High Tides:    {results.Date.ToShortDateString()}";
            highTideOne.Text = GetTideText(results.FirstHighTide, results);
            highTideTwo.Text = GetTideText(results.SecondHighTide, results);
            lowTideOne.Text = GetTideText(results.FirstLowTide, results);
            lowTideTwo.Text = GetTideText(results.SecondLowTide, results);
            sunTextView.Text = $"Rise : {results.SunRise.Value.ToShortTimeString()}  Set : {results.SunSet.Value.ToShortTimeString()}";
        }

        private static string GetTideText(DateTime? time, Core.DailyData data)
        {
            if (!time.HasValue)
                return string.Empty;

            var str = time.Value.ToShortTimeString();

            if (data.Date.Date == DateTime.Now.Date && data.Times.Any())
            {
                var nextTime = data.Times.Where(x => x.HasValue && x.Value > DateTime.Now).OrderBy(x => x.Value).FirstOrDefault();
                if (nextTime == time)
                {
                    str = $"{str} - *** Next Change ***";
                }
            }

            return str;
        }
    }
}

