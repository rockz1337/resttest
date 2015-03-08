using RESTTest;
using System;
using System.ComponentModel;
using System.Net;
using System.Xml;

public class getMKM
{
    
    public string resultError;
    public Boolean boolProxy;
    public delegate void callback(XmlDocument result);

    WebProxy myProxy;
    

	public getMKM(Boolean proxy)
	{
        boolProxy = proxy;
        if (boolProxy)
        {
            myProxy = new WebProxy("10.112.242.230:8080");
            // geht hier nicht myProxy.Credentials = new NetworkCredential("DE99995x.117", "counter");
            myProxy.UseDefaultCredentials = false;
            myProxy.BypassProxyOnLocal = false;
        }
	}

    public void Abfrage(String funktion, callback cb)
    {
        resultError = "";

        BackgroundWorker worker = new BackgroundWorker();

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
            try
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                XmlDocument doc = new XmlDocument();
                doc.Load(response.GetResponseStream());
                args.Result = doc;
            }
            catch (Exception ex)
            {
                resultError = ex.Message.ToString();
            }
        };

        worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
        {
            if (args.Error == null)
            {
                cb((XmlDocument)args.Result);
            }
            else
            {
                resultError =  args.Error.ToString();
            }
        };

        worker.RunWorkerAsync(funktion);
    }
}
