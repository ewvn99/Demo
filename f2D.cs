using Wew.Control;
using Wew.Media;

namespace Demo;

class f2D : cDockControl
{	cBitmap m_bmpImg; bool m_bDisposeBmp;
	readonly c2DParticle m_ptcParticle;
	readonly cInk m_inkInk; readonly cInkStyle m_inkInkStyle; readonly cSolidBrush m_brInk; Point m_ptLastInkPt;
	readonly cPathGeometry m_pagLayer;
	readonly cPathGeometry m_pagPrim;
		readonly cGeometryGroup m_ggrPrimGroup;
		readonly cBitmapBrush m_brPrim;
	readonly cComboBox cboEffects; readonly cPaintControl pntEffects;
	public static void RegisterEffects()
	{	cCustomVertexEfImplem.Register();
		cCustomShadowEfImplem.Register();
		cCustomTileEfImplem.Register();
	}
public f2D()
{	cTabControl tc; cTabControl.cTab tab; cButton btn; cPaintControl pnt, pntPart;

	Text = "2D";
	tc = new cTabControl {	Margins = new Rect(5)};
	tab = tc.Tabs.Add("Effects");
		float[] a_f = new float[] {	0, 0.15f, 0.65f, 0.75f, 0.9f};
		cboEffects = new cComboBox {	LocationMargin = new Point(10, 10), Width = 300};
			cboEffects.Load(new cSingleInputEffect?[]
					{	null
						, new cBrightnessEffect { WhitePoint = new Point(0.5f, 1)}
						, new cColorMatrixEffect {	Matrix = new Matrix5x4(0.213f, 0.213f, 0.213f, 0,  0.715f, 0.715f, 0.715f, 0,  0.072f, 0.072f, 0.072f, 0,  0, 0, 0, 1,  0, 0, 0, 0)}
						, new cColorMatrixEffect {	Matrix = new Matrix5x4(-1, 0, 0, 0,  0, -1, 0, 0,  0, 0, -1, 0,  0, 0, 0, 1,  1, 1, 1, 0)}
						, new cColorMatrixEffect {	Matrix = new Matrix5x4(1, 0, 0, 0,  0, 1, 0, 0,  0, 0, 0, 0,  0, 0, 0, 1,  0, 0, 0, 0)}
						, new cDiscreteTransferEffect {	Red = a_f, Green = a_f, Blue = a_f}
						, new cGaussianBlurEffect()
						, new cDirectionalBlurEffect {	Deviation = 20, Angle = 30}
						, new cEmbossEffect()
						, new cWaveEffect {   Offset = 0.5f, Skew = new Point(0.1f, 0.1f)}
						, new cRippleEffect()
						, new cBendEffect {	Distance = 0.6f}
						, new cWhirlEffect {	Spin = 0.5f}
						, new cBubbleEffect {	Flatten = 0.8f, Light = 0.1f}
						, new cDFTEffect()
						, new cCustomVertexEf()
						, new cCustomShadowEf {   Offset = new Point(100, 100), Color = eColor.Tomato}
						, new cCustomTileEf {   Region = new Rectangle(0.2f, 0.3f, 0.3f, 0.4f)}
					});
			cboEffects.SelectedIndex = 0;
			cboEffects.Items[0].Text = "None";
			cboEffects.Items[1].Text = "Brightness";
			cboEffects.Items[2].Text = "Black and white";
			cboEffects.Items[3].Text = "Negative";
			cboEffects.Items[4].Text = "Yellow channel";
			cboEffects.Items[5].Text = "Painting";
			cboEffects.Items[6].Text = "Gaussian blur";
			cboEffects.Items[7].Text = "Directional blur";
			cboEffects.Items[8].Text = "Emboss";
			cboEffects.Items[9].Text = "Wave";
			cboEffects.Items[10].Text = "Ripple";
			cboEffects.Items[11].Text = "Bend";
			cboEffects.Items[12].Text = "Whirl";
			cboEffects.Items[13].Text = "Bubble";
			cboEffects.Items[14].Text = "Discrete Fourier Transform";
			cboEffects.Items[15].Text = "Custom vertex";
			cboEffects.Items[16].Text = "Custom shadow";
			cboEffects.Items[17].Text = "Custom tile";
			cboEffects.SelectionChanged += cboEffect_SelectionChanged;
			tab.Content.AddControl(cboEffects);
		pntEffects = new cPaintControl {	 Margins = new Rect(10, 40, 10, 40)};
			pntEffects.Paint += pntEffect_Paint;
			tab.Content.AddControl(pntEffects);
		btn = new cButton {	Text = "Open", Margins = new Rect(10, float.NaN, float.NaN, 10)};
			btn.Click += btnBitmapOpen_Click;
			tab.Content.AddControl(btn);
	tab = tc.Tabs.Add("Particles");
		pntPart = new cPaintControl {	 Margins = new Rect(10)};
			pntPart.Paint += pntParticle_Paint;
			tab.Content.AddControl(pntPart);
	tab = tc.Tabs.Add("Ink");
		pnt = new cPaintControl {	 Margins = new Rect(10)};
			pnt.Paint += pntInk_Paint;
			pnt.MouseMove += pntInk_MouseMove;
			tab.Content.AddControl(pnt);
	tab = tc.Tabs.Add("Layer");
		pnt = new cPaintControl {	 Margins = new Rect(10)};
			pnt.Paint += pntLayer_Paint;
			tab.Content.AddControl(pnt);
	tab = tc.Tabs.Add("Primitives");
		pnt = new cPaintControl {	 Margins = new Rect(10)};
			pnt.Paint += pntPrim_Paint;
			tab.Content.AddControl(pnt);
	AddControl(tc);
	// ** Effects
	m_bmpImg = mRes.BmpPhoto;
	// ** Particles
	m_ptcParticle = new c2DParticle(wMain.Device);							// ** Create
	_ = m_ptcParticle.Add(200);
		for (int i = 0; i < m_ptcParticle.Count; i++)
		{	ref c2DParticle.Particle ptc = ref m_ptcParticle.Particles[i];

			ptc.Center = new Point(900 * Wew.mMath.Random(), 300 * Wew.mMath.Random());
			ptc.Size = new Point(20 + 20 * Wew.mMath.Random(), 20 + 20 * Wew.mMath.Random());
			ptc.OrientationAngle = Wew.mMath.Random() * 360;
			ptc.Color = Color.Random();
			ptc.Source = new Rectangle(500 * Wew.mMath.Random(), 500 * Wew.mMath.Random()
				, 1000 + 500 * Wew.mMath.Random(), 1000 + 500 * Wew.mMath.Random());
		}
		m_ptcParticle.Update(0, -1, true, true, true);
	cFloatAnimation faVar = new();											// ** Animate
	cStoryboard stoParticle = new(() =>
		{		if (m_ptcParticle.IsDisposed)	return false;
				if (tc.SelectedIndex != 1)	return true;					// Hidden: avoid calculations, exit
			for (int i = 0; i < m_ptcParticle.Count; i++)
			{	ref c2DParticle.Particle ptc = ref m_ptcParticle.Particles[i];

				ptc.OrientationAngle += faVar.Value;
				Color clr = ptc.Color + new Color(0.01f, 0.01f, 0.01f, 0.001f);
					if (clr.R > 1)	clr.R = 0;
					if (clr.G > 1)	clr.G = 0;
					if (clr.B > 1)	clr.B = 0;
					if (clr.A > 1)	clr.A = 0;
					ptc.Color = clr;
				Rectangle rt = ptc.Source;
					rt.Location += new Point(5, 5);
					if (rt.X > 1500)	rt.X = 0;
					if (rt.Y > 1500)	rt.Y = 0;
					ptc.Source = rt;
			}
			m_ptcParticle.Update(0, -1, true, true);
			pntPart.Invalidate();
			return true;
		});
		cTransition tra = new cFloatTransition(2, 10);
			stoParticle.AddTransition(tra, faVar);
		stoParticle.RepeatStoryboard(tra, true);
		stoParticle.Start();
	// ** Ink
	m_ptLastInkPt = new Point(50, 50);
	m_inkInk = new cInk(wMain.Device, new InkPoint(m_ptLastInkPt, 10));
	m_inkInkStyle = new cInkStyle(wMain.Device)
		{	NibShape = eInkNibShape.Square, NibTransform = Matrix3x2.FromRotation(5) * Matrix3x2.FromScale(0.1f, 1)
		};
	m_brInk = new cSolidBrush(eColor.Red, 0.5f);
	// ** Layers
	m_pagLayer = new cTextLayout("A", new cFont("arial", 400, eFontWeight.ExtraBlack)).GetGeometry(0);
	// ** Primitives
	using cEllipseGeometry eg1 = new(new Rectangle(5, 5, 100, 100));
		using cEllipseGeometry eg2 = new(new Rectangle(50, 50, 100, 100));
		m_pagPrim = new cPathGeometry((sink) =>	sink.AddCombinedGeometries(eg1, eg2, cPathGeometry.eCombine.Exclude));
	using cEllipseGeometry eg3 = new(new Rectangle(80, 80, 240, 240));
		using cEllipseGeometry eg4 = new(new Rectangle(100, 100, 200, 200));
		using cEllipseGeometry eg5 = new(new Rectangle(120, 120, 160, 160));
		using cEllipseGeometry eg6 = new(new Rectangle(140, 140, 220, 120));
		m_ggrPrimGroup = new cGeometryGroup(true, eg3, eg4, eg5, eg6);
	m_brPrim = new cBitmapBrush(mRes.BmpPhoto);
}
public override void Close()
{	foreach (cListBox.cItem it in cboEffects.Items)	((cSingleInputEffect?)it.Value)?.Dispose();
	if (m_bDisposeBmp)	m_bmpImg.Dispose();
	m_ptcParticle?.Dispose();
	m_inkInk?.Dispose(); m_inkInkStyle?.Dispose(); m_brInk?.Dispose();
	m_pagLayer?.Dispose();
	base.Close();
}
private void cboEffect_SelectionChanged(object sender)		{	pntEffects.Invalidate();}
private void pntEffect_Paint(object sender, PaintArgs e)
{	e.Graphics.DrawBitmap(m_bmpImg, pntEffects.ClientRectangle, eAlignment.Center, eResize.ScaleIfBig, 1, eInterpolation.Linear
		, (cSingleInputEffect?)cboEffects.SelectedValue);
}
private void btnBitmapOpen_Click(object sender)
{	string? sFile; cBitmap bmp;

	sFile = mDialog.ShowOpenFile(mMod.DLG_IMG_EXTS, mMod.DLG_IMG_GUID);	if (sFile is null) return;
	bmp = new cBitmap(sFile);	if (m_bDisposeBmp)	m_bmpImg.Dispose();
	m_bmpImg = bmp; m_bDisposeBmp = true;
	pntEffects.Invalidate();
}
private void pntParticle_Paint(object sender, PaintArgs e)	{	e.Graphics.DrawParticle(m_bmpImg, m_ptcParticle);}
private void pntInk_Paint(object sender, PaintArgs e)
{	e.Graphics.Antialias = true; e.Graphics.DrawInk(m_inkInk, m_brInk, m_inkInkStyle);
}
private void pntInk_MouseMove(object sender, MouseArgs e)
{		if (e.Button != eMouseButton.Left)	return;
	m_inkInk.Add(new InkSegment((m_ptLastInkPt * 2 + e.Location) / 3, 5
		, (m_ptLastInkPt + e.Location * 2) / 3, 5, e.Location, 5));//¿
	m_ptLastInkPt = e.Location;
	((cPaintControl)sender).Invalidate();
}
private void pntLayer_Paint(object sender, PaintArgs e)
{	cPaintControl pnt = (cPaintControl)sender; cGraphics g = e.Graphics;
	using cBrush br = new cRadialGradientBrush(eColor.Red, eColor.Transparent, eExtendMode.Mirror) {	Radius = new Point(150, 150)};
	
	g.PushLayer(null, m_pagLayer, Matrix3x2.FromTranslation(0, 400), true, 0.5f, br);
try
{		g.DrawBitmap(m_bmpImg, pnt.ClientRectangle, new Rectangle(300, 800, 500, 500));
		g.DrawText("String", new Rectangle(310, 150, 400, 300), eBrush.White, eFont.SystemBoldText, eTextFormat.Default);
} finally
{	g.PopLayer();
}
}
private void pntPrim_Paint(object sender, PaintArgs e)
{	cGraphics g = e.Graphics;

	g.Antialias = true;
	g.FillGeometry(m_pagPrim, eBrush.LightGreen); g.DrawGeometry(m_pagPrim, eBrush.Red);
	g.FillGeometry(m_ggrPrimGroup, eBrush.LightBlue); g.DrawGeometry(m_ggrPrimGroup, eBrush.Blue);
	g.FillMask(new Rectangle(400, 100, 100, 200), m_brPrim, new Rectangle(0, 0, 16, 32), Wew.eResource.BmpCut);
}
}
class cCustomVertexEfImplem : cEffectImplementation							// ** Implementation
{	public static readonly System.Guid CLSID = new (0x94cecb9, 0x76b9, 0x4cd8, 0xbe, 0x1c, 0x89, 0xb4, 0x5a, 0xf6, 0xd0, 0xa3);
	class cVertexTransform : cCustomDrawTransform
	{	static readonly System.Guid GUID_VERTEX_SHADER = new (0xd7023024, 0x5eb9, 0x4d00, 0x93, 0x32, 0x12, 0x4d, 0x6d, 0x40, 0xfa, 0x8d);
		static readonly System.Guid GUID_VERTEX_BUFFER = new (0x8906b492, 0xde9b, 0x4f02, 0x85, 0x5d, 0x36, 0xc0, 0x6e, 0xe8, 0xb9, 0xdf);
		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct ConstantBuffer
		{	public Matrix4x4 MatPitch;
			public Point Size;
			public float Radius, Circles;
		}
		cVertexBuffer m_vbVertexBuffer;
		public ConstantBuffer Properties;
		static readonly Point[] sa_ptMesh = sa_ptGenerateMesh();
		static Point[] sa_ptGenerateMesh()
		{
			const int TessellationAmount = 32;
			
			int numVertices = 6 * TessellationAmount * TessellationAmount;

			Point[] mesh = new Point[numVertices];
			
			float offset = 1.0f / TessellationAmount;

			for (int i = 0; i < TessellationAmount; i++)
			{
				for (int j = TessellationAmount - 1; j >= 0; j--)
				{
					int index = (i * TessellationAmount + j) * 6;

					// This mesh consists of pairs of triangles forming squares. Each square contains
					// six vertices, three for each triangle. 'offset' represents the distance between each vertice
					// in the triangles. In this mesh, we only set the x and y coordinates of the vertices, since
					// they are the only variable part of the geometry. In the vertex shader, z is generated
					// based on x, and w is defined to be '1'. The actual coordinates here range from 0 to 1,
					// these values are scaled up based on the size of the image in the vertex shader.

					mesh[index].X     = i * offset;
					mesh[index].Y     = j * offset;
					mesh[index + 1].X = i * offset;
					mesh[index + 1].Y = j * offset + offset;
					mesh[index + 2].X = i * offset + offset;
					mesh[index + 2].Y = j * offset;
					mesh[index + 3].X = i * offset + offset;
					mesh[index + 3].Y = j * offset;
					mesh[index + 4].X = i * offset;
					mesh[index + 4].Y = j * offset + offset;
					mesh[index + 5].X = i * offset + offset;
					mesh[index + 5].Y = j * offset + offset;
				}
			}

			return mesh;
		}
		public cVertexTransform(Context context) : base(1)
		{	m_vbVertexBuffer = context.CreateVertexBuffer(GUID_VERTEX_SHADER, mShaders.VS2DVertex
				, GUID_VERTEX_BUFFER, sa_ptMesh, Point.SIZE_OF, sa_ptMesh.Length
				, new VertexField("MESH_POSITION", 0, eVertexFieldType.Point)
				);
			Properties.MatPitch = Matrix4x4.FromRotation(-60, 0, 0).GetTransposed();
			Properties.Radius = 0.05f; Properties.Circles = 2;
		}
		protected override void Dispose(bool disposing)
		{	if (disposing)	m_vbVertexBuffer.Dispose();
			m_vbVertexBuffer = null!;
			base.Dispose(disposing);
		}
		public void Update()								{	r_SetVertexShaderConstantBuffer(Properties);}
		protected override void OnSetInfo()					{	r_SetVertexProcessing(m_vbVertexBuffer, GUID_VERTEX_SHADER);}
		protected override void OnInputSizeChanged()		{	Properties.Size = (Point)r_InputRect.Size; Update();} // ** Update
	}
	cVertexTransform m_traTrans;
	public static void Register()
	{	Register(CLSID, "Custom Vertex"										// Id and name
			, (out System.IntPtr implementation) => new cCustomVertexEfImplem().r_CreateProxy(out implementation) // Factory
			, 1																// Number of inputs
			);
	}
	protected override void Dispose(bool disposing)
	{	if (disposing)	m_traTrans?.Dispose();
		m_traTrans = null!;
		base.Dispose(disposing);
	}
	protected override void OnInitialize(Context context)
	{	m_traTrans = new cVertexTransform(context);
		r_SetSingleTransform(m_traTrans);
	}
	protected override void OnPrepareForRender()			{	m_traTrans.Update();}
}
public class cCustomVertexEf : cSingleInputEffect							// ** Handler
{	protected override System.Guid r_GetGuid()				{	return cCustomVertexEfImplem.CLSID;}
}
class cCustomShadowEfImplem : cEffectImplementation							// ** Implementation
{	static readonly System.Guid CLSID_D2D1Shadow = new (0xC67EA361, 0x1863, 0x4e69, 0x89, 0xDB, 0x69, 0x5D, 0x3E, 0x9A, 0x5B, 0x6B);
	public static readonly System.Guid CLSID = new (0x8190494b, 0xd2ee, 0x451e, 0xb9, 0xad, 0xd1, 0xa7, 0x27, 0x3c, 0x91, 0xc6);
	public enum ePropertyIndices											// ** Same order as registered
	{	Offset	= 0,														// Point
		Color																// Color
	}
	cEffect m_efWave, m_efShadow; cOffsetTransform m_otOffset;
	public static void Register()
	{	Register(CLSID, "CustomShadow"										// Id and name
			, (out System.IntPtr implementation) => new cCustomShadowEfImplem().r_CreateProxy(out implementation) // Factory
			, 1																// Number of inputs
			, new Property(ePropertyType.Point, "Offset")					// ** Properties: same order as ePropertyIndices
			, new Property(ePropertyType.Color, "Color")
			);
	}
	protected override void Dispose(bool disposing)
	{	if (disposing)	{	m_efWave?.Dispose(); m_efShadow?.Dispose(); m_otOffset?.Dispose();}
		m_efWave = null!; m_efShadow = null!; m_otOffset = null!;
		base.Dispose(disposing);
	}
	protected override void OnInitialize(Context context)
	{	cTransform? trfWave = null, trfShadow = null, trfCompos = null; cEffect? efCompos = null;

	try
	{	// ** Create wave
		m_efWave = context.CreateEffect(cCustomVertexEfImplem.CLSID);
		trfWave = context.CreateTransformFrom(m_efWave);
		r_AddTransform(trfWave);
		r_ConnectToInput(trfWave, 0, 0);									// Conect input to wave
		// ** Create shadow
		m_efShadow = context.CreateEffect(CLSID_D2D1Shadow);
		trfShadow = context.CreateTransformFrom(m_efShadow);
		r_AddTransform(trfShadow);
		r_Connect(trfWave, trfShadow, 0);									// Conect wave to shadow
		// ** Create offset
		m_otOffset = context.CreateOffsetTransform(default);
		r_AddTransform(m_otOffset);
		// ** Conect shadow and offset
		r_Connect(trfShadow, m_otOffset, 0);
		// ** Create composite
		efCompos = context.CreateEffect(cCompositeEffect.CLSID);
		trfCompos = context.CreateTransformFrom(efCompos);
		r_AddTransform(trfCompos);
		r_Connect(m_otOffset, trfCompos, 0);								// Conect offset to composite
		r_Connect(trfWave, trfCompos, 1);									// Conect wave to composite
		// ** Cfg output
		r_SetOutput(trfCompos);												// Conect composite to output
	} finally
	{	trfWave?.Dispose(); trfShadow?.Dispose(); trfCompos?.Dispose(); efCompos?.Dispose();
	}
	}
	protected override void r_Point0Set(Point value)			{	m_otOffset.Offset = (value * cGraphics.DipToPixel).GetRounded();} // Offset
	protected override void r_Color0Set(Color value)			{	m_efShadow.SetValue(1 /*D2D1_SHADOW_PROP_COLOR*/, value);} // Color
}
public class cCustomShadowEf : cSingleInputEffect							// ** Handler
{	Point m_ptOffset; Color m_clrColor;
	public Point Offset
	{	get													=>	m_ptOffset;
		set													{	if (value != m_ptOffset)	{	m_ptOffset = value; Invalidate();}}
	}
	public Color Color
	{	get													=>	m_clrColor;
		set													{	if (value != m_clrColor)	{	m_clrColor = value; Invalidate();}}
	}
	protected override System.Guid r_GetGuid()				{	return cCustomShadowEfImplem.CLSID;}
	protected override void r_Update(Effect NativeEffect, cGraphics g)
	{	NativeEffect.SetValue((int)cCustomShadowEfImplem.ePropertyIndices.Offset, m_ptOffset);
		NativeEffect.SetValue((int)cCustomShadowEfImplem.ePropertyIndices.Color, m_clrColor);
		base.r_Update(NativeEffect, g);
	}
}
class cCustomTileEfImplem : cEffectTransformImplementation					// ** Implementation
{	public static readonly System.Guid CLSID = new (0xc6025eeb, 0xd5a8, 0x464c, 0xb3, 0x45, 0xab, 0x63, 0x8e, 0x7e, 0xa1, 0x22);
	static readonly System.Guid GUID_PIXEL_SHADER = new (0x6ee14f63, 0xccfa, 0x4ea5, 0x80, 0x9f, 0x7f, 0x2, 0x59, 0x18, 0xfc, 0xa9);
	public enum ePropertyIndices											// ** Same order as registered
	{	Region	= 0															// Rectangle
	}
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	struct ConstantBuffer
	{	public Rectangle Region;
	}
	ConstantBuffer m_cbProperties; Rectangle m_rtRegion;
	public static void Register()
	{	Register(CLSID, "CustomTile"										// Id and name
			, (out System.IntPtr implementation) => new cCustomTileEfImplem().r_CreateProxy(out implementation) // Factory
			, 1																// Number of inputs
			, new Property(ePropertyType.Rectangle, "Region")				// ** Properties: same order as PropertyIndices
			);
	}
	protected override void OnInitialize(Context context)
	{	context.LoadPixelShader(GUID_PIXEL_SHADER, mShaders.PS2DTile);
		r_SetSingleTransform(r_Transform);
	}
	protected override void r_Rectangle0Set(Rectangle value)	{	m_rtRegion = value;} // Region
	protected override void OnPrepareForRender()
	{	Point ptSize = (Point)r_InputRect.Size;

		m_cbProperties.Region.Location = ptSize * m_rtRegion.Location; m_cbProperties.Region.Size = ptSize * m_rtRegion.Size;
		r_SetPixelShaderConstantBuffer(m_cbProperties);
	}
	protected override void OnSetInfo()						{	r_SetPixelShader(GUID_PIXEL_SHADER);}
	protected override void OnInputSizeChanged()			{	OnPrepareForRender();}
}
public class cCustomTileEf : cSingleInputEffect								// ** Handler
{	Rectangle m_rtRegion;
	public Rectangle Region
	{	get													=>	m_rtRegion;
		set													{	m_rtRegion = value; Invalidate();}
	}
	protected override System.Guid r_GetGuid()				{	return cCustomTileEfImplem.CLSID;}
	protected override void r_Update(Effect NativeEffect, cGraphics g)
	{	NativeEffect.SetValue((int)cCustomTileEfImplem.ePropertyIndices.Region, m_rtRegion);
		base.r_Update(NativeEffect, g);
	}
}