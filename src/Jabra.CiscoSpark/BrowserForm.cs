using System;
using System.Windows.Forms;

namespace Jabra.CiscoSpark
{
    public partial class BrowserForm : Form
    {

        public string Code { get; set; }
        public BrowserForm()
        {
            InitializeComponent();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            string returnUri = e.Url.AbsoluteUri;

            if (returnUri.StartsWith("http://localhost"))
            {
                var responseParameters = e.Url.Query.Split('&');

                var state = responseParameters[1].Split('=')[1];

                if (state == "success")
                {
                    Code = responseParameters[0].Split('=')[1];

                    Properties.Settings.Default["Code"] = Code;
                    Properties.Settings.Default.Save();
                }


                webBrowser1.Stop();
                webBrowser1.Navigate("about:blank");
//                Close();
            }

            if (returnUri.StartsWith("about:blank"))
            {
                Close();
            }

        }

        private void BrowserForm_Load(object sender, EventArgs e)
        {
            webBrowser1.Navigate("https://api.ciscospark.com/v1/authorize?client_id=C556432f895d42da8181b0d392efe4c4fa37535619d8db3b5e29bc4c826b28615&response_type=code&redirect_uri=http%3A%2F%2Flocalhost%2Fredirect.html&scope=spark%3Aall%20spark%3Akms&state=success");
        }

        public void NavigateTo()
        {
        }
    }
}
