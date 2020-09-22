using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Cecs475.BoardGames.WpfApp
{
    /// <summary>
    /// Interaction logic for LoadingPage.xaml
    /// </summary>
    public partial class LoadingPage : Window
    {
        public LoadingPage()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RestClient client = new RestClient("https://cecs475-boardamges.herokuapp.com/api/games");
            RestRequest request = new RestRequest(Method.GET);
            var response = await client.ExecuteAsync(request);
            JArray jArray = JArray.Parse(response.Content);
            IEnumerable<JToken> gameFiles = jArray.Select(t => t["Files"]);

            foreach(var game in gameFiles.Children())
            {
                WebClient webClient = new WebClient();
                var fileName = (string)game["FileName"];
                var url = new Uri((string)game["Url"]);
                var publicKey = (string)game["PublicKey"];
                var version = (string)game["Version"];
               
                await webClient.DownloadFileTaskAsync(url, @"games\" + fileName);

                fileName = fileName.Replace(".dll", "");
                Assembly.Load(fileName + ", Version=" + version + ", Culture=neutral, PublicKeyToken=" + publicKey);
            }

            new GameChoiceWindow().Show();
            Close();
        }
    }
}
