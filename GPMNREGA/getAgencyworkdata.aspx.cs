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

namespace gpmnrega2.api
{
    public partial class getAgencyworkdata : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int iterations = 0;
            try
            {

                if (Request.QueryString["district_code"] != null)
                {
                    string url = "";
                    using (StreamReader sr = new StreamReader(HttpContext.Current.Request.InputStream))
                    {
                        url = sr.ReadToEnd();//.Replace("workcode", "wkcode");
                        try
                        {
                            Uri uri = new Uri(url);
                            //fin year
                            string finyear = HttpUtility.ParseQueryString(uri.Query).Get("finyear");
                            string nmrstartDate = HttpUtility.ParseQueryString(uri.Query).Get("dtfrm");
                            if (finyear != null)
                                if (finyear == "")
                                {
                                    int dtmonth = int.Parse(nmrstartDate.Split('/')[1]);
                                    int year = int.Parse(nmrstartDate.Split('/')[2]);

                                    if (dtmonth > 3)
                                    {
                                        finyear = (year).ToString() + "-" + (year + 1).ToString();
                                    }
                                    else
                                    {
                                        finyear = (year - 1).ToString() + "-" + (year).ToString();
                                    }

                                    url = url.Replace("finyear=", "finyear=" + finyear);
                                }
                            // url = url.Replace("work_code", "wkcode");
                        }
                        catch { }
                    }
                    //string state_code = Request.QueryString["state_code"].ToString();
                    string district_code = Request.QueryString["district_code"].ToString();
                    string block_code = Request.QueryString["block_code"].ToString();
                    string panchayat_code = Request.QueryString["panchayat_code"].ToString();
                    string work_code = Request.QueryString["work_code"].ToString();
                    //string fin_year= Request.QueryString["fin_year"].ToString();
                    string baseurl = "https://nregastrep.nic.in/netnrega/";
                    string url1 = "";//https://mnregaweb4.nic.in/netnrega/state_html/wrk_cat_freeze.aspx?page=S&short_name=KN&state_name=KARNATAKA&state_code=15&fin_year=2024-2025&source=national&Digest=kn7MXlpcOCKVqWZJL1zt7g";
                    string mainurl = "https://nregastrep.nic.in/netnrega/homestciti.aspx?state_code=15&state_name=KARNATAKA&lflag=eng&labels=labels";

                    
                    HttpClient clientmain = new HttpClient();

                    HttpResponseMessage resp = clientmain.GetAsync(mainurl).Result;
                    if((resp.StatusCode == HttpStatusCode.ServiceUnavailable || resp.Content.Headers.ContentLength<1024) && iterations<3)
                    {
                        mainurl= "https://mnregaweb4.nic.in/netnrega/state_html/wrk_cat_freeze.aspx?page=S&short_name=KN&state_name=KARNATAKA&state_code=15&fin_year=2024-2025&source=national&Digest=kn7MXlpcOCKVqWZJL1zt7g";
                        baseurl = "https://mnregaweb4.nic.in/netnrega/";
                        iterations++;
                        goto reapeat;
                    }
                    var mainresp = resp.Content.ReadAsStringAsync().Result;
                    HtmlDocument maindoc = new HtmlDocument();
                    maindoc.LoadHtml(mainresp);

                    var a = maindoc.DocumentNode.SelectNodes("//a");
                   
                    for (int m = 300; m < a.Count; m++)
                    {
                        mainurl = baseurl + a[m].Attributes["href"].Value;
                        if (a[m].InnerText == "DPR Frozen Status")
                            break;
                    }
                reapeat:
                    CookieContainer cookies = new CookieContainer();
                    HttpClientHandler handler = new HttpClientHandler();
                    handler.CookieContainer = cookies;
                    HttpClient client = new HttpClient(handler);
                    
                    
                    HttpResponseMessage message = client.GetAsync(mainurl).Result; //"https://mnregaweb4.nic.in/netnrega/state_html/DPCApprove_Workdetail.aspx?state_code="+state_code+ "&district_code="+district_code+ "&block_code="+block_code+ "&panchayat_code="+panchayat_code+ "&work_code="+work_code+ "&fin_year="+fin_year+ "&Digest=s3kn").Result;
                    var res = message.Content.ReadAsStringAsync().Result;
                    if (res.Length < 1500 && iterations<2)
                    {
                        mainurl = "https://mnregaweb4.nic.in/netnrega/state_html/wrk_cat_freeze.aspx?page=S&short_name=KN&state_name=KARNATAKA&state_code=15&fin_year=2024-2025&source=national&Digest=kn7MXlpcOCKVqWZJL1zt7g"; //"https://mnregaweb4.nic.in/netnrega/";
                        baseurl = "https://mnregaweb4.nic.in/netnrega/";
                        iterations++;
                        goto reapeat;
                    }
                    //cookies=new Cookie()
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(res);

                    var distlinks = doc.DocumentNode.SelectNodes("//a");
                    string distfinaldprlink = "";
                    for (int i = 0; i < distlinks.Count; i++)
                    {
                        string distdprlink = distlinks[i].Attributes["href"].Value;
                        var parameters = HttpUtility.ParseQueryString(new Uri(baseurl + distdprlink).Query);
                        if (parameters.Get("district_code") != null)
                        {
                            if (parameters.Get("district_code") == district_code)
                            {
                                distfinaldprlink = baseurl + "state_html/" + distdprlink;
                                break;
                            }
                        }
                    }

                    //////////////////
                    string c = ((string[])message.Headers.GetValues("Set-Cookie"))[0].Split(';')[0];
                  
                    cookies.Add(new Uri(baseurl),new Cookie(c.Split('=')[0], c.Split('=')[1]));
           
                    HttpResponseMessage dprdistresponse = client.GetAsync(distfinaldprlink).Result;
                    var dprdistres = dprdistresponse.Content.ReadAsStringAsync().Result;

                    if (dprdistres.Length < 1500 && iterations < 2)
                    {
                        mainurl = "https://mnregaweb4.nic.in/netnrega/state_html/wrk_cat_freeze.aspx?page=S&short_name=KN&state_name=KARNATAKA&state_code=15&fin_year=2024-2025&source=national&Digest=kn7MXlpcOCKVqWZJL1zt7g"; //"https://mnregaweb4.nic.in/netnrega/";
                        baseurl = "https://mnregaweb4.nic.in/netnrega/";
                        iterations++;
                        goto reapeat;
                    }

                    doc = new HtmlDocument();
                    doc.LoadHtml(dprdistres);

                    var blocklinks = doc.DocumentNode.SelectNodes("//a");
                    string blockfinaldprlink = "";
                    for (int i = 0; i < blocklinks.Count; i++)
                    {
                        string blkdprlink = blocklinks[i].Attributes["href"].Value;
                        var parameters = HttpUtility.ParseQueryString(new Uri(baseurl + blkdprlink).Query);
                        if (parameters.Get("block_code") != null)
                        {
                            if (parameters.Get("block_code") == block_code)
                            {
                                blockfinaldprlink = baseurl + "state_html/" + blkdprlink;
                                break;
                            }
                        }
                    }

                    ///////////////////////


                    HttpResponseMessage dprblockresponse = client.GetAsync(blockfinaldprlink).Result;
                    var dprblockres = dprblockresponse.Content.ReadAsStringAsync().Result;

                    if (dprblockres.Length < 1500 && iterations < 2)
                    {
                        mainurl = "https://mnregaweb4.nic.in/netnrega/state_html/wrk_cat_freeze.aspx?page=S&short_name=KN&state_name=KARNATAKA&state_code=15&fin_year=2024-2025&source=national&Digest=kn7MXlpcOCKVqWZJL1zt7g"; //"https://mnregaweb4.nic.in/netnrega/";
                        baseurl = "https://mnregaweb4.nic.in/netnrega/";
                        iterations++;
                        goto reapeat;
                    }

                    doc = new HtmlDocument();
                    doc.LoadHtml(dprblockres);

                    var gplinks = doc.DocumentNode.SelectNodes("//a");
                    string gpfinaldprlink = "";
                    for (int i = 0; i < gplinks.Count; i++)
                    {
                        string gpdprlink = gplinks[i].Attributes["href"].Value;
                        var parameters = HttpUtility.ParseQueryString(new Uri(baseurl + gpdprlink).Query);
                        if (parameters.Get("panchayat_code") != null)
                        {
                            if (parameters.Get("panchayat_code") == panchayat_code)
                            {
                                gpfinaldprlink = baseurl + "state_html/" + gpdprlink;
                                break;
                            }
                        }
                    }

                    ////////////////////////////
                    ///

                    HttpResponseMessage dprgpresponse = client.GetAsync(gpfinaldprlink).Result;
                    var dprgpres = dprgpresponse.Content.ReadAsStringAsync().Result;

                    if (dprgpres.Length < 1500 && iterations < 2)
                    {
                        mainurl = "https://mnregaweb4.nic.in/netnrega/state_html/wrk_cat_freeze.aspx?page=S&short_name=KN&state_name=KARNATAKA&state_code=15&fin_year=2024-2025&source=national&Digest=kn7MXlpcOCKVqWZJL1zt7g"; //"https://mnregaweb4.nic.in/netnrega/";
                        baseurl = "https://mnregaweb4.nic.in/netnrega/";
                        iterations++;
                        goto reapeat;
                    }
                    doc = new HtmlDocument();
                    doc.LoadHtml(dprgpres);

                    var wklinks = doc.DocumentNode.SelectNodes("//a");
                    string wkfinaldprlink = "";
                    for (int i = 0; i < wklinks.Count; i++)
                    {
                        string wkdprlink = wklinks[i].Attributes["href"].Value;
                        var parameters = HttpUtility.ParseQueryString(new Uri(baseurl + wkdprlink).Query);
                        if (parameters.Get("work_code") != null)
                        {
                            if (parameters.Get("work_code").ToUpper() == work_code.ToUpper())
                            {
                                wkfinaldprlink = baseurl + "state_html/" + wkdprlink;
                                break;
                            }
                        }
                    }
                    if (wkfinaldprlink == "" && iterations<2)
                    {

                        mainurl = "https://mnregaweb4.nic.in/netnrega/state_html/wrk_cat_freeze.aspx?page=S&short_name=KN&state_name=KARNATAKA&state_code=15&fin_year=2024-2025&source=national&Digest=kn7MXlpcOCKVqWZJL1zt7g"; //"https://mnregaweb4.nic.in/netnrega/";
                        baseurl = "https://mnregaweb4.nic.in/netnrega/";
                        iterations++;
                        goto reapeat;
                    }

                    HttpResponseMessage dprworkresponse = client.GetAsync(wkfinaldprlink).Result;
                    string finalresponse = dprworkresponse.Content.ReadAsStringAsync().Result; 
                    if ((iterations < 2) &&( finalresponse.IndexOf("URL TEMPERED")>-1 || dprworkresponse.StatusCode==HttpStatusCode.ServiceUnavailable || dprworkresponse.RequestMessage.RequestUri.AbsoluteUri== "https://nregastrep.nic.in/netnrega/error.html?aspxerrorpath=/netnrega/state_html/DPCApprove_Workdetail.aspx"))
                    {
                        mainurl = "https://mnregaweb4.nic.in/netnrega/state_html/wrk_cat_freeze.aspx?page=S&short_name=KN&state_name=KARNATAKA&state_code=15&fin_year=2024-2025&source=national&Digest=kn7MXlpcOCKVqWZJL1zt7g";
                        baseurl = "https://mnregaweb4.nic.in/netnrega/";
                        iterations++;
                        goto reapeat;
                    }

                   

                    Response.Write(finalresponse);
                    Response.End();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message != "Thread was being aborted.")
                {
                    Response.ClearContent(); Response.StatusCode = 5001;
                    Response.StatusDescription = "Error connecting NREGA DataBase.";
                }
            }

        }


    }
}