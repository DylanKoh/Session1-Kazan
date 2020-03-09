using Newtonsoft.Json;
using Session1_Kazan.Model;
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
    public partial class EditPage : ContentPage
    {
        WebClient webClient = new WebClient();
        List<Department> _departments = new List<Department>();
        List<Location> _locations = new List<Location>();
        List<AssetGroup> _assetGroups = new List<AssetGroup>();
        List<Employee> _employees = new List<Employee>();
        List<Asset> _assets = new List<Asset>();
        CurrentAsset CurrentAsset = new CurrentAsset();
        string _assetSN = string.Empty;
        public EditPage()
        {
            webClient.Headers.Add("Content-Type", "application/json");
        }
        public EditPage(string assetSN)
        {
            InitializeComponent();
            _assetSN = assetSN;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPickers();
            if (_assetSN != string.Empty)
            {
                loadAssetDetails();
            }
            else
            {
                var getAssets = await webClient.UploadDataTaskAsync("http://10.0.2.2:49450/Assets", "POST", Encoding.UTF8.GetBytes(""));
                _assets = JsonConvert.DeserializeObject<List<Asset>>(Encoding.Default.GetString(getAssets));
            }

        }

        private async Task LoadPickers()
        {
            #region Getting all the employees and adding them into the picker for Accountable Party
            var employees = await webClient.UploadDataTaskAsync("http://10.0.2.2:49450/Employees", "POST", Encoding.UTF8.GetBytes(""));
            _employees = JsonConvert.DeserializeObject<List<Employee>>(Encoding.Default.GetString(employees));
            foreach (var item in _employees)
            {
                pAccountableParty.Items.Add(item.FirstName + " " + item.LastName);
            }
            #endregion

            #region Getting all the departments and adding them into the picker
            var getDepartments = await webClient.UploadDataTaskAsync("http://10.0.2.2:49450/Departments", "POST", Encoding.UTF8.GetBytes(""));
            _departments = JsonConvert.DeserializeObject<List<Department>>(Encoding.Default.GetString(getDepartments));
            foreach (var item in _departments)
            {
                pDepartment.Items.Add(item.Name);
            }
            #endregion


            #region Getting all the asset groups and adding them into the picker
            var getAssetGroups = await webClient.UploadDataTaskAsync("http://10.0.2.2:49450/AssetGroups", "POST", Encoding.UTF8.GetBytes(""));
            _assetGroups = JsonConvert.DeserializeObject<List<AssetGroup>>(Encoding.Default.GetString(getAssetGroups));
            foreach (var item in _assetGroups)
            {
                pAssetGroup.Items.Add(item.Name);
            }
            #endregion

            #region Getting all the departments' locations and adding them into the picker
            var getLocation = await webClient.UploadDataTaskAsync("http://10.0.2.2:49450/Locations", "POST", Encoding.UTF8.GetBytes(""));
            _locations = JsonConvert.DeserializeObject<List<Location>>(Encoding.Default.GetString(getLocation));
            foreach (var item in _locations)
            {
                pLocation.Items.Add(item.Name);
            }
            #endregion

            if (_assetSN != string.Empty)
            {
                var response = await webClient.UploadDataTaskAsync($"http://10.0.2.2:49450/Assets/Details?assetSN={_assetSN}", "POST", Encoding.UTF8.GetBytes(""));
                CurrentAsset = JsonConvert.DeserializeObject<CurrentAsset>(Encoding.Default.GetString(response));
            }
        }

        private void loadAssetDetails()
        {
            entryAssetName.Text = CurrentAsset.AssetName;
            pDepartment.SelectedItem = CurrentAsset.DepartmentName;
            pLocation.SelectedItem = CurrentAsset.LocationName;
            editorAssetDescription.Text = CurrentAsset.AssetDescription;
            pAssetGroup.SelectedItem = CurrentAsset.AssetGroup.ToString();
            pAccountableParty.SelectedItem = CurrentAsset.AccountableParty.ToString();
            lblAsset.Text = CurrentAsset.AssetSN;
            if (CurrentAsset.WarrantyDate != null)
            {
                dpExpiredWarranty.Date = Convert.ToDateTime(CurrentAsset.WarrantyDate);
            }
            pDepartment.IsEnabled = false;
            pLocation.IsEnabled = false;
            pAssetGroup.IsEnabled = false;
            entryAssetName.IsEnabled = false;

        }

        private void btnSubmit_Clicked(object sender, EventArgs e)
        {

        }

        private async void btnCancel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        


        private void pAssetGroup_SelectedIndexChanged(object sender, EventArgs e)
        {

            var getDepartmentID = (from x in _departments
                                   where x.Name == pDepartment.SelectedItem.ToString()
                                   select x.ID).FirstOrDefault();

            var getAssetGroupID = (from x in _assetGroups
                                   where x.Name == pAssetGroup.SelectedItem.ToString()
                                   select x.ID).FirstOrDefault();

            var newDepartmentID = getDepartmentID.ToString().PadLeft(2, '0');
            var newAssetGroupID = getAssetGroupID.ToString().PadLeft(2, '0');

            var lastAsset = (from x in _assets
                             where x.AssetSN.Contains($"{newDepartmentID}/{newAssetGroupID}")
                             orderby x.AssetSN descending
                             select x.AssetSN).FirstOrDefault();
            if (lastAsset != null)
            {
                var newNumber = (Int64.Parse(lastAsset.Split('/')[2]) + 1).ToString().PadLeft(4, '0');
                lblAsset.Text = $"{newDepartmentID}/{newAssetGroupID}/{newNumber}";
            }
            else
            {
                lblAsset.Text = $"{newDepartmentID}/{newAssetGroupID}/0001";
            }


        }

        

        

        private async void pDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_assetSN == string.Empty)
            {
                pLocation.Items.Clear();
                lblAsset.Text = string.Empty;
                var getDepartmentID = (from x in _departments
                                       where x.Name == pDepartment.SelectedItem.ToString()
                                       select x.ID).FirstOrDefault();
                var DepartmentLocation = await webClient.UploadDataTaskAsync($"http://10.0.2.2:49450/Departments/GetDepartmentLocations?DepartmentID={getDepartmentID}",
                    "POST", Encoding.UTF8.GetBytes(""));
                var GetDepartmentLocation = JsonConvert.DeserializeObject<List<DepartmentLocation>>(Encoding.Default.GetString(DepartmentLocation));

                foreach (var item in GetDepartmentLocation)
                {
                    var getLocation = (from x in _locations
                                       where x.ID == item.LocationID
                                       select x.Name).FirstOrDefault();
                    pLocation.Items.Add(getLocation);
                }
            }
        }
    }
}