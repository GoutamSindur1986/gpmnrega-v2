using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using HtmlAgilityPack;
using System.Collections;
using Newtonsoft.Json;

namespace gpmnrega2.api
{
    public partial class getworkdata : System.Web.UI.Page
    {
        //dpr report only don't modify code untill error in dpr
        protected void Page_Load(object sender, EventArgs e)
        {
            string url1 = "";
            int iterations = 0;
            try
            {

                if (Request.QueryString["district_code"] != null)
                {

                    string district_code = Request.QueryString["district_code"].ToString();
                    string block_code = Request.QueryString["block_code"].ToString();
                    string panchayat_code = Request.QueryString["panchayat_code"].ToString();
                    //string work_code = Request.QueryString["work_code"].ToString();
                    //string fin_year= Request.QueryString["fin_year"].ToString();
                    string baseurl = "https://nregastrep.nic.in/netnrega/";
                    string mainurl = "https://nregastrep.nic.in/netnrega/homestciti.aspx?state_code=15&state_name=KARNATAKA&lflag=eng&labels=labels";
                    HttpClient clientmain=new HttpClient();

                    
                    HttpResponseMessage resp = clientmain.GetAsync(mainurl).Result;
                    var mainresp = resp.Content.ReadAsStringAsync().Result;
                    HtmlDocument maindoc=new HtmlDocument();
                    maindoc.LoadHtml(mainresp);
                    if(iterations < 2 &&(mainresp.IndexOf("URL TEMPERED")>-1 || resp.StatusCode==HttpStatusCode.ServiceUnavailable))
                    {
                        mainurl = "https://mnregaweb4.nic.in/netnrega/state_html/wrk_cat_freeze.aspx?page=S&short_name=KN&state_name=KARNATAKA&state_code=15&fin_year=2022-2023&source=national&Digest=7LVcX/pXCsHpVKoF+Bjpcg";
                        baseurl = "https://mnregaweb4.nic.in/netnrega/";
                        goto RepeatTask;
                    }
                    var a = maindoc.DocumentNode.SelectNodes("//a");
                    
                    for (int m=300;m<a.Count;m++)
                    {
                        url1 = baseurl+ a[m].Attributes["href"].Value;
                        if (a[m].InnerText == "DPR Frozen Status")
                        {
                            mainurl = url1;
                            break;
                        }
                    }

                //;


                RepeatTask:
                    
                    CookieContainer cookies = new CookieContainer();
                    HttpClientHandler handler = new HttpClientHandler();
                    handler.CookieContainer = cookies;
                    HttpClient client = new HttpClient(handler);                  
                    HttpResponseMessage message = client.GetAsync(mainurl).Result; //"https://mnregaweb4.nic.in/netnrega/state_html/DPCApprove_Workdetail.aspx?state_code="+state_code+ "&district_code="+district_code+ "&block_code="+block_code+ "&panchayat_code="+panchayat_code+ "&work_code="+work_code+ "&fin_year="+fin_year+ "&Digest=s3kn").Result;
                    var res = message.Content.ReadAsStringAsync().Result;
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

                    HttpResponseMessage dprdistresponse = client.GetAsync(distfinaldprlink).Result;
                    var dprdistres = dprdistresponse.Content.ReadAsStringAsync().Result;
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
                    doc = new HtmlDocument();
                    doc.LoadHtml(dprgpres);

                    HtmlNodeCollection wklinks = doc.DocumentNode.SelectNodes("/html[1]/body[1]/form[1]/div[1]/center[1]/table[1]/tr");
                    
                    Dictionary<string, string[]> workJson = new Dictionary<string, string[]>();
                    for (int i = 0; i < wklinks.Count; i++)
                    {
                        HtmlDocument workdoc = new HtmlDocument();
                        workdoc.LoadHtml(wklinks[i].InnerHtml);
                        var wkdprtd = workdoc.DocumentNode.SelectNodes("//td");
                        var worklinks = workdoc.DocumentNode.SelectNodes("//a");
                        if (worklinks != null)
                        {
                            string worklink = baseurl+"state_html/" + workdoc.DocumentNode.SelectNodes("//a")[0].Attributes["href"].Value;

                            var parameters = HttpUtility.ParseQueryString(new Uri(baseurl + worklink).Query);

                            if (parameters.Get("work_code") != null )
                            {
                                try
                                {

                                    int i1 = wkdprtd[1].InnerText.IndexOf("(") + 1;
                                    int i2 = wkdprtd[1].InnerText.LastIndexOf(")") - 1;
                                    string wkname = parameters.Get("work_name");
                                    string wkstatus = wkdprtd[2].InnerText.Trim();
                                    
                                        string[] workdetails = new string[4];
                                        workdetails[0] = wkname;
                                        workdetails[1] = wkstatus;
                                        workdetails[2] = worklink;
                                        workdetails[3] = parameters.Get("work_code").ToUpper();
                                        workJson.Add(workdetails[3], workdetails);
                                   

                                }
                                catch (Exception)
                                {

                                    //throw;
                                }
                            }
                        }

                    }
                   


                    string workfinaljson = JsonConvert.SerializeObject(workJson);
                    Response.Write(workfinaljson);
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