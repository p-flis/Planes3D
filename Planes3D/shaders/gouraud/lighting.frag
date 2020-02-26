#version 330 core

uniform sampler2D texture0;

in vec3 OutColor;
in vec2 texCoord;

out vec4 FragColor;

void main()
{
	vec3 objectColor = texture(texture0, texCoord).xyz;
    vec3 result = OutColor*objectColor;
    FragColor = vec4(result, 1.0);
}