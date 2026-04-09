using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Http;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Globalization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Drawing;


namespace gpmnrega2.templates
{
    public partial class geotag : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string workcode = txtCode.InnerText = Request.Params["workcode"].ToString();
            txtdist.InnerText = Request.Params["ds"].ToString();
            txtblock.InnerText = Request.Params["bn"].ToString();
            txtPanch.InnerText = Request.Params["pn"].ToString();
            txtfin.InnerText = Request.Params["fin"].ToString();
            txtWn.InnerText = Request.Params["WorkName"].ToString();


            string bhuvanurl = "https://bhuvan-app2.nrsc.gov.in/mgnrega/nrega_dashboard_phase2/php/reports/accepted_geotags.php";
            HttpClient client = new HttpClient();

            for (int i = 1; i <= 3; i++)
            {
                string postdata = "username=unauthourized&sub_category_id=All&state_code=15&start_date=2019-03-31&" +
                             "stage=" + i + "&panchayat_code=" + Request.Params["panchayat_code"].ToString() + "&financial_year=All&end_date=" + DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "&district_code=" + Request.Params["district_code"].ToString() + "&category_id=All&" +
                             "block_code=" + Request.Params["block_code"].ToString() + "&accuracy=0";
                StringContent postData = new StringContent(postdata, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpRequestMessage reqmessage = new HttpRequestMessage(HttpMethod.Post, bhuvanurl) { Content = postData };
                HttpResponseMessage message = client.SendAsync(reqmessage).Result;
                JArray workarray = new JArray();
                try
                {
                    workarray = JsonConvert.DeserializeObject<JArray>(message.Content.ReadAsStringAsync().Result);
                    foreach (JObject item in workarray)
                    {
                        if (item.GetValue("workcode").ToString().Trim().ToLower() == Request.Params["workcode"].ToString().Trim().ToLower())
                        {

                            txtDate.InnerText = DateTime.Parse(item.GetValue("creationtime").ToString().Split(' ')[0]).ToString("dd/MM/yyyy",CultureInfo.InvariantCulture);
                            txtlat.InnerText = item.GetValue("lat").ToString();
                            txtlon.InnerText = item.GetValue("lon").ToString();


                            if (1 == i)
                            {
                                stage11.Src = "data:image/jpeg;base64," + fetchImageByte(item.GetValue("path1").ToString());
                                stage12.Src = "data:image/jpeg;base64," + fetchImageByte(item.GetValue("path2").ToString());
                                tblStage1.Visible = true;
                            }
                            if (2 == i)
                            {
                                stage21.Src = "data:image/jpeg;base64," + fetchImageByte(item.GetValue("path1").ToString());
                                stage22.Src = "data:image/jpeg;base64," + fetchImageByte(item.GetValue("path2").ToString());
                                tblStage2.Visible = true;
                            }
                            if (3 == i)
                            {
                                stage31.Src = "data:image/jpeg;base64," + fetchImageByte(item.GetValue("path1").ToString());
                                stage32.Src = "data:image/jpeg;base64," + fetchImageByte(item.GetValue("path2").ToString());
                                tblStage3.Visible = true;
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    gpmnrega2.Utility.Utility.SendErrorToText(ex);
                }

            }
            #region oldcode
            //if (slnofound)
            //{
            //    try
            //    {

            //        string asseturl = "https://bhuvan-app2.nrsc.gov.in/mgnrega/usrtasks/nrega_phase2/get/get_details.php?sno=" + slno;
            //        client = new HttpClient();
            //        HttpResponseMessage resp = client.GetAsync(asseturl).Result;
            //        response = resp.Content.ReadAsStringAsync().Result;
            //        HtmlDocument document = new HtmlDocument();
            //        document.LoadHtml(response);

            //        HtmlNodeCollection imgs = document.DocumentNode.SelectNodes("//a");
            //        HtmlNodeCollection trs = document.DocumentNode.SelectNodes("//tr");

            //        for (int i = 0; i < trs.Count; i++)
            //        {
            //            if (trs[i].InnerText.Contains("Longitude/Latitude"))
            //            {

            //                txtlon.InnerText = trs[i].InnerText.Replace("Longitude/Latitude", "").Replace("&nbsp;Analyse", "").Split('/')[0];
            //                txtlat.InnerText = trs[i].InnerText.Replace("Longitude/Latitude", "").Replace("&nbsp;Analyse", "").Split('/')[1];
            //                break;
            //            }
            //        }

            //        for (int i = 0; i < trs.Count; i++)
            //        {
            //            if (trs[i].InnerText.Contains("Creation Time"))
            //            {

            //                txtDate.InnerText = DateTime.Parse(trs[i].InnerText.Replace("Creation Time", "").Split(' ')[0]).ToString("d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            //                break;
            //            }
            //        }
            //        for (int i = 0; i < trs.Count; i++)
            //        {
            //            if (trs[i].InnerText.Contains("Work Name"))
            //            {

            //                txtWn.InnerText = trs[i].InnerText.Replace("Work Name", "");
            //                break;
            //            }
            //        }
            //        for (int i = 0; i < trs.Count; i++)
            //        {
            //            if (trs[i].InnerText.Contains("Work Financial Year"))
            //            {

            //                txtfin.InnerText = trs[i].InnerText.Replace("Work Financial Year", "");
            //                break;
            //            }
            //        }


            //        for (int i = 0; i < (imgs.Count - 1) / 2; i++)
            //        {
            //            int j, k;

            //            j = i == 0 ? 0 : i == 1 ? 2 : 4;
            //            k = i == 0 ? 1 : i == 1 ? 3 : 5;
            //            MemoryStream ms = new MemoryStream();
            //            string link = imgs[j].GetAttributeValue("onclick", "").Split('"')[1];
            //            client.GetAsync(link).Result.Content.ReadAsStreamAsync().Result.CopyTo(ms);
            //            byte[] data = new byte[ms.Length];
            //            data = ms.ToArray();
            //            if (i == 0)
            //            {
            //                stage11.Src = "data:image/jpeg;base64," + Convert.ToBase64String(data);
            //                tblStage1.Visible = true;
            //            }
            //            if (i == 1)
            //            {
            //                stage21.Src = "data:image/jpeg;base64," + Convert.ToBase64String(data);
            //                tblStage2.Visible = true;
            //            }
            //            if (i == 2)
            //            {
            //                stage31.Src = "data:image/jpeg;base64," + Convert.ToBase64String(data);
            //                tblStage3.Visible = true;
            //            }
            //            ms = new MemoryStream();
            //            link = imgs[k].GetAttributeValue("onclick", "").Split('"')[1];
            //            client.GetAsync(link).Result.Content.ReadAsStreamAsync().Result.CopyTo(ms);

            //            data = new byte[ms.Length];
            //            data = ms.ToArray();
            //            if (i == 0)
            //            {
            //                stage12.Src = "data:image/jpeg;base64," + Convert.ToBase64String(data);
            //            }
            //            if (i == 1)
            //            {
            //                stage22.Src = "data:image/jpeg;base64," + Convert.ToBase64String(data);
            //            }
            //            if (i == 2)
            //            {
            //                stage32.Src = "data:image/jpeg;base64," + Convert.ToBase64String(data);
            //            }


            //        }

            //    }
            //    catch (Exception ex)
            //    {

            //        throw;
            //    }

            //}

            #endregion

        }

        public static string fetchImageByte(string path)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                HttpClient client = new HttpClient();
                client.GetAsync(path).Result.Content.ReadAsStreamAsync().Result.CopyTo(ms);
                byte[] data = new byte[ms.Length];
                data = ms.ToArray();
                return Convert.ToBase64String(data);

            }
        }
    }
}