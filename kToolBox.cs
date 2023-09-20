using Wew.Control;
using Wew.Media;
using Wew;

namespace Demo;

class kToolBox : cDockControl
{	public readonly cToolBox ToolBox;
public kToolBox()
{	ToolBox = new cToolBox {	Margins = new Rect(5)};
		AddControl(ToolBox);
	Text = "ToolBox"; IsForm = false; HideOnClose = true; Dock = eDirection.Left; Width = ToolBox.Width;
	ToolBox.AddToolboxItem(typeof(System.Data.DataTable), mRes.BmpTable, "DataTable");
	ToolBox.AddToolboxItem(typeof(cDataReport.cLabel), eResource.Bmp1Page, "Label");
		ToolBox.AddToolboxItem(typeof(cDataReport.cField), eResource.BmpSample, "Field");
		ToolBox.AddToolboxItem(typeof(cDataReport.cBitmap), eResource.BmpFilter, "Bitmap");
}
}