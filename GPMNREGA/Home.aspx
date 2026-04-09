<%@ Page Title="" Language="C#" MasterPageFile="~/Auth/Auth.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="gpnmrega.Auth.Home" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript" src="../Scripts/jquery-3.4.1.min.js"></script>
    <script type="text/javascript" src="../Scripts/db.js?v1.4"></script>
    <script type="text/javascript" src="../Scripts/WebForms/storage.js?0.0.1"></script>
    <script type="text/javascript" src="../Scripts/gphome.js?v1.6"></script>
    <script type="text/javascript" src="../Scripts/Formtemplates/form6.js"></script>
    <script type="text/javascript" src="../Scripts/Formtemplates/form9.js"></script>
    <script type="text/javascript" src="../Scripts/Formtemplates/form8.js"></script>
    <script type="text/javascript" src="../Scripts/cmpStatement.js"></script>
    <script type="text/javascript" src="../Scripts/mr.js?v1.7.0.1"></script>
    <script type="text/javascript" src="../Scripts/moment.js?v1.1.9"></script>
    <script type="text/javascript" src="../Scripts/Formtemplates/checklist.js"></script>
    <script type="text/javascript" src="../Scripts/pdfmake.js"></script>
    <script type="text/javascript" src="../Scripts/vfs_fonts.js"></script>
    <script type="text/javascript" src="../Scripts/pdflib.js?v1.1.9"></script>
    <script type="text/javascript" src="../Scripts/Formtemplates/form8plus.js"></script>
    <script type="text/javascript" src="../Scripts/modalpopup.js"></script>
    <script type="text/javascript" src="../Scripts/Formtemplates/generategeotag.js"></script>
    <script type="text/javascript" src="../Scripts/Formtemplates/MusterRollMovement.js"></script>
    <style type="text/css" href="../Scripts/jquery-ui.min.css"></style>
    <script type="text/javascript" src="../Scripts/jquery-ui.min.js"></script>

    <style>
        .download {
            position: sticky;
            box-shadow: 2px 2px 3px #999;
            z-index: 100;
            top: 40vh;
        }

        .my-float {
            margin-top: 16px;
        }

        @font-face {
            font-family: tunga;
            src: url('../fonts/tunga.ttf');
        }

        @import url(https://fonts.googleapis.com/css?family=Open+Sans);

        .search {
            position: relative;
            display: flex;
        }

        .searchTerm {
            width: 100%;
            border: 3px solid #00B4CC;
            border-right: none;
            padding: 5px;
            height: 36px;
            border-radius: 5px 0 0 5px;
            outline: none;
            color: #000000;
        }

        .workbtn {
            border-radius: 5px 5px 5px 5px;
            color: midnightblue;
            height: 36px;
            width: 132px;
            margin-left: 10px;
            cursor: pointer;
            margin-top: 5px;
            margin-bottom: 5px;
            font-weight: 600;
            font-size: 12px;
        }

        .workbtn1 {
            border-radius: 5px 5px 5px 5px;
            color: midnightblue;
            height: 36px;
            width: 100px;
            margin-left: 2px;
            cursor: pointer;
            font-size: 9px;
            font-weight: 700;
            margin-top: 3px;
            margin-bottom: 3px;
        }

        .workbtn:hover {
            background: radial-gradient(circle, rgba(238,174,202,1) 0%, rgba(148,187,233,1) 100%);
        }

        .workbtn1:hover {
            background: radial-gradient(circle, rgba(238,174,202,1) 0%, rgba(148,187,233,1) 100%);
        }

        .searchDrop {
            width: 40%;
            border: 3px solid #00B4CC;
            padding: 5px;
            height: 36px;
            border-radius: 5px 5px 5px 5px;
            outline: none;
            color: #000000;
        }

        .searchTerm:focus {
            color: #000000;
        }

        .searchButton {
            width: 40px;
            height: 36px;
            border: 1px solid #00B4CC;
            background: #00B4CC;
            text-align: center;
            color: #fff;
            border-radius: 0 5px 5px 0;
            cursor: pointer;
            font-size: 20px;
        }

        /*Resize the wrap to see the search bar change!*/
        .wrap {
        }
        /*Resize the wrap to see the search bar change!*/
        table#tblWorkInfo {
            border: 1px solid black;
            border-collapse: collapse;
            width: 100%;
            display: none;
            margin-top: 3%;
        }

            table#tblWorkInfo th {
                border: 1px solid black;
                border-collapse: collapse;
            }

            table#tblWorkInfo td {
                border: 1px solid black;
                border-collapse: collapse;
            }

        table#tblNMR {
            border: 1px solid black;
            border-collapse: collapse;
        }

            table#tblNMR tr {
                border: 1px solid black;
                border-collapse: collapse;
            }

            table#tblNMR td {
                border: 1px solid black;
                border-collapse: collapse;
            }

        
        /* NMR list (new) */
        ul.nmr-list{
            list-style:none;
            padding:0;
            margin:0;
        }
        ul.nmr-list > li{
            display:flex;
            align-items:center;
            justify-content:space-between;
            gap:10px;
            padding:10px 12px;
            border:1px solid #d0d7de;
            border-radius:10px;
            margin:10px 0;
            background:#fff;
        }
        .nmr-meta{
            display:flex;
            align-items:center;
            gap:10px;
            min-width:200px;
            font-weight:700;
            color:#0b1f44;
        }
        .nmr-actions{
            display:flex;
            flex-wrap:wrap;
            gap:8px;
            justify-content:flex-end;
        }
        .nmr-actions .workbtn{
            margin-left:0 !important;
            margin-top:0;
            margin-bottom:0;
        }
        .nmr-download{
            width:18px;
            height:18px;
            cursor:pointer;
            vertical-align:middle;
        }
#progressBar {
            display: none;
        }



        header {
            border: 3px solid;
            height: 300px;
            position: relative;
        }

            header:before {
                content: 'Header';
                position: absolute;
                top: -10px;
                left: 50px;
                background: #fff;
                padding: 0 20px;
            }

        .blocker {
            position: fixed;
            top: 0;
            right: 0;
            bottom: 0;
            left: 0;
            width: 100%;
            height: 100%;
            overflow: auto;
            z-index: 1;
            padding: 20px;
            box-sizing: border-box;
            background-color: #000;
            background-color: rgba(0,0,0,0.75);
            text-align: center
        }

            .blocker:before {
                content: "";
                display: inline-block;
                height: 100%;
                vertical-align: middle;
                margin-right: -0.05em
            }

            .blocker.behind {
                background-color: transparent
            }

        .modal {
            display: none;
            vertical-align: middle;
            position: relative;
            z-index: 2;
            box-sizing: border-box;
            /*width: 90%;*/
            background: #fff;
            padding: 15px 30px;
            -webkit-border-radius: 8px;
            -moz-border-radius: 8px;
            -o-border-radius: 8px;
            -ms-border-radius: 8px;
            border-radius: 8px;
            -webkit-box-shadow: 0 0 10px #000;
            -moz-box-shadow: 0 0 10px #000;
            -o-box-shadow: 0 0 10px #000;
            -ms-box-shadow: 0 0 10px #000;
            box-shadow: 0 0 10px #000;
            text-align: left
        }

            .modal a.close-modal {
                position: absolute;
                top: -12.5px;
                right: -12.5px;
                display: block;
                width: 30px;
                height: 30px;
                text-indent: -9999px;
                background-size: contain;
                background-repeat: no-repeat;
                background-position: center center;
                background-image: url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADwAAAA8CAYAAAA6/NlyAAAAAXNSR0IArs4c6QAAA3hJREFUaAXlm8+K00Acx7MiCIJH/yw+gA9g25O49SL4AO3Bp1jw5NvktC+wF88qevK4BU97EmzxUBCEolK/n5gp3W6TTJPfpNPNF37MNsl85/vN/DaTmU6PknC4K+pniqeKJ3k8UnkvDxXJzzy+q/yaxxeVHxW/FNHjgRSeKt4rFoplzaAuHHDBGR2eS9G54reirsmienDCTRt7xwsp+KAoEmt9nLaGitZxrBbPFNaGfPloGw2t4JVamSt8xYW6Dg1oCYo3Yv+rCGViV160oMkcd8SYKnYV1Nb1aEOjCe6L5ZOiLfF120EjWhuBu3YIZt1NQmujnk5F4MgOpURzLfAwOBSTmzp3fpDxuI/pabxpqOoz2r2HLAb0GMbZKlNV5/Hg9XJypguryA7lPF5KMdTZQzHjqxNPhWhzIuAruOl1eNqKEx1tSh5rfbxdw7mOxCq4qS68ZTjKS1YVvilu559vWvFHhh4rZrdyZ69Vmpgdj8fJbDZLJpNJ0uv1cnr/gjrUhQMuI+ANjyuwftQ0bbL6Erp0mM/ny8Fg4M3LtdRxgMtKl3jwmIHVxYXChFy94/Rmpa/pTbNUhstKV+4Rr8lLQ9KlUvJKLyG8yvQ2s9SBy1Jb7jV5a0yapfF6apaZLjLLcWtd4sNrmJUMHyM+1xibTjH82Zh01TNlhsrOhdKTe00uAzZQmN6+KW+sDa/JD2PSVQ873m29yf+1Q9VDzfEYlHi1G5LKBBWZbtEsHbFwb1oYDwr1ZiF/2bnCSg1OBE/pfr9/bWx26UxJL3ONPISOLKUvQza0LZUxSKyjpdTGa/vDEr25rddbMM0Q3O6Lx3rqFvU+x6UrRKQY7tyrZecmD9FODy8uLizTmilwNj0kraNcAJhOp5aGVwsAGD5VmJBrWWbJSgWT9zrzWepQF47RaGSiKfeGx6Szi3gzmX/HHbihwBser4B9UJYpFBNX4R6vTn3VQnez0SymnrHQMsRYGTr1dSk34ljRqS/EMd2pLQ8YBp3a1PLfcqCpo8gtHkZFHKkTX6fs3MY0blKnth66rKCnU0VRGu37ONrQaA4eZDFtWAu2fXj9zjFkxTBOo8F7t926gTp/83Kyzzcy2kZD6xiqxTYnHLRFm3vHiRSwNSjkz3hoIzo8lCKWUlg/YtGs7tObunDAZfpDLbfEI15zsEIY3U/x/gHHc/G1zltnAgAAAABJRU5ErkJggg==')
            }

        .modal-spinner {
            display: none;
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translateY(-50%) translateX(-50%);
            padding: 12px 16px;
            border-radius: 5px;
            background-color: #111;
            height: 20px
        }

            .modal-spinner > div {
                border-radius: 100px;
                background-color: #fff;
                height: 20px;
                width: 2px;
                margin: 0 1px;
                display: inline-block;
                -webkit-animation: sk-stretchdelay 1.2s infinite ease-in-out;
                animation: sk-stretchdelay 1.2s infinite ease-in-out
            }

            .modal-spinner .rect2 {
                -webkit-animation-delay: -1.1s;
                animation-delay: -1.1s
            }

            .modal-spinner .rect3 {
                -webkit-animation-delay: -1.0s;
                animation-delay: -1.0s
            }

            .modal-spinner .rect4 {
                -webkit-animation-delay: -0.9s;
                animation-delay: -0.9s
            }

        @-webkit-keyframes sk-stretchdelay {
            0%,40%,100% {
                -webkit-transform: scaleY(0.5)
            }

            20% {
                -webkit-transform: scaleY(1.0)
            }
        }

        @keyframes sk-stretchdelay {
            0%,40%,100% {
                transform: scaleY(0.5);
                -webkit-transform: scaleY(0.5)
            }

            20% {
                transform: scaleY(1.0);
                -webkit-transform: scaleY(1.0)
            }
        }

        .cmptable tr {
            padding-bottom: 1%;
        }

        .centered {
            position: fixed;
            top: 40vh;
            left: 50%;
            /* bring your own prefixes */
            transform: translate(-50%, -50%);
        }

        .ui-menu-item {
            list-style: none;
            font-size: x-small;
            text-wrap: nowrap;
            cursor: default;
        }

        .ui-helper-hidden-accessible {
            display: none;
        }

        form {
            width: 100%;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table>
        <tr>
            <td style="vertical-align: top;">
                <div style="width: 950px" runat="server" id="divToPDF" onscroll="setProgressBar()">

                    <table style="width: 100%; padding-left: 2%; padding-right: 2%">
                        <tr>
                            <td style="vertical-align: bottom; width: 40%">
                                <span>Financial Year:</span>
                                <select id="ddlyear" class="searchDrop">
                                    <option value="2026-2027">2026-2027</option>
                                    <option value="2025-2026">2025-2026</option>
                                    <option value="2024-2025">2024-2025</option>                                    
                                </select>
                            </td>
                            <td style="vertical-align: top">
                                <span style="font-size: 12px; color: cornflowerblue">Search workcode in</span>  <span style="font-size: 12px; color: cornflowerblue" id="spnSearchPanchyat"></span><span style="font-size: 12px; color: cornflowerblue; padding-left: 2px">panchayat for year</span>  <span id="spnfinyear" style="font-size: 12px; color: cornflowerblue"></span>
                                <div class="wrap">

                                    <div class="search">
                                        <span style="white-space: pre; padding-top: 6px">Work Code:</span>
                                        <input type="text" id="txtSearchWorkCode" class="searchTerm" placeholder="Enter work code.....">
                                        <button type="submit" class="searchButton" id="btnSearch">
                                            <i class="fa fa-search"></i>
                                        </button>
                                    </div>
                                </div>
                            </td>
                        </tr>
                    </table>

                    <div id="resultPanel" style="background-image: url('/templates/homebgimgs/gandhiimg.JPG'); min-height: 400px; background-repeat: no-repeat; background-size: cover; width: 98%">
                        <div id="divResults">
                            <img src="../templates/homebgimgs/search.gif" style="width: 100px; margin-left: 50%; display: none" id="imgSearch" />
                            <img src="../templates/homebgimgs/search-bar.gif" style="width: 400px; height: 300px; margin-left: 25%; display: none;" id="imgResNotFound" />

                            <table id="tblWorkInfo" style="line-height: 2">

                                <tr>
                                    <td colspan="7" style="text-align: center">
                                        <h4>Work Forms</h4>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="2">
                                        <input type="button" id="btnCoverPage" class="workbtn" name="CoverPage" value="Cover Page" title="Cover Page" style="margin-left: 10px;" />
                                        <input type="button" id="btnWorkOrder" class="workbtn" name="WorkOrder" value="Work Order" title="Work Order" style="margin-left: 1%;" />
                                        <input type="button" id="btnCompletion" class="workbtn" name="completion" value="Work Completion" title="Work Completion" style="margin-left: 1%;" />
                                        <input type="button" id="btnCheckList" class="workbtn" name="checklist" value="Check List" title="Check List" style="margin-left: 1%;" />
                                        <input type="button" id="btnMstMov" class="workbtn" name="musterrolemov" value="MusterRole Movement" title="MusterRole Movement" style="margin-left: 1%;" />
                                        <input type="button" id="geotag" class="workbtn" name="geotag" value="GeoTag" title="Geo Tag" style="margin-left: 1%;" />
                                    </td>
                                </tr>

                                <tr style="font-size: 12px; font-family: tunga; font-weight: 600; margin-bottom: 5px">
                                    <td><span>Work Name:</span></td>
                                    <td><span style="font-family: tunga;" id="wrkName"></span></td>
                                </tr>
                                <tr style="font-size: 12px; font-weight: 600; margin-bottom: 5px">
                                    <td><span>Work Code:</span></td>
                                    <td><span id="wrkCode"></span></td>
                                </tr>
                                <tr style="font-size: 12px; font-weight: 600; margin-bottom: 5px">
                                    <td><span>Work Category:</span></td>
                                    <td><span id="wrkCategory"></span></td>
                                </tr>
                                <tr style="font-size: 12px; font-weight: 600; margin-bottom: 5px">
                                    <td><span>Execution Agency:</span></td>
                                    <td><span id="exeAgency"></span></td>
                                </tr>
                                <tr style="font-size: 12px; font-weight: 600; margin-bottom: 5px">
                                    <td><span>Financial Sanction No & Sanction Date:</span></td>
                                    <td><span id="finNoDate"></span></td>
                                </tr>
                                <tr style="font-size: 12px; font-weight: 600; margin-bottom: 5px">
                                    <td><span>Technical Sanction No & Sanction Date:</span></td>
                                    <td><span id="techNoDate"></span></td>
                                </tr>
                                <tr style="font-size: 12px; font-weight: 600; margin-bottom: 5px">
                                    <td><span>Comparative Statement</span></td>
                                    <td><span id="cmpstatement">
                                        <a href="javascript:buildCmpStatement()">Invitation/Comparative Statement/Supply Order</a>
                                    </span></td>
                                </tr>
                                <tr>
                                    <td colspan="2">
                                        <ul id="tblNMR" class="nmr-list"></ul>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </div>
                </div>
                <div id="progressBar" class="download">
                    <img src="../Content/downloading.gif" alt="downloading" id="imgDown" class="centered" style="display: none;" />
                    <img src="../Content/success1.gif" alt="downloading" id="downSuc" class="centered" style="display: none; width: 50px;" />
                    <img src="../Content/failed.gif" alt="downloading" id="downFail" class="centered" style="display: none; width: 50px;" />
                </div>
                <div style="display: none">
                    <canvas id="can"></canvas>
                    <img src="" id="geoimage" />
                </div>


                <div id="cmpstatmodule" class="modal">
                    <div>
                        <table width="100%">
                            <tr>
                                <td style="text-align: center"><span>Invitation Date: </span>
                                    <input type="date" id="invitationDate" />
                                </td>
                                <td style="text-align: center"><span>Comparative Date: </span>
                                    <input type="date" id="ComparativeDate" />
                                </td>
                                <td style="text-align: center"><span>Supply Order Date: </span>
                                    <input type="date" id="supOrderDate" />
                                </td>
                                <td style="text-align: center"><span>Supply Date: </span>
                                    <input type="date" id="supDate" />
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div id="vendorTbl" style="display: inline-block; padding-top: 2%; padding-bottom: 2%; width: 100%">
                    </div>
                    <br />
                    <input id="btnInvitation" type="button" onclick="generateCMPStatement()" style="margin-left: 30%; border-radius: 5px 5px 5px 5px; width: 40%; margin-bottom: 2%; cursor: pointer" value="Generate Invitation/Comparative/Supply Order statement" />
                </div>
                <div id="cmptemptable" style="display: none">
                    <table class="cmptable" style="border: 2px solid black; border-collapse: collapse; width: 100%" border="1">
                        <tr style="border-bottom: 3px solid cornflowerblue">
                            <th style="font-size: .7em; text-align: center">
                                <label for="selectedmat">Select</label><input type="checkbox" id="selectedmat" onchange="selectmat(this)" />
                            </th>
                            <th style="font-size: .7em; text-align: center">ಕ್ರ.ಸಂ.</th>
                            <th style="font-size: .7em; text-align: center; word-wrap: break-word">ಸಾಮಗ್ರಿ</th>
                            <th style="font-size: .7em; text-align: center">ಪ್ರಮಾಣ</th>
                            <th style="font-size: .7em; text-align: center">ದರ (SR)</th>
                            <th style="font-size: .7em; text-align: center">ಭಾಗವಹಿಸಿದವರ-1 
                    <br />
                                <span>GST</span>
                                <div id="vnd1autocomplete">
                                    <input type="text" class="vnd_1_Gst vendor" aria-label='vender1gst' style="padding-bottom: 3px" id="vnd1gst" />
                                </div>
                                <br />
                                <span>Name</span>
                                <input type="text" id="participant1name" value="" />
                                <br />
                                <span>Address</span>
                                <textarea aria-label='vender1' style="border-radius: 5px; height: 75px; width: 160px; border-width: 2px; border-color: cornflowerblue; resize: none" class="participant1"></textarea>

                            </th>
                            <th style="font-size: .7em; text-align: center">ಭಾಗವಹಿಸಿದವರ-2
                    <br />
                                <span>GST</span>
                                <input type="text" aria-label='vender2gst' class="vnd_2_Gst vendor" id="vnd2gst" />
                                <br />
                                <span>Name</span>
                                <input type="text" id="participant2name" value="" />
                                <br />
                                <span>Address</span>
                                <textarea aria-label='vender2' style="border-radius: 5px; height: 75px; width: 160px; border-width: 2px; border-color: cornflowerblue; resize: none" class="participant2"></textarea>


                            </th>
                            <th style="font-size: .7em; text-align: center">ಭಾಗವಹಿಸಿದವರ-3
                    <br />
                                <span>GST</span>
                                <input type="text" aria-label='vender3gst' class="vnd_3_Gst vendor" id="vnd3gst" /><br />
                                <span>Name</span>
                                <input type="text" id="participant3name" value="" />
                                <br />
                                <span>Address</span>
                                <textarea aria-label='vender3' style="border-radius: 5px; height: 75px; width: 160px; border-width: 2px; border-color: cornflowerblue; resize: none" class="participant3"></textarea>


                            </th>


                        </tr>


                    </table>
                </div>
            </td>
            <td style="vertical-align: text-top; text-wrap: nowrap; border-left: azure; border-left: groove;">
                <table>
                    <tr>
                        <td colspan="2">
                            <span style="text-decoration: underline; font-size: xx-small; font-weight: 600; color: brown; font-weight: 600">Works</span>
                        </td>

                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">Approved Works:</span>&nbsp&nbsp<span id="appWork" style="color: cadetblue; font-size: xx-small; font-weight: 600">Loading..</span>
                        </td>

                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">Ongoing Works:</span> &nbsp&nbsp <span id="ongoingWork" style="color: cadetblue; font-size: xx-small; font-weight: 600">Loading..</span>
                        </td>

                    </tr>

                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">Completed Works:</span>&nbsp&nbsp<span id="completedWork" style="color: cadetblue; font-size: xx-small; font-weight: 600">Loading..</span>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <span style="text-decoration: underline; font-size: xx-small; font-weight: 600; color: brown; font-weight: 600">Job Cards & Persons</span>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">SC:</span> &nbsp&nbsp <span id="scjobcard" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">ST:</span>&nbsp&nbsp  <span id="stjobcard" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>

                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">Others:</span>&nbsp&nbsp  <span id="otherjobcard" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>

                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">Men:</span>&nbsp&nbsp  <span id="jobcardMen" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>

                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">Women:</span>&nbsp&nbsp  <span id="jobcardWomen" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>

                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">Total:</span> &nbsp&nbsp   <span id="totaljobcard" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <span style="text-decoration: underline; font-size: xx-small; font-weight: 600; color: brown; font-weight: 600">Person Days</span>
                        </td>

                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">Person Days:</span> &nbsp&nbsp <span id="totalPersondays" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>

                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">SC Days:</span>&nbsp&nbsp<span id="scPersondays" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>

                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">ST Days:</span> &nbsp&nbsp <span id="stPersondays" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>

                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">Others Days:</span>  &nbsp&nbsp<span id="otherPersondays" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>

                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">Women Days:</span> &nbsp&nbsp<span id="womenPersondays" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>

                    </tr>

                    <tr>
                        <td colspan="2">
                            <span style="text-decoration: underline; font-size: xx-small; font-weight: 600; color: brown; font-weight: 600">Persons Employment Provided</span>
                        </td>

                    </tr>

                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">SC:</span>&nbsp&nbsp  <span id="scpersoncard" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>

                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">ST:</span>&nbsp&nbsp <span id="stpersoncard" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>

                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">Others:</span> &nbsp&nbsp <span id="otherpersoncard" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>

                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">Total:</span>&nbsp&nbsp <span id="totalpersoncard" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>

                    </tr>
                </table>
            </td>
            <td style="vertical-align: text-top; text-wrap: nowrap;">
                <table>
                    <tr>
                        <td colspan="2">
                            <span style="text-decoration: underline; font-size: xx-small; font-weight: 600; color: brown; font-weight: 600">Disabled Person</span>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">No. Person Registered</span>&nbsp&nbsp<span id="disabledjobcard" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span><br />
                            <span style="color: black; font-size: xx-small; font-weight: 600">No. Person Worked</span>&nbsp&nbsp<span id="disabledperswork" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span><br />
                            <span style="color: black; font-size: xx-small; font-weight: 600">No. Person days</span>&nbsp&nbsp<span id="disabledwork" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <span style="text-decoration: underline; font-size: xx-small; font-weight: 600; color: brown; font-weight: 600">E-MusterRoll</span>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">Issued:</span>&nbsp&nbsp<span id="issuedNmr" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">Filled:</span>&nbsp&nbsp <span id="filledNmr" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <span style="color: black; font-size: xx-small; font-weight: 600">Zero Attendence:</span>&nbsp&nbsp <span id="zeroattendence" style="color: cadetblue; font-size: xx-small; font-weight: 600"></span>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</asp:Content>
