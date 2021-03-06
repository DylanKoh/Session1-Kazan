﻿using Newtonsoft.Json;
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
        List<Department> _departments;
        List<Location> _locations;
        List<AssetGroup> _assetGroups;
        List<Employee> _employees;
        List<Asset> _assets;
        List<DepartmentLocation> _departLocations;
        List<AssetTranferLog> _assetTranferLogs;
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

            var getAssets = await webClient.UploadDataTaskAsync("http://10.0.2.2:49450/Assets", "POST", Encoding.UTF8.GetBytes(""));
            _assets = JsonConvert.DeserializeObject<List<Asset>>(Encoding.Default.GetString(getAssets));


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

            var getDepartmentLocations = await webClient.UploadDataTaskAsync($"http://10.0.2.2:49450/Departments/DepartmentLocations", "POST", Encoding.UTF8.GetBytes(""));
            _departLocations = JsonConvert.DeserializeObject<List<DepartmentLocation>>(Encoding.Default.GetString(getDepartmentLocations));

            var getTransferLogs = await webClient.UploadDataTaskAsync($"http://10.0.2.2:49450/AssetTransferLogs", "POST", Encoding.UTF8.GetBytes(""));
            _assetTranferLogs = JsonConvert.DeserializeObject<List<AssetTranferLog>>(Encoding.Default.GetString(getTransferLogs));
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

        private async void btnSubmit_Clicked(object sender, EventArgs e)
        {
            var getEmployeeID = (from x in _employees
                                 where pAccountableParty.SelectedItem.ToString().Equals(x.FirstName + " " + x.LastName)
                                 select x.ID).FirstOrDefault();
            var getDepartmentID = (from x in _departments
                                   where x.Name == pDepartment.SelectedItem.ToString()
                                   select x.ID).FirstOrDefault();

            var getLocationID = (from x in _locations
                                 where x.Name == pLocation.SelectedItem.ToString()
                                 select x.ID).FirstOrDefault();

            var getDepartmentLocations = (from x in _departLocations
                                          where x.DepartmentID == getDepartmentID && x.LocationID == getLocationID
                                          select x.ID).FirstOrDefault();

            var getAssetGroupID = (from x in _assetGroups
                                   where x.Name == pAssetGroup.SelectedItem.ToString()
                                   select x.ID).FirstOrDefault();

            if (_assetSN == string.Empty)
            {
                var newAsset = new Asset()
                {
                    AssetName = entryAssetName.Text,
                    AssetSN = lblAsset.Text,
                    EmployeeID = getEmployeeID,
                    Description = editorAssetDescription.Text.Trim(),
                    DepartmentLocationID = 1,
                    AssetGroupID = getAssetGroupID,
                    WarrantyDate = dpExpiredWarranty.Date
                };

                var jsonData = JsonConvert.SerializeObject(newAsset);
                using (var webClientSend = new WebClient())
                {
                    webClientSend.Headers.Add("Content-Type", "application/json");
                    webClientSend.Encoding = Encoding.UTF8;
                    var response = await webClientSend.UploadDataTaskAsync("http://10.0.2.2:49450/Assets/Create", "POST", Encoding.UTF8.GetBytes(jsonData));
                    var getStringResponse = Encoding.Default.GetString(response);
                    if (getStringResponse == "\"Asset created successful!\"")
                    {
                        await DisplayAlert("Add Asset", getStringResponse, "Ok");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await DisplayAlert("Add Asset", "Unable to add Asset. Please check your details and try again", "Ok");
                    }
                }


            }
            else
            {
                var updateAsset = (from x in _assets
                                   where x.AssetSN == lblAsset.Text
                                   select x).FirstOrDefault();
                updateAsset.Description = editorAssetDescription.Text.Trim();
                updateAsset.WarrantyDate = dpExpiredWarranty.Date;
                updateAsset.EmployeeID = getEmployeeID;
                var jsonData = JsonConvert.SerializeObject(updateAsset);
                using (var webClientEdit = new WebClient())
                {
                    webClientEdit.Headers.Add("Content-Type", "application/json");
                    webClientEdit.Encoding = Encoding.UTF8;
                    var response = await webClientEdit.UploadDataTaskAsync("http://10.0.2.2:49450/Assets/Edit", "POST", Encoding.UTF8.GetBytes(jsonData));
                    var getStringResponse = Encoding.Default.GetString(response);
                    if (getStringResponse == "\"Edit asset successful!\"")
                    {
                        await DisplayAlert("Add Asset", getStringResponse, "Ok");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await DisplayAlert("Add Asset", "Unable to edit Asset. Please check your details and try again", "Ok");
                    }
                }
            }
        }

        private async void btnCancel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }




        private async void pAssetGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pDepartment.SelectedItem == null)
            {
                await DisplayAlert("Asset SN", "Please select your department and reselect asset group to auto generate an Asset SN!", "Ok");
            }
            else
            {
                if (_assetSN == string.Empty)
                {
                    var getDepartmentID = (from x in _departments
                                           where x.Name == pDepartment.SelectedItem.ToString()
                                           select x.ID).FirstOrDefault();

                    var getAssetGroupID = (from x in _assetGroups
                                           where x.Name == pAssetGroup.SelectedItem.ToString()
                                           select x.ID).FirstOrDefault();

                    var newDepartmentID = getDepartmentID.ToString().PadLeft(2, '0');
                    var newAssetGroupID = getAssetGroupID.ToString().PadLeft(2, '0');

                    List<string> usedAssetSN = new List<string>();
                    usedAssetSN.Add((from x in _assets
                                     where x.AssetSN.Contains($"{newDepartmentID}/{newAssetGroupID}")
                                     orderby x.AssetSN descending
                                     select x.AssetSN).FirstOrDefault());
                    usedAssetSN.Add((from x in _assetTranferLogs
                                     where x.FromAssetSN.Contains($"{newDepartmentID}/{newAssetGroupID}")
                                     orderby x.FromAssetSN descending
                                     select x.FromAssetSN).FirstOrDefault());
                    usedAssetSN.OrderByDescending(x => x);
                    if (usedAssetSN.First() != null)
                    {
                        var newNumber = (Int64.Parse(usedAssetSN.First().Split('/')[2]) + 1).ToString().PadLeft(4, '0');
                        lblAsset.Text = $"{newDepartmentID}/{newAssetGroupID}/{newNumber}";
                    }
                    else
                    {
                        lblAsset.Text = $"{newDepartmentID}/{newAssetGroupID}/0001";
                    }
                }
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

        private async void btnCapture_Clicked(object sender, EventArgs e)
        {
        }

        private void btnBrowse_Clicked(object sender, EventArgs e)
        {

        }
    }
}