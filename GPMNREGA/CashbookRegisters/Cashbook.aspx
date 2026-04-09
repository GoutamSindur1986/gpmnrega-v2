<%@ Page Title="" Language="C#" MasterPageFile="~/Auth/Auth.Master" AutoEventWireup="true" CodeBehind="Cashbook.aspx.cs" Inherits="gpmnrega2.Auth.Cashbook" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <script type="text/javascript" src="../Scripts/jquery-3.4.1.min.js"></script>
    <script type="text/javascript" src="../Scripts/pdfmake.js"></script>

    <script type="text/javascript" src="../Scripts/db.js?v1.5"></script>
    <script type="text/javascript" src="../Scripts/home1.js?v1.7.4"></script>
    <script type="text/javascript" src="../Scripts/mr.js?v1.4.0"></script>
    <script type="text/javascript" src="../Scripts/cashbook.js?v0.3.5"></script>
    <script type="text/javascript" src="../Scripts/moment.js?v1.1.9"></script>
    <script type="text/javascript" src="../Scripts/pdflib.js?v1.1.9"></script>

    <script type="text/javascript" src="../Scripts/vfs_fonts.js"></script>

    <style>
        body {
            justify-content: center;
        }

        .workbtn1 {
            border-radius: 5px 5px 5px 5px;
            color: midnightblue;
            height: 36px;
            width: 103px;
            margin-left: 12px;
            cursor: pointer;
            font-size: 9px;
            font-weight: 700;
            margin-top: 3px;
            margin-bottom: 3px;
        }

        .workbtn2 {
            border-radius: 5px 5px 5px 5px;
            color: midnightblue;
            height: 36px;
            width: 185px;
            margin-left: 16px;
            cursor: pointer;
            font-size: 9px;
            font-weight: 700;
            margin-top: 3px;
            margin-bottom: 3px;
        }

            .workbtn2:hover {
                background: radial-gradient(circle, rgba(238,174,202,1) 0%, rgba(148,187,233,1) 100%);
            }

        #progressBar {
            z-index: 10;
            position: absolute;
            top: 0;
            text-align: center;
            width: 100%;
            display: none;
        }

        .container {
            padding: 20px 30px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="width: 850px" runat="server" id="divToPDF">
        <span style="font-family: tunga; font-size: 12px; font-weight: bold"><a href="../GPGuidelinefiles/GP_7_Register.pdf" target="_blank" style="font-family: tunga;">ಈ ಸುತ್ತೋಲೆಯಂತೆ</a> III & V ರೆಜಿಸ್ಟರ್‌ಗಳನ್ನು ಅಭಿವೃದ್ಧಿಪಡಿಸಲಾಗಿದೆ. </span>
        <fieldset>
            <legend style="margin-left: 35%; padding: 2px">
                <span>Financial Year:</span>
                <select id="ddlyear" class="searchDrop">
                    <option value="2026-2027">2026-2027</option>
                    <option value="2025-2026">2025-2026</option>
                    <option value="2024-2025">2024-2025</option>
                    <option value="2023-2024">2023-2024</option>
                </select>
            </legend>
            <table style="line-height: 3">
                <tr>

                    <td></td>
                </tr>
                <tr>
                    <td colspan="7" style="text-align: center">
                        <h4>Cashbook/Registers</h4>
                    </td>
                </tr>
                <tr>
                    <td style="width: 50%">
                        <fieldset>
                            <legend style="margin-left: 30%; padding: 2px"><span style="font-size: 12px; font-weight: 600">Select Month</span>
                                <select id="ddlMonth">
                                    <option value="00">Select</option>
                                    <option value="01">Jan</option>
                                    <option value="02">Feb</option>
                                    <option value="03">Mar</option>
                                    <option value="04">Apr</option>
                                    <option value="05">May</option>
                                    <option value="06">Jun</option>
                                    <option value="07">Jul</option>
                                    <option value="08">Aug</option>
                                    <option value="09">Sep</option>
                                    <option value="10">Oct</option>
                                    <option value="11">Nov</option>
                                    <option value="12">Dec</option>
                                </select>
                                &nbsp <span style="font-size: 12px; font-weight: 600">Select Quater</span>
                                <select id="ddlQtr">
                                    <option value="00">Select</option>
                                    <option value="0">01(Apr,May,Jun)</option>
                                    <option value="1">02(Jul,Aug,Sep)</option>
                                    <option value="2">03(Oct,Nov,Dec)</option>
                                    <option value="3">04(Jan,Feb,Mar)</option>
                                </select>
                            </legend>
                            <input type="button" id="FTOWageCash" value="FTO Wage Cash Book (Monthly)" class="workbtn2" />
                            <input type="button" id="FTOMatCash" value="FTO Material Cash Book (Monthly)" class="workbtn2" />
                            <input type="button" id="Register1" value="Register 1B (Quaterly)" class="workbtn2" />
                            <input type="button" id="Register3" value="Register 3 (Monthly)" class="workbtn2" />
                            <input type="button" id="Register4" value="Register 4 (Quaterly)" class="workbtn2" />
                            <input type="button" id="Register5" value="Register 5 (Yearly)" class="workbtn2" />
                            <input type="button" id="Register6" value="Register 6 (Yearly)" class="workbtn2" />
                            <input type="button" id="Register7" value="Register 7 (Yearly)" class="workbtn2" />
                        </fieldset>
                    </td>
                </tr>

            </table>
        </fieldset>
        <div id="hiddendiv" style="display: none">
        </div>
        <div id="progressBar" style="flex-flow: row; justify-content: center; width: 20%; margin-top: 15%; margin-left: 40%;">
            <img src="../Content/downloading.gif" alt="downloading" id="imgDown" style="display: none; margin-top: 20%" />
            <img src="../Content/success1.gif" alt="downloading" id="downSuc" style="display: none; position: absolute; width: 50px; margin-top: 20%" />
            <img src="../Content/failed.gif" alt="downloading" id="downFail" style="display: none; position: absolute; width: 50px; margin-top: 20%" />
        </div>
    </div>
</asp:Content>
