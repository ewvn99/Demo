using Wew.Control;
using Wew.Media;

namespace Demo;

class kPropertyGrid : cDockControl
{	public readonly cPropertyGrid PropertyGrid;
public kPropertyGrid()
{	PropertyGrid = new cPropertyGrid {	Margins = new Rect(0)};
		AddControl(PropertyGrid);
	Text = "Properties"; IsForm = false; HideOnClose = true; Dock = eDirection.Right; Width = PropertyGrid.Width;
}
}