using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Drawing.Design;
using NotImplementedException = System.NotImplementedException;
using Wew.Control;
using Wew.Media;
using Wew;
using System.Collections;
using System.Runtime.Serialization;
using System.Reflection;

namespace Demo;

public partial class Form1 : Form
{	readonly cDesignerHost sur;
	readonly PropertyGrid pg;
	readonly IToolboxService tbox; readonly toolitem toolit;
	public Form1()
	{
		InitializeComponent();

		sur = new cDesignerHost();
		Splitter spl = new Splitter() {	Dock = DockStyle.Right};
			Controls.Add(spl);
		pg = new PropertyGrid() {	Dock = DockStyle.Right, PropertySort = PropertySort.Alphabetical, Width = 600
			, Site = new sit(sur)
			};
			Controls.Add(pg);
		ContainerControl cnt = new () {	Dock = DockStyle.Top, AutoSize = true};
			Button btn = new Button() {	Text = "grab", Height = 30};
				btn.Click += Grab_Click;
				cnt.Controls.Add(btn);
			btn = new Button() {	Text = "tool", Height = 30, Left = 120};
				btn.Click += tool_Click;
				cnt.Controls.Add(btn);
			Controls.Add(cnt);
		tbox = new cToolBox();
			sur.ToolBox = tbox;
		toolit = new toolitem();
			tbox.AddToolboxItem(toolit, null);

		sur.SelectionService.SelectionChanged += SelSrv_SelectionChanged;
		sur.ComponentChangeService.ComponentChanged += Form1_ComponentChanged;
		sur.ComponentChangeService.ComponentRename += Form1_ComponentRename;
		sur.ComponentChangeService.ComponentRemoved += Form1_ComponentRemoved;
		WindowState = FormWindowState.Maximized;

		sur.BeginLoad(new ldr());
		Control vw = (Control)sur.View;
			vw.Dock = DockStyle.Fill;
			//vw.BackColor = Color.LightGray;
			Controls.Add(vw); Controls.SetChildIndex(vw, 0);

		ToolStrip ctl=new ToolStrip();
			((Control)sur.DesignerHost.RootComponent).Controls.Add(ctl);
			sur.DesignerHost.Container.Add(ctl);

		ListView lv=new (){	View = View.Details, Location = new System.Drawing.Point(10, 30)};
			_ = lv.Columns.Add("ww");
			((Control)sur.DesignerHost.RootComponent).Controls.Add(lv);
			sur.DesignerHost.Container.Add(lv);

	}
	private void Form1_ComponentRemoved(object? sender, ComponentEventArgs e)
	{
	}
	private void Form1_ComponentRename(object? sender, ComponentRenameEventArgs e)
	{
	}
	private void Form1_ComponentChanged(object? sender, ComponentChangedEventArgs e)
	{
		//pg.Refresh();

		//System.Diagnostics.Debug.Print("chg");
	}
	private void Grab_Click(object? sender, System.EventArgs e)
	{	DesignerSerializationManager ss = new DesignerSerializationManager();	_ = ss.CreateSession();
		TypeCodeDomSerializer dd = new TypeCodeDomSerializer();
			CodeTypeDeclaration cod = dd.Serialize(ss, sur.ComponentContainer.Components[0], null);
       
		CodeNamespace cnNam = new ("Names");	_ = cnNam.Types.Add(cod);

		CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
			CodeGeneratorOptions options = new () {	BracingStyle = "C", IndentString = "\t", BlankLinesBetweenMembers = false};
		CodeCompileUnit targetUnit = new CodeCompileUnit();	_ = targetUnit.Namespaces.Add(cnNam);

		using System.IO.StreamWriter sourceWriter = new ("e:\\dd.cs");

		provider.GenerateCodeFromCompileUnit(targetUnit, sourceWriter, options);
	}
	private void tool_Click(object? sender, System.EventArgs e)	{	tbox.SetSelectedToolboxItem(toolit);}
	private void SelSrv_SelectionChanged(object? sender, System.EventArgs e)
	{	pg.SelectedObjects = ((ISelectionService)sender!).GetSelectedComponents().ToObjectArray();
	}

	class ldr : DesignerLoader
	{	public override void BeginLoad(IDesignerLoaderHost host)
		{	Control ctl, cnt;

			Control raiz = (Control)host.CreateComponent(typeof(Form), "raíz");
			//ctl = (Control)host.CreateComponent(typeof(ToolStrip), "ctl");

                //InheritanceAttribute ia = (InheritanceAttribute)TypeDescriptor.GetAttributes(ctl)[typeof(InheritanceAttribute)];
                //if (ia is null || ia.InheritanceLevel == InheritanceLevel.NotInherited)
                //{
                //}

			ctl = (Control)host.CreateComponent(typeof(Button), "ctl2");
				ctl.Location = new System.Drawing.Point(50, 50);
				raiz.Controls.Add(ctl);
			ctl = new Label() {	Text="lbl", Location = new System.Drawing.Point(150, 50)};
				raiz.Controls.Add(ctl);
			cnt = (Control)host.CreateComponent(typeof(cnt), "ii");
				cnt.Location = new System.Drawing.Point(50, 80);
				raiz.Controls.Add(cnt);

				ctl = new Label() {	Text="lbl2"};
					host.Container.Add(ctl);
					ctl.Location = new System.Drawing.Point(50, 50);
					cnt.Controls.Add(ctl);

			cnt = (Control)host.CreateComponent(typeof(cnt), "cc");
				cnt.Location = new System.Drawing.Point(10, 180);
				raiz.Controls.Add(cnt);

			_ = host.CreateComponent(typeof(cGrid), "grd");

			// ** Fin
			host.EndLoad(null!, true, null);

		}
		public override void Dispose()	{}
	}
	class dsr : ParentControlDesigner
	{
		protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
		{
			base.OnGiveFeedback(e);
		}
		protected override void OnDragEnter(DragEventArgs de)
		{
			base.OnDragEnter(de);
			de.Effect = DragDropEffects.Copy;
		}
		protected override void OnDragOver(DragEventArgs de)
		{
			base.OnDragOver(de);
		}
		protected override void OnDragLeave(System.EventArgs e)
		{
			base.OnDragLeave(e);
		}
		protected override void OnMouseDragBegin(int x, int y)
		{
			base.OnMouseDragBegin(x, y);
		}
		protected override void OnMouseDragMove(int x, int y)
		{
			base.OnMouseDragMove(x, y);
		}
	}
	class toolitem : ToolboxItem
	{	public toolitem() : base(typeof(ToolStrip))	{}
		protected override IComponent[] CreateComponentsCore(IDesignerHost host, IDictionary defaultValues)
		{
			return base.CreateComponentsCore(host, defaultValues);
		}
		protected override void Deserialize(SerializationInfo info, StreamingContext context)
		{
			base.Deserialize(info, context);
		}
		protected override object FilterPropertyValue(string propertyName, object value)
		{
			return base.FilterPropertyValue(propertyName, value);
		}
		protected override System.Type GetType(IDesignerHost host, AssemblyName assemblyName, string typeName, bool reference)
		{
			return base.GetType(host, assemblyName, typeName, reference);
		}
		public override void Initialize(System.Type type)
		{
			base.Initialize(type);
		}
		protected override void OnComponentsCreated(ToolboxComponentsCreatedEventArgs args)
		{
			base.OnComponentsCreated(args);
		}
		protected override void Serialize(SerializationInfo info, StreamingContext context)
		{
			base.Serialize(info, context);
		}
		protected override object ValidatePropertyValue(string propertyName, object value)
		{
			return base.ValidatePropertyValue(propertyName, value);
		}
		public override string Version => base.Version;
		public override void Lock()
		{
			base.Lock();
		}
	}

		[Designer(typeof(dsr))]
	class cnt : GroupBox{}

	private class sit : System.ComponentModel.ISite
	{
		readonly DesignSurface m_sur;
		public sit(DesignSurface sur)	{	this.m_sur = sur;}

		IComponent ISite.Component => throw new System.NotImplementedException();

		IContainer? ISite.Container => null;

		bool ISite.DesignMode => false;

		string? ISite.Name { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

		object? System.IServiceProvider.GetService(System.Type serviceType)
		{
			if (serviceType == typeof(IDesignerHost))	return m_sur.GetService(serviceType);
			if (serviceType == typeof(System.Windows.Forms.AmbientProperties))	return null;
			if (serviceType == typeof(IDesignerEventService))	return null;
			if (serviceType == typeof(IHelpService))	return null;
			if (serviceType == typeof(System.Windows.Forms.Design.IUIService))	return null;
			if (serviceType == typeof(System.Drawing.Design.IPropertyValueUIService))	return null;
			if (serviceType == typeof(IEventBindingService))	return null;

			throw new System.NotImplementedException();
		}
	}

}