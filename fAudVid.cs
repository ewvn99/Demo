using TimeSpan = System.TimeSpan;
using Math = System.Math;
using Wew.Control;
using Wew.Media;
using Wew;

namespace Demo;

class fAudVid : cDockControl
{	const int SAMPLE_RATE		= 44100;
	const float TIME_FRACTION	= 1.0f / SAMPLE_RATE;
	const int AUD_SAMPLES		= SAMPLE_RATE / 20;
	const int TOT_AUD_SAMPLES	= AUD_SAMPLES * 2;
	class cUserPlayer : cPlayer
	{	readonly cColorKeyEffect m_ckeKey; readonly c3DTransformEffect m_3teMirror; Matrix4x4 m_m44Mirror;
		cSingleInputEffect? m_sieCurrEffect;
		public cUserPlayer()
		{	m_ckeKey = new cColorKeyEffect {	Background = mRes.BmpDisplMap, Color = eColor.White, Tolerance = 0.3f};
			m_m44Mirror = new Matrix4x4(-1, 0, 0, 0,  0, 1, 0, 0,  0, 0, 1, 0,  0, 0, 0, 1);
				m_3teMirror = new c3DTransformEffect {	Matrix = m_m44Mirror};
			cLabel lbl = new () {	LocationMargin = new Point(150, 0), Text = "Effect"};
				ToolBar.AddControl(lbl);
			float[] a_f = new float[] {	0, 0.15f, 0.65f, 0.75f, 0.9f};
			cComboBox cbo = new () {	LocationMargin = new Point(200, 0), Width = 300};
				cbo.Load(new cSingleInputEffect?[]
					{	null
						, m_ckeKey
						, new cBrightnessEffect {	WhitePoint = new Point(0.5f, 1)}
						, new cColorMatrixEffect {	Matrix = new Matrix5x4(0.213f, 0.213f, 0.213f, 0,  0.715f, 0.715f, 0.715f, 0,  0.072f, 0.072f, 0.072f, 0,  0, 0, 0, 1,  0, 0, 0, 0)}
						, m_3teMirror
						, new cColorMatrixEffect {	Matrix = new Matrix5x4(-1, 0, 0, 0,  0, -1, 0, 0,  0, 0, -1, 0,  0, 0, 0, 1,  1, 1, 1, 0)}
						, new cColorMatrixEffect {	Matrix = new Matrix5x4(1, 0, 0, 0,  0, 1, 0, 0,  0, 0, 0, 0,  0, 0, 0, 1,  0, 0, 0, 0)}
						, new cDiscreteTransferEffect {	Red = a_f, Green = a_f, Blue = a_f}
						, new cGaussianBlurEffect()
						, new cDirectionalBlurEffect {	Deviation = 20, Angle = 30}
						, new cConvolveMatrixEffect {	Matrix = new float[] {	-2.2f, -1.4f, -0.4f,  -1, 9, -1,  -1, -1, -1}}
						, new cThicknessEffect {	KernelWidth = 8}
						, new cTileEffect {	Rectangle = new Rectangle(200, 100, 200, 200), ModeX = eExtendMode.Mirror}
						, new cDisplacementMapEffect {	Map = mRes.BmpDisplMap, Scale = 30}
					});
				cbo.SelectedIndex = 0;
				cbo.Items[0].Text = "None";
				cbo.Items[1].Text = "Color key";
				cbo.Items[2].Text = "Brightness";
				cbo.Items[3].Text = "Black and white";
				cbo.Items[4].Text = "Mirror";
				cbo.Items[5].Text = "Negative";
				cbo.Items[6].Text = "Yellow channel";
				cbo.Items[7].Text = "Painting";
				cbo.Items[8].Text = "Gaussian blur";
				cbo.Items[9].Text = "Directional blur";
				cbo.Items[10].Text = "Emboss";
				cbo.Items[11].Text = "Thin";
				cbo.Items[12].Text = "Tile";
				cbo.Items[13].Text = "Displacement map";
				cbo.SelectionChanged += cboEffect_SelectionChanged;
				ToolBar.AddControl(cbo);
			cToolButton tbt = new () {	Bounds = new Rectangle(510, 0, 21, 21)
					, Bitmap = Wew.eResource.BmpOpen, ToolTip = "Open key bitmap"};
				tbt.Click += tbtOpenKeyBmp_Click;
				ToolBar.AddControl(tbt);
			cColorButton cbt = new () {	Bounds = new Rectangle(540, 0, 21, 21), ToolTip = "Color key"};
				cbt.ColorChanged += tbtColorKey_Click;
				ToolBar.AddControl(cbt);
			cSlider sli = new () {	LocationMargin = new Point(570, 0), Width = 80, Maximum = 0.1f, Value = 0.05f};
				sli.ValueChanged += sliKeyTolerance_ValueChanged;
				ToolBar.AddControl(sli);
			OpenDialogGuid = mMod.DLG_VID_GUID;
			//SubtitleProperties.BackColor = eColor.White; SubtitleProperties.FillColor = eColor.Transparent;
			//	SubtitleProperties.InnerBorderColor = eColor.Blue; SubtitleProperties.OuterBorderColor = eColor.Red;
		}
		protected override void OnPaintFrame(cGraphics g, cGpuBitmap frame, Rectangle destination)
		{	if (m_sieCurrEffect == m_3teMirror && frame.Size.X != m_m44Mirror.M30)
			{	m_m44Mirror.M30 = frame.Size.X; m_3teMirror.Matrix = m_m44Mirror;
			}
			g.DrawBitmap(frame, destination, eAlignment.None, eResize.Stretch, 1, eInterpolation.Linear, m_sieCurrEffect);
		}
		private void cboEffect_SelectionChanged(object sender)
		{	m_sieCurrEffect = (cSingleInputEffect?)((cComboBox)sender).SelectedValue; View.Invalidate();
		}
		private void tbtOpenKeyBmp_Click(object sender)
		{	string? s;
		
			s = mDialog.ShowOpenFile(mMod.DLG_IMG_EXTS, mMod.DLG_IMG_GUID);
			if (s is not null) {	m_ckeKey.Background = new cBitmap(s); View.Invalidate();}
		}
		private void tbtColorKey_Click(object sender)		{	m_ckeKey.Color = ((cColorButton)sender).Color; View.Invalidate();}
		private void sliKeyTolerance_ValueChanged(object sender, cTrackBar.eAction action)
		{	m_ckeKey.Tolerance = ((cSlider)sender).Value; View.Invalidate();
		}
	}
	class cPlayIn3D : cContainer
	{	readonly c3DModel m_mdlTv, m_mdlScreen; cTexture? m_texScreen; readonly cMan m_manMan;
		cMedia? m_medSource; cVideoFrame? m_vfmFrame;
		readonly cRenderControl renView;
		readonly cTimer m_tmrTimer;
		public cPlayIn3D()
		{	cButton btn;

			renView = new cRenderControl {	Margins = new Rect(10, 10, 10, 35), BackColor = eBrush.LightSteelBlue
					, FocusMode = eFocusMode.FocusControl};
				renView.Render += renView_Render;
				AddControl(renView);
			btn = new cButton {	Margins = new Rect(10, float.NaN, float.NaN, 10), Text = "Open"};
				btn.Click += btnOpen_Click;
				AddControl(btn);
			btn = new cButton {	Margins = new Rect(120, float.NaN, float.NaN, 10), Text = "Close"};
				btn.Click += btnClose_Click;
				AddControl(btn);
			m_mdlTv = new c3DModel(wMain.Device, typeof(mRes), "Res.Tv.mdl");
				m_mdlScreen = m_mdlTv.Parts["Frame"].Parts["Screen"];
			m_manMan = new cMan(wMain.Device) {	Z = 15, Yaw = 180};
				m_manMan.SitOnFloor();
			renView.Camera.Location = new Vector(-8, 8, 30); renView.Camera.Yaw = 170; renView.Camera.Pitch = 10;
			m_tmrTimer = new cTimer {	Interval = 30};
				m_tmrTimer.Tick += m_tmrTimer_Tick;
		}
		public override void Close()
		{	btnClose_Click(null); m_mdlTv.Dispose();
			base.Close();
		}
		private void renView_Render(object sender, c3DGraphics g3d)
		{	g3d.SetDefaults(renView.Camera);
			g3d.Render(m_mdlTv); g3d.Render(m_manMan);
		}
		private void btnOpen_Click(object sender)
		{	string? s; cMediaSource.cVideoTrack? vtkV;

			s = mDialog.ShowOpenFile(mMod.DLG_VID_EXTS, mMod.DLG_VID_GUID);	if (s is null) return;
			btnClose_Click(null);
			m_medSource = new cMedia(wMain.Device, s); m_medSource.GetFirstTracks(out vtkV, out _);
				if (vtkV is null)	{	btnClose_Click(null); return;}
			vtkV.Selected = true; vtkV.SetOutputFormat(vtkV.Format, true);
			m_tmrTimer.Start();
		}
		private void btnClose_Click(object? sender)
		{	m_tmrTimer.Stop();
			m_medSource?.Dispose(); m_medSource = null;
			m_vfmFrame?.Dispose();
		}
		private void m_tmrTimer_Tick(object sender)
		{	// ** Read frame
			cAudioFrame? afm = null; cSubtitleFrame? sfm = null;
			
			if (m_medSource?.Read(out _, out _, ref m_vfmFrame, ref afm, ref sfm) == eMediaReadResult.Eof)
			{	btnClose_Click(null);
				return;
			}
			// ** Show frame
			if (m_vfmFrame?.IsEmpty == false)
			{	m_vfmFrame.GetTexture(ref m_texScreen); m_mdlScreen.Material.Texture = m_texScreen; renView.Invalidate();
			}
		}
	}
	class cCapture : cContainer
	{	cMedia? m_medSource; cVideoFrame? m_vfmVideo; cAudioFrame? m_afm; cGpuBitmap? m_gbmImg;
		readonly cComboBox cboDevVid, cboDevAud;
		readonly cPaintControl pntVideo; readonly cButton btnPause;
		readonly cTimer m_tmrTimer;
		public cCapture()
		{	cLabel lbl; cButton btn;

			lbl = new cLabel {	LocationMargin = new Point(10, 10), Text = "Video device"};
				AddControl(lbl);
			cboDevVid = new cComboBox {	LocationMargin = new Point(100, 10), Width = 300};
				cboDevVid.Load(cMediaSource.GetCaptureDevices(true));
				AddControl(cboDevVid);
			lbl = new cLabel {	LocationMargin = new Point(410, 10), Text = "Audio device"};
				AddControl(lbl);
			cboDevAud = new cComboBox {	LocationMargin = new Point(500, 10), Width = 300};
				cboDevAud.Load(cMediaSource.GetCaptureDevices(false));
				AddControl(cboDevAud);
			pntVideo = new cPaintControl {	Bounds = new Rectangle(10, 40, 600, 320), BackColor = eBrush.Black};
				pntVideo.Paint += pntVideo_Paint;
				AddControl(pntVideo);
			btn = new cButton {	LocationMargin = new Point(10, 370), Text = "Start"};
				btn.Click += btnStart_Click;
				AddControl(btn);
			btnPause = new cButton {	LocationMargin = new Point(120, 370), Text = "Pause", Type = eButtonType.Check};
				btnPause.Click += btnPause_Click;
				AddControl(btnPause);
			btn = new cButton {	LocationMargin = new Point(230, 370), Text = "Stop"};
				btn.Click += btnStop_Click;
				AddControl(btn);
			m_tmrTimer = new cTimer {	Interval = 15};
				m_tmrTimer.Tick += m_tmrTimer_Tick;
			cboDevVid.SelectIndex(0);
			cboDevAud.SelectIndex(0);
		}
		public new void Close()								{	btnStop_Click(null);}
		private void pntVideo_Paint(object sender, PaintArgs e)
		{	if (m_gbmImg is not null)
				e.Graphics.DrawBitmap(m_gbmImg, pntVideo.ClientRectangle, eAlignment.Center | eAlignment.Middle, eResize.Scale);
		}
		private void btnStart_Click(object sender)
		{	cMediaSource.cVideoTrack? vtkV; cMediaSource.cAudioTrack? atkA;

				if (cboDevVid.SelectedItem is null && cboDevAud.SelectedItem is null)	return;
			btnStop_Click(null);
			m_medSource = new cMedia((cboDevVid.SelectedValue is not null ? ((CaptureDevice)cboDevVid.SelectedValue).Id : null) // ** Open
				, (cboDevAud.SelectedValue is not null ? ((CaptureDevice)cboDevAud.SelectedValue).Id : null)
				, wMain.Device);
			m_medSource.GetFirstTracks(out vtkV, out atkA);
			if (vtkV is not null)	{	vtkV.Selected = true; vtkV.SetOutputFormat(vtkV.Format, true);}
			if (atkA is not null)		atkA.Selected = true;
			m_tmrTimer.Start();
		}
		private void btnPause_Click(object sender)			{	if (btnPause.Checked)	m_tmrTimer.Stop();	else	m_tmrTimer.Start();}
		private void btnStop_Click(object? sender)
		{	m_medSource?.Dispose(); m_medSource = null;
			m_vfmVideo?.Dispose(); m_vfmVideo = null;
			m_gbmImg?.Dispose(); m_gbmImg = null;
			btnPause.Checked = false; m_tmrTimer.Stop(); pntVideo.Invalidate();
		}
		private void m_tmrTimer_Tick(object sender)
		{	cSubtitleFrame? sfm = null;
		
			// ** Read frame
			if (m_medSource?.Read(out _, out _, ref m_vfmVideo, ref m_afm, ref sfm) == eMediaReadResult.Eof)
			{	btnStop_Click(null);
				return;
			}
			// ** Show frame
			if (m_vfmVideo?.IsEmpty == false)
			{	m_vfmVideo.GetBitmap(ref m_gbmImg); pntVideo.Invalidate();
			}
		}
	}
	class cTranscode : cContainer
	{	const int COL_ID	= 0;
		const int COL_TP	= 1;
		const int COL_W		= 2;
		const int COL_H		= 3;
		const int COL_FPS	= 4;
		const int COL_BPS	= 5;
		const int COL_CHN	= 6;
		cMedia? m_medSource; bool m_bProcessing;
		string[]? ma_sPath; bool m_bIsSequence;
		readonly cEditControl edtSource; readonly cTextBox txtDest;
		readonly cGrid grdPistas;
		readonly cTimeControl timStart, timEnd;
		readonly cPlayer plyPlayer;
		readonly cButton btnPause; readonly cSlider sliPriority;
		public cTranscode()
		{	cLabel lbl; cButton btn;

			lbl = new cLabel {	LocationMargin = new Point(10, 10), Text = "Sources"};
					AddControl(lbl);
				edtSource = new cEditControl {	Bounds = new Rectangle(80, 10, 300, 50), ReadOnly = true};
					AddControl(edtSource);
				btn = new cButton {	LocationMargin = new Point(390, 10), Width = 90, Text = "Open source"};
					btn.Click += btnOpen_Click;
					AddControl(btn);
				btn = new cButton {	LocationMargin = new Point(390, 40), Width = 90, Text = "Open sequence"};
					btn.Click += btnOpenSeq_Click;
					AddControl(btn);
			lbl = new cLabel {	LocationMargin = new Point(10, 70), Text = "Tracks"};
					AddControl(lbl);
				grdPistas = new cGrid {	Bounds = new Rectangle(80, 70, 400, 170), ColumnCount = 7};
						grdPistas.ConfigureColumn(COL_ID, "Id", 30, eTextFormat.CenterMiddle);
						grdPistas.ConfigureColumn(COL_TP, "Type", 50, eTextFormat.CenterMiddle);
						grdPistas.ConfigureColumn(COL_W, "Width", 50, eTextFormat.RightMiddle, null, false, true, typeof(int));
						grdPistas.ConfigureColumn(COL_H, "Height", 50, eTextFormat.RightMiddle, null, false, true, typeof(int));
						grdPistas.ConfigureColumn(COL_FPS, "Fps", 50, eTextFormat.RightMiddle, null, false, true, typeof(int));
						grdPistas.ConfigureColumn(COL_BPS, "Bit rate", 60, eTextFormat.RightMiddle, null, false, true, typeof(int));
						grdPistas.ConfigureColumn(COL_CHN, "Channels", 60, eTextFormat.RightMiddle, null, false, true, typeof(int));
					AddControl(grdPistas);
			btn = new cButton {	LocationMargin = new Point(80, 250), Width = 120, Text = "Remove"};
				btn.Click += btnRemove_Click;
				AddControl(btn);
			lbl = new cLabel {	LocationMargin = new Point(10, 280), Text = "Start"};
					AddControl(lbl);
				timStart = new cTimeControl {	LocationMargin = new Point(80, 280)};
					AddControl(timStart);
			lbl = new cLabel {	LocationMargin = new Point(190, 280), Text = "End"};
				AddControl(lbl);
				timEnd = new cTimeControl {	LocationMargin = new Point(260, 280)};
					AddControl(timEnd);
			lbl = new cLabel {	LocationMargin = new Point(10, 310), Text = "Target"};
					AddControl(lbl);
				txtDest = new cTextBox {	LocationMargin = new Point(80, 310), Width = 360};
					AddControl(txtDest);
				btn = new cButton {	LocationMargin = new Point(450, 310), Width = 30, Text = "..."};
					btn.Click += btnSetDest_Click;
					AddControl(btn);
			btn = new cButton {	LocationMargin = new Point(500, 10), Width = 120, Text = "Play"};
					btn.Click += btnPlay_Click;
					AddControl(btn);
				plyPlayer = new cPlayer {	Bounds = new Rectangle(500, 40, 470, 290)};
					AddControl(plyPlayer);
			btn = new cButton {	LocationMargin = new Point(80, 370), Text = "Transcode"};
				btn.Click += btnTranscode_Click;
				AddControl(btn);
			btnPause = new cButton {	LocationMargin = new Point(190, 370), Text = "Pause", Type = eButtonType.Check};
				AddControl(btnPause);
			btn = new cButton {	LocationMargin = new Point(300, 370), Text = "Cancel"};
				btn.Click += btnCancel_Click;
				AddControl(btn);
			lbl = new cLabel {	LocationMargin = new Point(430, 370), Text = "Priority"};
					AddControl(lbl);
				sliPriority = new cSlider {	LocationMargin = new Point(480, 370), Width = 70, Maximum = 30, Value = 20};
					AddControl(sliPriority);
		}
		public new bool Close()								{	btnCancel_Click(this); return !m_bProcessing;}
		private void btnOpen_Click(object sender)
		{	string[]? a_s;

				if (m_bProcessing)	return;
			a_s = mDialog.ShowOpenFiles(mMod.DLG_VID_EXTS, mMod.DLG_VID_GUID);	if (a_s is null) return;
			m_Open(a_s, false);
		}
		private void btnOpenSeq_Click(object sender)
		{	string[]? a_s;

				if (m_bProcessing)	return;
			a_s = mDialog.ShowOpenFiles(mMod.DLG_VID_EXTS, mMod.DLG_VID_GUID);	if (a_s is null) return;
			a_s.Sort();
			m_Open(a_s, true);
		}
		private void btnPlay_Click(object sender)
		{		if (ma_sPath is null)	return;
			if (m_bIsSequence)	plyPlayer.OpenMedia(new cFileListStream(ma_sPath), true);	else	plyPlayer.OpenMedia(ma_sPath);
			plyPlayer.Play();
		}
		private void btnRemove_Click(object sender)			{	grdPistas.RemoveSelectedRows();}
		private void btnSetDest_Click(object sender)
		{	string? s;

			s = mDialog.ShowSaveFile("Mp4 videos|*.mp4;*.mpg", mMod.DLG_VID_GUID);	if (s is not null) txtDest.Text = s;
		}
		private void btnTranscode_Click(object sender)
		{	cMediaSink? snkSink = null; cFrame? fmeFrame; cVideoFrame? vfm = null; cAudioFrame? afm = null; cSubtitleFrame? sfm = null;
			TimeSpan tsStartTs, tsEndTs; System.DateTime dt;

				if (m_medSource is null || m_bProcessing || grdPistas.RowCount == 1 || txtDest.Text == "" // Invalid settings: exit
						|| timStart.Value >= timEnd.Value || timEnd.Value > m_medSource.Duration)
				{	return;
				}
			btnPause.Checked = false; m_bProcessing = true;
			wMain.Device.ProgressMinimum = (int)timStart.Value.TotalSeconds;
				wMain.Device.ProgressMaximum = (int)timEnd.Value.TotalSeconds; wMain.Device.ProgressValue = 0;
				wMain.Device.ProgressState = cWindow.eTaskButtonProgress.Normal;
		try
		{	// ** Create target
			snkSink = new cMediaSink(txtDest.Text);
			// ** Add tracks
			for (int i = 1; i < grdPistas.RowCount; i++)
			{	cMediaSource.cTrack stkTrack = m_medSource.Tracks[(int)grdPistas[i, COL_ID]! - 1];

				if (stkTrack is cMediaSource.cVideoTrack vtkVid)
				{	VideoFormat vfmt = vtkVid.Format; int iFps = (int)grdPistas[i, COL_FPS]!;

					vfmt.Size = new PointI((int)grdPistas[i, COL_W]!, (int)grdPistas[i, COL_H]!);
						vfmt.BitRate = (int)grdPistas[i, COL_BPS]!;
						if (iFps != mMath.RoundToInt(vfmt.Fps))	{	vfmt.FpsNum = 1; vfmt.FpsDen = iFps;}
					if (vtkVid.Type == snkSink.DefaultVideoType && vfmt.Size == vtkVid.Format.Size) // ** Copy
					{	vtkVid.DestinationTrack = snkSink.AddVideoTrack(vtkVid);
					} else													// ** Transcode
					{	vtkVid.SetOutputFormat(vfmt, false); vtkVid.DestinationTrack = snkSink.AddVideoTrack(vfmt);
					}
					vtkVid.Selected = true;
				} else if (stkTrack is cMediaSource.cAudioTrack atkAud)
				{	AudioFormat afmt = atkAud.Format;
					
					if ((int)grdPistas[i, COL_CHN]! != afmt.Channels)	afmt.ChannelLayout = 0;
					afmt.Channels = (int)grdPistas[i, COL_CHN]!;
					afmt.SampleRate = (int)grdPistas[i, COL_FPS]!; afmt.BitRate = (int)grdPistas[i, COL_BPS]!;
					if (atkAud.Type == snkSink.DefaultAudioType				// ** Copy
						&& afmt.Channels == atkAud.Format.Channels && afmt.SampleRate == atkAud.Format.SampleRate)
					{	atkAud.DestinationTrack = snkSink.AddAudioTrack(atkAud);
					} else													// ** Transcode
					{	atkAud.SetOutputFormat(afmt); atkAud.DestinationTrack = snkSink.AddAudioTrack(afmt);
					}
					atkAud.Selected = true;
				} else if (stkTrack is cMediaSource.cSubtitleTrack stkSubtit)
				{	stkSubtit.DestinationTrack = snkSink.AddSubtitleTrack(stkSubtit); // ** Copy
					stkSubtit.Selected = true;
				}
			}
			// ** Set range
			tsStartTs = timStart.Value; m_medSource.Seek(tsStartTs);
			tsEndTs = (timEnd.Value < m_medSource.Duration ? timEnd.Value : TimeSpan.MaxValue); // Try to read the whole file (duration might be wrong)
			// ** Process frames
			eMediaReadResult rr; cMediaSource.cTrack? stkSrcTrack; cMediaSink.cTrack kstDestTrack;
			TimeSpan tsAbsoluteTs, tsRelativeTs, tsMax;
			System.IO.FileInfo fi = new (txtDest.Text);

			snkSink.BeginWriting();
			dt = System.DateTime.Now; tsMax = TimeSpan.Zero;
			// ** Read frame
			while ((rr = m_medSource.Read(out stkSrcTrack, out fmeFrame, ref vfm, ref afm, ref sfm)) != eMediaReadResult.Eof)
			{		if (rr != eMediaReadResult.Ok || fmeFrame is null)	continue;
				kstDestTrack = stkSrcTrack!.DestinationTrack!; tsAbsoluteTs = fmeFrame.TimeStamp;
				// ** Write frame
				tsRelativeTs = tsAbsoluteTs - tsStartTs;					// ** Make ts relative to user start
				if (tsAbsoluteTs >= tsStartTs)
				{	if (tsAbsoluteTs <= tsEndTs)
					{	fmeFrame.TimeStamp = tsRelativeTs;
						snkSink.WriteFrame(kstDestTrack, fmeFrame);
					} else													// ** End of track: flush
						stkSrcTrack.Flush();
				}
				fmeFrame.Clear();
				// ** Show progress: allow to cancel
				if ((System.DateTime.Now - dt).TotalSeconds >= 1)
				{	if (tsRelativeTs > tsMax)	tsMax = tsRelativeTs;
					s_ShowStatistics(tsMax, fi);
					mApplication.DoEvents();								// ** Get user input
					while (btnPause.Checked && m_medSource is not null)		// ** Paused: wait
					{	wMain.Device.ProgressState = cWindow.eTaskButtonProgress.Paused;
						mApplication.DoEvents(); System.Threading.Thread.Sleep(1);
					}
					wMain.Device.ProgressState = cWindow.eTaskButtonProgress.Normal;
					if (m_medSource is null)	{	snkSink.EndWriting(); return;} // ** Canceled: exit
					dt = System.DateTime.Now;
				}
				int iSleep = (int)(sliPriority.Maximum - sliPriority.Value); // ** Avoid cpu overload
				if (iSleep != 0)	System.Threading.Thread.Sleep(iSleep);
			}
			// ** Close file
			snkSink.EndWriting(); s_ShowStatistics(tsMax, fi);
			mDialog.MsgBoxInformation("Transcoding finished", "Transcode");
			btnCancel_Click(null);
		} catch (System.Exception ex)
		{	wMain.Device.ProgressState = cWindow.eTaskButtonProgress.Error; mDialog.MsgBoxError(ex.Message);
		} finally
		{	snkSink?.Dispose();
			m_bProcessing = false; vfm?.Dispose(); afm?.Dispose(); sfm?.Dispose();
			wMain.Stat2 = null;
		}
			wMain.Device.ProgressState = cWindow.eTaskButtonProgress.None;
		}
		private void btnCancel_Click(object? sender)
		{		if (sender is not null && m_bProcessing && !mDialog.MsgBoxQuestion("Cancel process?", "Transcode"))	return;
			grdPistas.RowCount = 1;
			timStart.Value = timEnd.Value = TimeSpan.Zero; edtSource.Text = null; txtDest.Text = null;
			m_medSource?.Dispose(); m_medSource = null; ma_sPath = null;
		}
		private void m_Open(string[] a_sPath, bool bIsSequence)
		{	int i;

				if (m_bProcessing)	return;
			btnCancel_Click(null);
		try
		{	m_medSource = (bIsSequence ? new cMedia(new cFileListStream(a_sPath), wMain.Device, true) : new cMedia(wMain.Device, a_sPath));
			timEnd.Value = m_medSource.Duration;
			grdPistas.RowCount = 1 + m_medSource.Tracks.Count; i = 1;
			foreach (cMediaSource.cTrack stm in m_medSource.Tracks)
			{	grdPistas[i, COL_ID] = i;
				if (stm is cMediaSource.cVideoTrack vtk)
				{	VideoFormat vtp = vtk.Format;
				
					grdPistas[i, COL_TP] = "Video";
					grdPistas[i, COL_FPS] = mMath.RoundToInt(vtp.Fps); grdPistas[i, COL_BPS] = (vtp.BitRate != 0 ? vtp.BitRate : 500000);
					grdPistas[i, COL_W] = vtp.Size.X; grdPistas[i, COL_H] = vtp.Size.Y;
				} else if (stm is cMediaSource.cAudioTrack atk)
				{	AudioFormat atp = atk.Format;
				
					grdPistas[i, COL_TP] = "Audio";
					grdPistas[i, COL_FPS] = atp.SampleRate; grdPistas[i, COL_BPS] = (atp.BitRate != 0 ? atp.BitRate : 128000);
					grdPistas[i, COL_CHN] = atp.Channels;
				} else if (stm is cMediaSource.cSubtitleTrack stk)
				{	SubtitleFormat stp = stk.Format;
				
					grdPistas[i, COL_TP] = "Subtitle";
				}
				i++;
			}
			ma_sPath = a_sPath; m_bIsSequence = bIsSequence; edtSource.Text = string.Join("\r\n", ma_sPath);
		} catch (System.Exception ex)
		{	mDialog.MsgBoxError(ex.Message);
			btnCancel_Click(null);
		}
		}
	}
	class cConcatenate : cContainer
	{	const int COL_SRC		= 0;
		const int COL_PATH		= 1;
		const int COL_START		= 2;
		const int COL_END		= 3;
		const int COL_ID		= 4;
		const int COL_STM_TP	= 5;
		const int COL_TP		= 6;
		const int COL_W			= 7;
		const int COL_H			= 8;
		const int COL_FPS		= 9;
		const int COL_BPS		= 10;
		const int COL_CHN		= 11;
		cMedia? m_medSource; bool m_bProcessing;
		readonly cGrid grdSources;
		readonly cTextBox txtDest;
		readonly cButton btnPause;
		public cConcatenate()
		{	cLabel lbl; cButton btn;

			lbl = new cLabel {	LocationMargin = new Point(10, 10), Text = "Sources"};
					AddControl(lbl);
				grdSources = new cGrid {	Bounds = new Rectangle(80, 10, 870, 260), ColumnCount = 12};
						grdSources.Columns(COL_SRC).Visible = false;
						grdSources.ConfigureColumn(COL_PATH, "Path", 300);
						grdSources.ConfigureColumn(COL_START, "Start", 60, eTextFormat.CenterMiddle, null, false, true, typeof(TimeSpan));
						grdSources.ConfigureColumn(COL_END, "End", 60, eTextFormat.CenterMiddle, null, false, true, typeof(TimeSpan));
						grdSources.ConfigureColumn(COL_ID, "Id", 30, eTextFormat.CenterMiddle);
						grdSources.ConfigureColumn(COL_STM_TP, "Track type", 70, eTextFormat.CenterMiddle);
						grdSources.ConfigureColumn(COL_TP, "Type", 50, eTextFormat.CenterMiddle);
						grdSources.ConfigureColumn(COL_W, "Width", 50, eTextFormat.RightMiddle);
						grdSources.ConfigureColumn(COL_H, "Height", 50, eTextFormat.RightMiddle);
						grdSources.ConfigureColumn(COL_FPS, "Fps", 50, eTextFormat.RightMiddle);
						grdSources.ConfigureColumn(COL_BPS, "Bit rate", 60, eTextFormat.RightMiddle);
						grdSources.ConfigureColumn(COL_CHN, "Channels", 60, eTextFormat.RightMiddle);
					AddControl(grdSources);
				btn = new cButton {	LocationMargin = new Point(80, 280), Width = 120, Text = "Add source"};
					btn.Click += btnAddSource_Click;
					AddControl(btn);
				btn = new cButton {	LocationMargin = new Point(210, 280), Width = 120, Text = "Remove"};
					btn.Click += btnRemove_Click;
					AddControl(btn);
			lbl = new cLabel {	LocationMargin = new Point(10, 310), Text = "Target"};
					AddControl(lbl);
				txtDest = new cTextBox {	LocationMargin = new Point(80, 310), Width = 360};
					AddControl(txtDest);
				btn = new cButton {	LocationMargin = new Point(450, 310), Width = 30, Text = "..."};
					btn.Click += btnSetDest_Click;
					AddControl(btn);
			btn = new cButton {	LocationMargin = new Point(80, 370), Text = "Concatenate"};
				btn.Click += btnConcat_Click;
				AddControl(btn);
			btnPause = new cButton {	LocationMargin = new Point(190, 370), Text = "Pause", Type = eButtonType.Check};
				AddControl(btnPause);
			btn = new cButton {	LocationMargin = new Point(300, 370), Text = "Cancel"};
				btn.Click += btnCancel_Click;
				AddControl(btn);
		}
		private void btnAddSource_Click(object sender)
		{	string? s; int iRow, iStm;

				if (m_bProcessing)	return;
			s = mDialog.ShowOpenFile(mMod.DLG_VID_EXTS, mMod.DLG_VID_GUID);	if (s is null) return;
		try
		{	m_medSource = new cMedia(wMain.Device, s);
			iRow = grdSources.RowCount; grdSources.RowCount += 1 + m_medSource.Tracks.Count;
			grdSources[iRow, COL_SRC] = m_medSource; grdSources[iRow, COL_PATH] = s;
				grdSources[iRow, COL_START] = TimeSpan.Zero; grdSources[iRow, COL_END] = m_medSource.Duration;
			iRow++; iStm = 1;
			foreach (cMediaSource.cTrack stm in m_medSource.Tracks)
			{	grdSources[iRow, COL_ID] = iStm;
				if (stm is cMediaSource.cVideoTrack vtk)
				{	VideoFormat vtp = vtk.Format;
				
					grdSources[iRow, COL_STM_TP] = "Video"; grdSources[iRow, COL_TP] = vtk.Type.ToString();
					grdSources[iRow, COL_FPS] = mMath.RoundToInt(vtp.Fps);
					grdSources[iRow, COL_W] = vtp.Size.X; grdSources[iRow, COL_H] = vtp.Size.Y;
				} else if (stm is cMediaSource.cAudioTrack atk)
				{	AudioFormat atp = atk.Format;
				
					grdSources[iRow, COL_STM_TP] = "Audio"; grdSources[iRow, COL_TP] = atk.Type.ToString();
					grdSources[iRow, COL_FPS] = atp.SampleRate; grdSources[iRow, COL_BPS] = (atp.BitRate != 0 ? atp.BitRate : 128000);
					grdSources[iRow, COL_CHN] = atp.Channels;
				} else if (stm is cMediaSource.cSubtitleTrack stk)
				{	SubtitleFormat stp = stk.Format;
				
					grdSources[iRow, COL_STM_TP] = "Subtitle"; grdSources[iRow, COL_TP] = stk.Type.ToString();
				}
				iRow++; iStm++;
			}
		} catch (System.Exception ex)
		{	mDialog.MsgBoxError(ex.Message);
		}
		}
		private void btnRemove_Click(object sender)
		{	int iRow;
		
			iRow = grdSources.CurrentRow;	if (iRow == -1)	return;
			if (grdSources[iRow, COL_SRC] is not null)						// ** Remove source
			{	do	grdSources.RemoveRows(iRow, 1);	 while (iRow < grdSources.RowCount && grdSources[iRow, COL_SRC] is null);
			} else															// ** Remove track
			{	grdSources.RemoveRows(iRow, 1);
			}
		}
		private void btnSetDest_Click(object sender)
		{	string? s;

			s = mDialog.ShowSaveFile("Mp4 videos|*.mp4;*.mpg", mMod.DLG_VID_GUID);	if (s is not null) txtDest.Text = s;
		}
		private void btnConcat_Click(object sender)
		{	int iRow, iRow0, iRow0Max; cMedia medSrc0, medSrc; cMediaSource.cTrack stmStm;
			cMediaSink? snkSink = null; cFrame? fmeFrame; cVideoFrame? vfm = null; cAudioFrame? afm = null; cSubtitleFrame? sfm = null;
			TimeSpan tsMax; System.DateTime dt;
			System.IO.FileInfo fi;

				if (m_bProcessing || grdSources.RowCount == 1 || txtDest.Text == "") // Invalid settings: exit
				{	return;
				}
			btnPause.Checked = false; m_bProcessing = true;
			wMain.Device.ProgressState = cWindow.eTaskButtonProgress.Indeterminate;
		try
		{	// ** Create target
			snkSink = new cMediaSink(txtDest.Text);
			// ** Add tracks (only from first source)
			iRow = 1; medSrc0 = (cMedia)grdSources[iRow, COL_SRC]!;
			for (iRow++; iRow < grdSources.RowCount && grdSources[iRow, COL_SRC] is null; iRow++)
			{	stmStm = medSrc0.Tracks[(int)grdSources[iRow, COL_ID]! - 1];
				if (stmStm is cMediaSource.cVideoTrack vtkVid)
				{	if (vtkVid.Type == snkSink.DefaultVideoType)			// Same type
					{	vtkVid.DestinationTrack = snkSink.AddVideoTrack(vtkVid);
					} else													// Error
					{	throw new System.FormatException();
					}
					vtkVid.Selected = true;
				} else if (stmStm is cMediaSource.cAudioTrack atkAud)
				{	if (atkAud.Type == snkSink.DefaultAudioType)			// Same type
					{	atkAud.DestinationTrack = snkSink.AddAudioTrack(atkAud);
					} else													// Error
					{	throw new System.FormatException();
					}
					atkAud.Selected = true;
				} else if (stmStm is cMediaSource.cSubtitleTrack stkSubtit)
				{	stkSubtit.DestinationTrack = snkSink.AddSubtitleTrack(stkSubtit);
					stkSubtit.Selected = true;
				}
			}
			// ** Validate tracks of other sources
			iRow0Max = iRow;
			for (; iRow < grdSources.RowCount; )
			{	medSrc = (cMedia)grdSources[iRow, COL_SRC]!; iRow0 = 2;
				for (iRow++; iRow < grdSources.RowCount && grdSources[iRow, COL_SRC] is null; iRow++, iRow0++)
				{		if (iRow0 == iRow0Max)	throw new System.FormatException("Track count mismatch");
					stmStm = medSrc.Tracks[(int)grdSources[iRow, COL_ID]! - 1];
					if (stmStm is cMediaSource.cVideoTrack vtkVid)
					{	cMediaSource.cVideoTrack vst0 = (cMediaSource.cVideoTrack)medSrc0.Tracks[(int)grdSources[iRow0, COL_ID]! - 1];

						if (vtkVid.Type == vst0.Type                        // Same type and fmt
							/*&& vstVid.Format.Width == vst0.Format.Width && vstVid.Format.Height == vst0.Format.Height*/)
						{	vtkVid.DestinationTrack = vst0.DestinationTrack;
						} else												// Error
						{	throw new System.FormatException();
						}
						vtkVid.Selected = true;
					} else if (stmStm is cMediaSource.cAudioTrack atkAud)
					{	cMediaSource.cAudioTrack ast0 = (cMediaSource.cAudioTrack)medSrc0.Tracks[(int)grdSources[iRow0, COL_ID]! - 1];

						if (atkAud.Type == ast0.Type				         // Same type and fmt
								&& atkAud.Format.Channels == ast0.Format.Channels && atkAud.Format.SampleRate == ast0.Format.SampleRate)
						{	atkAud.DestinationTrack = ast0.DestinationTrack;
						} else												// Error
						{	throw new System.FormatException();
						}
						atkAud.Selected = true;
					} else if (stmStm is cMediaSource.cSubtitleTrack stkSubtit)
					{	cMediaSource.cSubtitleTrack sst0 = (cMediaSource.cSubtitleTrack)medSrc0.Tracks[(int)grdSources[iRow0, COL_ID]! - 1];

						stkSubtit.DestinationTrack = sst0.DestinationTrack;
						stkSubtit.Selected = true;
					}
				}
					if (iRow0 != iRow0Max)	throw new System.FormatException("Track count mismatch");
			}
			// ** Copy frames
			eMediaReadResult rr; cMediaSource.cTrack? stkSrcTrack; cMediaSink.cTrack ktkDestTrack;
			TimeSpan tsSinkTs, tsStartTs, tsEndTs, tsSrcTs, tsRelativeTs;

			snkSink.BeginWriting();
			dt = System.DateTime.Now; tsSinkTs = tsMax = TimeSpan.Zero;
			fi = new System.IO.FileInfo(txtDest.Text);
			for (iRow = 1; iRow < grdSources.RowCount; iRow++)
			{	medSrc = (cMedia)grdSources[iRow, COL_SRC]!;	if (medSrc is null)	continue; // Stm: continue
				// ** Set range
				tsStartTs = (TimeSpan)grdSources[iRow, COL_START]!; medSrc.Seek(tsStartTs);
				tsEndTs = (TimeSpan)grdSources[iRow, COL_END]!;
					if (tsEndTs >= medSrc.Duration)	tsEndTs = TimeSpan.MaxValue; // Try to read the whole file (duration might be wrong)
				// ** Read frame
				while ((rr = medSrc.Read(out stkSrcTrack, out fmeFrame, ref vfm, ref afm, ref sfm)) != eMediaReadResult.Eof)
				{		if (rr != eMediaReadResult.Ok || fmeFrame is null)	continue;
					ktkDestTrack = stkSrcTrack!.DestinationTrack!; tsSrcTs = fmeFrame.TimeStamp;
					// ** Write frame
					tsRelativeTs = tsSinkTs + tsSrcTs - tsStartTs;			// ** Make ts relative to user start
					if (tsRelativeTs > tsMax)	tsMax = tsRelativeTs;
					if (tsSrcTs >= tsStartTs)
					{	if (tsSrcTs <= tsEndTs)
						{	fmeFrame.TimeStamp = tsRelativeTs;
							snkSink.WriteFrame(ktkDestTrack, fmeFrame);
						} else												// ** End of track: flush
							stkSrcTrack.Flush();
					}
					// ** Show progress: allow to cancel
					if ((System.DateTime.Now - dt).TotalSeconds >= 1)
					{	s_ShowStatistics(tsMax, fi);
						mApplication.DoEvents();							// ** Get user input
						while (btnPause.Checked && grdSources.RowCount != 1) // ** Paused: wait
						{	wMain.Device.ProgressState = cWindow.eTaskButtonProgress.Paused;
							mApplication.DoEvents(); System.Threading.Thread.Sleep(1);
						}
						if (grdSources.RowCount == 1)	{	snkSink.EndWriting(); return;} // ** Canceled: exit
						dt = System.DateTime.Now;
					}
				}
				tsSinkTs = tsMax;
			}
			// ** Close file
			snkSink.EndWriting(); s_ShowStatistics(tsMax, fi);
			mDialog.MsgBoxInformation("Transcoding finished", "Transcode");
			btnCancel_Click(null);
		} catch (System.Exception ex)
		{	wMain.Device.ProgressState = cWindow.eTaskButtonProgress.Error; mDialog.MsgBoxError(ex.Message);
		} finally
		{	snkSink?.Dispose();
			m_bProcessing = false; vfm?.Dispose(); afm?.Dispose(); sfm?.Dispose();
			wMain.Stat2 = null;
		}
			wMain.Device.ProgressState = cWindow.eTaskButtonProgress.None;
		}
		private void btnCancel_Click(object? sender)
		{		if (sender is not null && m_bProcessing && !mDialog.MsgBoxQuestion("Cancel process?", "Concatenate"))	return;
			for (int iRow = 1; iRow < grdSources.RowCount; iRow++)
			{	((cMediaSource?)grdSources[iRow, COL_SRC])?.Dispose(); grdSources[iRow, COL_SRC] = null;
			}
			grdSources.RowCount = 1; txtDest.Text = null;
		}
	}
	class cGenerate : cContainer
	{	bool m_bGenerating;
		public cGenerate()
		{	cButton btn;
		
			btn = new cButton {	LocationMargin = new Point(10, 10), Text = "Generate..."};
				btn.Click += btnGenerate_Click;
				AddControl(btn);
		}
		public new bool Close()								{	return !m_bGenerating;}
		private void btnGenerate_Click(object sender)
		{   /*const*/ TimeSpan DURATION = new (0, 0, 3); const int SLEEP = 10;
			string? sFile; cMediaSink snkSink; cMediaSink.cVideoTrack vtkVideoTrack; cMediaSink.cAudioTrack atkAudioTrack;
			VideoFormat vfmtVideo; System.IO.FileInfo fi; TimeSpan tsMax;

				if (m_bGenerating)	return;
			// ** Create output file
			sFile = mDialog.ShowSaveFile("Mp4 videos|*.mp4|All files|*.*", mMod.DLG_VID_GUID);	if (sFile is null)	return;
			snkSink = new cMediaSink(sFile); m_bGenerating = true;
			wMain.Device.ProgressMinimum = 0; wMain.Device.ProgressMaximum = (int)DURATION.TotalSeconds; wMain.Device.ProgressValue = 0;
				wMain.Device.ProgressState = cWindow.eTaskButtonProgress.Normal;
		try
		{	// ** Add video track
			vfmtVideo = new VideoFormat(630, 420);
			vtkVideoTrack = snkSink.AddVideoTrack(vfmtVideo);
			// ** Add audio track
			atkAudioTrack = snkSink.AddAudioTrack(new AudioFormat(2, SAMPLE_RATE));
			// ** Write
			snkSink.BeginWriting();
			fi = new System.IO.FileInfo(sFile); tsMax = TimeSpan.Zero;
			using cVideoFrame vfmVideo = new (); using cAudioFrame afmAudio = new ();
			using cPlane plnModel = new (wMain.Device) {	Yaw = 90, RudderYaw = -1};
			TimeSpan tsVFrameDurat; float fVFrameDuratSecs;
			cCamera camCam;
			float[] a_fFrame; bool bAudioLeftChn; double dTimeL, dTimeR, fVol;
			System.DateTime dt;
	
			tsVFrameDurat = new TimeSpan((long)(10000000 / vfmtVideo.Fps)); fVFrameDuratSecs = (float)tsVFrameDurat.TotalSeconds;
			camCam = new cCamera(vfmtVideo.Size.X, vfmtVideo.Size.Y) {	Pitch = -10, Location = new Vector(0, 1, -10)};
			a_fFrame = new float[TOT_AUD_SAMPLES]; bAudioLeftChn = true; dTimeL = dTimeR = 0; fVol = mMath.PI / 2;
			dt = System.DateTime.Now;
			for (TimeSpan tsVideoTs = TimeSpan.Zero, tsAudioTs = TimeSpan.Zero; tsVideoTs < DURATION; )
			{	// ** Compose video
				vfmVideo.Reset(wMain.Device, vfmtVideo.Size.X, vfmtVideo.Size.Y);
				//wMain.Device.Render3DTo(vfmVideo, (object o, c3DGraphics g3d) =>
				//{	g3d.ClearTarget(wMain.Device.BackBuffer, eColor.SkyBlue); g3d.SetDefaults(camCam);
				//	g3d.Render(plnModel); _ = plnModel.Act(fVFrameDuratSecs);
				//});
				//wMain.Device.Render2DTo(vfmVideo, (cGraphics g) =>
				//{	if (tsVideoTs.TotalSeconds is >= 1 and <= 4)
				//	{	g.DrawText("Text", new Rectangle(50, 290, 200, 20), eBrush.Black, eFont.SystemBoldText, eTextFormat.Default);
				//	}
				//});
				// ** Write video
				vfmVideo.TimeStamp = tsVideoTs;
				snkSink.WriteFrame(vtkVideoTrack, vfmVideo);
				tsVideoTs += tsVFrameDurat; vfmVideo.Clear();
				// ** Compose audio
				while (tsAudioTs < tsVideoTs)
				{	if (bAudioLeftChn)
						m_GenerateAudio(afmAudio, a_fFrame, ref dTimeL, ref dTimeR, 150, mMath.Sin(fVol) / 2 + 0.5f, 0, 0);
					else
					{	m_GenerateAudio(afmAudio, a_fFrame, ref dTimeL, ref dTimeR, 0, 0, 150, -mMath.Sin(fVol) / 2 + 0.5f);
					}
					bAudioLeftChn = !bAudioLeftChn; fVol = (fVol + 0.08f) % mMath.PIx2;
					// ** Write audio
					afmAudio.TimeStamp = tsAudioTs;
					snkSink.WriteFrame(atkAudioTrack, afmAudio);
					tsAudioTs += afmAudio.Duration; afmAudio.Clear();
				}
				// ** Show progress
				if ((System.DateTime.Now - dt).TotalSeconds >= 1)
				{	if (tsVideoTs > tsMax)	tsMax = tsVideoTs;
					s_ShowStatistics(tsMax, fi);
					mApplication.DoEvents();
					if (IsClosed)	{	snkSink.Dispose(); return;}		// ** Form closed: exit
					dt = System.DateTime.Now;
				}
				System.Threading.Thread.Sleep(SLEEP);
			}
			// ** Close file
			snkSink.EndWriting(); s_ShowStatistics(tsMax, fi);
			mDialog.MsgBoxInformation("Generation finished", "Generate");
		} catch (System.Exception ex)
		{	wMain.Device.ProgressState = cWindow.eTaskButtonProgress.Error; mDialog.MsgBoxError(ex.Message);
		} finally
		{	m_bGenerating = false;
			snkSink.Dispose();
			wMain.Stat2 = null;
		}
			wMain.Device.ProgressState = cWindow.eTaskButtonProgress.None;
		}
		private void m_GenerateAudio(cAudioFrame afmAudio, float[] a_fFrame, ref double dTimeL, ref double dTimeR
				, float fFrecL, float fVolL, float fFrecR, float fVolR)
		{	double dTL = dTimeL, dTR = dTimeR;

			fFrecL *= mMath.PIx2 / SAMPLE_RATE; fFrecR *= mMath.PIx2 / SAMPLE_RATE;
				for (int i = 0; i < TOT_AUD_SAMPLES; )
				{   a_fFrame[i++] = mMath.Sin(dTL) * fVolL; a_fFrame[i++] = mMath.Sin(dTR) * fVolR;
					dTL += fFrecL; dTR += fFrecR;
				}
				dTimeL = dTL % mMath.PIx2; dTimeR = dTR % mMath.PIx2;
				afmAudio.Reset(a_fFrame, AUD_SAMPLES, 2);
			afmAudio.Duration = TimeSpan.FromSeconds((double)AUD_SAMPLES / SAMPLE_RATE);
		}
	}
	class cSynthesizer : cContainer
	{	private class cBeatCtl : cStackPanel
		{	float m_fTime; bool m_bBeat;
			readonly cSlider sliNote, sliAttack, sliDecay, sliSustain, sliSustainAmp, sliRelease, sliDelay, sliRepetition;
			readonly cComboBox cboPreset;
			public event dEvent? Changed, Restart;
			public cBeatCtl()
			{	cLabel lbl;

				lbl = new cLabel {	Text = "Note"};
						AddControl(lbl);
					sliNote = new cSlider {	Width = 60, Minimum = 1, Maximum = 16, Value = 1};
						sliNote.ValueChanged += sliSlider_ValueChanged;
						AddControl(sliNote);
				lbl = new cLabel {	Text = "Attack"};
						AddControl(lbl);
					sliAttack = new cSlider {	Width = 60, Minimum = 0, Maximum = 1, Value = 0.01f};
						sliAttack.ValueChanged += sliSlider_ValueChanged;
						AddControl(sliAttack);
				lbl = new cLabel {	Text = "Decay"};
						AddControl(lbl);
					sliDecay = new cSlider {	Width = 60, Minimum = 0, Maximum = 2, Value = 0.5f};
						sliDecay.ValueChanged += sliSlider_ValueChanged;
						AddControl(sliDecay);
				lbl = new cLabel {	Text = "Sustain amp"};
						AddControl(lbl);
					sliSustainAmp = new cSlider {	Width = 60, Minimum = 0, Maximum = 1, Value = 0.01f};
						sliSustainAmp.ValueChanged += sliSlider_ValueChanged;
						AddControl(sliSustainAmp);
				lbl = new cLabel {	Text = "Sustain"};
						AddControl(lbl);
					sliSustain = new cSlider {	Width = 60, Minimum = 0, Maximum = 1, Value = 0.01f};
						sliSustain.ValueChanged += sliSlider_ValueChanged;
						AddControl(sliSustain);
				lbl = new cLabel {	Text = "Release"};
						AddControl(lbl);
					sliRelease = new cSlider {	Width = 60, Minimum = 0, Maximum = 1, Value = 0.01f};
						sliRelease.ValueChanged += sliSlider_ValueChanged;
						AddControl(sliRelease);
				lbl = new cLabel {	Text = "Delay"};
						AddControl(lbl);
					sliDelay = new cSlider {	Width = 60, Minimum = 0, Maximum = 10, Value = 0};
						sliDelay.ValueChanged += sliDelay_ValueChanged;
						AddControl(sliDelay);
				lbl = new cLabel {	Text = "Repetition"};
						AddControl(lbl);
					sliRepetition = new cSlider {	Width = 60, Minimum = 0, Maximum = 4, Value = 1.5f};
						sliRepetition.ValueChanged += sliSlider_ValueChanged;
						AddControl(sliRepetition);
				lbl = new cLabel {	Text = "Preset"};
						AddControl(lbl);
					cboPreset = new cComboBox {	Width = 60, Tag = this};
						cboPreset.Load(new string[] {	"A", "B"});
						cboPreset.SelectionChanged += cboPreset_SelectionChanged;
						AddControl(cboPreset);
				AutoSize = eAutoSize.Both; BorderStyle = eBorderStyle.Default;
				Reset();
			}
			public void WriteSound(float[] a_fFrame)
			{	float fNote = sliNote.Value;								// Get properties
				float fFreq = m_fNoteToFreq(fNote);
				float fAttackTime = sliAttack.Value;
				float fDecayTime = sliDecay.Value;
				float fSustainTime = sliSustain.Value;
				float fSustainAmplitude = sliSustainAmp.Value;
				float fReleaseTime = sliRelease.Value;
				float fRepetition = sliRepetition.Value;

				for (int iSamp = 0; iSamp < TOT_AUD_SAMPLES; iSamp += 2)
				{	if (m_bBeat)
					{	float fVal, fAmp, fBeatTime;

						// ** Calculate value
						fVal = m_fOscSin(fFreq)								// Generate carrier
						//fVal = m_fOscSquare(fFreq) * 0.5f					// Generate carrier
						//fVal = (float)(Math.Asin(Math.Sin(fFreq)) * (2.0 / Math.PI)); // Generate carrier
						//fVal = (float)((2.0 / Math.PI) * (fFreq * Math.PI * (m_fTime % (1.0 / fFreq)) - (Math.PI / 2.0))); // Generate carrier
								+ m_fOscSin(fFreq, 1, 0.125f) * 0.3f		// Add noise with out-of-phase angles but same freq
								+ m_fOscSin(fFreq, 1, 0.25f) * 0.1f
							+ m_fOscSin(m_fNoteToFreq(fNote - 12)) * 0.25f	// Modify freq
							+ m_fOscSin(m_fNoteToFreq(fNote - 24)) * 0.1f
							;
						// ** Modulate amplitude
						fBeatTime = m_fTime;
						if (fBeatTime <= fAttackTime)
							fAmp = (fAttackTime != 0 ? fBeatTime / fAttackTime : 1);
						else if ((fBeatTime -= fAttackTime) < fDecayTime)
							fAmp = 1 - (fBeatTime / fDecayTime) * (1 - fSustainAmplitude);
						else if ((fBeatTime -= fDecayTime) <= fSustainTime)
							fAmp = fSustainAmplitude;
						else if ((fBeatTime -= fSustainTime) < fReleaseTime)
							fAmp = fSustainAmplitude - (fBeatTime / fReleaseTime) * fSustainAmplitude;
						else
						{	fAmp = 0; m_bBeat = false;
						}
						fVal *= fAmp;
						// ** Add value to channels
						a_fFrame[iSamp] += fVal; a_fFrame[iSamp + 1] += fVal;
						m_fTime += TIME_FRACTION;
					} else if (m_fTime >= fRepetition || m_fTime == 0)
					{	m_fTime = 0; m_bBeat = true;
					} else
					{	m_fTime += TIME_FRACTION;
					}
				}
			}
			public void Reset()								{	m_fTime = -sliDelay.Value; m_bBeat = false;}
			private void sliSlider_ValueChanged(object sender, cTrackBar.eAction action)	{	Changed?.Invoke(this);}
			private void sliDelay_ValueChanged(object sender, cTrackBar.eAction action)		{	Restart?.Invoke(this);}
			private void cboPreset_SelectionChanged(object sender)
			{	cBeatCtl bct = (cBeatCtl)cboPreset.Tag!;

				switch (cboPreset.SelectedIndex)
				{	case 0:
						bct.sliAttack.Value = 0.01f;
						bct.sliDecay.Value = 1;
						bct.sliSustain.Value = 0;
						bct.sliSustainAmp.Value = 0;
						bct.sliRelease.Value = 1;
						break;
					case 1:
						bct.sliAttack.Value = 0.01f;
						bct.sliDecay.Value = 0.15f;
						bct.sliSustain.Value = 0;
						bct.sliSustainAmp.Value = 0;
						bct.sliRelease.Value = 0;
						break;
				}
			}
			private float m_fNoteToFreq(float fNote)		{	return (float)(256 * Math.Pow(1.0594630943592952645618252949463, fNote));}
			private float m_fCalcAngle(float fFreq, float fSpeed = 1, float fOffset = 0)
			{	return mMath.PIx2 * fFreq * m_fTime * fSpeed + fOffset * mMath.PIx2;
			}
			private float m_fOscSin(float fFreq, float fFactor = 1, float fOffset = 0)
			{	return mMath.Sin(m_fCalcAngle(fFreq, fFactor, fOffset));
			}
			private float m_fOscSquare(float fFreq, float fFactor = 1, float fOffset = 0)
			{	return (mMath.Sin(m_fCalcAngle(fFreq, fFactor, fOffset)) >= 0 ? 1 : -1);
			}
		}
		readonly cAudioRenderer m_arnAudRen; readonly float[] ma_fFrame;
		readonly cStackPanel spnBeats;
		readonly cButton btnPlay;
		readonly cSlider sliVol;
		readonly cTimer m_tmrTimer;
		public cSynthesizer()
		{	cLabel lbl; cScrollableControl sct; cButton btn;

			lbl = new cLabel {	LocationMargin = new Point(10, 10), Text = "Beats"};
				AddControl(lbl);
			sct = new cScrollableControl {	LocationMargin = new Point(50, 10), RightMargin = 10, Height = 350};
					sct.VerticalBar.Visible = true; sct.VerticalBar.AutoHide = false; sct.HorizontalBar.AutoHide = false;
					AddControl(sct);
				spnBeats = new cStackPanel {	AutoSize = eAutoSize.Both, Direction = eDirection.Bottom};
					sct.ClientArea.AddControl(spnBeats);
			btn = new cButton {	Margins = new Rect(50, float.NaN, float.NaN, 0), Text = "Add"};
				btn.Click += btnAdd_Click;
				AddControl(btn);
			btnPlay = new cButton {	Margins = new Rect(160, float.NaN, float.NaN, 0), Text = "Play / Stop", Type = eButtonType.Check};
				btnPlay.Click += btnPlay_Click;
				AddControl(btnPlay);
			sliVol = new cSlider {	Margins = new Rect(float.NaN, float.NaN, 5, 0), Width = 70, Value = 50};
				sliVol.ValueChanged += sliVol_ValueChanged;
				AddControl(sliVol);
			m_tmrTimer = new cTimer {	Interval = 40};
				m_tmrTimer.Tick += m_tmrTimer_Tick;
			m_arnAudRen = new cAudioRenderer();
				m_arnAudRen.SetFormat(new AudioFormat(2, SAMPLE_RATE));
			ma_fFrame = new float[TOT_AUD_SAMPLES];
			btnAdd_Click(null);
		}
		public override void Close()						{	m_tmrTimer.Stop(); m_arnAudRen.Dispose(); base.Close();}
		private void btnAdd_Click(object? sender)
		{	cBeatCtl bea;
		
			bea = new cBeatCtl();
			bea.Changed += beaBeat_Changed; bea.Restart += beaBeat_Restart;
			spnBeats.AddControl(bea);
		}
		private void btnPlay_Click(object sender)
		{	if (btnPlay.Checked)
			{	beaBeat_Restart(null);
				m_tmrTimer.Start(); m_arnAudRen.Play();
			} else
			{	m_tmrTimer.Stop(); m_arnAudRen.Pause(); m_arnAudRen.Flush();
			}
		}
		private void beaBeat_Changed(object sender)			{}
		private void beaBeat_Restart(object? sender)
		{	for (int i = 0; i < spnBeats.Controls.Count; i++)	((cBeatCtl)spnBeats.Controls[i]).Reset();
		}
		private void sliVol_ValueChanged(object sender, cTrackBar.eAction action)	{	m_arnAudRen.Volume = sliVol.Value;}
		private void m_tmrTimer_Tick(object sender)
		{	ma_fFrame.Clear();												// Set to silence
			for (int i = 0; i < spnBeats.Controls.Count; i++)	((cBeatCtl)spnBeats.Controls[i]).WriteSound(ma_fFrame); // Add sounds
			for (int i = 0; i < TOT_AUD_SAMPLES; i++)	ma_fFrame[i].Clamp(-1, 1); // Clamp
			m_arnAudRen.WriteFrame(ma_fFrame, AUD_SAMPLES);					// Play
				while (!m_arnAudRen.IsWriteEnded)	System.Threading.Thread.Sleep(10);
		}
	}
	readonly cCapture m_capCapture;
	readonly cTranscode m_traTranscode;
	readonly cConcatenate m_conConcatenate;
	readonly cGenerate m_genGenerate;
	static void s_ShowStatistics(TimeSpan tsCurrent, System.IO.FileInfo fiSink)
	{	fiSink.Refresh();
		wMain.Stat2 = string.Format("Time: {0:hh\\:mm\\:ss}, Size: {1}", tsCurrent, mHelper.FormatSize(fiSink.Length));
		wMain.Device.ProgressValue = (int)tsCurrent.TotalSeconds;
	}
public fAudVid()
{	cTabControl tc; cTabControl.cTab tab;

	Text = "Audio/video";
	tc = new cTabControl {	Margins = new Rect(5)};
	tab = tc.Tabs.Add("Player");
		tab.Content = new cUserPlayer();
	tab = tc.Tabs.Add("Play inside 3D");
		tab.Content = new cPlayIn3D();
	m_capCapture = new cCapture();
		tab = tc.Tabs.Add("Capture");
		tab.Content = m_capCapture;
	m_traTranscode = new cTranscode();
		//tab = tc.Tabs.Add("Transcode");
		//tab.Content = m_traTranscode;
	m_conConcatenate = new cConcatenate();
		//tab = tc.Tabs.Add("Concatenate");
		//tab.Content = m_conConcatenate;
	m_genGenerate = new cGenerate();
		tab = tc.Tabs.Add("Generate");
		tab.Content = m_genGenerate;
	tab = tc.Tabs.Add("Synthesizer");
		tab.Content = new cSynthesizer();
	AddControl(tc);
}
public override bool CanClose(eCloseReason reason = eCloseReason.User)
{	m_capCapture.Close();
	return m_genGenerate.Close() && m_traTranscode.Close();
}
}