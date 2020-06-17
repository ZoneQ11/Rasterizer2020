#version 330

// shader input
in vec2 uv;					// interpolated texture coordinates
in vec4 normal;				// interpolated normal
in vec4 worldPos;
uniform sampler2D pixels;	// texture sampler

// shader output
out vec4 outputColor;

// lights
uniform int numLights;
uniform vec3 lightPos;
uniform vec3 ambient = vec3(0.05f,0.05f,0.05f);

// lights
struct Light {
	vec4 pos;
	vec3 col;
	//vec3 dir;
};

uniform Light lights[20];

vec4 calcLight (Light l)
{
	vec3 L  = l.pos.xyz - worldPos.xyz;
	float dist = L.length();

	L = normalize( L );
	vec3 materialColor = texture( pixels, uv ).xyz;
	float attenuation = 1.0f / (dist * dist);

	vec3 Rv = reflect(L,normal.xyz);

	vec3 diffuse = materialColor * max( 0.0f, dot( L, normal.xyz ) ) * attenuation * l.col;
	vec3 specular = materialColor * pow( max( 0.0f, dot( L, Rv ) ), 10.0f) *attenuation * l.col;

	vec4 pixelColor = vec4( diffuse + specular, 1);
	return pixelColor;
}

// fragment shader
void main()
{
	vec4 colorSum = vec4(ambient.xyz,1) * texture( pixels, uv );
	for( int i = 0; i < numLights; i++ )
		colorSum += calcLight(lights[i]);
	outputColor = colorSum;
}