#include "VS2DEffect.hlsli"

namespace VS2DVertex
{
	struct Vertex
	{	float2 Position			: MESH_POSITION;
	};
	cbuffer Properties			: register(b1)
	{	float4x4 g_m44Pitch;
		float2 g_ptSize;
		float g_fRadius;
		float g_ptCircles;
	};

	Pixel main(Vertex vertex)
	{	Pixel res; float2 ptPos; float z; float fPixDist;

		ptPos = vertex.Position;											// [0, 1]
		fPixDist = length(ptPos - float2(0.5, 0.5));
		z = sin(fPixDist * 4 * PI * g_ptCircles) * g_fRadius;				// [-radius, radius]
		// ** Convert [0, 1] -> [0, size]
		res.SceneSpaceOutput.xy = ptPos * g_ptSize;
		res.SceneSpaceOutput.z = z * g_ptSize.x;
		res.SceneSpaceOutput.w = 1;
		// ** Perform a transform
		res.SceneSpaceOutput = mul(res.SceneSpaceOutput, g_m44Pitch);
		// ** Calculate clip-space position
		res.ClipSpaceOutput = CalculateClip(res.SceneSpaceOutput, 1);
		res.ClipSpaceOutput.z = (g_fRadius + z) / 2;						// [0, radius].  Visible = [0, 1>
		// ** Calculate texel-space coordinates
		res.TexelSpaceInput0 = CalculateTexel0(vertex.Position * g_ptSize);
		return res;
	}
}

namespace PS2DTile
{
	cbuffer Properties			: register(b0)
	{	float4 g_rtRegion;
	};

	float4 main(Pixel pix)	: SV_Target
	{	float2 ptPos;

		//ptPos = pix.SceneSpaceOutput.xy * pix.TexelSpaceInput0.zw;
		//float x, y;
		//x = abs(ptPos.x);

		//y = pow(x, -1.0 / 0.5);
		//y = 1 - (y / 10 * 0.5 + 0.5);
		//if (abs(y - ptPos.y) < 0.001)	return float4(1, 0, 0, 1);

		//y = pow(x, 1.0 / 0.5);
		//y = 1 - (y / 10 * 0.5 + 0.5);
		//if (abs(y - ptPos.y) < 0.001)	return float4(0, 1, 0, 1);

		//y = pow(x, -1.0 / 3); y -= 1;
		//y = 1 - (y / 10 * 0.5 + 0.5);
		//if (abs(y - ptPos.y) < 0.001)	return float4(0, 0, 1, 1);

		//y = pow(x, 1.0 / 3);
		//y = 1 - (y / 10 * 0.5 + 0.5);
		//if (abs(y - ptPos.y) < 0.001)	return float4(1, 1, 0, 1);

		//return float4(1, 1, 1, 1);

		ptPos = g_rtRegion.xy + pix.SceneSpaceOutput.xy % g_rtRegion.zw;
		return texTexture0.Sample(smpSampler0, PixelToTexel0(ptPos, pix));
	}
}