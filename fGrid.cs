using DataTable = System.Data.DataTable;
using DateTime = System.DateTime;
using Wew.Control;
using Wew.Media;
using Wew;

namespace Demo;

class fGrid : cDockControl
{	readonly cGrid grdGrd;
	readonly cLabel lblPos;
public fGrid()
{	Text = "Grid";
	grdGrd = new cGrid() {	Margins = new Rect(10, 10, 10, 40)};
		grdGrd.RowChanged += grdGrd_CoordinateChanged;
		grdGrd.ColumnChanged += grdGrd_CoordinateChanged;
		grdGrd.EditValidate += grdGrd_EditValidation;
		//grdGrd.SortCompare += grdGrd_SortCompare;
		AddControl(grdGrd);
	cButton btn = new () {	Text = "Load", Margins = new Rect(10, float.NaN, float.NaN, 10)};
		btn.Click += btnLoad_Click;
		AddControl(btn);
	btn = new () {	Text = "Add columns", Margins = new Rect(120, float.NaN, float.NaN, 10)};
		btn.Click += btnAddColumns_Click;
		AddControl(btn);
	btn = new () {	Text = "Remove columns", Margins = new Rect(230, float.NaN, float.NaN, 10)};
		btn.Click += btnRemoveColumns_Click;
		AddControl(btn);
	btn = new () {	Text = "Add rows", Margins = new Rect(340, float.NaN, float.NaN, 10)};
		btn.Click += btnAddRows_Click;
		AddControl(btn);
	btn = new () {	Text = "Remove rows", Margins = new Rect(450, float.NaN, float.NaN, 10)};
		btn.Click += btnRemoveRows_Click;
		AddControl(btn);
	lblPos = new cLabel() {	Margins = new Rect(560, float.NaN, float.NaN, 10)};
		AddControl(lblPos);
	// ** Set columns
	//grdGrd.DefaultColumnWidth = 240;
		grdGrd.ColumnCount = 8;
		grdGrd.HeaderColumnCount = 1;
		grdGrd.ConfigureColumn(0, "Column 0", 80);
			grdGrd.Columns(0).Font = new cFont("Arial", 13, eFontWeight.Normal, eFontStyle.Oblique);
		grdGrd.ConfigureColumn(1, "Column 1", 170, eTextFormat.RightMiddle, name: "DataColumn1");
			grdGrd.Columns(1).Resizable = false;
			grdGrd.Columns(1).AllowFilter = true;
		grdGrd.ConfigureColumn(2, "Column 2", 180, ReadOnly: false);
			grdGrd.Columns(2).BackColor = eBrush.LightGreen;
			grdGrd.Columns(2).AllowFilter = true;
		grdGrd.ConfigureColumn(3, "Column 3", 220, eTextFormat.CenterMiddle, null, false, true, typeof(System.DateTime), name: "DataColumn2");
			grdGrd.Columns(3).Format = "yyyy/MM/dd";
			grdGrd.Columns(3).DisplayIndex = 1;
			grdGrd.Columns(3).ComboProperties = new cGrid.ComboProperties(new DateTime[] {	new DateTime(2023, 04, 26), new DateTime(2020, 02, 20), new DateTime(2020, 11, 18)});
			grdGrd.Columns(3).AllowFilter = true;
		grdGrd.ConfigureColumn(4, "Column 4", 100, new TextFormat() {	Wrapping = eWrapping.Wrap}, null, false, name: "DataColumn3");
			grdGrd.Columns(4).Type = typeof(bool);
			grdGrd.Columns(4).AllowFilter = true;
		grdGrd.ConfigureColumn(5, "Column 5", 200, new TextFormat() {	Alignment = eTextAlignment.Right}, null, false);
			grdGrd.Columns(5).TextColor = eBrush.Blue;
			grdGrd.Columns(5).AllowFilter = true;
	// ** Set rows
	//grdGrd.DefaultRowHeight = 40; grdGrd.Rows(0).Height = 40;
		grdGrd.RowCount = 100;
		System.DateTime dt = System.DateTime.Now;
		for (int i = 1; i < grdGrd.RowCount; i++)
		{	grdGrd[i, 0] = "text " + i; grdGrd[i, 1] = i; grdGrd[i, 3] = dt; grdGrd[i, 5] = "value " + i;
			dt = dt.AddDays((i % 3) / 2);
		}
		grdGrd[35, 4] = "value with long multiline string and wrapping";
	grdGrd.Rows(7).Visible = false;
	grdGrd.Rows(10).BackColor = eBrush.LightSlateGray;
		grdGrd.Rows(20).TextColor = eBrush.Orange;
		grdGrd.Rows(30).Font = new cFont("Courier", 8, eFontWeight.Bold);
		grdGrd.Rows(40).Height = 40;
	// ** Set cells
	cGrid.Cell cel;
	
	cel = grdGrd.Cells(8, 1);
		cel.Type = typeof(short);
		cel.ReadOnly = false;
	grdGrd[5, 2] = new Point(); grdGrd.Cells(5, 2).Type = typeof(Point); grdGrd.Cells(5, 2).TextLayout.SetColor(eBrush.Red, 0, 2);
	grdGrd[6, 2] = new Vector(1, 2, 3); grdGrd.Cells(6, 2).Type = typeof(Vector); grdGrd.Cells(6, 2).TextLayout.SetColor(eBrush.Red, 3, 2);
	grdGrd[10, 2] = mRes.BmpAni;
	cel = grdGrd.Cells(11, 1);
		cel.ComboProperties = new cGrid.ComboProperties(new int[] {	15, 99, 3, 8}, null, true);
		cel.Type = typeof(int);
		cel.ReadOnly = false;
	grdGrd.Cells(12, 3).ComboProperties = new ();							// Avoid combo
	grdGrd[12, 4] = true;
	grdGrd.Cells(12, 0).BackColor = eBrush.LightSalmon;
	grdGrd[13, 4] = true; grdGrd.Cells(13, 4).ReadOnly = true;
	cel = grdGrd.Cells(14, 2);
		cel.Control = new cSlider() {	BackColor = null, FocusMode = eFocusMode.ActivateWindow, MinimumSize = new Point(150, 50), MaximumSize = new Point(150, 50)};
		cel.ShowControl(true);
		cel.TextFormat = eTextFormat.RightMiddle;
	grdGrd[16, 2] = new cRadialGradientBrush(eColor.Red, eColor.Blue, eExtendMode.Mirror)
			{	Center = new Point(60, 10), Radius = new Point(40, 10)
			};
		grdGrd.Cells(16, 2).Type = typeof(cBrush);
	grdGrd[17, 2] = eColor.Pink; grdGrd.Cells(17, 2).Type = typeof(Color);
	cel = grdGrd.Cells(18, 2);
		cel.Control = new cProgressBar() {	Value = 33, ProgressColor = eBrush.Red, HitTestTransparent = eHitTestTransparent.Background};
		cel.ShowControl(true);
		cel.TextFormat = eTextFormat.CenterMiddle;
	grdGrd.Cells(22, 1).TextColor = eBrush.Green;
	grdGrd.Cells(32, 5).Font = new cFont("Sans serif", 8, eFontWeight.Thin);
	grdGrd.Cells(42, 3).Format = "hh:mm:ss tt";
		System.Globalization.DateTimeFormatInfo dtf = (System.Globalization.DateTimeFormatInfo)System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.Clone();
			dtf.AMDesignator = "aM"; dtf.PMDesignator = "Pm";
		grdGrd.Cells(42, 3).FormatProvider = dtf;
	grdGrd[40, 5] = "Text layout value with long multiline string and wrapping *";
		cTextLayout tl = grdGrd.Cells(40, 5).TextLayout;
		tl.Wrapping = eWrapping.Wrap;
		tl.SetColor(eBrush.Red, 5, 6);
		tl.SetFont(eFont.SystemBoldText, 12, 5);
		tl.SetControl(new cTextLayout.cPicture(mRes.BmpSmile, new Point(15, 15), 12), 58);
	// ** Set grid
	//grdGrd.BackColor = eBrush.SeaGreen;
	//grdGrd.HeaderBackColor = eBrush.Blue;
	//grdGrd.HeaderTextColor = eBrush.White;
	//grdGrd.CellBackColor = eBrush.Beige;
	//grdGrd.TextColor = eBrush.Blue;
	//grdGrd.HighlightColor = new cSolidBrush(eColor.LightSalmon, 0.1f);
	//grdGrd.SelectionColor = new cSolidBrush(eColor.Salmon, 0.2f);
	grdGrd.MultiSelect = true;
	grdGrd.HighlightSelectedRows = true;
	//grdGrd.AlternRowColor = eBrush.Yellow;
	//grdGrd.HeaderLineColor = eBrush.Black;
	//grdGrd.LineColor = null;
	grdGrd.FrozenCells = new cGrid.CellRange(4, 1, 7, 2);
	//grdGrd.AllowColumnReorder = false;
	grdGrd.AllowRowResize = cGrid.eResize.FromCell;
	grdGrd_CoordinateChanged(null);
}
private void grdGrd_CoordinateChanged(object? sender)		{	lblPos.Text = $"Row={grdGrd.CurrentRow}, Column={grdGrd.CurrentColumn}";}
private void grdGrd_EditValidation(object sender, ref cGrid.EditArgs e)
{	lblPos.Text = $"Validating: Row={grdGrd.CurrentRow}, Column={grdGrd.CurrentColumn}, Value={e.Value}";
}
private void grdGrd_SortCompare(object sender, in cGrid.SortCompareArgs e, out int result, out bool SkipColumn)
{	result = 0; SkipColumn = false;
	// ** First time: move images to the beginning, ignore direction
	if (e.IsFirstColumn == true)
	{	if (grdGrd[e.RowA, 2] is cImage)
		{	if (grdGrd[e.RowB, 2] is not cImage)	result = -1;
		} else if (grdGrd[e.RowB, 2] is cImage)
			result = 1;
	}
	// ** Ignore column 2
	if (e.Column == 2)
	{	SkipColumn = true;
		if (e.IsFirstColumn == false)	result = e.RowA - e.RowB;			// It was the last (or only) column, return a result (different from 0)
	}
	//if (!e.Ascending)	result *= -1;
}
private void btnLoad_Click(object sender)
{
	//DataTable tbl;
	//grdGrd.ColumnCount = 0;
	//tbl = new DataTable();
	//_ = tbl.Columns.Add("DataColumn1", typeof(short));
	//	_ = tbl.Columns.Add("DataColumn2", typeof(string));
	//	_ = tbl.Columns.Add("DataColumn3", typeof(float));
	//for (int i = 0; i < 10; i++)
	//{	_ = tbl.Rows.Add(i, "string " + i, i * 2.5f);
	//}
	//_ = tbl.Rows.Add();
	//tbl.Rows[0][1] = null;
	//grdGrd.Load(tbl, true);

	cDataConnection cn = new cDataConnection(@"server=(localdb)\MSSQLLocalDB; AttachDBFilename=E:\_Cod\_ant\Bd\musical.mdf");
	DataTable tbl = new ();
	int i = cn.Execute("pCanta", tbl, null, "@Canta_Nomb", "pet%");
	grdGrd.ColumnCount = 0; grdGrd.Load(tbl, true);

	//var va = from v in new cGrid[2]{new cGrid(), new cGrid()} select v;
	//grdGrd.Load(va.ToList(), true);
}
private void btnAddColumns_Click(object sender)				{	grdGrd.ColumnCount += 10;}
private void btnRemoveColumns_Click(object sender)			{	grdGrd.RemoveColumns(3, 2);}
private void btnAddRows_Click(object sender)				{	grdGrd.InsertRows(4, 16);}
private void btnRemoveRows_Click(object sender)				{	grdGrd.RemoveRows(3, 2);}
}