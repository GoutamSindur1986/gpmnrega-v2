using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using HtmlAgilityPack;
using System.Net.Http;
using System.IO;
using System.Web.Http.Results;
using static Google.Rpc.Context.AttributeContext.Types;

namespace gpmnrega2.Registers
{
    public partial class WageCashWork : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                string dist_code, block_code, pcode, dist_name, block_name, ppname, finyear;
                dist_code = Request.QueryString["dist_code"];
                block_code = Request.QueryString["block_code"];
                pcode = Request.QueryString["panchayat_code"];
                dist_name = Request.QueryString["district_name"];
                block_name = Request.QueryString["block_name"];
                ppname = Request.QueryString["panchayat_name"];
                finyear = Request.QueryString["fin_year"];

                string url = "https://nregastrep.nic.in/netnrega/Progofficer/PoIndexFrame.aspx?flag_debited=S&lflag=eng&District_Code=" + dist_code + "&district_name=" + dist_name + "&state_name=KARNATAKA&state_Code=15&finyear=" + finyear + "&check=1&block_name=" + block_name + "&Block_Code=" + block_code;

                WebRequest webreq = (HttpWebRequest)WebRequest.Create(url);

                HttpWebResponse issueResponse = (HttpWebResponse)webreq.GetResponse();
                string blockcontent = new StreamReader(issueResponse.GetResponseStream()).ReadToEnd();
                string session = issueResponse.Headers.Get("Set-Cookie").Split('=')[1].Split(';')[0];
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(blockcontent);
                string emusterlink = "";
                var blinks = doc.DocumentNode.SelectNodes("//a");
                for (int i = 50; i < blinks.Count; i++)
                {
                    emusterlink = blinks[i].Attributes["href"].Value.Replace("../", "https://nregastrep.nic.in/netnrega/");
                    var parser = HttpUtility.ParseQueryString(emusterlink);
                    if (parser != null & emusterlink.Contains("/emuster_wagelist_rpt.aspx?"))
                        break;
                }

                var emust = (HttpWebRequest)WebRequest.Create(emusterlink);
                emust.CookieContainer = new CookieContainer();
                emust.Method = "GET";
                emust.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });
                string webresp = new StreamReader(((HttpWebResponse)emust.GetResponse()).GetResponseStream()).ReadToEnd();

                doc = new HtmlDocument();
                doc.LoadHtml(webresp);
                HtmlNodeCollection musterlink = new HtmlNodeCollection(doc.DocumentNode);
                if (Request.QueryString["type"] == "9")
                    musterlink = doc.DocumentNode.SelectNodes("//table[1]//tr//td[3]//a");
                else
                    musterlink = doc.DocumentNode.SelectNodes("//table[1]//tr//td[12]//a");

                string requestMustlink = "";

                foreach (var item in musterlink)
                {
                    requestMustlink = "https://nregastrep.nic.in/netnrega/state_html/" + item.Attributes["href"].Value;
                    var parser = HttpUtility.ParseQueryString(requestMustlink);
                    if (parser != null && parser.Get("panchayat_code") != null)
                    {
                        if (parser.Get("panchayat_code") == pcode)
                            break;
                    }
                }

                var emuster = (HttpWebRequest)WebRequest.Create(requestMustlink);
                emuster.CookieContainer = new CookieContainer();
                emuster.Method = "GET";
                emuster.Timeout = 50000;
                emuster.CookieContainer.Add(new Cookie("ASP.NET_SessionId", session) { Domain = "nregastrep.nic.in" });
                string emusterresp = new StreamReader(((HttpWebResponse)emuster.GetResponse()).GetResponseStream()).ReadToEnd();

                Response.Write(emusterresp);
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
    }
}