using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Forms;
using Jabra.CiscoSpark.Properties;
using JabraSDK;
using Newtonsoft.Json;
using RestSharp;

namespace Jabra.CiscoSpark
{
    public partial class frmDisplay : Form
    {
        private string _accessToken;
        private List<IDevice> _availableDevices;
        private IDeviceService _deviceService;

        public frmDisplay()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _deviceService = ServiceFactory.CreateDeviceService("u3XQfSxdxEq7LrkkRBLh8g");


            _accessToken = Settings.Default["AccessToken"].ToString();


            if (string.IsNullOrEmpty(_accessToken))
                LoginProcess();

            var people = GetPeople();

            if (people == null)
            {
                Settings.Default["Code"] = "";

                Settings.Default["AccessToken"] = "";


                LoginProcess();
            }

            if (people != null)
            {
                PopulateScreen(people);
                timer.Enabled = true;
            }

            _deviceService.FirstScanDone += FirstScanDone;
            _deviceService.DeviceAdded += DeviceAddedEvent;
            _deviceService.DeviceRemoved += DeviceRemovedEvent;
        }

        private void PopulateScreen(People people)
        {
            lblUser.Text = people.displayName;
            lblStatus.Text = people.status;
            lblLastAccessedAt.Text = DateTime.Now.ToLongTimeString();

            if (_availableDevices != null && _availableDevices.Count > 0)
            {
                var strDevices =
                    _availableDevices.Aggregate("", (current, availableDevice) => current + availableDevice.Name);

                lblDevice.Text = strDevices;
            }
            else
            {
                lblDevice.Text = @"No busylight supported device connected";
            }
        }

        private void LoginProcess()
        {
            var browserForm = new BrowserForm();
            browserForm.ShowDialog();

            var authorizationCode = Settings.Default["Code"].ToString();

            var accessTokens = getAccessTokens(authorizationCode);

            Settings.Default["AccessToken"] = accessTokens.access_token;
            Settings.Default["RefreshToken"] = accessTokens.refresh_token;

            Settings.Default["AT_Expires_In"] = DateTime.Now.AddSeconds(accessTokens.expires_in);
            Settings.Default["RT_Expires_In"] = DateTime.Now.AddSeconds(accessTokens.refresh_token_expires_in);

            Settings.Default.Save();

            _accessToken = accessTokens.access_token;
        }

        public Token getAccessTokens(string code)
        {
            var client = new RestClient("https://api.ciscospark.com/v1/access_token");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded",
                $"grant_type=authorization_code&client_id=C556432f895d42da8181b0d392efe4c4fa37535619d8db3b5e29bc4c826b28615&client_secret=60623f65e375c56a80236ecda70bb04405d91238fbe69b35c74739295083ec1a&code={code}&redirect_uri=http%3A%2F%2Flocalhost%2Fredirect.html",
                ParameterType.RequestBody);
            var restResponse = client.Execute<Token>(request);

            if (restResponse.StatusCode == HttpStatusCode.OK)
                return restResponse.Data;

            return null;
        }


        private void DeviceRemovedEvent(object sender, DeviceRemovedEventArgs e)
        {
            if (e.Device.IsSetBusylightSupported)
                _availableDevices.Remove(e.Device);
        }

        private void DeviceAddedEvent(object sender, DeviceAddedEventArgs e)
        {
            if (e.Device.IsSetBusylightSupported)
                _availableDevices.Add(e.Device);
        }


        private void FirstScanDone(object sender, string e)
        {
            _availableDevices = _deviceService.AvailableDevices.Where(c => c.IsSetBusylightSupported).ToList();
        }


        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                SetBusyLight();
            }
            catch (Exception exception)
            {
                lblStatus.Text = "Error";
            }
        }

        private void SetBusyLight()
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                LoginProcess();
                return;
            }

            var people = GetPeople();

            if (_availableDevices.Count == 0)
            {
                PopulateScreen(people);
                return;
            }

            if (people.status.ToLower().Equals("donotdisturb"))
            {
                label1.ForeColor = Color.Red;
                foreach (var availableDevice in _availableDevices)
                    if (availableDevice.IsSetBusylightSupported)
                    {
                        availableDevice.Lock();
                        availableDevice.SetBusylightState(true);
                        availableDevice.Unlock();
                    }
            }
            else
            {
                label1.ForeColor = Color.Green;
                foreach (var availableDevice in _availableDevices)
                    if (availableDevice.IsSetBusylightSupported)
                    {
                        availableDevice.Lock();
                        availableDevice.SetBusylightState(false);
                        availableDevice.Unlock();
                    }
            }

            PopulateScreen(people);
        }


        private People GetPeople()
        {
            using (var httpClient = new HttpClient {BaseAddress = new Uri("https://api.ciscospark.com")})
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Add("Accept", "Application/json");

                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _accessToken);


                var responseMessage = httpClient.GetAsync("/v1/people/me").Result;

                if (responseMessage.IsSuccessStatusCode)
                {
                    var jsonPeople = responseMessage.Content.ReadAsStringAsync().Result;

                    var people = JsonConvert.DeserializeObject<People>(jsonPeople);

                    return people;
                }

                return null;
            }
        }
    }
}