using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Http;
using Grpc.Core;
using Microsoft.Ajax.Utilities;
using HtmlAgilityPack;
using System.IO;
using System.Net;

namespace gpmnrega2.api
{
    public partial class jobCardHH : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                string district_code, block_code, panchayat_code, district_name, block_name, panchayat_name, fin_year, check, lflag;
                if (!IsPostBack)
                {
                    district_code = "District_Code=" + Request.QueryString["District_Code"] + "&";
                    block_name = "block_name=" + Request.QueryString["block_name"] + "&";
                    block_code = "block_code=" + Request.QueryString["block_code"] + "&";
                    panchayat_name = "Panchayat_name=test";
                    panchayat_code = "Panchayat_Code=" + Request.QueryString["Panchayat_Code"] + "&";
                    district_name = "district_name=" + Request.QueryString["district_name"] + "&";
                    fin_year = "fin_year=2024-2025&";
                    check = "check=1&";
                    lflag = "lflag=eng&";

                    string url = "https://nregastrep.nic.in/netnrega/homestciti.aspx?state_code=15&state_name=KARNATAKA&lflag=eng&labels=labels";

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.AllowAutoRedirect = true;
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";

                    HttpWebResponse issueResponse = (HttpWebResponse)request.GetResponse();
                    var agencydist = new StreamReader(issueResponse.GetResponseStream()).ReadToEnd();

                    request = (HttpWebRequest)WebRequest.Create("https://nregastrep.nic.in/netnrega/IndexFrame.aspx?" + lflag + check + block_code + district_code + block_name + district_name + panchayat_code + fin_year + "&state_name=KARNATAKA&state_Code=15&" + panchayat_name);
                    string session = issueResponse.Headers.Get("Set-Cookie").Split('=')[1].Split(';')[0];
                    request.CookieContainer = new CookieContainer();

                    request.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });
                    HttpWebResponse jobcardresp = (HttpWebResponse)request.GetResponse();

                    string jobcardlink = new StreamReader(jobcardresp.GetResponseStream()).ReadToEnd();

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(jobcardlink);

                    var alllinks = doc.DocumentNode.SelectNodes("//a");
                    string joblink = "";
                    for (int k = 0; k < alllinks.Count; k++)
                    {
                        if (alllinks[k].InnerText.Trim() == "Job card/Employment Register")
                        {
                            joblink = "https://nregastrep.nic.in/netnrega/" + alllinks[k].Attributes["href"].Value;
                            break;
                        }
                    }

                    request = (HttpWebRequest)WebRequest.Create(joblink);
                    request.CookieContainer = new CookieContainer();
                    request.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });
                    request.AllowAutoRedirect = true;

                    HttpWebResponse jobcardsresp = (HttpWebResponse)request.GetResponse();

                    string jobcardlinks = new StreamReader(jobcardsresp.GetResponseStream()).ReadToEnd();
                    Response.Write(jobcardlinks);
                    Response.End();

                }
            }
            catch (Exception ex)
            {
                if (ex.Message != "Thread was being aborted.")
                {
                    Response.ClearContent();
                    Response.StatusCode = 5001;
                    Response.Write("Error connecting NREGA DataBase.");
                   
                }
            }
        }
    }
}