using System;
using System.Drawing;
using Tesseract;
using OpenCV.Net;
using System.Drawing.Imaging;

namespace RESTTest
{
    class OCRProcessor : IDisposable
    {
        Tesseract.TesseractEngine tess;
        String PfadZumBild;
        MainWindow Parent;

        public OCRProcessor(String Bild, MainWindow parent)
        {
            PfadZumBild = Bild;
            Parent = parent;
            tess = new Tesseract.TesseractEngine(@".\tessdata", "eng", EngineMode.Default); // , "configfile"
        }

        ~OCRProcessor()
        {
            Dispose(false); //I am *not* calling you from Dispose, it's *not* safe
        }

        
        public String Version ()
        {
            return tess.Version;
        }

        public String process()
        {
            // have to load Pix via a bitmap since Pix doesn't support loading a stream.
            using (Bitmap bmp = new Bitmap(@".\" + PfadZumBild))
            {
                Parent.setBild(bmp); // DEBUG ONLY

                
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                var data = bmpData.Scan0;
                
                //use the data in the ::Mat constructor.
                OpenCV.Net.Mat A = new Mat(bmp.Height, bmp.Width, Depth.S32, 3, data);
                

                squares findeKarte = new squares();
                bmp.UnlockBits(bmpData); //Remember to unlock!!!

                /*
                vector<OpenCV.Net.Point> approx;

                for (size_t i = 0; i < contours.size(); i++)
                {
                    approxPolyDP(Mat(contours[i]), approx,
                                 arcLength(Mat(contours[i]), true) * 0.02, true);

                    if (approx.size() == 4 &&
                        fabs(contourArea(Mat(approx))) > 1000 &&
                        isContourConvex(Mat(approx)))
                    {
                        double maxCosine = 0;

                        for (int j = 2; j < 5; j++)
                        {
                            double cosine = fabs(angle(approx[j % 4], approx[j - 2], approx[j - 1]));
                            maxCosine = MAX(maxCosine, cosine);
                        }

                        if (maxCosine < 0.3)
                            squares.push_back(approx);
                    }
                }*/


                // herausfinden, wie der Winkel gerade ist

                // Bild drehen, bis die Karte senkrecht ist
                Bitmap bmpRotated = bmp;
                if (bmp.Height < bmp.Width)
                {
                    bmpRotated.RotateFlip(RotateFlipType.Rotate90FlipNone);
                } 
                Parent.setBild(bmpRotated); // DEBUG ONLY
                /*Bitmap bmpR = new Bitmap(bmp.Width, bmp.Height);;
                using (Graphics graphics = Graphics.FromImage(bmpR))
                {
                    graphics.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2); // Drehpunkt in der Mitte
                    graphics.RotateTransform((float)10.5);
                    graphics.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);
                    graphics.DrawImage(bmp, new Point(0, 0));
                }*/

                // herausfinden, wo der Rand ist
                int cutTop = bmpRotated.Height / 100 * 3;  // DEBUG 3% wegnemhen an jeder seite, unabhängig von Auflösung und DPI
                int cutLeft = bmpRotated.Width / 100 * 3;
                int cutBot = bmpRotated.Width / 100 * 3;
                int cutRight = bmpRotated.Height / 100 * 3;
                

                // Bild zurechtschneiden
                Rectangle r = new Rectangle(cutLeft, cutTop, bmpRotated.Width - (cutRight + cutLeft), bmpRotated.Height - (cutBot + cutTop)); //cutLeft, bmpR.Width - (cutTop + cutBot), bmpR.Height - (cutLeft + cutRight));
                Bitmap bmpCropped = new Bitmap(r.Width, r.Height, bmp.PixelFormat);
                bmpCropped.SetResolution(bmpRotated.HorizontalResolution, bmpRotated.VerticalResolution);
                Graphics g = Graphics.FromImage(bmpCropped);
                g.DrawImage(bmpRotated, -r.X, -r.Y);

                Parent.setBild(bmpCropped);// DEBUG ONLY

                // wenn das Bild passt, die Abmessungen der OCR Bereiche berechnen
                // Karte: 720x971
                // 3240 x 4320 r.Width, r.Height 
                // 90 = 0,125 W
                // 66 = 0,068 H
                // 33 = 0,0339 H
                //

                Tesseract.Rect KartenName = new Tesseract.Rect((int)(bmpCropped.Width * 0.125), (int)(bmpCropped.Height * 0.068), (int)(bmpCropped.Width * 0.555), (int)(bmpCropped.Height * 0.0339));
                Rectangle KName = new Rectangle((int)(bmpCropped.Width * 0.125), (int)(bmpCropped.Height * 0.068), (int)(bmpCropped.Width * 0.555), (int)(bmpCropped.Height * 0.0339));

                Tesseract.Rect KartenTyp = new Tesseract.Rect((int)(bmpCropped.Width * 0.125), (int)(bmpCropped.Height * 0.5489), (int)(bmpCropped.Width * 0.125), (int)(bmpCropped.Height * 0.0339));
                Rectangle KTyp = new Rectangle((int)(bmpCropped.Width * 0.125), (int)(bmpCropped.Height * 0.5489), (int)(bmpCropped.Width * 0.125), (int)(bmpCropped.Height * 0.0339));

                Tesseract.Rect KartenText = new Tesseract.Rect((int)(bmpCropped.Width * 0.1319), (int)(bmpCropped.Height * 0.5942), (int)(bmpCropped.Width * 0.7152), (int)(bmpCropped.Height * 0.2584));
                Rectangle KText = new Rectangle((int)(bmpCropped.Width * 0.1319), (int)(bmpCropped.Height * 0.5942), (int)(bmpCropped.Width * 0.7152), (int)(bmpCropped.Height * 0.2584));

                Tesseract.Rect Artist = new Tesseract.Rect((int)(bmpCropped.Width * 0.125), (int)(bmpCropped.Height * 0.8671), (int)(bmpCropped.Width * 0.4166), (int)(bmpCropped.Height * 0.0236));
                Rectangle KArtist = new Rectangle((int)(bmpCropped.Width * 0.125), (int)(bmpCropped.Height * 0.8671), (int)(bmpCropped.Width * 0.4166), (int)(bmpCropped.Height * 0.0236));

                Tesseract.Rect Edition = new Tesseract.Rect((int)(bmpCropped.Width * 0.7916), (int)(bmpCropped.Height * 0.5489), (int)(bmpCropped.Width * 0.0625), (int)(bmpCropped.Height * 0.0339));
                Rectangle KEdition = new Rectangle((int)(bmpCropped.Width * 0.7916), (int)(bmpCropped.Height * 0.5489), (int)(bmpCropped.Width * 0.0625), (int)(bmpCropped.Height * 0.0339));

                SolidBrush br = new SolidBrush(Color.FromArgb(90, 180, 0, 0));
                

                using (Graphics gra = Graphics.FromImage(bmpCropped))
                {
                    gra.DrawRectangle(new Pen(Color.FromArgb(125, 0, 0)), KName);
                    gra.FillRectangle(br, KName);
                    gra.DrawRectangle(new Pen(Color.FromArgb(125, 0, 0)), KTyp);
                    gra.FillRectangle(br, KTyp);
                    gra.DrawRectangle(new Pen(Color.FromArgb(125, 0, 0)), KText);
                    gra.FillRectangle(br, KText);
                    gra.DrawRectangle(new Pen(Color.FromArgb(125, 0, 0)), KArtist);
                    gra.FillRectangle(br, KArtist);
                    gra.DrawRectangle(new Pen(Color.FromArgb(125, 0, 0)), KEdition);
                    gra.FillRectangle(br, KEdition);
                }

                Parent.setBild(bmpCropped);// DEBUG ONLY
               
                using (var pix = PixConverter.ToPix(bmpCropped))
                {
                    using (var page = tess.Process(pix
                        , KartenName
                        //, KartenTyp
                        //, KartenText
                        //, Artist
                        //, Edition
                        , PageSegMode.Auto))
                    {
                        String re = page.GetMeanConfidence().ToString();
                        //re += Environment.NewLine + page.GetHOCRText(0);
                        re += Environment.NewLine + page.GetText();
                        return re;
                    }
                }
            }
        }

        public void Dispose()
        {
            //try
            //{
            Dispose(true); //I am calling you from Dispose, it's safe
            GC.SuppressFinalize(this); //Hey, GC: don't bother calling finalize later
            /*}
            finally -- nur wenn abgeleitete Klasse
            {
                base.Dispose();
            }*/
        }

        protected void Dispose(Boolean itIsSafeToAlsoFreeManagedObjects)
        {
            //Free unmanaged resources
            //Win32.DestroyHandle(this.gdiCursorBitmapStreamFileHandle);

            //Free managed resources too, but only if I'm being called from Dispose
            //(If I'm being called from Finalize then the objects might not exist
            //anymore
            if (itIsSafeToAlsoFreeManagedObjects)
            {
                if (this.tess != null)
                {
                    this.tess.Dispose();
                    this.tess = null;
                }
                /*if (this.frameBufferImage != null)
                {
                    this.frameBufferImage.Dispose();
                    this.frameBufferImage = null;
                }*/
            }
        }
    }
}