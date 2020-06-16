#version 330

// shader input
in vec2 uv;					// interpolated texture coordinates
in vec4 normal;				// interpolated normal
in vec4 worldPos;
uniform sampler2D pixels;	// texture sampler

// shader output
out vec4 outputColor;

// lights
uniform vec3 lightPos;

uniform vec4 lp0;
uniform vec3 lc0;

// fragment shader
void main()
{
	vec3 L  = lp0.xyz - worldPos.xyz;
	float dist = L.length();

	L = normalize( L );
	vec3 materialColor = texture( pixels, uv ).xyz;
	float attenuation = 1.0f / (dist * dist);

	float ambient = 0.05f;
	vec3 Rv = reflect(L,normal.xyz);

	vec3 diffuse = materialColor * max( 0.0f, dot( L, normal.xyz ) ) * attenuation * lc0;
    vec3 specular = materialColor * pow( max( 0.0f, dot( L, Rv ) ), 10.0f) *attenuation * lc0;

	outputColor = vec4( ambient + diffuse + specular , 1);
}