using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace gpmnrega2.api
{
    public partial class getNmrData : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {


                string url = "";
                using (StreamReader sr = new StreamReader(HttpContext.Current.Request.InputStream))
                {
                    url = sr.ReadToEnd();//.Replace("workcode", "wkcode");
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

                }
                HttpClient client = new HttpClient();
                HttpResponseMessage message = client.GetAsync(url).Result;
                var res = message.Content.ReadAsStringAsync().Result;
                Response.Write(res);
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