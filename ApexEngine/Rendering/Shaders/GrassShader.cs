﻿using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApexEngine.Rendering.Shaders
{
    public class GrassShader : Shader
    {
        private static Assets.ShaderTextLoader textLoader = Assets.ShaderTextLoader.GetInstance();
        float globalTime = 0f;

        public GrassShader(ShaderProperties properties)
            : base(properties, (string)textLoader.Load(AppDomain.CurrentDomain.BaseDirectory + "\\shaders\\grass.vert"),
                               (string)textLoader.Load(AppDomain.CurrentDomain.BaseDirectory + "\\shaders\\default.frag"))
        {
        }

        public override void ApplyMaterial(Material material)
        {
            base.ApplyMaterial(material);

            this.SetUniform(MATERIAL_AMBIENTCOLOR, material.GetVector4f(Material.COLOR_AMBIENT));
            this.SetUniform(MATERIAL_DIFFUSECOLOR, material.GetVector4f(Material.COLOR_DIFFUSE));
            this.SetUniform(MATERIAL_SPECULARCOLOR, material.GetVector4f(Material.COLOR_SPECULAR));
            this.SetUniform(MATERIAL_SHININESS, material.GetFloat(Material.SHININESS));
            this.SetUniform(MATERIAL_ROUGHNESS, material.GetFloat(Material.ROUGHNESS));
            this.SetUniform(MATERIAL_METALNESS, material.GetFloat(Material.METALNESS));
            this.SetUniform(MATERIAL_SPECULARTECHNIQUE, material.GetInt(Material.TECHNIQUE_SPECULAR));
            this.SetUniform(MATERIAL_SPECULAREXPONENT, material.GetFloat(Material.SPECULAR_EXPONENT));
            this.SetUniform(MATERIAL_PERPIXELLIGHTING, material.GetInt(Material.TECHNIQUE_PER_PIXEL_LIGHTING));

            int blendMode = material.GetInt(Material.MATERIAL_BLENDMODE);
            if (blendMode == 1)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }
        }

        public override void End()
        {
            base.End();

            GL.Disable(EnableCap.Blend);
        }

        public override void Update(Environment environment, Camera cam, Mesh mesh)
        {
            base.Update(environment, cam, mesh);

            environment.DirectionalLight.BindLight(0, this);
            environment.AmbientLight.BindLight(0, this);
            for (int i = 0; i < environment.PointLights.Count; i++)
            {
                environment.PointLights[i].BindLight(i, this);
            }

            SetUniform(ENV_NUMPOINTLIGHTS, environment.PointLights.Count);
            SetUniform(ENV_FOGSTART, environment.FogStart);
            SetUniform(ENV_FOGEND, environment.FogEnd);
            SetUniform(ENV_FOGCOLOR, environment.FogColor);
            SetUniform(APEX_ELAPSEDTIME, environment.ElapsedTime);

            if (currentMaterial != null)
            {
                Texture diffuseTex = currentMaterial.GetTexture(Material.TEXTURE_DIFFUSE);
                if (diffuseTex != null)
                {
                    Texture.ActiveTextureSlot(0);
                    diffuseTex.Use();
                    SetUniform("Material_DiffuseMap", 0);
                    SetUniform("Material_HasDiffuseMap", 1);
                }
                else
                {
                    SetUniform("Material_HasDiffuseMap", 0);
                }

                Texture normalTex = currentMaterial.GetTexture(Material.TEXTURE_NORMAL);
                if (normalTex != null)
                {
                    Texture.ActiveTextureSlot(1);
                    normalTex.Use();
                    SetUniform("Material_NormalMap", 1);
                    SetUniform("Material_HasNormalMap", 1);
                }
                else
                {
                    SetUniform("Material_HasNormalMap", 0);
                }

                Texture heightTex = currentMaterial.GetTexture(Material.TEXTURE_HEIGHT);
                if (heightTex != null)
                {
                    Texture.ActiveTextureSlot(2);
                    heightTex.Use();
                    SetUniform("Material_HeightMap", 2);
                    SetUniform("Material_HasHeightMap", 1);
                }
                else
                {
                    SetUniform("Material_HasHeightMap", 0);
                }

                Texture envTex = currentMaterial.GetTexture(Material.TEXTURE_ENV);
                if (envTex != null)
                {
                    Texture.ActiveTextureSlot(3);
                    envTex.Use();
                    SetUniform("Material_EnvironmentMap", 3);
                    SetUniform("Material_HasEnvironmentMap", 1);
                }
                else
                {
                    SetUniform("Material_HasEnvironmentMap", 0);
                }
            }
            if (environment.ShadowsEnabled)
            {
                SetUniform("Env_ShadowsEnabled", 1);
                for (int i = 0; i < 4; i++)
                {
                    Texture.ActiveTextureSlot(3 + i);
                    environment.ShadowMaps[i].Use();
                    SetUniform("Env_ShadowMap" + i.ToString(), 3 + i);
                    SetUniform("Env_ShadowMatrix" + i.ToString(), environment.ShadowMatrices[i]);
                    SetUniform("Env_ShadowMapSplits[" + i.ToString() + "]", environment.ShadowMapSplits[i]);
                }
            }
        }
    }
}
