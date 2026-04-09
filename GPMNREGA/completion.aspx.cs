using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace gpnmrega.templates.Kannada
{
    public partial class completion : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.Params["workcode"] != null)
            {
                txtPanchayat3.InnerText = txtPanchayat1.InnerText = Request.Params["panchayat_NameRegional"];
                txtBlock.InnerText = Request.Params["blockNameRegional"];
                txtDist.InnerText = Request.Params["districtNameRegional"];
                txtTSancationNo.InnerText = Request.Params["techSanctionNo"];
                txtWorkCode.InnerText = Request.Params["workcode"];
                txtWorkName.InnerText = txtWorkName1.InnerText = Request.Params["workName"];
                txtWorkOrdeNoDate.InnerText = Request.Params["techSanctionNo"].Contains("/TS") ? Request.Params["techSanctionNo"]
                    .Substring(0, Request.Params["techSanctionNo"].Length - 3) : Request.Params["techSanctionNo"];
                txtWorkOrdeNoDate.InnerText += " & " + Request.Params["techSanctionDate"];
                txtUnskilled.InnerText = Request.Params["UskilledExp"];
                txtTotal.InnerText = Request.Params["workCostTotal"];
                txtMat.InnerText = Request.Params["MaterialCost"];
                karimag.Src = "~/Content/karemblem.jpg";

            }
        }
    }
}