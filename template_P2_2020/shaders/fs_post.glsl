#version 330

// shader input
in vec2 P;						// fragment position in screen space
in vec2 uv;						// interpolated texture coordinates
uniform sampler2D pixels;		// input texture (1st pass render target)

// shader output
out vec3 outputColor;

void main()
{
	// retrieve input pixel	
	vec4 r = texture( pixels, vec2 (uv[0] - 0.000f, uv[1] - 0.000f) );
	vec4 g = texture( pixels, vec2 (uv[0] - 0.001f, uv[1] - 0.001f) );
	vec4 b = texture( pixels, vec2 (uv[0] - 0.002f, uv[1] - 0.002f) );

	outputColor = vec3(r.r, g.g, b.b);
	outputColor *= 1-pow(sqrt(pow(P[0],2)+pow(P[1],2)),1.5);
}

// EOF