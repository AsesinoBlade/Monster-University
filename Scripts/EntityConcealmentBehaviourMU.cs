// Project:     Monster University for Daggerfall Unity
// Author:      DunnyOfPenwick
// Origin Date: January 2024

using UnityEngine;
using UnityEngine.Rendering;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Entity;


namespace MonsterUniversity
{
    public class EntityConcealmentBehaviourMU : EntityConcealmentBehaviour
    {
        ShadowCastingMode originalShadowCastingMode;
        Shader originalShader;
        Color originalColor;
        Shader ghostShader;
        MagicalConcealmentFlags concealmentFlags;
        Light chameleonAura;


        void Start()
        {
            if (meshRenderer)
            {
                originalShadowCastingMode = meshRenderer.shadowCastingMode;
                originalShader = meshRenderer.material.shader;
                originalColor = meshRenderer.material.color;
            }

            ghostShader = Shader.Find(MaterialReader._DaggerfallGhostShaderName);

            concealmentFlags = MagicalConcealmentFlags.None;
        }


        /// <summary>
        /// Called on Update() to activate/deactivate visual concealment effects on this entity.
        /// </summary>
        protected override void MakeConcealed(bool concealed)
        {
            if (!meshRenderer)
                return;

            if (entityBehaviour.Entity.MagicalConcealmentFlags == concealmentFlags)
                return; //no change, keep the same

            concealmentFlags = entityBehaviour.Entity.MagicalConcealmentFlags;

            //Reset everything to defaults whenever concealment changes.
            meshRenderer.shadowCastingMode = originalShadowCastingMode;
            meshRenderer.material.shader = originalShader;
            meshRenderer.material.color = originalColor;
            meshRenderer.enabled = true;

            ToggleChameleonAura(false);

            if (!concealed)
                return;

            if (entityBehaviour.Entity.IsInvisible)
            {
                meshRenderer.enabled = false;
                return;
            }

            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.material.shader = ghostShader;
            meshRenderer.material.SetFloat("_Cutoff", 0.01f);
            Color color = Color.white;

            if (entityBehaviour.Entity.IsAShade)
            {
                color = new Color(0.1f, 0.1f, 0.1f, 0.4f);
            }

            if (entityBehaviour.Entity.IsBlending)
            {
                color.a = 0.02f;
                ToggleChameleonAura(true);
            }

            meshRenderer.material.color = color;
        }


        /// <summary>
        /// Create or enable/disable the chameleon aura, a small light centered on the entity when
        /// the Chameleon spell effect is active.
        /// </summary>
        void ToggleChameleonAura(bool enable)
        {
            if (chameleonAura)
            {
                chameleonAura.enabled = enable;
            }
            else if (enable)
            {
                //The chameleon spell isn't supposed to be as effective in dark areas.
                //Add a small light to make it easier to see blending enemies in the dark.
                chameleonAura = entityBehaviour.gameObject.AddComponent<Light>();
                chameleonAura.color = new Color(0.5f, 0.5f, 0.7f);
                chameleonAura.type = LightType.Point;
                chameleonAura.range = 1.5f;
                chameleonAura.hideFlags = HideFlags.HideInInspector; //This flag tells the First Person Lighting mod to ignore this light
                chameleonAura.intensity = 0.85f;
                chameleonAura.shadows = LightShadows.None;
            }
        }

    } //EntityConcealmentBehaviourMU



} //namespace
