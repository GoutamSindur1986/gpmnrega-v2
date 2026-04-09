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

namespace gpmnrega2.Registers
{
    public partial class MaterialFTODetails : System.Web.UI.Page
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

                var webreq = (HttpWebRequest)WebRequest.Create(ftolink);
                webreq.Method = "GET";
                webreq.CookieContainer = new CookieContainer();
                webreq.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });

                HttpWebResponse issueResponse = (HttpWebResponse)webreq.GetResponse();
                string blockcontent = new StreamReader(issueResponse.GetResponseStream()).ReadToEnd();

                doc = new HtmlDocument();
                doc.LoadHtml(blockcontent);
                string lflag = "lflag=eng&";
                string __EVENTTARGET1 = "__EVENTTARGET=ctl00$ContentPlaceHolder1$RBtnLst$1&";
                string __EVENTARGUMENT1 = "__EVENTARGUMENT=&";
                string __LASTFOCUS1 = "__LASTFOCUS=&";
                string __VIEWSTATE1 = "__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "")) + "&";
                string __VIEWSTATEGENERATOR1 = "__VIEWSTATEGENERATOR=" + doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "") + "&";
                string __SCROLLPOSITIONX1 = "__SCROLLPOSITIONX=0&";
                string __SCROLLPOSITIONY1 = "__SCROLLPOSITIONY=0&";
                string __VIEWSTATEENCRYPTED1 = "__VIEWSTATEENCRYPTED=&";
                string RBtnLst1 = "ctl00$ContentPlaceHolder1$RBtnLst=M&";
                string RBtnLstIsEfms1 = "ctl00$ContentPlaceHolder1$RBtnLstIsEfms=B&";
                string HiddenField1 = "ctl00$ContentPlaceHolder1$HiddenField1=";

                string postdata = __EVENTTARGET1 + __EVENTARGUMENT1 + __LASTFOCUS1 + __VIEWSTATE1 + __SCROLLPOSITIONY1 + __SCROLLPOSITIONX1 + __VIEWSTATEENCRYPTED1 + __VIEWSTATEGENERATOR1 + RBtnLst1 + RBtnLstIsEfms1 + HiddenField1;
                //request2
                var request1 = (HttpWebRequest)HttpWebRequest.Create(ftolink);
                //request1.CookieContainer = new CookieContainer();
                // request1.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });
                request1.AllowAutoRedirect = true;
                request1.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
                request1.Method = "POST";
                var data = Encoding.UTF8.GetBytes(postdata);
                request1.ContentType = "application/x-www-form-urlencoded";
                request1.ContentLength = data.Length;
                using (var sr = request1.GetRequestStream())
                {
                    sr.Write(data, 0, data.Length);
                }
                var issueResponseM = (HttpWebResponse)request1.GetResponse();

                var responseString = new StreamReader(issueResponseM.GetResponseStream()).ReadToEnd();

                doc.LoadHtml(responseString);

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