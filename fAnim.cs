using Math = System.Math;
using Wew.Control;
using Wew.Media;

namespace Demo;

class fAnim : cDockControl
{	class cBtn : cButton
	{	cStoryboard? m_stoBorder; cBrushAnimation? m_braBorder;
		cStoryboard? m_stoSize; cFloatAnimation? m_fanResize; Point m_ptSize, m_ptCenter;
		protected override void OnMouseEnter()				{	m_AnimateBorder(eBrush.SystemSelectionBorder.Color); base.OnMouseEnter();}
		protected override void OnMouseLeave()				{	m_AnimateBorder(eColor.Transparent); base.OnMouseLeave();}
		protected override void OnPaint(PaintArgs e)
		{	base.OnPaint(e);
			if (m_braBorder is not null && m_braBorder.Value.Color != eColor.Transparent)
				e.Graphics.DrawRectangle(ContentRectangle, m_braBorder.Value);
		}
		protected override void OnClick()					{	m_AnimateSize(); base.OnClick();}
		private void m_AnimateBorder(Color clrEnd)
		{	if (m_braBorder is null)	m_braBorder = new cBrushAnimation(eColor.Transparent);
			m_stoBorder?.Dispose();
			m_stoBorder = new cStoryboard(this);
				m_stoBorder.AddTransition(new cColorTransition(0.3f, clrEnd), m_braBorder);
				m_stoBorder.Start();
		}
		private void m_AnimateSize()
		{	if (m_fanResize is null)	{	m_fanResize = new cFloatAnimation(); m_ptSize = Size;}
			m_ptCenter = Center;
			m_stoSize?.Dispose();
			m_stoSize = new cStoryboard(() =>
				{	Size = m_ptSize * m_fanResize.Value;
					Center = m_ptCenter;
					return true;
				});
				m_stoSize.AddTransition(new cFloatTransition(0.25f, 0.25f, 1, 1.2f, true), m_fanResize);
				m_stoSize.Start();
		}
	}
	cStoryboard? m_stoTutor; readonly cPicture picArrow;
	cStoryboard? m_stoLinear; readonly cGeometryControl gctGeom;
	readonly cBitmapAnimation banBitmap;
	readonly cButton btnBitmap;
public fAnim()
{	cTabControl tc; cTabControl.cTab tab; cLabel lbl; cTextBox txt; cButton btn;

	Text = "Animation";
	tc = new cTabControl {	Margins = new Rect(5)};
	tab = tc.Tabs.Add("Tutorial");
		lbl = new cLabel {	Text = "Name", LocationMargin = new Point(50, 50)};
			tab.Content.AddControl(lbl);
		txt = new cTextBox {	LocationMargin = new Point(200, 50), Width = 300};
			txt.TextChanged += txtName_TextChanged;
			tab.Content.AddControl(txt);
		lbl = new cLabel {	Text = "Address", LocationMargin = new Point(50, 100)};
			tab.Content.AddControl(lbl);
		txt = new cTextBox {	LocationMargin = new Point(200, 100), Width = 300};
			txt.TextChanged += txtAddress_TextChanged;
			tab.Content.AddControl(txt);
		cBtn btn2 = new () {	Text = "Send", LocationMargin = new Point(200, 150)};
			btn2.Click += btnSend_Click;
			tab.Content.AddControl(btn2);
		btn = new cButton {	Text = "Start", LocationMargin = new Point(50, 350)};
			btn.Click += btnTutor_Click;
			tab.Content.AddControl(btn);
		picArrow = new cPicture {	Size = new Point(30, 20), Bitmap = mRes.BmpArrow, BorderStyle = eBorderStyle.None, Visible = false};
			tab.Content.AddControl(picArrow);
	tab = tc.Tabs.Add("Linear");
		gctGeom = new cGeometryControl {	Size = new Point(100, 50), BackColor = null, StrokeThickness = 5, Visible = false
			, Geometry = new cPathGeometry(new Point[] {	new Point(0, 0), new Point(100, 25), new Point(0, 50)}, true)};
			tab.Content.AddControl(gctGeom);
		btn = new cButton {	Text = "Start", LocationMargin = new Point(50, 350)};
			btn.Click += btnLinear_Click;
			tab.Content.AddControl(btn);
	tab = tc.Tabs.Add("Bitmap");
		cPaintControl pnt = new () {	Size = new Point(150, 150), SliceLocation = new Point(0.5f, 0.5f)
				, BackColor = eBrush.LightGray};
			pnt.Paint += pntBitmap_Paint;
			tab.Content.AddControl(pnt);
		btnBitmap = new cButton {	Text = "Pause", LocationMargin = new Point(50, 350)};
			btnBitmap.Click += btnBitmapPause_Click;
			tab.Content.AddControl(btnBitmap);
		btn = new cButton {	Text = "Open", LocationMargin = new Point(160, 350)};
			btn.Click += btnBitmapOpen_Click;
			tab.Content.AddControl(btn);
		banBitmap = new cBitmapAnimation((object sender) => pnt.Invalidate(pnt.ClientRectangle)) {	Bitmap = mRes.BmpAni};
	AddControl(tc);
}
public override void Close()								{	banBitmap.Bitmap = null; base.Close();}
private void txtName_TextChanged(object sender)
{	if (Equals(m_stoTutor?.Tag, 1))	m_CreateTutorStoryboard(new Point(510, 100), new Point(520, 100), 2);
}
private void txtAddress_TextChanged(object sender)
{	if (Equals(m_stoTutor?.Tag, 2))	m_CreateTutorStoryboard(new Point(310, 150), new Point(320, 150), 3);
}
private void btnSend_Click(object sender)					{	m_stoTutor?.Dispose(); m_stoTutor = null; picArrow.Visible = false;}
private void btnTutor_Click(object sender)
{		if (Equals(m_stoTutor?.Tag, 1))	return;
	m_CreateTutorStoryboard(new Point(510, 50), new Point(520, 50), 1);
}
private void m_CreateTutorStoryboard(Point ptStart, Point ptEnd, int iTag)
{	cPointAnimation ptaVar; cTransition tra;

	ptaVar = new cPointAnimation(ptStart);
	m_stoTutor?.Dispose();
	m_stoTutor = new cStoryboard(() =>
		{		if (picArrow.IsClosed)	return false;
			picArrow.LocationMargin = ptaVar.Value; picArrow.Visible = true; return true;
		}) {	Tag = iTag};
	tra = new cPointTransition(0.5f, ptEnd); m_stoTutor.AddTransition(tra, ptaVar);
	m_stoTutor.RepeatStoryboard(tra, true);
	m_stoTutor.Start();
}
private void btnLinear_Click(object sender)
{	cPointAnimation ptaPos = new (new Point(10, 0), 300);
	cFloatAnimation fanAngle = new () {	Maximum = 360};
	cBrushAnimation braFill = new (eColor.Red), braBorder = new (eColor.Gray);
	cPathGeometry pg = new ((cPathGeometry.Sink snk) =>
		{	snk.BeginFigure(new Point(300, 50), false);
				snk.AddLine(new Point(500, 50));
				snk.AddArc(new Point(200, 150), new Point(50, 50), 45, false, false);
			snk.EndFigure(false);
		});
	cTransition tra;

	gctGeom.LocationMargin = new Point(10, 300); gctGeom.Visible = true;
	m_stoLinear?.Dispose();
	m_stoLinear = new cStoryboard(() =>
		{		if (gctGeom.IsClosed)	return false;
			gctGeom.LocationMargin = ptaPos.Value;							// or gctGeom.Location = ptaPos.Value; then invalidate
			gctGeom.Angle = fanAngle.Value;
			gctGeom.FillColor = braFill.Value;
			gctGeom.StrokeColor = braBorder.Value;
			return true;
		});
	// ** 0
	m_stoLinear.AddTransition(new cPointBounceTransition(2, 200, 300, 2, 2), ptaPos);
	m_stoLinear.AddTransition(new cUserFloatTransition(2
			, (float time, float InitialValue)	=>	180 * (float)Math.Abs(Math.Sin(Math.PI * 2 * time))
			, true)
		, fanAngle);
	m_stoLinear.AddTransition(new cColorTransition(2, eColor.Magenta, 0, 0, 1, 1), braBorder);
	// ** 2
	m_stoLinear.AddTransition(new cPointPathTransition(4, pg), ptaPos);
	m_stoLinear.AddTransition(new cFloatPathAngleTransition(4, pg), fanAngle);
	m_stoLinear.AddTransitionAt(new cColorUserTransition(2, (float time, Color InitialValue) =>
		{	float fCycle = (float)Math.Abs(Math.Sin(Math.PI / 3 * time));
			return Color.FromHSL(fCycle, 1, 0.5f);
		}, true), braFill, 2);
	m_stoLinear.AddTransition(new cColorTransition(2, eColor.Cyan, 0, 0, 1, 1), braBorder);
	// ** 4
	m_stoLinear.AddTransition(new cColorTransition(2, eColor.Green, 0, 0, 1, 1), braFill);
	tra = new cColorTransition(2, eColor.Salmon, 0, 0, 1, 1); m_stoLinear.AddTransition(tra, braBorder);
	m_stoLinear.RepeatStoryboard(tra, true);
	m_stoLinear.Start();
}
private void pntBitmap_Paint(object sender, PaintArgs e)
{	e.Graphics.DrawAnimation(banBitmap, ((cPaintControl)sender).ClientRectangle, eAlignment.None, eResize.ScaleIfBig);
}
private void btnBitmapPause_Click(object sender)
{	banBitmap.Enabled = !banBitmap.Enabled; btnBitmap.Text = (banBitmap.Enabled ? "Pause" : "Continue");
}
private void btnBitmapOpen_Click(object sender)
{	string? sFile;

	sFile = mDialog.ShowOpenFile(mMod.DLG_IMG_EXTS, mMod.DLG_IMG_GUID);	if (sFile is null) return;
	banBitmap.Bitmap = new cBitmap(sFile);
}
}