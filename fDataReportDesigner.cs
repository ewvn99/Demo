using System.ComponentModel;
using System.ComponentModel.Design;
using Wew.Control;
using Wew.Media;
using Wew;

namespace Demo;

partial class fDataReportDesigner : cDockControl
{	readonly cDesignerHost m_dhHost; readonly cDesignerSerializationManager m_dsmSeria;
public fDataReportDesigner()
{	Text = "Data Report Designer";
	r_Commands.Add(new cCommandState(eCommand.Save, Save_Exec));
	cToolBar tb = new () {	Height = 40};
		_ = tb.Items.Add("form", eResource.Bmp2Pages, Frm_Click);
		AddControl(tb);
	m_dhHost = new cDesignerHost();
		m_dhHost.ComponentChangeService.ComponentAdded += CpoChgSvc_ComponentAdded;
	m_dsmSeria = new cDesignerSerializationManager() {	DesignerHost = m_dhHost.DesignerHost};
	//m_dhHost.DesignerSerializationManager = m_dsmSeria;
	// ** Load
	m_dhHost.BeginLoad(new cDataReportLoader());
	// ** Show
	cControl ctlView = m_dhHost.GetView();
		ctlView.Margins = new Rect(0); ctlView.BorderStyle = eBorderStyle.Default;
		AddControl(ctlView);
}
public override void Close()								{	base.Close(); m_dhHost.Dispose();} // Call OnDeactivated first
protected override void OnActivated()
{	OnDockBarActivated(wMain.ToolBox, true); OnDockBarActivated(wMain.PropertyGrid, true);
}
protected override void OnDeactivated()
{	OnDockBarActivated(wMain.ToolBox, false); OnDockBarActivated(wMain.PropertyGrid, false);
}
protected override void OnDockBarActivated(cDockControl DockBar, bool active)
{	if (active)
	{	if (DockBar == wMain.ToolBox && wMain.ToolBox.Visible)	m_dhHost.ToolBox = wMain.ToolBox.ToolBox;
		if (DockBar == wMain.PropertyGrid && wMain.PropertyGrid.Visible)
		{	wMain.PropertyGrid.PropertyGrid.ClearComponents();
				wMain.PropertyGrid.PropertyGrid.AddComponents(m_dhHost.ComponentContainer.Components.ToObjectArray());
			m_dhHost.PropertyGrid = wMain.PropertyGrid.PropertyGrid;
		}
	} else
	{	if (DockBar == wMain.ToolBox)	m_dhHost.ToolBox = null;
		if (DockBar == wMain.PropertyGrid && wMain.PropertyGrid.PropertyGrid.ServiceProvider == m_dhHost)
		{	wMain.PropertyGrid.PropertyGrid.ClearComponents();
			m_dhHost.PropertyGrid = null;
		}
	}
}
private void CpoChgSvc_ComponentAdded(object? sender, ComponentEventArgs e)
{	if (e.Component is not null && wMain.PropertyGrid.PropertyGrid.ServiceProvider == m_dhHost)
		wMain.PropertyGrid.PropertyGrid.AddComponents(e.Component);
}
private void Save_Exec(object sender, object? args)
{	IComponent root = m_dhHost.DesignerHost.RootComponent;
	m_dsmSeria.Save("e:\\dd.cs", root, (string?)TypeDescriptor.GetProperties(root)["Namespace"]!.GetValue(root));
}
private void Frm_Click(object sender)						{	Form1 f = new (); f.Show();}
}