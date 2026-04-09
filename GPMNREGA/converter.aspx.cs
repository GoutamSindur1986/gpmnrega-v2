using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EvoPdf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Drawing;
using System.Drawing.Text;

namespace gpnmrega.templates
{
    public partial class converter : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string data = "";
            string fileName = "";
            //JObject json = new JObject();

            using (StreamReader reader = new StreamReader(HttpContext.Current.Request.InputStream))
            {
                try
                {
                    data = reader.ReadToEnd();
                }
                catch (Exception ex)
                {
                    gpmnrega2.Utility.Utility.SendErrorToText(ex);
                }
            }
            try
            {

                HtmlToPdfConverter htmlToPdfConverter = new HtmlToPdfConverter();
                htmlToPdfConverter.LicenseKey = "aOb25/T05/Pz8PLn/+n35/T26fb16f7+/v7n9w==";
                //htmlToPdfConverter.LicenseKey = "4W9+bn19bn5ue2B+bn1/YH98YHd3d3c=";
                if (Request.Cookies["test"] != null)
                {
                    byte[] checkSub = Convert.FromBase64String(Request.Cookies["test"].Value.Split('|')[1]);
                    if (Encoding.UTF8.GetString(checkSub) == "No")
                    {
                        htmlToPdfConverter.BeforeRenderPdfPageEvent += new BeforeRenderPdfPageDelegate(htmlToPdfConverter_BeforeRenderPdfPageEvent);
                    }
                }
                else
                {
                    htmlToPdfConverter.BeforeRenderPdfPageEvent += new BeforeRenderPdfPageDelegate(htmlToPdfConverter_BeforeRenderPdfPageEvent);

                }

                htmlToPdfConverter.ConversionDelay = 0;

                htmlToPdfConverter.PdfDocumentOptions.LeftMargin = 5;
                htmlToPdfConverter.PdfDocumentOptions.RightMargin = 5;
                htmlToPdfConverter.PdfDocumentOptions.TopMargin = 5;
                byte[] outPdfBuffer = Encoding.ASCII.GetBytes("");

                if (Request.Params["page"] != null)
                {
                    switch (Request.Params["page"].ToString())
                    {
                        case "CoverPage":
                            outPdfBuffer = htmlToPdfConverter.ConvertHtml(data, "https://" + Request.Url.Authority + "/templates/" + Request.Params["stateName"].ToString() + "/CoverPage");
                            fileName = Request.QueryString["workcode"]+ "_" + DateTime.Now.ToString("yyyy-MM-dd-mm-ss") + "_" + "CoverPage.pdf";
                            break;
                        case "WorkOrder":
                            outPdfBuffer = htmlToPdfConverter.ConvertHtml(data, "https://" + Request.Url.Authority + "/templates/" + Request.Params["stateName"].ToString() + "/workorder.aspx");
                            fileName = Request.QueryString["workcode"] + "_" + DateTime.Now.ToString("yyyy-MM-dd-mm-ss") + "_" + "workorder.pdf";
                            break;
                        case "completion":
                            outPdfBuffer = htmlToPdfConverter.ConvertHtml(data, "https://" + Request.Url.Authority + "/templates/" + Request.Params["stateName"].ToString() + "/completion.aspx");
                            fileName = Request.QueryString["workcode"] + "_" + DateTime.Now.ToString("yyyy-MM-dd-mm-ss") + "_" + "completion.pdf";
                            break;
                        case "checklist":
                            outPdfBuffer = htmlToPdfConverter.ConvertHtml(data, "https://" + Request.Url.Authority + "/templates/" + Request.Params["stateName"].ToString() + "/checklist.aspx");
                            fileName = Request.QueryString["workcode"] + "_" + DateTime.Now.ToString("yyyy-MM-dd-mm-ss") + "_" + "checklist.pdf";
                            break;
                        case "musterrolemov":
                            outPdfBuffer = htmlToPdfConverter.ConvertHtml(data, "https://" + Request.Url.Authority + "/templates/" + Request.Params["stateName"].ToString() + "/musterrolemov.aspx");
                            fileName = Request.QueryString["workcode"] + "_" + DateTime.Now.ToString("yyyy-MM-dd-mm-ss") + "_" + "musterrolemov.pdf";
                            break;
                        case "Form6":
                            outPdfBuffer = htmlToPdfConverter.ConvertHtml(data, "https://" + Request.Url.Authority + "/templates/" + Request.Params["stateName"].ToString() + "/form6");
                            fileName = Request.QueryString["workcode"] + "_" + DateTime.Now.ToString("yyyy-MM-dd-mm-ss") + "_" + "form6.pdf";
                            break;
                        case "Form8":
                            outPdfBuffer = htmlToPdfConverter.ConvertHtml(data, "https://" + Request.Url.Authority + "/templates/" + Request.Params["stateName"].ToString() + "/form8");
                            fileName = Request.QueryString["workcode"] + "_" + DateTime.Now.ToString("yyyy-MM-dd-mm-ss") + "_" + "form8.pdf";
                            break;
                        case "Form9":
                            outPdfBuffer = htmlToPdfConverter.ConvertHtml(data, "https://" + Request.Url.Authority + "/templates/" + Request.Params["stateName"].ToString() + "/form9");
                            fileName = Request.QueryString["workcode"] + "_" + DateTime.Now.ToString("yyyy-MM-dd-mm-ss") + "_" + "form9.pdf";
                            break;
                        case "FTO":
                            htmlToPdfConverter.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Landscape;
                            outPdfBuffer = htmlToPdfConverter.ConvertHtml(data, "https://" + Request.Url.Authority + "/templates/" + Request.Params["stateName"].ToString() + "/blanknmr.aspx");
                            fileName = Request.QueryString["FTO"]   + "_" + DateTime.Now.ToString("yyyy-MM-dd-mm-ss") + "_" + "FTO.pdf";
                            break;
                        case "WageList":
                            htmlToPdfConverter.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Landscape;
                            outPdfBuffer = htmlToPdfConverter.ConvertHtml(data, "https://" + Request.Url.Authority + "/templates/" + Request.Params["stateName"].ToString() + "/blanknmr.aspx");
                            fileName = Request.QueryString["WageList"] + "_" + DateTime.Now.ToString("yyyy-MM-dd-mm-ss") + "_" + "WageList.pdf";
                            break;
                        case "blknmr":
                            htmlToPdfConverter.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Landscape;
                            outPdfBuffer = htmlToPdfConverter.ConvertHtml(data, "https://" + Request.Url.Authority + "/templates/" + Request.Params["stateName"].ToString() + "/blknmr1.aspx");
                            fileName = Request.Params["workcode"].ToString() + "_" + DateTime.Now.ToString("yyyy-MM-dd-mm-ss") + "_" + "blanknmr.pdf";
                            break;
                        case "filledNmr":
                            htmlToPdfConverter.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Landscape;
                            outPdfBuffer = htmlToPdfConverter.ConvertHtml(data, "https://" + Request.Url.Authority + "/templates/" + Request.Params["stateName"].ToString() + "/blknmr1.aspx");
                            fileName = Request.Params["workcode"].ToString() + "_" + DateTime.Now.ToString("yyyy-MM-dd-mm-ss") + "_" + "filledNMR.pdf";
                            break;
                        case "GeoTag":
                            outPdfBuffer = htmlToPdfConverter.ConvertHtml(data, "https://" + Request.Url.Authority + "/templates/geotag");
                            fileName = Request.Params["workcode"].ToString() + "_" + DateTime.Now.ToString("yyyy-MM-dd-mm-ss") + "_" + "geotag.pdf";
                            break;
                        default:
                            outPdfBuffer = Encoding.ASCII.GetBytes("No Param Found.");
                            break;
                    }

                }
                // Send the PDF as response to browser

                // Set response content type
                Response.AddHeader("Content-Type", "application/pdf");

                // Instruct the browser to open the PDF file as an attachment or inline
                Response.AddHeader("Content-Disposition", String.Format("attachment; filename=" + fileName + "; size={0}", outPdfBuffer.Length.ToString()));

                // Write the PDF document buffer to HTTP response
                Response.BinaryWrite(outPdfBuffer);

                // End the HTTP response and stop the current page processing

                Response.End();

            }
            catch (Exception ex)
            {
                gpmnrega2.Utility.Utility.SendErrorToText(ex);

            }

        }

        private void HtmlToPdfConverter_BeforeRenderPdfPageEvent(BeforeRenderPdfPageParams eventParams)
        {

        }

        private Uri Uri(string v)
        {
            throw new NotImplementedException();
        }

        void htmlToPdfConverter_BeforeRenderPdfPageEvent(BeforeRenderPdfPageParams eventParams)
        {
            try
            {
                // Get the PDF page being rendered
                PdfPage pdfPage = eventParams.Page;

                // Get the PDF page drawable area width and height
                float pdfPageWidth = pdfPage.ClientRectangle.Width;
                float pdfPageHeight = pdfPage.ClientRectangle.Height;

                // The image to be added as background
                string backgroundImagePath = Server.MapPath("~/Content/trial.png");

                // The image element to add in background
                ImageElement backgroundImageElement = new ImageElement(0, 0, pdfPageWidth, pdfPageHeight, backgroundImagePath);
                backgroundImageElement.KeepAspectRatio = false;
                backgroundImageElement.EnlargeEnabled = true;

                // Add the background image element to PDF page before the main content is rendered
                pdfPage.AddElement(backgroundImageElement);

                PdfFont titleFont = pdfPage.Document.AddFont(new Font("Times New Roman", 20, FontStyle.Bold, GraphicsUnit.Point));
                PdfFont subtitleFont = pdfPage.Document.AddFont(new Font("Times New Roman", 15, FontStyle.Regular, GraphicsUnit.Point));
                PdfFont linkTextFont = pdfPage.Document.AddFont(new Font("Times New Roman", 15, FontStyle.Bold, GraphicsUnit.Point));
                linkTextFont.IsUnderline = true;

                float xLocation = pdfPage.ClientRectangle.Width / 3;
                float yLocation = pdfPage.ClientRectangle.Height / 2;

                // Add document title
                TextElement titleTextElement = new TextElement(xLocation, yLocation, "This is trial version", titleFont);
                AddElementResult addElementResult = pdfPage.AddElement(titleTextElement);

                yLocation = addElementResult.EndPageBounds.Bottom + 15;
                string text = "CLICK HERE TO BUY PAID VERSION!";
                float textWidth = linkTextFont.GetTextWidth(text);
                TextElement linkTextElement = new TextElement(xLocation, yLocation, text, linkTextFont);
                linkTextElement.ForeColor = Color.Navy;
                addElementResult = pdfPage.AddElement(linkTextElement);

                // Create the URI link element having the size of the text element
                RectangleF linkRectangle = new RectangleF(xLocation, yLocation, textWidth, addElementResult.EndPageBounds.Height);
                string url = "https://www.gpmnrega.com/auth/PaySub";
                LinkUrlElement uriLink = new LinkUrlElement(linkRectangle, url);
                pdfPage.AddElement(uriLink);
            }
            catch (Exception ex)
            {
                gpmnrega2.Utility.Utility.SendErrorToText(ex);

            }
        }
    }
}