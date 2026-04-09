using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HtmlAgilityPack;

namespace gpmnrega2.Registers
{
    public partial class Register3Workmap : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                string url = "";
                string distcode, blockcode, pcode, statecode, statename, distname, blockname, finyear, pname;
                distcode = Request.QueryString["dist_code"];
                pcode = Request.QueryString["panchayat_code"];
                statecode = Request.QueryString["state_Code"];
                blockcode = Request.QueryString["block_code"];
                statename = Request.QueryString["state_name"];
                distname = Request.QueryString["dist_name"];
                blockname = Request.QueryString["block_name"];
                finyear = Request.QueryString["fin_year"];
                pname = Request.QueryString["pname"];

                HttpClient client = new HttpClient();
                HttpResponseMessage message = client.GetAsync("https://nregastrep.nic.in/netnrega/Progofficer/PoIndexFrame.aspx?flag_debited=S&lflag=eng&District_Code=" + distcode + "&district_name=" + distname + "&state_name=KARNATAKA&state_Code=15&finyear=" + finyear + "&check=1&block_name=" + blockname + "&Block_Code=" + blockcode).Result;
                var res = message.Content.ReadAsStringAsync().Result;

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(res);
                var links = doc.DocumentNode.SelectNodes("//a");// [70].Attributes["href"].Value.Replace("../", "https://nregastrep.nic.in/netnrega/");
                string link = "";
                for(int i=50; i<links.Count; i++)
                {
                    link = links[i].Attributes["href"].Value.Replace("../", "https://nregastrep.nic.in/netnrega/");
                    if (link.Contains("emuster_wagelist_rpt.aspx?"))
                        break;


                }
                HttpResponseMessage panchyatresp = client.GetAsync(link).Result;
                var panchresp = panchyatresp.Content.ReadAsStringAsync().Result;

                doc = new HtmlDocument();
                doc.LoadHtml(panchresp);

                var panchlinks = doc.DocumentNode.SelectNodes("//center[1]//table[1]//tr//td[3]//a");
                string finalpachlink = "";
                foreach (var panchlink in panchlinks)
                {
                    var plink = "https://nregastrep.nic.in/netnrega/state_html/" + panchlink.Attributes["href"].Value;
                    var parser = HttpUtility.ParseQueryString(plink);
                    if (parser != null)
                    {
                        if (parser.Get("panchayat_code") == pcode)
                        {
                            finalpachlink = plink; break;

                        }
                    }
                }
                HttpResponseMessage finalworklink = client.GetAsync(finalpachlink).Result;
                var finalres = finalworklink.Content.ReadAsStringAsync().Result;
                Response.Write(finalres);
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