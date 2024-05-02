using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TwitchLib.Api;
using NHttp;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;



namespace TwitchGFL
{
    public partial class Form1 : Form
    {
        private HttpServer WebServer;
        //
        private readonly string ClientId = Properties.Settings.Default.ClientId;
        private readonly string ClientSecret = Properties.Settings.Default.ClientSecret;
        private readonly List<string> Scopes = new List<string>
        { "chat:read"};
        private readonly string RedirectUrl = "http://localhost";
        private string CachedOwnerOfChannelAccessToken = "zzzzz";
        TwitchAPI TheTwitchAPI;
        List<string> Bans = new List<string>();
        bool isAutoMode = false;



        public event EventHandler TwitchApiReady;




        public Form1(bool _isAutoMode = false)
        {
            InitializeComponent();
            this.Shown += Form1_Shown;
            this.FormClosing += Form1_FormClosing;
            dateTimeFrom.Text = DateTime.Now.AddDays(-1).ToString("dd.MM.yyyy HH:mm");
            dateTimeUntil.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            numericUpDown1.Value = int.Parse(Properties.Settings.Default.MinimumViews);
            if (!string.IsNullOrEmpty(Properties.Settings.Default.GrabTopVideos))
            {
                numericUpDown2.Value = int.Parse(Properties.Settings.Default.GrabTopVideos);
            }
            else
            {
                numericUpDown2.Value = 10;
            }

            LoadBans();
            AuthApp();

            if(_isAutoMode)
            {
                isAutoMode = true;
                TwitchApiReady += Form1_TwitchApiReady;
            }
        }

        private async void Form1_TwitchApiReady(object sender, EventArgs e)
        {
            DateTime fromDateTime = DateTime.Now.AddDays(-1);
            DateTime toDateTime = DateTime.Now;
            GenerateClips(fromDateTime, toDateTime);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(WebServer != null)
            {
                WebServer.Stop();
                WebServer.Dispose();
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            InitWebServer();
        }


        void AuthApp()
        {
            var authUrl = $"https://id.twitch.tv/oauth2/authorize?response_type=code&client_id={ClientId}&redirect_uri={RedirectUrl}&scope={string.Join("+", Scopes)}";
            System.Diagnostics.Process.Start(authUrl);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            //Grab Clips
            if (TheTwitchAPI == null)
            {
                return;
            }


            DateTime fromDateTime = dateTimeFrom.Value;
            DateTime toDateTime = dateTimeUntil.Value;



            GenerateClips(fromDateTime, toDateTime);
        }


        public async void GenerateClips(DateTime fromDate, DateTime toDate)
        {
            string saveFile = "";
            int viewsLimit = int.Parse(Properties.Settings.Default.MinimumViews);
            var hp = await TheTwitchAPI.Helix.Games.GetGamesAsync(null, new List<string>() { "Path of Exile" });
            var clipy = await TheTwitchAPI.Helix.Clips.GetClipsAsync(null, hp.Games[0].Id.ToString(), null, null, null, fromDate, toDate, 100);
            textBox1.Text = "";
            foreach (var item in clipy.Clips.OrderByDescending(x => x.ViewCount))
            {
                if (item.ViewCount < viewsLimit)
                {
                    continue;
                }

                if(Bans.Contains(item.BroadcasterName.ToUpper().Trim()))
                {
                    //block by streamer name
                    continue;
                }

                if (isAutoMode)
                {
                    saveFile += $"{item.Url}\r\n";
                }
                else
                {
                    textBox1.Text += $"{item.Url}\r\n";
                }
            }

            if (isAutoMode)
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (!string.IsNullOrEmpty(Properties.Settings.Default.FileSavePath))
                {
                    path = Properties.Settings.Default.FileSavePath;
                }

                string fileName = Properties.Settings.Default.FileName;
                fileName = fileName.Replace("dd", DateTime.Now.Day.ToString("00"));
                fileName = fileName.Replace("MM", DateTime.Now.Month.ToString("00"));
                fileName = fileName.Replace("yyyy", DateTime.Now.Year.ToString());
                fileName = fileName.Replace("HH", DateTime.Now.Hour.ToString("00"));
                fileName = fileName.Replace("mm", DateTime.Now.Minute.ToString("00"));

                string filePath = Path.Combine(path, fileName);
                File.WriteAllText(filePath, saveFile);
                Application.Exit();
            }




        }



        void InitWebServer()
        {
            WebServer = new HttpServer();
            WebServer.EndPoint = new System.Net.IPEndPoint(IPAddress.Loopback, 80);

            WebServer.RequestReceived += WebServer_RequestReceived;
            WebServer.Start();
        }

        private async void WebServer_RequestReceived(object sender, HttpRequestEventArgs e)
        {
            using (var writer = new StreamWriter(e.Response.OutputStream))
            {
                if (e.Request.QueryString.AllKeys.Any("code".Contains))
                {
                    var code = e.Request.QueryString["code"];
                    var ownerOfChannelAccessAndRefresh = await getAccessAndRefreshTokens(code);
                    CachedOwnerOfChannelAccessToken = ownerOfChannelAccessAndRefresh.Item1;
                    SetNameAndIdByOauthUser(CachedOwnerOfChannelAccessToken).Wait();
                    InitlaizeTwitchAPI(CachedOwnerOfChannelAccessToken);

                }
            }
        }

        async void ConnectUsingCachedCode(string code)
        {
            var ownerOfChannelAccessAndRefresh = await getAccessAndRefreshTokens(code);
            CachedOwnerOfChannelAccessToken = ownerOfChannelAccessAndRefresh.Item1;
            SetNameAndIdByOauthUser(CachedOwnerOfChannelAccessToken).Wait();
            InitlaizeTwitchAPI(CachedOwnerOfChannelAccessToken);
        }



        void InitlaizeTwitchAPI(string accessToken)
        {
            TheTwitchAPI = new TwitchAPI();
            TheTwitchAPI.Settings.ClientId = ClientId;
            TheTwitchAPI.Settings.AccessToken = accessToken;
            TwitchApiReady?.Invoke(null,null);
        }



        async Task SetNameAndIdByOauthUser(string accessToken)
        {
            var api = new TwitchLib.Api.TwitchAPI();
            api.Settings.ClientId = ClientId;
            api.Settings.AccessToken = accessToken;

            var oauthUser = await api.Helix.Users.GetUsersAsync();
            string TwitchChannelId = oauthUser.Users[0].Id;
            string TwitchChannelName = oauthUser.Users[0].Login;
        }



        async Task<Tuple<string, string>> getAccessAndRefreshTokens(string code)
        {
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
            {
                { "client_id", ClientId },
                { "client_secret", ClientSecret },
                { "code", code },
                { "grant_type", "authorization_code" },
                { "redirect_uri", RedirectUrl }
            };


            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://id.twitch.tv/oauth2/token", content);

            var responseString = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseString);
            return new Tuple<string, string>(json["access_token"].ToString(), json["refresh_token"].ToString());

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MinimumViews = numericUpDown1.Value.ToString();
            Properties.Settings.Default.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var bansForm = new BlackList().ShowDialog();
            LoadBans();
        }

        void LoadBans()
        {
            try
            {
                var bannedList = System.IO.File.ReadAllText("bans.json");
                Bans = JsonConvert.DeserializeObject<List<string>>(bannedList);
            }
            catch { }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            new Settings().ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int counter = 0;
            int limiter = int.Parse(Properties.Settings.Default.GrabTopVideos);
            foreach (var item in textBox1.Lines)
            {
                System.Diagnostics.Process.Start(item);
                counter++;
                if(counter >= limiter)
                {
                    break;
                }
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.GrabTopVideos = numericUpDown2.Value.ToString();
            Properties.Settings.Default.Save();
        }
    }
}
