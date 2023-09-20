using Wew.Control;
using Wew.Media;

namespace Demo;

class fLayout : cDockControl
{
public fLayout()
{	cStackPanel spn; cButton btn; cSeparator sep;

	Text = "Layout";
	spn = new cStackPanel() {	BorderStyle = eBorderStyle.Dark, Border = new Rect(5)
			, LocationMargin = new Point(10, 10), BackColor = eBrush.Red
			, Direction = eDirection.Bottom, AutoSize = eAutoSize.Both, Separation = 10, Size = new Point(250, 200)};
		btn = new cButton() {	Text = "Button 1", LocationMargin = new Point(5, 5)};
			spn.AddControl(btn);
		sep = new cSeparator() {	LeftMargin = 5};
			spn.AddControl(sep);
		btn = new cButton() {	Text = "Button 2", LeftMargin = 5, RightMargin = 5, Size = new Point(200, 50)};
			spn.AddControl(btn);
		btn = new cButton() {	Text = "Button 3 stretched", LeftMargin = 5, Width = 120, BottomMargin = 0};
			spn.AddControl(btn);
		AddControl(spn);
	// ** Stack panel with reverse order: children must have left/top margin set to NaN
	spn = new cStackPanel() {	BorderStyle = eBorderStyle.Dark, Border = new Rect(5)
			, LocationMargin = new Point(250, 10), BackColor = eBrush.Red
			, Direction = eDirection.Top, /*AutoSize = eAutoSize.Both,*/ Separation = 10, Size = new Point(250, 200)};
		btn = new cButton() {	Text = "Button 1", TopMargin = float.NaN};
			spn.AddControl(btn);
		sep = new cSeparator() {	TopMargin = float.NaN};
			spn.AddControl(sep);
		btn = new cButton() {	Text = "Button 2", TopMargin = float.NaN, Size = new Point(200, 50)};
			spn.AddControl(btn);
		btn = new cButton() {	Text = "Button 3 stretched", Width = 120, TopMargin = 0};
			spn.AddControl(btn);
		AddControl(spn);
	cScrollableControl scl = new () {	BorderStyle = eBorderStyle.Dark, Border = new Rect(5)
			, LocationMargin = new Point(550, 10), BackColor = eBrush.Red
			, /*AutoSize = eAutoSize.Both,*/ Size = new Point(250, 200)};
		btn = new cButton() {	Text = "Button 1", LocationMargin = new Point(50, 50)};
			scl.ClientArea.AddControl(btn);
		sep = new cSeparator() {	LocationMargin = new Point(50, 100), SliceWidth = 0.75f};
			scl.ClientArea.AddControl(sep);
		btn = new cButton() {	Text = "Button 2", LocationMargin = new Point(50, 250), Size = new Point(200, 50)};
			scl.ClientArea.AddControl(btn);
		btn = new cButton() {	Text = "Button 3 stretched", Width = 120, LocationMargin = new Point(250, 350)};
			scl.ClientArea.AddControl(btn);
		AddControl(scl);
}
}