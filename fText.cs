using Wew.Control;
using Wew.Media;

namespace Demo;

class fText : cDockControl
{	class cRectangleProvider : cTextLayout.cRectangleProvider
	{	readonly float m_fWidth; float m_fY;
		public static new cTextLayout.Tab[] Tabs = new cTextLayout.Tab[]
			{	new cTextLayout.Tab(1, eTextAlignment.Right), new cTextLayout.Tab(10), new cTextLayout.Tab(200)
			};
		public cRectangleProvider(float width)
		{	m_fWidth = width; Tabs[0].Size = m_fWidth / 2 - 5;
		}
		public override void Reset()
		{	m_fY = 0; base.Tabs = null;
			base.Reset();
		}
		public override void OnLineAdded(Rectangle rectangle, bool IsParagraphEnd, int EndCharIndex)
		{	m_fY += rectangle.Height;
		}
		protected override System.Collections.Generic.IEnumerator<Rectangle> r_GetRectangles()
		{	yield return new Rectangle(0, m_fY, m_fWidth, 20);
			yield return new Rectangle(0, m_fY, m_fWidth, 20);
			yield return new Rectangle(0, m_fY, m_fWidth, 20);
			Alignment = eTextAlignment.Center;
				yield return new Rectangle(0, m_fY, m_fWidth, 20);
			base.Tabs = Tabs;
				while (true)	yield return new Rectangle(0, m_fY, m_fWidth, 20);
		}
	}
	class cLayoutLbl : cLabel
	{	public new fText Parent;
		static readonly cBrush s_brTab = new cSolidBrush(eColor.Yellow, 0.5f);
		static readonly cBrush s_brAncho = new cSolidBrush(eColor.Cyan, 0.5f);
		protected override void OnPaint(PaintArgs e)
		{	//float fBottom = ClientRectangle.Bottom;
			//float fLastTab = 0, fTabs = 0;

			base.OnPaint(e);
			//foreach (cTextLayout.Tab tab in cRectangleProvider.sa_tabTabs)
			//{	fLastTab = tab.Size; fTabs += fLastTab; e.Graphics.DrawVerticalLine(fTabs, 1, fBottom, eBrush.Yellow);
			//}
			//fTabs += fLastTab; e.Graphics.DrawVerticalLine(fTabs, 1, fBottom, s_brTab);
			//e.Graphics.DrawVerticalLine(Parent.m_fLayoutWidth, 1, fBottom, s_brAncho);
			e.Graphics.DrawRectangle(Parent.m_rtLayoutChar, eBrush.Blue);
		}
	}
	readonly cTextLayout m_tlShapes, m_tlShapes2; float m_fShapesY; bool m_bDrawShapes2;
		Rectangle m_rtShapesChar; readonly cPaintControl pntShapes;
	readonly cLayoutLbl lblLayout; Rectangle m_rtLayoutChar; float m_fLayoutWidth;
	readonly cEditControl edtText;
public fText()
{	cTabControl tc; cTabControl.cTab tab; cButton btn; string s;

	Text = "Text";
	tc = new cTabControl {	Margins = new Rect(5)};
	tab = tc.Tabs.Add("Shapes");
		pntShapes = new cPaintControl {	Margins = new Rect(10, 10, 10, 40)};
			pntShapes.Paint += pntShapes_Paint;
			pntShapes.MouseDown += pntShapes_MouseDown;
			tab.Content.AddControl(pntShapes);
		btn = new cButton {	Text = "DX", LeftMargin = 10, TopMargin = float.NaN, BottomMargin = 10};
			btn.Click += btnDX_Click;
			tab.Content.AddControl(btn);
		btn = new cButton {	Text = "Glyph", LeftMargin = 120, TopMargin = float.NaN, BottomMargin = 10};
			btn.Click += btnGlyph_Click;
			tab.Content.AddControl(btn);
		btn = new cButton {	Text = "Columns", LeftMargin = 230, TopMargin = float.NaN, BottomMargin = 10};
			btn.Click += btnCols_Click;
			tab.Content.AddControl(btn);
		btn = new cButton {	Text = "Newspaper", LeftMargin = 340, TopMargin = float.NaN, BottomMargin = 10};
			btn.Click += btnNews_Click;
			tab.Content.AddControl(btn);
	tab = tc.Tabs.Add("Layout");
		lblLayout = new cLayoutLbl {	Parent = this, Margins = new Rect(10, 10, 10, 50), AutoSize = eAutoSize.None
					, Text = "Arial 15pt, bold and italic.  Gabriola 20pt, oblique, with superscript (a), subscript (b) and fraction (2/3)."
						+ " Object * Background and highlight.  Outline.  Error.  Compound char (A\u0308)\n\n"
						+ "Cast\n\n"
						+ "Actor		John Doe\n"
						+ "Producer		Peter Smith\n"
						+ "Director		Jane Jones\n"
						+ "Assistant		Mary Davis\n"
						+ "😁😂😃😄😅😆😇😈😉😊😋😌😍😎😏😐😒😓😔😖😘😚😜😝😞😠😡😢😣😤😥😨😩😪😫😭😰😱😲😳😵😶😷"
					, Font = new cFont("segoe ui emoji", 12), LineAlignment = eParagraphAlignment.Top, BorderStyle = eBorderStyle.Default
					, Wrapping = eWrapping.Wrap
				};
			lblLayout.MouseDown += lblLayout_MouseDown;
			lblLayout.MouseUp += lblLayout_MouseUp;
			lblLayout.MouseMove += lblLayout_MouseMove;
			tab.Content.AddControl(lblLayout);
		btn = new cButton {	Text = "Layout", LeftMargin = 10, TopMargin = float.NaN, BottomMargin = 10};
			btn.Click += btnLayout_Click;
			tab.Content.AddControl(btn);
	tab = tc.Tabs.Add("Edit control");
		edtText = new cEditControl
			{	Text = "0 sdd  qwert aaa/bbb rty BB qwe asdfg\tcc5 rrt6 77\u263a 😁 878 99 ere	ert 787	\t990 ty66 tyuu eeB\n1 qwe ghgY\n2 tt ty yyt\trtr\te\trt eer zz qwtyyR\n3	rt s	2	34	vctJK\n4 XC"
					, Font = new cFont("segoe ui symbol", 16)//gabriola
					, TextColor = eBrush.Red
					, Width = 200//, Wrapping = eWrapping.Wrap, AutoSize = eAutoSize.None
					, TextAlignment = eTextAlignment.Left //, RightMargin = 10
					, LeftMargin = 10, TopMargin = 10, RightMargin = 10, BottomMargin = 80, HandlesTab = true
			};
			edtText.SelectionChanged += txt_SelectionChanged;
			tab.Content.AddControl(edtText);
		btn = new cButton {	Text = "Spell check", LeftMargin = 10, TopMargin = float.NaN, BottomMargin = 10};
			btn.Click += btnSpell_Click;
			tab.Content.AddControl(btn);
	AddControl(tc);
	tc.SelectedIndex = 2;
	s = "DirectWrite provides factored layers of functionality, with each layer interacting seamlessly with the next. "
#if true
            + "The API design gives an application the freedom and flexibility to adopt individual layers depending on their needs and schedule.\n"
            + "The text layout API provides the highest level functionality available from DirectWrite. "
            + "It provides services for the application to measure, display, and interact with richly formatted text strings. "
            + "This text API can be used in applications that currently use Win32’s DrawText to build a modern UI with richly formatted text.\n"
            + "* Text-intensive applications that implement their own layout engine may use the next layer down: the script processor. "
            + "The script processor segments text into runs of similar properties and handles the mapping from Unicode codepoints "
            + "to the appropriate glyph in the font.\n"
            + "DirectWrite's own layout is built upon this same font and script processing system. "
            + "This sample demonstrates how a custom layout can utilize the information from script itemization, bidi analysis, line breaking analysis, and shaping, "
            + "to accomplish text measurement/fitting, line breaking, basic justification, and drawing.\n"
            + "The glyph-rendering layer is the lowest layer and provides glyph-rendering functionality for applications "
            + "that implement their own complete text layout engine. The glyph rendering layer is also useful for applications that implement a custom "
            + "renderer to modify the glyph-drawing behavior through the callback function in the DirectWrite text-formatting API.\n"
            + "The DirectWrite font system is available to all the functional layers, and enables an application to access font and glyph information. "
            + "It is designed to handle common font technologies and data formats. The DirectWrite font model follows the common typographic practice of "
            + "supporting any number of weights, styles, and stretches in the same font family. This model, the same model followed by WPF and CSS, "
            + "specifies that fonts differing only in weight (bold, light, etc.), style (upright, italic, or oblique) or stretch (narrow, condensed, wide, etc.) "
            + "are considered to be members of a single font family.\n"
            + "Text in DirectWrite is rendered using Microsoft® ClearType®, which enhances the clarity and readability of text. "
            + "ClearType takes advantage of the fact that modern LCD displays have RGB stripes for each pixel that can be controlled individually. "
            + "DirectWrite uses the latest enhancements to ClearType, first included with Windows Vista® with Windows Presentation Foundation, "
            + "that enables it to evaluate not just the individual letters but also the spacing between letters. "
            + "Before these ClearType enhancements, text with a \"reading\" size of 10 or 12 points was difficult to display: "
            + "we could place either 1 pixel in between letters, which was often too little, or 2 pixels, which was often too much. "
            + "Using the extra resolution in the subpixels provides us with fractional spacing, which improves the evenness and symmetry of the entire page.\n"
            + "The subpixel ClearType positioning offers the most accurate spacing of characters on screen, "
            + "especially at small sizes where the difference between a sub-pixel and a whole pixel represents a significant proportion of glyph width. "
            + "It allows text to be measured in ideal resolution space and rendered at its natural position at the LCD color stripe, subpixel granularity. "
            + "Text measured and rendered using this technology is, by definition, "
            + "resolution-independent—meaning the exact same layout of text is achieved across the range of various display resolutions.\n"
            + "Unlike either flavor of GDI's ClearType rendering, sub-pixel ClearType offers the most accurate width of characters. "
            + "The Text String API adopts sub-pixel text rendering by default, which means it measures text at its ideal resolution independent "
            + "to the current display resolution, and produces the glyph positioning result based on the truly scaled glyph advance widths and positioning offsets."
#endif
			;
	m_tlShapes = new cTextLayout(s, new cFont("segoe ui symbol", 8)) {	Wrapping = eWrapping.Wrap, Alignment = eTextAlignment.Justified};
		m_tlShapes.SetColor(eBrush.Blue, 1162, 159);
	m_tlShapes2 = new cTextLayout(s, new cFont("arial", 14), 1162, 159) {	Wrapping = eWrapping.Wrap};
}
private void pntShapes_Paint(object sender, PaintArgs e)
{	e.Graphics.DrawTextLayout(m_tlShapes, new Point(0, m_fShapesY), eBrush.Black);
	e.Graphics.DrawRectangle(m_rtShapesChar, eBrush.Blue);
	if (m_bDrawShapes2)	e.Graphics.DrawTextLayout(m_tlShapes2, default, eBrush.Blue);
}
private void pntShapes_MouseDown(object sender, MouseArgs e)
{	int index, line;

	m_rtShapesChar.Location = e.Location; m_rtShapesChar.Y -= m_fShapesY;
		_ = m_tlShapes.HitTest(ref m_rtShapesChar.Location, out index, out _, out _);
		m_rtShapesChar.Y += m_fShapesY;
		m_rtShapesChar.Width = m_tlShapes.CharToPoint(index, out line, true).X - m_rtShapesChar.X;
		m_rtShapesChar.Height = m_tlShapes[line].Bounds.Height;
	pntShapes.Invalidate();
}
private void btnDX_Click(object sender)
{	cTextLayout.cGeometryRectangleProvider grp = new (
			new cTextLayout("DX", new cFont("arial", 370, eFontWeight.ExtraBlack)).GetGeometry(0), 25)
		{	Alignment = eTextAlignment.Justified
		};

	m_tlShapes.PerformLayout(grp); m_fShapesY = 360;
	m_bDrawShapes2 = false;
	m_rtShapesChar = default; pntShapes.Invalidate();
}
private void btnGlyph_Click(object sender)
{	cTextLayout.cGeometryRectangleProvider grp = new (
			new cFont("segoe ui symbol", 400).GetGlyphGeometry(1450), 25)
		{	Alignment = eTextAlignment.Justified
		};

	m_tlShapes.PerformLayout(grp); m_fShapesY = 420;
	m_bDrawShapes2 = false;
	m_rtShapesChar = default; pntShapes.Invalidate();
}
private void btnCols_Click(object sender)
{	cTextLayout.cColumRectangleProvider crp = new (300, 320, 20)
		{	Alignment = eTextAlignment.Justified
		};

	m_tlShapes.PerformLayout(crp); m_fShapesY = 0;
	m_bDrawShapes2 = false;
	m_rtShapesChar = default; pntShapes.Invalidate();
}
private void btnNews_Click(object sender)
{	using cPathGeometry pgGeo = new((cPathGeometry.Sink snk)
		=> snk.AddCombinedGeometries(new cRectangleGeometry(new Rectangle(0, 0, 1200, 320))
			, new cEllipseGeometry(new Rectangle(180, 70, 250, 250))
			, cPathGeometry.eCombine.Exclude));
	cTextLayout.cGeometryRectangleProvider grp = new (pgGeo, 25, 300, 20)
		{	Alignment = eTextAlignment.Justified
		};
	cTextLayout.cGeometryRectangleProvider grp2 = new (new cEllipseGeometry(new Rectangle(200, 100, 210, 210)), 50)
		{	Alignment = eTextAlignment.Center
		};

	m_tlShapes.PerformLayout(grp); m_fShapesY = 0;
	m_tlShapes2.PerformLayout(grp2); m_bDrawShapes2 = true;
	m_rtShapesChar = default; pntShapes.Invalidate();
}
private void lblLayout_MouseDown(object sender, MouseArgs e)
{	cTextLayout tl = lblLayout.TextLayout; int index, line;

	tl.OnMouseDown(e);
	m_rtLayoutChar.Location = e.Location;
		_ = tl.HitTest(ref m_rtLayoutChar.Location, out index, out _, out _);
		m_rtLayoutChar.Width = tl.CharToPoint(index, out line, true).X - m_rtLayoutChar.X;
		m_rtLayoutChar.Height = tl[line].Bounds.Height;
	lblLayout.Invalidate();
}
private void lblLayout_MouseUp(object sender, MouseArgs e)	{	lblLayout.TextLayout.OnMouseUp(e);}
private void lblLayout_MouseMove(object sender, MouseArgs e)	{	lblLayout.TextLayout.OnMouseMove(e);}
private void btnLayout_Click(object sender)
{	cTextLayout tl = lblLayout.TextLayout; cTextLayout.cPicture pic;

	tl.SetFont(new cFont("arial", 15), 0, 28);
		tl.SetFont(new cFont("arial", 15, eFontWeight.Bold), 12, 4);
		tl.SetFont(new cFont("arial", 15, eFontWeight.Normal, eFontStyle.Italic), 21, 6);
	tl.SetFont(new cFont("gabriola", 20), 28, 81);
		tl.SetFontFeatures(new FontFeature[] {	new FontFeature(eFontFeature.StylisticSet7, 1)}, 28, 8);
		tl.SetFont(new cFont("gabriola", 20, eFontWeight.Normal, eFontStyle.Oblique), 45, 7);
		tl.SetUnderline(59, 15);
			tl.SetScriptPosition(cTextLayout.eScriptPosition.Superscript, 72, 1);
				tl.SetColor(eBrush.Blue, 72, 1);
		tl.SetUnderline(76, 13);
			tl.SetStrikethrough(76, 13);
			tl.SetScriptPosition(cTextLayout.eScriptPosition.Subscript, 87, 1);
				tl.SetColor(eBrush.Red, 87, 1);
			tl.SetFontFeatures(new FontFeature[] {	new FontFeature(eFontFeature.Fractions, 1)}, 104, 3);
		pic = new cTextLayout.cPicture(mRes.BmpSmile, new Point(18, 18), 16);
			pic.Click += picSmile_Click;
			tl.SetControl(pic, 117);
		tl.SetBackColor(new cSolidBrush(eColor.Yellow, 0.5f), 119, 25);
			tl.SetSelection(cTextLayout.SelectionInactive, 130, 3);
			tl.SetSelection(new cTextLayout.cSelection(eBrush.SystemSelection, eBrush.Gray, new cLineStyle(eDash.Dot, eCapStyle.Square, eLineJoin.Miter), 2)
					, 134, 9);
				tl.SetUnderline(new cTextLayout.cStraightLine(eBrush.Green, 4, 7), 119, 25);
	tl.SetFont(new cFont("arial", 40, eFontWeight.Bold), 146, 7);
		tl.SetColor(new cRadialGradientBrush(eColor.Yellow, eColor.Red, eExtendMode.Mirror) {	Radius = new Point(20, 20)}
			, 146, 7);
		tl.SetOutline(new cTextLayout.cOutline(eBrush.Green, new cLineStyle(eDash.Dash, eCapStyle.Triangle, eLineJoin.Miter), 2), 146, 7);
	tl.SetUnderline(new cTextLayout.cStraightLine(Wew.eResource.BrushErrorUnderline, 5), 156, 5);
	tl.SetFont(new cFont("arial", 15, eFontWeight.Bold), 184, 4);
		tl.SetUnderline(184, 4);
	tl.PerformLayout(new cRectangleProvider(lblLayout.Width)); m_fLayoutWidth = lblLayout.Width; lblLayout.CustumTextLayoutShape = true;
	lblLayout.Invalidate();
}
private void picSmile_Click(object sender)					{	mDialog.MsgBoxInformation("Click", "Layout");}
private void txt_SelectionChanged(object sender)			{	wMain.Stat2 = edtText.SelectionRange.Location.ToString();}
private void btnSpell_Click(object sender)
{	Wew.cSpellCheck sc = Wew.cSpellCheck.GetSpellChecker(Wew.cSpellCheck.SupportedLanguages[^1]);
	string str = "la árto casí"; //¿sc.AutoCorrect("casí", 0, -1, "cosí", 0, -1);
	foreach (Wew.SpellError ser in sc.Check(str))
	{	cMenu mnu = new ();
	
		foreach (string s in sc.GetSuggestions(str, ser.Index, ser.Count))	_ = mnu.Items.Add(s);
		mnu.Show((cButton)sender); mDialog.MsgBoxInformation("", "");
	}
}
}