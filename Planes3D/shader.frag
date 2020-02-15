#version 330

out vec4 outputColor;

in vec2 texCoord;

uniform sampler2D texture0;

void main()
{
	FragColor = vec4(1.0); // set all 4 vector values to 1.0
    //outputColor = texture(texture0, texCoord);
}