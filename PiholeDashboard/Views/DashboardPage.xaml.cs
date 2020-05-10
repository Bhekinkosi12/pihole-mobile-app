﻿using System;
using System.ComponentModel;
using Xamarin.Forms;
using System.Text.Json;
using PiholeDashboard.Models;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace PiholeDashboard.Views
{
    [DesignTimeVisible(false)]
    public partial class DashboardPage : ContentPage
    {
        public PiHoleConfig config { get; } = new PiHoleConfig();
        public Summary summary { get; private set; } = new Summary();

        public DashboardPage()
        {
            InitializeComponent();
            BindingContext = this;

            OnPropertyChanged(nameof(summary));
            OnPropertyChanged(nameof(config));
        }

        async Task ErrorAlert(string customMsg)
        {
            var wantsHelp = await DisplayAlert("Error", customMsg, "Open Help", "OK");
            if (wantsHelp)
            {
                await Navigation.PushModalAsync(new NavigationPage(new HelpModal()));
            }
        }

        async void AddItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(new NewItemPage()));
        }

        async void RefreshData_Clicked(object sender, EventArgs e)
        {
            try
            {
                var uri = $"{App.Current.Properties["Uri"]}/admin/api.php?summaryRaw";

                HttpClient _client = new HttpClient();
                _client.Timeout = TimeSpan.FromSeconds(5);
                var res = await _client.GetAsync(uri);

                if (res.IsSuccessStatusCode)
                {
                    var content = await res.Content.ReadAsStringAsync();
                    summary = JsonSerializer.Deserialize<Summary>(content);
                    // Property Changed!
                    OnPropertyChanged(nameof(summary));       
                }
                else
                {
                    string errStr = "Error fetching Pihole Summary (err=3)";
                    await ErrorAlert(errStr);
                    Console.WriteLine($"{errStr}");
                }
            }
            catch (Exception err)
            {
                string errStr = "Could not connect to PiHole service. Ensure your URI is correct. (err=4)";
                await ErrorAlert(errStr);
                Console.WriteLine($"{errStr}: {err}");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Set Uri
            if (App.Current.Properties.ContainsKey("Uri"))
            {
                var url = App.Current.Properties["Uri"] as string;
                config.Uri = url;
            }

            // Set ApiKey
            if (App.Current.Properties.ContainsKey("ApiKey"))
            {
                var key = App.Current.Properties["ApiKey"] as string;
                config.ApiKey = key;
            }

            // Refresh
            OnPropertyChanged(nameof(config));
        }
    }
}