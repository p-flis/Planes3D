﻿#version 330 core
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
uniform sampler2D texture0;
uniform vec3 skyColour;

in vec3 Normal;
in vec3 FragPos;
in float visibility;
in vec2 texCoord;

out vec4 FragColor;

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir);

void main()
{
	vec3 objectColor = texture(texture0, texCoord).xyz;
    
    vec3 ambient = light.ambient * material.ambient; 
    
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(light.position - FragPos);
    
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = light.diffuse * (diff * material.diffuse);
    
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 h = normalize(lightDir + viewDir);

    float spec = pow(max(dot(h, norm), 0.0), material.shininess);
    vec3 specular = light.specular * (spec * material.specular);
    
    vec3 please = CalcSpotLight(spotLight, norm, FragPos, viewDir);
    
    vec3 result = (ambient + diffuse + specular + please)*objectColor;
    FragColor = vec4(result, 1.0);
    FragColor = mix(vec4(skyColour, 1.0), FragColor, visibility);
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    //diffuse shading
    vec3 lightDir = normalize(light.position - fragPos);
    float diff = max(dot(normal, lightDir), 0.0);

    //specular shading
    vec3 h = normalize(lightDir + viewDir);
    float spec = pow(max(dot(h, normal), 0.0), material.shininess);

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