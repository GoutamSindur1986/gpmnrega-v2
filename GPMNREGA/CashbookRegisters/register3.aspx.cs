using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Http;
using System.Net;
using HtmlAgilityPack;
using System.Configuration;

namespace gpmnrega2.Registers
{
    public partial class register3 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Dictionary<string, string> request = new Dictionary<string, string>();
            try
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                string finyear = Request.Params["finyear"].ToString();
                string from = Request.Params["from"].ToString();
                string to = Request.Params["to"].ToString();
                string distcode = Request.Params["dist_code"].ToString();
                string blockcode = Request.Params["block_code"].ToString();
                string panchayatcode = Request.Params["panch"].ToString();
                string eventtarget = "";
                var finresp = new HttpResponseMessage();
                HttpClient client = new HttpClient();

                HttpResponseMessage resp = client.GetAsync("https://mnregaweb4.nic.in/netnrega/SocialAudit/SA_LoginReport.aspx?id=15").Result;

                if (finyear != ConfigurationManager.AppSettings["finyear"].ToString())
                {
                    eventtarget = "ctl00$ContentPlaceHolder1$ddlFin";
                    client = new HttpClient();
                    var finreq = new HttpRequestMessage(HttpMethod.Post, "https://mnregaweb4.nic.in/netnrega/SocialAudit/SA_LoginReport.aspx?id=15")
                    {
                        Content = new FormUrlEncodedContent(fillRequest(resp.Content.ReadAsStringAsync().Result, eventtarget, finyear, "0", "0",
                        "0", "0", "0"))
                    };
                    finresp = client.SendAsync(finreq).Result;
                }

                eventtarget = "ctl00$ContentPlaceHolder1$ddldist";
                if (finyear != ConfigurationManager.AppSettings["finyear"].ToString())
                {
                    dict = fillRequest(finresp.Content.ReadAsStringAsync().Result, eventtarget, finyear, distcode, "0", "0", "0", "0");
                }
                else
                {
                    dict = fillRequest(resp.Content.ReadAsStringAsync().Result, eventtarget, finyear, distcode, "0", "0", "0", "0");
                }
                client = new HttpClient();
                var req = new HttpRequestMessage(HttpMethod.Post, "https://mnregaweb4.nic.in/netnrega/SocialAudit/SA_LoginReport.aspx?id=15")
                {
                    Content = new FormUrlEncodedContent(dict)
                };

                var res = client.SendAsync(req).Result;
                eventtarget = "ctl00$ContentPlaceHolder1$ddlblock";

                client = new HttpClient();
                var req1 = new HttpRequestMessage(HttpMethod.Post, "https://mnregaweb4.nic.in/netnrega/SocialAudit/SA_LoginReport.aspx?id=15")
                {
                    Content = new FormUrlEncodedContent(fillRequest(res.Content.ReadAsStringAsync().Result, eventtarget, finyear, distcode, blockcode,
                    "0", "0", "0"))
                };
                var res1 = client.SendAsync(req1).Result;

                eventtarget = "ctl00$ContentPlaceHolder1$ddlpanchayat";
                client = new HttpClient();
                var req2 = new HttpRequestMessage(HttpMethod.Post, "https://mnregaweb4.nic.in/netnrega/SocialAudit/SA_LoginReport.aspx?id=15")
                {
                    Content = new FormUrlEncodedContent(fillRequest(res1.Content.ReadAsStringAsync().Result, eventtarget, finyear, distcode, blockcode,
                    panchayatcode, "0", "0"))
                };
                var res2 = client.SendAsync(req2).Result;

                CookieContainer cookies = new CookieContainer();
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = cookies;

                eventtarget = "ctl00$ContentPlaceHolder1$rbLoginLevel$1";
                client = new HttpClient(handler);
                var req3 = new HttpRequestMessage(HttpMethod.Post, "https://mnregaweb4.nic.in/netnrega/SocialAudit/SA_LoginReport.aspx?id=15")
                {
                    Content = new FormUrlEncodedContent(fillRequest(res2.Content.ReadAsStringAsync().Result, eventtarget, finyear, distcode, blockcode,
                    panchayatcode, "0", "0"))
                };
                var res3 = client.SendAsync(req3).Result;



                eventtarget = "ctl00$ContentPlaceHolder1$login";
                cookies = new CookieContainer();
                handler = new HttpClientHandler();
                handler.CookieContainer = cookies;

                client = new HttpClient(handler);
                var req4 = new HttpRequestMessage(HttpMethod.Post, "https://mnregaweb4.nic.in/netnrega/SocialAudit/SA_LoginReport.aspx?id=15")
                {
                    Content = new FormUrlEncodedContent(fillRequest(res3.Content.ReadAsStringAsync().Result, eventtarget, finyear, distcode, blockcode,
                    panchayatcode, from, to))
                };
                HttpResponseMessage res4 = client.SendAsync(req4).Result;


                client = new HttpClient(handler);
                HttpResponseMessage message = client.GetAsync("https://mnregaweb4.nic.in/netnrega/SocialAudit/IndexFrame.aspx").Result;

                client = new HttpClient(handler);
                HttpResponseMessage consolidatepay = client.GetAsync("https://mnregaweb4.nic.in/netnrega/SocialAudit/Consolidate_pay_wrker.aspx").Result;

                Response.Write(consolidatepay.Content.ReadAsStringAsync().Result);
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

        public Dictionary<string, string> fillRequest(string resp, string eventtarget, string finyear, string dist, string block, string panchyat, string from, string to)
        {

            string dynamicsalt, viewstate, evetvalidation, viewstategenerator;
            Dictionary<string, string> request = new Dictionary<string, string>();
            HtmlDocument doc = new HtmlDocument() { OptionUseIdAttribute = true };
            doc.LoadHtml(resp);
            dynamicsalt = doc.GetElementbyId("ctl00_ContentPlaceHolder1_dynSalt").GetAttributeValue("value", "");
            viewstate = doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "");
            evetvalidation = doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "");
            viewstategenerator = doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "");
            request.Add("ctl00$ContentPlaceHolder1$ddlFin", finyear);

            request.Add("ctl00$ContentPlaceHolder1$ddldist", dist);

            request.Add("ctl00$ContentPlaceHolder1$ddlblock", block);

            request.Add("ctl00$ContentPlaceHolder1$ddlpanchayat", panchyat);
            if (eventtarget == "ctl00$ContentPlaceHolder1$login")
            {
                request.Add("ctl00$ContentPlaceHolder1$rbLoginLevel", "1");
                request.Add("ctl00$ContentPlaceHolder1$txtperiodFrom", from);
                request.Add("ctl00$ContentPlaceHolder1$txtperiodTo", to);
                request.Add("ctl00$ContentPlaceHolder1$login", "Get Reports");
            }

            if (eventtarget == "ctl00$ContentPlaceHolder1$rbLoginLevel$1")
                request.Add("ctl00$ContentPlaceHolder1$rbLoginLevel", "1");

            request.Add("ctl00$ScriptManager1", "ctl00$UpdatePanel1|" + eventtarget);

            if (eventtarget == "ctl00$ContentPlaceHolder1$login")
                request.Add("__EVENTTARGET", "");
            else
                request.Add("__EVENTTARGET", eventtarget);

            request.Add("__EVENTARGUMENT", "");

            request.Add("__LASTFOCUS", "");
            request.Add("__VIEWSTATE", viewstate);
            request.Add("__VIEWSTATEGENERATOR", viewstategenerator);

            request.Add("__VIEWSTATEENCRYPTED", "");
            request.Add("__EVENTVALIDATION", evetvalidation);
            request.Add("ctl00$ContentPlaceHolder1$dynSalt", dynamicsalt);
            return request;
        }
    }
}