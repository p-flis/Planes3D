#version 330 core

layout(location = 0) in vec3 aPosition;

layout(location = 1) in vec2 aTexCoord;

layout(location = 2) in vec3 aNormal;

out vec2 texCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform vec3 color;

out vec3 Normal;
out vec3 FragPos;
out vec3 OutColor;


void main(void)
{
	OutColor = color;

	texCoord = aTexCoord;
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;

	FragPos = vec3(vec4(aPosition, 1.0) * model);
    Normal = aNormal * mat3(transpose(inverse(model)));
}