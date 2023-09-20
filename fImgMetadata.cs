using Wew.Control;
using Wew.Media;

namespace Demo;

class fImgMetadata : cDockControl
{	cBitmap? m_bmpImg;
	readonly cEditControl edDat;
public fImgMetadata()
{	cButton btn;

	Text = "Image metadata";
	edDat = new cEditControl {	Margins = new Rect(10, 10, 10, 40), TabSize = 500};
		AddControl(edDat);
	btn = new cButton {	Text = "Open", Margins = new Rect(10, float.NaN, float.NaN, 10)};
		btn.Click += btnOpen_Click;
		AddControl(btn);
	btn = new cButton {	Text = "Write metadata", Margins = new Rect(120, float.NaN, float.NaN, 10)};
		btn.Click += btnWrite_Click;
		AddControl(btn);
}
public override void Close()								{	m_bmpImg?.Dispose(); base.Close();}
private void btnOpen_Click(object sender)
{	string? sFile;

	sFile = mDialog.ShowOpenFile(mMod.DLG_IMG_EXTS, mMod.DLG_IMG_GUID);	if (sFile is null) return;
	m_bmpImg?.Dispose();
	m_bmpImg = new cBitmap(sFile, true);
		if (m_bmpImg.Metadata is null)	{	m_bmpImg.Dispose(); m_bmpImg = null; mDialog.MsgBoxExclamation("No metadata"); return;}
	edDat.Text = null;
	m_AddPathsRecursive(true, "", m_bmpImg.Metadata.GetPaths(true));
	m_AddPathsRecursive(false, "", m_bmpImg.Metadata.GetPaths(false));
	edDat.ClearUndo();
}
private void btnWrite_Click(object sender)
{	string? sFile;

		if (m_bmpImg is null)	{	mDialog.MsgBoxExclamation("Open an image first"); return;}
	sFile = mDialog.ShowSaveFile("Jpeg|*.jpg|Tiff|*.tif", mMod.DLG_IMG_GUID);	if (sFile is null) return;
	cBitmap.Save(sFile, default, null, m_bmpImg.FrameCount
		, (cBitmap.cFrameEncoder fe) =>
		{	cBitmap.MetadataWriter? mw;

			_ = fe.WriteContainerMetadata(m_bmpImg.Metadata);
			for (int i = 0, iFrames = (mImaging.IsMultiframe(fe.ContainerFormat) ? m_bmpImg.FrameCount : 1); i < iFrames; i++)
			{	fe.Create();
				mw = fe.WriteMetadata(m_bmpImg[i].Metadata);
					if (mw is not null)
					{	if (fe.ContainerFormat == cBitmap.eContainerFormat.Jpeg)
						{	mw.Value.WriteString("/app1/ifd/{ushort=315}", "John Smith"); // Author
							//mw.Value.Remove("/xmp/dc:creator");			// Author: different path when removing
						} else if (fe.ContainerFormat == cBitmap.eContainerFormat.Tiff)
						{	mw.Value.WriteString("/ifd/{ushort=315}", "John Smith"); // Author
						}
					}
				fe.Write(m_bmpImg[i]);
			}
		});
}
void m_AddPathsRecursive(bool ImageLevel, string sRoot, string[]? a_sNodes)
{	string sPath;
	
		if (a_sNodes is null)	return;
	foreach (string sNode in a_sNodes)
	{	sPath = sRoot + sNode;
		edDat.AddText(string.Format("{0}	{1}\n", sPath, m_bmpImg!.Metadata!.GetValue(ImageLevel, sPath)));
		m_AddPathsRecursive(ImageLevel, sPath, m_bmpImg.Metadata.GetPaths(ImageLevel, sPath));
	}
}
}