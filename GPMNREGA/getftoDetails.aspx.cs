using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using HtmlAgilityPack;
using System.Net.Http;

namespace gpmnrega2.api
{
    public partial class getftoDetails : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                Dictionary<string, string> map = new Dictionary<string, string>() { { "2025-2026", "VIYEYkV6KCjmigpCauTElQ" }, { "2024-2025", "G5nkV/MnRcIFaFkhI3Hsyw" }, { "2023-2024", "0Q3J/VJe0jM6Dsi8JF5ueA" }, { "2022-2023", "QCziIGEXM4BBB2VukVqkOQ" }, { "2021-2022", "3eCaVeN5tPmW91mkdhTBjg" }, { "2020-2021", "0bInl8ptge+QQGaJcC+Wow" }, { "2019-2020", "6yAWOrQeWWvv5uerhRvImA" } };
                string[] ftono = Request.QueryString["fto_no"].Split('_');
                string finYear = "";
                if(int.Parse(ftono[1].Substring(2,2))>=1 &&  int.Parse(ftono[1].Substring(2, 2)) <= 3){

                    finYear = "20" + (int.Parse(ftono[1].Substring(4, 2)) - 1).ToString() + "-20" + (int.Parse(ftono[1].Substring(4, 2))).ToString();

                }
                else
                {
                    finYear = "20" + (int.Parse(ftono[1].Substring(4, 2))).ToString() + "-20" + (int.Parse(ftono[1].Substring(4, 2))+1).ToString();
                }

                string ftourl = "https://mnregaweb4.nic.in/netnrega/FTO/FTOReport.aspx?page=s&mode=B&flg=W&state_name=KARNATAKA&state_code=15&fin_year=" + finYear + "&dstyp=B&source=national&Digest=" + map[finYear];
                string baseurl = "https://mnregaweb4.nic.in/netnrega/FTO/";
                HttpClient client = new HttpClient();
                string ftostate = client.GetAsync(ftourl).Result.Content.ReadAsStringAsync().Result;

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(ftostate);

                var distlinks = doc.DocumentNode.SelectNodes("//table[2]//td[2]//a");
                string distftolink = "";
                foreach (var link in distlinks)
                {
                    var param = HttpUtility.ParseQueryString(new Uri(baseurl + link.Attributes["href"].Value).Query);
                    if (param.Get("district_code") != null)
                    {
                        if (param.Get("district_code") == Request.QueryString["district_code"])
                        {
                            distftolink = baseurl + link.Attributes["href"].Value;
                            break;
                        }
                    }
                }

                string ftodist = client.GetAsync(distftolink).Result.Content.ReadAsStringAsync().Result;

                doc = new HtmlDocument();
                doc.LoadHtml(ftodist);

                var blinks = doc.DocumentNode.SelectNodes("//table[2]//td[2]//a");
                string bftolink = "";
                foreach (var link in blinks)
                {
                    var param = HttpUtility.ParseQueryString(new Uri(baseurl + link.Attributes["href"].Value).Query);
                    if (param.Get("block_code") != null)
                    {
                        if (param.Get("block_code") == Request.QueryString["block_code"])
                        {
                            bftolink = baseurl + link.Attributes["href"].Value;
                            break;
                        }
                    }
                }

                string ftoblock = client.GetAsync(bftolink).Result.Content.ReadAsStringAsync().Result;

                doc = new HtmlDocument();
                doc.LoadHtml(ftoblock);

                var gplinks = doc.DocumentNode.SelectNodes("//table[2]//td[4]//a");
                string gpftolink = "";
                foreach (var link in gplinks)
                {
                    var param = HttpUtility.ParseQueryString(new Uri(baseurl + link.Attributes["href"].Value).Query);
                    if (param.Get("panchayat_code") != null)
                    {
                        if (param.Get("panchayat_code") == Request.QueryString["panchayat_code"])
                        {
                            gpftolink = baseurl + link.Attributes["href"].Value;
                            break;
                        }
                    }
                }

                string fto = client.GetAsync(gpftolink).Result.Content.ReadAsStringAsync().Result;

                doc = new HtmlDocument();
                doc.LoadHtml(fto);

                var fto1 = doc.DocumentNode.SelectNodes("//a");
                string ftolink = "";
                foreach (var link in fto1)
                {
                    var param = HttpUtility.ParseQueryString(new Uri(baseurl + link.Attributes["href"].Value).Query);
                    if (param.Get("fto_no") != null)
                    {
                        if (param.Get("fto_no") == Request.QueryString["fto_no"])
                        {
                            ftolink = baseurl + link.Attributes["href"].Value;
                            break;
                        }
                    }
                }

                string ftoresp = client.GetAsync(ftolink).Result.Content.ReadAsStringAsync().Result;
                Response.Write(ftoresp);
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