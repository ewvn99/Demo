#include "VS.hlsli"

namespace Sky
{
	struct PixelSky
	{	float4 Position			: SV_POSITION;
		float3 PositionW		: POSITION;
	};

	namespace VS
	{
		PixelSky main(VertexColor vertex)
		{	PixelSky res; float4 f4WorldPos;

			f4WorldPos = mul(float4(vertex.Position.xyz, 1), g_WorldMatrix);
			res.Position = mul(f4WorldPos, g_ViewProjectionMatrix).xyww;
			res.PositionW = vertex.Position.xyz;
			return res;
		}
	}
	namespace PS
	{
		float4 main(PixelSky pixel) : SV_TARGET
		{	return g_txcCube.Sample(g_Sampler, pixel.PositionW);
		}
	}
}

namespace Terrain
{
	struct VertexTerrain
	{	float4 Position			: POSITION;
		float2 Texcoord			: TEXCOORD;
		float2 MinMax			: MINMAX;
	};
	struct PixelTerrain
	{	float4 Position			: SV_POSITION;
		float3 PositionW		: POSITION;
		float2 Texcoord			: TEXCOORD0;
		float2 TexcoordDetail	: TEXCOORD1;
		float4 ShadowPosition[LIGHT_MAX]	: SHADOW;
	};

	namespace VS
	{
		VertexTerrain main(VertexTerrain vertex)			{	return vertex;}
	}

	namespace HS
	{
		float m_fCalcTessFactor(float3 vPos)
		{	float d = distance(vPos, g_CameraPosition);

			return round(pow(2, lerp(6, 0, saturate((d - 20) / (700 - 20)))));
		}
		bool m_bFrustumContains(float4 fruFrustum[6], float3 vCenter, float3 vExtents)
		{	for (int i = 0; i < 6; i++)
			{	float r = dot(vExtents, abs(fruFrustum[i].xyz));			// This is always positive
				float s = dot(float4(vCenter, 1), fruFrustum[i]);			// Signed distance from center to plane
			
				if ((s + r) < 0)	return false;							// If the box is completely behind any of the frustum planes then it is outside the frustum
			}
			return true;
		}
		TessFactor m_facConstFn(InputPatch<VertexTerrain, 4> patch, uint PatchID : SV_PrimitiveID)
		{	TessFactor res;

			// ** Frustum cull: exit
			float3 vMin = float3(patch[2].Position.x, patch[0].MinMax.x, patch[2].Position.z);
			float3 vMax = float3(patch[1].Position.x, patch[0].MinMax.y, patch[1].Position.z);
			float3 vBoxCenter = (vMin + vMax) / 2, vBoxExtents = (vMax - vMin) / 2;
			
			if (!m_bFrustumContains(g_CameraFrustum, vBoxCenter, vBoxExtents))
			{	res.Edges[0] = 0; res.Edges[1] = 0; res.Edges[2] = 0; res.Edges[3] = 0; res.Inside[0] = 0; res.Inside[1] = 0;
				return res;
			}
			// ** Calculate factors
			float3 vE0, vE1, vE2, vE3, vC;

			vE0 = (patch[0].Position + patch[2].Position).xyz / 2;
				vE1 = (patch[0].Position + patch[1].Position).xyz / 2;
				vE2 = (patch[1].Position + patch[3].Position).xyz / 2;
				vE3 = (patch[2].Position + patch[3].Position).xyz / 2;
				vC = (patch[0].Position + patch[1].Position + patch[2].Position + patch[3].Position).xyz / 4;
			res.Edges[0] = m_fCalcTessFactor(vE0); res.Edges[1] = m_fCalcTessFactor(vE1);
				res.Edges[2] = m_fCalcTessFactor(vE2); res.Edges[3] = m_fCalcTessFactor(vE3);
				res.Inside[0] = m_fCalcTessFactor(vC); res.Inside[1] = res.Inside[0];
			return res;
		}
		[domain("quad")] [partitioning("fractional_even")] [outputtopology("triangle_cw")] [outputcontrolpoints(4)]
		[patchconstantfunc("Terrain::HS::m_facConstFn")] [maxtessfactor(64.0f)]
		VertexTerrain main(InputPatch<VertexTerrain, 4> patch, uint i : SV_OutputControlPointID, uint PatchID : SV_PrimitiveID)
		{	return patch[i];
		}
	}

	namespace DS
	{
		Texture2D texHeightMap	: register(t7);

		[domain("quad")]
		PixelTerrain main(TessFactor input, float2 uv : SV_DomainLocation, const OutputPatch<VertexTerrain, 4> quad)
		{	PixelTerrain res; VertexTerrain v;

			v.Position = lerp(lerp(quad[0].Position, quad[1].Position, uv.x), lerp(quad[2].Position, quad[3].Position, uv.x), uv.y); 
				v.Texcoord = lerp(lerp(quad[0].Texcoord, quad[1].Texcoord, uv.x), lerp(quad[2].Texcoord, quad[3].Texcoord, uv.x), uv.y); 
				v.Position.y = texHeightMap.SampleLevel(g_Sampler, v.Texcoord, 0).r; v.Position.w = 1;
			res.PositionW = mul(v.Position, g_WorldMatrix).xyz;
			res.Position = mul(float4(res.PositionW, 1), g_ViewProjectionMatrix);
			res.Texcoord = v.Texcoord;
			res.TexcoordDetail = v.Texcoord * 50;
			CalculatePixelShadow(res.PositionW, res.ShadowPosition);
			return res;
		}
	}

	namespace PS
	{
		static const float TEXEL_SIZE	= 1;
	
		SamplerState g_smpWrap		: register(s3);
		Texture2DArray a_texLayers	: register(t5);
		Texture2D texBlend			: register(t6);
		Texture2D texHeightMap		: register(t7);

		float4 main(PixelTerrain pixel) : SV_TARGET
		{	float4 clr, clr1, clr2, clr3, clr4, clrBlend; float fLeftY, fRightY, fTopY, fBottomY;
			float3 vTang, vBinorm, vNormal, vVertToCam;

			// ** Calc normal
			fLeftY = texHeightMap.Sample(g_Sampler, pixel.Texcoord, int2(-1, 0)).r;
			fRightY = texHeightMap.Sample(g_Sampler, pixel.Texcoord, int2(1, 0)).r;
			fTopY = texHeightMap.Sample(g_Sampler, pixel.Texcoord, int2(0, 1)).r;
			fBottomY = texHeightMap.Sample(g_Sampler, pixel.Texcoord, int2(0, -1)).r;
			vTang = normalize(float3(-2 * TEXEL_SIZE, fLeftY - fRightY, 0));
			vBinorm = normalize(float3(0, fTopY - fBottomY, 2 * TEXEL_SIZE)); 
			vNormal = cross(vTang, vBinorm);
			// ** Get color
			clr = a_texLayers.Sample(g_smpWrap, float3(pixel.TexcoordDetail, 0));
				clr1 = a_texLayers.Sample(g_smpWrap, float3(pixel.TexcoordDetail, 1));
				clr2 = a_texLayers.Sample(g_smpWrap, float3(pixel.TexcoordDetail, 2));
				clr3 = a_texLayers.Sample(g_smpWrap, float3(pixel.TexcoordDetail, 3));
				clr4 = a_texLayers.Sample(g_smpWrap, float3(pixel.TexcoordDetail, 4));
			clrBlend = texBlend.Sample(g_smpWrap, pixel.Texcoord);
				clr = lerp(clr, clr1, clrBlend.r); clr = lerp(clr, clr2, clrBlend.g);
				clr = lerp(clr, clr3, clrBlend.b); clr = lerp(clr, clr4, clrBlend.a);
			// ** Apply light
			vVertToCam = g_CameraPosition - pixel.PositionW;
			clr = CalculateLight(clr, pixel.PositionW, vNormal, vVertToCam, pixel.ShadowPosition);
			// ** Apply fog
			clr = CalculateFog(clr, vVertToCam);
			return saturate(clr);
		}
	}
}

namespace VSInstance
{
	struct VertexCustom
	{	VertexInstance VertexInst;
	};

	Pixel main(VertexCustom vertex)
	{	Pixel res;
	
		res = CalculateInstancePixel(vertex.VertexInst.World, vertex.VertexInst.Vertex);
		return res;
	}
}

namespace Reflect
{
	struct PixelReflect
	{	Pixel Pixel;
		float4 ReflectPos	: REFLECT;
	};

	namespace VS
	{
		cbuffer BufReflect : register(b3)
		{	matrix g_ReflectMatrix;											// View and projection
		};

		PixelReflect main(Vertex vertex)
		{	PixelReflect res;
	
			res.Pixel = CalculatePixel(vertex);
			res.ReflectPos = mul(float4(res.Pixel.PositionW, 1), g_ReflectMatrix);
			return res;
		}
	}
	namespace PS
	{
		float4 main(PixelReflect pixel) : SV_TARGET
		{	float4 clr; float2 ptReflect; float3 vVertToCam;

			pixel.ReflectPos.y = -pixel.ReflectPos.y; ptReflect = pixel.ReflectPos.xy / pixel.ReflectPos.w / 2 + 0.5;
			// ** Get color: apply reflex
			clr = lerp(g_Color, g_texTexture.Sample(g_Sampler, ptReflect), 0.5) * g_Alpha;
			ClipAlpha(clr);
			// ** Apply light
			vVertToCam = g_CameraPosition - pixel.Pixel.PositionW;
			pixel.Pixel.Normal = normalize(pixel.Pixel.Normal);					// Vertex interpolation alters values
			clr = CalculateLight(clr, pixel.Pixel.PositionW, pixel.Pixel.Normal, vVertToCam, pixel.Pixel.ShadowPosition);
			// ** Apply fog
			clr = CalculateFog(clr, vVertToCam);
			return clr;
		}
	}
}

namespace PSFire
{
	cbuffer buffer : register(b2)
	{	int g_iTextIdx;
	};
	Texture2DArray a_texTexture	: register(t1);

	float4 main(Pixel pixel) : SV_TARGET
	{	float4 clr;

		clr = a_texTexture.Sample(g_Sampler, float3(pixel.Texcoord, g_iTextIdx));
		ClipAlpha(clr);
		return clr;
	}
}

namespace Rain
{
	#define RAIN_WIDTH				100
	#define RAIN_TOP				300
	#define DROP_SPEED				500
	#define DROP_HEIGHT				20

	namespace GSGenerate													// Generate particles
	{
		#define MAX_PARTICLES		5
		#define GENERATION_TIME		0.02
		#define PARTICLE_DURATION	2
		void CreateVertex(VertexParticle input, inout PointStream<VertexParticle> output)
		{	for (int i = 0; i < MAX_PARTICLES; i++)
			{	VertexParticle p;

				p.Position = float4(RandomVector((float)i / MAX_PARTICLES) * RAIN_WIDTH, 1); p.Position.y = RAIN_TOP;
					p.Normal = normalize(float3(0.2, -1, 0));
					p.Type = PARTICLE_PART; p.Age = 0; p.Scale = 0;
				output.Append(p);
			}
		}
		#include "Particle.hlsli"
	}

	struct RainVertex
	{	float4 Position		: POSITION;
		float3 Normal		: NORMAL;
		int Type			: TYPE;
	};
	struct RainPixel
	{	float4 Position		: SV_POSITION;
		float2 Texcoord		: TEXCOORD;
	};

	namespace VS
	{
		RainVertex main(VertexParticle vertex)
		{	RainVertex ver;
		
			ver.Position = vertex.Position + float4(vertex.Normal * vertex.Age * DROP_SPEED, 0); // Move downward
			ver.Normal = vertex.Normal; ver.Type = vertex.Type;
			return ver;
		}
	}
	namespace GS
	{
		[maxvertexcount(2)]
		void main(point RainVertex input[1], inout LineStream<RainPixel> output) // Convert point into line
		{	RainPixel pix;
		
				if (input[0].Type != PARTICLE_PART)	return;
			input[0].Position = mul(input[0].Position, g_WorldMatrix);
			pix.Position = mul(input[0].Position, g_ViewProjectionMatrix);
				pix.Texcoord = 0;
				output.Append(pix);
			pix.Position = mul(input[0].Position + float4(input[0].Normal * DROP_HEIGHT, 0), g_ViewProjectionMatrix);
				pix.Texcoord = 1;
				output.Append(pix);
		}
	}
	namespace PS
	{
		float4 main(RainPixel pixel) : SV_TARGET			{	return g_texTexture.Sample(g_Sampler, pixel.Texcoord);}
	}
}

namespace CSBitmap
{
	#define NUM_THREADS			32
	#define PI					3.14159265358979323846

	SamplerState smpSampler		: register(s0);
	//SamplerState smpSampler
	//	{	Filter = MIN_MAG_MIP_LINEAR;
	//		AddressU = Wrap;
	//		AddressV = Wrap;
	//	};
	Texture2D texIn				: register(t0);
	RWTexture2D<float4> texOut	: register(u0);

	[numthreads(NUM_THREADS, NUM_THREADS, 1)]
	void main(
			// dtiThread - Uniquely identifies a given execution of the shader, most commonly used parameter.
			// Definition: (groupId.x * NUM_THREADS_X + groupThreadId.x, groupId.y * NUMTHREADS_Y + groupThreadId.y, groupId.z * NUMTHREADS_Z + groupThreadId.z)
			uint3 dtiThread  : SV_DispatchThreadID,
			// groupThreadId - Identifies an individual thread within a thread group.
			// Range: (0 to NUMTHREADS_X - 1, 0 to NUMTHREADS_Y - 1, 0 to NUMTHREADS_Z - 1)
			uint3 groupThreadId : SV_GroupThreadID,
			// groupId - Identifies which thread group the individual thread is being executed in.
			// Range defined in DFTTransform::CalculateThreadgroups
			uint3 groupId : SV_GroupID,
			// One dimensional indentifier of a compute shader thread within a thread group.
			// Range: (0 to NUMTHREADS_X * NUMTHREADS_Y * NUMTHREADS_Z - 1)
			uint  groupIndex : SV_GroupIndex)
	{	uint width, height;

		texIn.GetDimensions(width, height);

		// It is likely that the compute shader will execute beyond the bounds of the input image, since the shader is executed in chunks sized by
		// the threadgroup size defined in the app. For this reason each shader should ensure the current dtiThread is within the bounds of the input image before proceeding
		if (dtiThread.x >= width || dtiThread.y >= height)	return;

		// This code represents the inner summation in the Discrete Fourier Transform (DFT) equation.
		float2 value = float2(0, 0);
		for (uint n = 0; n < width; n++)
		{
			//float4 color = texIn.SampleLevel(smpSampler, float2(n + 0.5, dtiThread.y + 0.5), 0); // Add 0.5 to hit the center of the pixel.
			float4 color = texIn[float2(n + 0.5, dtiThread.y + 0.5)]; // Add 0.5 to hit the center of the pixel.

			float lum = color.r * 0.2125 +
				color.g * 0.7154 +
				color.b * 0.0721;

			float arg = -2.0 * PI * (float)dtiThread.x * (float)n / (float)width;
			float sinArg, cosArg;
			sincos(arg, sinArg, cosArg);
			value += lum * float2(cosArg, sinArg);
		}

		//float4 color = texIn[dtiThread.xy];
		//color.r += 0.4;
		//texOut[dtiThread.xy] = color;
		//texOut[dtiThread.xy] = texIn.SampleLevel(smpSampler, dtiThread.xy, 0);

		texOut[dtiThread.xy] = float4(value.x, value.y, 0, 1);
	}
}

namespace CSCalculation
{
	#define NUM_THREADS			32
	#define RECORDS_PER_THREAD	10

	struct InRecord
	{	int Id;
		int Value1, Value2;
	};
	struct OutRecord
	{	int Id;
		int Sum;
	};

	cbuffer Args							: register(b0)
	{	int g_iRecordCount;
	};
	StructuredBuffer<InRecord> g_InBuff		: register(t0);
	RWStructuredBuffer<OutRecord> g_OutBuff	: register(u0);

	[numthreads(NUM_THREADS, 1, 1)]
	void main(uint3 dtiThread  : SV_DispatchThreadID)
	{	int iStartIdx, iEndIdx;

		iStartIdx = dtiThread.x * RECORDS_PER_THREAD;	if (iStartIdx >= g_iRecordCount)	return;
		iEndIdx = iStartIdx + RECORDS_PER_THREAD;		if (iEndIdx > g_iRecordCount)		iEndIdx = g_iRecordCount;
		while (iStartIdx < iEndIdx)
		{	g_OutBuff[iStartIdx].Id = g_InBuff[iStartIdx].Id;
			g_OutBuff[iStartIdx].Sum = g_InBuff[iStartIdx].Value1 + g_InBuff[iStartIdx].Value2;
			iStartIdx++;
		}
	}
}