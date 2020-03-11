using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Session1_Kazan
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HistoryPage : ContentPage
    {
        
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

            }
        }

        private void btnBack_Clicked(object sender, EventArgs e)
        {

        }
    }
}