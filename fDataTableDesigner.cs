using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Xml;
using IDictionary = System.Collections.IDictionary;
using Wew.Control;
using Wew.Media;
using Wew;
using System.Collections.Generic;

namespace Demo;

partial class fDataTableDesigner : cDockControl
{	readonly cDesignerHost m_dhHost;
	string? m_sFileName;
public fDataTableDesigner(string? sFileName = null)
{	Text = "Data Table Designer";
	r_Commands.Add(new cCommandState(eCommand.Save, Save_Exec));
	cToolBar tb = new () {	Height = 40};
		_ = tb.Items.Add("form", eResource.Bmp2Pages, Frm_Click);
		AddControl(tb);
	m_dhHost = new cDesignerHost();
		m_dhHost.ComponentChangeService.ComponentAdded += CpoChgSvc_ComponentAdded;
		m_dhHost.CreatingDesigner += m_dhHost_CreatingDesigner;
	// ** Load
	m_dhHost.BeginLoad(new cDsrLoader { FileName = sFileName});
	// ** Show
	cControl ctlView = m_dhHost.GetView();
		ctlView.Margins = new Rect(0); ctlView.BorderStyle = eBorderStyle.Default;
		AddControl(ctlView);
	m_sFileName = sFileName;
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
private void m_dhHost_CreatingDesigner(object sender, IComponent component, bool rootDesigner, ref IDesigner? designer)
{	if (designer is not null)		return;
	if (component is DataTable)	{	designer = new cDsrTbl(); return;}
	if (component is DataColumn)	designer = new cDsrCol();
}
private void Save_Exec(object sender, object? args)
{	if (m_sFileName is null)
	{	string? sPath;
	
		sPath = mDialog.ShowSaveFile("DataTable Designer file|*.dtf", wMain.DsrFileDlgGuid);	if (sPath is null) return;
		m_sFileName = sPath;
	}
	((cDsrRoot)m_dhHost.DesignerHost.GetDesigner(m_dhHost.DesignerHost.RootComponent)!).Save(m_sFileName);
}
private void Frm_Click(object sender)
{	
	//Form1 f = new (); f.Show();

	System.Windows.Forms.IDataObject dob = mClipboard.GetDataObject();
	string ss = (string)dob.GetData("CORE_CF_DESIGNERCOMPONENTS");
	//System.Text.Json.JsonDocument jj = System.Text.Json.JsonDocument.Parse(ss);
	//System.Text.Json.JsonElement.ObjectEnumerator oo = jj.RootElement.EnumerateObject();
	//foreach (System.Text.Json.JsonProperty item in oo)
	//{
	//	System.Text.Json.JsonElement.ObjectEnumerator oo2 = item.Value.EnumerateObject();
	//	foreach (System.Text.Json.JsonProperty item2 in oo2)
	//	{	
	//		System.Text.Json.JsonElement.ArrayEnumerator arr = item2.Value.EnumerateArray();
	//		foreach (System.Text.Json.JsonElement elm in arr)
	//		{	
	//
	//		}
	//	}
	//}

	//byte[] a_by = (byte[])dob.GetData("CF_DESIGNERCOMPONENTS_V2");
	//System.IO.MemoryStream s = new (a_by);
	//System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new ();
	//_ = s.Seek(0, System.IO.SeekOrigin.Begin);
	//object serializationData = formatter.Deserialize(s);

}
}
	partial class fDataTableDesigner
	{
// ** cRoot -------------------------------------------------------------------------------------------------------------------------------
	[Designer(typeof(cDsrRoot), typeof(IRootDesigner))]
class cRoot : cComponent
{		[DefaultValue(null), ParenthesizePropertyName(true), cConstraint(eInputType.Idientifier), Description("Code namespace")]
	public string? Namespace								{	get; set;}
}
// ** cDsrLoader --------------------------------------------------------------------------------------------------------------------------
class cDsrLoader : DesignerLoader
{	public string? FileName;
	public override void BeginLoad(IDesignerLoaderHost host)
	{	cRoot rot;

		// ** Root
		rot = new cRoot();
		host.Container.Add(rot, "Designer");
		// ** Tbls
		if (FileName is not null)
		{	cContainer cntCtlRoot = ((cScrollableControl)((cDesigner)host.GetDesigner(rot)!).DesigControl!).ClientArea;
			XmlDocument xd = new (); XmlElement elmRoot; PropertyDescriptorCollection pdcRoot = TypeDescriptor.GetProperties(rot);
		
			xd.Load(FileName); elmRoot = xd["tables"]!;
			pdcRoot["Name"]!.SetValue(rot, elmRoot.Attributes!["name"]?.Value);
				pdcRoot[nameof(cRoot.Namespace)]!.SetValue(rot, elmRoot.Attributes!["namespace"]?.Value);
			foreach (XmlNode ndTbl in elmRoot.ChildNodes)
			{	DataTable tbl = new (); XmlAttributeCollection xac = ndTbl.Attributes!;
				PropertyDescriptorCollection pdc; cDsrTbl dsr; cControl ctl; bool b;

				host.Container.Add(tbl, ndTbl.Attributes!["name"]!.Value);
				pdc = TypeDescriptor.GetProperties(tbl);
				dsr = (cDsrTbl)host.GetDesigner(tbl)!;
				ctl = dsr.DesigControl!;
				// ** Fields
				dsr.Load(ndTbl);
				// ** Tbl
				pdc[nameof(cDsrTbl.RowType)]!.SetValue(tbl, xac["RowType"]!.Value);
				if (bool.TryParse(xac["Public"]?.Value, out b))	pdc[nameof(cDsrTbl.Public)]!.SetValue(tbl, b);
				if (bool.TryParse(xac["CaseSensitive"]?.Value, out b))	tbl.CaseSensitive = b;
				if (xac["Locale"]?.Value is {} s)	tbl.Locale = new System.Globalization.CultureInfo(s);
				if (int.TryParse(xac["MinimumCapacity"]?.Value, out int i))	tbl.MinimumCapacity = i;
				tbl.Namespace = xac["Namespace"]?.Value;
				tbl.Prefix = xac["Prefix"]?.Value;
				if (System.Enum.TryParse(xac["RemotingFormat"]?.Value, out SerializationFormat sfm))	tbl.RemotingFormat = sfm;
				tbl.TableName = xac["TableName"]?.Value;
				tbl.DisplayExpression = xac["DisplayExpression"]?.Value;	// Can refer to fields
				// ** Ctl
				_ = Rectangle.TryParse(xac["bounds"]!.Value, out Rectangle rt); pdc[nameof(cDsrTbl.Bounds)]!.SetValue(tbl, rt);
				cntCtlRoot.AddControl(ctl);
			}
		}
		// ** End
		host.EndLoad(null!, true, null);
	}
	public override void Dispose()							{}
}
// ** cDsrRoot ----------------------------------------------------------------------------------------------------------------------------
class cDsrRoot : cDocumentDesigner
{	private class cCtlRoot : cScrollableControl
	{	public cCtlRoot(cRoot rotRoot)
		{	((IComponent)this).Site = ((IComponent)rotRoot).Site;
			Margins = new Rect(0);
			BackColor = new cLinearGradientBrush(eColor.LightBlue, eColor.SkyBlue, eExtendMode.Wrap) {	End = new Point(0, 5)};
		}
		public new bool HitTest(Point ptLocation)
		{	return (VerticalBar.Visible & VerticalBar.Container.RealBounds.Contains(ptLocation))
				|| (HorizontalBar.Visible & HorizontalBar.Container.RealBounds.Contains(ptLocation));
		}
		public new void OnMouseWheel(ref MouseArgs e)		{	base.OnMouseWheel(ref e);}
	}
	/*readonly*/ cCtlRoot? m_croCtl;
	public override eResizeMode ResizeMode					=>	eResizeMode.Visible;
	protected override bool r_SnapLinesEnabled				=>	false;
	public override void Initialize(IComponent component)
	{	base.Initialize(component);
		DesigControl = m_croCtl = new cCtlRoot((cRoot)component);
		InitializeRoot();
	}
	public void Save(string sFileName)
	{	using XmlWriter xw = XmlWriter.Create(sFileName, new XmlWriterSettings {	Indent = true, IndentChars = "\t"});

		// ** Save
		xw.WriteStartElement("tables");
			xw.WriteAttributeString("name", Component.Site!.Name);
			string? s = ((cRoot)Component).Namespace;	if (!string.IsNullOrEmpty(s))	xw.WriteAttributeString("namespace", s);
			// ** Tables
			foreach (cControl ctl in m_croCtl!.ClientArea.Controls)
			{	if (DesignerHost!.GetDesigner(ctl, out cDesigner? dsr))	(dsr as cDsrTbl)?.Save(xw);
			}
		// ** Generate
		m_Generate(sFileName);
	}
	protected override bool OnHitTest(Point location)		{	return m_croCtl!.HitTest(location);}
	protected override void OnMouseWheel(ref MouseArgs e)	{	m_croCtl!.OnMouseWheel(ref e);}
	private void m_Generate(string sFileName)
	{	using System.IO.StreamWriter sw = System.IO.File.CreateText(System.IO.Path.ChangeExtension(sFileName, "Designer.cs"));

		sw.WriteLine("// <auto-generated/>\r\n");
		sw.WriteLine("#nullable enable\r\n");
		sw.WriteLine("using DataRow = System.Data.DataRow;");
		sw.WriteLine("using DataColumn = System.Data.DataColumn;");
		sw.WriteLine("using DataRowBuilder = System.Data.DataRowBuilder;");
		sw.WriteLine("using Serializable = System.SerializableAttribute;");
		sw.WriteLine("using DebuggerStepThrough = System.Diagnostics.DebuggerStepThroughAttribute;");
		sw.WriteLine("using SerializationInfo = System.Runtime.Serialization.SerializationInfo;");
		sw.WriteLine("using StreamingContext = System.Runtime.Serialization.StreamingContext;");
		sw.WriteLine("using MemberNotNull = System.Diagnostics.CodeAnalysis.MemberNotNullAttribute;\r\n");
		string? s = ((cRoot)Component).Namespace;	if (!string.IsNullOrEmpty(s))	sw.WriteLine($"namespace {s};\r\n");
		foreach (cControl ctl in m_croCtl!.ClientArea.Controls)
		{	if (DesignerHost!.GetDesigner(ctl, out cDesigner? dsr) && dsr is cDsrTbl dtb)	dtb.Generate(sw);
		}
	}
}
// ** cDsrTbl -----------------------------------------------------------------------------------------------------------------------------
class cDsrTbl : cDesigner
{	private class cCtlTbl : cContainer
	{	const int TITLE_HEIGHT	= 21;
		const int COL_PK		= 0;
		const int COL_NAME		= 1;
		const int COL_OBJ		= 2;
		const int PK_WIDTH		= 20;
		class cGrd : cGrid
		{	readonly cDsrTbl m_dtbDsr;
			public cGrd(cDsrTbl dtbDsr)						{	m_dtbDsr = dtbDsr;}
			protected override void OnMouseDown(MouseArgs e)	{	base.OnMouseDown(e); OnRowChanged();}
			protected override void OnRowChanged()
			{	if (CurrentRow != -1)
					m_dtbDsr.SelectionService!.SetSelectedComponents(new object[] { this[CurrentRow, COL_OBJ]!}
						, SelectionTypes.Replace);
			}
			protected override void OnMouseUp(MouseArgs e)
			{	base.OnMouseUp(e);
				if (e.Button == eMouseButton.Right)	m_dtbDsr.OnContextMenu(PointToScreen(e.Location));
			}
			protected override void OnDisplayRectangleChanged()
			{	Columns(COL_NAME).Width = (DisplaySize.X - PK_WIDTH).GetPositiveOrZero();
				base.OnDisplayRectangleChanged();
			}
		}
		readonly cToolButton tbtDropDown;
		readonly cGrd grdFields;
		readonly cDsrTbl m_dtbDsr;
		public cCtlTbl(cDsrTbl drdDsr, DataTable tblTbl)
		{	((IComponent)this).Site = tblTbl.Site;
			m_dtbDsr = drdDsr;
			Size = new Point(150, 150);
				Border = new Rect(3);
				MinimumSize += new Point(100, TITLE_HEIGHT);
				BackColor = eBrush.LightSteelBlue;
			FocusMode = eFocusMode.FocusControl;
			tbtDropDown = new cToolButton() {	LeftMargin = float.NaN, RightMargin = 0, Size = new Point(TITLE_HEIGHT, TITLE_HEIGHT)
					, BackColor = null, Bitmap = eResource.BmpArrowDown};
				tbtDropDown.Click += tbtDropDown_Click;
				AddControl(tbtDropDown);
			grdFields = new cGrd(m_dtbDsr) {	Margins = new Rect(0, TITLE_HEIGHT, 0, 0), BorderStyle = eBorderStyle.None
					, ColumnCount = 3, HeaderColumnCount = 1, RowCount = 0};
				grdFields.VerticalBar.AutoHide = true; grdFields.HorizontalBar.AutoHide = false;
				grdFields.Columns(COL_PK).Width = PK_WIDTH;
				grdFields.Columns(COL_OBJ).Visible = false;
				AddControl(grdFields);
		}
		public void Load(XmlNode ndTbl)
		{	XmlNode ndFields = ndTbl["columns"]!; int j;

			grdFields.RowCount = ndFields.ChildNodes.Count; j = 0;
			foreach (XmlNode nd in ndFields.ChildNodes)
			{	DataColumn col = new (); XmlAttributeCollection xac = nd.Attributes!;
				PropertyDescriptorCollection pdc; bool bIsPK, b; int i;

				m_dtbDsr.m_ncnCnt!.Add(col, xac["name"]!.Value);
				pdc = TypeDescriptor.GetProperties(col);
				// ** Field
				col.ColumnName = xac["ColumnName"]!.Value;
				pdc[nameof(DataColumn.DataType)]!.SetValue(col, xac["type"]!.Value);
				if (bool.TryParse(xac["pk"]?.Value, out bIsPK) && bIsPK)	pdc[nameof(cDsrCol.IsPK)]!.SetValue(col, true);
				col.AllowDBNull = (!bool.TryParse(xac["AllowDBNull"]?.Value, out b) || b);
				if (bool.TryParse(xac["AutoIncrement"]?.Value, out b))		col.AutoIncrement = b;
				if (int.TryParse(xac["AutoIncrementSeed"]?.Value, out i))	col.AutoIncrementSeed = i;
				if (int.TryParse(xac["AutoIncrementStep"]?.Value, out i))	col.AutoIncrementStep = i;
				col.Caption = xac["Caption"]?.Value;
				if (System.Enum.TryParse(xac["ColumnMapping"]?.Value, out MappingType mtp))		col.ColumnMapping = mtp;
				if (System.Enum.TryParse(xac["DateTimeMode"]?.Value, out DataSetDateTime ddt))	col.DateTimeMode = ddt;
				pdc[nameof(DataColumn.DefaultValue)]!.SetValue(col, xac["DefaultValue"]?.Value);
				col.Expression = xac["Expression"]?.Value;
				if (int.TryParse(xac["MaxLength"]?.Value, out i))			col.MaxLength = i;
				col.Namespace = xac["Namespace"]?.Value;
				col.Prefix = xac["Prefix"]?.Value;
				if (bool.TryParse(xac["ReadOnly"]?.Value, out b))			col.ReadOnly = b;
				if (bool.TryParse(xac["Unique"]?.Value, out b))				col.Unique = b;
				m_dtbDsr.m_Tbl.Columns.Add(col);
				// ** Row
				if (bIsPK)	grdFields[j, COL_PK] = mRes.BmpKey;
				grdFields[j, COL_NAME] = col.Site!.Name; grdFields[j, COL_OBJ] = col;
				j++;
			}
		}
		public void Save(XmlWriter xwDoc)
		{	xwDoc.WriteStartElement("columns");
				for (int j = 0; j < grdFields.RowCount; j++)
				{	DataColumn col = (DataColumn)grdFields[j, COL_OBJ]!;
					PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(col);

					xwDoc.WriteStartElement("column");
						xwDoc.WriteAttributeString("name", col.Site!.Name);
						xwDoc.WriteAttributeString("ColumnName", col.ColumnName);
						xwDoc.WriteAttributeString("type", (string)pdc[nameof(DataColumn.DataType)]!.GetValue(col)!);
						if ((bool)pdc[nameof(cDsrCol.IsPK)]!.GetValue(col)!)		xwDoc.WriteAttributeString("pk", bool.TrueString);
						if (!col.AllowDBNull)			xwDoc.WriteAttributeString("AllowDBNull", bool.FalseString);
						if (col.AutoIncrement)			xwDoc.WriteAttributeString("AutoIncrement", bool.TrueString);
						if (col.AutoIncrementSeed != 0)	xwDoc.WriteAttributeString("AutoIncrementSeed", col.AutoIncrementSeed.ToString());
						if (col.AutoIncrementStep != 1)	xwDoc.WriteAttributeString("AutoIncrementStep", col.AutoIncrementStep.ToString());
						xwDoc.WriteAttributeString("Caption", col.Caption);
						if (col.ColumnMapping != MappingType.Element)				xwDoc.WriteAttributeString("ColumnMapping", col.ColumnMapping.ToString());
						if (col.DateTimeMode != DataSetDateTime.UnspecifiedLocal)	xwDoc.WriteAttributeString("DateTimeMode", col.DateTimeMode.ToString());
						string s = (string)pdc[nameof(DataColumn.DefaultValue)]!.GetValue(col)!;
							if (!string.IsNullOrEmpty(s))			xwDoc.WriteAttributeString("DefaultValue", s);
						if (!string.IsNullOrEmpty(col.Expression))	xwDoc.WriteAttributeString("Expression", col.Expression);
						if (col.MaxLength != -1)	xwDoc.WriteAttributeString("MaxLength", col.MaxLength.ToString());
						if (!string.IsNullOrEmpty(col.Namespace) && col.Namespace != col.Table!.Namespace)	xwDoc.WriteAttributeString("Namespace", col.Namespace);
						if (!string.IsNullOrEmpty(col.Prefix))		xwDoc.WriteAttributeString("Prefix", col.Prefix);
						if (col.ReadOnly)							xwDoc.WriteAttributeString("ReadOnly", bool.TrueString);
						if (col.Unique)								xwDoc.WriteAttributeString("Unique", bool.TrueString);
					xwDoc.WriteEndElement();
				}
			xwDoc.WriteEndElement();
		}
		public void RefreshField(DataColumn colCol)
		{	int iRow = m_iFieldToRow(colCol);
			
			if (iRow != -1)	grdFields[iRow, COL_NAME] = colCol.Site!.Name;
		}
		public void SelectField(DataColumn colCol)			{	int iRow = m_iFieldToRow(colCol);	if (iRow != -1)	grdFields.CurrentRow = iRow;}
		public void CmdAddField()							{	m_CreateField(grdFields.RowCount, true);}
		public void CmdInsertField(DataColumn colCol)		{	int iRow = m_iFieldToRow(colCol);	if (iRow != -1)	m_CreateField(iRow, true);}
		public void CmdPK(DataColumn colCol)
		{	int iRow = m_iFieldToRow(colCol);
				
				if (iRow == -1)	return;
			DataColumn col = (DataColumn)grdFields[iRow, COL_OBJ]!;
				if (col.Expression != "")	{	mDialog.MsgBoxExclamation($"Column '{col.Site!.Name}' has Expression"); return;} // Has expression: exit
			PropertyDescriptor pd = TypeDescriptor.GetProperties(col)[nameof(cDsrCol.IsPK)]!; bool bIsPK = (bool)pd.GetValue(col)!;

			pd.SetValue(col, !bIsPK); grdFields[iRow, COL_PK] = (!bIsPK ? mRes.BmpKey : null);
		}
		public void CmdDeleteField(DataColumn colCol)
		{	int iRow = m_iFieldToRow(colCol);
				
			if (iRow != -1)	{	m_dtbDsr.m_Tbl.Columns.Remove(colCol); grdFields.RemoveRows(iRow, 1);}
		}
		public new bool HitTest(Point ptLocation)
		{	return grdFields.RealBounds.Contains(ptLocation) || tbtDropDown.RealBounds.Contains(ptLocation);
		}
		protected override void OnPaint(PaintArgs e)
		{	Rectangle rt = ClientRectangle; bool bSelected = m_dtbDsr.SelectionService!.GetComponentSelected(m_dtbDsr.m_Tbl);

			base.OnPaint(e);												// Border
			rt.Height = TITLE_HEIGHT; e.Graphics.FillRectangle(rt, (bSelected ? eBrush.Blue : eBrush.LightSteelBlue)); // Title bar
			rt.Scroll(3, 0);												// Title
				e.Graphics.DrawText(m_dtbDsr.m_Tbl.Site!.Name, rt, (bSelected ? eBrush.White : eBrush.Black)
					, eFont.SystemBoldText, eTextFormat.LeftMiddle);
		}
		protected override void OnClientRectangleChanged()
		{	tbtDropDown.Bitmap = (ClientHeight > TITLE_HEIGHT ? eResource.BmpArrowUp : eResource.BmpArrowDown);
			base.OnClientRectangleChanged();
		}
		private void tbtDropDown_Click(object sender)
		{	float fClientHeight = TITLE_HEIGHT;

			if (ClientHeight <= TITLE_HEIGHT)	fClientHeight += grdFields.RowCount * grdFields.DefaultRowHeight; // ** Expand
			m_SetClientHeight(fClientHeight); 
		}
		private void m_AdjustHeight()
		{	float fClientHeight = TITLE_HEIGHT + grdFields.RowCount * grdFields.DefaultRowHeight;

			if (ClientHeight < fClientHeight)	m_SetClientHeight(fClientHeight);
		}
		private void m_SetClientHeight(float fClientHeight)
		{	TypeDescriptor.GetProperties(m_dtbDsr.m_Tbl)[nameof(cDsrTbl.Bounds)]!.SetValue(m_dtbDsr.m_Tbl
				, new Rectangle(LocationMargin, new Point(ClientWidth, fClientHeight) + Border));
		}
		private void m_CreateField(int iRow, bool bSetDefaults)
		{	DataColumn col = new ();
		
			m_dtbDsr.m_Tbl.Columns.Add(col); col.SetOrdinal(iRow);
			m_dtbDsr.DesignerHost!.Container.Add(col);
			if (bSetDefaults)
			{	((cDesigner)m_dtbDsr.DesignerHost.GetDesigner(col)!).InitializeNewComponent(null);
				m_dtbDsr.SelectionService!.SetSelectedComponents(new IComponent[] { col}, SelectionTypes.Replace);
			}
			grdFields.InsertRows(iRow, 1); grdFields[iRow, COL_NAME] = col.Site!.Name; grdFields[iRow, COL_OBJ] = col;
			grdFields.CurrentRow = iRow;
			m_AdjustHeight();
		}
		private int m_iFieldToRow(DataColumn colCol)
		{	for (int j = 0; j < grdFields.RowCount; j++)	if (grdFields[j, COL_OBJ] == colCol)	return j;
			return -1;
		}
	}
	INestedContainer? m_ncnCnt;												// Allows fields from different tables to have the same name
	private static string s_sQuote(string sStr, bool bSimple = false)		// Sets double or simple quotes
	{	if (bSimple)	return sStr.Replace("'", "''");						// Simple: exit
		return sStr.Replace("\"", "\\\"");									// Double
	}
	public override eResizeMode ResizeMode					=>	eResizeMode.Visible	| eResizeMode.AllSizeable | eResizeMode.Moveable;
	private cCtlTbl m_Ctl									=>	(cCtlTbl)DesigControl!;
	private DataTable m_Tbl									=>	(DataTable)Component!;
	public override void Initialize(IComponent component)
	{	base.Initialize(component);
		DesigControl = new cCtlTbl(this, m_Tbl);
		_ = Verbs.Add(new DesignerVerb("Add field", m_CmdAddField));
		DesignSurface dsf = (DesignSurface)GetService(typeof(DesignSurface));
		m_ncnCnt = dsf.CreateNestedContainer(component);
	}
	public override void InitializeNewComponent(IDictionary defaultValues)
	{	base.InitializeNewComponent(defaultValues);
		Bounds = DesigControl!.Bounds;
		RowType = Component.Site!.Name + "Row";
	}
	public void RefreshField(DataColumn colCol)				{	m_Ctl.RefreshField(colCol);}
	public void SelectField(DataColumn colCol)				{	m_Ctl.SelectField(colCol);}
	public void CmdInsertField(DataColumn colCol)			{	m_Ctl.CmdInsertField(colCol);}
	public void CmdPK(DataColumn colCol)					{	m_Ctl.CmdPK(colCol);}
	public void CmdDeleteField(DataColumn colCol)			{	m_Ctl.CmdDeleteField(colCol);}
	public void Load(XmlNode ndTbl)							{	m_Ctl.Load(ndTbl);}
	public void Save(XmlWriter xwDoc)
	{	DataTable? tbl = m_Tbl;

		xwDoc.WriteStartElement("table");
			xwDoc.WriteAttributeString("name", tbl.Site!.Name);
			xwDoc.WriteAttributeString("RowType", RowType);
			if (Public)	xwDoc.WriteAttributeString("Public", bool.TrueString);
			if (tbl.CaseSensitive)	xwDoc.WriteAttributeString("CaseSensitive", bool.TrueString);
			if (!string.IsNullOrEmpty(tbl.DisplayExpression))	xwDoc.WriteAttributeString("DisplayExpression", tbl.DisplayExpression);
			if (tbl.Locale != System.Globalization.CultureInfo.CurrentCulture)	xwDoc.WriteAttributeString("Locale", tbl.Locale.ToString());
			if (tbl.MinimumCapacity != 50)	xwDoc.WriteAttributeString("MinimumCapacity", tbl.MinimumCapacity.ToString());
			if (!string.IsNullOrEmpty(tbl.Namespace))	xwDoc.WriteAttributeString("Namespace", tbl.Namespace);
			if (!string.IsNullOrEmpty(tbl.Prefix))	xwDoc.WriteAttributeString("Prefix", tbl.Prefix);
			if (tbl.RemotingFormat != SerializationFormat.Xml)	xwDoc.WriteAttributeString("RemotingFormat", tbl.RemotingFormat.ToString());
			if (!string.IsNullOrEmpty(tbl.TableName))	xwDoc.WriteAttributeString("TableName", tbl.TableName);
			xwDoc.WriteAttributeString("bounds", Bounds.ConvertToString());
			// ** Cols
			m_Ctl.Save(xwDoc);
		xwDoc.WriteEndElement();
	}
	public void Generate(System.IO.StreamWriter swDoc)
	{	DataTable? tbl = m_Tbl;
		string sTbl = tbl.Site!.Name!, sRow = RowType!; DataColumnCollection dcc = tbl.Columns; bool bHasPK = false;

		// ** Determine pk
		foreach (DataColumn col in dcc)
		{	if (!(bool)TypeDescriptor.GetProperties(col)[nameof(cDsrCol.IsPK)]!.GetValue(col)!)	{	bHasPK = true; break;}
		}
		// ** Tbl	
		swDoc.WriteLine("	[Serializable]");
		swDoc.WriteLine("	[DebuggerStepThrough]");
		swDoc.WriteLine($"{(Public ? "public " : null)}class {sTbl} : System.Data.TypedTableBase<{sTbl}.{sRow}>");
		// ** Row
		swDoc.WriteLine($"{{	public class {sRow} : DataRow");
			swDoc.WriteLine($"	{{	public readonly new {sTbl} Table;");
			swDoc.WriteLine($"		internal {sRow}(DataRowBuilder builder) : base(builder)	{{ Table = ({sTbl})base.Table;}}");
			// ** Row: props
			foreach (DataColumn col in dcc)
			{	string sTp = (string)TypeDescriptor.GetProperties(col)[nameof(cDsrCol.f_TypeAbbrev)]!.GetValue(col)!, sField = col.Site!.Name!;

				if (col.AllowDBNull)	sTp += '?';
				swDoc.WriteLine($"		public {sTp} {sField}");
				if (col.AllowDBNull)
				{	swDoc.WriteLine($"		{{	get												=>	(!IsNull(Table.m_col{sField}) ? ({sTp})this[Table.m_col{sField}] : null);");
					swDoc.WriteLine($"			set												=>	this[Table.m_col{sField}] = (value ?? System.Convert.DBNull);");
				} else
				{	swDoc.WriteLine($"		{{	get												=>	({sTp})this[Table.m_col{sField}];");
					swDoc.WriteLine($"			set												=>	this[Table.m_col{sField}] = value;");
				}
				swDoc.WriteLine("		}");
			}
			swDoc.WriteLine("	}");
		// ** Tbl: fields
		if (dcc.Count != 0)	{	swDoc.Write("	DataColumn "); i_WriteCols(swDoc, false, false, "m_col"); swDoc.WriteLine(";");}
		// ** Tbl: ctors
		swDoc.WriteLine($"	public {sTbl}()");
		swDoc.Write("	{");
			foreach (DataColumn col in dcc)
			{	string sField = "m_col" + col.Site!.Name; PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(col);
		
				swDoc.Write($"		{sField} = Columns.Add(\"{s_sQuote(col.ColumnName)}\", typeof({(string)pdc[nameof(cDsrCol.f_TypeAbbrev)]!.GetValue(col)!}));");
				if (!col.AllowDBNull)							swDoc.Write($" {sField}.AllowDBNull = false;");
				if (col.AutoIncrement)							swDoc.Write($" {sField}.AutoIncrement = true;");
				if (col.AutoIncrementSeed != 0)					swDoc.Write($" {sField}.AutoIncrementSeed = {col.AutoIncrementSeed};");
				if (col.AutoIncrementStep != 1)					swDoc.Write($" {sField}.AutoIncrementStep = {col.AutoIncrementStep};");
				swDoc.Write($" {sField}.Caption = \"{s_sQuote(col.Caption)}\";");
				if (col.ColumnMapping != MappingType.Element)	swDoc.Write($" {sField}.ColumnMapping = System.Data.MappingType.{col.ColumnMapping};");
				if (col.DateTimeMode != DataSetDateTime.UnspecifiedLocal)	swDoc.Write($" {sField}.DateTimeMode = System.Data.DataSetDateTime.{col.DateTimeMode};");
				if (!string.IsNullOrEmpty((string)pdc[nameof(DataColumn.DefaultValue)]!.GetValue(col)!))	swDoc.Write($" {sField}.DefaultValue = {col.DefaultValue};");
				if (!string.IsNullOrEmpty(col.Expression))		swDoc.Write($" {sField}.Expression = \"{s_sQuote(col.Expression)}\";");
				if (col.MaxLength != -1)						swDoc.Write($" {sField}.MaxLength = {col.MaxLength};");
				if (!string.IsNullOrEmpty(col.Namespace) && col.Namespace != tbl!.Namespace)	swDoc.Write($" {sField}.Namespace = \"{s_sQuote(col.Namespace)}\";");
				if (!string.IsNullOrEmpty(col.Prefix))			swDoc.Write($" {sField}.Prefix = \"{s_sQuote(col.Prefix)}\";");
				if (col.ReadOnly)								swDoc.Write($" {sField}.ReadOnly = true;");
				if (col.Unique)									swDoc.Write($" {sField}.Unique = true;");
				swDoc.WriteLine();
			}
			// ** Props of tbl
			if (bHasPK)	{	swDoc.Write("		PrimaryKey = new DataColumn[] {	"); i_WriteCols(swDoc, false, true, "m_col"); swDoc.WriteLine("};");}
			if (tbl.CaseSensitive)	swDoc.WriteLine("		CaseSensitive = true;");
			if (!string.IsNullOrEmpty(tbl.DisplayExpression))	swDoc.WriteLine($"		DisplayExpression = \"{s_sQuote(tbl.DisplayExpression)}\";"); // Can ref to fields
			if (tbl.Locale != System.Globalization.CultureInfo.CurrentCulture)	swDoc.WriteLine($"		Locale = new System.Globalization.CultureInfo(\"{s_sQuote(tbl.Locale.ToString())}\");");
			if (tbl.MinimumCapacity != 50)	swDoc.WriteLine($"		MinimumCapacity = {tbl.MinimumCapacity};");
			if (!string.IsNullOrEmpty(tbl.Namespace))	swDoc.WriteLine($"		Namespace = \"{s_sQuote(tbl.Namespace)}\";");
			if (!string.IsNullOrEmpty(tbl.Prefix))	swDoc.WriteLine($"		Prefix = \"{s_sQuote(tbl.Prefix)}\";");
			if (tbl.RemotingFormat != SerializationFormat.Xml)	swDoc.WriteLine($"		RemotingFormat = System.Data.SerializationFormat.{tbl.RemotingFormat};");
			if (!string.IsNullOrEmpty(tbl.TableName))	swDoc.WriteLine($"		TableName = \"{s_sQuote(tbl.TableName)}\";");
		swDoc.WriteLine("	}");
		swDoc.WriteLine($"	protected {sTbl}(SerializationInfo info, StreamingContext context) : base(info, context)	{{	m_Ctor();}}");
		swDoc.WriteLine($"	public override System.Data.DataTable Clone()				{{	{sTbl} tbl = ({sTbl})base.Clone();	tbl.m_Ctor(); return tbl;}}");
		swDoc.Write("		[MemberNotNull("); i_WriteCols(swDoc, false, false, "nameof(m_col", ")"); swDoc.WriteLine(")]");
		swDoc.WriteLine("	private void m_Ctor()");
		swDoc.Write("	{");
			foreach (DataColumn col in dcc)	swDoc.WriteLine($"		m_col{col.Site!.Name} = Columns[\"{s_sQuote(col.ColumnName)}\"]!;");
		swDoc.WriteLine("	}");
		// ** Tbl: props
		swDoc.WriteLine($"	public {sRow} this[int index]							=>	({sRow})Rows[index];");
		foreach (DataColumn col in dcc)	swDoc.WriteLine($"	public DataColumn {col.Site!.Name}									=>	m_col{col.Site!.Name};");
		swDoc.WriteLine("	public int Count										=>	Rows.Count;");
		// ** Tbl: mets
		swDoc.WriteLine($"	public {sRow} New()										{{	return ({sRow})NewRow();}}");
		swDoc.Write($"	public {sRow} Add("); i_WriteCols(swDoc, true, false); swDoc.Write($")	{{	return ({sRow})Rows.Add("); i_WriteCols(swDoc, false, false); swDoc.WriteLine(");}");
		swDoc.WriteLine($"	public void Add({sRow} {sRow})							{{	Rows.Add({sRow});}}");
		swDoc.WriteLine($"	public void Remove({sRow} {sRow})						{{	Rows.Remove({sRow});}}");
		if (bHasPK)
		{	swDoc.Write($"	public {sRow}? Find("); i_WriteCols(swDoc, true, true); swDoc.Write($")								{{	return ({sRow}?)Rows.Find("); i_WriteCols(swDoc, false, true); swDoc.WriteLine(");}");
		}
		swDoc.WriteLine($"	protected override DataRow NewRowFromBuilder(DataRowBuilder builder)	{{	return new {sRow}(builder);}}");
		swDoc.WriteLine($"	protected override System.Type GetRowType()				{{	return typeof({sRow});}}");
		swDoc.WriteLine("}");
	// ** Submets
		void i_WriteCols(System.IO.StreamWriter swDoc, bool bWriteTp, bool bOnlyPK, string? sPref = null, string? sSuf = null)
		{	bool bComma = false;

			foreach (DataColumn col in dcc)
			{	if (bOnlyPK && !(bool)TypeDescriptor.GetProperties(col)[nameof(cDsrCol.IsPK)]!.GetValue(col)!)	continue; // Asking for pk and it's not: ommit
				if (bComma)	swDoc.Write(", ");
				if (bWriteTp)	swDoc.Write($"{(string)TypeDescriptor.GetProperties(col)[nameof(cDsrCol.f_TypeAbbrev)]!.GetValue(col)!} ");
				swDoc.Write(sPref); swDoc.Write(col.Site!.Name); swDoc.Write(sSuf); bComma = true;
			}
		}
	}
	protected override void PreFilterProperties(IDictionary properties)
	{	base.PreFilterProperties(properties);
		properties["Bounds"] = TypeDescriptor.CreateProperty(typeof(cDsrTbl), "Bounds", typeof(Rectangle));
			properties["RowType"] = TypeDescriptor.CreateProperty(typeof(cDsrTbl), "RowType", typeof(string));
			properties["Public"] = TypeDescriptor.CreateProperty(typeof(cDsrTbl), "Public", typeof(bool));
			properties["Namespace"] = TypeDescriptor.CreateProperty(typeof(DataTable)
				, (PropertyDescriptor)properties["Namespace"]!, new System.Attribute[] { new DefaultValueAttribute(string.Empty)});
			properties["Columns"] = TypeDescriptor.CreateProperty(typeof(DataTable)
				, (PropertyDescriptor)properties["Columns"]!, new System.Attribute[] { new BrowsableAttribute(false)});
		properties.Remove("Constraints");
			properties.Remove("PrimaryKey");
	}
	protected override void OnSetBounds(bool IsEnd, in Rectangle bounds)
	{	if (!IsEnd)
			DesigControl!.Bounds = bounds;
		else
			TypeDescriptor.GetProperties(Component)[nameof(Bounds)]!.SetValue(Component, DesigControl!.Bounds);
	}
	protected override bool OnHitTest(Point location)		{	return m_Ctl.HitTest(location);}
	private void m_CmdAddField(object? sender, System.EventArgs arg)	{	m_Ctl.CmdAddField();}
#region Props
#pragma warning disable IDE1006												// Name styles
		[Browsable(false), DesignOnly(true)]
	internal Rectangle Bounds												// ** It's internal only to get the name of the property
	{	get													=>	DesigControl!.Bounds;
		set													=>	DesigControl!.Bounds = value;
	}
		[ParenthesizePropertyName(true), MergableProperty(false), cConstraint(eInputType.Idientifier), Description("Entity name")
		, DesignOnly(true)]
	internal string? RowType								{	get; set;}	// ** It's internal only to get the name of the property
		[Description("Public or internal"), DefaultValue(false), DesignOnly(true)]
	internal bool Public									{	get; set;}	// ** It's internal only to get the name of the property
#pragma warning restore IDE1006												// Name styles
#endregion
}
// ** cDsrCol -----------------------------------------------------------------------------------------------------------------------------
class cDsrCol : cDesigner
{	private class cConv : System.ComponentModel.TypeConverter
	{	static readonly string[] sa_sTps = new string[]	{	"System.Boolean", "System.Byte", "System.Byte[]", "System.Char"
			, "System.DateTime", "System.DateTimeOffset", "System.Decimal", "System.Double", "System.Guid"
			, "System.Int16", "System.Int32", "System.Int64", "System.Object", "System.SByte", "System.Single"
			, "System.String", "System.TimeSpan", "System.UInt16", "System.UInt32", "System.UInt64"
			, "Wew.Media.cBitmapBase"};
		public override bool CanConvertFrom(ITypeDescriptorContext? context, System.Type sourceType)
		{	if (sourceType == typeof(string))	return true;
			return base.CanConvertFrom(context, sourceType);
		}
		public override object? ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
		{	if (value.GetType() == typeof(string))	return (string)value;
			return base.ConvertFrom(context, culture, value);
		}
		public override bool GetStandardValuesSupported(ITypeDescriptorContext? context)	{	return true;}
		public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)	{	return new StandardValuesCollection(sa_sTps);}
	}
	string m_sTp = "System.String", m_sTpAbrev = "string";
	private DataColumn? m_Column							=>	(DataColumn)Component;
	private cDsrTbl? m_DsrTbl								=>	(cDsrTbl?)DesignerHost!.GetDesigner(m_Column!.Table!);
	public override void Initialize(IComponent component)
	{	base.Initialize(component);
		_ = Verbs.Add(new DesignerVerb("Insert field", m_CmdInsertField, new CommandID(default, 0)));
		_ = Verbs.Add(new DesignerVerb("Primary key", m_CmdPK, new CommandID(default, 1)));
	}
	public override void InitializeNewComponent(IDictionary defaultValues)
	{	base.InitializeNewComponent(defaultValues);
		((DataColumn)Component).AllowDBNull = false;
		DataType = ((DataColumn)Component).DataType.ToString();
	}
	protected override void PreFilterProperties(IDictionary properties)
	{	base.PreFilterProperties(properties);
		properties["DataType"] = TypeDescriptor.CreateProperty(typeof(cDsrCol), "DataType", typeof(string));
			properties["f_TypeAbbrev"] = TypeDescriptor.CreateProperty(typeof(cDsrCol), "f_TypeAbbrev", typeof(string));
			properties["DefaultValue"] = TypeDescriptor.CreateProperty(typeof(cDsrCol), "DefaultValue", typeof(string));
			properties["AllowDBNull"] = TypeDescriptor.CreateProperty(typeof(DataColumn)
				, (PropertyDescriptor)properties["AllowDBNull"]!, new DefaultValueAttribute(false));
			properties["Namespace"] = TypeDescriptor.CreateProperty(typeof(DataColumn)
				, (PropertyDescriptor)properties["Namespace"]!, new System.Attribute[] { new DefaultValueAttribute(string.Empty)});
			properties["Expression"] = TypeDescriptor.CreateProperty(typeof(cDsrCol)
				, (PropertyDescriptor)properties["Expression"]!);
		properties["IsPK"] = TypeDescriptor.CreateProperty(typeof(cDsrCol), "IsPK", typeof(bool));
	}
	protected override void OnComponentRenaming(ComponentRenameEventArgs e)	{	m_DsrTbl!.RefreshField(m_Column!);}
	protected override void DeleteComponent()				{	m_DsrTbl!.CmdDeleteField(m_Column!); base.DeleteComponent();}
	protected override void OnSelected()					{	m_DsrTbl!.SelectField(m_Column!);}
	private void m_CmdInsertField(object? sender, System.EventArgs arg)	{	m_DsrTbl!.CmdInsertField(m_Column!);}
	private void m_CmdPK(object? sender, System.EventArgs arg)	{	m_DsrTbl!.CmdPK(m_Column!);}
#region Props
#pragma warning disable IDE1006												// Name styles
		[DefaultValue("System.String"), Description("Data type.  If it's an user type, it's considered as String only during design")
		, TypeConverter(typeof(cConv))]
	private string DataType
	{	get													=>	m_sTp;
		set
		{		if (value is null)	throw new System.ArgumentNullException(nameof(value));
			m_sTp = value;
			m_sTpAbrev = value switch
				{	"System.Int32"		=>	"int",
					"System.String"		=>	"string",
					"System.Boolean"	=>	"bool",
					"System.Int16"		=>	"short",
					"System.Int64"		=>	"long",
					"System.Byte"		=>	"byte",
					"System.Byte[]"		=>	"byte[]",
					"System.Single"		=>	"float",
					"System.Double"		=>	"double",
					"System.Char"		=>	"char",
					"System.Object"		=>	"object",
					"System.SByte"		=>	"sbyte",
					"System.UInt32"		=>	"uint",
					"System.UInt16"		=>	"ushort",
					"System.UInt64"		=>	"ulong",
					_					=>	value
				};
			((DataColumn)Component).DataType = (System.Type.GetType(value) ?? typeof(string)); // ** To allow to edit DefaultValue, DateTimeMode, etc
		}
	}
		[DefaultValue(null), Description("You must enter code for this property")]
	private string? DefaultValue							{	get; set;}
	private string Expression
	{	get													=>	m_Column!.Expression;
		set
		{	if (value != "" && IsPK)	m_DsrTbl!.CmdPK(m_Column!);			// Remove PK
			m_Column!.Expression = value;
		}
	}
		[DefaultValue(false), Browsable(false), DesignOnly(true)]
	internal bool IsPK										{	get; set;}	// ** It's internal only to get the name of the property
		[Browsable(false), DesignOnly(true)]
	internal string f_TypeAbbrev							=>	m_sTpAbrev;	// ** It's internal only to get the name of the property
#pragma warning restore IDE1006												// Name styles
#endregion
}
	}