using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace gpmnrega2.Registers
{
    public partial class register3paymentmap : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                Dictionary<string, string> map = new Dictionary<string, string>() { { "2024-2025", "5YHFAActhLUtixYtIsQBnA" }, { "2023-2024", "OfRfLUocdbxNwIcfPZuyPg" }, { "2022-2023", "7LVcX/pXCsHpVKoF+Bjpcg" }, { "2021-2022", "jZP4Yw2WjJsIxrZ74SLtKQ" }, { "2020-2021", "iu0fI8WnWph8tFN1VebTqg" } };

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
                var links = doc.DocumentNode.SelectNodes("//a");// [95].Attributes["href"].Value.Replace("../", "https://nregastrep.nic.in/netnrega/");
                string link = "";
                for(int i=70;i<links.Count;i++)
                {
                    link = links[i].Attributes["href"].Value.Replace("../", "https://nregastrep.nic.in/netnrega/");
                    if (link.Contains("citizen_html/funddisreport.aspx?"))
                        break;
                }
                HttpResponseMessage panchyatresp = client.GetAsync(link).Result;
                var panchresp = panchyatresp.Content.ReadAsStringAsync().Result;

                doc = new HtmlDocument();
                doc.LoadHtml(panchresp);
                var expenselinks = doc.DocumentNode.SelectNodes("//a");
                string finalexpenselink = "";
                foreach (var item in expenselinks)
                {
                    link = item.Attributes["href"].Value.Replace("../../", "https://nregastrep.nic.in/netnrega/");
                    var parser = HttpUtility.ParseQueryString(link);
                    if (link.ToString().Contains("mustworkexpn.aspx") && parser.Get("panchayat_code") == pcode)
                    {
                        finalexpenselink = link; break;
                    }
                }

                HttpResponseMessage finexp = client.GetAsync(finalexpenselink).Result;
                var finalresp = finexp.Content.ReadAsStringAsync().Result;
                Response.Write(finalresp);
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