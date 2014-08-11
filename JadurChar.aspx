<%@ Page Title="Flood Information WebGIS for Jadur Char" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeFile="JadurChar.aspx.cs" Inherits="Kulkandi" %>
<%@ Register TagPrefix="highcharts" Namespace="Highcharts.UI" Assembly="Highcharts" %>
<%@ Register TagPrefix="aspmap" Namespace="AspMap.Web" Assembly="AspMapNET" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <script type="text/javascript" src="Scripts/jquery-1.7.min.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <ajaxToolkit:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server">
    </ajaxToolkit:ToolkitScriptManager>
    <div style="float:left">
        <asp:UpdatePanel ID="UpdatePanel2" runat="server">
            <ContentTemplate>
			    <asp:ImageButton id="ImageButton1" runat="server" ImageUrl="tools/zoomfull.gif" BorderStyle="Outset" BorderWidth="1px" ToolTip="Zoom All" BorderColor="White" OnClick="zoomFull_Click"></asp:ImageButton>
			    <aspmap:MapToolButton id="MapToolButton1" runat="server" ImageUrl="tools/zoomin.gif" Map="map" ToolTip="Zoom In"></aspmap:MapToolButton>
			    <aspmap:MapToolButton id="MapToolButton2" runat="server" ImageUrl="tools/zoomout.gif" ToolTip="Zoom Out" Map="map" MapTool="ZoomOut"></aspmap:MapToolButton>
			    <aspmap:MapToolButton id="MapToolButton3" runat="server" ImageUrl="tools/pan.gif" ToolTip="Pan" Map="map" MapTool="Pan"></aspmap:MapToolButton>
            </ContentTemplate>
        </asp:UpdatePanel>
        <strong>Background Map</strong><br />
        <asp:RadioButton id="RadioButton1" Text="Google Maps" TextAlign="Right" AutoPostBack="True" Checked="True" OnCheckedChanged="RadioButton1_CheckedChanged" GroupName="bgmap" runat="server" /><br>
        <asp:RadioButton id="RadioButton2" Text="OpenStreeMap" TextAlign="Right" AutoPostBack="True"  Checked="false" OnCheckedChanged="RadioButton2_CheckedChanged" GroupName="bgmap" runat="server" /><br>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <strong>Layer</strong>
                <strong>Inudation</strong><br />
                <asp:RadioButtonList ID="innudationList" runat="server" AutoPostBack="True">
                </asp:RadioButtonList>
		        <img src="WD.jpg" alt="Legend of water depth" style="height: 121px; width: 142px"/>
                <asp:CheckBoxList ID="layerList" runat="server" AutoPostBack="True">
                </asp:CheckBoxList>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <div style="float:right">
        <asp:UpdatePanel ID="UpdatePanel3" runat="server">
            <ContentTemplate>
                <aspmap:Map id="map" EnableSession="true" runat="server" Width="100%" 
                        Height="700px" BackColor="#E6E6FA" ImageFormat="Gif" SmoothingMode="AntiAlias"
					    FontQuality="ClearType" MapTool="Pan" style="margin-top: 0px"></aspmap:Map>
			    <asp:Label ID="status" runat="server"></asp:Label>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
