using Wew.Control;
using Wew.Media;

namespace Demo;

class fServices : cDockControl
{	static byte[]? sa_byKey, sa_byIV;
public fServices()
{	cTabControl tc; cTabControl.cTab tab; cButton btn;

	Text = "Services";
	tc = new cTabControl {	Margins = new Rect(5)};
	tab = tc.Tabs.Add("Encryption");
		btn = new cButton {	Text = "Encrypt", LocationMargin = new Point(50, 50)};
			btn.Click += btnEncrypt_Click;
			tab.Content.AddControl(btn);
		btn = new cButton {	Text = "Decrypt", LocationMargin = new Point(50, 150)};
			btn.Click += btnDecrypt_Click;
			tab.Content.AddControl(btn);
	AddControl(tc);
}
private void btnEncrypt_Click(object sender)
{	if (sa_byKey is null)	Wew.cCryptoFile.GenerateKey(out sa_byKey, out sa_byIV); // ** Generate key
	using Wew.cCryptoFile crf = new ("aa.txt", System.IO.FileMode.Create, sa_byKey, sa_byIV!); // Create stream
	
	crf.WriteBool(true);													// Write
	crf.WriteString("string");
	mDialog.MsgBoxInformation("Written: True, string", "Encrypt");
}
private void btnDecrypt_Click(object sender)
{		if (sa_byKey is null)	return;
	bool b; string s;
	using Wew.cCryptoFile crf = new ("aa.txt", System.IO.FileMode.Open, sa_byKey, sa_byIV!); // Open stream
	
	b = crf.ReadBool();														// Read
	s = crf.ReadString();
	mDialog.MsgBoxInformation($"Read: {b}, {s}", "Decrypt");
}
}