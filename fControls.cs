using Wew.Control;
using Wew.Media;

namespace Demo;

class fControls : cDockControl
{
public fControls()
{	Text = "Controls";
	cWrapPanel wrp = new () {	LocationMargin = new Point(10, 10), Size = new Point(900, 100)};
		cLabel lbl = new () {	Text = "Label"};
			wrp.AddControl(lbl);
		cLinkLabel lnk = new () {	Text = "Download", Target = "http://aa.com/bb.txt"};
			lnk.LinkClicked += lnk_LinkClicked;
			wrp.AddControl(lnk);
		cTextBox txt = new () {	Text = "Text boxA\u0308"};
			wrp.AddControl(txt);
		cNumericTextBox ntx = new () {	Type = eNumberType.Floating, FloatValue = 200.45f};
			wrp.AddControl(ntx);
		cDateControl dtc = new () {	Value = new System.DateTime(1980, 11, 22)
				, Minimum = new System.DateTime(2017, 7, 3), Maximum = new System.DateTime(2018, 4, 26)};
			wrp.AddControl(dtc);
		cTimeControl tct = new () {	Value = new System.TimeSpan(1, 20, 35)};
			wrp.AddControl(tct);
		cComboBox cbo = new ();
			cbo.Load(new string[] {	"Item 1", "Item 2", "Item 3"});
			cbo.SelectedIndex = 1;
			wrp.AddControl(cbo);
		cSeparator sep = new ();
			wrp.AddControl(sep);
		cButton btn = new () {	Text = "Button", Type = eButtonType.Split};
			wrp.AddControl(btn);
		cCheckBox chk = new () {	Text = "Check box"};
			wrp.AddControl(chk);
		cRadioButton rbt1 = new () {	Text = "Radio button 1"};
			wrp.AddControl(rbt1);
		cRadioButton rbt2 = new () {	Text = "Radio button 2"};
			wrp.AddControl(rbt2);
		cScrollBar sbr = new () {	Width = 100, LargeChange = 300, Value = 10};
			wrp.AddControl(sbr);
		cSlider sli = new () {	Width = 100, Value = 80, TickFrequency = 40, SnapToTick = true, AutoToolTip = true};
			wrp.AddControl(sli);
		cProgressBar pb = new () {	Width = 150, Value = 80};
			wrp.AddControl(pb);
		cPicture pic = new () {	Size = new Point(30, 30), Bitmap = mRes.BmpSmile, ToolTip = "Picture"};
			wrp.AddControl(pic);
		cGeometryControl geo = new () {	Size = new Point(30, 30), ToolTip = "Geometry"
				, Geometry = new cPathGeometry(new Point[] {	new Point(0, 10), new Point(5, 0), new Point(10, 10)}, true)
				, FillColor = eBrush.Orange};
			wrp.AddControl(geo);
		cColorButton cbt = new () {	Color = eColor.LightSkyBlue, ToolTip = "Color"};
			wrp.AddControl(cbt);
		AddControl(wrp);
	cResizableBorder rzb = new () {	Control = wrp};
		AddControl(rzb);
	cScrollableControl scc = new () {	Margins = new Rect(10, 120, 50, 10), ClientEndMargin = new Point(10, 10)};
		cGrid grd = new () {	LocationMargin = new Point(10, 10), Size = new Point(250, 100)
				, ColumnCount = 4, RowCount = 5};
			grd.Columns(0).Visible = false;
			scc.ClientArea.AddControl(grd);
		cTabControl tac = new () {	LocationMargin = new Point(290, 10), Size = new Point(100, 100)};
			_ = tac.Tabs.Add("Tab 1");
			_ = tac.Tabs.Add("Tab 2");
			scc.ClientArea.AddControl(tac);
		cBorderControl brd = new () {	Bounds = new Rectangle(420, 0, 140, 104) 
				, BorderColor = eBrush.Blue, Border = new Rect(3)
				, LineStyle = new cLineStyle(eDash.DashDot, eCapStyle.Round, eLineJoin.Round)};
			scc.ClientArea.AddControl(brd);
		cListBox lbx = new () {	LocationMargin = new Point(430, 10)};
			lbx.Load(new string[] {	"Item A", "Item B", "Item C"}); lbx.SelectedIndex = 2;
			scc.ClientArea.AddControl(lbx);
		cStackPanel stk = new () {	LocationMargin = new Point(570, 10), Size = new Point(100, 100)
					, BorderStyle = eBorderStyle.Default, Separation = 0};
			cButton btn3 = new () {	Width = 20, Text = "Button 3"};
				stk.AddControl(btn3);
			cSplitter spl = new () {	Vertical = true, Height = 100, ToolTip = "Splitter"};
				spl.Dragging += (object sender, Point offset) => btn3.Width += offset.X;
				stk.AddControl(spl);
			cButton btn4 = new () {	Width = 50, Text = "Button 4"};
				stk.AddControl(btn4);
			scc.ClientArea.AddControl(stk);
		cEditControl edt = new () {	Bounds = new Rectangle(710, 10, 250, 100)
				, Text = "Edit control\na\ns\nd\nf\ng"};
			scc.ClientArea.AddControl(edt);
		cPlayer ply = new () {	LocationMargin = new Point(10, 120), Size = new Point(250, 100)};
			scc.ClientArea.AddControl(ply);
		cPaintControl pai = new () {	LocationMargin = new Point(290, 120), Size = new Point(100, 100), ToolTip = "Paint"
				, BorderStyle = eBorderStyle.Default};
			pai.Paint += pai_Paint;
			scc.ClientArea.AddControl(pai);
		cRenderControl rnc = new () {	LocationMargin = new Point(430, 120), Size = new Point(100, 100), ToolTip = "Render"
				, Model = new cPlane(wMain.Device), BorderStyle = eBorderStyle.Default};
			rnc.Camera.MoveZ(-10);
			scc.ClientArea.AddControl(rnc);
		cMap map = new () {	LocationMargin = new Point(570, 120), Size = new Point(100, 100), ToolTip = "Map"
				, BorderStyle = eBorderStyle.Default};
			cMapLayer lay = new ();
				lay.Points.Add(new cMapPin {	Text = "Pin"});
				map.Layers.Add(lay);
			scc.ClientArea.AddControl(map);
		cCalendarControl cac = new () {	LocationMargin = new Point(10, 230)
				, Minimum = new System.DateTime(2017, 7, 3), Maximum = new System.DateTime(2025, 4, 26)};
			scc.ClientArea.AddControl(cac);
		cChart chr = new () {	 LocationMargin = new Point(210, 230), Size = new Point(460, 250)
				, Title = new cTextLayout("Sales", eFont.SystemHeader) {	Alignment = eTextAlignment.Center}
				, BorderStyle = eBorderStyle.Default, Border = new Rect(10)};
			System.Data.DataTable tbl = new ();
				_ = tbl.Columns.Add("Column 1", typeof(string));
					_ = tbl.Columns.Add("Column 2", typeof(float));
				_ = tbl.Rows.Add("Value 1", 100);
					_ = tbl.Rows.Add("Value 2", 300.2);
					_ = tbl.Rows.Add("Value 3", 200);
					_ = tbl.Rows.Add("Value 4", 100);
				chr.SetData(tbl, "Column 1", "Column 2");
			scc.ClientArea.AddControl(chr);
		AddControl(scc);
	wrp.BringToFront(); rzb.BringToFront();
}
private void lnk_LinkClicked(object sender, object? target)	{	mDialog.MsgBoxInformation((string?)target, "Link clicked");}
private void pai_Paint(object sender, PaintArgs e)
{	e.Graphics.FillRoundedRectangle(new RoundedRectangle(20, 20, 60, 40, 10, 10), eBrush.Green);
	e.Graphics.DrawBitmap(mRes.BmpCfg, new Rectangle(40, 50, 50, 50));
}
}