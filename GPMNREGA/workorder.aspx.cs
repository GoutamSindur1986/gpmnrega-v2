using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;

namespace gpnmrega.templates.Kannada
{
    public partial class workorder : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.Params["workcode"] != null)
            {
                txtPachayat.InnerText = txtPachayat3.InnerText = Request.Params["panchayat_NameRegional"].ToString().Split(',')[0].Trim();
                txtSanctionNo.InnerText = Request.Params["techSanctionNo"].ToString().Split(',')[0].Trim().Contains("/TS") ? Request.Params["techSanctionNo"].ToString().
                    Substring(0, Request.Params["techSanctionNo"].ToString().Split(',')[0].Trim().Length - 3) : Request.Params["techSanctionNo"].ToString().Split(',')[0].Trim();
                txtWorkCode.InnerText = Request.Params["workcode"].ToString().Split(',')[0].Trim();
                txtWorkName.InnerText = Request.Params["workName"].ToString().Split(',')[0].Trim();
                txtDate.InnerText = DateTime.ParseExact(Request.Params["techSanctionDate"].ToString().Split(',')[0].Trim(), "d/M/yyyy", CultureInfo.InvariantCulture).AddDays(2).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                txtBlock.InnerText = txtBlock1.InnerText = Request.Params["blockNameRegional"].ToString();
                txtExpense.InnerText = Request.Params["workCostTotal"].ToString().Split(',')[0].Trim();
                txtYear.InnerText = Request.Params["workYear"].ToString().Split(',')[0].Trim();

            }
        }
    }
}