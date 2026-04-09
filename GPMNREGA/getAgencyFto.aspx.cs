using Google.Api.Gax.Grpc;
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
using static Google.Cloud.Translate.V3.BatchTranslateDocumentMetadata.Types;

namespace gpmnrega2.api
{
    public partial class getAgencyFto : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                string searchurl = "https://mnregaweb4.nic.in/netnrega/nregasearch1.aspx";
                string fin_year = Request.QueryString["srch"];
                string ftoNo = Request.QueryString["ftoNo"];

                var request = (HttpWebRequest)HttpWebRequest.Create(searchurl);
                request.AllowAutoRedirect = true;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";

                string getsearchresp = "";
                try
                {
                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        getsearchresp = streamReader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    gpmnrega2.Utility.Utility.SendErrorToText(ex);
                }
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(getsearchresp);

                string __ASYNCPOST = "__ASYNCPOST=true&";
                string __EVENTARGUMENT = "__EVENTARGUMENT=&";
                string __EVENTTARGET = "__EVENTTARGET=ddl_search&";
                string __EVENTVALIDATION = "__EVENTVALIDATION=" + Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "")) + "&";
                string __LASTFOCUS = "__LASTFOCUS=&";
                string __VIEWSTATE = "__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "")) + "&";
                string __VIEWSTATEENCRYPTED = "__VIEWSTATEENCRYPTED=&";
                string __VIEWSTATEGENERATOR = "__VIEWSTATEGENERATOR=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "")) + "&";
                string as_fid = "as_fid=" + Server.UrlEncode(doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='as_fid']").Attributes["value"].Value) + "&";
                string as_sfid = "as_sfid=" + Server.UrlEncode(doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='as_sfid']").Attributes["value"].Value) + "&";
                string ddl_search = "ddl_search=FTO&";
                string ddl_state = "ddl_state=Select&";
                string districtname = "districtname=&";
                string hhshortname = "hhshortname=&";
                string ScriptManager1 = "ScriptManager1=UpdatePanel1|ddl_search&";
                string searchname = "searchname=&";
                string statename = "statename=&";
                string txt_keyword2 = "txt_keyword2=&";

                string postdata1 = __ASYNCPOST + __EVENTARGUMENT + __EVENTTARGET + __EVENTVALIDATION + __LASTFOCUS + __VIEWSTATE + __VIEWSTATEENCRYPTED +
                                   __VIEWSTATEGENERATOR + as_fid + as_sfid + ddl_search + ddl_state + districtname + hhshortname + ScriptManager1 + searchname +
                                   statename + txt_keyword2;

                var workrequest = (HttpWebRequest)HttpWebRequest.Create(searchurl);
                workrequest.Method = "POST";
                workrequest.AllowAutoRedirect = true;
                workrequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";

                var data = Encoding.UTF8.GetBytes(postdata1);
                workrequest.ContentType = "application/x-www-form-urlencoded";
                workrequest.ContentLength = data.Length;
                using (var sr = workrequest.GetRequestStream())
                {
                    sr.Write(data, 0, data.Length);
                }
                var workResponse = (HttpWebResponse)workrequest.GetResponse();

                var workresponseString = new StreamReader(workResponse.GetResponseStream()).ReadToEnd().Split('|');

                #region state
                __ASYNCPOST = "__ASYNCPOST=true&";
                __EVENTARGUMENT = "__EVENTARGUMENT=&";
                __EVENTTARGET = "__EVENTTARGET=ddl_state&";
                __EVENTVALIDATION = "__EVENTVALIDATION=" + Server.UrlEncode(workresponseString[Array.IndexOf(workresponseString, "__EVENTVALIDATION") + 1]) + "&";
                __LASTFOCUS = "__LASTFOCUS=&";
                __VIEWSTATE = "__VIEWSTATE=" + Server.UrlEncode(workresponseString[Array.IndexOf(workresponseString, "__VIEWSTATE") + 1]) + "&";
                __VIEWSTATEENCRYPTED = "__VIEWSTATEENCRYPTED=&";
                __VIEWSTATEGENERATOR = "__VIEWSTATEGENERATOR=" + Server.UrlEncode(workresponseString[Array.IndexOf(workresponseString, "__VIEWSTATEGENERATOR") + 1]) + "&";
                as_fid = "as_fid=" + Server.UrlEncode(doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='as_fid']").Attributes["value"].Value) + "&";
                as_sfid = "as_sfid=" + Server.UrlEncode(doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='as_sfid']").Attributes["value"].Value) + "&";
                ddl_search = "ddl_search=FTO&";
                ddl_state = "ddl_state=15&";
                districtname = "districtname=&";
                hhshortname = "hhshortname=&";
                ScriptManager1 = "ScriptManager1=UpdatePanel1|ddl_state&";
                searchname = "searchname=&";
                statename = "statename=&";
                txt_keyword2 = "txt_keyword2=&";

                string postdata2 = __ASYNCPOST + __EVENTARGUMENT + __EVENTTARGET + __EVENTVALIDATION + __LASTFOCUS + __VIEWSTATE + __VIEWSTATEENCRYPTED +
                                   __VIEWSTATEGENERATOR + as_fid + as_sfid + ddl_search + ddl_state + districtname + hhshortname + ScriptManager1 + searchname +
                                   statename + txt_keyword2;

                var staterequest = (HttpWebRequest)HttpWebRequest.Create(searchurl);
                staterequest.Method = "POST";
                staterequest.AllowAutoRedirect = true;
                staterequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";

                var datastate = Encoding.UTF8.GetBytes(postdata2);
                staterequest.ContentType = "application/x-www-form-urlencoded";
                staterequest.ContentLength = datastate.Length;
                using (var sr = staterequest.GetRequestStream())
                {
                    sr.Write(datastate, 0, datastate.Length);
                }
                var stateResponse = (HttpWebResponse)staterequest.GetResponse();

                var stateresponseString = new StreamReader(stateResponse.GetResponseStream()).ReadToEnd().Split('|');

                #endregion

                #region dist

                __ASYNCPOST = "__ASYNCPOST=true&";
                __EVENTARGUMENT = "__EVENTARGUMENT=&";
                __EVENTTARGET = "__EVENTTARGET=ddl_district&";
                __EVENTVALIDATION = "__EVENTVALIDATION=" + Server.UrlEncode(stateresponseString[Array.IndexOf(stateresponseString, "__EVENTVALIDATION") + 1]) + "&";
                __LASTFOCUS = "__LASTFOCUS=&";
                __VIEWSTATE = "__VIEWSTATE=" + Server.UrlEncode(stateresponseString[Array.IndexOf(stateresponseString, "__VIEWSTATE") + 1]) + "&";
                __VIEWSTATEENCRYPTED = "__VIEWSTATEENCRYPTED=&";
                __VIEWSTATEGENERATOR = "__VIEWSTATEGENERATOR=" + Server.UrlEncode(stateresponseString[Array.IndexOf(stateresponseString, "__VIEWSTATEGENERATOR") + 1]) + "&";
                as_fid = "as_fid=" + Server.UrlEncode(doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='as_fid']").Attributes["value"].Value) + "&";
                as_sfid = "as_sfid=" + Server.UrlEncode(doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='as_sfid']").Attributes["value"].Value) + "&";
                ddl_search = "ddl_search=FTO&";
                ddl_state = "ddl_state=15&";
                districtname = "districtname=&";
                hhshortname = "hhshortname=&";
                ScriptManager1 = "ScriptManager1=UpdatePanel1|ddl_district&";
                searchname = "searchname=&";
                statename = "statename=&";
                txt_keyword2 = "txt_keyword2=&";
                string ddl_district = "ddl_district=" + Request.QueryString["district_code"].ToString() + "&";


                string postdata3 = __ASYNCPOST + __EVENTARGUMENT + __EVENTTARGET + __EVENTVALIDATION + __LASTFOCUS + __VIEWSTATE + __VIEWSTATEENCRYPTED +
                                  __VIEWSTATEGENERATOR + as_fid + as_sfid + ddl_search + ddl_state + districtname + hhshortname + ScriptManager1 + searchname +
                                  statename + txt_keyword2 + ddl_district;

                var distrequest = (HttpWebRequest)HttpWebRequest.Create(searchurl);
                distrequest.Method = "POST";
                distrequest.AllowAutoRedirect = true;
                distrequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";

                var datadist = Encoding.UTF8.GetBytes(postdata3);
                distrequest.ContentType = "application/x-www-form-urlencoded";
                distrequest.ContentLength = datadist.Length;
                using (var sr = distrequest.GetRequestStream())
                {
                    sr.Write(datadist, 0, datadist.Length);
                }
                var distResponse = (HttpWebResponse)distrequest.GetResponse();

                var distresponseString = new StreamReader(distResponse.GetResponseStream()).ReadToEnd().Split('|');

                #endregion

                #region searchkey


                __EVENTARGUMENT = "__EVENTARGUMENT=&";
                __EVENTTARGET = "__EVENTTARGET=&";
                __EVENTVALIDATION = "__EVENTVALIDATION=" + Server.UrlEncode(distresponseString[Array.IndexOf(distresponseString, "__EVENTVALIDATION") + 1]) + "&";
                __LASTFOCUS = "__LASTFOCUS=&";
                __VIEWSTATE = "__VIEWSTATE=" + Server.UrlEncode(distresponseString[Array.IndexOf(distresponseString, "__VIEWSTATE") + 1]) + "&";
                __VIEWSTATEENCRYPTED = "__VIEWSTATEENCRYPTED=&";
                __VIEWSTATEGENERATOR = "__VIEWSTATEGENERATOR=" + Server.UrlEncode(distresponseString[Array.IndexOf(distresponseString, "__VIEWSTATEGENERATOR") + 1]) + "&";
                as_fid = "as_fid=" + Server.UrlEncode(doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='as_fid']").Attributes["value"].Value) + "&";
                as_sfid = "as_sfid=" + Server.UrlEncode(doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='as_sfid']").Attributes["value"].Value) + "&";
                ddl_search = "ddl_search=FTO&";
                ddl_state = "ddl_state=15&";
                districtname = "districtname=" + Request.QueryString["district_name"].ToString() + "&";
                hhshortname = "hhshortname=KN&";
                ScriptManager1 = "ScriptManager1=UpdatePanel1|ddl_district&";
                searchname = "searchname=FTO&";
                statename = "statename=KARNATAKA&";
                txt_keyword2 = "txt_keyword2=" + ftoNo + "&";
                string btngo = "btn_go=GO&";

                string postdata4 = __EVENTARGUMENT + __EVENTTARGET + __EVENTVALIDATION + __LASTFOCUS + __VIEWSTATE + __VIEWSTATEENCRYPTED +
                                  __VIEWSTATEGENERATOR + as_fid + as_sfid + ddl_search + ddl_state + districtname + hhshortname + searchname +
                                  statename + txt_keyword2 + ddl_district + btngo;

                var textrequest = (HttpWebRequest)HttpWebRequest.Create(searchurl);
                textrequest.Method = "POST";
                textrequest.AllowAutoRedirect = true;
                textrequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";

                var datatext = Encoding.UTF8.GetBytes(postdata4);
                textrequest.ContentType = "application/x-www-form-urlencoded";
                textrequest.ContentLength = datatext.Length;
                using (var sr = textrequest.GetRequestStream())
                {
                    sr.Write(datatext, 0, datatext.Length);
                }
                var textResponse = (HttpWebResponse)textrequest.GetResponse();

                var textresponseString = new StreamReader(textResponse.GetResponseStream()).ReadToEnd();

                #endregion

                doc = new HtmlDocument();
                doc.LoadHtml(textresponseString);
                int startindex = doc.ParsedText.IndexOf("&Digest=");
                int endindex = doc.ParsedText.IndexOf("', 'popup_window',");

                int length = endindex - startindex;
                string digest = doc.ParsedText.Substring(startindex, length);

                string wgurl = "https://mnregaweb4.nic.in/netnrega/master_search1.aspx?flag=2&wsrch=fto&district_code=" + Request.QueryString["district_code"].ToString() + "&state_name=KARNATAKA&district_name=" + Request.QueryString["district_name"].ToString() + "&short_name=KN&srch=" + ftoNo + digest;

                HttpClient client = new HttpClient();
                HttpResponseMessage wg1 = client.GetAsync(wgurl).Result;

                string wg1resp = wg1.Content.ReadAsStringAsync().Result;

                doc.LoadHtml(wg1resp);
                var wglink = doc.DocumentNode.SelectNodes("//a");
                HttpResponseMessage message = new HttpResponseMessage();
                string wglinkresp = "";
                if (wglink.Count > 3)
                {
                    wglinkresp = "https://mnregaweb4.nic.in/netnrega/" + wglink[2].Attributes["href"].Value;
                    message = client.GetAsync(wglinkresp).Result;

                }
                else
                {
                    string __EVENTTARGET1 = "__EVENTTARGET=ddl_yr&";
                    string __EVENTARGUMENT1 = "__EVENTARGUMENT=&";
                    string __LASTFOCUS1 = "__LASTFOCUS=&";
                    string __VIEWSTATE1 = "__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "")) + "&";
                    string __VIEWSTATEGENERATOR1 = "__VIEWSTATEGENERATOR=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "")) + "&";
                    string __VIEWSTATEENCRYPTED1 = "__VIEWSTATEENCRYPTED=&";
                    string __EVENTVALIDATION1 = "__EVENTVALIDATION=" + Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "")) + "&";
                    string ddl_yr1 = "ddl_yr=" + fin_year + "&";

                    string postdata5 = __EVENTARGUMENT1 + ddl_yr1 + __VIEWSTATE1 + __VIEWSTATEGENERATOR1 + __VIEWSTATEENCRYPTED1 + __EVENTTARGET1 + __LASTFOCUS1 + __EVENTVALIDATION1;

                    var wagerequest = (HttpWebRequest)HttpWebRequest.Create(wgurl);
                    wagerequest.Method = "POST";
                    wagerequest.AllowAutoRedirect = true;
                    wagerequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";

                    var wgtext = Encoding.UTF8.GetBytes(postdata5);
                    wagerequest.ContentType = "application/x-www-form-urlencoded";
                    wagerequest.ContentLength = wgtext.Length;
                    using (var sr = wagerequest.GetRequestStream())
                    {
                        sr.Write(wgtext, 0, wgtext.Length);
                    }
                    var wgResponse = (HttpWebResponse)wagerequest.GetResponse();

                    var wgresponseString = new StreamReader(wgResponse.GetResponseStream()).ReadToEnd();

                    doc.LoadHtml(wgresponseString);

                    var wglink1 = doc.DocumentNode.SelectNodes("//a");
                    if (wglink1.Count > 3)
                    {
                        wglinkresp = "https://mnregaweb4.nic.in/netnrega/" + wglink1[2].Attributes["href"].Value;
                        message = client.GetAsync(wglinkresp).Result;
                    }


                }

                if (message != null)
                {
                    if (message.StatusCode == HttpStatusCode.OK)
                    {
                        doc = new HtmlDocument();
                        doc.LoadHtml(message.Content.ReadAsStringAsync().Result);
                        HttpWebRequest ftodetailsrequest = (HttpWebRequest)WebRequest.Create(wglinkresp);

                        __EVENTTARGET = "__EVENTTARGET=ctl00$ContentPlaceHolder1$Ddfto&";
                        __EVENTARGUMENT = "__EVENTARGUMENT=&";
                        __LASTFOCUS = "__LASTFOCUS=&";
                        __VIEWSTATE = "__VIEWSTATE=" + Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "")) + "&";
                        __VIEWSTATEGENERATOR = "__VIEWSTATEGENERATOR=" + doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "") + "&";
                        __VIEWSTATEENCRYPTED = "__VIEWSTATEENCRYPTED=&";
                        __EVENTVALIDATION = "__EVENTVALIDATION=" + Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "")) + "&";
                        string Ddfin = "ctl00$ContentPlaceHolder1$Ddfin=" + fin_year + "&";
                        string Ddstate = "ctl00$ContentPlaceHolder1$Ddstate=15&";
                        string Txtfto = "ctl00$ContentPlaceHolder1$Txtfto=" + ftoNo + "&";
                        string Ddfto = "ctl00$ContentPlaceHolder1$Ddfto=" + ftoNo;

                        string ftopost = __EVENTTARGET + __EVENTARGUMENT + __LASTFOCUS + __VIEWSTATE + __VIEWSTATEGENERATOR + __VIEWSTATEENCRYPTED
                                       + __EVENTVALIDATION + Ddfin + Ddstate + Txtfto + Ddfto;
                        ftodetailsrequest.Method = "POST";

                        ftodetailsrequest.AllowAutoRedirect = true;
                        ftodetailsrequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";

                        var ftopostdata = Encoding.UTF8.GetBytes(ftopost);
                        ftodetailsrequest.ContentType = "application/x-www-form-urlencoded";
                        ftodetailsrequest.ContentLength = ftopostdata.Length;
                        using (var sr = ftodetailsrequest.GetRequestStream())
                        {
                            sr.Write(ftopostdata, 0, ftopostdata.Length);
                        }
                        var finalftodata = (HttpWebResponse)ftodetailsrequest.GetResponse();

                        var ftohtml = new StreamReader(finalftodata.GetResponseStream()).ReadToEnd();
                        Response.Write(ftohtml);
                        Response.End();

                    }
                }

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