using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace RESTTest
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WebProxy myProxy;
        

        public MainWindow()
        {
            InitializeComponent();
            myProxy = new WebProxy("10.112.242.230:8080");
            // geht hier nicht myProxy.Credentials = new NetworkCredential("DE99995x.117", "counter");
            myProxy.UseDefaultCredentials = false;
            myProxy.BypassProxyOnLocal = false;
        }

        static public string Beautify(XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }
            return sb.ToString();
        }

        private void Abfrage(String funktion)
        {

            BackgroundWorker worker = new BackgroundWorker();
            Boolean boolProxy = useProxy.IsChecked.Value;

            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                string Funktion = (string)args.Argument;
                String method = "GET";
                String url = "https://www.mkmapi.eu/ws/v1.1/" + Funktion;

                
                HttpWebRequest request = WebRequest.CreateHttp(url) as HttpWebRequest;
                OAuthHeader header = new OAuthHeader();
                request.Headers.Add(HttpRequestHeader.Authorization, header.getAuthorizationHeader(method, url));
                request.Method = method;
                if (boolProxy)
                {
                    myProxy.Credentials = new NetworkCredential("DE99995x.117", "counter");
                    request.Proxy = myProxy;
                }
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                XmlDocument doc = new XmlDocument();
                doc.Load(response.GetResponseStream());

                String[] Erg = new String[2];
                Erg[0] = Beautify(doc);
                XmlNodeList xnList = doc.SelectNodes("/response/product[1]/image");
                foreach (XmlNode xn in xnList)
                {
                    Erg[1] = xn.InnerText;
                }

                // XML Formatieren
                args.Result = Erg;
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                if (args.Error == null)
                {
                    String[] result = (String[])args.Result;
                    tbErg.Text = result[0];
                    tbErg.Height = (tbErg.Text.Length / 100) * tbErg.LineHeight;

                    if (result[1] != null && result[1].Length > 0)
                    {
                        holeKartenBild(result[1]);
                    }
                }
                else
                {
                    lblStatus.Content = args.Error;
                }
            };

            worker.RunWorkerAsync(funktion);

        }


        private void holeKartenBild(String PfadZumBild)
        {
            BitmapImage bild;
            BackgroundWorker worker = new BackgroundWorker();
            lblStatus.Content += ". Lade Bild";
            Boolean boolProxy = useProxy.IsChecked.Value;

            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                string myPfadZumBild = PfadZumBild;// (string)args.Argument;
                String method = "GET";
                String url = "https://www.magickartenmarkt.de/" + myPfadZumBild;

                HttpWebRequest request = WebRequest.CreateHttp(url) as HttpWebRequest;
                //OAuthHeader header = new OAuthHeader();
                //request.Headers.Add(HttpRequestHeader.Authorization, header.getAuthorizationHeader(method, url));
                request.Method = method;
                if (boolProxy)
                {
                    myProxy.Credentials = new NetworkCredential("DE99995x.117", "counter");
                    request.Proxy = myProxy;
                }
                request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.Default);


                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    using (BinaryReader reader = new BinaryReader(response.GetResponseStream()))
                    {
                        Byte[] buffer = reader.ReadBytes((int)response.ContentLength);
                        //stream.Read(buffer, 0, buffer.Length);
                        MemoryStream byteStream = new System.IO.MemoryStream(buffer);
                        bild = new BitmapImage();
                        bild.BeginInit();
                        bild.CacheOption = BitmapCacheOption.OnLoad;
                        bild.StreamSource = byteStream;
                        bild.EndInit();

                        if (bild.IsDownloading == true) 
                        {
                            System.Threading.Thread.Sleep(500);
                        }
                        if (bild.IsDownloading == true)
                        {
                            System.Threading.Thread.Sleep(1500);
                        }


                    }
                }
                bild.Freeze();
                args.Result = bild;

            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                if (args.Error == null)
                {
                    BitmapImage resp = args.Result as BitmapImage;
                    imgKarte.Width = resp.Width;
                    imgKarte.Height = resp.Height;
                    imgKarte.Source = resp;
                    lblStatus.Content = "Bild geladen";
                }
                else
                {
                    lblStatus.Content = args.Error;
                }
            };

            worker.RunWorkerAsync(PfadZumBild);
            //*/
        }

        
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            lblStatus.Content = "Start Abfrage welche Spiele es gibt";
            Abfrage("games");
            
        }

        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            lblStatus.Content = "Starte Abfrage Accountdaten";
            Abfrage("account");

        }

        private void btnSuche_Click(object sender, RoutedEventArgs e)
        {
            // Ergebnisfelder zurücksetzen
            tbErg.Text = "";
            imgKarte.Source = null;
            // Abfrage starten
            lblStatus.Content = "Starte Suche nach " + txtSuche.Text;
            Abfrage("products/" + txtSuche.Text + "/1/1/false");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbErg.Text);
        }

        private void tbErg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(tbErg.Text); // https://github.com/rockz1337/test.git
        }

    }
}
