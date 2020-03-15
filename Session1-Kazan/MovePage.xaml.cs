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
    public partial class MovePage : ContentPage
    {
        string _assetSN;
        List<Asset> _assets;
        List<Department> _departments;
        List<Location> _locations;
        List<DepartmentLocation> _departmentLocations;
        List<AssetTranferLog> _assetTranferLogs;
        public MovePage(string assetSN)
        {
            InitializeComponent();
            _assetSN = assetSN;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("Content-Type", "application/json");
                var getResponse = await webClient.UploadDataTaskAsync($"http://10.0.2.2:49450/Assets/Details?assetSN={_assetSN}", "POST", Encoding.UTF8.GetBytes(""));
                var selectedAsset = JsonConvert.DeserializeObject<CurrentAsset>(Encoding.Default.GetString(getResponse));
                entryAssetName.Text = selectedAsset.AssetName;
                entryAssetSN.Text = selectedAsset.AssetSN;
                entryCurrentDepartment.Text = selectedAsset.DepartmentName;
                var getAssetGroupID = _assetSN.Split('/')[1];
                entryNewAssetSN.Text = $"??/{getAssetGroupID}/????";
            }
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("Content-Type", "application/json");
                var getResponse = await webClient.UploadDataTaskAsync($"http://10.0.2.2:49450/Departments", "POST", Encoding.UTF8.GetBytes(""));
                _departments = JsonConvert.DeserializeObject<List<Department>>(Encoding.Default.GetString(getResponse));
                foreach (var item in _departments)
                {
                    pDestinationDepartment.Items.Add(item.Name);
                }

                pDestinationDepartment.Items.Remove(entryCurrentDepartment.Text);

                var getLocation = await webClient.UploadDataTaskAsync("http://10.0.2.2:49450/Locations", "POST", Encoding.UTF8.GetBytes(""));
                _locations = JsonConvert.DeserializeObject<List<Location>>(Encoding.Default.GetString(getLocation));
            }
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("Content-Type", "application/json");
                var getResponse = await webClient.UploadDataTaskAsync($"http://10.0.2.2:49450/Assets", "POST", Encoding.UTF8.GetBytes(""));
                _assets = JsonConvert.DeserializeObject<List<Asset>>(Encoding.Default.GetString(getResponse));

                var getDepartmentLocations = await webClient.UploadDataTaskAsync($"http://10.0.2.2:49450/Departments/DepartmentLocations", "POST", Encoding.UTF8.GetBytes(""));
                _departmentLocations = JsonConvert.DeserializeObject<List<DepartmentLocation>>(Encoding.Default.GetString(getDepartmentLocations));

                var getTransferLogs = await webClient.UploadDataTaskAsync($"http://10.0.2.2:49450/AssetTransferLogs", "POST", Encoding.UTF8.GetBytes(""));
                _assetTranferLogs = JsonConvert.DeserializeObject<List<AssetTranferLog>>(Encoding.Default.GetString(getTransferLogs));
            }
        }

        private async void pDestinationDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("Content-Type", "application/json");
                pDestinationLocation.Items.Clear();
                var getDepartmentID = (from x in _departments
                                       where x.Name == pDestinationDepartment.SelectedItem.ToString()
                                       select x.ID).FirstOrDefault();
                var DepartmentLocation = await webClient.UploadDataTaskAsync($"http://10.0.2.2:49450/Departments/GetDepartmentLocations?DepartmentID={getDepartmentID}",
                    "POST", Encoding.UTF8.GetBytes(""));
                var GetDepartmentLocation = JsonConvert.DeserializeObject<List<DepartmentLocation>>(Encoding.Default.GetString(DepartmentLocation));

                foreach (var item in GetDepartmentLocation)
                {
                    var getLocation = (from x in _locations
                                       where x.ID == item.LocationID
                                       select x.Name).FirstOrDefault();
                    pDestinationLocation.Items.Add(getLocation);
                }
            }


        }

        private void pDestinationLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            var getNewDepartmentID = (from x in _departments
                                      where x.Name == pDestinationDepartment.SelectedItem.ToString()
                                      select x.ID).FirstOrDefault();
            var getLocationID = (from x in _locations
                                 where x.Name == pDestinationLocation.SelectedItem.ToString()
                                 select x.ID).FirstOrDefault();
            var getAssetID = (from x in _assets
                              where x.AssetSN == entryAssetSN.Text
                              select x.ID).FirstOrDefault();
            var getDepartmentLocationID = (from x in _departmentLocations
                                           where x.DepartmentID == getNewDepartmentID && x.LocationID == getLocationID
                                           select x.ID).FirstOrDefault();

            var newDepartmentID = getNewDepartmentID.ToString().PadLeft(2, '0');
            var assetGroupID = entryNewAssetSN.Text.Split('/')[1];

            var getPossiblePrevious = (from x in _assetTranferLogs
                                       where x.AssetID == getAssetID && x.FromDepartmentLocationID == getDepartmentLocationID
                                       select x.FromAssetSN).FirstOrDefault();
            if (getPossiblePrevious != null)
            {
                entryNewAssetSN.Text = getPossiblePrevious;
            }
            else
            {
                List<string> usedAssetSN = new List<string>();
                usedAssetSN.Add((from x in _assets
                                 where x.AssetSN.Contains($"{newDepartmentID}/{assetGroupID}")
                                 orderby x.AssetSN descending
                                 select x.AssetSN).FirstOrDefault());
                usedAssetSN.Add((from x in _assetTranferLogs
                                 where x.FromAssetSN.Contains($"{newDepartmentID}/{assetGroupID}")
                                 orderby x.FromAssetSN descending
                                 select x.FromAssetSN).FirstOrDefault());
                usedAssetSN.OrderByDescending(x => x);
                if (usedAssetSN.First() != null)
                {
                    var newNumber = (Int64.Parse(usedAssetSN.First().Split('/')[2]) + 1).ToString().PadLeft(4, '0');
                    entryNewAssetSN.Text = $"{newDepartmentID}/{assetGroupID}/{newNumber}";
                }
                else
                {
                    entryNewAssetSN.Text = $"{newDepartmentID}/{assetGroupID}/0001";
                }
            }
        }

        private void btnSubmit_Clicked(object sender, EventArgs e)
        {

        }

        private async void btnCancel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}