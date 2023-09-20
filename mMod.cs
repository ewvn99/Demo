using Wew.Media;
using Wew;

namespace Demo;

static class mRes
{	public static readonly cBitmap BmpAni		= new (typeof(mRes), "Res.Ani.gif");
	public static readonly cBitmap BmpTool		= new (typeof(mRes), "Res.Tool.png");
	public static readonly cBitmap BmpCfg		= new (typeof(mRes), "Res.Cfg.png");
	public static readonly cBitmap BmpSmile		= new (typeof(mRes), "Res.Smile.png");
	public static readonly cBitmap BmpArrow		= new (typeof(mRes), "Res.Arrow.png");
	public static readonly cBitmap BmpCube		= new (typeof(mRes), "Res.Cube.png");
	public static readonly cBitmap BmpDisplMap	= new (typeof(mRes), "Res.DisplMap.png");
	public static readonly cBitmap BmpPhoto		= new (typeof(mRes), "Res.Photo.jpg");
	public static readonly cBitmap BmpTable		= new (typeof(mRes), "Res.Table.png");
	public static readonly cBitmap BmpKey		= new (typeof(mRes), "Res.Key.png");
	public static byte[] BinTerrain				=>	typeof(mRes).GetResource("Res.Terrain.bin");
}
static class mMod
{	public static readonly string DLG_IMG_EXTS		= mImaging.GetFileDialogFilter(false);
	public const string DLG_VID_EXTS				= "Videos|*.mp4;*.mpg;*.vob;*.wmv;*.avi|All files|*.*";
	public static readonly System.Guid DLG_IMG_GUID	= new ("{CED10D86-E21D-4f52-8496-F7DE6CE4A5AD}");
	public static readonly System.Guid DLG_VID_GUID	= new ("{BED10D86-E21D-4f52-8496-F7DE6CE4A5AD}");
}