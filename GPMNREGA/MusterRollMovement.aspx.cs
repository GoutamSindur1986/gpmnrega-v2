using Google.Type;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Net.Http;
using HtmlAgilityPack;
using System.Text;
using Grpc.Core;
using Google.Protobuf;
using System.Globalization;

namespace gpmnrega2.templates.KARNATAKA
{
    public partial class MusterRollMovement : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

            string workcode = Request.Params["workcode"].ToString();
            string distcode = Request.Params["distcode"].ToString();
            string blockcode = Request.Params["blockcode"].ToString();
            string panchcode = Request.Params["gpcode"].ToString();
            string data = "";
            Dictionary<int, string> responseData = new Dictionary<int, string>();
            JToken finYear = null;
            JToken NMRNo = null;
            JObject json = null;
            HttpClient client;
            using (StreamReader reader = new StreamReader(HttpContext.Current.Request.InputStream))
            {
                try
                {
                    data = reader.ReadToEnd();
                    json = JsonConvert.DeserializeObject<JObject>(data);
                    finYear = json.SelectToken("$.NMRYears");
                    NMRNo = json.SelectToken("$.NMRS");
                }
                catch (Exception ex)
                {
                    gpmnrega2.Utility.Utility.SendErrorToText(ex);
                }
            }

            Dictionary<string,string> years=new Dictionary<string, string>() { {"2025-2026", "rrqHsqSKol9m5Qs9mqXIvg" }, {"2024-2025", "YF+8sXKOdXUOi3psKaS+nA" },{ "2023-2024", "Y+NX28PGfSd8xSatAEQGbQ" },{ "2022-2023", "ygoW2kfBkDzt0x81BI3S6g"},{ "2021-2022", "ADBwMpShs9Ky/Z4IHpij0A" },{ "2020-2021", "XVHWcm0cCjKEJf2GE2kgqQ"},{ "2019-2020", "kiOJeLwfQzUj8T+aIHcqWg" } };

            foreach (JValue year in finYear)
            {
            StateResp:
                string url = "https://nregastrep.nic.in/netnrega/dynamic_muster_track.aspx?lflag=eng&state_code=15&fin_year=" + year.Value + "&state_name=KARNATAKA&Digest=" + years[year.Value.ToString()];

                client = new HttpClient();

                HttpResponseMessage responseMessage = client.GetAsync(url).Result;

                if (responseMessage.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    goto StateResp;
                }
                HtmlDocument doc = new HtmlDocument() { OptionUseIdAttribute = true };
                doc.LoadHtml(responseMessage.Content.ReadAsStringAsync().Result);


                string requestState = string.Format("__EVENTTARGET=ctl00%24ContentPlaceHolder1%24ddl_state&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__SCROLLPOSITIONX=0&__SCROLLPOSITIONY=0&__VIEWSTATEENCRYPTED=&__EVENTVALIDATION={2}&ctl00%24ContentPlaceHolder1%24ddl_state=15&ctl00%24ContentPlaceHolder1%24TextBox1=&ctl00%24ContentPlaceHolder1%24TextBox2=&ctl00%24ContentPlaceHolder1%24Rbtn_pay=0", Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "")),
                                      Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "")), Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "")));
            DistResp:
                HttpContent content = new StringContent(requestState, Encoding.UTF8, "application/x-www-form-urlencoded");

                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };

                HttpResponseMessage statePost = client.SendAsync(httpRequest).Result;
                if (statePost.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    goto DistResp;
                }
                doc = new HtmlDocument() { OptionUseIdAttribute = true };
                doc.LoadHtml(statePost.Content.ReadAsStringAsync().Result);



                string requestDist = string.Format("__EVENTTARGET=ctl00%24ContentPlaceHolder1%24ddl_dist&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__SCROLLPOSITIONX=0&__SCROLLPOSITIONY=0&__VIEWSTATEENCRYPTED=&__EVENTVALIDATION={2}&ctl00%24ContentPlaceHolder1%24ddl_state=15&ctl00%24ContentPlaceHolder1%24ddl_dist={3}&ctl00%24ContentPlaceHolder1%24TextBox1=&ctl00%24ContentPlaceHolder1%24TextBox2=&ctl00%24ContentPlaceHolder1%24Rbtn_pay=0", Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "")),
                                    Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "")), Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "")), distcode);
            BlockResp:
                content = new StringContent(requestDist, Encoding.UTF8, "application/x-www-form-urlencoded");

                httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };

                HttpResponseMessage distpost = client.SendAsync(httpRequest).Result;

                if (distpost.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    goto BlockResp;
                }

                doc = new HtmlDocument() { OptionUseIdAttribute = true };
                doc.LoadHtml(distpost.Content.ReadAsStringAsync().Result);

                string requestBlock = string.Format("__EVENTTARGET=ctl00%24ContentPlaceHolder1%24ddl_blk&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__SCROLLPOSITIONX=0&__SCROLLPOSITIONY=0&__VIEWSTATEENCRYPTED=&__EVENTVALIDATION={2}&ctl00%24ContentPlaceHolder1%24ddl_state=15&ctl00%24ContentPlaceHolder1%24ddl_dist={3}&ctl00%24ContentPlaceHolder1%24ddl_blk={4}&ctl00%24ContentPlaceHolder1%24TextBox1=&ctl00%24ContentPlaceHolder1%24TextBox2=&ctl00%24ContentPlaceHolder1%24Rbtn_pay=0", Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "")),
                                     Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "")), Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "")), distcode, blockcode);
            PanchayatResp:
                content = new StringContent(requestBlock, Encoding.UTF8, "application/x-www-form-urlencoded");

                httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };

                HttpResponseMessage blockpost = client.SendAsync(httpRequest).Result;
                if (blockpost.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    goto PanchayatResp;
                }

                doc = new HtmlDocument() { OptionUseIdAttribute = true };
                doc.LoadHtml(blockpost.Content.ReadAsStringAsync().Result);

                string requestPanch = string.Format("__EVENTTARGET=&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE={0}&__VIEWSTATEGENERATOR={1}&__SCROLLPOSITIONX=0&__SCROLLPOSITIONY=0&__VIEWSTATEENCRYPTED=&__EVENTVALIDATION={2}&ctl00%24ContentPlaceHolder1%24ddl_state=15&ctl00%24ContentPlaceHolder1%24ddl_dist={3}&ctl00%24ContentPlaceHolder1%24ddl_blk={4}&ctl00%24ContentPlaceHolder1%24ddl_pan={5}&ctl00%24ContentPlaceHolder1%24TextBox1=&ctl00%24ContentPlaceHolder1%24TextBox2=&ctl00%24ContentPlaceHolder1%24Rbtn_pay=0&ctl00%24ContentPlaceHolder1%24Button1=submit", Server.UrlEncode(doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "")),
                    Server.UrlEncode(doc.GetElementbyId("__VIEWSTATEGENERATOR").GetAttributeValue("value", "")), Server.UrlEncode(doc.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "")), distcode, blockcode, panchcode);
            ReportResp:
                content = new StringContent(requestPanch, Encoding.UTF8, "application/x-www-form-urlencoded");

                httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };

                HttpResponseMessage panchpost = client.SendAsync(httpRequest).Result;
                if (panchpost.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    goto ReportResp;
                }
                doc = new HtmlDocument() { OptionUseIdAttribute = true };
                doc.LoadHtml(panchpost.Content.ReadAsStringAsync().Result);

                HtmlNodeCollection trs = doc.DocumentNode.SelectNodes("//html[1]//body[1]//form[1]//div[1]//table[2]//tr");

                foreach (JObject nmr in NMRNo)
                {
                    for (int i = 2; i < trs.Count; i++)
                    {
                        if (trs[i].SelectNodes("td[3]")[0].InnerText.Trim() == ((JValue)((nmr)["NMRNo"])).Value.ToString() &&
                            trs[i].SelectNodes("td[6]")[0].InnerText.Trim() == workcode)
                        {
                            System.DateTime wageDate = System.DateTime.ParseExact(trs[i].SelectNodes("td[9]")[0].InnerText.Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture);

                            System.DateTime FtoDate1 = System.DateTime.ParseExact(trs[i].SelectNodes("td[13]")[0].InnerText.Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture);

                            System.DateTime FtoDate2 = System.DateTime.ParseExact(trs[i].SelectNodes("td[14]")[0].InnerText.Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture);

                            System.DateTime MustClDate = System.DateTime.ParseExact(trs[i].SelectNodes("td[7]")[0].InnerText.Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture);

                            string value = ((JValue)((nmr)["DateFrom"])).Value.ToString() + "," + MustClDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + "," +
                                           wageDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + "," + MustClDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + "," +
                                           wageDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + "," + FtoDate1.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + "," + FtoDate2.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                            if (!responseData.ContainsKey(int.Parse(((JValue)((nmr)["NMRNo"])).Value.ToString())))
                            {
                                responseData.Add(int.Parse(((JValue)((nmr)["NMRNo"])).Value.ToString()), value);
                            }
                            else
                                break;
                        }
                    }
                }

            }

            Response.Write(JsonConvert.SerializeObject(responseData));
            Response.End();

            }
            catch (Exception ex)
            {
                if (ex.Message != "Thread was being aborted.")
                {
                    Response.Clear();
                    Response.StatusCode = 5001;
                    Response.Write("Error occurred please try again.");
                }
            }
        }
    }
}