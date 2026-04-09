using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using static System.Collections.Specialized.BitVector32;
using static Google.Rpc.Context.AttributeContext.Types;

namespace gpmnrega2.Registers
{
    public partial class register467 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                int register = int.Parse(Request.Params["register"].ToString());

                string result = "";
                string finyear = Request.Params["fin_year"].ToString().Split('-')[0];
                string fin_year = Request.Params["fin_year"].ToString();

                string district_code = Request.Params["District_Code"].ToString();
                string district_name = Request.Params["district_name"].ToString();

                string state_name = Request.Params["state_name"].ToString();
                string state_Code = Request.Params["state_Code"].ToString();

                string block_name = Request.Params["block_name"].ToString();
                string block_code = Request.Params["block_code"].ToString();

                string Panchayat_name = Request.Params["Panchayat_name"].ToString();
                string Panchayat_Code = Request.Params["Panchayat_Code"].ToString();



                string[] quatersStart = new string[] { "01/04/" + finyear, "01/07/" + finyear, "01/10/" + finyear, "01/01/" + (int.Parse(finyear) + 1).ToString() };
                string[] quaterDate = new string[] { "30/06/" + finyear, "30/09/" + finyear, "31/12/" + finyear, "31/03/" + (int.Parse(finyear) + 1).ToString() };

                HttpClient client = new HttpClient();
                HttpResponseMessage message = client.GetAsync("https://nregastrep.nic.in/netnrega/IndexFrame.aspx?lflag=eng&District_Code=" + district_code + "&district_name=" + district_name + "&state_name=KARNATAKA&state_Code=15&block_name=" + block_name + "&block_code=" + block_code + "&fin_year=" + fin_year + "&check=1&Panchayat_name=" + Panchayat_name + "&Panchayat_Code=" + Panchayat_Code).Result;

               
                string cookies = message.Headers.GetValues("Set-Cookie").ElementAt(0).Split('=')[1].Split(';')[0];



                if (register == 4)
                {
                    HtmlDocument doc1 = new HtmlDocument();
                    doc1.LoadHtml(message.Content.ReadAsStringAsync().Result);
                    var links = doc1.DocumentNode.SelectNodes("//a");
                    string link = "";// "https://nregastrep.nic.in/netnrega/" + Server.UrlDecode(doc1.DocumentNode.SelectNodes("//a")[40].Attributes["href"].Value);
                    foreach(var item in links)
                    {
                        if (item.InnerText.Trim() == "Work Register")
                        {
                            link = "https://nregastrep.nic.in/netnrega/" + Server.UrlDecode(item.Attributes["href"].Value);
                            break;
                        }
                    }
                    int quater = int.Parse(Request.Params["quater"].ToString());
                    HttpResponseMessage register4raw = client.GetAsync(link).Result;

                    Dictionary<string, string> postData = new Dictionary<string, string>();
                    HtmlDocument doc = new HtmlDocument() { OptionUseIdAttribute = true };
                    doc.LoadHtml(register4raw.Content.ReadAsStringAsync().Result);

                    string body = @"__EVENTTARGET=&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "")) +
                                  "&__VIEWSTATEGENERATOR=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "")) + "&__VIEWSTATEENCRYPTED=&__EVENTVALIDATION=" +
                                  Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "")) + "&ctl00$ContentPlaceHolder1$TextBox3=" + quatersStart[quater] +
                                  "&ctl00$ContentPlaceHolder1$TextBox4=" + quaterDate[quater] + "&ctl00$ContentPlaceHolder1$ddrows=10&ctl00$ContentPlaceHolder1$Button1=submit&ctl00$ContentPlaceHolder1$RadioButtonList2=0";


                    HttpContent content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

                    HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, link)
                    {
                        Content = content

                    };

                    HttpResponseMessage register4filtered = client.PostAsync(link, content).Result;
                    result = register4filtered.Content.ReadAsStringAsync().Result;
                }
                if (register == 7)
                {
                    HtmlDocument doc1 = new HtmlDocument();
                    doc1.LoadHtml(message.Content.ReadAsStringAsync().Result);
                    var links = doc1.DocumentNode.SelectNodes("//a");
                    string link = "";// "https://nregastrep.nic.in/netnrega/" + Server.UrlDecode(doc1.DocumentNode.SelectNodes("//a")[40].Attributes["href"].Value);
                    foreach (var item in links)
                    {
                        if (item.InnerText.Trim() == "Material Register")
                        {
                            link = "https://nregastrep.nic.in/netnrega/" + Server.UrlDecode(item.Attributes["href"].Value);
                            break;
                        }
                    }
                    HttpResponseMessage register7raw = client.GetAsync(link).Result;
                    result = register7raw.Content.ReadAsStringAsync().Result;
                    HtmlDocument doc = new HtmlDocument() { OptionUseIdAttribute = true };

                    doc.LoadHtml(register7raw.Content.ReadAsStringAsync().Result);
                    string body = @"__EVENTTARGET=ctl00$ContentPlaceHolder1$ddl_finyr&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "")) +
                                 "&__VIEWSTATEGENERATOR=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "")) + "&__VIEWSTATEENCRYPTED=&__EVENTVALIDATION=" +
                                 Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "")) + "&ctl00$ContentPlaceHolder1$ddl_finyr=" + fin_year;


                    HttpContent content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

                    HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, link)
                    {
                        Content = content

                    };

                    HttpResponseMessage register7filtered = client.SendAsync(httpRequest).Result;
                    result = register7filtered.Content.ReadAsStringAsync().Result;

                }
                if (register == 6)
                {
                    HtmlDocument doc1 = new HtmlDocument();
                    doc1.LoadHtml(message.Content.ReadAsStringAsync().Result);
                    var links = doc1.DocumentNode.SelectNodes("//a");
                    string link = "";// "https://nregastrep.nic.in/netnrega/" + Server.UrlDecode(doc1.DocumentNode.SelectNodes("//a")[40].Attributes["href"].Value);
                    foreach (var item in links)
                    {
                        if (item.InnerText.Trim() == "Complaint Register")
                        {
                            link = "https://nregastrep.nic.in/netnrega/" + Server.UrlDecode(item.Attributes["href"].Value);
                            break;
                        }
                    }
                    HttpResponseMessage register6raw = client.GetAsync(link).Result;
                    result = register6raw.Content.ReadAsStringAsync().Result;
                }
                if (register == 1)
                {
                    HtmlDocument doc1 = new HtmlDocument();
                    doc1.LoadHtml(message.Content.ReadAsStringAsync().Result);
                    var links = doc1.DocumentNode.SelectNodes("//a");
                    string link = "";// "https://nregastrep.nic.in/netnrega/" + Server.UrlDecode(doc1.DocumentNode.SelectNodes("//a")[40].Attributes["href"].Value);
                    foreach (var item in links)
                    {
                        if (item.InnerText.Trim() == "Register No. 1 Part-C")
                        {
                            link = "https://nregastrep.nic.in/netnrega/" + Server.UrlDecode(item.Attributes["href"].Value);
                            break;
                        }
                    }

                    HttpWebRequest client1 = (HttpWebRequest) WebRequest.CreateHttp(link);
                    client1.CookieContainer = new CookieContainer();
                    client1.CookieContainer.Add(new Cookie("ASP.NET_SessionId", cookies) { Domain = "nregastrep.nic.in" });
                   

                    HtmlDocument doc = new HtmlDocument() { OptionUseIdAttribute = true };
                    HttpWebResponse jobcardresp = (HttpWebResponse)client1.GetResponse();


                    if (Request.Params["quater"].ToString() == "0")
                    {
                        
                        result = new StreamReader(jobcardresp.GetResponseStream()).ReadToEnd();
                    }
                    else
                    {
                        HttpWebRequest client2 = (HttpWebRequest)WebRequest.CreateHttp(link);

                        string requestQtr = Request.Params["quater"].ToString() == "1" ? "(7,8,9)" : Request.Params["quater"].ToString() == "2" ? "(10,11,12)" : "(1,2,3)";
                        doc.LoadHtml(new StreamReader(jobcardresp.GetResponseStream()).ReadToEnd());

                        string body = @"__EVENTTARGET=ctl00$ContentPlaceHolder1$ddl_quarter&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "")) +
                                     "&__VIEWSTATEGENERATOR=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "")) + "&__VIEWSTATEENCRYPTED=&__EVENTVALIDATION=" +
                                     Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "")) + "&__SCROLLPOSITIONX=0&__SCROLLPOSITIONY=0" +
                                     "&ctl00$ContentPlaceHolder1$ddl_fin=" + fin_year + "&ctl00$ContentPlaceHolder1$ddl_quarter=" + requestQtr + "&ctl00$ContentPlaceHolder1$ddrows=10";



                        client2.CookieContainer = new CookieContainer();
                        client2.CookieContainer.Add(new Cookie("ASP.NET_SessionId", cookies) { Domain = "nregastrep.nic.in" });
                        client2.Method = "POST";
                        var data = Encoding.UTF8.GetBytes(body);
                        client2.ContentType = "application/x-www-form-urlencoded";
                        client2.ContentLength = data.Length;
                        using (var sr = client2.GetRequestStream())
                        {
                            sr.Write(data, 0, data.Length);
                        }
                        var workResponse = (HttpWebResponse)client2.GetResponse();

                        result= new StreamReader(workResponse.GetResponseStream()).ReadToEnd();

                        

                    }
                }
                Response.Write(result);
                HttpContext.Current.Response.End();

            }
            catch (Exception ex)
            {
                if (ex.Message != "Thread was being aborted.")
                {
                    Response.ClearContent();
                    Response.StatusCode = 5001;
                    Response.StatusDescription = "Error connecting NREGA DataBase.";

                }
            }

        }
    }
}