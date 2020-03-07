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
    public partial class EditPage : ContentPage
    {
        WebClient webClient = new WebClient();
        List<Department> _departments;
        List<Location> _locations;
        List<AssetGroup> _assetGroups;
        List<Employee> _employees;
        string _assetSN;
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
            #region Getting all the departments and adding them into the picker
            var getDepartments = await webClient.UploadDataTaskAsync("http://10.0.2.2:49450/Departments", "POST", Encoding.UTF8.GetBytes(""));
            _departments = JsonConvert.DeserializeObject<List<Department>>(Encoding.Default.GetString(getDepartments));
            foreach (var item in _departments)
            {
                pDepartment.Items.Add(item.Name);
            }
            #endregion

            #region Getting all the department's locations and adding them into the picker
            var getDepartmentLocations = await webClient.UploadDataTaskAsync("http://10.0.2.2:49450/Locations", "POST", Encoding.UTF8.GetBytes(""));
            _locations = JsonConvert.DeserializeObject<List<Location>>(Encoding.Default.GetString(getDepartmentLocations));
            foreach (var item in _locations)
            {
                pLocation.Items.Add(item.Name);
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

            #region Getting all the employees and adding them into the picker for Accountable Party
            var employees = await webClient.UploadDataTaskAsync("http://10.0.2.2:49450/Employees", "POST", Encoding.UTF8.GetBytes(""));
            _employees = JsonConvert.DeserializeObject<List<Employee>>(Encoding.Default.GetString(employees));
            foreach (var item in _employees)
            {
                pAccountableParty.Items.Add(item.FirstName + " " + item.LastName);
            }
            #endregion

            if (_assetSN != string.Empty)
            {
                getAssetDetails();
            }
            
        }

        private async void getAssetDetails()
        {
            var response = await webClient.UploadDataTaskAsync($"http://10.0.2.2:49450/Assets/Details?assetSN={_assetSN}", "POST", Encoding.UTF8.GetBytes(""));
            var getClass = JsonConvert.DeserializeObject<CurrentAsset>(Encoding.Default.GetString(response));
            entryAssetName.Text = getClass.AssetName;
            pDepartment.SelectedItem = getClass.DepartmentName;
            pLocation.SelectedItem = getClass.LocationName;
            editorAssetDescription.Text = getClass.AssetDescription;
            pAssetGroup.SelectedItem = getClass.AssetGroup;
            pAccountableParty.SelectedItem = getClass.AccountableParty;
            lblAsset.Text = getClass.AssetSN;
            if (getClass.WarrantyDate != null)
            {
                dpExpiredWarranty.Date = Convert.ToDateTime(getClass.WarrantyDate);
            }
            pDepartment.IsEnabled = false;
            pLocation.IsEnabled = false;
            pAssetGroup.IsEnabled = false;
            entryAssetName.IsEnabled = false;
            
        }

        private async void btnSubmit_Clicked(object sender, EventArgs e)
        {

        }

        private async void btnCancel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void pDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pAssetGroup.SelectedItem == null)
            {
                lblAsset.Text = string.Empty;
            }
            else
            {

            }
        }

        private void pLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void pAssetGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pDepartment.SelectedItem == null)
            {
                lblAsset.Text = string.Empty;
            }
            else
            {

            }
        }
    }
}