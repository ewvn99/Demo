using Wew.Control;
using Wew.Media;

namespace Demo;

partial class wMain : cWindow
{	static cLabel lblStat2;
	public static cWindow Device;
	public static kToolBox ToolBox;
	public static kPropertyGrid PropertyGrid;
	public static readonly System.Guid DsrFileDlgGuid = new ("CE6D7019-9B19-43C0-9AB7-8ECA7C57E580");
public static string? Stat2									{	set	=>	lblStat2.Text = value;}
public wMain()
{	cMenuBar mb; cToolBar tb; cStatusBar sb; cToolButton tbt, tbt2; cCommandState csCfg; cToolComboBox tcb;
	string[] a_sZoom = new string[] {	"10 %", "25 %", "50 %", "75 %", "100 %", "200 %", "400 %", "1000 %", "2000 %"};

	// ** Load resource font
	cFont.AddFonts(GetType(), "Res.Digital.ttf");
	// ** Configure window
	Icon = mRes.BmpTool; Text = "Demo";
	eCommand.Zoom.Shortcut = eKey.Z | eKey.AltModifier;
	csCfg = new cCommandState(new cCommand("Config", mRes.BmpCfg), cmdCfg_Click);
	mb = new cMenuBar();
		tbt = mb.Items.Add("Controls");
			tbt.Menu = new cMenu();
			_ = tbt.Menu.Items.Add("Controls", null, mniControls_Click);
			_ = tbt.Menu.Items.Add("User controls", null, mniUserControls_Click);
			_ = tbt.Menu.Items.Add("Grid", null, mniGrid_Click);
			_ = tbt.Menu.Items.Add("Layout", null, mniLayout_Click);
			_ = tbt.Menu.Items.Add("Chart", null, mniChart_Click);
			_ = tbt.Menu.Items.Add("Print", null, mniPrint_Click);
			tbt2 = tbt.Menu.Items.Add("Designer", null, mniDataReportDesigner_Click);
				tbt2.Menu = new cMenu();
				_ = tbt2.Menu.Items.Add("Data table", null, mniDataTableDesigner_Click);
				_ = tbt2.Menu.Items.Add("Data report", null, mniDataReportDesigner_Click);
			tbt.Menu.Add(eCommand.Separator, eCommand.Exit);
		tbt = mb.Items.Add("Media");
			tbt.Menu = new cMenu();
			_ = tbt.Menu.Items.Add("Animation", null, mniAnim_Click);
			_ = tbt.Menu.Items.Add("Text", null, mniText_Click);
			_ = tbt.Menu.Items.Add("2D", null, mni2D_Click);
			_ = tbt.Menu.Items.Add("Image metadata", null, mniImgMetadata_Click);
			_ = tbt.Menu.Items.Add("Compute shader", null, mniCompute_Click);
			_ = tbt.Menu.Items.Add("3D inside 2D", null, mni3DInside2D_Click);
			_ = tbt.Menu.Items.Add("3D", null, mni3D_Click);
			_ = tbt.Menu.Items.Add("Audio/video", null, mniAudVid_Click);
		tbt = mb.Items.Add("Services");
			tbt.Menu = new cMenu();
			_ = tbt.Menu.Items.Add("Services", null, mniServ_Click);
		tbt = mb.Items.Add("Menu");
			tbt.Menu = new cMenu();
			tcb = new cToolComboBox(eCommand.Zoom) {	Width = 120, Style = cComboBox.eStyle.DropDown
						, InputFilter = eInputType.Positive, MultiExecution = true};
					tcb.Load(a_sZoom);
				tbt.Menu.Items.Add(tcb);
			tbt2 = tbt.Menu.Items.Add("Format");
				tbt2.Menu = new cMenu();
				_ = tbt2.Menu.Items.Add(csCfg.Command);
			tbt.Menu.Add(eCommand.Copy, eCommand.Paste);
		tbt = mb.Items.Add("View");
		mb.Items.Add(new cSeparator());
		tbt = mb.Items.Add("Help");
		tbt = new cToolButton {	Text = "Login", Bitmap = mRes.BmpSmile, AutoSize = eAutoSize.Width, Cursor = eCursor.Hand
				, LeftMargin = float.NaN, RightMargin = 0, Height = 21, Dock = eDirection.Right, DisplayMode = eButtonDisplay.IconBeforeText};
			tbt.TextLayout.SetUnderline(0);
			tbt.Click += (s) => mDialog.MsgBoxInformation("click", "Login");
			mb.AddControl(tbt);
		ToolStrips.Add(mb);
		Root.AddControl(mb);
	tb = new cToolBar {	Height = 36};
		tbt = tb.Items.Add("Open", Wew.eResource.BmpOpen, tbtOpen_Click);
		tb.Add(eCommand.Save, eCommand.Separator, eCommand.Cut, eCommand.Copy, eCommand.Paste
			, eCommand.Separator, eCommand.Undo, eCommand.Redo);
		tbt = tb.Items.Add("ToolBox", mRes.BmpTool, tbtToolBox_Click);
		tbt = tb.Items.Add("Properties", mRes.BmpCfg, tbtPropertyGrid_Click);
		tb.Items.Add(new cSeparator());
		tbt = tb.Items.Add("Tools", mRes.BmpTool, null);
			tbt.Type = eButtonType.Split;
			tbt.Menu = new cMenu();
			_ = tbt.Menu.Items.Add(csCfg.Command);
		tb.Items.Add(new cSeparator());
		tcb = new cToolComboBox(eCommand.Zoom) {	Width = 120, Style = cComboBox.eStyle.DropDown
					, InputFilter = eInputType.Positive, MultiExecution = true};
				tcb.Load(a_sZoom);
			tb.Items.Add(tcb);
		ToolStrips.Add(tb);
		Root.AddControl(tb);
	sb = new cStatusBar();
		sb.AddLabel("Label 1", 200).Text = "State 1";
		sb.AddControl(new cSeparator());
		lblStat2 = sb.AddLabel("Label 2", 300); lblStat2.Text = "State 2";
		Root.AddControl(sb);
	Device = this;
	ToolBox = new kToolBox {	Container = Root};
		//ToolBox.Show();
	PropertyGrid = new kPropertyGrid {	Container = Root};
		//PropertyGrid.Show();
	LoadCommands(this, csCfg, new cCommandState(eCommand.Exit, cmdExit_Click, true), new cCommandState(eCommand.Zoom, cmdZoom_Click, true));
	f2D.RegisterEffects();
	//mniDataTableDesigner_Click(null!);
	Size = new Point(1000, 580); CenterToScreen();
}
private void mniControls_Click(object sender)				{	fControls frm = new () {	Container = Root}; frm.Show();}
private void mniUserControls_Click(object sender)			{	fUserControls frm = new () {	Container = Root}; frm.Show();}
private void mniGrid_Click(object sender)					{	fGrid frm = new () {	Container = Root}; frm.Show();}
private void mniLayout_Click(object sender)					{	fLayout frm = new () {	Container = Root}; frm.Show();}
private void mniChart_Click(object sender)					{	fChart frm = new () {	Container = Root}; frm.Show();}
private void mniPrint_Click(object sender)					{	fPrint frm = new () {	Container = Root}; frm.Show();}
private void mniDataTableDesigner_Click(object sender)		{	fDataTableDesigner frm = new () {	Container = Root}; frm.Show();}
private void mniDataReportDesigner_Click(object sender)		{	fDataReportDesigner frm = new () {	Container = Root}; frm.Show();}
private void mniAnim_Click(object sender)					{	fAnim frm = new () {	Container = Root}; frm.Show();}
private void mniText_Click(object sender)					{	fText frm = new () {	Container = Root}; frm.Show();}
private void mni2D_Click(object sender)						{	f2D frm = new () {	Container = Root}; frm.Show();}
private void mniImgMetadata_Click(object sender)			{	fImgMetadata frm = new () {	Container = Root}; frm.Show();}
private void mniCompute_Click(object sender)				{	fComputeShader frm = new () {	Container = Root}; frm.Show();}
private void mni3DInside2D_Click(object sender)				{	f3DInside2D frm = new ()	{	Container = Root}; frm.Show();}
private void mni3D_Click(object sender)						{	w3D wnd = new (); wnd.Show(this);}
private void mniAudVid_Click(object sender)					{	fAudVid frm = new () {	Container = Root}; frm.Show();}
private void mniServ_Click(object sender)					{	fServices frm = new () {	Container = Root}; frm.Show();}
private void cmdCfg_Click(cCommand command, object? args)	{	mDialog.MsgBoxInformation("config", "cfg");}
private void cmdExit_Click(cCommand command, object? args)	{	Close();}
private void cmdZoom_Click(cCommand command, object? args)	{	mDialog.MsgBoxInformation("click", "Zoom");}
private void tbtOpen_Click(object sender)
{	string? sPath;
	
	sPath = mDialog.ShowOpenFile("DataTable Designer file|*.dtf", DsrFileDlgGuid);	if (sPath is null) return;
	if (System.IO.Path.GetExtension(sPath).ToLower() == ".dtf")
	{	fDataTableDesigner frm = new (sPath) {	Container = Root};
	
		frm.Show();
	} else
		mDialog.MsgBoxExclamation("Invalid file", "Open");
}
private void tbtToolBox_Click(object sender)				{	ToolBox.Show();}
private void tbtPropertyGrid_Click(object sender)			{	PropertyGrid.Show();}
}