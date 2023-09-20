using Wew.Control;
using Wew.Media;

namespace Demo;

class f3DInside2D : cDockControl
{	readonly cPlane m_plnModel;
	readonly cRenderControl renRender;
public f3DInside2D()
{	Text = "3D inside 2D";
	renRender = new cRenderControl {	BackColor = eBrush.LightSteelBlue, Margins = new Rect(0), FocusMode = eFocusMode.FocusControl};
			renRender.KeyDown += renRender_KeyDown;
		AddControl(renRender);
	m_plnModel = new cPlane(wMain.Device) {	Pitch = -45, Yaw = 45};
		renRender.Model = m_plnModel;
	renRender.Camera.MoveZ(-10);
	cFloatAnimation fan = new ();
	cStoryboard sto = new (() =>
		{		if (m_plnModel.IsDisposed)	return false;
			m_plnModel.Propeller.Roll = -fan.Value;
			renRender.Invalidate();
			return true;
		});
		sto.RepeatStoryboard(sto.AddFloatLinearTransition(0.5f, 360, fan), false);
		sto.Start();
}
public override void Close()								{	m_plnModel.Dispose(); base.Close();}
private void renRender_KeyDown(object sender, ref KeyArgs e)	{	m_plnModel.OnKeyDown(ref e);}
}