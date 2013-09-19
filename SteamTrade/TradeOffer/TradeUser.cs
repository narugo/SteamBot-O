using System;
using System.IO;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using SteamKit2;

namespace SteamTrade.TradeOffer
{
    public class TradeUser
    {
        static CookieContainer Cookies = new CookieContainer();

        public TradeUser(string sessionId, string token)
        {
            Cookies.Add(new Cookie("sessionid", sessionId, String.Empty, "steamcommunity.com"));
            Cookies.Add(new Cookie("steamLogin", token, String.Empty, "steamcommunity.com"));
        }

        public string Fetch(string url, string method, NameValueCollection data = null, bool ajax = false)
        {
            HttpWebResponse response = Request(url, method, data, ajax);
            StreamReader reader = new StreamReader(response.GetResponseStream());
            return reader.ReadToEnd();
        }

        public static HttpWebResponse Request(string url, string method, NameValueCollection data = null, bool ajax = false)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            request.Method = method;

            request.Accept = "text/javascript, text/html, application/xml, text/xml, */*";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.Host = "steamcommunity.com";
            request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/536.11 (KHTML, like Gecko) Chrome/20.0.1132.47 Safari/536.11";
            request.Referer = "http://steamcommunity.com/tradeoffer/1";

            request.CookieContainer = Cookies;

            if (ajax)
            {
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                request.Headers.Add("X-Prototype-Version", "1.7");
            }

            // Request data
            if (data != null)
            {
                string dataString = String.Join("&", Array.ConvertAll(data.AllKeys, key =>
                    String.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(data[key]))
                )
                );

                byte[] dataBytes = Encoding.ASCII.GetBytes(dataString);
                request.ContentLength = dataBytes.Length;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(dataBytes, 0, dataBytes.Length);
            }

            // Get the response
            try
            {
                return request.GetResponse() as HttpWebResponse;
            }
            catch (WebException we)
            {
                if (we.Response != null)
                {
                    return (HttpWebResponse)we.Response;
                }
                throw;
            }
        }

        public TradeOffer getTrade(int id)
        {
            return new TradeOffer(this, id, null);
        }

        public TradeOffer newTrade(SteamID partner){
            return new TradeOffer(this, 0, partner);
        }

        public TradeOffer getTrades(SteamID tradeList)
        {
            return new TradeOffer(this, 0, null, tradeList);
        }
    }
}
