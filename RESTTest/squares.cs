using OpenCV.Net;
using System;

namespace RESTTest
{
    class squares
    {
        int thresh = 50, N = 11;

        // helper function:
        // finds a cosine of angle between vectors
        // from pt0->pt1 and from pt0->pt2
        private double angle( Point pt1, Point pt2, Point pt0 )
        {
            double dx1 = pt1.X - pt0.X;
            double dy1 = pt1.Y - pt0.Y;
            double dx2 = pt2.X - pt0.X;
            double dy2 = pt2.Y - pt0.Y;
            return (dx1*dx2 + dy1*dy2)/Math.Sqrt((dx1*dx1 + dy1*dy1)*(dx2*dx2 + dy2*dy2) + 1e-10);
        }

        // returns sequence of squares detected on the image.
        // the sequence is stored in the specified memory storage
        public void findSquares(Mat image, Point[][] squares)
        {
            squares.Initialize();

            Mat pyr, timg, gray0, gray;
            pyr = new Mat(image.Cols, image.Rows, Depth.S32, 3);
            timg = new Mat(image.Cols, image.Rows, Depth.S32, 3);
            gray0 = new Mat(image.Cols, image.Rows, Depth.U8, 3);
            gray = new Mat(image.Cols, image.Rows, Depth.U8, 3);
            
            // down-scale and upscale the image to filter out the noise
            OpenCV.Net.CV.PyrDown(image, pyr); // new OpenCV.Net.Size(image.Cols/2, image.Rows/2)
            OpenCV.Net.CV.PyrUp(pyr, timg);
            Point[][] contours;
    
            // find squares in every color plane of the image
            for( int c = 0; c < 3; c++ )
            {
                int[] ch = {c, 0};
                OpenCV.Net.Arr[] rw;
                //OpenCV.Net.CV.MixChannels(timg, gray0, ch);
                
                // try several threshold levels
                /*for( int l = 0; l < N; l++ )
                {
                    // hack: use Canny instead of zero threshold level.
                    // Canny helps to catch squares with gradient shading
                    if( l == 0 )
                    {
                        // apply Canny. Take the upper threshold from slider
                        // and set the lower to 0 (which forces edges merging)
                        Canny(gray0, gray, 0, thresh, 5);
                        // dilate canny output to remove potential
                        // holes between edge segments
                        dilate(gray, gray, Mat(), Point(-1,-1));
                    }
                    else
                    {
                        // apply threshold if l!=0:
                        //     tgray(x,y) = gray(x,y) < (l+1)*255/N ? 255 : 0
                        gray = gray0 >= (l+1)*255/N;
                    }

                    // find contours and store them all as a list
                    findContours(gray, contours, CV_RETR_LIST, CV_CHAIN_APPROX_SIMPLE);

                    vector<Point> approx;
            
                    // test each contour
                    for( size_t i = 0; i < contours.size(); i++ )
                    {
                        // approximate contour with accuracy proportional
                        // to the contour perimeter
                        approxPolyDP(Mat(contours[i]), approx, arcLength(Mat(contours[i]), true)*0.02, true);
                
                        // square contours should have 4 vertices after approximation
                        // relatively large area (to filter out noisy contours)
                        // and be convex.
                        // Note: absolute value of an area is used because
                        // area may be positive or negative - in accordance with the
                        // contour orientation
                        if( approx.size() == 4 &&
                            fabs(contourArea(Mat(approx))) > 1000 &&
                            isContourConvex(Mat(approx)) )
                        {
                            double maxCosine = 0;

                            for( int j = 2; j < 5; j++ )
                            {
                                // find the maximum cosine of the angle between joint edges
                                double cosine = fabs(angle(approx[j%4], approx[j-2], approx[j-1]));
                                maxCosine = MAX(maxCosine, cosine);
                            }

                            // if cosines of all angles are small
                            // (all angles are ~90 degree) then write quandrange
                            // vertices to resultant sequence
                            if( maxCosine < 0.3 )
                                squares.push_back(approx);
                        }
                    }
                }*/
            }
        }

    }
}
