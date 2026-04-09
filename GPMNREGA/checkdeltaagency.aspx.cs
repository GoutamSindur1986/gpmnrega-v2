using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace gpmnrega2.templates
{
    public partial class checkdeltaagency : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            string dist_code = Request.Params["dist_code"].ToString();
            string block_code = Request.Params["block_code"].ToString();

            string work_code = Request.Params["work_code"].ToString();


            string dist_name = Request.Params["dist_name"].ToString();
            string block_name = Request.Params["block_name"].ToString();
            string finyear = ConfigurationManager.AppSettings["finyear"].ToString();
            string fin1 = finyear.Split('-')[0];
            List<string> nmrLoaded = new List<string>();

            using (StreamReader sr = new StreamReader(Request.InputStream))
            {
                nmrLoaded = sr.ReadToEnd().Split(',').ToList();
            }

            string agencyLink = "https://nregastrep.nic.in/Netnrega/citizen_html/mustlogin.aspx?lflag=eng&page=C&state_code=15&Digest=qHd1FqX1X7mb3mOaDUib0g";


            if (Request["IsAgency"] != null)
            {
                if (Request["IsAgency"].ToString() == "true")
                {
                    try
                    {
                    indexresp:
                        HttpResponseMessage indexresp = new HttpResponseMessage();
                        HttpClient client = new HttpClient();

                        indexresp = client.GetAsync(agencyLink).Result;
                        if (indexresp.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            goto indexresp;
                        }


                        string agencyFin = indexresp.Content.ReadAsStringAsync().Result;

                        HtmlDocument doc = new HtmlDocument() { OptionUseIdAttribute = true };
                        doc.LoadHtml(agencyFin);

                        string placeholder = "ctl00$ContentPlaceHolder1$ScriptManager1=ctl00$ContentPlaceHolder1$update|ctl00$ContentPlaceHolder1$ddlFin";
                        string dllfin = "&ctl00$ContentPlaceHolder1$ddlFin="+fin1;
                        string ddldist = "&ctl00$ContentPlaceHolder1$ddldistrict=0";
                        string ddlblock = "&ctl00$ContentPlaceHolder1$ddlblock=0";
                        string viewgen = "&__VIEWSTATEGENERATOR=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", ""));
                        string viewstateencry = "&__VIEWSTATEENCRYPTED=";
                        string viewstate = "&__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", ""));
                        string sy = "&__SCROLLPOSITIONY=0";
                        string sx = "&__SCROLLPOSITIONX=0";
                        string lastfocus = "&__LASTFOCUS=";
                        string evntvalidation = "&__EVENTVALIDATION=" + Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", ""));
                        string evnttarget = "&__EVENTTARGET=ctl00$ContentPlaceHolder1$ddlFin";
                        string eventarg = "&__EVENTARGUMENT=";
                        string async = "&__ASYNCPOST=false";

                        //****************************************************************************************************

                        var request = (HttpWebRequest)System.Net.HttpWebRequest.Create(agencyLink);
                        request.AllowAutoRedirect = true;
                        request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
                        request.Method = "POST";
                        string postdata = placeholder + dllfin + ddldist + ddlblock + viewgen + viewstateencry + viewstate + sy + sx + lastfocus + evnttarget + eventarg + async + evntvalidation + "&";

                        var data = Encoding.UTF8.GetBytes(postdata);
                        request.ContentType = "application/x-www-form-urlencoded";
                        request.ContentLength = data.Length;

                        using (var sr = request.GetRequestStream())
                        {
                            sr.Write(data, 0, data.Length);
                        }
                    issueResponse5:
                        HttpWebResponse issueResponse;

                        issueResponse = (HttpWebResponse)request.GetResponse();



                        if (issueResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            goto issueResponse5;
                        }

                        var agencydist = new StreamReader(issueResponse.GetResponseStream()).ReadToEnd();


                        //*********************************************************************************************

                        doc = new HtmlDocument() { OptionUseIdAttribute = true };
                        doc.LoadHtml(agencydist);

                        string[] distvalues = agencydist.Split('|');

                        placeholder = "ctl00$ContentPlaceHolder1$ScriptManager1=ctl00$ContentPlaceHolder1$update|ctl00$ContentPlaceHolder1$ddldistrict";
                        dllfin = "&ctl00$ContentPlaceHolder1$ddlFin="+ fin1;
                        ddldist = "&ctl00$ContentPlaceHolder1$ddldistrict=" + dist_code;
                        ddlblock = "&ctl00$ContentPlaceHolder1$ddlblock=0";
                        viewgen = "&__VIEWSTATEGENERATOR=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", ""));
                        viewstateencry = "&__VIEWSTATEENCRYPTED=";
                        viewstate = "&__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", ""));
                        sy = "&__SCROLLPOSITIONY=0";
                        sx = "&__SCROLLPOSITIONX=0";
                        lastfocus = "&__LASTFOCUS=";
                        evntvalidation = "&__EVENTVALIDATION=" + Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", ""));
                        evnttarget = "&__EVENTTARGET=ctl00$ContentPlaceHolder1$ddldistrict";
                        eventarg = "&__EVENTARGUMENT=";
                        async = "&__ASYNCPOST=false";


                        request = (HttpWebRequest)System.Net.HttpWebRequest.Create(agencyLink);
                        request.AllowAutoRedirect = true;
                        request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
                        request.Method = "POST";
                        string session = issueResponse.Headers.Get("Set-Cookie").Split('=')[1].Split(';')[0];
                        request.CookieContainer = new CookieContainer();

                        request.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });

                        postdata = placeholder + dllfin + ddldist + ddlblock + viewgen + viewstateencry + viewstate + sy + sx + lastfocus + evnttarget + eventarg + async + evntvalidation + "&";

                        data = Encoding.UTF8.GetBytes(postdata);
                        request.ContentType = "application/x-www-form-urlencoded";
                        request.ContentLength = data.Length;

                        using (var sr = request.GetRequestStream())
                        {
                            sr.Write(data, 0, data.Length);
                        }
                    issueResponse1:

                        issueResponse = (HttpWebResponse)request.GetResponse();
                        if (issueResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            goto issueResponse1;
                        }


                        string agencyblockhtml = new StreamReader(issueResponse.GetResponseStream()).ReadToEnd();

                        doc = new HtmlDocument() { OptionUseIdAttribute = true };
                        doc.LoadHtml(agencyblockhtml);

                        string[] agencyBlock = new StreamReader(issueResponse.GetResponseStream()).ReadToEnd().Split('|');

                        placeholder = "ctl00$ContentPlaceHolder1$ScriptManager1=ctl00$ContentPlaceHolder1$update|ctl00$ContentPlaceHolder1$ddlblock";
                        dllfin = "&ctl00$ContentPlaceHolder1$ddlFin="+fin1;
                        ddldist = "&ctl00$ContentPlaceHolder1$ddldistrict=" + dist_code;
                        ddlblock = "&ctl00$ContentPlaceHolder1$ddlblock=" + block_code;
                        viewgen = "&__VIEWSTATEGENERATOR=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", ""));
                        viewstateencry = "&__VIEWSTATEENCRYPTED=";
                        viewstate = "&__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", ""));
                        sy = "&__SCROLLPOSITIONY=0";
                        sx = "&__SCROLLPOSITIONX=0";
                        lastfocus = "&__LASTFOCUS=";
                        evntvalidation = "&__EVENTVALIDATION=" + Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", ""));
                        evnttarget = "&__EVENTTARGET=ctl00$ContentPlaceHolder1$ddlblock";
                        eventarg = "&__EVENTARGUMENT=";
                        async = "&__ASYNCPOST=false";

                        request = (HttpWebRequest)System.Net.HttpWebRequest.Create(agencyLink);
                        request.AllowAutoRedirect = true;
                        request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
                        request.Method = "POST";
                        postdata = placeholder + dllfin + ddldist + ddlblock + viewgen + viewstateencry + viewstate + sy + sx + lastfocus + evnttarget + eventarg + async + evntvalidation + "&";

                        data = Encoding.UTF8.GetBytes(postdata);
                        request.ContentType = "application/x-www-form-urlencoded";
                        //session = issueResponse.Headers.Get("Set-Cookie").Split('=')[1].Split(';')[0];
                        request.CookieContainer = new CookieContainer();

                        request.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });
                        request.ContentLength = data.Length;


                        using (var sr = request.GetRequestStream())
                        {
                            sr.Write(data, 0, data.Length);
                        }
                    issueResponse2:

                        issueResponse = (HttpWebResponse)request.GetResponse();
                        if (issueResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            goto issueResponse2;
                        }


                        string agencyProceedhtml = new StreamReader(issueResponse.GetResponseStream()).ReadToEnd();

                        doc = new HtmlDocument() { OptionUseIdAttribute = true };
                        doc.LoadHtml(agencyProceedhtml);

                        string[] proceed = new StreamReader(issueResponse.GetResponseStream()).ReadToEnd().Split('|');
                        placeholder = "ctl00$ContentPlaceHolder1$ScriptManager1=ctl00$ContentPlaceHolder1$update|ctl00$ContentPlaceHolder1$btProceed";
                        dllfin = "&ctl00$ContentPlaceHolder1$ddlFin=" + fin1;
                        ddldist = "&ctl00$ContentPlaceHolder1$ddldistrict=" + dist_code;
                        ddlblock = "&ctl00$ContentPlaceHolder1$ddlblock=" + block_code;
                        viewgen = "&__VIEWSTATEGENERATOR=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", ""));
                        viewstateencry = "&__VIEWSTATEENCRYPTED=";
                        viewstate = "&__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", ""));
                        sy = "&__SCROLLPOSITIONY=0";
                        sx = "&__SCROLLPOSITIONX=0";
                        lastfocus = "&__LASTFOCUS=";
                        evntvalidation = "&__EVENTVALIDATION=" + Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", ""));
                        evnttarget = "&__EVENTTARGET=ctl00$ContentPlaceHolder1$btProceed";
                        eventarg = "&__EVENTARGUMENT=";
                        async = "&__ASYNCPOST=false";
                        string btnproceed = "&ctl00$ContentPlaceHolder1$btProceed=Proceed";


                        request = (HttpWebRequest)System.Net.HttpWebRequest.Create(agencyLink);
                        request.AllowAutoRedirect = true;
                        request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
                        request.Method = "POST";
                        postdata = placeholder + dllfin + ddldist + ddlblock + viewgen + viewstateencry + viewstate + sy + sx + lastfocus + evnttarget + eventarg + async + evntvalidation + btnproceed + "&";

                        data = Encoding.UTF8.GetBytes(postdata);
                        request.ContentType = "application/x-www-form-urlencoded";
                        //session = issueResponse.Headers.Get("Set-Cookie").Split('=')[1].Split(';')[0];
                        request.CookieContainer = new CookieContainer();

                        request.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });
                        request.ContentLength = data.Length;

                        using (var sr = request.GetRequestStream())
                        {
                            sr.Write(data, 0, data.Length);
                        }
                    issueResponse3:

                        issueResponse = (HttpWebResponse)request.GetResponseAsync().Result;
                        if (issueResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            goto issueResponse3;
                        }

                        string selectIssue = new StreamReader(issueResponse.GetResponseStream()).ReadToEnd();

                        doc = new HtmlDocument() { OptionUseIdAttribute = true };
                        doc.LoadHtml(selectIssue);

                        //*********************get request end


                        string __EVENTARGUMENT = "__EVENTARGUMENT=";
                        string __EVENTTARGET = "&__EVENTTARGET=ctl00$ContentPlaceHolder1$btnprin";
                        string __EVENTVALIDATION = "&__EVENTVALIDATION="+ Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", ""));
                        string __LASTFOCUS = "&__LASTFOCUS=";
                        string __VIEWSTATE = "&__VIEWSTATE="+ Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", ""));
                        string __VIEWSTATEENCRYPTED = "&__VIEWSTATEENCRYPTED=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEENCRYPTED").GetAttributeValue("value", ""));
                        string __VIEWSTATEGENERATOR = "&__VIEWSTATEGENERATOR=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", ""));
                        string btnfill = "&ctl00$ContentPlaceHolder1$btnfill=btnfill";
                        string btnprin = "&ctl00$ContentPlaceHolder1$btnprin=btnprin";
                        string ddlFinYear = "&ctl00$ContentPlaceHolder1$ddlFinYear=" + ConfigurationManager.AppSettings["finyear"];
                        string ddlMsrno = "&ctl00$ContentPlaceHolder1$ddlMsrno=---select---";
                        string ddlwork = "&ctl00$ContentPlaceHolder1$ddlwork=---select---";
                        string txtSearch = "&ctl00$ContentPlaceHolder1$txtSearch=";

                        postdata = __EVENTARGUMENT + __EVENTTARGET + __EVENTVALIDATION + __LASTFOCUS + __VIEWSTATE + __VIEWSTATEENCRYPTED + __VIEWSTATEGENERATOR + btnfill + btnprin + ddlFinYear + ddlMsrno + ddlwork + txtSearch;
                        string digest = doc.GetElementbyId("aspnetForm").Attributes["action"].Value.Split(new string[] { "Digest=" }, StringSplitOptions.None)[1];

                        string agencyLink2 = issueResponse.ResponseUri.AbsoluteUri; //"https://nregastrep.nic.in/Netnrega/citizen_html/musternew.aspx?state_name=KARNATAKA&district_name=" + dist_name + "&block_name=" + block_name + "&state_code=15&district_code=" + dist_code + "&fin_year=" + ConfigurationManager.AppSettings["finyear"].ToString() + "&Digest=" + digest;

                        //request2
                        var request1 = (HttpWebRequest)HttpWebRequest.Create(agencyLink2);
                        request1.AllowAutoRedirect = true;
                        request1.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.57 Safari/537.17";
                        request1.Method = "POST";
                        data = Encoding.UTF8.GetBytes(postdata);
                        request1.ContentType = "application/x-www-form-urlencoded";
                        request1.ContentLength = data.Length;

                        request1.CookieContainer = new CookieContainer();

                        request1.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });

                        using (var sr = request1.GetRequestStream())
                        {
                            sr.Write(data, 0, data.Length);
                        }
                    issueResponse4:

                        issueResponse = (HttpWebResponse)request1.GetResponse();
                        if (issueResponse.StatusCode == HttpStatusCode.ServiceUnavailable) { goto issueResponse4; }

                        var responseString = new StreamReader(issueResponse.GetResponseStream()).ReadToEnd();

                        StringContent content = new StringContent(postdata, Encoding.UTF8, "application/x-www-form-urlencoded");

                        doc = new HtmlDocument() { OptionUseIdAttribute = true };

                        doc.LoadHtml(responseString);

                        async = "&__ASYNCPOST=true";

                        __EVENTTARGET = "&__EVENTTARGET=ctl00$ContentPlaceHolder1$ddlwork";
                        ddlwork = "&ctl00$ContentPlaceHolder1$ddlwork=" + Server.UrlEncode(work_code.ToUpper());
                        string ScriptManager1 = "&ctl00$ContentPlaceHolder1$ScriptManager1=ctl00$ContentPlaceHolder1$ScriptManager1|ctl00$ContentPlaceHolder1$ddlwork";
                        __EVENTVALIDATION = "&__EVENTVALIDATION=" + Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", ""));
                        __VIEWSTATE = "&__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", ""));
                        __VIEWSTATEENCRYPTED = "&__VIEWSTATEENCRYPTED=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEENCRYPTED").GetAttributeValue("value", ""));
                        __VIEWSTATEGENERATOR = "&__VIEWSTATEGENERATOR=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", ""));

                        string postdata1 = __EVENTARGUMENT + __EVENTTARGET + __EVENTVALIDATION + __LASTFOCUS + __VIEWSTATE + ScriptManager1 + __VIEWSTATEENCRYPTED + __VIEWSTATEGENERATOR + btnprin + ddlFinYear + ddlwork + txtSearch + async + "&";

                        var request2 = (HttpWebRequest)HttpWebRequest.Create(agencyLink2);
                        request2.AllowAutoRedirect = true;
                        request2.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.57 Safari/537.17";
                        request2.Method = "POST";
                        var data1 = Encoding.UTF8.GetBytes(postdata1);
                        request2.ContentType = "application/x-www-form-urlencoded";
                        request2.ContentLength = data1.Length;
                        request2.CookieContainer = new CookieContainer();

                        request2.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });

                        using (var sr = request2.GetRequestStream())
                        {
                            sr.Write(data1, 0, data1.Length);
                        }
                    workResponse:
                        HttpWebResponse workResponse;

                        workResponse = (HttpWebResponse)request2.GetResponse();
                        if (workResponse.StatusCode == HttpStatusCode.ServiceUnavailable) { goto workResponse; }


                        var workrespString = new StreamReader(workResponse.GetResponseStream()).ReadToEnd();
                        string[] workresp = workrespString.Split('|');

                        doc = new HtmlDocument() { OptionUseIdAttribute = true };

                        doc.LoadHtml(workrespString);

                        __EVENTTARGET = "&__EVENTTARGET=ctl00$ContentPlaceHolder1$ddlMsrno";
                        ScriptManager1 = "&ctl00$ContentPlaceHolder1$ScriptManager1=ctl00$ContentPlaceHolder1$UpdatePanel2|ctl00$ContentPlaceHolder1$ddlMsrno";
                        __EVENTVALIDATION = "&__EVENTVALIDATION=" + Server.UrlEncode(workresp[Array.IndexOf(workresp, "__EVENTVALIDATION") + 1]);
                        __VIEWSTATE = "&__VIEWSTATE=" + Server.UrlEncode(workresp[Array.IndexOf(workresp, "__VIEWSTATE") + 1]);
                        __VIEWSTATEENCRYPTED = "&__VIEWSTATEENCRYPTED=" + Server.UrlEncode(workresp[Array.IndexOf(workresp, "__VIEWSTATEENCRYPTED") + 1]);
                        __VIEWSTATEGENERATOR = "&__VIEWSTATEGENERATOR=" + Server.UrlEncode(workresp[Array.IndexOf(workresp, "__VIEWSTATEGENERATOR") + 1]);

                        HtmlNodeCollection msrno = new HtmlNodeCollection(doc.DocumentNode);
                        if (doc.GetElementbyId("ctl00_ContentPlaceHolder1_ddlMsrno") != null)
                            msrno = doc.GetElementbyId("ctl00_ContentPlaceHolder1_ddlMsrno").SelectNodes("option");

                        string nmrs = "";
                        string nmr = "{'NMRNO':{0}, 'DateFrom':{1}, 'DateTo':{2}, 'JC':[{3}]}";
                        string jcno = "{'JCNO':{0}, 'appName':{1}, 'appAddress':{2}},";

                        for (int i = 1; i < msrno.Count; i++)
                        {
                            if (nmrLoaded.IndexOf(msrno[i].InnerText.Split('~')[0].Trim()) == -1)
                            {

                                ddlMsrno = "&ctl00$ContentPlaceHolder1$ddlMsrno=" + msrno[i].InnerText;
                                string postmsrdata = __EVENTARGUMENT + __EVENTTARGET + __EVENTVALIDATION + __LASTFOCUS + __VIEWSTATE + ScriptManager1 + __VIEWSTATEENCRYPTED + __VIEWSTATEGENERATOR + btnprin + ddlFinYear + ddlMsrno + ddlwork + txtSearch + async + "&";


                                var reqmsr = (HttpWebRequest)HttpWebRequest.Create(agencyLink2);
                                reqmsr.AllowAutoRedirect = true;
                                reqmsr.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.57 Safari/537.17";
                                reqmsr.Method = "POST";
                                var datamsr = Encoding.UTF8.GetBytes(postmsrdata);
                                reqmsr.ContentType = "application/x-www-form-urlencoded";
                                reqmsr.ContentLength = datamsr.Length;

                                reqmsr.CookieContainer = new CookieContainer();

                                reqmsr.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });

                                using (var sr = reqmsr.GetRequestStream())
                                {
                                    sr.Write(datamsr, 0, datamsr.Length);
                                }
                            Response:
                                HttpWebResponse msrresponse;

                                msrresponse = (HttpWebResponse)reqmsr.GetResponse();
                                if (msrresponse.StatusCode == HttpStatusCode.ServiceUnavailable)
                                {
                                    goto Response;
                                }

                                var msrrespString = new StreamReader(msrresponse.GetResponseStream()).ReadToEnd();

                                nmrs += msrrespString;
                            }
                        }
                        Response.Write(nmrs);

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
    }
}