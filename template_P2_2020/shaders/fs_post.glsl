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
	//outputColor = texture( pixels, uv ).rgb;
	// apply dummy postprocessing effect
	//float dist = P.x * P.x + P.y * P.y;
	//outputColor *= sin( dist * 50 ) * 0.25f + 0.75f;
	vec4 r = texture( pixels, vec2 (uv[0], uv[1]) );

	outputColor = vec3(r.r, r.g, r.b);
	outputColor *= 1-pow(sqrt(pow(uv[0],2)+pow(uv[1],2)),1.5);
}

// EOF