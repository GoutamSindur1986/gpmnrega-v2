using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using EvoPdf;
using Newtonsoft.Json.Linq;

namespace gpnmrega.templates.Kannada
{
    public partial class CoverPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (Request.Params["workcode"] != null)
                {
                    if (!string.IsNullOrEmpty(Request.Params["workcode"].ToString()))
                    {
                        JObject json = (JObject)Session[Request.Params["workcode"].ToString().Split(',')[0]];
                        txtPanchayat.InnerText  = txtPanchyat3.InnerText = Request.Params["panchayat_NameRegional"].ToString().Split(',')[0];
                        txtBlock.InnerText = txtBlock1.InnerText = Request.Params["blockNameRegional"].ToString().Split(',')[0];
                        txtDist.InnerText = txtdist1.InnerText = Request.Params["districtNameRegional"].ToString().Split(',')[0];
                        txtState.InnerText = txtState1.InnerText = Request.Params["stateNameRegional"].ToString().Split(',')[0];
                        txtsdate.InnerText = Request.Params["startdate"].ToString().Split(',')[0];
                        txtWorkCode.InnerText = Request.Params["workcode"].ToString().Split(',')[0];
                        txtWorkName.InnerText = Request.Params["workName"].ToString().Split(',')[0];
                        txtWorkYear.InnerText = Request.Params["workYear"].ToString().Split(',')[0];
                        txttechno.InnerText = Request.Params["techSanctionNo"].ToString().Split(',')[0];
                        txtTotal.InnerText = txtTotal1.InnerText = Request.Params["workCostTotal"].ToString().Split(',')[0];
                        txtunskill.InnerText = Request.Params["UskilledExp"].ToString().Split(',')[0];
                        txtMaterial.InnerText = Request.Params["MaterialCost"].ToString().Split(',')[0];
                        txtExagency.InnerText = Request.Params["executionAgency"].ToString().Split(',')[0];
                        txtLA.InnerText = Request.Params["VidhanSabha"].ToString().Split(',')[0];
                        txtLS.InnerText = Request.Params["LokSabha"].ToString().Split(',')[0];
                        txtcat.InnerText = Request.Params["workCategory"].ToString().Split(',')[0];
                        txtLA.InnerText = Request.Params["VidhanSabha"].ToString().Split(',')[0];
                        txtLS.InnerText = Request.Params["LokSabha"].ToString().Split(',')[0];
                    }
                }
            }
            catch (Exception ex)
            {

                if (ex.Message != "Thread was being aborted.")
                {
                    Response.Clear();
                    Response.StatusCode = 5001;
                    Response.Write("Error occurred please try again.");
                }

            }
        }
    }
}