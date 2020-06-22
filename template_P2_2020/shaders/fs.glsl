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
	vec3 dir;
	float cutOff;
	float outerCutOff;
};

uniform Light lights[20];

vec4 calcLight (Light l)
{
	vec4 pixelColor;
	vec3 materialColor = texture( pixels, uv ).xyz;
	vec3 normal = normalize(normal.xyz);
	
	if(l.dir == vec3(0,0,0))
	{
		vec3 L  = l.pos.xyz - worldPos.xyz;
		float dist = L.length();
		float attenuation = 1.0f / (dist * dist);

		L = normalize( L );
		vec3 Rv = reflect(-L,normal);
		vec3 viewDir = normalize(viewPos - worldPos.xyz);
		
		vec3 standard = materialColor * attenuation * l.col;
		vec3 diffuse = standard * max( 0.0f, dot( L, normal ) );
		vec3 specular = standard * pow( max( 0.0f, dot( viewDir, Rv ) ), 128.0f);
		pixelColor = vec4( diffuse + specular, 1);
	}
	else
	{
		vec3 D  = l.pos.xyz - worldPos.xyz;
		float dist = D.length();

		vec3 L  = normalize(-l.dir);
		float theta = dot(normalize(D), L);
		float epsilon = l.cutOff - l.outerCutOff;
		float intensity = clamp((theta - l.outerCutOff) / epsilon, 0.0f, 1.0f);

		if (theta > l.outerCutOff)
		{
			float attenuation = 1.0f / (dist * dist);

			vec3 Rv = reflect(-L,normal);
			vec3 viewDir = normalize(viewPos - worldPos.xyz);

			vec3 standard = materialColor * attenuation * l.col * intensity;
			vec3 diffuse = standard * max( 0.0f, dot( L, normal ) );
			vec3 specular = standard * pow( max( 0.0f, dot( viewDir, Rv ) ), 128.0f);
			pixelColor = vec4( diffuse + specular, 1);
		}
		else
		{
			pixelColor = vec4(0,0,0,0);
		}
	}
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