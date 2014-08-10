<%@ Page Title="Flood Information WebGIS for Jadur Char" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeFile="JadurChar.aspx.cs" Inherits="Kulkandi" %>
<%@ Register TagPrefix="highcharts" Namespace="Highcharts.UI" Assembly="Highcharts" %>
<%@ Register TagPrefix="aspmap" Namespace="AspMap.Web" Assembly="AspMapNET" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <script type="text/javascript" src="Scripts/jquery-1.7.min.js"></script>
    <style type="text/css">
        .auto-style1 {
            width: 184px;
        }
        .auto-style3 {
            width: 200px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <ajaxToolkit:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server">
    </ajaxToolkit:ToolkitScriptManager>
    <table style="width:100%">
        <tr>
            <td style="vertical-align:top; width:200px; height:500px">
							<asp:ImageButton id="ImageButton1" runat="server" ImageUrl="tools/zoomfull.gif" BorderStyle="Outset"
								BorderWidth="1px" ToolTip="Zoom All" BorderColor="White" OnClick="zoomFull_Click"></asp:ImageButton>
							<aspmap:MapToolButton id="MapToolButton1" runat="server" ImageUrl="tools/zoomin.gif" Map="map" ToolTip="Zoom In"></aspmap:MapToolButton>
							<aspmap:MapToolButton id="MapToolButton2" runat="server" ImageUrl="tools/zoomout.gif" ToolTip="Zoom Out"
								Map="map" MapTool="ZoomOut"></aspmap:MapToolButton>
							<aspmap:MapToolButton id="MapToolButton3" runat="server" ImageUrl="tools/pan.gif" ToolTip="Pan" Map="map" MapTool="Pan"></aspmap:MapToolButton>
                <table class="auto-style1">
                    <tr>
                        <td style="vertical-align:top; " class="auto-style3">
                           <strong>Layer</strong>
                        </td>
                    </tr>
                     <tr>
                        <td>
                            <strong>Background Map</strong><br />
                            <asp:RadioButton id="RadioButton1" Text="Google Maps" TextAlign="Right" AutoPostBack="True" Checked="True" OnCheckedChanged="RadioButton1_CheckedChanged" GroupName="bgmap" runat="server" /><br>
                            <asp:RadioButton id="RadioButton2" Text="OpenStreeMap" TextAlign="Right" AutoPostBack="True"  Checked="false" OnCheckedChanged="RadioButton2_CheckedChanged" GroupName="bgmap" runat="server" /><br>
                            <strong>Inudation</strong><br />
                            <asp:RadioButtonList ID="innudationList" runat="server" AutoPostBack="True">
                            </asp:RadioButtonList>

						    <img src="WD.jpg" alt="ADB" style="height: 121px; width: 142px"/>

</td>
                    </tr>
                </table>
                            <br />
                            <asp:CheckBoxList ID="layerList" runat="server" AutoPostBack="True">
                            </asp:CheckBoxList>
            </td>
						<td style="vertical-align:top; width:600px; height:700px"">
                                 <aspmap:Map id="map" EnableSession="true" runat="server" Width="600px" 
                                         Height="700px" BackColor="#E6E6FA" ImageFormat="Gif" SmoothingMode="AntiAlias"
								         FontQuality="ClearType" MapTool="Pan" onmarkerclick="map_MarkerClick" style="margin-top: 0px"></aspmap:Map>
					         <asp:Label ID="status" runat="server"></asp:Label>
            </td>
            <td style="vertical-align:top; width:200px; height:500px">
               <aspmap:Legend id="legend" runat="server" Width="150px"  ImageFormat="Png" />
            </td>
        </tr>
    </table>
    <div>
        <asp:Panel ID="modal_div_chart" runat="server" Style="display: none" CssClass="modal-div">
            <asp:Panel ID="modal_div_drag_chart" runat="server" Height="20px" CssClass="modal-div-drag">
                <table style="width: 100%">
                    <tr>
                        <td align="center" valign="middle" style="padding-right: 5px; font-weight: bold">
                        </td>
                        <td style="padding-left: 5px;margin-top:-5px; width: 20px; text-align: right;">
                            <asp:LinkButton ID="modal_cancel_button_chart" CausesValidation="false" ForeColor="Black" OnClientClick="return false;" CssClass="modal-div-cancel-button" runat="server" Text="X" ToolTip="Cancel" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:Panel ID="modal_div_content_chart" runat="server" CssClass="modal-div-content">
                <asp:UpdatePanel ID="UpdatePanel11" runat="server">
                    <ContentTemplate>
                        <div>
                            <highcharts:LineChart ID="hcVendas" runat="server" Width="850" Height="350" />
                        </div>
                        
                        <div style="padding: 15px 10px 30px 10px; width:850px!important; text-align:center; display:table-cell; background-color:aliceblue">
                            <asp:Label ID="lblChartTable" runat="server"></asp:Label>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </asp:Panel>
        </asp:Panel>
        <asp:LinkButton ID="modal_target_button_chart" runat="server"></asp:LinkButton>
        <asp:LinkButton ID="modal_ok_button_chart" runat="server"></asp:LinkButton>
        <ajaxToolKit:ModalPopupExtender ID="modal_chart" PopupControlID="modal_div_chart" PopupDragHandleControlID="modal_div_drag_chart" CancelControlID="modal_cancel_button_chart" TargetControlID="modal_target_button_chart"
            OkControlID="modal_ok_button_chart" BackgroundCssClass="ModalBackground" RepositionMode="RepositionOnWindowResizeAndScroll" DropShadow="true" Drag="true" runat="server">
            <Animations>
                <OnShown>
                    <Fadein Duration="0.50" />
                </OnShown>
                <OnHiding>
                    <Fadeout Duration="0.75" />
                </OnHiding>
            </Animations>
        </ajaxToolKit:ModalPopupExtender>
    </div>
</asp:Content>
