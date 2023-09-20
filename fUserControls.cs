using Wew.Control;
using Wew.Media;

namespace Demo;

class fUserControls : cDockControl
{	class cCtl : cControl
	{	public cCtl()
		{
		}
	}
	readonly uMatrix umaMatrix;
public fUserControls()
{	Text = "User controls";
	cScrollableControl scc = new () {	LocationMargin = new Point(10, 10), Margins = new Rect(5), ClientEndMargin = new Point(10, 10)};
		uSlider usl = new () {	LocationMargin = new Point(10, 10)
				, Text = "Slider", Value = 50};
			scc.ClientArea.AddControl(usl);
		uPoint upt = new () {	LocationMargin = new Point(10, 40)
				, Text = "Point", Value = new Point(50, 60)};
			scc.ClientArea.AddControl(upt);
		uVector uv = new () {	LocationMargin = new Point(10, 70)
				, Text = "Vector", Value = new Vector(50, 60, 70)};
			scc.ClientArea.AddControl(uv);
		uBrush ubr = new () {	LocationMargin = new Point(400, 10), BorderStyle = eBorderStyle.Default, ToolTip = "Brush"};
			scc.ClientArea.AddControl(ubr);
		uColor ucl = new () {	LocationMargin = new Point(10, 100)};
			scc.ClientArea.AddControl(ucl);
		uLineStyle uls = new () {	LocationMargin = new Point(710, 10), ToolTip = "Line style"
				, BorderStyle = eBorderStyle.Default};
			scc.ClientArea.AddControl(uls);
		umaMatrix = new () {	LocationMargin = new Point(710, 370)};
			umaMatrix.Reset(new string[] {"X", "Y", "Z", "W"}, 4); umaMatrix.Matrix4x4 = Matrix4x4.Identity;
			scc.ClientArea.AddControl(umaMatrix);
		cCtl ctCtl = new cCtl() {	Bounds = new Rectangle(960, 10, 30, 20), BackColor = eBrush.Red};
			scc.ClientArea.AddControl(ctCtl);
		uFont ufn = new () {	LocationMargin = new Point(10, 510)
				, BorderStyle = eBorderStyle.Default};
			scc.ClientArea.AddControl(ufn);
		uGlyphs ugl = new () {	LocationMargin = new Point(520, 510), Width = 600
				, BorderStyle = eBorderStyle.Default};
			scc.ClientArea.AddControl(ugl);
		uLightControl ulc = new () {	LocationMargin = new Point(10, 1020), ToolTip = "Light"
				, BorderStyle = eBorderStyle.Default};
			scc.ClientArea.AddControl(ulc);
		u3DModelControl u3m = new () {	LocationMargin = new Point(380, 1120), ToolTip = "3D model"
				, BorderStyle = eBorderStyle.Default};
			scc.ClientArea.AddControl(u3m);
		uPropertyGroup pgr = new () {	LocationMargin = new Point(750, 1120), RightMargin = float.NaN, Width = 250
				, Text = "Group"};
			pgr.AddControl(new cTextBox {	Text = "Text box 1"});
			pgr.AddControl(new cTextBox {	Text = "Text box 2"});
			uPropertySubgroup psg = new () {	Text = "Subgroup"};
				psg.AddControl(new cTextBox {	Text = "Text box 3"});
				psg.AddControl(new cButton {	Text = "...", Width = 25});
				pgr.AddControl(psg);
			scc.ClientArea.AddControl(pgr);
		AddControl(scc);
}
protected override void OnKeyDownPreview(ref KeyArgs e)
{	if (e.Key == eKey.F9)
	{	umaMatrix.Enabled = false;
	}
	base.OnKeyDownPreview(ref e);
}
private void lnk_LinkClicked(object sender, object target)	{	mDialog.MsgBoxInformation((string)target, "Link clicked");}
private void pai_Paint(object sender, PaintArgs e)
{	e.Graphics.FillRoundedRectangle(new RoundedRectangle(20, 20, 60, 40, 10, 10), eBrush.Green);
	e.Graphics.DrawBitmap(mRes.BmpCfg, new Rectangle(50, 50, 30, 30));
}
}