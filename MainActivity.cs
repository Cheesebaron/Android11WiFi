using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Net.Wifi;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;

namespace Android11Wifi
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private const int AddWifiSettingsRequestCode = 4242;
        private EditText _ssid;
        private EditText _psk;
        private Button _connect;
        private TextView _result;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            _ssid = FindViewById<EditText>(Resource.Id.ssid);
            _psk = FindViewById<EditText>(Resource.Id.psk);
            _connect = FindViewById<Button>(Resource.Id.button_connect);
            _result = FindViewById<TextView>(Resource.Id.result);

            _connect.Click += OnConnect;
        }

        private void OnConnect(object sender, EventArgs e)
        {
            var ssid = _ssid.Text;
            var psk = _psk.Text;
            
            AddWifi(ssid, psk);
        }

        private void AddWifi(string ssid, string psk)
        {
            var intent = new Intent("android.settings.WIFI_ADD_NETWORKS");
            var bundle = new Bundle();
            bundle.PutParcelableArrayList("android.provider.extra.WIFI_NETWORK_LIST",
                new List<IParcelable>
                {
                    new WifiNetworkSuggestion.Builder()
                        .SetSsid(ssid)
                        .SetWpa2Passphrase(psk)
                        .Build()
                });

            intent.PutExtras(bundle);
            
            StartActivityForResult(intent, AddWifiSettingsRequestCode);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            
            if (requestCode == AddWifiSettingsRequestCode)
            {
                if (data != null && data.HasExtra("android.provider.extra.WIFI_NETWORK_RESULT_LIST"))
                {
                    var extras =
                        data.GetIntegerArrayListExtra("android.provider.extra.WIFI_NETWORK_RESULT_LIST")
                            ?.Select(i => i.IntValue()).ToArray() ?? new int[0];

                    if (extras.Length > 0)
                    {
                        var ok = extras.Select(GetResultFromCode).All(r => r == Result.Ok);
                        _result.Text = $"Result {ok}";
                        return;
                    }
                }

                _result.Text = $"Result {resultCode == Result.Ok}";
            }
        }

        private static Result GetResultFromCode(int code) =>
            code switch
            {
                0 => Result.Ok,
                2 => Result.Ok,
                _ => Result.Canceled
            };
    }
}