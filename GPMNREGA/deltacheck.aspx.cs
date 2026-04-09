using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Http;
using HtmlAgilityPack;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using System.Configuration;

namespace gpmnrega2.templates
{
    public partial class deltacheck : System.Web.UI.Page
    {
        private static readonly HttpClient _HttpClient = new HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // string first_query = "id=2&lflag=local&ExeL=GP";
                string state_code = Request.Params["state_code"].ToString();
                string dist_code = Request.Params["dist_code"].ToString();
                string block_code = Request.Params["block_code"].ToString();
                string panch_code = Request.Params["panch_code"].ToString();
                string fin_year = Request.Params["fin_year"].ToString();
                string work_code = Request.Params["work_code"].ToString();
                string state_name = Request.Params["state_name"].ToString();
                string dist_name = Request.Params["dist_name"].ToString();
                string block_name = Request.Params["block_name"].ToString();
                string panch_name = Request.Params["panch_name"].ToString();
                List<string> nmrLoaded = new List<string>();

                using (StreamReader sr = new StreamReader(Request.InputStream))
                {
                    nmrLoaded = sr.ReadToEnd().Split(',').ToList();
                }

                string finyear = ConfigurationManager.AppSettings["finyear"].ToString();
                HttpClient client = new HttpClient();

                var request = (HttpWebRequest)HttpWebRequest.Create("https://nregastrep.nic.in/netnrega/IndexFrame.aspx?lflag=eng&District_Code=" + dist_code + "&district_name=" + dist_name + "&state_name=KARNATAKA&state_Code=15&block_name=" + block_name + "&block_code=" + block_code + "&fin_year=" + finyear + "&check=1&Panchayat_name=" + panch_name + "&Panchayat_Code=" + panch_code);
                request.AllowAutoRedirect = true;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";

                string content1 = "";
                try
                {
                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        content1 = streamReader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    gpmnrega2.Utility.Utility.SendErrorToText(ex);
                }
                HtmlDocument doc = new HtmlDocument() { OptionUseIdAttribute = true };
                doc.LoadHtml(content1);

                var musterrolllink = doc.DocumentNode.SelectNodes("//a");
                string mustlink = "";// doc.DocumentNode.SelectNodes("//a")[20].Attributes["href"].Value;

                for (int i = 0; i < musterrolllink.Count; i++)
                {
                    if (musterrolllink[i].InnerText.Trim() == "Muster Roll")
                        mustlink = musterrolllink[i].Attributes["href"].Value;
                }

                var mustreq = (HttpWebRequest)HttpWebRequest.Create("https://nregastrep.nic.in/netnrega/" + mustlink);
                mustreq.AllowAutoRedirect = true;
                mustreq.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
                mustreq.Method = "GET";

                mustreq.ContentType = "application/x-www-form-urlencoded";

                var issueMust = (HttpWebResponse)mustreq.GetResponse();

                var responsemust = new StreamReader(issueMust.GetResponseStream()).ReadToEnd();

                doc = new HtmlDocument() { OptionUseIdAttribute = true };
                doc.LoadHtml(responsemust);

                string __EVENTTARGET = "__EVENTTARGET=ctl00$ContentPlaceHolder1$btnprin";
                string __EVENTARGUMENT = "&__EVENTARGUMENT&";
                string __LASTFOCUS = "&_LASTFOCUS=&";
                string __VIEWSTATE = "&__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "")) + "&";
                string __VIEWSTATEGENERATOR = "&__VIEWSTATEGENERATOR=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "")) + "&";
                string __SCROLLPOSITIONX = "__SCROLLPOSITIONX=0&";
                string __SCROLLPOSITIONY = "__SCROLLPOSITIONY=0&";
                string __VIEWSTATEENCRYPTED = "__VIEWSTATEENCRYPTED=&";
                string __EVENTVALIDATION = "__EVENTVALIDATION=" + Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "")) + "&";
                string ddlFinYear = "ctl00$ContentPlaceHolder1$ddlFinYear=2024-2025&";
                string btnfill = "ctl00$ContentPlaceHolder1$btnfill=btnfill&";
                string btnprin = "ctl00$ContentPlaceHolder1$btnprin=btnprin&";
                string txtSearch = "ctl00$ContentPlaceHolder1$txtSearch=&";
                string ddlwork = "ctl00$ContentPlaceHolder1$ddlwork=---select---&";
                string ddlMsrno = "ctl00$ContentPlaceHolder1$ddlMsrno=---select---";



                string postdata = __EVENTTARGET + __EVENTARGUMENT + __EVENTVALIDATION + __LASTFOCUS + __VIEWSTATE + __VIEWSTATEENCRYPTED + __VIEWSTATEGENERATOR + btnfill + btnprin + ddlFinYear + ddlwork + txtSearch + ddlMsrno;
                //request2
                var request1 = (HttpWebRequest)HttpWebRequest.Create("https://nregastrep.nic.in/netnrega/" + mustlink);
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
                var issueResponse = (HttpWebResponse)request1.GetResponse();

                var responseString = new StreamReader(issueResponse.GetResponseStream()).ReadToEnd();



                StringContent content = new StringContent(postdata, Encoding.UTF8, "application/x-www-form-urlencoded");

                // HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://nregastrep.nic.in/netnrega/Citizen_html/Musternew.aspx?id=2&lflag=local&ExeL=GP&fin_year=" + fin_year + "&state_code=" + state_code + "&district_code=" + dist_code + "&block_code=" + block_code + "&panchayat_code=" + panch_code + "&State_name=" + state_name + "&District_name=" + dist_name + "&Block_name=" + block_name + "&panchayat_name=" + panch_name + "&Digest=o0HkiofYRQA4s2CxVZUqxw") { Content = content };
                // HttpResponseMessage issueresp = client.SendAsync(requestMessage).Result;

                doc = new HtmlDocument() { OptionUseIdAttribute = true };

                doc.LoadHtml(responseString);

                string async = "__ASYNCPOST=true";
                __EVENTARGUMENT = "&__EVENTARGUMENT=";
                __EVENTTARGET = "&__EVENTTARGET=ctl00$ContentPlaceHolder1$ddlwork";
                ddlwork = "&ctl00$ContentPlaceHolder1$ddlwork=" + Server.UrlEncode(work_code.ToUpper());
                txtSearch = "&ctl00$ContentPlaceHolder1$txtSearch=" + Server.UrlEncode(work_code.ToUpper());
                string ScriptManager1 = "&ctl00$ContentPlaceHolder1$ScriptManager1=ctl00$ContentPlaceHolder1$ScriptManager1|ctl00$ContentPlaceHolder1$ddlwork";
                __EVENTVALIDATION = "&__EVENTVALIDATION=" + Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", ""));
                __VIEWSTATE = "&__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", ""));
                __VIEWSTATEENCRYPTED = "&__VIEWSTATEENCRYPTED=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEENCRYPTED").GetAttributeValue("value", ""));
                __VIEWSTATEGENERATOR = "&__VIEWSTATEGENERATOR=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", ""));
                __LASTFOCUS = "&__LASTFOCUS=";
                ddlMsrno = "&ctl00$ContentPlaceHolder1$ddlMsrno=---Select---";
                btnprin = "&ctl00$ContentPlaceHolder1$btnprin=btnprin";
                ddlFinYear = "&ctl00$ContentPlaceHolder1$ddlFinYear=2024-2025";
                __SCROLLPOSITIONX = "&__SCROLLPOSITIONX=0";
                __SCROLLPOSITIONY = "&__SCROLLPOSITIONY=0";
                string postdata1 = async + __EVENTARGUMENT + __EVENTTARGET + __EVENTVALIDATION + __LASTFOCUS + __VIEWSTATE + ScriptManager1 + __VIEWSTATEENCRYPTED + __VIEWSTATEGENERATOR + btnprin + ddlFinYear + ddlwork + txtSearch + ddlMsrno + __SCROLLPOSITIONX + __SCROLLPOSITIONY;

                var request2 = (HttpWebRequest)HttpWebRequest.Create("https://nregastrep.nic.in/netnrega/" + mustlink);
                request2.AllowAutoRedirect = true;
                request2.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
                request2.Method = "POST";
                var data1 = Encoding.UTF8.GetBytes(postdata1);
                request2.ContentType = "application/x-www-form-urlencoded";
                request2.ContentLength = data1.Length;
                using (var sr = request2.GetRequestStream())
                {
                    sr.Write(data1, 0, data1.Length);
                }
                var workResponse = (HttpWebResponse)request2.GetResponse();

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
                    try
                    {

                        if (nmrLoaded.IndexOf(msrno[i].InnerText.Split('~')[0].Trim()) == -1)
                        {
                            ddlMsrno = "&ctl00$ContentPlaceHolder1$ddlMsrno=" + msrno[i].InnerText;
                            string postmsrdata = async + __EVENTARGUMENT + __EVENTTARGET + __EVENTVALIDATION + __LASTFOCUS + __VIEWSTATE + ScriptManager1 + __VIEWSTATEENCRYPTED + __VIEWSTATEGENERATOR + btnprin + ddlFinYear + ddlMsrno + ddlwork + txtSearch;

                            var reqmsr = (HttpWebRequest)HttpWebRequest.Create("https://nregastrep.nic.in/netnrega/" + mustlink);
                            reqmsr.AllowAutoRedirect = true;
                            reqmsr.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
                            reqmsr.Method = "POST";
                            var datamsr = Encoding.UTF8.GetBytes(postmsrdata);
                            reqmsr.ContentType = "application/x-www-form-urlencoded";
                            reqmsr.ContentLength = datamsr.Length;
                            using (var sr = reqmsr.GetRequestStream())
                            {
                                sr.Write(datamsr, 0, datamsr.Length);
                            }
                            var msrresponse = (HttpWebResponse)reqmsr.GetResponse();

                            var msrrespString = new StreamReader(msrresponse.GetResponseStream()).ReadToEnd();

                            nmrs += msrrespString;
                        }

                    }
                    catch (Exception ex)
                    {
                        Response.Write(ex.Message);

                    }
                }

                Response.Write(nmrs);
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



