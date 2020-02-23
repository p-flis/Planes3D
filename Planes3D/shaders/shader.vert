#version 330 core

layout(location = 0) in vec3 aPosition;

layout(location = 1) in vec2 aTexCoord;

layout(location = 2) in vec3 aNormal;

out vec2 texCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform float fogDensity;
uniform float fogGradient;
uniform vec3 color;

out vec3 Normal;
out vec3 FragPos;
out vec3 OutColor;
out float visibility;


void main(void)
{
	OutColor = color;

	texCoord = aTexCoord;
	vec4 pos_mod = vec4(aPosition, 1.0) * model;
	vec4 pos_mod_view = pos_mod * view;
    gl_Position = pos_mod_view * projection;

	FragPos = vec3(vec4(aPosition, 1.0) * model);
    Normal = aNormal * mat3(transpose(inverse(model)));
	
	float distance = length(pos_mod_view.xyz);
	visibility = exp(-pow((distance*fogDensity), fogGradient));
	visibility = clamp(visibility, 0.0, 1.0);
}