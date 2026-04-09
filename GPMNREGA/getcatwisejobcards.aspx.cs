using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HtmlAgilityPack;
using System.Threading;

namespace gpmnrega2.api
{
    public partial class getcatwisejobcards : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                string Panchayat_Code = Request.QueryString["Panchayat_Code"].ToString();
                string block_code = Request.QueryString["block_code"].ToString();
                string blockname = Request.QueryString["block_name"].ToString();
                string distname = Request.QueryString["dist_name"].ToString();
                string distcode = Request.QueryString["dist_code"].ToString();
                string panchname = Request.QueryString["panchayatname"].ToString();
                string finyear = ConfigurationManager.AppSettings["finyear"].ToString();

                var request = (HttpWebRequest)WebRequest.Create("https://nregastrep.nic.in/netnrega/IndexFrame.aspx?lflag=eng&District_Code=" + distcode + "&district_name=" + distname + "&state_name=KARNATAKA&state_Code=15&block_name=" + blockname + "&block_code=" + block_code + "&fin_year=" + finyear + "&check=1&Panchayat_name=" + panchname + "&Panchayat_Code=" + Panchayat_Code);
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36;";

                var Indexindex = (HttpWebResponse)request.GetResponse();
                string session = Indexindex.Headers.Get("Set-Cookie").Split('=')[1].Split(';')[0];
                HtmlDocument doc = new HtmlDocument();
                string response = new StreamReader(Indexindex.GetResponseStream()).ReadToEnd();
                doc.LoadHtml(response);

                var links = doc.DocumentNode.SelectNodes("//a");

                string catwiselink = "";
                string disabledpersons = "";
                string mustrolldetails = "";
                string scstemployement = "";
                foreach (var link in links)
                {
                    if (link.InnerText.Trim() == "Registration Caste Wise")
                    {
                        catwiselink = "https://nregastrep.nic.in/netnrega/" + link.Attributes["href"].Value; break;
                    }
                }
                var httpClient = (HttpWebRequest)WebRequest.Create("https://nregastrep.nic.in/netnrega/Progofficer/PoIndexFrame.aspx?flag_debited=S&lflag=eng&District_Code=" + distcode + "&district_name=" + distname + "&state_name=KARNATAKA&state_Code=15&finyear=" + finyear + "&check=1&block_name=" + blockname + "&Block_Code=" + block_code);
                var poresp = (HttpWebResponse)httpClient.GetResponse();
                string posession = poresp.Headers.Get("Set-Cookie").Split('=')[1].Split(';')[0];
                string resps = new StreamReader(poresp.GetResponseStream()).ReadToEnd();
                doc = new HtmlDocument();
                doc.LoadHtml(resps);
                var linkers = doc.DocumentNode.SelectNodes("//a");
                for (int i = 0; i < linkers.Count; i++)
                {
                    string link = linkers[i].Attributes["href"].Value.Replace("../", "https://nregastrep.nic.in/netnrega/");
                    if (link.Contains("emuster_wagelist_rpt.aspx?"))
                    {
                        mustrolldetails = link;
                    }
                    if (link.Contains("stdisabled.aspx?"))
                    {
                        disabledpersons = link;//"https://nregastrep.nic.in/netnrega/" + linkers[i].Attributes["href"].Value;
                    }
                    if (link.Contains("empstatusnewall_scst.aspx?"))
                    {
                        scstemployement = link;// "https://nregastrep.nic.in/netnrega/" + linkers[i].Attributes["href"].Value;
                    }
                    if (mustrolldetails != "" && disabledpersons != "" && scstemployement != "")
                        break;

                }


                var httpjobcardrequest = (HttpWebRequest)WebRequest.Create(catwiselink);
                httpjobcardrequest.Method = "GET";
                httpjobcardrequest.CookieContainer = new CookieContainer();
                httpjobcardrequest.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });

                string jobcards = new StreamReader(httpjobcardrequest.GetResponse().GetResponseStream()).ReadToEnd();
                doc = new HtmlDocument();
                doc.LoadHtml(jobcards);
                string node = doc.DocumentNode.SelectSingleNode("//table[@bgcolor='skyblue']/tr[last()]").OuterHtml;


                var mustertrollreq = (HttpWebRequest)WebRequest.Create(mustrolldetails);
                mustertrollreq.Method = "GET";
                mustertrollreq.CookieContainer = new CookieContainer();
                mustertrollreq.CookieContainer.Add(new Cookie("ASP.NET_SessionId", posession) { Domain = "nregastrep.nic.in" });

                string respmustrol = new StreamReader(mustertrollreq.GetResponse().GetResponseStream()).ReadToEnd();


                var disabledreq = (HttpWebRequest)WebRequest.Create(disabledpersons);
                disabledreq.Method = "GET";
                disabledreq.CookieContainer = new CookieContainer();
                disabledreq.CookieContainer.Add(new Cookie("ASP.NET_SessionId", posession) { Domain = "nregastrep.nic.in" });

                string disabledres = new StreamReader(disabledreq.GetResponse().GetResponseStream()).ReadToEnd();


                var scstreq = (HttpWebRequest)WebRequest.Create(scstemployement);
                scstreq.Method = "GET";
                scstreq.CookieContainer = new CookieContainer();
                scstreq.CookieContainer.Add(new Cookie("ASP.NET_SessionId", posession) { Domain = "nregastrep.nic.in" });

                string scstres = new StreamReader(scstreq.GetResponse().GetResponseStream()).ReadToEnd();


                Response.Write(node + "$$" + respmustrol + "$$" + disabledres + "$$" + scstres);
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