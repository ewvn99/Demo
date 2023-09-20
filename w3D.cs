using System.Collections;
using Wew.Control;
using Wew.Media;
using Wew;

namespace Demo;

class w3D : cWindow
{	const int TERRAIN_WIDTH					= 2049;
	const int TERRAIN_DEPTH					= 2049;
	const int TERRAIN_HEIGHT				= 50;
	const int TERRAIN_ROWCOL_COUNT			= 32;
	const int TERRAIN_QUAD_SIZE				= 64;
	const int TERRAIN_QUAD_ORIG_X			= -TERRAIN_WIDTH / 2;
	const int TERRAIN_QUAD_ORIG_Z			= TERRAIN_DEPTH / 2;
	const int TERRAIN_SAMP_WRAP_SLOT		= 3;
	const int TERRAIN_TEX_LAYERS_SLOT		= 5;
	const int TERRAIN_TEX_BLEND_SLOT		= 6;
	const int TERRAIN_TEX_HEIGHT_SLOT		= 7;
	const int PARTICLE_COUNT				= 100;
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	struct VertexTerrain
	{	public Vector Position;
		public Point Texcoord;
		public Point MinMax;
		public VertexTerrain(Vector position, Point texcoord)	{	this.Position = position; this.Texcoord = texcoord; MinMax = default;}
	}
		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Size = 16)]
	struct BufFire
	{	public int TexIdx;
	}
	class cCam : cCamera
	{	readonly w3D m_w3dWnd;
		public bool Moved;
		public cCam(w3D window) : base(1, 1)				{	m_w3dWnd = window;}
		protected override void OnPositionChanged()			{	base.OnPositionChanged(); Moved = true; m_w3dWnd.OnCamMoved();}
	}
	readonly c3DSimpleBufferNoCache<Matrix4x4> m_bufReflect;
	readonly c3DSimpleBuffer<BufFire> m_bufFire;
	readonly cCam m_camCam; readonly cCamera m_camWater, m_camTeapot;
	readonly c3DModel m_mdlSky, m_mdlTerrain, m_mdlTrees1, m_mdlTrees2, m_mdlWater, m_mdlFire, m_mdlWall, m_mdlModel, m_mdlTeapot
		, m_mdlRain;
	readonly cDepthStencilState m_dssSkyDepthLessEqual;
	float m_fFireTime;
	readonly cTexture m_texTerrainHeight, m_texTerrain, m_texTerrainBlend; readonly float[] ma_fTerrainHeight;
	c3DBuffer m_bufRain1, m_bufRain2;
	readonly cGeometryShader m_gsRainGenerate, m_gsRain; readonly cVertexShader m_vsRain;
	readonly cBulbLight m_bulBulb; readonly cFlashLight m_flaFlash;
	readonly cGame3DSound m_gs3Music, m_gs3Sound;
	static readonly Vector[] sa_vCubeFront = {	Vector.XAxis, -Vector.XAxis, Vector.YAxis, -Vector.YAxis, Vector.ZAxis, -Vector.ZAxis};
	static readonly Vector[] sa_vCubeUp = {	Vector.YAxis, Vector.YAxis, -Vector.ZAxis, Vector.ZAxis, Vector.YAxis, Vector.YAxis};
public w3D()
{	// ** Create buffers
	m_bufReflect = new c3DSimpleBufferNoCache<Matrix4x4>(this, e3DBind.ConstantBuffer); m_bufReflect.AssignToVS(3);
	m_bufFire = new c3DSimpleBuffer<BufFire>(this, e3DBind.ConstantBuffer); m_bufFire.AssignToPS(2);
	// ** Create sky
	m_mdlSky = new c3DModel(this, GetType(), "Res.Sphere.mdl")
		{	VertexShader = new cVertexShader(this, mShaders.SkyVS, VertexColor.InputLayout), ScaleX = 1.7f, ScaleY = 1.7f, ScaleZ = 1.7f
		};
		m_mdlSky.Material.PixelShader = new cPixelShader(this, mShaders.SkyPS);
		m_mdlSky.Material.CubeTexture = new cTexture(this, GetType(), "Res.Cube.dds");
		m_dssSkyDepthLessEqual = new cDepthStencilState(this, true, eComparisonFunction.LessEqual);
	// ** Create terrain
	ma_fTerrainHeight = System.Array.ConvertAll(mRes.BinTerrain, (byte b) => b / 255f * TERRAIN_HEIGHT);
	VertexTerrain[] a_verTerrainVertices = new VertexTerrain[TERRAIN_ROWCOL_COUNT * TERRAIN_ROWCOL_COUNT]; // Quad vertices
		for (int j = 0, y = 0; j < TERRAIN_ROWCOL_COUNT; j++, y += TERRAIN_QUAD_SIZE)
		{	for (int i = 0, x = 0; i < TERRAIN_ROWCOL_COUNT; i++, x+= TERRAIN_QUAD_SIZE)
				a_verTerrainVertices[j * TERRAIN_ROWCOL_COUNT + i] =
					new VertexTerrain(
						new Vector(TERRAIN_QUAD_ORIG_X + x, ma_fTerrainHeight[y * TERRAIN_WIDTH + x], TERRAIN_QUAD_ORIG_Z - y)
						, new Point(1f / TERRAIN_ROWCOL_COUNT * i, 1f / TERRAIN_ROWCOL_COUNT * j));
		}
	short[] a_shTerrainIndices = new short[TERRAIN_ROWCOL_COUNT * TERRAIN_ROWCOL_COUNT * 4]; // Quad indices
		for (int j = 0, y = 0, k = 0; j < TERRAIN_ROWCOL_COUNT - 1; j++, y += TERRAIN_QUAD_SIZE)
		{	for (int i = 0, x = 0; i < TERRAIN_ROWCOL_COUNT - 1; i++, k += 4)
			{	a_shTerrainIndices[k] = (short)(j * TERRAIN_ROWCOL_COUNT + i);
				a_shTerrainIndices[k + 1] = (short)(j * TERRAIN_ROWCOL_COUNT + i + 1);
				a_shTerrainIndices[k + 2] = (short)((j + 1) * TERRAIN_ROWCOL_COUNT + i);
				a_shTerrainIndices[k + 3] = (short)((j + 1) * TERRAIN_ROWCOL_COUNT + i + 1);

				float fMin = float.MaxValue, fMax = float.MinValue;
			
				for (int cx = 0; cx < TERRAIN_QUAD_SIZE; cx++, x++)
				{	for (int cy = 0, y2 = y; cy < TERRAIN_QUAD_SIZE; cy++, y2++)
					{	float f = ma_fTerrainHeight[y2 * TERRAIN_WIDTH + x];
						fMin = System.Math.Min(fMin, f); fMax = System.Math.Max(fMax, f);
					}
				}
				a_verTerrainVertices[a_shTerrainIndices[k]].MinMax = new Point(fMin, fMax);
			}
		}
	m_mdlTerrain = new c3DModel(this, a_verTerrainVertices, a_shTerrainIndices)
		{	VertexShader = new cVertexShader(this, mShaders.TerrainVS
				, new cVertexShader.Field[]
					{	new (eSemantic.Position), new (eSemantic.Texcoord), new ("MINMAX", 0, eVertexFieldType.Point)
					})
			, Topology = eVertexTopology.Patch4
			, HullShader = new cHullShader(this, mShaders.TerrainHS)
			, DomainShader = new cDomainShader(this, mShaders.TerrainDS)
			//, MeshVisible = true
		};
		m_mdlTerrain.Material.PixelShader = new cPixelShader(this, mShaders.TerrainPS);
	m_texTerrainHeight = new cTexture(this, TERRAIN_WIDTH, TERRAIN_DEPTH, e3DBind.ShaderResource, ePixelFormat.Bits32RFloat, 1, false
		, ma_fTerrainHeight, (TERRAIN_WIDTH * sizeof(float)));
	m_texTerrain = new cTexture(this, GetType(), "Res.Terrain.dds");
	m_texTerrainBlend = new cTexture(this, GetType(), "Res.TerrainBlend.dds");//¿mipmap
	// ** Create trees
	m_mdlTrees1 = new c3DModel(this
				, new VertexBillboard(new Point(3, 3), m_vGetTerrainVectorAt(370, 0, -500))
				, new VertexBillboard(new Point(1.5f, 2.7f), m_vGetTerrainVectorAt(480, 0, -540))
				, new VertexBillboard(new Point(1.3f, 2.9f), m_vGetTerrainVectorAt(430, 0, -500)));
		m_mdlTrees1.Material.Texture = new cTexture(this, GetType(), "Res.Tree1.dds");
	m_mdlTrees2 = new c3DModel(this
				, new VertexBillboard(new Point(1.3f, 2.9f), m_vGetTerrainVectorAt(450, 0, -552))
				, new VertexBillboard(new Point(1.5f, 2.7f), m_vGetTerrainVectorAt(360, 0, -520))
				, new VertexBillboard(new Point(1, 1), m_vGetTerrainVectorAt(350, 0, -590)));
		m_mdlTrees2.Material.Texture = new cTexture(this, GetType(), "Res.Tree2.dds");
	// ** Create water
	m_mdlWater = new c3DModel(this, GetType(), "Res.Water.mdl")
		{	VertexShader = new cVertexShader(this, mShaders.ReflectVS, Vertex.InputLayout)
			, Location = m_vGetTerrainVectorAt(417, 1, -543), ScaleX = 5, ScaleZ = 7
		};
		m_mdlWater.Material.PixelShader = new cPixelShader(this, mShaders.ReflectPS);
		m_mdlWater.Material.Texture = new cTexture(this, 400, 400, (e3DBind.RenderTarget | e3DBind.ShaderResource));
		m_mdlWater.Material.AllowTransparency = true; m_mdlWater.Material.Alpha = 0.5f;
		m_camWater = new cCamera(400, 400, 90, 0.1f, 100);
	// ** Create fire
	m_mdlFire = new c3DModel(this, new VertexBillboard(new Point(0.5f, 0.5f), m_vGetTerrainVectorAt(400, 0, -515)));
		m_mdlFire.Material.PixelShader = new cPixelShader(this, mShaders.PSFire);
		m_mdlFire.Material.Texture = new cTexture(this, GetType(), "Res.Fire.dds");
	// ** Create wall
	m_mdlWall = new c3DModel(this, GetType(), "Res.Wall.mdl")	{	Location = m_vGetTerrainVectorAt(426, 5, -528), Yaw = -135};
	// ** Create model
	m_mdlModel = new cMan(this) {	Yaw = 0};
	VertexInstance[] a_vsi =												// ** Instance data
		{	new VertexInstance {	World = new Matrix4x4(0, 45, 0, 0.9f, 0.7f, 0.9f, m_vGetTerrainVectorAt(400, 1, -548)).GetTransposed()}
			, new VertexInstance {	World = new Matrix4x4(0, 0, 0, 1, 1, 1, m_vGetTerrainVectorAt(468, 0, -513)).GetTransposed()}
			, new VertexInstance {	World = new Matrix4x4(0, -45, 0, 0.9f, 0.5f, 0.9f, m_vGetTerrainVectorAt(502, 0, -583)).GetTransposed()}
		};
		m_mdlModel.CreateInstances(a_vsi.Length, a_vsi, new cVertexShader(this, mShaders.VSInstance, VertexInstance.InputLayout));
	// ** Create teapot
	m_mdlTeapot = new c3DModel(this, GetType(), "Res.Teapot.mdl")
		{	Location = m_vGetTerrainVectorAt(405, 20, -521)
			, ShadowModel = new cPlane(this) {	Location = m_vGetTerrainVectorAt(405, 20, -521)}
		};
		m_mdlTeapot.Material.CubeTexture = new cTexture(this, 256, 256, (e3DBind.RenderTarget | e3DBind.ShaderResource), IsCube: true);
	foreach (c3DModel mdl in m_mdlTeapot.Parts)	mdl.Material.CubeTexture = m_mdlTeapot.Material.CubeTexture;
	m_camTeapot = new cCamera(256, 256, 90, 0.1f, 100);
	// ** Create rain
	m_bufRain1 = new c3DBuffer(this, (e3DBind.Vertex | e3DBind.StreamOutput), PARTICLE_COUNT, VertexParticle.SIZE_OF);
	m_bufRain2 = new c3DBuffer(this, (e3DBind.Vertex | e3DBind.StreamOutput), PARTICLE_COUNT, VertexParticle.SIZE_OF);
	m_mdlRain = new c3DModel(this)
		{	Topology = eVertexTopology.Point
			, Vertices = m_bufRain1, VertexCount = 1
			, Location = m_vGetTerrainVectorAt(397, 0, -545)
		};
		m_mdlRain.Material.PixelShader = new cPixelShader(this, mShaders.RainPS);
		m_mdlRain.Material.Texture = new cTexture(this, GetType(), "Res.Rain.dds");
	m_gsRainGenerate = new cGeometryShader(this, mShaders.RainGSGenerate, VertexParticle.StreamOutputLayout);
	m_vsRain = new (this, mShaders.RainVS, VertexParticle.InputLayout);
	m_gsRain = new cGeometryShader(this, mShaders.RainGS);
	// ** Create lights
	SunLight.Color = eColor.Red; SunLight.Location = m_vGetTerrainVectorAt(405, 80, -521); SunLight.CastsShadows = true;
	m_bulBulb = new cBulbLight {	Color = eColor.Blue, Location = m_vGetTerrainVectorAt(405, 60, -501)
		, Range = 130, LinearAttenuation = 0.7f, QuadraticAttenuation = 0.3f, CastsShadows = true};
	m_flaFlash = new cFlashLight {	Color = eColor.Green, Location = m_vGetTerrainVectorAt(405, 60, -521)
		, Range = 150, LinearAttenuation = 0, QuadraticAttenuation = 0
		, Falloff = 0.5f, UmbraAngle = 35, PenumbraAngle = 55, CastsShadows = true};
	// ** Create sounds
	m_gs3Music = new cGame3DSound(GetType(), "Res.Music.mp3", true);
		m_gs3Music.SetEffect(cGameSound.Reverb.DEFAULT);
		m_gs3Music.SetLocation(m_mdlTeapot);
		m_gs3Music.Play(true);
	m_gs3Sound = new cGame3DSound(GetType(), "Res.Hit.wav");
		m_gs3Sound.SetEffect(cGameSound.Reverb.DEFAULT);
		m_gs3Sound.SetLocation(m_mdlModel);
	// ** Create camera
	m_camCam = new cCam(this);
		m_camCam.Location = m_vGetTerrainVectorAt(406, 0, -585);			// Move cam after variable is set and all object are created
	m_UpdateCamY(); m_camCam.Pitch = 10;									// ** Set properties after field is assigned
	// ** Configure window
	ClientSize = new Point(856, 459); CenterToScreen();
	Icon = mRes.BmpCube; Text = "3D";
	r_NotifyRender3D = true;												// Send event to draw 3d
	Root.Hide();                                                            // Hide the main 2D container
	//Paused = false;															// Set repeated mode
	InitializeRandom();
	InitializeShadows();
}
protected override void OnClosed(eCloseReason reason)
{	m_bufReflect.Dispose();
	m_bufFire.Dispose();
	m_mdlSky.VertexShader.Dispose();
		m_mdlSky.Material.PixelShader.Dispose();
		m_mdlSky.Dispose();
		m_dssSkyDepthLessEqual.Dispose();
	m_mdlTerrain.VertexShader.Dispose();
		m_mdlTerrain.HullShader?.Dispose();
		m_mdlTerrain.DomainShader?.Dispose();
		m_mdlTerrain.Material.PixelShader.Dispose();
		m_mdlTerrain.Dispose();
		m_texTerrainHeight.Dispose(); m_texTerrain.Dispose(); m_texTerrainBlend.Dispose();
	m_mdlTrees1.Dispose();
	m_mdlTrees2.Dispose();
	m_mdlWater.VertexShader.Dispose();
		m_mdlWater.Material.PixelShader.Dispose();
		m_mdlWater.Dispose();
	m_mdlFire.Material.PixelShader.Dispose();
		m_mdlFire.Dispose();
	m_mdlWall.Dispose();
	m_mdlModel.VertexShader.Dispose();
		m_mdlModel.Dispose();
	m_mdlTeapot.Dispose();
	m_mdlRain.Material.PixelShader.Dispose();
		m_mdlRain.Dispose();
		m_bufRain1.Dispose(); m_bufRain2.Dispose();
		m_gsRainGenerate.Dispose(); m_vsRain.Dispose(); m_gsRain.Dispose();
	m_gs3Music.Dispose(); m_gs3Sound.Dispose();
	base.OnClosed(reason);
	System.GC.Collect();
}
protected override void OnResize()							{	m_camCam.Size = ClientSize;} // Resize the camera when the view is resized
protected override void OnCompose(float ElapsedSeconds)
{	_ = m_mdlModel.Act(ElapsedSeconds);
	m_bufFire.Data.TexIdx = (int)m_fFireTime % 6; m_bufFire.Write(); m_fFireTime += ElapsedSeconds * 10 * 0.5f;
	m_gs3Music.CalculateAudio();
	//m_camCam.Yaw += 0.2f;
}
 int ii=1;//¿
protected override void OnRender3D(c3DGraphics g3d)
{	// ** Reset
	g3d.SetDefaults(m_camCam);												// Clear depth buffer; set camera and matrix and material buffers
	// ** Generate particles
	m_mdlRain.VertexShader = VSParticle; m_mdlRain.GeometryShader = m_gsRainGenerate; g3d.SOSetTarget(m_bufRain2);
		g3d.Render(m_mdlRain);
	m_mdlRain.VertexShader = m_vsRain; m_mdlRain.GeometryShader = m_gsRain; g3d.SOSetTarget(null);
	ii *=5;if(ii>PARTICLE_COUNT)	ii=PARTICLE_COUNT;//¿ DrawAuto
	m_mdlRain.Vertices = m_bufRain2; m_mdlRain.VertexCount = ii/*PARTICLE_COUNT*/; (m_bufRain1, m_bufRain2) = (m_bufRain2, m_bufRain1);
	// ** Generate shadows
	if (m_camCam.Moved)
	{	SunLight.CalculateShadowMatrix(m_mdlTeapot.Location);
		m_bulBulb.CalculateShadowMatrix(m_mdlTeapot.Location);
		m_flaFlash.CalculateShadowMatrix(m_mdlTeapot.Location);
	}
	g3d.SetLight(SunLight, 0); g3d.SetLight(m_bulBulb, 1); g3d.SetLight(m_flaFlash, 2);
	g3d.GenerateShadows((c3DGraphics g3d, cLight light) => m_RenderModels(g3d, true, false, true));
	// ** Initialize
	g3d.SetBlendState(g3d.TransparentBlend);
	g3d.AmbientColorFactor = eColor.White;
	g3d.SetFog(true, 600, 1000, eColor.LightGray);
	// ** Render reflections for water
	if (m_camCam.Moved)
	{	Vector vIncid, vN;

		vIncid = m_mdlWater.Location - m_camCam.Location; vN = Vector.YAxis; vIncid = (vIncid - vN * 2 * vIncid.Dot(vN)).GetNormalized();
		m_camWater.Location = m_mdlWater.Location; m_camWater.Frame(m_mdlWater, vIncid, m_camCam.YAxis);
	}
	g3d.SetTarget(m_mdlWater.Material.Texture!);
		g3d.SetCamera(m_camWater);
		m_RenderModels(g3d, false, false, true);
	// ** Render reflections for teapot (cubic environment mapping)
	m_camTeapot.Location = m_mdlTeapot.Location;
	for (int i = 0; i < 6; i++)
	{	g3d.SetTarget(m_mdlTeapot.Material.CubeTexture!, i);
		m_camTeapot.LookAt(m_camTeapot.Location + sa_vCubeFront[i], sa_vCubeUp[i], sa_vCubeFront[i]);
			g3d.SetCamera(m_camTeapot);
		m_RenderModels(g3d, false, true, false);
	}
	// ** Render scene
	g3d.SetTarget(BackBuffer);												// Restore target
	g3d.SetCamera(m_camCam);												// Restore matrix
	m_RenderModels(g3d, false, true, true);
	m_camCam.Moved = false;
}
protected override void OnKeyDown(ref KeyArgs e)
{	c3DModel mdlshadow = m_mdlTeapot.ShadowModel!;

	switch (e.Key)
	{	case eKey.Up:		if (e.Control)	m_camCam.Pitch -= 5;	else	{	m_camCam.MoveZ(2); m_UpdateCamY();}
			break;
		case eKey.Down:		if (e.Control)	m_camCam.Pitch += 5;	else	{	m_camCam.MoveZ(-2); m_UpdateCamY();}
			break;
		case eKey.Left:		if (e.Control)	{	m_camCam.MoveX(-10); m_UpdateCamY();}	else	m_camCam.Yaw -= 10;
			break;
		case eKey.Right:	if (e.Control)	{	m_camCam.MoveX(10); m_UpdateCamY();}	else	m_camCam.Yaw += 10;
			break;
		case eKey.OemPlus:	m_camCam.Y += 5;
			break;
		case eKey.OemMinus:	m_camCam.Y -= 5;
			break;
		case eKey.D1:		SunLight.Enabled = !SunLight.Enabled;
			break;
		case eKey.D2:		m_bulBulb.Enabled = !m_bulBulb.Enabled;
			break;
		case eKey.D3:		m_flaFlash.Enabled = !m_flaFlash.Enabled;
			break;
		case eKey.Z:		if (e.Control)	SunLight.X -= 10;		else	SunLight.Z -= 10;
			break;
		case eKey.X:		if (e.Control)	SunLight.X += 10;		else	SunLight.Z += 10;
			break;
		case eKey.C:		if (e.Control)	m_bulBulb.X -= 10;		else	m_bulBulb.Z -= 10;
			break;
		case eKey.V:		if (e.Control)	m_bulBulb.X += 10;		else	m_bulBulb.Z += 10;
			break;
		case eKey.B:		if (e.Control)	m_flaFlash.X -= 10;		else	m_flaFlash.Z -= 10;
			break;
		case eKey.N:		if (e.Control)	m_flaFlash.X += 10;		else	m_flaFlash.Z += 10;
			break;
		case eKey.A:		SunLight.Y -= 10;
			break;
		case eKey.S:		SunLight.Y += 10;
			break;
		case eKey.D:		m_bulBulb.Y -= 10;
			break;
		case eKey.F:		m_bulBulb.Y += 10;
			break;
		case eKey.G:		m_flaFlash.Y -= 10;
			break;
		case eKey.H:		m_flaFlash.Y += 10;
			break;
		case eKey.J:		if (e.Control)	{	m_mdlTeapot.Pitch -= 10; mdlshadow.Pitch -= 10;}	else	{	m_mdlTeapot.Yaw += 10; mdlshadow.Yaw += 10;}
			m_gs3Music.SetLocation(m_mdlTeapot);
			break;
		case eKey.K:		if (e.Control)	{	m_mdlTeapot.Pitch += 10; mdlshadow.Pitch += 10;}	else	{	m_mdlTeapot.Yaw -= 10; mdlshadow.Yaw -= 10;}
			m_gs3Music.SetLocation(m_mdlTeapot);
			break;
		case eKey.Q:		cGameMasterSound.Music.Enabled = !cGameMasterSound.Music.Enabled;
			break;
		case eKey.W:		m_gs3Sound.Play();
			break;
	}
	Invalidate(); m_gs3Music.CalculateAudio();//¿
}
private void OnCamMoved()
{	// ** Update listener
	cGame3DSound.SetListenerLocation(m_camCam);
	m_gs3Music.Dirty = true; m_gs3Sound.Dirty = true;
}
private void m_RenderModels(c3DGraphics g3d, bool bRenderingShadows, bool bRenderWater, bool bRenderDoughnut)
{	// ** Reset
	g3d.ClearTarget(eColor.CornflowerBlue);									// Clear background
	g3d.ClearDepth();														// Clear depth
	// ** Render sky
	if (!bRenderingShadows)
	{	g3d.SetDepthStencilState(m_dssSkyDepthLessEqual);					// Prevent depth test fail when z = 1
			g3d.Render(m_mdlSky);
		g3d.SetDepthStencilState(null);
	}
	// ** Render terrain
	g3d.SetSampler(g3d.SamplerMinMagMipPointWrap, TERRAIN_SAMP_WRAP_SLOT);
	g3d.DSSetTexture(m_texTerrainHeight, TERRAIN_TEX_HEIGHT_SLOT);
	g3d.SetTexture(m_texTerrain, TERRAIN_TEX_LAYERS_SLOT); g3d.SetTexture(m_texTerrainBlend, TERRAIN_TEX_BLEND_SLOT);
		g3d.SetTexture(m_texTerrainHeight, TERRAIN_TEX_HEIGHT_SLOT);
	g3d.Render(m_mdlTerrain);
	// ** Render trees
	g3d.Render(m_mdlTrees1);
	g3d.Render(m_mdlTrees2);
	// ** Render water
	if (bRenderWater)
	{	m_bufReflect.Write(m_camWater.TransposedViewProjectionMatrix);
		g3d.Render(m_mdlWater);
	}
	// ** Render fire
	if (!bRenderingShadows)	g3d.Render(m_mdlFire);
	// ** Render wall
	g3d.Render(m_mdlWall);
	// ** Render model
	g3d.Render(m_mdlModel);
	// ** Render teapot
	if (bRenderDoughnut)	g3d.Render(m_mdlTeapot);
	// ** Render rain
	if (!bRenderingShadows)	g3d.Render(m_mdlRain);
}
private float m_fGetTerrainHeightAt(float x, float z)
{	int iCol, iRow; float A, B, C, D;

	x -= TERRAIN_QUAD_ORIG_X; z = -z + TERRAIN_QUAD_ORIG_Z;
		x.Clamp(0, TERRAIN_WIDTH - 1 - 0.1f); z.Clamp(0, TERRAIN_DEPTH - 1 - 0.1f); // Don't take the borders
	iCol = mMath.Floor(x); iRow = mMath.Floor(z);
		A = ma_fTerrainHeight[iRow * TERRAIN_WIDTH + iCol];					// A____B
		B = ma_fTerrainHeight[iRow * TERRAIN_WIDTH + iCol + 1];				//  | /|
		C = ma_fTerrainHeight[(iRow + 1) * TERRAIN_WIDTH + iCol];			//  |/ |
		D = ma_fTerrainHeight[(iRow + 1) * TERRAIN_WIDTH + iCol + 1];		// C----D
	x -= iCol; z -= iRow;
	if (x + z <= 1.0f)	return A + x * (B - A) + z * (C - A);				// Triangle ABC
	return D + (1 - x) * (C - D) + (1 - z) * (B - D);						// Triangle BDC
}
private Vector m_vGetTerrainVectorAt(float x, float fOffsetY, float z)	{	return new Vector(x, m_fGetTerrainHeightAt(x, z) + fOffsetY, z);}
void m_UpdateCamY()											{	m_camCam.Y = m_fGetTerrainHeightAt(m_camCam.X, m_camCam.Z) + 5;}
}
class cMan : c3DModel
{	readonly c3DModel m_mdlTorso, m_mdlHead;
		readonly c3DModel m_mdlLeftArm, m_mdlLeftForearm, m_mdlLeftHand, m_mdlRightArm, m_mdlRightForearm, m_mdlRightHand;
	readonly c3DModel m_mdlPelvis;
		readonly c3DModel m_mdlLeftLeg, m_mdlLeftCalf, m_mdlLeftFoot, m_mdlRightLeg, m_mdlRightCalf, m_mdlRightFoot;
	float m_fVeloc;
// ** Ctor/dtor
	public cMan(cWindow device) : base(device, typeof(mRes), "Res.Man.mdl")
	{	m_mdlTorso = Parts["Torso"];
			m_mdlHead = m_mdlTorso.Parts["Head"];
			m_mdlLeftArm = m_mdlTorso.Parts["Left arm"];
				m_mdlLeftForearm = m_mdlLeftArm.Parts[0];
					m_mdlLeftHand = m_mdlLeftForearm.Parts[0];
			m_mdlRightArm = m_mdlTorso.Parts["Right arm"];
				m_mdlRightForearm = m_mdlRightArm.Parts[0];
					m_mdlRightHand = m_mdlRightForearm.Parts[0];
		m_mdlPelvis = Parts["Pelvis"];
			m_mdlLeftLeg = m_mdlPelvis.Parts["Left leg"];
				m_mdlLeftCalf = m_mdlLeftLeg.Parts[0];
					m_mdlLeftFoot = m_mdlLeftCalf.Parts[0];
			m_mdlRightLeg = m_mdlPelvis.Parts["Right leg"];
				m_mdlRightCalf = m_mdlRightLeg.Parts[0];
					m_mdlRightFoot = m_mdlRightCalf.Parts[0];
		m_fVeloc = 60;
		Yaw = 90;
	}
// ** Mets
	public void SitOnFloor()
	{	m_mdlLeftArm.Pitch = 10; m_mdlLeftForearm.Pitch = -60;
		m_mdlRightArm.Pitch = 10; m_mdlRightForearm.Pitch = -60;
		m_mdlPelvis.Pitch = -20;
			m_mdlLeftLeg.Pitch = -95; m_mdlLeftCalf.Pitch = 55; m_mdlLeftFoot.Pitch = 40;
			m_mdlRightLeg.Pitch = -95; m_mdlRightCalf.Pitch = 55; m_mdlRightFoot.Pitch = 40;
		Y -= 8;
	}
	public override void OnKeyDown(ref KeyArgs e)
	{	switch (e.Key)
		{	case eKey.Up:		Y += 0.9f;
				break;
			case eKey.Down:		Y -= 0.9f;
				break;
			case eKey.Left:		Yaw -= 2f;
				break;
			case eKey.Right:	Yaw += 2f;
				break;
			case eKey.OemPlus:	MoveZ(1);
				break;
			case eKey.OemMinus:	MoveZ(-1);
				break;
			case eKey.A:		m_fVeloc -= 5;
				break;
			case eKey.S:		m_fVeloc += 5;
				break;
		}
	}
	protected override IEnumerator OnAct()
	{	float fVelocCalf, fPitch;

		while (true)
		{	fVelocCalf = float.NaN; fPitch = m_fVeloc * r_ElapsedSeconds;
			while (m_mdlLeftArm.Pitch > -30)
			{	m_mdlLeftArm.Pitch -= fPitch; m_mdlLeftForearm.Pitch -= fPitch / 2;
				m_mdlRightArm.Pitch += fPitch;	if (m_mdlRightForearm.Pitch < 0)	m_mdlRightForearm.Pitch += fPitch / 2;
				if (m_mdlRightLeg.Pitch > 18)
				{	m_mdlRightCalf.Pitch += fPitch;
				} else
				{	if (float.IsNaN(fVelocCalf))	fVelocCalf = m_mdlRightCalf.Pitch / (30 + m_mdlRightLeg.Pitch);
					m_mdlRightCalf.Pitch -= fPitch * fVelocCalf;
				}
				m_mdlRightLeg.Pitch -= fPitch;
				m_mdlLeftLeg.Pitch += fPitch;	if (m_mdlLeftCalf.Pitch > 20)	m_mdlLeftCalf.Pitch += fPitch;
				yield return null;
			}
			m_mdlRightCalf.Pitch = 0; fVelocCalf = float.NaN;
			while (m_mdlLeftArm.Pitch < 30)
			{	m_mdlLeftArm.Pitch += fPitch;	if (m_mdlLeftForearm.Pitch < 0)	m_mdlLeftForearm.Pitch += fPitch / 2;
				m_mdlRightArm.Pitch -= fPitch; m_mdlRightForearm.Pitch -= fPitch / 2;
				if (m_mdlLeftLeg.Pitch > 18)
				{	m_mdlLeftCalf.Pitch += fPitch;
				} else
				{	if (float.IsNaN(fVelocCalf))	fVelocCalf = m_mdlLeftCalf.Pitch / (30 + m_mdlLeftLeg.Pitch);
					m_mdlLeftCalf.Pitch -= fPitch * fVelocCalf;
				}
				m_mdlLeftLeg.Pitch -= fPitch;
				m_mdlRightLeg.Pitch += fPitch;	if (m_mdlRightLeg.Pitch > 20)	m_mdlRightCalf.Pitch += fPitch;
				yield return null;
			}
			m_mdlLeftCalf.Pitch = 0;
		}
	}
}
class cPlane : c3DModel
{	readonly c3DModel m_mdlElevator, m_mdlRudder;
	float m_fElevation, m_fRudderYaw;
	public readonly c3DModel Propeller;
// ** Ctor/dtor
	public cPlane(cWindow device) : base(device, typeof(mRes), "Res.Airplane.mdl")
	{	Propeller = Parts[5]; m_mdlElevator = Parts[3]; m_mdlRudder = Parts[6];
	}
// ** Props
	public float Elevation
	{	get													=>	m_fElevation;
		set
		{	m_fElevation = mMath.Round(value.GetClamped(-1, 1), 2); m_mdlElevator.Pitch = m_fElevation * 15;
		}
	}
	public float RudderYaw
	{	get													=>	m_fRudderYaw;
		set
		{	m_fRudderYaw = mMath.Round(value.GetClamped(-1, 1), 2); m_mdlRudder.Yaw = -m_fRudderYaw * 15;
		}
	}
// ** Mets
	public override void OnKeyDown(ref KeyArgs e)
	{	switch (e.Key)
		{	case eKey.Up:		Elevation += 0.1f; e.Handled = true;
				break;
			case eKey.Down:		Elevation -= 0.1f; e.Handled = true;
				break;
			case eKey.Left:		RudderYaw -= 0.1f; e.Handled = true;
				break;
			case eKey.Right:	RudderYaw += 0.1f; e.Handled = true;
				break;
		}
	}
	public override bool Act(float ElapsedSeconds)
	{	Propeller.Roll -= ElapsedSeconds * 360 * 5;							// Repetitive actions
		return base.Act(ElapsedSeconds);									// Main actions
	}
	protected override IEnumerator OnAct()
	{	while (true)
		{	Pitch -= m_fElevation * r_ElapsedSeconds * 10;
			Yaw += m_fRudderYaw * r_ElapsedSeconds * 100;
			yield return null;
		}
	}
}