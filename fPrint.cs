using DataTable = System.Data.DataTable;
using DateTime = System.DateTime;
using Wew.Control;
using Wew.Media;

namespace Demo;

class fPrint : cDockControl
{	readonly cPrintPreview pvwPrint;
	readonly cCheckBox chkPrintPage;
	readonly cChart chrChart;
	readonly cDataReport m_drpReport;
public fPrint()
{	cButton btn; DataTable tbl;

	Text = "Print";
	btn = new () { LocationMargin = new Point(10, 10), Text = "Print document"};
		btn.Click += btnPrintDoc_Click;
		AddControl(btn);
	btn = new cButton() { LocationMargin = new Point(120, 10), Text = "Print report"};
		btn.Click += btnPrintReport_Click;
		AddControl(btn);
	pvwPrint = new cPrintPreview {	Margins = new Rect(10, 35, 10, 10)};
		pvwPrint.Cancelled += pvwPrint_Cancelled;
		AddControl(pvwPrint);
	chkPrintPage = new cCheckBox {	Text = "Print page numbers", Checked = true};
		chkPrintPage.CheckStateChanged += chkPrintPage_CheckStateChanged;
		pvwPrint.SettingsBar.Controls.Insert(pvwPrint.SettingsBar.Controls.Count - 1, chkPrintPage);
	chrChart = new cChart {	Size = new Point(400, 200)						// ** Create chart
			, Title = new cTextLayout("Sales", eFont.SystemHeader) {	Alignment = eTextAlignment.Center}};
		tbl = new DataTable();
			_ = tbl.Columns.Add("Column_1", typeof(string));
				_ = tbl.Columns.Add("Column_2", typeof(float));
			_ = tbl.Rows.Add("Value 1", 100);
				_ = tbl.Rows.Add("Value 2", 300.2);
				_ = tbl.Rows.Add("Value 3", 200);
				_ = tbl.Rows.Add("Value 4", 100);
		chrChart.SetData(tbl, "Column_1", "Column_2");
	m_drpReport = new cDataReport {	Font = new cFont("Courier", 12)};
		m_drpReport.PageHeader.Height = 25;
			m_drpReport.PageHeader.Items.Add(new cDataReport.cVariable() {	Bounds = new Rectangle(500, 0, 50, 25), Variable = cDataReport.cVariable.eVariable.PageNumber, TextFormat = eTextFormat.RightTop});
			m_drpReport.PageHeader.Items.Add(new cDataReport.cLabel() {	Bounds = new Rectangle(555, 0, 10, 25), Text = "/"});
			m_drpReport.PageHeader.Items.Add(new cDataReport.cVariable() {	Bounds = new Rectangle(565, 0, 50, 25), Variable = cDataReport.cVariable.eVariable.PageCount});
		m_drpReport.ReportHeader.Height = 100;
			m_drpReport.ReportHeader.Items.Add(new cDataReport.cBitmap() {	Bounds = new Rectangle(10, 0, 80, 80), Bitmap = mRes.BmpPhoto, Resize = eResize.ScaleIfBig});
			m_drpReport.ReportHeader.Items.Add(new cDataReport.cLabel() {	Bounds = new Rectangle(200, 0, 500, 25), Text = "Sales 2023", Font = eFont.SystemHeader, ForeColor = eBrush.DodgerBlue});
			m_drpReport.ReportHeader.Items.Add(new cDataReport.cVariable() {	Bounds = new Rectangle(400, 40, 200, 25), Variable = cDataReport.cVariable.eVariable.Date, Font = eFont.SystemBoldText, ForeColor = eBrush.MidnightBlue});
		m_drpReport.BodyPageHeader.Height = 100;
			m_drpReport.BodyPageHeader.PrintAfterReportHeader = true;
			m_drpReport.BodyPageHeader.Items.Add(new cDataReport.cItem() {	Bounds = new Rectangle(0, 0, 600, 25), Shape = cDataReport.cItem.eShape.Rectangle, LineColor = eBrush.MediumSlateBlue, BackColor = new cRadialGradientBrush(eColor.LightYellow, eColor.Blue, eExtendMode.Mirror) {	Center = new Point(300, 10), Radius = new Point(25, 10)}});
			m_drpReport.BodyPageHeader.Items.Add(new cDataReport.cLabel() {	Bounds = new Rectangle(0, 0, 100, 25), Text = "Date", Font = eFont.SystemBoldText, TextFormat = eTextFormat.CenterMiddle});
			m_drpReport.BodyPageHeader.Items.Add(new cDataReport.cLabel() {	Bounds = new Rectangle(100, 0, 100, 25), Text = "Sal21", Font = eFont.SystemBoldText, TextFormat = eTextFormat.CenterMiddle});
			m_drpReport.BodyPageHeader.Items.Add(new cDataReport.cLabel() {	Bounds = new Rectangle(200, 0, 100, 25), Text = "Sal22", Font = eFont.SystemBoldText, TextFormat = eTextFormat.CenterMiddle});
			m_drpReport.BodyPageHeader.Items.Add(new cDataReport.cLabel() {	Bounds = new Rectangle(300, 0, 100, 25), Text = "Sal23", Font = eFont.SystemBoldText, TextFormat = eTextFormat.CenterMiddle, ForeColor = eBrush.OrangeRed});
		m_drpReport.Body.Group = new cDataReport.cGroup() {	Field = "Year"};
			m_drpReport.Body.Group.Header.Height = 80;
			m_drpReport.Body.Group.Header.Items.Add(new cDataReport.cField() {	Bounds = new Rectangle(0, 0, 100, 25), Field = "Year", Format = "0", TextFormat = eTextFormat.CenterMiddle, Border = new Rect(2), Shape = cDataReport.cItem.eShape.Bottom});
			m_drpReport.Body.Group.SubGroup = new cDataReport.cGroup() {	Field = "Month"};
				m_drpReport.Body.Group.SubGroup.Header.Height = 80;
				m_drpReport.Body.Group.SubGroup.Header.Items.Add(new cDataReport.cField() {	Bounds = new Rectangle(50, 0, 100, 25), Field = "Month", Format = "MMMM", TextFormat = eTextFormat.CenterMiddle, Border = new Rect(2), Shape = cDataReport.cItem.eShape.Bottom});
		m_drpReport.Body.Height = 80;
			m_drpReport.Body.Items.Add(new cDataReport.cField() {	Bounds = new Rectangle(0, 0, 100, 25), Field = "Date", Format = "MMM-dd", TextFormat = eTextFormat.CenterMiddle, Border = new Rect(2), Shape = cDataReport.cItem.eShape.Rectangle});
			m_drpReport.Body.Items.Add(new cDataReport.cField() {	Bounds = new Rectangle(100, 0, 100, 25), Field = "Sal21", Format = "0.00", TextFormat = eTextFormat.RightMiddle, Border = new Rect(2), Shape = cDataReport.cItem.eShape.Rectangle});
			m_drpReport.Body.Items.Add(new cDataReport.cField() {	Bounds = new Rectangle(200, 0, 100, 25), Field = "Sal22", Format = "0.00", TextFormat = eTextFormat.RightMiddle, Border = new Rect(2), Shape = cDataReport.cItem.eShape.Rectangle});
			m_drpReport.Body.Items.Add(new cDataReport.cField() {	Bounds = new Rectangle(300, 0, 100, 25), Field = "Sal23", Format = "0.00", TextFormat = eTextFormat.RightMiddle, Border = new Rect(2), Shape = cDataReport.cItem.eShape.Rectangle, ForeColor = eBrush.OrangeRed});
			m_drpReport.Body.Items.Add(new cDataReport.cField() {	Bounds = new Rectangle(300, 25, 100, 25), Field = "Sal23", Format = "0.00", TextFormat = eTextFormat.RightMiddle, Border = new Rect(2), Shape = cDataReport.cItem.eShape.RoundedRectangle, RoundedRectangleRadius = new Point(3, 3), ForeColor = eBrush.Indigo, GroupOperation = cDataReport.cField.eGroupOperation.Sum, GroupRange = cDataReport.cField.eGroupRange.ToCurrentRow});
		m_drpReport.BodyPageFooter.Height = 30;
			m_drpReport.BodyPageFooter.Items.Add(new cDataReport.cLabel() {	Bounds = new Rectangle(0, 0, 100, 25), Text = "Subtotal", TextFormat = eTextFormat.RightTop, ForeColor = eBrush.Green});
			m_drpReport.BodyPageFooter.Items.Add(new cDataReport.cField() {	Bounds = new Rectangle(100, 0, 100, 25), Field = "Sal21", Format = "0.00", TextFormat = eTextFormat.RightMiddle, Border = new Rect(2), Shape = cDataReport.cItem.eShape.Rectangle, GroupOperation = cDataReport.cField.eGroupOperation.Sum, GroupRange = cDataReport.cField.eGroupRange.Page});
			m_drpReport.BodyPageFooter.Items.Add(new cDataReport.cField() {	Bounds = new Rectangle(200, 0, 100, 25), Field = "Sal22", Format = "0.00", TextFormat = eTextFormat.RightMiddle, Border = new Rect(2), Shape = cDataReport.cItem.eShape.Rectangle, GroupOperation = cDataReport.cField.eGroupOperation.Sum, GroupRange = cDataReport.cField.eGroupRange.Page});
			m_drpReport.BodyPageFooter.Items.Add(new cDataReport.cField() {	Bounds = new Rectangle(300, 0, 100, 25), Field = "Sal23", Format = "0.00", TextFormat = eTextFormat.RightMiddle, Border = new Rect(2), Shape = cDataReport.cItem.eShape.Rectangle, ForeColor = eBrush.OrangeRed, GroupOperation = cDataReport.cField.eGroupOperation.Sum, GroupRange = cDataReport.cField.eGroupRange.Page});
		m_drpReport.ReportFooter.Height = 100;
			m_drpReport.ReportFooter.Items.Add(new cDataReport.cLabel() {	Bounds = new Rectangle(0, 0, 100, 25), Text = "Total", TextFormat = eTextFormat.RightTop});
			m_drpReport.ReportFooter.Items.Add(new cDataReport.cField() {	Bounds = new Rectangle(100, 0, 100, 25), Field = "Sal21", Format = "0.00", TextFormat = eTextFormat.RightMiddle, Border = new Rect(2), Shape = cDataReport.cItem.eShape.Rectangle, GroupOperation = cDataReport.cField.eGroupOperation.Sum, GroupRange = cDataReport.cField.eGroupRange.ToEnd});
			m_drpReport.ReportFooter.Items.Add(new cDataReport.cField() {	Bounds = new Rectangle(200, 0, 100, 25), Field = "Sal22", Format = "0.00", TextFormat = eTextFormat.RightMiddle, Border = new Rect(2), Shape = cDataReport.cItem.eShape.Rectangle, GroupOperation = cDataReport.cField.eGroupOperation.Sum, GroupRange = cDataReport.cField.eGroupRange.ToEnd});
			m_drpReport.ReportFooter.Items.Add(new cDataReport.cField() {	Bounds = new Rectangle(300, 0, 100, 25), Field = "Sal23", Format = "0.00", TextFormat = eTextFormat.RightMiddle, Border = new Rect(2), Shape = cDataReport.cItem.eShape.Rectangle, ForeColor = eBrush.OrangeRed, GroupOperation = cDataReport.cField.eGroupOperation.Sum, GroupRange = cDataReport.cField.eGroupRange.ToEnd});
		m_drpReport.PageFooter.Height = 25;
			m_drpReport.PageFooter.Items.Add(new cDataReport.cVariable() {	Bounds = new Rectangle(500, 0, 100, 25), Variable = cDataReport.cVariable.eVariable.PageNumber});
		tbl = new DataTable();
			_ = tbl.Columns.Add("Date", typeof(DateTime));
			_ = tbl.Columns.Add("Month", typeof(DateTime));
			_ = tbl.Columns.Add("Year", typeof(int));
				tbl.Columns.Add("Sal21", typeof(float)).Caption = "2021";
				tbl.Columns.Add("Sal22", typeof(float)).Caption = "2022";
				tbl.Columns.Add("Sal23", typeof(float)).Caption = "2023";
			for (DateTime dt = new (2023, 1, 1), dtEnd = dt.AddDays(365 * 2); dt <= dtEnd; dt = dt.AddDays(15))
			{	_ = tbl.Rows.Add(dt, new DateTime(dt.Year, dt.Month, 1), dt.Year
					, Wew.mMath.Random() * 500, Wew.mMath.Random() * 500, Wew.mMath.Random() * 500);
			}
		m_drpReport.SetDataSource(tbl);
	btnPrintReport_Click(null);												// ** Layout pages after control is placed in a window
	pvwPrint.Zoom = cPrintPreview.eZoom.TwoPages;
}
private void btnPrintDoc_Click(object? sender)
{	m_drpReport.Printer = null;
	chkPrintPage.Enabled = true;
	pvwPrint.SettingsChanged += pvwPrint_SettingsChanged;
		pvwPrint.PaintPage += pvwPrint_PaintPage;
	pvwPrint_SettingsChanged(null);
}
private void btnPrintReport_Click(object? sender)
{	chkPrintPage.Enabled = false;
	pvwPrint.SettingsChanged -= pvwPrint_SettingsChanged;
		pvwPrint.PaintPage -= pvwPrint_PaintPage;
	m_drpReport.Printer = pvwPrint;
}
private void pvwPrint_SettingsChanged(object? sender)
{	pvwPrint.PageCount = (pvwPrint.PageSize.Y > 1100 ? 5 : 21);
	pvwPrint.setPage(1, new cPrintPreview.Page {	Horizontal = true});
	pvwPrint.setPage(2, new cPrintPreview.Page {	Border = new Rect(10, 20, 5, 15)});
	pvwPrint.setPage(4, new cPrintPreview.Page {	Horizontal = true});
}
private void chkPrintPage_CheckStateChanged(object sender)	{	pvwPrint.Refresh();}
private void pvwPrint_PaintPage(object sender, cGraphics g, bool IsPrinting, int page, Rectangle destination)
{	int iLine;

	iLine = page * 2 + 1;
	if (page == 0)
	{	g.DrawText("Title 0", destination, eBrush.Blue, eFont.SystemBoldText, new TextFormat(eTextAlignment.Center, eParagraphAlignment.Top));
			destination.Scroll(0, 50);
		g.DrawText($"Line {iLine}", destination, eBrush.Black, eFont.SystemText, eTextFormat.Default); destination.Scroll(0, 30);
		g.DrawText($"Line {iLine + 1}", destination, eBrush.Black, eFont.SystemText, eTextFormat.Default); destination.Scroll(0, 30);
		g.DrawBitmap(mRes.BmpDisplMap, destination);
	} else
	{	g.DrawText($"Line {iLine}", destination, eBrush.Black, eFont.SystemText, eTextFormat.Default); destination.Scroll(0, 30);
		g.DrawText($"Line {iLine + 1}", destination, eBrush.Black, eFont.SystemText, eTextFormat.Default); destination.Scroll(0, 30);
		if (page == 1)	chrChart.Paint(g, destination);
	}
	if (chkPrintPage.Checked)
	{	destination.Y = destination.Bottom; destination.Height = 15;
		g.DrawText($"pag {page + 1}", destination, eBrush.Black, eFont.SystemText, eTextFormat.CenterMiddle);
	}
}
private void pvwPrint_Cancelled(object sender)				{	Close();}
}