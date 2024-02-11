using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace JotWin.View
{
    public partial class LicenseWindow : Window
    {
        bool allowClose = false;
        readonly MainAppWindow mainWindow;
        readonly string? userEmail;
        public LicenseWindow(MainAppWindow mainWindow, bool userIsActive)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            userEmail = mainWindow.userEmail;
            StartTrialView.Visibility = userIsActive ? Visibility.Collapsed : Visibility.Visible;
            EndTrialView.Visibility = userIsActive ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void OpenLink(string link)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = link,
                UseShellExecute = true
            });
        }

        public void Purchase_Click(object sender, EventArgs e)
        {
            OpenLink("https://www.espres.so/collections/jot");
        }
        public void Support_Click(object sender, EventArgs e)
        {
            OpenLink("https://www.espres.so/support");
        }
        public void LearnMore_Click(object sender, EventArgs e)
        {
            OpenLink("https://www.espres.so/download/jot");
        }

        public void StartTrial_Click(object sender, EventArgs e)
        {
            if (userEmail == null)
            {
                Debug.WriteLine("start trial -> No useremail");
                return;
            }
            MainAppWindow.Post("jot-activate", userEmail, HandleJotActivate);
            DispatcherTimer dispatcherTimer = new();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            if (StartTrial_Text.Text.Length >= 5)
            {
                StartTrial_Text.Text = "";
            }
            StartTrial_Text.Text += '.';
        }

        private void HandleJotActivate(dynamic? response)
        {
            if (response == null)
            {
                Debug.WriteLine("jot-activate -> null response");
                return;
            }

            bool active = response.active ?? false;
            bool trial = response.trial ?? false;
            bool expired = response.expired ?? false;
            bool licence = response.licence ?? false;
            DateTime expiry = response.expiry ?? DateTime.Now.AddDays(-1);

            if (licence && !expired)
            {
                ValidClose();
                return;
            }
            if (active && trial && !expired)
            {
                var daysRemaining = expiry.Subtract(DateTime.Now).Days;
                mainWindow.TrialRemainingText.Text = $"{daysRemaining} days left in trial";
                ValidClose();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!allowClose)
            {
                Application.Current.Shutdown();
            }
        }

        public void ValidClose()
        {
            allowClose = true;
            Close();
            mainWindow.Show();
        }
    }







    public class Keys
    {
            public static string backend_private_key = @"-----BEGIN RSA PRIVATE KEY-----
MIIEpQIBAAKCAQEA57bpgNcvJWmm9523n1NzKsZWbouDosGzFRTKtiJFg0r/Xj+r
C9PC70Qz6hOkkQ2PTv62Xx/3qKp3vy6G4ZPKKlSQMD7+v/ceTvUc6/oZH5CQeeJT
BdCRqjjTBEpZsJezvFGnWxt66FUEEasneYJLf5fHbK0X0PLYG/p1/JlozFebLApY
8ng6VvJY5NVZPBndZJz4aNy/bNSARfur8MCfpnLSaxTZMlmOko7rJwu37YNZUDb6
DmkdOj3qofmx6g2/lrIyRVGnWb5RLEpH7MQ/Cr3fQnfELkMgDtzJr4v4OnVqeAqc
UfQpCxsiUHuUcmNYvgnMUc7wglASL6Ji8/pfuQIDAQABAoIBAQCD7Mc7PMix81pF
xr0h8EA0zWGuZK5YvxG34fOcCR0sqEmGnpdDD5j/4wPvtKlQkLjUD/9DX48Ar7Wn
2tSeoCdNPIIvhd5C626NI88Ip4cgSo2HZ40/VUVp9hpmafJwsZ56jL7NB3NNzgGY
EatS6hUUGxVG0bIqm+jg3RPJ8ooRNIZieEKb9ZPc9sF78rlBnLU7bBeL9komHmax
uFvH37xiSMVEj374IGoA2D9NmvZ63t2z06DnUZS2tNSpDyD69oC5drd+Y+Pb3+n0
fAWyhOyBtHNK7CRQPPyr2FnSWPfHcs2e0kIFVNtscGzz8wvAYaFdzU1StFl3KoY6
ZjuxaSsBAoGBAPW8vQAf1V8TiEPuFijU6AZ2QcLs/pZjk7yK60jZPUuPrMAHoMV6
rZI8+w7GovkYB0uA0L+OkI9MDHcXydYofMY1g+InfC/u05WDavYVdX/sVbAJE3jy
BE3r+dyfMT88Eky5VWCDaVvbHrNL1jSfH5+FE+KTIaa8pK5td8Ypac+ZAoGBAPFk
QG8qzBPv9hT5BfGk8yAWdneoG56KJQG5jCIhplu9G5pF1i1WrUuTXy6XbnzHTP+D
oeK+BBDeTLUDkHBSK3SygbpSfQxxxcLMkdToVsKH+4PX/eMEFkfQsyYuymq40FkI
Sq2qebLzDHxFUtHEjx6VCS82ELjDkDKC970IcaUhAoGBAOOAwvHUhvQo3yUUzUss
IUuqPCO8yc5tjh8l9cJR7R+BeoumBEAP3ZXgAwag+8zlZAuQzLIryMYBwCCZ03ED
ttDCRsEfkSfHUfe/3UzKfSfbo1EAdhio5zdE2uRYNX23nbGOe+6IewhhisCv2zaI
gvxqdghz8tmtGEGscxDw0lcxAoGBAIIha2MCjVXKLL0NqhiktbR8p4zGAW2sR1rw
rgzQWyBlh/XY5Cc47N8rKUqytAtsXaP5UFIt4X8+d9e1fi4u/eJBQRIy2drVkqj7
IzrFrc/dAsgGroWtdF1ussVIwDJcQ2VbxPZuSoEf6YEs1gLjlcwEyBi9arJQKvIw
DGHJpYpBAoGACZMvGt4ujhvG6KIgaqwOrZaawymjmYH/ywSglzvNo8d+3qrMpsrU
ulPM4q4JCTk20/vJrKfdQ0AhY+/hqIaUUY+k7FBwL20Br4oNgRktOV6WYzV6R2MH
bb1UdZVFwuSAFzsO3ZByTzwdRwhdODqiVaXD3y2jrSIxjaeP14/bchI=
-----END RSA PRIVATE KEY-----";


        public static string public_key = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA57bpgNcvJWmm9523n1Nz
KsZWbouDosGzFRTKtiJFg0r/Xj+rC9PC70Qz6hOkkQ2PTv62Xx/3qKp3vy6G4ZPK
KlSQMD7+v/ceTvUc6/oZH5CQeeJTBdCRqjjTBEpZsJezvFGnWxt66FUEEasneYJL
f5fHbK0X0PLYG/p1/JlozFebLApY8ng6VvJY5NVZPBndZJz4aNy/bNSARfur8MCf
pnLSaxTZMlmOko7rJwu37YNZUDb6DmkdOj3qofmx6g2/lrIyRVGnWb5RLEpH7MQ/
Cr3fQnfELkMgDtzJr4v4OnVqeAqcUfQpCxsiUHuUcmNYvgnMUc7wglASL6Ji8/pf
uQIDAQAB
-----END PUBLIC KEY-----";


        public static string backend_public_key = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAowUJp9QQcdGUdzWvmtW+
BdSAV0jjgo4sfZUR1XUYBxfH+qsFDNusHnwHfQb75mVl62JMGrxx9DDj2ltJ4QDs
ODgZKPic6W4nJAwggkHQldP1lB8mH4M2ipj9HA7xA1eNdYwI135br547UyrUR0V4
yIVRxxtRWOeqj3OHleY0EkPKaTtoLQcjhjTfyl9kCLcVzx8zU/++4/LZaEydXxbc
tADfd1fjRBP31HtFiXrpX2I12/ttb1RLfsgUX1hYGLxOevyA1OeADf1XmdIizDZg
PqvbirurSjaQH4KW/OKIAh3SlZqKx7wGnpitWKFoe87pPZ9z/j5YMhC1rCgRVxZ2
EQIDAQAB
-----END PUBLIC KEY-----";

    }





}






