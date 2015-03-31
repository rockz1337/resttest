using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;

namespace RESTTest
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        getMKM mkmApi;
        int idGame;
        int idLanguage;

        
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


        public MainWindow()
        {
            InitializeComponent();
            mkmApi = new getMKM(false);
            // Abfrage Game
            mkmApi.Abfrage("games", new getMKM.callback(sezteIdGame));

            // Abfrage Language
            /*  
                1 - English
                2 - French
                3 - German
                4 - Spanish
                5 - Italian 
             */
            idLanguage = 3;
        }


        private void sezteIdGame(XmlDocument xml)
        {
            if (xml != null)
            {
                XmlNodeList xnList = xml.SelectNodes("/response/game[name='Magic the Gathering']/idGame");
                foreach (XmlNode xn in xnList)
                {
                    idGame = int.Parse(xn.InnerText);
                }
                this.Title = this.Title + " GameId: " + idGame.ToString();
            }
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
                /*if (boolProxy)
                {
                    myProxy.Credentials = new NetworkCredential("DE99995x.117", "counter");
                    request.Proxy = myProxy;
                }*/
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
            //getMKM.callback cb = new getMKM.callback(zeigeXML);
            mkmApi.Abfrage("games", new getMKM.callback(zeigeXML));
        }

        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            lblStatus.Content = "Starte Abfrage Accountdaten";
            mkmApi.Abfrage("account", new getMKM.callback(zeigeXML));
        }


        private void btnSuche_Click(object sender, RoutedEventArgs e)
        {
            // Ergebnisfelder zurücksetzen
            tbErg.Text = "";
            imgKarte.Source = null;
            // Abfrage starten
            lblStatus.Content = "Starte Suche nach " + txtSuche.Text;
            mkmApi.Abfrage("products/" + txtSuche.Text + "/" + idGame + "/" + idLanguage + "/false", new getMKM.callback(zeigeBild));
        }


        private void btnTry_Click(object sender, RoutedEventArgs e)
        {
            startTry();
        }


        private void startTry()
        {
            lblStatus.Content = "";
            // String optimieren und ab dafür
            if (txtTry.Text.Length > 0)
            {
                txtTry.Text = txtTry.Text.Trim();
                txtTry.Text = txtTry.Text.TrimStart('/');
                txtTry.Text = txtTry.Text.Replace(":idGame", idGame.ToString());
                txtTry.Text = txtTry.Text.Replace(":idLanguage", idLanguage.ToString());
                mkmApi.Abfrage(txtTry.Text, new getMKM.callback(zeigeXML));
            }
            else
            {
                lblStatus.Content = "Das Abfragefeld ist leer.";
            }
        }


        private void zeigeXML(XmlDocument xml)
        {
            if (xml != null && mkmApi.resultError.Length == 0)
            {
                tbErg.Text = Beautify(xml);
            }
            else
            {
                lblStatus.Content = mkmApi.resultError;
                tbErg.Text = "";
            }
            tbErg.Height = (tbErg.Text.Length / 100) * tbErg.LineHeight;
        }


        private void zeigeBild(XmlDocument xml)
        {
            zeigeXML(xml);

            String BildRef = "";
            XmlNodeList xnList = xml.SelectNodes("/response/product[1]/image");
            foreach (XmlNode xn in xnList)
            {
                BildRef = xn.InnerText;
            }

            if (BildRef != null && BildRef.Length > 0)
            {
                holeKartenBild(BildRef);
            }
        }


        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        private static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }

        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                  new Action(delegate { }));
        }

        public void setBild(System.Drawing.Bitmap bmp)
        {
            BitmapSource bmps = loadBitmap(bmp);
            if (bmps.Width > 1500)
            {
                imgKarte.Width = bmps.Width / 4;
                imgKarte.Height = bmps.Height / 4;
            }
            else
            {
                imgKarte.Width = bmps.Width;
                imgKarte.Height = bmps.Height;
            }
            imgKarte.Source = bmps;
            DoEvents();
        }


        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbErg.Text);
        }

        private void tbErg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(tbErg.Text); // https://github.com/rockz1337/test.git
        }

        private void useProxy_Checked(object sender, RoutedEventArgs e)
        {
            mkmApi.boolProxy = useProxy.IsChecked.Value;
        }

        private void txtTry_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                startTry();
            }
        }



        private void machOCR(String dasBild)
        {
            lblStatus.Content = "Starte OCR - ";
            /*BackgroundWorker worker = new BackgroundWorker();
Auskommentiert damit zum Debuggen das Bild auf die Oberfläche gegeben werden kann
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {*/
            using (var op = new OCRProcessor(dasBild, this)) // DSCN4299.JPG  
            {
                String[] Result = new String[2];
                Result[1] = op.Version();
                Result[0] = op.process();
                /*args.Result = Result;
            }
        };

        worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
        {
            if (args.Error == null)
            {
                String[] Result = args.Result as String[];
                */
                tbErg.Text = Result[0];
                lblStatus.Content += Result[1];
                lblStatus.Content += " - OCR beendet";
            }
            /*else
            {
                lblStatus.Content = args.Error;
            }
        };

        worker.RunWorkerAsync();
        */
        }


        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            machOCR("206537124.jpg");
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            machOCR("DSCN4300.JPG");
        }

    }
}
