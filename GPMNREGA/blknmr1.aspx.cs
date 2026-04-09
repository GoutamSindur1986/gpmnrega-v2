using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Web;
using System.Globalization;
using System.Text.RegularExpressions;

namespace gpmnrega2.templates.KARNATAKA
{
    public partial class blknmr1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                string data = "";

                JArray json = new JArray();
                JArray accdata = new JArray();
                JArray jcdata1 = new JArray();
                JObject otherdata = new JObject();
                try
                {

                }
                catch (Exception)
                {

                    throw;
                }
                using (StreamReader reader = new StreamReader(HttpContext.Current.Request.InputStream))
                {
                    try
                    {
                        data = reader.ReadToEnd();
                        json = JsonConvert.DeserializeObject<JArray>(data);
                        accdata = JsonConvert.DeserializeObject<JArray>(json[0].ToString());
                       // jcdata1 = JsonConvert.DeserializeObject<JArray>(json[0].ToString());
                        otherdata = JsonConvert.DeserializeObject<JObject>(json[1].ToString());
                        //json.Property("blockNameRegional").Value.ToString();
                    }
                    catch (Exception ex)
                    {
                        gpmnrega2.Utility.Utility.SendErrorToText(ex);
                    }
                }
                string dayth = @"<font size='1'><b>&nbsp;{0}&nbsp;</b></font>";
                string catth = @"<font size='1'><b>Category </b></font>";
                string tdgn = "<font size='1'>{0}</font>";
                string jbcard = "<font size='1'>{0}</font>";
                string bank = "Bank<br>{0}<br>{1}";
                string daytd = "&nbsp;";
                string lastRow1td = @"<font size='1'><b>ಕಾಮಗಾರಿ ಆರಂಭ ದಿನಾಂಕ :</b></font>";
                string lastRow2td = "<font size='1'><b> ಒಟ್ಟು </b></font>";


                string lastrowtd = @"&nbsp;";

                IEnumerable<JToken> jcdata = jcdata1.OrderBy(r => int.Parse(r["SiNo"].ToString()));
                txtAgency1.InnerHtml = string.Format(txtAgency1.InnerText, otherdata.Property("agency").Value.ToString().Replace("(3)", "").Trim());
                txtBlock.InnerHtml = string.Format(txtBlock.InnerText, otherdata.Property("block").Value.ToString());
                txtDist.InnerHtml = string.Format(txtDist.InnerText, otherdata.Property("distName").Value.ToString());
                //txtEDdate1.InnerText = string.Format(txtEDdate1.InnerText,otherdata.Property("endDate").Value.ToString());
                //txtfinSanctionNoDate.InnerText = otherdata.Property("finSanNo").Value.ToString() + "(" + otherdata.Property("finSanDate").Value.ToString() + ")";
                txtFinyear.InnerHtml = string.Format(txtFinyear.InnerText, otherdata.Property("finYear").Value.ToString());
                txtMusterNo.InnerHtml = string.Format(txtMusterNo.InnerText, otherdata.Property("msrno").Value.ToString());
                txtPanchayat.InnerHtml = string.Format(txtPanchayat.InnerText, otherdata.Property("panchayat").Value.ToString());
                txtPrntDate.InnerHtml = string.Format(txtPrntDate.InnerText, DateTime.ParseExact(otherdata.Property("startDate").Value.ToString(), "d/M/yyyy", CultureInfo.InvariantCulture).ToString("d/M/yyyy", CultureInfo.InvariantCulture));
                txtState.InnerHtml = string.Format(txtState.InnerText, otherdata.Property("state").Value.ToString());
                txtStdate1.InnerHtml = string.Format(txtStdate1.InnerText, otherdata.Property("startDate").Value.ToString(), otherdata.Property("endDate").Value.ToString());
                txtTechsancNoDate1.InnerHtml = string.Format(txtTechsancNoDate1.InnerText, otherdata.Property("techSanNo").Value.ToString() + "(" + otherdata.Property("techSanDate").Value.ToString() + ")",
                                                             otherdata.Property("finSanNo").Value.ToString() + "(" + otherdata.Property("finSanDate").Value.ToString() + ")");
                txtWkCode1.InnerHtml = string.Format(txtWkCode1.InnerText, otherdata.Property("workcode").Value.ToString());
                txtWkName1.InnerHtml = string.Format(txtWkName1.InnerText, otherdata.Property("workname").Value.ToString());
                txttechStaff.InnerHtml = string.Format(txttechStaff.InnerHtml, otherdata.Property("techStaff").Value.ToString());
                thdays.ColSpan = int.Parse(otherdata.Property("wkdays").Value.ToString());

                HtmlTableRow daysRow = new HtmlTableRow();
                for (int i = 0; i < thdays.ColSpan; i++)
                {
                    HtmlTableCell daycell = new HtmlTableCell();
                    daycell.Align = "center";
                    // daycell.Height = "22";
                    daycell.Width = "3%";
                    daycell.InnerHtml = string.Format(dayth, (i + 1).ToString());
                    daysRow.Cells.Add(daycell);
                }
                HtmlTableCell daycat = new HtmlTableCell();
                daycat.Align = "center";
                //daycat.Height = "22";
                daycat.Width = "5%";
                daycat.InnerHtml = catth;
                daysRow.Cells.Add(daycat);

                tblnmr.Rows.Add(daysRow);

                foreach (var item in accdata)
                {
                    HtmlTableRow datarow = new HtmlTableRow();
                    datarow.Style.Add("width", "100%");
                    HtmlTableCell genCell = new HtmlTableCell();

                    // genCell.Height = "22";
                    genCell.Width = "139";
                    genCell.Align = "center";
                    genCell.InnerHtml = string.Format(tdgn, ((JObject)item).Property("SiNo").Value.ToString());

                    datarow.Cells.Add(genCell);

                    HtmlTableCell jobCardCell = new HtmlTableCell();
                    //jobCardCell.Height = "22";
                    jobCardCell.Width = "400";
                    jobCardCell.Align = "left";
                    jobCardCell.InnerHtml = string.Format(jbcard, ((JObject)item).Property("JobCardNo").Value.ToString());
                    datarow.Cells.Add(jobCardCell);

                    HtmlTableCell applicantCell = new HtmlTableCell();
                    //applicantCell.Height = "22";
                    applicantCell.Width = "139";
                    applicantCell.Align = "center";
                    applicantCell.Style.Add("font-family", "tunga");
                    applicantCell.InnerHtml = string.Format(tdgn, ((JObject)item).Property("ApplicantName").Value.ToString());



                    HtmlTableCell hh = new HtmlTableCell();
                    // hh.Height = "22";
                    hh.Width = "139";
                    hh.Align = "center";
                    hh.Style.Add("font-family", "tunga");
                    hh.InnerHtml = string.Format(tdgn, ((JObject)item).Property("hh").Value.ToString().Trim());

                    HtmlTableCell bkdetails = new HtmlTableCell();
                    bkdetails.InnerHtml = string.Format(bank, ((JObject)item).Property("bkname").Value.ToString().Trim(),
                                                              ((JObject)item).Property("bkacc").Value.ToString().Trim());
                    HtmlTableCell place = new HtmlTableCell();
                    // place.Height = "22";
                    place.Width = "139";
                    place.Align = "center";
                    place.Style.Add("font-family", "tunga");
                    place.InnerHtml = string.Format(tdgn, ((JObject)item).Property("Place").Value.ToString().Trim());

                    datarow.Cells.Add(hh);
                    datarow.Cells.Add(applicantCell);
                    datarow.Cells.Add(place);
                    datarow.Cells.Add(bkdetails);


                    for (int i = 0; i < thdays.ColSpan; i++)
                    {
                        HtmlTableCell daycell = new HtmlTableCell();
                        daycell.InnerHtml = daytd;
                        datarow.Cells.Add(daycell);
                    }
                    for (int i = 0; i < 7; i++)
                    {
                        if (i == 3)
                        {
                            HtmlTableCell cat = new HtmlTableCell();
                            // cat.Height = "22";
                            cat.Width = "206";
                            cat.Style.Add("text-align", "center");
                            cat.InnerText = ((JObject)item).Property("category").Value.ToString();
                            datarow.Cells.Add(cat);
                        }
                        else
                        {
                            HtmlTableCell daycell = new HtmlTableCell();
                            daycell.InnerHtml = lastrowtd;
                            datarow.Cells.Add(daycell);
                        }
                    }

                    tblnmr.Rows.Add(datarow);
                }
                HtmlTableRow LastRow = new HtmlTableRow();
                LastRow.Style.Add("width", "100%");
                HtmlTableCell lastrow1stcell = new HtmlTableCell();

                lastrow1stcell.Align = "left";
                lastrow1stcell.ColSpan = 5;
                lastrow1stcell.InnerHtml = lastRow1td;

                HtmlTableCell lastrow2stcell = new HtmlTableCell();
                lastrow2stcell.Align = "right";
                lastrow2stcell.InnerHtml = lastRow2td;

                LastRow.Cells.Add(lastrow1stcell);
                LastRow.Cells.Add(lastrow2stcell);

                for (int i = 0; i < thdays.ColSpan; i++)
                {
                    HtmlTableCell daycell = new HtmlTableCell();

                    daycell.Align = "center";
                    daycell.Width = "2%";
                    daycell.InnerHtml = daytd;
                    LastRow.Cells.Add(daycell);
                }
                for (int i = 0; i < 7; i++)
                {
                    HtmlTableCell daycell = new HtmlTableCell();

                    //daycell.Height = "22";
                    daycell.Width = "206";
                    daycell.InnerHtml = lastrowtd;
                    LastRow.Cells.Add(daycell);
                }

                tblnmr.Rows.Add(LastRow);

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