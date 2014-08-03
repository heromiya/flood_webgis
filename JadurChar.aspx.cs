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


public partial class Kulkandi : System.Web.UI.Page
{
    protected void Page_Load(object sender, System.EventArgs e)
    {

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
            map.MapUnit = MeasureUnit.Degree;
            map.ScaleBar.Visible = true;
            map.ScaleBar.BarUnit = UnitSystem.Metric;
            //overviewMap.MapTool = MapTool.Point;
            //overviewMap.ClientScript = ClientScriptType.NoScript;

            //Add_WL_Stations_Markers();
            AddShapefile();
            FillLayerList();
            //AddOverviewMapLayers();
            
        }
           

        hcVendas.Exporting = new Exporting { enabled = true };

    }

    void AddOSMLayer()
    {
        OSMLayer osmLayer = new OSMLayer("http://a.tile.openstreetmap.org/{z}/{x}/{y}.png");

        map.BackgroundLayer = osmLayer;
    }

    void AddGoogleMapsLayer()
    {
        // You have to sign up for a Maps API key at http://code.google.com/apis/maps/signup.html.

        GoogleMapsLayer gml = new GoogleMapsLayer();

        gml.MapType = GoogleMapType.Normal;

        // When a GoogleMapsLayer object is set as a background layer: 1) the coordinate system of the
        // Map control will be set to PCS_PopularVisualisationMercator; 2) 20 zoom levels
        // from Google Maps will be added to the ZoomLevels collection of the Map control;
        // 3) the FullExtent property of the Map control will be set to the full extent of Google Maps.

        map.BackgroundLayer = gml;
    }

    protected void map_MarkerClick(object sender, MarkerClickEventArgs e)
    {
        modal_chart.Show();


        string val = map.Markers[e.MarkerIndex].Argument;

        string markerId = val.Substring(0, val.IndexOf("_"));
        val = val.Substring(val.IndexOf("_") + 1, val.Length - (val.IndexOf("_") + 1));
        string stationName = val;

        DataTable dtStation = GetDataTable("select * from wl_station where station_name='" + stationName + "'");
        double dangerLevel = 0.0;
        double recordLevel = 0.0;
        string riverName = string.Empty;
        if (dtStation != null && dtStation.Rows.Count > 0)
        {
            riverName = dtStation.Rows[0]["River_Name"].ToString();
            dangerLevel = Convert.ToDouble(dtStation.Rows[0]["Danger_Level"]);
            recordLevel = Convert.ToDouble(dtStation.Rows[0]["RHWL"]);
        }



        DateTime date = DateTime.Now;
        DateTime waterLevelDate = DateTime.Now;
        double waterLevel = 0.0;
        bool isFirstRow = true;
        DataTable table = LoadCSV(stationName);
        if (table != null)
        {
            if (table.Rows.Count > 0)
            {
                var pointObserved = new PointCollection();
                var pointForecast = new PointCollection();
                var pointRecorded = new PointCollection();
                var pointDanger = new PointCollection();

                foreach (DataRow dr in table.Rows)
                {
                    if (!string.IsNullOrEmpty(dr[1].ToString()) && !string.IsNullOrEmpty(dr[2].ToString()))
                    {
                        try
                        {
                            date = Convert.ToDateTime(dr[0]);
                            if (isFirstRow)
                            {
                                pointRecorded.Add(new Highcharts.Core.Point(Convert.ToInt64(date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds), recordLevel));
                                pointDanger.Add(new Highcharts.Core.Point(Convert.ToInt64(date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds), dangerLevel));

                            }
                            isFirstRow = false;

                            if (!(dr[1].ToString().Equals("-9999")))
                            {
                                pointObserved.Add(new Highcharts.Core.Point(Convert.ToInt64(date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds), Convert.ToDouble(dr[1])));
                                waterLevelDate = date;
                                waterLevel = Convert.ToDouble(dr[1]);
                            }
                            if (!(dr[2].ToString().Equals("-9999")))
                            {
                                pointForecast.Add(new Highcharts.Core.Point(Convert.ToInt64(date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds), Convert.ToDouble(dr[2])));
                            }

                        }
                        catch
                        {

                        }

                    }//end
                }//end foreach

                //Exporting cd = new Exporting();
                //cd.
                pointRecorded.Add(new Highcharts.Core.Point(Convert.ToInt64(date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds), recordLevel));
                pointDanger.Add(new Highcharts.Core.Point(Convert.ToInt64(date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds), dangerLevel));


                if (hcVendas.XAxis.Count > 0)
                    hcVendas.XAxis.Clear();
                if (hcVendas.YAxis.Count > 0)
                    hcVendas.YAxis.Clear();
                hcVendas.Title = new Title(stationName + " at " + riverName + " (Year " + DateTime.Now.Year.ToString() + " )");
                hcVendas.SubTitle = new SubTitle("As on " + waterLevelDate.ToString("dd-MMM"));
                //hcVendas.YAxis.Add(new YAxisItem { title = new Title("Water Level (mPWD)") });

                ToolTip tip = new ToolTip();
                // tooltip: {
                //valueSuffix: '(mPWD)',
                //shared: true,
                //crosshairs: true,
                //xDateFormat: '%d-%b-%Y %H:%M',
                //valueDecimals: 2

                //tip.shared.GetValueOrDefault(true);
                //tip.shared = true;
                tip.crosshairs = true;
                tip.formatter = "Highcharts.dateFormat('%d-%b-%Y %H:%M', this.x) +': '+ this.y +' (mPWD)'";

                hcVendas.Tooltip = tip;
                YAxisItem y = new YAxisItem();
                y.min = null;
                y.title = new Title("Water Level (mPWD)");
                y.minorGridLineWidth = 0;
                y.gridLineWidth = 1;

                PlotBands plotBands = new PlotBands();

                PlotBand plotBand = new PlotBand();
                plotBand.from = 0;
                plotBand.to = 5.5;
                plotBand.color = "#FFFFFF";
                plotBands.Add(plotBand);
                y.plotBands = plotBands;


                hcVendas.YAxis.Add(y);
                //YAxis y = new YAxis();
                //y.Min = null;
                //hcVendas.XAxis.Add(new XAxisItem { categories = new[] { "1994", "1995", "1996", "1997", "1998", "1999", "2000", "2001", "2002" } });
                XAxisItem xAxis = new XAxisItem();
                //xAxis.categories = xVal;
                xAxis.type = AxisDataType.datetime;
                //DateTimeLabelFormats dateLabelFormat =new DateTimeLabelFormats();
                //dateLabelFormat.day="%e - %b <br /> %H:%M";
                xAxis.dateTimeLabelFormats = new DateTimeLabelFormats { day = "%e - %b <br /> %H:%M" };
                Labels lbl = new Labels();
                lbl.align = Align.center;
                lbl.step = 1;
                xAxis.labels = lbl;
                hcVendas.XAxis.Add(xAxis);
                //New data collection

                if (hcVendas.Series.Count > 0)
                    hcVendas.Series.Clear();
                var series1 = new Collection<Serie>();

                if (series1.Count > 0)
                    series1.Clear();
                var s1 = new Serie { data = pointObserved.ToArray() };
                s1.name = "Observed Water Level";
                series1.Add(s1);

                var s2 = new Serie { data = pointForecast.ToArray() };
                s2.name = "Forecast Water Level";
                series1.Add(s2);

                series1.Add(new Serie { name = "Recorded Highest Water Level", data = pointRecorded.ToArray() });
                series1.Add(new Serie { name = "Danger Water Level", data = pointDanger.ToArray() });
                //bind 
                //
                PlotOptionsLine po = new PlotOptionsLine();
                SerieStates ss = new SerieStates();
                ss.hover = new SerieStateSettings { lineWidth = 2 };
                po.states = ss;
                po.lineWidth = 1;
                po.marker = new Highcharts.Core.Marker { radius = 2 };
                hcVendas.PlotOptions = po;
                hcVendas.DataSource = series1;
                hcVendas.DataBind();
            }

        }


        if (dtStation != null && dtStation.Rows.Count > 0)
        {

            Table tbl = new Table();
            tbl.CellPadding = 3;
            tbl.CssClass = "info-table";
            TableRow tr;
            TableCell tc;
            tr = new TableRow();
            tc = new TableCell();
            tc.CssClass = "info-table-td-head-center";
            tc.Text = "River Name";
            tc.RowSpan = 2;
            tr.Cells.Add(tc);
            tc = new TableCell();
            tc.CssClass = "info-table-td-head-center";
            tc.Text = "Station Name";
            tc.RowSpan = 2;
            tr.Cells.Add(tc);
            tc = new TableCell();
            tc.CssClass = "info-table-td-head-center";
            tc.Text = "RHWL (mPWD)";
            tc.RowSpan = 2;
            tr.Cells.Add(tc);
            tc = new TableCell();
            tc.CssClass = "info-table-td-head-center";
            tc.Text = "Danger Level (mPWD)";
            tc.RowSpan = 2;
            tr.Cells.Add(tc);
            //tc = new TableCell();
            //tc.CssClass = "info-table-td-head-center";
            //tc.Text = waterLevelDate.AddDays(-1).ToString("dd-MMM");
            //tr.Cells.Add(tc);
            tc = new TableCell();
            tc.CssClass = "info-table-td-head-center";
            tc.Text = waterLevelDate.ToString("dd-MMM");
            tr.Cells.Add(tc);
            tbl.Rows.Add(tr);

            tr = new TableRow();
            //tc = new TableCell();
            //tc.Text = "";
            //tc.ColumnSpan = 2;
            //tr.Cells.Add(tc);
            //tc = new TableCell();
            //tc.CssClass = "info-table-td-head-center";
            //tc.Text = waterLevelDate.AddDays(-1).ToString("hh:mm tt");
            //tr.Cells.Add(tc);
            tc = new TableCell();
            tc.CssClass = "info-table-td-head-center";
            tc.Text = waterLevelDate.ToString("hh:mm tt");
            tr.Cells.Add(tc);
            tbl.Rows.Add(tr);



            tr = new TableRow();
            tc = new TableCell();
            tc.CssClass = "info-table-td-center";
            tc.Text = riverName;
            tr.Cells.Add(tc);
            tc = new TableCell();
            tc.CssClass = "info-table-td-center";
            tc.Text = stationName;
            tr.Cells.Add(tc);
            tc = new TableCell();
            tc.CssClass = "info-table-td-center";
            tc.Text = recordLevel.ToString();
            tr.Cells.Add(tc);
            tc = new TableCell();
            tc.CssClass = "info-table-td-center";
            tc.Text = dangerLevel.ToString();
            tr.Cells.Add(tc);
            tc = new TableCell();
            tc.CssClass = "info-table-td-center";
            tc.Text = waterLevel.ToString();
            tr.Cells.Add(tc);
            //tc = new TableCell();
            //tc.CssClass = "info-table-td-center";
            //tc.Text = dangerLevel.ToString();
            //tr.Cells.Add(tc);
            tbl.Rows.Add(tr);

            StringWriter sw = new StringWriter();
            tbl.RenderControl(new HtmlTextWriter(sw));
            lblChartTable.Text = sw.ToString();
            //riverName = dtStation.Rows[0]["River_Name"].ToString();
            //dangerLevel = Convert.ToDouble(dtStation.Rows[0]["Danger_Level"]);
            //recordLevel = Convert.ToDouble(dtStation.Rows[0]["RHWL"]);
        }

        modal_chart.Show();
    }



    //protected void Add_WL_Stations_Markers()
    //{
    //    if (map.Markers.Count > 0)
    //        map.Markers.Clear();

    //    DataTable dt = GetDataTable("select * from WL_Station");
    //    if (dt != null)
    //    {
    //        StringBuilder sb;
    //        MarkerSymbol mSymbol = new MarkerSymbol("images/marker-normal.png", 12, 23);
    //        string wLType = string.Empty;
    //        double wlToday = 0.0;
    //        double wlDanger = 0.0;
    //        DateTime curDate = System.DateTime.Now;
    //        foreach (DataRow row in dt.Rows)
    //        {
    //            AspMap.Point pnt = new AspMap.Point(Convert.ToDouble(row["longitude"].ToString()), Convert.ToDouble(row["latitude"].ToString()));
    //            sb = new StringBuilder();
    //            sb.Append("");
    //            string cd = "<div class=\"marker_info\"><table cellpadding=\"0\" cellspacing=\"0\" class=\"marker_table\"><tr><td colspan=\"2\" class=\"marker_top_td\">Station Name: " + row["Station_Name"].ToString() + "</td></tr><tr><td class=\"marker_left_td\">Denger Level</td><td class=\"marker_right_td\">" + row["Danger_Level"].ToString() + "</td></tr><tr><td class=\"marker_left_td\">Yesterday Level</td><td class=\"marker_right_td\">" + row["WL_Yesterday"].ToString() + "</td></tr><tr><td class=\"marker_left_td\">Today Level</td><td class=\"marker_right_td\">" + row["WL_Today"].ToString() + "</td></tr><tr><td class=\"marker_left_td\">24 Hours Level</td><td class=\"marker_right_td\">" + row["WL_24Hrs"].ToString() + "</td></tr><tr><td class=\"marker_left_td\">48 Hours Level</td><td class=\"marker_right_td\">" + row["WL_48Hrs"].ToString() + "</td></tr><tr><td class=\"marker_left_td\">72 Hours Level</td><td class=\"marker_right_td\">" + row["WL_72Hrs"].ToString() + "</td></tr></table></div>";
    //            string cd = "<div class=\"marker_info\"><table cellpadding=\"0\" cellspacing=\"0\" class=\"marker_table\"><tr><td colspan=\"2\" class=\"marker_top_td\">Station Name: " + row["Station_Name"].ToString() + "</td></tr><tr><td class=\"marker_left_td\">Denger Level</td><td class=\"marker_right_td\">" + row["Danger_Level"].ToString() + "</td></tr><tr><td class=\"marker_left_td\">" + curDate.AddDays(-1).ToString("dd-MMM-yyyy") + "</td><td class=\"marker_right_td\">" + row["WL_Yesterday"].ToString() + "</td></tr><tr><td class=\"marker_left_td\">" + curDate.ToString("dd-MMM-yyyy") + "</td><td class=\"marker_right_td\">" + row["WL_Today"].ToString() + "</td></tr><tr><td class=\"marker_left_td\">" + curDate.AddDays(1).ToString("dd-MMM-yyyy") + "</td><td class=\"marker_right_td\">" + row["WL_24Hrs"].ToString() + "</td></tr><tr><td class=\"marker_left_td\">" + curDate.AddDays(2).ToString("dd-MMM-yyyy") + "</td><td class=\"marker_right_td\">" + row["WL_48Hrs"].ToString() + "</td></tr><tr><td class=\"marker_left_td\">" + curDate.AddDays(3).ToString("dd-MMM-yyyy") + "</td><td class=\"marker_right_td\">" + row["WL_72Hrs"].ToString() + "</td></tr></table></div>";

    //            wlToday = Convert.ToDouble(row["WL_Today"].ToString());
    //            wlDanger = Convert.ToDouble(row["Danger_Level"].ToString());

    //            if (wlToday < (wlDanger - 0.5))
    //                wLType = "normal";
    //            else if ((wlToday >= (wlDanger - 0.5)) && (wlToday < wlDanger))
    //                wLType = "warning";
    //            else if ((wlToday >= wlDanger) && (wlToday < wlDanger + 1.0))
    //                wLType = "danger";
    //            else if (wlToday >= wlDanger + 1.0)
    //                wLType = "severe";

    //            switch (wLType)
    //            {
    //                case "normal":
    //                    mSymbol = new MarkerSymbol("images/marker-normal.png", 12, 23);
    //                    break;
    //                case "warning":
    //                    mSymbol = new MarkerSymbol("images/marker-warning.png", 12, 23);
    //                    break;
    //                case "danger":
    //                    mSymbol = new MarkerSymbol("images/marker-danger.png", 12, 23);
    //                    break;
    //                case "severe":
    //                    mSymbol = new MarkerSymbol("images/marker-severe.png", 12, 23);
    //                    break;
    //                default:
    //                    break;
    //            }



    //            AspMap.Web.Marker marker = new AspMap.Web.Marker(pnt, mSymbol, cd);
    //            marker.Clickable = true;
    //            marker.Argument = row["Station_ID"].ToString() + "_" + row["Station_Name"].ToString();
    //            map.Markers.Add(marker);
    //        }
    //    }
    //    dt.Dispose();

    //}

    protected void Page_PreRender(object sender, System.EventArgs e)
    {
        //UpdateLayerProperties();
        UpdateLayerVisibility();
        //UpdateOverviewMapProperties();

        // set legend properties
        legend.AutoSize = true;
        legend.Add("Legend:"); // title
        legend.LegendFont.Name = "Arial";
        legend.LegendFont.Size = 16;
        legend.LegendFont.Bold = true;

        // populate the legend from the map layers collection
        // see also the Legend.Populate method
        for (int i = 0; i < map.LayerCount; i++)
        {
            if (map.IsLayerVisible(i))
            {
                AspMap.Layer layer = map[i];
                legend.Add(layer.Name, layer.LayerType, layer.Symbol);
            }
        }
    }


    //private void AddOverviewMapLayers()
    //{
    //    string LayerFolder = MapPath("MAPS/GEO/");

    //    AspMap.Layer layer = overviewMap.AddLayer(LayerFolder + "Area.shp");
    //    layer.Name = "Area";
    //    layer.Symbol.Size = 1;
    //    layer.Symbol.LineColor = Color.FromArgb(199, 172, 116);
    //    layer.Symbol.FillColor = Color.FromArgb(242, 236, 223);
    //}

    //private void UpdateOverviewMapProperties()
    //{
    //    overviewMap.ZoomFull();
    //    overviewMap.MapShapes.Clear();

    //    AspMap.Rectangle extent = map.Extent;

    //    // draw the extent of the map as a rectangle
    //    MapShape mapShape = overviewMap.MapShapes.Add(extent);
    //    mapShape.Symbol.Size = 2;
    //    mapShape.Symbol.LineColor = Color.Red;
    //    mapShape.Symbol.FillStyle = FillStyle.Invisible;

    //    // draw the extent of the map as a point (if it is too small to be displayed as a rectangle)
    //    mapShape = overviewMap.MapShapes.Add(new AspMap.Point(extent.Left, extent.Top));
    //    mapShape.Symbol.Size = 2;
    //    mapShape.Symbol.LineColor = Color.Red;
    //    mapShape.Symbol.PointStyle = PointStyle.Square;
    //}


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
    }


    //private void UpdateLayerProperties()
    //{
    //    AspMap.Layer Layer;
    //    AspMap.Feature feature;
    //    AspMap.FeatureRenderer renderer;

    //    //----------------------------------------------------
    //    Layer = map["Area"];

    //    Layer.Symbol.FillColor = Color.WhiteSmoke;
    //    //layer.LabelField = "STATE_ABBR";
    //    Layer.ShowLabels = true;
    //    Layer.LabelFont.Name = "Verdana";
    //    Layer.LabelFont.Size = 12;
    //    Layer.LabelFont.Bold = true;
    //    Layer.LabelStyle = LabelStyle.PolygonCenter;
    //    Layer.Description = "Surveyed Area";
    //    Layer.CoordinateSystem = CoordSystem.WGS1984;


    //    //----------------------------------------------------
    //    //Layer = map["Today"];
    //    //Layer.CoordinateSystem = CoordSystem.WGS1984;


    //    //Layer = map["park"];

    //    //Layer.LabelField = "NAME";
    //    //Layer.ShowLabels = true;
    //    //Layer.LabelFont.Outline = true;
    //    //Layer.LabelFont.Size = 11;
    //    //Layer.LabelFont.Bold = true;
    //    //Layer.Symbol.FillColor = Color.FromArgb(143, 175, 47);
    //    //Layer.Symbol.LineColor = Layer.Symbol.FillColor;

    //    //----------------------------------------------------
    //    //Layer = map["waterarea"];

    //    //Layer.LabelField = "NAME";
    //    //Layer.ShowLabels = true;
    //    //Layer.LabelFont.Outline = true;
    //    //Layer.LabelFont.Size = 12;
    //    //Layer.Symbol.FillColor = Color.FromArgb(159, 159, 223);
    //    //Layer.Symbol.LineColor = Layer.Symbol.FillColor;

    //    //----------------------------------------------------
    //    //Layer = map["water"];

    //    //Layer.LabelField = "NAME";
    //    //Layer.ShowLabels = true;
    //    //Layer.LabelFont.Size = 9;
    //    //Layer.Symbol.FillColor = Color.FromArgb(159, 159, 223);
    //    //Layer.Symbol.LineColor = Layer.Symbol.FillColor;
    //    //Layer.LabelFont.Color = Color.FromArgb(0, 0, 128);

    //    //----------------------------------------------------
    //    //Layer = map["airport"];

    //    //Layer.LabelField = "NAME";
    //    //Layer.ShowLabels = true;
    //    //Layer.LabelFont.Outline = true;
    //    //Layer.LabelFont.Size = 11;
    //    //Layer.Symbol.FillColor = Color.FromArgb(43, 147, 43);

    //    //----------------------------------------------------
    //    //Layer = map["street"];

    //    //Layer.LabelField = "NAME";
    //    //Layer.LabelFont.Size = 10;

    //    //Layer.Symbol.LineStyle = LineStyle.Road;
    //    //Layer.Symbol.LineColor = Color.FromArgb(171, 158, 137);
    //    //Layer.Symbol.InnerColor = Color.White;

    //    //if (map.MapScale >= 75000)
    //    //    Layer.Symbol.Size = 3;
    //    //else if (map.MapScale >= 37000)
    //    //{
    //    //    Layer.Symbol.Size = 4;
    //    //    Layer.ShowLabels = true;
    //    //    Layer.LabelFont.Outline = true;
    //    //}
    //    //else
    //    //{
    //    //    Layer.Symbol.Size = 6;
    //    //    Layer.ShowLabels = true;
    //    //    Layer.LabelFont.Outline = true;
    //    //}

    //    //----------------------------------------------------
    //    //Layer = map["railroad"];

    //    //Layer.LabelField = "NAME";
    //    //Layer.ShowLabels = true;
    //    //Layer.LabelFont.Outline = true;
    //    //Layer.LabelFont.Size = 10;
    //    //Layer.Symbol.LineStyle = LineStyle.Railroad;

    //    //----------------------------------------------------
    //    //Layer = map["institution"];

    //    //Layer.LabelField = "NAME";
    //    //Layer.ShowLabels = true;
    //    //Layer.LabelFont.Name = "Times New Roman";
    //    //Layer.LabelFont.Outline = true;
    //    //Layer.LabelFont.Size = 12;
    //    //Layer.UseDefaultSymbol = false;

    //    //renderer = Layer.Renderer;
    //    //renderer.Field = "FCC";

    //    // cemetery symbol
    //    //feature = renderer.Add();
    //    //feature.Value = "D82";
    //    //feature.Symbol.PointStyle = PointStyle.Bitmap;
    //    //feature.Symbol.Bitmap = MapPath("symbols/cemetery.bmp");
    //    //feature.Symbol.Size = 16;
    //    //feature.Symbol.TransparentColor = Color.White;
    //    //feature.Description = "Cemetery";

    //    // school symbol
    //    //feature = renderer.Add();
    //    //feature.Value = "D43";
    //    //feature.Symbol.PointStyle = PointStyle.Bitmap;
    //    //feature.Symbol.Bitmap = MapPath("symbols/school.bmp");
    //    //feature.Symbol.Size = 16;
    //    //feature.Symbol.TransparentColor = Color.White;
    //    //feature.Description = "School";

    //    // church symbol
    //    //feature = renderer.Add();
    //    //feature.Value = "D44";
    //    //feature.Symbol.PointStyle = PointStyle.Bitmap;
    //    //feature.Symbol.Bitmap = MapPath("symbols/church.bmp");
    //    //feature.Symbol.Size = 16;
    //    //feature.Symbol.TransparentColor = Color.White;
    //    //feature.Description = "Church";

    //    // hospital symbol
    //    //feature = renderer.Add();
    //    //feature.Value = "D31";
    //    //feature.Symbol.PointStyle = PointStyle.Bitmap;
    //    //feature.Symbol.Bitmap = MapPath("symbols/hospital.bmp");
    //    //feature.Symbol.Size = 16;
    //    //feature.Symbol.TransparentColor = Color.White;
    //    //feature.Description = "Hospital";
    //}



    protected void zoomFull_Click(object sender, System.Web.UI.ImageClickEventArgs e)
    {
        map.ZoomFull();
    }


    //protected void overviewMap_PointTool(object sender, PointToolEventArgs e)
    //{
    //    AspMap.Rectangle extent = map.Extent;
    //    extent.Offset(e.Point.X - extent.Center.X, e.Point.Y - extent.Center.Y);
    //    map.Extent = extent;
    //}


    void AddShapefile()
    {
        AspMap.Layer layer;

        layer = map.AddLayer(MapPath("InundationMap/Today Inundation Map.jpg"));
        layer.Description = "Inundation Map Today";
        layer.CoordinateSystem = CoordSystem.WGS1984;
        layer.Visible = false;

        layer = map.AddLayer(MapPath("InundationMap/1Day Inundation Map.jpg"));
        layer.Description = "Inundation Map 1 Today";
        layer.CoordinateSystem = CoordSystem.WGS1984;
        layer.Visible = false;

        layer = map.AddLayer(MapPath("InundationMap/2Day Inundation Map.jpg"));
        layer.Description = "Inundation Map 2 Today";
        layer.CoordinateSystem = CoordSystem.WGS1984;
        layer.Visible = false;

        layer = map.AddLayer(MapPath("InundationMap/3Day Inundation Map.jpg"));
        layer.Description = "Inundation Map 3 Today";
        layer.CoordinateSystem = CoordSystem.WGS1984;
        layer.Visible = false;

        layer = map.AddLayer(MapPath("InundationMap/4Day Inundation Map.jpg"));
        layer.Description = "Inundation Map 4 Today";
        layer.CoordinateSystem = CoordSystem.WGS1984;
        layer.Visible = false;

        layer = map.AddLayer(MapPath("InundationMap/5Day Inundation Map.jpg"));
        layer.Description = "Inundation Map 5 Today";
        layer.CoordinateSystem = CoordSystem.WGS1984;
        layer.Visible = false;

        layer = map.AddLayer(MapPath("MAPS/GEO/Area.shp"));
        layer.Symbol.FillColor = Color.WhiteSmoke;
        //layer.LabelField = "STATE_ABBR";
        layer.ShowLabels = true;
        layer.LabelFont.Name = "Verdana";
        layer.LabelFont.Size = 12;
        layer.LabelFont.Bold = true;
        layer.LabelStyle = LabelStyle.PolygonCenter;
        layer.Description = "Surveyed Area";

        // The coordinate system of the shapefile must be set explicitly or must
        // be specified in a .prj file.
        //layer.CoordinateSystem = CoordSystem.WGS1984;

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

        // The coordinate system of the shapefile must be set explicitly or must
        // be specified in a .prj file.
        //layer.CoordinateSystem = CoordSystem.WGS1984;   


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



    public DataTable LoadCSV(string fileName)
    {
        DataTable dt = new DataTable("csv");
        try
        {
            // Creates and opens an ODBC connection
            string strConnString = "Provider=Microsoft.Jet.OleDb.4.0; Data Source = " + Server.MapPath("Model_data") + "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\";Persist Security Info=False";

            OleDbConnection conn = new OleDbConnection(strConnString.Trim());
            string sql = string.Empty;


            sql = "select * from [" + fileName + ".csv]";
            conn.Open();
            //Creates the data adapter
            OleDbDataAdapter da = new OleDbDataAdapter(sql, conn);

            //Fills dataset with the records from CSV file
            da.Fill(dt);
            da.Dispose();
            //closes the connection
            conn.Close();
            conn.Dispose();
        }
        catch (Exception e) //Error
        {
        }
        return dt;
    }
    public void AddChart(string fileName)
    {


        //LoadCSV(100);

        //string strFileName = "D://FXDEAL.csv";
        //OleDbConnection conn = new OleDbConnection("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " + System.IO.Path.GetDirectoryName(strFileName) + "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\"");
        OleDbConnection conn = new OleDbConnection("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " + Server.MapPath("Model_data") + "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\"");

        conn.Open();
        OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT * FROM [Aricha.csv]", conn);
        DataSet ds = new DataSet("Temp");
        adapter.Fill(ds);
        DataTable tb = ds.Tables[0];

        string cd = Server.MapPath("Model_data") + "\\Aricha.csv";
        //string conStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Server.MapPath("Model_data") + ";Extended Properties='text;HDR=Yes;IMEX=1;FMT=Delimited(,)';";
        //OleDbDataAdapter da = new OleDbDataAdapter("select * from Aricha", conStr);
        //DataTable dt = new DataTable();
        //try
        //{
        //    da.Fill(dt);
        //}
        //catch (Exception ex)
        //{
        //}

    }

    public DataTable GetDataTable(string sql)
    {
        OleDbDataAdapter da = new OleDbDataAdapter(sql, "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\\ffwc.mdb;");
        DataTable dt = new DataTable();
        try
        {
            da.Fill(dt);
        }
        catch
        {
            dt = null;
        }
        finally
        {
            da.Dispose();
        }
        return dt;
    }

    public string GetFieldValue(string sql)
    {
        string retVal = string.Empty;
        OleDbConnection cnn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\\ffwc.mdb;");

        OleDbCommand cmd = new OleDbCommand();
        try
        {
            cnn.Open();
            cmd.Connection = cnn;
            cmd.CommandText = sql;
            retVal = cmd.ExecuteScalar().ToString();

        }
        catch
        {
            retVal = string.Empty;
        }
        finally
        {
            cmd.Dispose();
            cnn.Close();
            cnn.Dispose();
        }


        return retVal;
    }


    protected void CheckBox1_CheckedChanged(object sender, EventArgs e)
    {
        if (CheckBox1.Checked)
        {

            AddGoogleMapsLayer();
            map.ImageFormat = ImageFormat.Png;
            map.ImageOpacity = 0.5;
            map.CenterAt(map.CoordinateSystem.FromWgs84(89.82, 25.51));
            map.ZoomLevel = 13;


        }
        else
        {
            map.BackgroundLayer = null;

        }
        //if (!IsPostBack)
        //{

        //    map.CenterAt(map.CoordinateSystem.FromWgs84(89.75, 25.12));
        //}
    }

    protected void CheckBox2_CheckedChanged(object sender, EventArgs e)
    {
        if (CheckBox2.Checked)
        {

            AddOSMLayer();
            map.ImageFormat = ImageFormat.Png;
            map.ImageOpacity = 0.5;
            map.CenterAt(map.CoordinateSystem.FromWgs84(89.82, 25.51));
            map.ZoomLevel = 13;


        }
        else
        {
            map.BackgroundLayer = null;

        }
       
    }

    //protected void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
    //{
    //    AspMap.Recordset rs = map["Union"].SearchExpression("UNINAME=\"" + ListBox1.SelectedValue + "\"");

    //    if (!rs.EOF)
    //    {            
    //        // zoom to the feature
    //        map.Extent = rs.RecordExtent;

    //        // highlight the feature
    //        map.MapShapes.Clear();
    //        MapShape ms = map.MapShapes.Add(rs.Shape);
    //        ms.Symbol.FillColor = Color.Yellow;
    //        ms.Symbol.Size = 2;
    //    }

    //    if (map.CoordinateSystem == null)
    //        map.Extent = rs.RecordExtent;
    //    else
    //        map.Extent = map.CoordinateSystem.FromWgs84(rs.RecordExtent);
    //}


}

