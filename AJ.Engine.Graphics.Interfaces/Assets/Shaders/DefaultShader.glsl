$VERTEX$
#version 450 core
uniform mat4 projectionMatrix;
uniform mat4 transformMatrix;

layout (location = 0) in vec3 v_position;
layout (location = 1) in vec3 v_normal;
layout (location = 2) in vec2 v_uv;
layout (location = 3) in vec4 v_color;
layout (location = 4) in float v_texChannel;
layout (location = 5) in mat4 v_transform;

out vec3  f_normal;
out vec2  f_uv;
out vec4  f_color;
flat out int f_texChannel;

void main() {
	f_normal = v_normal;
	f_uv = v_uv;
	f_color = v_color;
	f_texChannel = int(floor(v_texChannel));
	f_texChannel = clamp(f_texChannel, 0, 16);
	gl_Position = projectionMatrix * transformMatrix * v_transform * vec4(v_position.xyz, 1);
}

$FRAGMENT$
#version 450 core

uniform sampler2D textureSampler[16];

in vec3  f_normal;
in vec2  f_uv;
in vec4  f_color;
flat in int f_texChannel;

out vec4 color;

void main(){
	vec4 outputColor = f_color;

	if(f_texChannel < 16){
		switch(f_texChannel)
		{
			case 0: outputColor *= texture(textureSampler[0], f_uv); break;
			case 1: outputColor *= texture(textureSampler[1], f_uv); break;
			case 2: outputColor *= texture(textureSampler[2], f_uv); break;
			case 3: outputColor *= texture(textureSampler[3], f_uv); break;
			case 4: outputColor *= texture(textureSampler[4], f_uv); break;
			case 5: outputColor *= texture(textureSampler[5], f_uv); break;
			case 6: outputColor *= texture(textureSampler[6], f_uv); break;
			case 7: outputColor *= texture(textureSampler[7], f_uv); break;
			case 8: outputColor *= texture(textureSampler[8], f_uv); break;
			case 9: outputColor *= texture(textureSampler[9], f_uv); break;
			case 10: outputColor *= texture(textureSampler[10], f_uv); break;
			case 11: outputColor *= texture(textureSampler[11], f_uv); break;
			case 12: outputColor *= texture(textureSampler[12], f_uv); break;
			case 13: outputColor *= texture(textureSampler[13], f_uv); break;
			case 14: outputColor *= texture(textureSampler[14], f_uv); break;
			case 15: outputColor *= texture(textureSampler[15], f_uv); break;
		}

		if(outputColor.w == 0.0){
			discard;
		}
	}

	color = outputColor;
}
