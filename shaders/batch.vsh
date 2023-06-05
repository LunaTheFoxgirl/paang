
struct vtxin {
	float4 pos : POSITION;
	float4 color : COLOR0;
	float2 uv : TEXCOORD0;
};

struct vtxout {
	float4 pos : POSITION;
	float4 color : COLOR0;
	float2 uv : TEXCOORD0;
};

void main(in vtxin IN, out vtxout OUT, uniform float4x4 mvp) {
	OUT.pos = mul(IN.pos, mvp);
	OUT.color = IN.color;
	OUT.uv = IN.uv;
}