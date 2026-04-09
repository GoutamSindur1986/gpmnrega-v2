using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HtmlAgilityPack;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;

namespace gpmnrega2.Registers
{
    public partial class assetscompleted : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                string finyear = Request.QueryString["fin_year"];
                string distcode = Request.QueryString["dist_code"];
                string link = "";
                string url = "https://nregastrep.nic.in/netnrega/homestciti.aspx?state_code=15&state_name=KARNATAKA&lflag=eng&labels=labels";
                HttpClient client = new HttpClient();
                HttpResponseMessage response = client.GetAsync(url).Result;
                string currentfin = "";
                if (DateTime.Now.Month < 3 && DateTime.Now.Month > -1)
                {
                    currentfin = (DateTime.Now.Year - 1) + "-" + DateTime.Now.Year;
                }
                else if (DateTime.Now.Month > 2 && DateTime.Now.Month < 12)
                {
                    currentfin = (DateTime.Now.Year) + "-" + (DateTime.Now.Year + 1);
                }
                if (finyear == currentfin)
                {
                    var stateresponse = response.Content.ReadAsStringAsync().Result;
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(stateresponse);
                    var links = document.DocumentNode.SelectNodes("//a");

                    for(int a=325; a<links.Count;a++)
                    {
                        if (links[a].InnerText.Trim() == "Assets Created")
                        {
                            link = "https://nregastrep.nic.in/netnrega/" + document.DocumentNode.SelectNodes("//a")[a].Attributes["href"].Value;//Assets Created
                            break;
                        }
                    }
                }
                else
                {
                    var stateresponse = response.Content.ReadAsStringAsync().Result;
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(stateresponse);
                    string __EVENTTARGET = "__EVENTTARGET=fin_year&";
                    string __EVENTARGUMENT = "__EVENTARGUMENT=&";
                    string __LASTFOCUS = "__LASTFOCUS=&";
                    string __VIEWSTATE = "__VIEWSTATE=" + Server.UrlEncode(document.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "")) + "&";
                    string __VIEWSTATEGENERATOR = "__VIEWSTATEGENERATOR=" + document.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "") + "&";
                    string __SCROLLPOSITIONX = "__SCROLLPOSITIONX=0&";
                    string __SCROLLPOSITIONY = "__SCROLLPOSITIONY=400&";
                    string __VIEWSTATEENCRYPTED = "__VIEWSTATEENCRYPTED=&";
                    string __EVENTVALIDATION = "__EVENTVALIDATION=" + Server.UrlEncode(document.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "")) + "&";
                    string fin_year = "fin_year=" + finyear;
                    string finresp = __EVENTTARGET + __EVENTARGUMENT + __LASTFOCUS + __VIEWSTATE + __VIEWSTATEGENERATOR + __SCROLLPOSITIONX + __SCROLLPOSITIONY + __VIEWSTATEENCRYPTED + __EVENTVALIDATION + fin_year;
                    HttpContent content1 = new StringContent(finresp, Encoding.UTF8, "application/x-www-form-urlencoded");

                    HttpRequestMessage httpRequest1 = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = content1
                    };

                    HttpResponseMessage stateresp = client.SendAsync(httpRequest1).Result;
                    var staterespcnt = stateresp.Content.ReadAsStringAsync().Result;
                    document = new HtmlDocument();
                    document.LoadHtml(staterespcnt);
                    var links = document.DocumentNode.SelectNodes("//a");

                    for (int a=325;a<links.Count;a++)
                    {
                        if (links[a].InnerText.Trim() == "Assets Created")
                        {
                            link = "https://nregastrep.nic.in/netnrega/" + document.DocumentNode.SelectNodes("//a")[a].Attributes["href"].Value;//Assets Created
                            break;
                        }
                    }
                }

                HttpResponseMessage distresponse = client.GetAsync(link).Result;
                var distresp = distresponse.Content.ReadAsStringAsync().Result;
                Response.Write(distresp);
                Response.End();

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