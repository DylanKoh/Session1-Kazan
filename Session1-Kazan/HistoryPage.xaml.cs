using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Session1_Kazan.Model.HolderClass;

namespace Session1_Kazan
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HistoryPage : ContentPage
    {
        List<Asset> _assets;
        string _assetSN;
        public HistoryPage(string assetSN)
        {
            InitializeComponent();
            _assetSN = assetSN;
        }
        

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            using (var webClient = new WebClient())
            {
                var response = await webClient.UploadDataTaskAsync("http://10.0.2.2:49450/Assets", "POST", Encoding.UTF8.GetBytes(""));
                _assets = JsonConvert.DeserializeObject<List<Asset>>(Encoding.Default.GetString(response));
            }
            var getAssetID = (from x in _assets
                              where x.AssetSN == _assetSN
                              select x.ID).First();

            using (var webClient = new WebClient())
            {
                var response = await webClient.UploadDataTaskAsync($"http://10.0.2.2:49450/AssetTransferLogs/GetAssetLogs?assetID={getAssetID}", "POST", Encoding.UTF8.GetBytes(""));
                var getString = Encoding.Default.GetString(response);
                if (getString == "\"No details available\"")
                {
                    lvTransfer.IsVisible = false;
                    lblNull.IsVisible = true;
                }
                else
                {
                    var listSource = JsonConvert.DeserializeObject<List<History>>(getString);
                    lvTransfer.ItemsSource = listSource;
                }
            }
        }

        private async void btnBack_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}