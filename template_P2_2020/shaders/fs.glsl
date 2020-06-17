#version 330

// shader input
in vec2 uv;					// interpolated texture coordinates
in vec4 normal;				// interpolated normal
in vec4 worldPos;
uniform sampler2D pixels;	// texture sampler
uniform vec3 viewPos;       // camera position

// shader output
out vec4 outputColor;

// lights
uniform int numLights;
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
	vec3 normal = normalize(normal.xyz);
	float dist = L.length();

	L = normalize( L );
	vec3 materialColor = texture( pixels, uv ).xyz;
	float attenuation = 1.0f / (dist * dist);

	vec3 Rv = reflect(-L,normal);
	vec3 viewDir = normalize(viewPos - worldPos.xyz);

	vec3 diffuse = materialColor * attenuation * l.col * max( 0.0f, dot( L, normal ) );
	vec3 specular = materialColor * attenuation * l.col * pow( max( 0.0f, dot( viewDir, Rv ) ), 128.0f);

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