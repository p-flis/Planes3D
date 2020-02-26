#version 330 core

layout(location = 0) in vec3 aPosition;

layout(location = 1) in vec2 aTexCoord;

layout(location = 2) in vec3 aNormal;


uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform float fogDensity;
uniform float fogGradient;
uniform vec3 color;

struct Material {
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    float shininess;
};

struct Light {
    vec3 position;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

struct SpotLight {
    vec3  position;
    vec3  direction;
    float cutOff;
    float outerCutOff;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    float constant;
    float linear;
    float quadratic;
};

uniform SpotLight spotLight;
uniform Light light;
uniform Material material;
uniform vec3 viewPos;
uniform vec3 skyColour;

out vec3 OutColor;
out vec2 texCoord;

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir);

void main(void)
{
	OutColor = color;

    ///Transformations
	texCoord = aTexCoord;
	vec4 pos_mod = vec4(aPosition, 1.0) * model;
	vec4 pos_mod_view = pos_mod * view;
    gl_Position = pos_mod_view * projection;

	vec3 FragPos = vec3(vec4(aPosition, 1.0) * model);
    vec3 Normal = aNormal * mat3(transpose(inverse(model)));
	
	float distance = length(pos_mod_view.xyz);
	float visibility = exp(-pow((distance*fogDensity), fogGradient));
	visibility = clamp(visibility, 0.0, 1.0);

	///Light calculation    
    vec3 ambient = light.ambient * material.ambient; 
    
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(light.position - FragPos);
    
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = light.diffuse * (diff * material.diffuse);

    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * (spec * material.specular);
    
    vec3 please = CalcSpotLight(spotLight, norm, FragPos, viewDir);
    
    vec3 result = (ambient + diffuse + specular + please);
    OutColor = mix(skyColour, result, visibility);
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    //diffuse shading
    vec3 lightDir = normalize(light.position - fragPos);
    float diff = max(dot(normal, lightDir), 0.0);

    //specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);

    //attenuation
    float distance    = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance +
    light.quadratic * (distance * distance));

    //spotlight intensity
    float theta     = dot(lightDir, normalize(-light.direction));
    float epsilon   = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);

    //combine results
    vec3 ambient = light.ambient * material.ambient;
    vec3 diffuse = light.diffuse * material.diffuse;
    vec3 specular = light.specular * spec * material.specular;
    
    ambient  *= attenuation;
    diffuse  *= attenuation * intensity;
    specular *= attenuation * intensity;
    return (ambient + diffuse + specular);
}