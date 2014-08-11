using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.ComponentModel;
using System.Data;
using AjaxControlToolkit;
using System.Data.OleDb;
using cd = NGChart;
using System.Drawing;
using System.Web.SessionState;
using System.Web.UI.HtmlControls;
using AspMap;
using AspMap.Web;
using AspMap.Web.Extensions;
using System.Data.Odbc;
using System.Text;
using System.IO;

using Highcharts;
using Highcharts.Core;
using Highcharts.Core.PlotOptions;
using Highcharts.Core.Appearance;
using Highcharts.Core.Data.Chart;
using Highcharts.UI;
using Highcharts.Core.Options;

using System.Collections.ObjectModel;

public partial class App
{
    static public GoogleMapsLayer gml = new GoogleMapsLayer();
    static public OSMLayer osmLayer = new OSMLayer("http://a.tile.openstreetmap.org/{z}/{x}/{y}.png");
    static public AspMap.Point center = new AspMap.Point();
    static public AspMap.Rectangle extent = new AspMap.Rectangle();
    static public AspMap.Layer[] inundation = new AspMap.Layer[7];
    static public AspMap.Rectangle fullextent = new AspMap.Rectangle();
}

public partial class Kulkandi : System.Web.UI.Page
{
    protected void Page_Load(object sender, System.EventArgs e)
    {
        for (int i = 0; i < 7; i++)
        {
            App.inundation[i] = new AspMap.Layer();
        }
        if (!IsPostBack)
        {
            if (map.LayerCount > 0)
                map.RemoveAllLayers();
            if (map.BackgroundLayer != null)
                map.BackgroundLayer = null;
            if (map.Hotspots.Count > 0)
                map.Hotspots.Clear();
            if (map.MapShapes.Count > 0)
                map.MapShapes.Clear();
            map.MapUnit = MeasureUnit.Meter;
            map.ScaleBar.Visible = true;
            map.ScaleBar.BarUnit = UnitSystem.Metric;
            map.CoordinateSystem = new AspMap.CoordSystem(CoordSystemCode.PCS_PopularVisualisationMercator);

            App.gml.MapType = GoogleMapType.Normal;
            App.gml.Visible = true;
            map.BackgroundLayer = App.gml;
            map.ImageFormat = ImageFormat.Png;
            map.ImageOpacity = 0.75;

            AddShapefile();

            App.fullextent.Bottom = 2929924.996;
            App.fullextent.Top = 2950432.713;
            App.fullextent.Left = 9994310.118;
            App.fullextent.Right = 10004636.304;
            map.FullExtent = App.fullextent;
            map.Extent = App.fullextent;
            FillLayerList();
        }
    }

    protected void Page_PreRender(object sender, System.EventArgs e)
    {
        UpdateLayerVisibility();
    }

    private void FillLayerList()
    {
        if (IsPostBack) return;

        foreach (AspMap.Layer layer in map)
        {
            ListItem item = new ListItem(layer.Description, layer.Name);
            item.Selected = layer.Visible;
            layerList.Items.Add(item);
        }
    }

    private void UpdateLayerVisibility()
    {
        if (!IsPostBack) return;

        foreach (ListItem item in layerList.Items)
        {
            map[item.Value].Visible = item.Selected;
        }
        foreach (ListItem item in innudationList.Items)
        {
            if(item.Text != "Disable inudation map"){
                map[item.Value].Visible = item.Selected;
            }
        }
    }

    protected void zoomFull_Click(object sender, System.Web.UI.ImageClickEventArgs e)
    {
        map.Extent = App.fullextent;
    }

    void AddShapefile()
    {
        AspMap.Layer layer;
        ListItem item;
        for (int i = 0; i < 6; i++)
        {
            App.inundation[i] = map.AddLayer(MapPath("InundationMap/inundation_day" + i + ".tif"));
            App.inundation[i].Description = "Inundation Day " + i;
            App.inundation[i].CoordinateSystem = new AspMap.CoordSystem(CoordSystemCode.PCS_PopularVisualisationMercator);
            App.inundation[i].Visible = false;
            item = new ListItem(App.inundation[i].Description, App.inundation[i].Name);
            item.Selected = App.inundation[i].Visible;
            innudationList.Items.Add(item);
        }
        App.inundation[6] = map.AddLayer(MapPath("InundationMap/null.tif"));
        App.inundation[6].Description = "Disable inudation map";
        App.inundation[6].CoordinateSystem = new AspMap.CoordSystem(CoordSystemCode.PCS_PopularVisualisationMercator);
        App.inundation[6].Visible = false;
        App.inundation[6].Opacity = 0;
        item = new ListItem(App.inundation[6].Description, App.inundation[6].Name);
        item.Selected = App.inundation[6].Visible;
        innudationList.Items.Add(item);

        layer = map.AddLayer(MapPath("MAPS/GEO/Homestead.shp"));
        layer.Symbol.FillColor = Color.FromArgb(178, 178, 178);
        layer.Symbol.LineColor = Color.FromArgb(178, 178, 178);
        //layer.LabelField = "STATE_ABBR";
        layer.ShowLabels = true;
        layer.LabelFont.Name = "Verdana";
        layer.LabelFont.Size = 12;
        layer.LabelFont.Bold = true;
        layer.LabelStyle = LabelStyle.PolygonCenter;
        layer.Description = "Home Stead";

        layer = map.AddLayer(MapPath("MAPS/GEO/River_Khal.shp"));
        layer.Symbol.LineColor = Color.FromArgb(64, 176, 235);
        layer.Symbol.Size = 2;
        layer.LabelField = "Name";
        layer.ShowLabels = true;
        layer.LabelFont.Outline = true;
        layer.LabelFont.Size = 10;
        layer.LabelFont.Color = Color.Blue;
        layer.Symbol.LineStyle = LineStyle.Solid;
        layer.Description = "River/Khal";

        layer = map.AddLayer(MapPath("MAPS/GEO/Main_Road.shp"));
        layer.Symbol.LineStyle = LineStyle.Road;
        layer.Symbol.LineColor = Color.FromArgb(255, 0, 0);
        layer.Symbol.Size = 2;
        layer.LabelField = "ROADNAME";
        layer.ShowLabels = true;
        layer.LabelFont.Outline = true;
        layer.LabelFont.Size = 10;
        layer.LabelFont.Color = Color.Red;
        layer.Symbol.LineStyle = LineStyle.Solid;
        layer.Description = "Main_Road";

        layer = map.AddLayer(MapPath("MAPS/GEO/Local_Road.shp"));
        layer.Symbol.LineStyle = LineStyle.Road;
        layer.Symbol.LineColor = Color.FromArgb(255, 127, 127);
        layer.Symbol.Size = 1;
        layer.LabelField = "ROADNAME";
        layer.ShowLabels = true;
        layer.LabelFont.Outline = true;
        layer.LabelFont.Size = 10;
        layer.LabelFont.Color = Color.Red;
        layer.Symbol.LineStyle = LineStyle.Solid;
        layer.Description = "Local_Road";

        layer = map.AddLayer(MapPath("MAPS/GEO/Bazar.shp"));
        layer.LabelField = "NAME";
        layer.ShowLabels = true;
        layer.LabelFont.Name = "Times New Roman";
        layer.LabelFont.Outline = true;
        layer.LabelFont.Size = 12;
        layer.Symbol.PointStyle = PointStyle.PlaceTown;
        layer.Symbol.Size = 15;
        layer.Description = "Market Place";

        layer = map.AddLayer(MapPath("MAPS/GEO/Clinic.shp"));
        layer.LabelField = "NAME";
        layer.ShowLabels = true;
        layer.LabelFont.Name = "Times New Roman";
        layer.LabelFont.Outline = true;
        layer.LabelFont.Size = 12;
        layer.Symbol.PointStyle = PointStyle.Hospital;
        layer.Symbol.Size = 15;
        layer.Description = "Clinic/Hospital";

        layer = map.AddLayer(MapPath("MAPS/GEO/College.shp"));
        layer.LabelField = "NAME";
        layer.ShowLabels = true;
        layer.LabelFont.Name = "Times New Roman";
        layer.LabelFont.Outline = true;
        layer.LabelFont.Size = 12;
        layer.Symbol.PointStyle = PointStyle.University;
        layer.Symbol.Size = 15;
        layer.Description = "College";

        layer = map.AddLayer(MapPath("MAPS/GEO/Mosque.shp"));
        layer.LabelField = "NAME";
        layer.ShowLabels = true;
        layer.LabelFont.Name = "Times New Roman";
        layer.LabelFont.Outline = true;
        layer.LabelFont.Size = 12;
        layer.Symbol.PointStyle = PointStyle.Monument;
        layer.Symbol.Size = 15;
        layer.Description = "Mosque";


        layer = map.AddLayer(MapPath("MAPS/GEO/School.shp"));
        layer.LabelField = "NAME";
        layer.ShowLabels = true;
        layer.LabelFont.Name = "Times New Roman";
        layer.LabelFont.Outline = true;
        layer.LabelFont.Size = 12;
        layer.Symbol.PointStyle = PointStyle.School;
        layer.Symbol.Size = 15;
        layer.Description = "School";

        layer = map.AddLayer(MapPath("MAPS/GEO/Madrasha.shp"));
        layer.LabelField = "NAME";
        layer.ShowLabels = true;
        layer.LabelFont.Name = "Times New Roman";
        layer.LabelFont.Outline = true;
        layer.LabelFont.Size = 12;
        layer.Symbol.PointStyle = PointStyle.School;
        layer.Symbol.Size = 15;
        layer.Description = "Madrasha";


        layer = map.AddLayer(MapPath("MAPS/GEO/Post_Office.shp"));
        layer.LabelField = "NAME";
        layer.ShowLabels = true;
        layer.LabelFont.Name = "Times New Roman";
        layer.LabelFont.Outline = true;
        layer.LabelFont.Size = 12;
        layer.Symbol.PointStyle = PointStyle.PostOffice;
        layer.Symbol.Size = 15;
        layer.Description = "Post Office";

        layer = map.AddLayer(MapPath("MAPS/GEO/Union_Office.shp"));
        layer.LabelField = "NAME";
        layer.ShowLabels = true;
        layer.LabelFont.Name = "Times New Roman";
        layer.LabelFont.Outline = true;
        layer.LabelFont.Size = 12;
        layer.Symbol.PointStyle = PointStyle.PostOffice;
        layer.Symbol.Size = 15;
        layer.Description = "Union Office";

        layer = map.AddLayer(MapPath("MAPS/GEO/Upazila_Office.shp"));
        layer.LabelField = "NAME";
        layer.ShowLabels = true;
        layer.LabelFont.Name = "Times New Roman";
        layer.LabelFont.Outline = true;
        layer.LabelFont.Size = 12;
        layer.Symbol.PointStyle = PointStyle.PostOffice;
        layer.Symbol.Size = 15;
        layer.Description = "Upazila Office";



        layer = map.AddLayer(MapPath("MAPS/GEO/Place_Name.shp"));
        //layer.MaxScale = 500000;
        layer.Symbol.Size = 1;
        layer.LabelField = "Name";
        layer.ShowLabels = true;
        layer.LabelFont.Outline = true;
        layer.LabelFont.Size = 15;
        layer.LabelFont.Color = Color.Black;
        layer.Symbol.LineStyle = LineStyle.Solid;
        layer.Description = "Place Name";

        layer = map.AddLayer(MapPath("MAPS/GEO/Union Name.shp"));
        //layer.MaxScale = 500000;
        layer.Symbol.Size = 5;
        layer.LabelField = "UNINAME";
        layer.ShowLabels = true;
        layer.LabelFont.Outline = true;
        layer.LabelFont.Size = 12;
        layer.LabelFont.Color = Color.Black;
        layer.Symbol.LineStyle = LineStyle.Solid;
        layer.Description = "Union Name";

        layer = map.AddLayer(MapPath("MAPS/GEO/Union Boundary.shp"));
        //layer.Symbol.FillColor = Color.FromArgb(178, 178, 178);
        layer.Symbol.LineColor = Color.FromArgb(0, 0, 0);
        layer.Symbol.Size = 2;
        //layer.LabelField = "UNINAME";
        layer.ShowLabels = true;
        layer.LabelFont.Name = "Verdana";
        layer.LabelFont.Size = 12;
        layer.LabelFont.Bold = true;
        layer.LabelStyle = LabelStyle.PolygonCenter;
        layer.Description = "Union Boundary";
        layer.CoordinateSystem = CoordSystem.WGS1984;
        // The coordinate system of the shapefile must be set explicitly or must
        // be specified in a .prj file.
        //layer.CoordinateSystem = CoordSystem.WGS1984;

    }

    protected void RadioButton1_CheckedChanged(object sender, EventArgs e)
    {
        if (RadioButton1.Checked)
        {
            App.extent = map.Extent;
            App.gml.MapType = GoogleMapType.Normal;
            App.gml.Visible = true;
            App.osmLayer.Visible = false;
            map.BackgroundLayer = App.gml;
            map.Extent = App.extent;
        }
    }

    protected void RadioButton2_CheckedChanged(object sender, EventArgs e)
    {
        if (RadioButton2.Checked)
        {
            App.extent = map.Extent;
            map.BackgroundLayer = App.osmLayer;
            App.gml.Visible = false;
            App.osmLayer.Visible = true;
            map.Extent = App.extent;
        }
    }

}
