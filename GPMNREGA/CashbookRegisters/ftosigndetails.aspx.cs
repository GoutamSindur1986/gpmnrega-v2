using EvoPdf;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Google.Rpc.Context.AttributeContext.Types;
using static Google.Rpc.Help.Types;

namespace gpmnrega2.Registers
{
    public partial class ftosigndetails : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                string primaryUrl = "https://nregastrep.nic.in/netnrega/homestciti.aspx?state_code=15&state_name=KARNATAKA&lflag=eng&labels=labels";
                string dist_code, block_code, pcode, dist_name, block_name, ppname, finyear;
                dist_code = Request.QueryString["district_code"];
                block_code = Request.QueryString["block_code"];
                pcode = Request.QueryString["panchayat_code"];
                dist_name = Request.QueryString["district_name"];
                block_name = Request.QueryString["block_name"];
                ppname = Request.QueryString["panchayat_name"];
                finyear = Request.QueryString["fin_year"];

                HttpClient client = new HttpClient();
                HttpResponseMessage primaryresp = client.GetAsync(primaryUrl).Result;
                var primarycontent = primaryresp.Content.ReadAsStringAsync().Result;
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(primarycontent);
                string session = primaryresp.Headers.GetValues("Set-Cookie").FirstOrDefault().Split('=')[1].Split(';')[0];
                string ftolink = "";
                string currentfin = "";
                if (DateTime.Now.Month < 4 && DateTime.Now.Month > 0)
                {
                    currentfin = (DateTime.Now.Year - 1) + "-" + DateTime.Now.Year;
                }
                else if (DateTime.Now.Month > 3 && DateTime.Now.Month < 13)
                {
                    currentfin = (DateTime.Now.Year) + "-" + (DateTime.Now.Year + 1);
                }
                var selectNode = doc.DocumentNode.SelectSingleNode("//select[@id='fin_year']");
                currentfin = selectNode.SelectSingleNode(".//option[@selected]").InnerText;
                if (finyear == currentfin)
                {
                    var primarylinks = doc.DocumentNode.SelectNodes("//a");
                    for (int i = 250; i < primarylinks.Count; i++)
                    {
                        ftolink = "https://nregastrep.nic.in/netnrega/" + primarylinks[i].Attributes["href"].Value;
                        if (primarylinks[i].Attributes["href"].Value.Contains("FTO/FTOReport.aspx?"))
                            break;

                    }

                }
                else
                {
                    string __EVENTTARGET = "__EVENTTARGET=fin_year&";
                    string __EVENTARGUMENT = "__EVENTARGUMENT=&";
                    string __LASTFOCUS = "__LASTFOCUS=&";
                    string __VIEWSTATE = "__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "")) + "&";
                    string __VIEWSTATEGENERATOR = "__VIEWSTATEGENERATOR=" + doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "") + "&";
                    string __SCROLLPOSITIONX = "__SCROLLPOSITIONX=0&";
                    string __SCROLLPOSITIONY = "__SCROLLPOSITIONY=400&";
                    string __VIEWSTATEENCRYPTED = "__VIEWSTATEENCRYPTED=&";
                    string __EVENTVALIDATION = "__EVENTVALIDATION=" + Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "")) + "&";
                    string fin_year = "fin_year=" + finyear;

                    string finresp = __EVENTTARGET + __EVENTARGUMENT + __LASTFOCUS + __VIEWSTATE + __VIEWSTATEGENERATOR + __SCROLLPOSITIONX + __SCROLLPOSITIONY + __VIEWSTATEENCRYPTED + __EVENTVALIDATION + fin_year;
                    HttpContent content1 = new StringContent(finresp, Encoding.UTF8, "application/x-www-form-urlencoded");

                    HttpRequestMessage httpRequest1 = new HttpRequestMessage(HttpMethod.Post, primaryUrl)
                    {
                        Content = content1
                    };

                    HttpResponseMessage stateresp = client.SendAsync(httpRequest1).Result;
                    var staterespcnt = stateresp.Content.ReadAsStringAsync().Result;
                    doc = new HtmlDocument();
                    doc.LoadHtml(staterespcnt);
                    var primarylinks = doc.DocumentNode.SelectNodes("//a");
                    for (int i = 250; i < primarylinks.Count; i++)
                    {
                        ftolink = "https://nregastrep.nic.in/netnrega/" + primarylinks[i].Attributes["href"].Value;
                        if (primarylinks[i].Attributes["href"].Value.Contains("FTO/FTOReport.aspx?"))
                            break;
                    }
                }

                //string url = "https://nregastrep.nic.in/netnrega/Progofficer/PoIndexFrame.aspx?flag_debited=S&lflag=eng&District_Code=" + dist_code + "&district_name=" + dist_name + "&state_name=KARNATAKA&state_Code=15&finyear=" + finyear + "&check=1&block_name=" + block_name + "&Block_Code=" + block_code;

                var webreq = (HttpWebRequest)WebRequest.Create(ftolink);
                webreq.Method = "GET";
                webreq.CookieContainer = new CookieContainer();
                webreq.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });

                HttpWebResponse issueResponse = (HttpWebResponse)webreq.GetResponse();
                string blockcontent = new StreamReader(issueResponse.GetResponseStream()).ReadToEnd();

                doc = new HtmlDocument();
                doc.LoadHtml(blockcontent);

                var musterlink = doc.DocumentNode.SelectNodes("//table[2]//tr//td[2]//a");
                string requestdistMustlink = "";
                foreach (var item in musterlink)
                {
                    requestdistMustlink = "https://nregastrep.nic.in/netnrega/FTO/" + item.Attributes["href"].Value;
                    var parser = HttpUtility.ParseQueryString(requestdistMustlink);
                    if (parser != null && parser.Get("district_code") != null)
                    {
                        if (parser.Get("district_code") == dist_code)
                            break;
                    }
                }

                var requestblock = (HttpWebRequest)WebRequest.Create(requestdistMustlink);
                requestblock.CookieContainer = new CookieContainer();
                requestblock.Method = "GET";
                requestblock.Timeout = 10000;
                requestblock.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });
                string emusterresp = new StreamReader(((HttpWebResponse)requestblock.GetResponse()).GetResponseStream()).ReadToEnd();

                doc = new HtmlDocument();
                doc.LoadHtml(emusterresp);
                var requestblocklinks = doc.DocumentNode.SelectNodes("//table[2]//tr//td[2]//a");
                string requestblocklink = "";
                foreach (var item in requestblocklinks)
                {
                    requestblocklink = "https://nregastrep.nic.in/netnrega/FTO/" + item.Attributes["href"].Value;
                    var parser = HttpUtility.ParseQueryString(requestblocklink);
                    if (parser.Get("block_code") == block_code)
                        break;
                }


                var requestpanch = (HttpWebRequest)WebRequest.Create(requestblocklink);
                requestpanch.CookieContainer = new CookieContainer();
                requestpanch.Method = "GET";
                requestpanch.Timeout = 10000;
                requestpanch.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });
                string requestpanchres = new StreamReader(((HttpWebResponse)requestpanch.GetResponse()).GetResponseStream()).ReadToEnd();

                doc = new HtmlDocument();
                doc.LoadHtml(requestpanchres);
                var requestpanchlinks = doc.DocumentNode.SelectNodes("//table[2]//tr//td[6]//a");
                string requestpanchlink = "";
                foreach (var item in requestpanchlinks)
                {
                    requestpanchlink = "https://nregastrep.nic.in/netnrega/FTO/" + item.Attributes["href"].Value;
                    var parser = HttpUtility.ParseQueryString(requestpanchlink);
                    if (parser.Get("panchayat_code") == pcode)
                        break;
                }

                var panchfto = (HttpWebRequest)WebRequest.Create(requestpanchlink);
                panchfto.Method = "GET";
                panchfto.CookieContainer = new CookieContainer();
                panchfto.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });

                string requestedpanchres = new StreamReader(((HttpWebResponse)panchfto.GetResponse()).GetResponseStream()).ReadToEnd();

                Response.Write(requestedpanchres);
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