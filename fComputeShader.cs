using Wew.Control;
using Wew.Media;
using Wew;

namespace Demo;

class fComputeShader : cDockControl
{	const int BITMAP_NUM_THREADS		= 32;
	const int CALC_NUM_THREADS			= 32;
	const int CALC_RECORDS_PER_THREAD	= 10;
	const int CALC_RECORD_COUNT			= 500;
		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Size = 16)]
	struct CalculationArgs
	{	public int RecordCount;
	};
	struct CalculationInRecord
	{	public int Id;
		public int Value1, Value2;
	};
	struct CalculationOutRecord
	{	public int Id;
		public int Sum;
	};
	cBitmap? m_bmpImg;
	readonly cComputeShader m_csShaderBitmap;
	readonly cPaintControl pntPaint;
public fComputeShader()
{	cTabControl tc; cTabControl.cTab tab; cButton btn;

	Text = "Compute shader";
	tc = new cTabControl {	Margins = new Rect(5)};
	tab = tc.Tabs.Add("Bitmap");
		pntPaint = new cPaintControl {	 Margins = new Rect(10, 40, 10, 40)};
			pntPaint.Paint += pntPaint_Paint;
			tab.Content.AddControl(pntPaint);
		btn = new cButton {	Text = "Open", Margins = new Rect(10, float.NaN, float.NaN, 10)};
			btn.Click += btnBitmapOpen_Click;
			tab.Content.AddControl(btn);
	tab = tc.Tabs.Add("Calculation");
		btn = new cButton {	Text = "Execute", Margins = new Rect(10, float.NaN, float.NaN, 10)};
			btn.Click += btnExecute_Click;
			tab.Content.AddControl(btn);
	AddControl(tc);
	m_csShaderBitmap = new cComputeShader(wMain.Device, mShaders.CSBitmap);
}
public override void Close()								{	m_csShaderBitmap.Dispose(); m_bmpImg?.Dispose(); base.Close();}
private void pntPaint_Paint(object sender, PaintArgs e)
{	if (m_bmpImg is not null)
		e.Graphics.DrawBitmap(m_bmpImg, pntPaint.ClientRectangle, eAlignment.Center, eResize.ScaleIfBig, 1, eInterpolation.Linear);
}
private void btnBitmapOpen_Click(object sender)
{	string? sFile;

	sFile = mDialog.ShowOpenFile(mMod.DLG_IMG_EXTS, mMod.DLG_IMG_GUID);	if (sFile is null) return;
	using cTexture texIn = new (wMain.Device, sFile), texOut = new (wMain.Device, texIn.Size.X, texIn.Size.Y, e3DBind.UnorderedAccess);

	wMain.Device.CSSetShader(m_csShaderBitmap);
		wMain.Device.CSSetTexture(texIn, 0, false); wMain.Device.CSSetTexture(texOut, 0, true);
		wMain.Device.CSDispatch((Point)texIn.Size / BITMAP_NUM_THREADS);
			wMain.Device.CSSetTexture(null, 0, false); wMain.Device.CSSetTexture(null, 0, true); // Release textures
	m_bmpImg?.Dispose(); m_bmpImg = null; m_bmpImg = texOut.CopyPixelsToBitmap();
	pntPaint.Invalidate();
}
private void btnExecute_Click(object sender)
{	CalculationOutRecord[] a_corOut;

	a_corOut = new CalculationOutRecord[CALC_RECORD_COUNT];
	using cComputeShader csShader = new (wMain.Device, mShaders.CSCalculation);
	using c3DSimpleBuffer<CalculationArgs> cbArgs = new (wMain.Device, e3DBind.ConstantBuffer);
	using c3DBuffer<CalculationInRecord> sbIn = new (wMain.Device, e3DBind.StructuredShaderResource, CALC_RECORD_COUNT);
	using c3DBuffer sbOut = new (wMain.Device, e3DBind.StructuredUnorderedAccess, CALC_RECORD_COUNT, a_corOut.Stride());
	
	wMain.Device.CSSetShader(csShader);
	cbArgs.AssignToCS(0, false);
		cbArgs.Data.RecordCount = CALC_RECORD_COUNT; cbArgs.Write();
	sbIn.AssignToCS(0, false);
		for (int i = 0; i < CALC_RECORD_COUNT; i++)	sbIn.Data[i] = new CalculationInRecord {	Id = i, Value1 = i + 1, Value2 = i * 2};
		sbIn.Write();
	sbOut.AssignToCS(0, true);
	int iTotalThreads = Wew.mMath.Ceil((float)CALC_RECORD_COUNT / CALC_RECORDS_PER_THREAD);
	wMain.Device.CSDispatch(Wew.mMath.Ceil((float)iTotalThreads / CALC_NUM_THREADS), 1);
	sbOut.Read(a_corOut);
	wMain.Device.CSSetShader(null); cbArgs.DetachFromCS(0, false); sbIn.DetachFromCS(0, false); sbOut.DetachFromCS(0, true); // Release objs
	mDialog.MsgBoxInformation($"Done: Id={a_corOut[0].Id}, Sum={a_corOut[0].Sum}", "Calculation");
}
}