using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using static Session1_Kazan.Model.HolderClass;

namespace Session1_Kazan
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        List<ListViewCells> _list = new List<ListViewCells>();
        List<Department> _departments;
        List<AssetGroup> _assetGroups;
        WebClient webClient = new WebClient();
        public MainPage()
        {
            InitializeComponent();
            webClient.Headers.Add("Content-Type", "application/json");
        }
            


        protected override async void OnAppearing()
        {
            base.OnAppearing();
            pDepartments.Items.Clear();
            pAssetGroups.Items.Clear();
            var departments = await webClient.UploadDataTaskAsync("http://10.0.2.2:49450/Departments", "POST", Encoding.UTF8.GetBytes(""));
            _departments = JsonConvert.DeserializeObject<List<Department>>(Encoding.Default.GetString(departments));
            foreach (var item in _departments)
            {
                pDepartments.Items.Add(item.Name);
            }

            var assetGroups = await webClient.UploadDataTaskAsync("http://10.0.2.2:49450/AssetGroups", "POST", Encoding.UTF8.GetBytes(""));
            _assetGroups = JsonConvert.DeserializeObject<List<AssetGroup>>(Encoding.Default.GetString(assetGroups));
            foreach (var item in _assetGroups)
            {
                pAssetGroups.Items.Add(item.Name);
            }
            populateList();
        }

        private async void populateList()
        {
            _list = await GetList();
            lvAsset.ItemsSource = _list;
        }

        private async Task<List<ListViewCells>> GetList()
        {
            var webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "application/json");
            var response = await webClient.UploadDataTaskAsync("http://10.0.2.2:49450/Assets/GetView", "POST", Encoding.UTF8.GetBytes(""));
            var getList = JsonConvert.DeserializeObject<List<ListViewCells>>(Encoding.Default.GetString(response));
            return getList;
        }

        private void entrySearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (entrySearch.Text.Trim() != string.Empty)
            {
                var getRelevant = (from x in _list
                                   where x.AssetName.ToLower().Contains(entrySearch.Text.ToLower())
                                   select x).ToList();
                lvAsset.ItemsSource = getRelevant;
            }
            else
            {
                lvAsset.ItemsSource = _list;
            }

        }


        private async void btnEdit_Clicked(object sender, EventArgs e)
        {
            ImageButton button = (ImageButton)sender;
            var listViewItem = (StackLayout)button.Parent;
            var label = (StackLayout)listViewItem.Children[1];
            var item = (Label)label.Children[2];
            await Navigation.PushAsync(new EditPage(item.Text));
        }

        private async void btnMove_Clicked(object sender, EventArgs e)
        {
            ImageButton button = (ImageButton)sender;
            var listViewItem = (StackLayout)button.Parent;
            var label = (StackLayout)listViewItem.Children[1];
            var item = (Label)label.Children[2];
            await Navigation.PushAsync(new MovePage(item.Text));
        }

        private async void btnHistory_Clicked(object sender, EventArgs e)
        {
            ImageButton button = (ImageButton)sender;
            var listViewItem = (StackLayout)button.Parent;
            var label = (StackLayout)listViewItem.Children[1];
            var item = (Label)label.Children[2];
            await Navigation.PushAsync(new HistoryPage(item.Text));
        }

        private async void btnAdd_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditPage(string.Empty));
        }

        private void pDepartments_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pAssetGroups.SelectedItem != null)
            {

                var getRelevantAsset = (from x in _list
                                        where x.AssetGroup == pAssetGroups.SelectedItem.ToString() && x.DepartmentName == pDepartments.SelectedItem.ToString()
                                        select x).ToList();
                lvAsset.ItemsSource = getRelevantAsset;
            }
            else if (pAssetGroups.SelectedItem == null)
            {
                var getRelevantAsset = (from x in _list
                                        where x.DepartmentName == pDepartments.SelectedItem.ToString()
                                        select x).ToList();
                lvAsset.ItemsSource = getRelevantAsset;
            }
            else
            {
                lvAsset.ItemsSource = _list;
            }
            
        }

        private void pAssetGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pDepartments.SelectedItem != null)
            {

                var getRelevantAsset = (from x in _list
                                        where x.AssetGroup == pAssetGroups.SelectedItem.ToString() && x.DepartmentName == pDepartments.SelectedItem.ToString()
                                        select x).ToList();
                lvAsset.ItemsSource = getRelevantAsset;
            }
            else if (pDepartments.SelectedItem == null)
            {
                var getRelevantAsset = (from x in _list
                                        where x.AssetGroup == pAssetGroups.SelectedItem.ToString()
                                        select x).ToList();
                lvAsset.ItemsSource = getRelevantAsset;
            }
            else
            {
                lvAsset.ItemsSource = _list;
            }
        }
    }
}
