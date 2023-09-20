using DataTable = System.Data.DataTable;
using DateTime = System.DateTime;
using Wew.Control;
using Wew.Media;

namespace Demo;

class fChart : cDockControl
{	readonly cChart chrChart;
public fChart()
{	Text = "Chart";
	cLabel lbl = new () {	LocationMargin = new Point(10, 10), Text = "Type"};
		AddControl(lbl);
	cComboBox cbo = new () {	LocationMargin = new Point(60, 10)};
		_ = cbo.Items.Add(cChart.eType.Bar, "Bar");
		_ = cbo.Items.Add(cChart.eType.Line, "Line");
		cbo.SelectedIndex = 0;
		cbo.SelectionChanged += cboType_SelectionChanged;
		AddControl(cbo);
	cButton btn = new () { LocationMargin = new Point(200, 10), Text = "Print"};
		btn.Click += btnPrint_Click;
		AddControl(btn);
	chrChart = new cChart {	 Margins = new Rect(10, 35, 10, 10)
			, Title = new cTextLayout("Sales report\n2023", eFont.SystemHeader) {	Alignment = eTextAlignment.Center}
			, ValueFormat = "0.00"
			, LabelFormat = "MMM-dd"
			, ValueFont = new cFont("Tahoma", 9, eFontWeight.Bold)
			, LabelFont = new cFont("Sans serif", 9, eFontWeight.Normal, eFontStyle.Italic)
			, ValueColor = eBrush.Crimson
			, LabelColor = eBrush.LimeGreen
			//, LabelFrequency = 28
			, AxisColor = eBrush.Indigo
			, GuideColor = eBrush.Moccasin
		};
		chrChart.Title.SetFont(new cFont("Courier", 10, eFontWeight.Normal, eFontStyle.Oblique), 13);
			chrChart.Title.SetColor(eBrush.DodgerBlue, 0, 12);
			chrChart.Title.SetUnderline(0, 12);
		chrChart.Palette[0] = eBrush.Chartreuse;
		AddControl(chrChart);
	DataTable tbl = new DataTable();
		_ = tbl.Columns.Add("Date", typeof(DateTime));
			tbl.Columns.Add("Sal21", typeof(float)).Caption = "2021";
			tbl.Columns.Add("Sal22", typeof(float)).Caption = "2022";
			tbl.Columns.Add("Sal23", typeof(float)).Caption = "2023";
		for (DateTime dt = new (2023, 1, 1), dtEnd = dt.AddDays(365); dt <= dtEnd; dt = dt.AddDays(30))
		{	_ = tbl.Rows.Add(dt, Wew.mMath.Random() * 500, Wew.mMath.Random() * 500, Wew.mMath.Random() * 500);
		}
	chrChart.SetData(tbl, "Date", "Sal21", "Sal22", "Sal23");
}
private void cboType_SelectionChanged(object sender)
{	chrChart.Type = (cChart.eType)((cComboBox)sender).SelectedItem!.Value!;
}
private void btnPrint_Click(object sender)
{	cPrintPreview pvw = new () {	Margins = new Rect(10), PageCount = 1, Horizontal = true};
		pvw.PaintPage += (object sender, cGraphics g, bool IsPrinting, int page, Rectangle destination) => chrChart.Paint(g, destination);
		pvw.Cancelled += (object sender) => pvw.Close();
		AddControl(pvw);
}
}