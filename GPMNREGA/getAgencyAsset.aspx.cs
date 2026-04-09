using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

namespace gpmnrega2.api
{
    public partial class getAgencyAsset : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                string searchurl = "https://mnregaweb4.nic.in/netnrega/nregasearch1.aspx";

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
                string ddl_search = "ddl_search=Work&";
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

                string workresponseString = new StreamReader(workResponse.GetResponseStream()).ReadToEnd();

                string[] ids1 = workresponseString.Split('|');

                doc = new HtmlDocument();
                doc.LoadHtml(workresponseString);

                #region state
                __ASYNCPOST = "__ASYNCPOST=true&";
                __EVENTARGUMENT = "__EVENTARGUMENT=&";
                __EVENTTARGET = "__EVENTTARGET=ddl_state&";
                __EVENTVALIDATION = "__EVENTVALIDATION=" + Server.UrlEncode(ids1[Array.IndexOf(ids1, "__EVENTVALIDATION") + 1]) + "&";
                __LASTFOCUS = "__LASTFOCUS=&";
                __VIEWSTATE = "__VIEWSTATE=" + Server.UrlEncode(ids1[Array.IndexOf(ids1, "__VIEWSTATE") + 1]) + "&";
                __VIEWSTATEENCRYPTED = "__VIEWSTATEENCRYPTED=&";
                __VIEWSTATEGENERATOR = "__VIEWSTATEGENERATOR=" + Server.UrlEncode(ids1[Array.IndexOf(ids1, "__VIEWSTATEGENERATOR") + 1]) + "&";
                //as_fid = "as_fid=" + Server.UrlEncode(doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='as_fid']").Attributes["value"].Value) + "&";
                //as_sfid = "as_sfid=" + Server.UrlEncode(doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='as_sfid']").Attributes["value"].Value) + "&";
                ddl_search = "ddl_search=Work&";
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

                string[] stateresponseString = new StreamReader(stateResponse.GetResponseStream()).ReadToEnd().Split('|');

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
                //as_fid = "as_fid=" + Server.UrlEncode(doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='as_fid']").Attributes["value"].Value) + "&";
                //as_sfid = "as_sfid=" + Server.UrlEncode(doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='as_sfid']").Attributes["value"].Value) + "&";
                ddl_search = "ddl_search=Work&";
                ddl_state = "ddl_state=15&";
                districtname = "districtname=&";
                hhshortname = "hhshortname=&";
                ScriptManager1 = "ScriptManager1=UpdatePanel1|ddl_district&";
                searchname = "searchname=&";
                statename = "statename=&";
                txt_keyword2 = "txt_keyword2=&";
                string ddl_district = "ddl_district=" + Request.QueryString["District_code"].ToString() + "&";


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
                //as_fid = "as_fid=" + Server.UrlEncode(doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='as_fid']").Attributes["value"].Value) + "&";
                //as_sfid = "as_sfid=" + Server.UrlEncode(doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='as_sfid']").Attributes["value"].Value) + "&";
                ddl_search = "ddl_search=Work&";
                ddl_state = "ddl_state=15&";
                districtname = "districtname=" + Request.QueryString["district_name"].ToString() + "&";
                hhshortname = "hhshortname=KN&";
                ScriptManager1 = "ScriptManager1=UpdatePanel1|ddl_district&";
                searchname = "searchname=Work&";
                statename = "statename=KARNATAKA&";
                txt_keyword2 = "txt_keyword2=" + Request.QueryString["wkcode"].ToString() + "&";
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

                string mainurl = "https://mnregaweb4.nic.in/netnrega/master_search1.aspx?flag=2&wsrch=wrk&district_code=" + Request.QueryString["District_code"] + "&state_name=KARNATAKA&district_name=" + Request.QueryString["district_name"] + "&short_name=KN&srch=" + Request.QueryString["wkcode"] + doc.ParsedText.Substring(startindex, length);

                var finalrequest = (HttpWebRequest)HttpWebRequest.Create(mainurl);
                finalrequest.Method = "GET";
                finalrequest.AllowAutoRedirect = true;
                finalrequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";

                var finalResponse = (HttpWebResponse)finalrequest.GetResponse();

                var finalresponseString = new StreamReader(finalResponse.GetResponseStream()).ReadToEnd();

                doc = new HtmlDocument();
                doc.LoadHtml(finalresponseString);
                var link = doc.DocumentNode.SelectNodes("//a")[2];

                string finalurl = "https://mnregaweb4.nic.in/netnrega/" + link.Attributes["href"].Value;

                var result = (HttpWebRequest)HttpWebRequest.Create(finalurl);
                result.Method = "GET";
                result.AllowAutoRedirect = true;
                result.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";

                var resultResponse = (HttpWebResponse)result.GetResponse();

                var resultfinalresponseString = new StreamReader(resultResponse.GetResponseStream()).ReadToEnd();


                Response.Write(finalresponseString);
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