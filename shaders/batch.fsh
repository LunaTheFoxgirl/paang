sampler2D tex0;

struct vtxout {
	float4 pos : POSITION;
	float4 color : COLOR0;
	float2 uv : TEXCOORD0;
};

struct frgout {
	float4 color : COLOR0;
};

void main(in vtxout IN, out frgout OUT) {
	OUT.color = mul(tex2D(tex0, IN.uv), IN.color);
}