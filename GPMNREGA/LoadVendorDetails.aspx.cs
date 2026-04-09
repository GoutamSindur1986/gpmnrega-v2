using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using HtmlAgilityPack;
using System.IO;
using System.Web.UI.HtmlControls;
using Newtonsoft.Json;
using System.Text;

namespace gpmnrega2.api
{
    public partial class LoadVendorDetails : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["dist_code"] != null)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://nregastrep.nic.in/netnrega/state_html/freez_vendor_rpt.aspx?lflag=eng&state_code=15&state_name=KARNATAKAeng&fin_year=2024-2025&page=s&typ=R&Digest=qIpIZ2htWNsX87pDDYwL6Q");
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";

                HttpWebResponse message = (HttpWebResponse)request.GetResponse();
                if (message != null)
                {
                    var statevendorresp = new StreamReader(message.GetResponseStream()).ReadToEnd();
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(statevendorresp);

                    var distlinks = doc.DocumentNode.SelectNodes("//a");
                    string distdata = "";

                    foreach (var distlink in distlinks)
                    {
                        distdata = "https://nregastrep.nic.in/netnrega/state_html/" + distlink.Attributes["href"].Value;
                        var queryparam = HttpUtility.ParseQueryString(distdata);
                        if (queryparam != null)
                        {
                            if (queryparam.Get("district_code") == Request.QueryString["dist_code"])
                                break;
                        }
                    }
                    HttpWebRequest requestdist = (HttpWebRequest)WebRequest.Create(distdata);
                    requestdist.Method = "GET";
                    requestdist.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";

                    HttpWebResponse distmessage = (HttpWebResponse)requestdist.GetResponse();
                    var distresp = new StreamReader(distmessage.GetResponseStream()).ReadToEnd();
                    doc.LoadHtml(distresp);
                    string jsonresp = "[";
                    var alltrs = doc.DocumentNode.SelectNodes("//table[1]//tr");

                    for(int i = 2; i < alltrs.Count; i++)
                    {
                        jsonresp+="{\"value\":\"" + alltrs[i].ChildNodes[3].InnerText.Trim() + "\",\"label\":\"" + alltrs[i].ChildNodes[5].InnerText.Trim() + "\"},";
                    }
                    jsonresp= jsonresp.Remove(jsonresp.LastIndexOf(','), 1);
                    jsonresp += "]";
                 
                    Response.Write(jsonresp);
                    Response.End();
                }
            }
        }
    }
}