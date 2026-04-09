using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using HtmlAgilityPack;
using System.Net.Http;
using static Google.Rpc.Help.Types;
using System.Drawing.Drawing2D;
using System.Text;
using System.Web.Http.Results;

namespace gpmnrega2.Registers
{
    public partial class assetregister : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                string distcode, blockcode, pcode, distname, blockname, pname, finyear;
                distname = Request.QueryString["district_name"];
                distcode = Request.QueryString["district_code"];
                blockcode = Request.QueryString["block_code"];
                blockname = Request.QueryString["block_name"];
                pcode = Request.QueryString["panchayat_code"];
                pname = Request.QueryString["panchayat_name"];
                finyear = Request.QueryString["fin_year"];

                string currentfin = "";
                HttpClient client = new HttpClient();
                string url = "https://nreganarep.nic.in/netnrega/MISreport4.aspx";
                HttpResponseMessage response = client.GetAsync(url).Result;
                var misresp = response.Content.ReadAsStringAsync().Result;
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(misresp);

                #region postlogin

                string __VIEWSTATE = "__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "")) + "&";
                string __VIEWSTATEGENERATOR = "__VIEWSTATEGENERATOR=" + doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "") + "&";
                string __EVENTVALIDATION = "__EVENTVALIDATION=" + Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "")) + "&";
                string txtCaptcha = "ctl00$ContentPlaceHolder1$txtCaptcha=" + doc.GetElementbyId("ContentPlaceHolder1_hfCaptcha").GetAttributeValue("value", "") + "&";
                string hfCaptcha = "ctl00$ContentPlaceHolder1$hfCaptcha=" + doc.GetElementbyId("ContentPlaceHolder1_hfCaptcha").GetAttributeValue("value", "") + "&";
                string btnLogin = "ctl00$ContentPlaceHolder1$btnLogin=Verify Code";
                string body = __VIEWSTATE + __VIEWSTATEGENERATOR + __EVENTVALIDATION + txtCaptcha + hfCaptcha + btnLogin;
                HttpContent content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content

                };

                HttpResponseMessage misresponse = client.SendAsync(httpRequest).Result;
                var result = misresponse.Content.ReadAsStringAsync().Result;
                doc = new HtmlDocument();
                doc.LoadHtml(result);
                // doc.DocumentNode.SelectNodes("//a")[35].Attributes["href"].Value;
                #endregion

                #region yearcheck
                if (DateTime.Now.Month < 3 && DateTime.Now.Month > -1)
                {
                    currentfin = (DateTime.Now.Year - 1) + "-" + DateTime.Now.Year;
                }
                else if (DateTime.Now.Month > 2 && DateTime.Now.Month < 12)
                {
                    currentfin = (DateTime.Now.Year) + "-" + (DateTime.Now.Year + 1);
                }
                #endregion

                if (currentfin == finyear)
                {
                    string __EVENTTARGET = "__EVENTTARGET=ctl00$ContentPlaceHolder1$ddl_States&";
                    string __EVENTARGUMENT = "__EVENTARGUMENT=&";
                    string __LASTFOCUS = "__LASTFOCUS=&";
                    __VIEWSTATE = "__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "")) + "&";
                    __VIEWSTATEGENERATOR = "__VIEWSTATEGENERATOR=" + doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "") + "&";
                    __EVENTVALIDATION = "__EVENTVALIDATION=" + Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "")) + "&";
                    txtCaptcha = "ctl00$ContentPlaceHolder1$txtCaptcha=&";
                    //hfCaptcha = "ctl00$ContentPlaceHolder1$hfCaptcha=" + doc.GetElementbyId("ctl00$ContentPlaceHolder1$hfCaptcha").GetAttributeValue("value", "") + "&";
                    string ddlfinyr = "ctl00$ContentPlaceHolder1$ddlfinyr=" + finyear + "&";
                    string ddl_States = "ctl00$ContentPlaceHolder1$ddl_States=15DKNY";
                    string misrespbodu = __EVENTTARGET + __EVENTARGUMENT + __LASTFOCUS + __VIEWSTATE + __VIEWSTATEGENERATOR + __EVENTVALIDATION + txtCaptcha + hfCaptcha + ddlfinyr + ddl_States;
                    HttpContent content1 = new StringContent(misrespbodu, Encoding.UTF8, "application/x-www-form-urlencoded");

                    HttpRequestMessage httpRequest1 = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = content1

                    };

                    HttpResponseMessage misre = client.SendAsync(httpRequest1).Result;
                    var misresult = misre.Content.ReadAsStringAsync().Result;
                    doc = new HtmlDocument();
                    doc.LoadHtml(misresult);
                    var requestdistlinks = doc.DocumentNode.SelectNodes("//a");
                    string requestdistlink = "";
                    for (int i = 200; i < requestdistlinks.Count; i++)
                    {
                        var item = requestdistlinks[i];
                        if (item.InnerText.Trim() == "Monitoring Report for Asset Id")
                        {
                            requestdistlink = item.Attributes["href"].Value; break;
                        }
                    }
                    HttpResponseMessage message = client.GetAsync(requestdistlink).Result;
                    var resmessage = message.Content.ReadAsStringAsync().Result;
                    doc = new HtmlDocument();
                    doc.LoadHtml(resmessage);


                }
                else
                {
                    string __EVENTTARGET = "__EVENTTARGET=ctl00$ContentPlaceHolder1$ddlfinyr&";
                    string __EVENTARGUMENT = "__EVENTARGUMENT=&";
                    string __LASTFOCUS = "__LASTFOCUS=&";
                    __VIEWSTATE = "__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "")) + "&";
                    __VIEWSTATEGENERATOR = "__VIEWSTATEGENERATOR=" + doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "") + "&";
                    __EVENTVALIDATION = "__EVENTVALIDATION=" + Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "")) + "&";
                    txtCaptcha = "ctl00$ContentPlaceHolder1$txtCaptcha=" + doc.GetElementbyId("ContentPlaceHolder1_hfCaptcha").GetAttributeValue("value", "") + "&";
                    hfCaptcha = "ctl00$ContentPlaceHolder1$hfCaptcha=" + doc.GetElementbyId("ContentPlaceHolder1_hfCaptcha").GetAttributeValue("value", "") + "&";
                    string ddlfinyr = "ctl00$ContentPlaceHolder1$ddlfinyr=" + finyear + "&";
                    string ddl_States = "ctl00$ContentPlaceHolder1$ddl_States=99";
                    string postbody = __EVENTTARGET + __EVENTARGUMENT + __LASTFOCUS + __VIEWSTATE + __VIEWSTATEGENERATOR + __EVENTVALIDATION + txtCaptcha + ddlfinyr + ddl_States;
                    HttpContent content1 = new StringContent(postbody, Encoding.UTF8, "application/x-www-form-urlencoded");

                    HttpRequestMessage httpRequest1 = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = content1
                    };

                    HttpResponseMessage misre = client.SendAsync(httpRequest1).Result;
                    var misresult = misre.Content.ReadAsStringAsync().Result;
                    doc = new HtmlDocument();
                    doc.LoadHtml(misresult);

                    __EVENTTARGET = "__EVENTTARGET=ctl00$ContentPlaceHolder1$ddl_States&";
                    __EVENTARGUMENT = "__EVENTARGUMENT=&";
                    __LASTFOCUS = "__LASTFOCUS=&";
                    __VIEWSTATE = "__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "")) + "&";
                    __VIEWSTATEGENERATOR = "__VIEWSTATEGENERATOR=" + doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "") + "&";
                    __EVENTVALIDATION = "__EVENTVALIDATION=" + Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "")) + "&";
                    txtCaptcha = "ctl00$ContentPlaceHolder1$txtCaptcha=&";
                    hfCaptcha = "ctl00$ContentPlaceHolder1$hfCaptcha=&";// + doc.GetElementbyId("ctl00$ContentPlaceHolder1$hfCaptcha").GetAttributeValue("value", "") + "&";
                    ddlfinyr = "ctl00$ContentPlaceHolder1$ddlfinyr=" + finyear + "&";
                    ddl_States = "ctl00$ContentPlaceHolder1$ddl_States=15DKNY";
                    string misrespbodu = __EVENTTARGET + __EVENTARGUMENT + __LASTFOCUS + __VIEWSTATE + __VIEWSTATEGENERATOR + __EVENTVALIDATION + txtCaptcha + hfCaptcha + ddlfinyr + ddl_States;
                    HttpContent stateresp = new StringContent(misrespbodu, Encoding.UTF8, "application/x-www-form-urlencoded");

                    HttpRequestMessage httpstatereq = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = stateresp
                    };

                    HttpResponseMessage misrep = client.SendAsync(httpstatereq).Result;
                    var misresu = misrep.Content.ReadAsStringAsync().Result;
                    doc = new HtmlDocument();
                    doc.LoadHtml(misresu);
                    var requestdistlinks = doc.DocumentNode.SelectNodes("//a");
                    string requestdistlink = "";
                    for (int i = 200; i < requestdistlinks.Count; i++)
                    {
                        var item = requestdistlinks[i];
                        if (item.InnerText.Trim() == "Monitoring Report for Asset Id")
                        {
                            requestdistlink = item.Attributes["href"].Value; break;
                        }
                    }

                    HttpResponseMessage message = client.GetAsync(requestdistlink).Result;
                    var resmessage = message.Content.ReadAsStringAsync().Result;
                    doc = new HtmlDocument();
                    doc.LoadHtml(resmessage);

                }

                var distlinks = doc.DocumentNode.SelectNodes("//a");
                string finaldistlink = "";
                foreach (var link in distlinks)
                {
                    var reqdistlink = "https://mnregaweb4.nic.in/netnrega/" + link.Attributes["href"].Value;
                    var parser = HttpUtility.ParseQueryString(reqdistlink);
                    if (parser != null)
                    {
                        if (parser.Get("district_code") == distcode)
                        {
                            finaldistlink = reqdistlink; break;
                        }
                    }
                }

                HttpResponseMessage distresp = client.GetAsync(finaldistlink).Result;
                var blockresp = distresp.Content.ReadAsStringAsync().Result;
                doc = new HtmlDocument();
                doc.LoadHtml(blockresp);
                var blinks = doc.DocumentNode.SelectNodes("//a");
                string finalblink = "";
                foreach (var blink in blinks)
                {
                    string requestblink = "https://mnregaweb4.nic.in/netnrega/" + blink.Attributes["href"].Value;
                    var parser = HttpUtility.ParseQueryString(requestblink);
                    if (parser != null)
                    {
                        if (parser.Get("block_code") == blockcode)
                        {
                            finalblink = requestblink; break;
                        }
                    }

                }

                HttpResponseMessage bresp = client.GetAsync(finalblink).Result;
                var plinks = bresp.Content.ReadAsStringAsync().Result;
                doc = new HtmlDocument();
                doc.LoadHtml(plinks);
                var plink = doc.DocumentNode.SelectNodes("//a");
                string finplink = "";
                foreach (var link in plink)
                {
                    string reqplink = "https://mnregaweb4.nic.in/netnrega/" + link.Attributes["href"].Value;
                    var parser = HttpUtility.ParseQueryString(reqplink);
                    if (parser != null)
                    {
                        if (parser.Get("panchayat_code") == pcode)
                        {
                            finplink = reqplink; break;
                        }
                    }
                }

                HttpResponseMessage finalpanresp = client.GetAsync(finplink).Result;
                var finalresp = finalpanresp.Content.ReadAsStringAsync().Result;

                Response.Write(finalresp);
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