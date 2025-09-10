using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using uSource.Decals;
using uSource.Formats.Source.VTF;
using uSource.MathLib;
using uSource.Example;
using System;
using uSource;

using System.Security.Cryptography;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace uSource.Formats.Source.VBSP
{
    //TODO:
    //Rework this & make

    public static class EntitySetup
    {
        public static void PlaceRopes(string bspEntityData, GameObject world, GameObject ropePrefab)
        {
            // Parse entities from BSP data
            List<SourceEntity> entities = BSPParser.ParseEntities(bspEntityData);

            // Container for ropes
            GameObject staticContainer = new GameObject("Ropes");
            staticContainer.transform.SetParent(world.transform);

            int ropeIndex = 1;
            foreach (var entity in entities)
            {
                // Process ropes based on classname
                if (entity.GetStringValue("classname") != "keyframe_rope" && entity.GetStringValue("classname") != "move_rope")
                    continue;

                SourceEntity nextEntity = GetEntityByName(entities, entity.GetStringValue("NextKey"));
                if (nextEntity == null)
                    continue;

                GameObject ropeObject = UnityEngine.Object.Instantiate(ropePrefab);
                ropeObject.name = $"Rope_{ropeIndex}";

                RopeSim ropeSim = ropeObject.GetComponentInChildren<RopeSim>();
                RopeMeshRenderer ropeMeshRenderer = ropeObject.GetComponentInChildren<RopeMeshRenderer>();

                if (ropeSim != null && ropeMeshRenderer != null)
                {
                    Vector3 startPos = SourceToUnityPosition(entity.GetVectorValue("origin"));
                    Vector3 endPos = SourceToUnityPosition(nextEntity.GetVectorValue("origin"));

                    ropeSim.start.position = startPos;
                    ropeSim.end.position = endPos;

                    SetupRopeType(ropeSim, ropeMeshRenderer, entity.GetStringValue("classname"));
                    ropeObject.transform.SetParent(staticContainer.transform);
                    ropeIndex++;
                }
                else
                {
                    Debug.LogError("RopeSim or RopeMeshRenderer component missing on ropePrefab.");
                }
            }
            Debug.Log("Ropes placed successfully.");
        }

        private static void SetupRopeType(RopeSim ropeSim, RopeMeshRenderer ropeMeshRenderer, string classname)
        {
            if (classname == "keyframe_rope")
            {
                ropeSim.ConfigureKeyframeSettings(ropeSim.settleTime, ropeSim.animationLength, ropeSim.animationFPS, ropeSim.recordAnimation);
                ropeMeshRenderer.CreateRope(ropeSim.start.position, ropeSim.end.position);
            }
            else if (classname == "move_rope")
            {
                ropeSim.ConfigureMovementSettings(ropeSim.gravity.sqrMagnitude, ropeSim.strength, ropeSim.damping, ropeSim.noise, ropeSim.windDirection, ropeSim.windAmount);
                ropeMeshRenderer.CreateRope(ropeSim.start.position, ropeSim.end.position);
            }
            else
            {
                Debug.LogWarning("Unknown rope type: " + classname);
            }
        }

        private static SourceEntity GetEntityByName(List<SourceEntity> entities, string targetname)
        {
            foreach (var entity in entities)
            {
                if (entity.GetStringValue("targetname") == targetname)
                    return entity;
            }
            return null;
        }

        private static Vector3 SourceToUnityPosition(Vector3 source)
        {
            return new Vector3(source.x, source.z, source.y) * 0.0254f; // Converts Source to Unity scale
        }

        public static void Configure(this Transform transform, List<String> Data)
        {
            //return;
            String Classname = Data[Data.FindIndex(n => n == "classname") + 1], Targetname = Data[Data.FindIndex(n => n == "targetname") + 1];
            transform.name = Classname;

            //ResourceManager.LoadModel("editor/axis_helper").SetParent(transform, false);

            Int32 OriginIndex = Data.FindIndex(n => n == "origin");
            if (OriginIndex != -1)
            {
                //Old but gold
                String[] origin = Data[OriginIndex + 1].Split(' ');

                while (origin.Length != 3)
                {
                    Int32 TempIndex = OriginIndex + 1;
                    origin = Data[Data.FindIndex(TempIndex, n => n == "origin") + 1].Split(' ');
                }
                //Old but gold

                transform.position = new Vector3(-origin[1].ToSingle(), origin[2].ToSingle(), origin[0].ToSingle()) * uLoader.UnitScale;
            }

            Int32 AnglesIndex = Data.FindIndex(n => n == "angles");
            if (AnglesIndex != -1)
            {
                Vector3 EulerAngles = Data[AnglesIndex + 1].ToVector3();

                EulerAngles = new Vector3(EulerAngles.x, -EulerAngles.y, -EulerAngles.z);

                if (Classname.StartsWith("light", StringComparison.Ordinal))
                    EulerAngles.x = -EulerAngles.x;

                Int32 PitchIndex = Data.FindIndex(n => n == "pitch");
                //Lights
                if (PitchIndex != -1)
                    EulerAngles.x = -Data[PitchIndex + 1].ToSingle();

                transform.eulerAngles = EulerAngles;
            }

            if (Classname.Contains("trigger"))
            {
                for (Int32 i = 0; i < transform.childCount; i++)
                {
                    GameObject Child = transform.GetChild(i).gameObject;
                    Child.SetActive(false);
                    Child.AddComponent<BoxCollider>().isTrigger = true;
                }
            }

#if UNITY_EDITOR

            if (Classname.Equals("env_sprite"))
            {
                // Ensure the HDRP Lens Flare Data is available
                LensFlareComponentSRP lensFlare = transform.gameObject.AddComponent<LensFlareComponentSRP>();

                // Load the HDRP lens flare asset
                if (VBSPFile.GlowFlare == null)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets("Glow t:LensFlareDataSRP")[0]);
                    VBSPFile.GlowFlare = UnityEditor.AssetDatabase.LoadAssetAtPath<LensFlareDataSRP>(path);
                }

                if (VBSPFile.GlowFlare != null)
                {
                    // Assign the Lens Flare Data
                    lensFlare.lensFlareData = VBSPFile.GlowFlare;

                    // Configure HDRP-specific properties
                    lensFlare.intensity = Data[Data.FindIndex(n => n == "scale") + 1].ToSingle();
                    // Assuming the data string is in the format "R G B A" (e.g., "255 255 255 255")
                    string colorString = Data[Data.FindIndex(n => n == "rendercolor") + 1];
                    string[] colorComponents = colorString.Split(' ');

                    if (colorComponents.Length >= 3)
                    {
                        // Parse the color values
                        float r = float.Parse(colorComponents[0]) / 255f; // Convert from 0-255 to 0-1
                        float g = float.Parse(colorComponents[1]) / 255f;
                        float b = float.Parse(colorComponents[2]) / 255f;
                        float a = colorComponents.Length > 3 ? float.Parse(colorComponents[3]) / 255f : 1f; // Default alpha to 1 if not provided

                        // Create a GradientColorKey array
                        GradientColorKey[] colorKeys = new GradientColorKey[1];
                        colorKeys[0].color = new Color(r, g, b, a);
                        colorKeys[0].time = 0f; // The time at which this color is used in the gradient

                        // Create a GradientAlphaKey array
                        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[1];
                        alphaKeys[0].alpha = a;
                        alphaKeys[0].time = 0f;

                        // Create the gradient
                        Gradient gradient = new Gradient();
                        gradient.SetKeys(colorKeys, alphaKeys);

                        // Assign the gradient to the lens flare data
                        lensFlare.lensFlareData.elements[0].colorGradient = gradient;
                    }
                    else
                    {
                        Debug.LogWarning("Invalid rendercolor format in Data.");
                    }

                    lensFlare.lensFlareData.elements[0].noiseSpeed = Data[Data.FindIndex(n => n == "GlowProxySize") + 1].ToSingle();
                }
                else
                {
                    Debug.LogError("Failed to load HDRP Lens Flare Data.");
                }

                return;
            }

            // 4) env_cubemap  -------------------------------------------------------
            bool handledEnvCubemap = false;
            if (Classname.Equals("env_cubemap"))
            {
                handledEnvCubemap = true;

                // Tworzymy osobny obiekt probe (nie jako child)
                GameObject probeObj = new GameObject("env_cubemap_probe");
                probeObj.transform.localRotation = Quaternion.identity;

                // Pozycja z origin jeśli jest
                int originIndex = Data.FindIndex(n => n == "origin");
                if (originIndex != -1 && originIndex + 1 < Data.Count)
                {
                    string[] originParts = Data[originIndex + 1].Split(' ');
                    if (originParts.Length >= 3)
                    {
                        Vector3 origin = new Vector3(
                            -originParts[1].ToSingle(),
                            originParts[2].ToSingle(),
                            originParts[0].ToSingle()
                        ) * uLoader.UnitScale;
                        probeObj.transform.position = origin;
                    }
                }

                // Reflection Probe (HDRP)
                ReflectionProbe probe = probeObj.AddComponent<ReflectionProbe>();
                probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
                probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;

                // Wczytaj parametry
                int cubemapSize = 128;
                int cubemapSizeIndex = Data.FindIndex(n => n == "cubemapsize");
                if (cubemapSizeIndex != -1 && cubemapSizeIndex + 1 < Data.Count)
                    int.TryParse(Data[cubemapSizeIndex + 1], out cubemapSize);

                int hdrSize = 0;
                int hdrSizeIndex = Data.FindIndex(n => n == "cubemapsizehdr");
                if (hdrSizeIndex != -1 && hdrSizeIndex + 1 < Data.Count)
                    int.TryParse(Data[hdrSizeIndex + 1], out hdrSize);
                if (hdrSize > 0)
                    cubemapSize = hdrSize;

                probe.resolution = Mathf.Clamp(cubemapSize, 16, 4096);

                // Box projection / influence volume size
                float boxSize = 4f;
                int boxSizeIndex = Data.FindIndex(n => n == "env_cubemap_boxsize");
                if (boxSizeIndex != -1 && boxSizeIndex + 1 < Data.Count)
                    float.TryParse(Data[boxSizeIndex + 1], out boxSize);
                probe.size = Vector3.one * boxSize;
                probe.boxProjection = true;

                // Intensity
                float intensity = 1f;
                int intensityIndex = Data.FindIndex(n => n == "env_cubemap_intensity");
                if (intensityIndex != -1 && intensityIndex + 1 < Data.Count)
                    float.TryParse(Data[intensityIndex + 1], out intensity);
                probe.intensity = intensity;

                // Enabled / startdisabled
                bool startDisabled = false;
                int startDisabledIndex = Data.FindIndex(n => n == "startdisabled");
                if (startDisabledIndex != -1 && startDisabledIndex + 1 < Data.Count)
                    startDisabled = Data[startDisabledIndex + 1] == "1";
                probe.enabled = !startDisabled;

                bool manualUpdate = false;
                int manualUpdateIndex = Data.FindIndex(n => n == "manualupdate");
                if (manualUpdateIndex != -1 && manualUpdateIndex + 1 < Data.Count)
                    manualUpdate = Data[manualUpdateIndex + 1] == "1";

                if (probe.enabled && !manualUpdate)
                    probe.RenderProbe();

                // HDRP-specific extras
                HDAdditionalReflectionData hdData = probeObj.AddComponent<HDAdditionalReflectionData>();
                hdData.influenceVolume.shape = InfluenceShape.Box;
                hdData.influenceVolume.boxSize = Vector3.one * boxSize;
                hdData.multiplier = intensity;
                hdData.frameSettings.SetEnabled(FrameSettingsField.RayTracing, true);

                return;
            }

            // Fallbackowy globalny probe jeśli nie było env_cubemap (np. worldspawn)
            if (!handledEnvCubemap && Classname.Equals("worldspawn"))
            {

                if (GameObject.Find("Fallback_EnvCubemap_Probe") == null)
                {
                    GameObject fallbackProbeObj = new GameObject("Fallback_EnvCubemap_Probe");
                    fallbackProbeObj.transform.position = Vector3.zero;
                    fallbackProbeObj.transform.rotation = Quaternion.identity;

                    ReflectionProbe fallbackProbe = fallbackProbeObj.AddComponent<ReflectionProbe>();
                    fallbackProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
                    fallbackProbe.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
                    fallbackProbe.resolution = 256;
                    fallbackProbe.size = Vector3.one * 50f;
                    fallbackProbe.boxProjection = true;
                    fallbackProbe.intensity = 1f;
                    fallbackProbe.RenderProbe();

                    HDAdditionalReflectionData hdDataFallback = fallbackProbeObj.AddComponent<HDAdditionalReflectionData>();
                    hdDataFallback.influenceVolume.shape = InfluenceShape.Box;
                    hdDataFallback.influenceVolume.boxSize = Vector3.one * 50f;
                    hdDataFallback.multiplier = 1f;
                    hdDataFallback.frameSettings.SetEnabled(FrameSettingsField.RayTracing, true);
                }
#endif
            }



           // if (Classname.Equals("point_viewcontrol"))
           // {
             //   transform.gameObject.AddComponent<point_viewcontrol>().Start();
           // }

            //3D Skybox
            if (uLoader.Use3DSkybox && Classname.Equals("sky_camera"))
            {
                //Setup 3DSkybox
                Camera playerCamera = new GameObject("CameraPlayer").AddComponent<Camera>();
                Camera skyCamera = transform.gameObject.AddComponent<Camera>();

                CameraFly camFly = playerCamera.gameObject.AddComponent<CameraFly>();
                camFly.skyScale = Data[Data.FindIndex(n => n == "scale") + 1].ToSingle();
                camFly.offset3DSky = transform.position;
                camFly.skyCamera = skyCamera.transform;

                playerCamera.depth = -1;
                playerCamera.clearFlags = CameraClearFlags.Depth;

                skyCamera.depth = -2;
                skyCamera.clearFlags = CameraClearFlags.Skybox;
                //Setup 3DSkybox
                return;
            }

            #region the whole Black Mesa entities

            //3D Skybox
            if (Classname.Equals("info_player_start"))
            {
                uResourceManager.LoadModel("editor/playerstart", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_healthcharger"))
            {
                uResourceManager.LoadModel("props_blackmesa/health_charger", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_suitcharger"))
            {
                uResourceManager.LoadModel("props_blackmesa/hev_charger", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_357"))
            {
                uResourceManager.LoadModel("weapons/w_357", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_human_security"))
            {
                uResourceManager.LoadModel("humans/guard", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_human_scientist_kleiner"))
            {
                uResourceManager.LoadModel("humans/scientist_kliener", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_human_scientist_female"))
            {
                uResourceManager.LoadModel("humans/scientist_female", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_human_scientist_eli"))
            {
                uResourceManager.LoadModel("humans/scientist_eli", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_human_scientist"))
            {
                uResourceManager.LoadModel("humans/scientist", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_crowbar"))
            {
                uResourceManager.LoadModel("weapons/w_crowbar", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_crossbow"))
            {
                uResourceManager.LoadModel("weapons/w_crossbow", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_frag"))
            {
                uResourceManager.LoadModel("weapons/w_grenade", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_glock"))
            {
                uResourceManager.LoadModel("weapons/w_glock", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_gluon"))
            {
                uResourceManager.LoadModel("weapons/w_egon_pickup", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_hivehand"))
            {
                uResourceManager.LoadModel("weapons/w_hgun", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_mp5"))
            {
                uResourceManager.LoadModel("weapons/w_mp5", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_shotgun"))
            {
                uResourceManager.LoadModel("weapons/w_shotgun", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_rpg"))
            {
                uResourceManager.LoadModel("weapons/w_rpg", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_snark"))
            {
                uResourceManager.LoadModel("xenians/snarknest", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }

            if (Classname.Equals("item_weapon_rpg"))
            {
                uResourceManager.LoadModel("weapons/w_rpg_mp", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_satchel"))
            {
                uResourceManager.LoadModel("weapons/w_satchel_mp", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_crowbar"))
            {
                uResourceManager.LoadModel("weapons/w_crowbar_mp", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_crossbow"))
            {
                uResourceManager.LoadModel("weapons/w_crossbow_mp", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_frag"))
            {
                uResourceManager.LoadModel("weapons/w_grenade_mp", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_glock"))
            {
                uResourceManager.LoadModel("weapons/w_glock_mp", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_gluon"))
            {
                uResourceManager.LoadModel("weapons/w_egon_pickup_mp", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_hivehand"))
            {
                uResourceManager.LoadModel("weapons/w_hgun_mp", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_mp5"))
            {
                uResourceManager.LoadModel("weapons/w_mp5_mp", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_shotgun"))
            {
                uResourceManager.LoadModel("weapons/w_shotgun_mp", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_rpg"))
            {
                uResourceManager.LoadModel("weapons/w_rpg_mp", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_snark"))
            {
                uResourceManager.LoadModel("xenians/snarknest", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }

            if (Classname.Equals("item_weapon_rpg"))
            {
                uResourceManager.LoadModel("weapons/w_rpg", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_satchel"))
            {
                uResourceManager.LoadModel("weapons/w_satchel", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_tau"))
            {
                uResourceManager.LoadModel("weapons/w_gauss", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_tripmine"))
            {
                uResourceManager.LoadModel("weapons/w_tripmine", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_ammo_357"))
            {
                uResourceManager.LoadModel("weapons/w_357ammobox", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }

            if (Classname.Equals("item_ammo_crossbow"))
            {
                uResourceManager.LoadModel("weapons/w_crossbow_clip", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }

            if (Classname.Equals("item_ammo_glock"))
            {
                uResourceManager.LoadModel("weapons/w_9mmclip", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_ammo_energy"))
            {
                uResourceManager.LoadModel("weapons/w_gaussammo", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_ammo_mp5"))
            {
                uResourceManager.LoadModel("weapons/w_9mmarclip", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("info_bigmomma"))
            {
                uResourceManager.LoadModel("editor/ground_node", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_ammo_shotgun"))
            {
                uResourceManager.LoadModel("weapons/w_shotbox", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_grenade_mp5"))
            {
                uResourceManager.LoadModel("weapons/w_argrenade", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_grenade_rpg"))
            {
                uResourceManager.LoadModel("weapons/w_rpgammo", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_ammo_canister"))
            {
                uResourceManager.LoadModel("weapons/w_weaponbox", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_ammo_crate"))
            {
                uResourceManager.LoadModel("items/ammocrate_rockets", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_suit"))
            {
                uResourceManager.LoadModel("props_am/hev_suit", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_battery"))
            {
                uResourceManager.LoadModel("weapons/w_battery", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_healthkit"))
            {
                uResourceManager.LoadModel("weapons/w_medkit", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_healthkit"))
            {
                uResourceManager.LoadModel("weapons/w_medkit_classic", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_healthkit"))
            {
                uResourceManager.LoadModel("weapons/w_medkit_stiff", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }


            if (Classname.Equals("item_longjump"))
            {
                uResourceManager.LoadModel("weapons/w_longjump", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("grenade_satchel"))
            {
                uResourceManager.LoadModel("weapons/w_satchel", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("grenade_tripmine"))
            {
                uResourceManager.LoadModel("weapons/w_tripmine", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_tau"))
            {
                uResourceManager.LoadModel("weapons/w_gauss", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_weapon_tripmine"))
            {
                uResourceManager.LoadModel("weapons/w_tripmine", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_ammo_357"))
            {
                uResourceManager.LoadModel("weapons/w_357ammobox", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("weapons/w_crossbow_clip"))
            {
                uResourceManager.LoadModel("weapons/w_357ammobox", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_ammo_crossbow"))
            {
                uResourceManager.LoadModel("weapons/w_crossbow_clip", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }

            if (Classname.Equals("item_ammo_glock"))
            {
                uResourceManager.LoadModel("weapons/w_9mmclip", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_ammo_energy"))
            {
                uResourceManager.LoadModel("weapons/w_gaussammo", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_ammo_mp5"))
            {
                uResourceManager.LoadModel("weapons/w_9mmarclip", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("info_bigmomma"))
            {
                uResourceManager.LoadModel("editor/ground_node", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_ammo_shotgun"))
            {
                uResourceManager.LoadModel("weapons/w_shotbox", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_grenade_mp5"))
            {
                uResourceManager.LoadModel("weapons/w_argrenade", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_grenade_rpg"))
            {
                uResourceManager.LoadModel("weapons/w_rpgammo", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_ammo_canister"))
            {
                uResourceManager.LoadModel("weapons/w_weaponbox", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_ammo_crate"))
            {
                uResourceManager.LoadModel("items/ammocrate_rockets", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_suit"))
            {
                uResourceManager.LoadModel("props_am/hev_suit", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }

            if (Classname.Equals("item_crate"))
            {
                uResourceManager.LoadModel("props_generic/bm_supplycrate01", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }

            if (Classname.Equals("item_longjump"))
            {
                uResourceManager.LoadModel("weapons/w_longjump", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("grenade_satchel"))
            {
                uResourceManager.LoadModel("weapons/w_satchel", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("grenade_tripmine"))
            {
                uResourceManager.LoadModel("weapons/w_tripmine", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_nihilanth"))
            {
                uResourceManager.LoadModel("xenians/nihilanth", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_ichthyosaur"))
            {
                uResourceManager.LoadModel("ichthyosaur", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_maintenance"))
            {
                uResourceManager.LoadModel("humans/maintenance/maintenance_1", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_gman"))
            {
                uResourceManager.LoadModel("gman", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_generic"))
            {
                uResourceManager.LoadModel("gman", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_barnacle"))
            {
                uResourceManager.LoadModel("barnacle", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_xortEB"))
            {
                uResourceManager.LoadModel("vortigaunt_slave", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_headcrab"))
            {
                uResourceManager.LoadModel("headcrabclassic", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_headcrab_fast"))
            {
                uResourceManager.LoadModel("Headcrab", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_headcrab_black"))
            {
                uResourceManager.LoadModel("Headcrabblack", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_headcrab_baby"))
            {
                uResourceManager.LoadModel("xenians/bebcrab", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_beneathticle"))
            {
                uResourceManager.LoadModel("xenians/barnacle_underwater", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("physics_cannister"))
            {
                uResourceManager.LoadModel("fire_equipment/w_weldtank", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("func_fish_pool"))
            {
                uResourceManager.LoadModel("Junkola", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_bullsquid"))
            {
                uResourceManager.LoadModel("xenians/bullsquid", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_houndeye"))
            {
                uResourceManager.LoadModel("xenians/houndeye", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_bullsquid_melee"))
            {
                uResourceManager.LoadModel("xenians/bullsquid", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_houndeye_suicide"))
            {
                uResourceManager.LoadModel("xenians/houndeye_suicide", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_houndeye_knockback"))
            {
                uResourceManager.LoadModel("xenians/houndeye_knockback", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_human_assassin"))
            {
                uResourceManager.LoadModel("humans/hassassin", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_human_commander"))
            {
                uResourceManager.LoadModel("humans/marine", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_human_medic"))
            {
                uResourceManager.LoadModel("humans/marine", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("misc_xen_shield"))
            {
                uResourceManager.LoadModel("xenians/shield/pentagonal.hexecontahedron/nihilanth/panel.%03d", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_human_grunt"))
            {
                uResourceManager.LoadModel("humans/marine", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_human_grenadier"))
            {
                uResourceManager.LoadModel("humans/marine", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_xontroller"))
            {
                uResourceManager.LoadModel("xenians/controller", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_alien_grunt_unarmored"))
            {
                uResourceManager.LoadModel("xenians/agrunt_unarmored", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_alien_grunt_melee"))
            {
                string text4 = Data[Data.FindIndex((string n) => n == "model") + 1];
                uResourceManager.LoadModel("xenians/agrunt_unarmored", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_alien_grunt"))
            {
                uResourceManager.LoadModel("xenians/agrunt", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_xen_grunt"))
            {
                uResourceManager.LoadModel("xenians/agrunt", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_protozoan"))
            {
                uResourceManager.LoadModel("xenians/protozoan", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_cockroach"))
            {
                uResourceManager.LoadModel("fauna/roach", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_flyer_flock"))
            {
                uResourceManager.LoadModel("xenians/flock", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_gargantua"))
            {
                uResourceManager.LoadModel("xenians/garg", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_gonarch"))
            {
                uResourceManager.LoadModel("xenians/gonarch", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_manta"))
            {
                uResourceManager.LoadModel("xenians/manta_jet", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }

            if (Classname.Equals("npc_apache"))
            {
                uResourceManager.LoadModel("props_vehicles/apache", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_osprey"))
            {
                uResourceManager.LoadModel("props_vehicles/osprey", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_rat"))
            {
                uResourceManager.LoadModel("fauna/rat", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_tentacle"))
            {
                uResourceManager.LoadModel("xenians/tentacle", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_xentacle"))
            {
                uResourceManager.LoadModel("xenians/xentacle", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_snark"))
            {
                uResourceManager.LoadModel("xenians/snark", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("item_tow_missile"))
            {
                uResourceManager.LoadModel("props_marines/tow_missile_projectile", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }

            if (Classname.Equals("npc_lav"))
            {
                uResourceManager.LoadModel("props_vehicles/lav", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }



            if (Classname.Equals("npc_sentry_ceiling"))
            {
                uResourceManager.LoadModel("npcs/sentry_ceiling", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }

            if (Classname.Equals("npc_alien_grunt_elite"))
            {
                uResourceManager.LoadModel("xenians/agrunt", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_sentry_ground"))
            {
                uResourceManager.LoadModel("npcs/sentry_ground", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("prop_train_awesome"))
            {
                uResourceManager.LoadModel("props_vehicles/oar_awesome_tram", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("prop_train_apprehension"))
            {
                uResourceManager.LoadModel("props_vehicles/oar_tram", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_zombie_scientist"))
            {
                uResourceManager.LoadModel("zombies/zombie_sci", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_abrams"))
            {
                uResourceManager.LoadModel("props_vehicles/abrams", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("env_mortar_controller"))
            {
                uResourceManager.LoadModel("props_st/airstrike_map", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_zombie_scientist_torso"))
            {
                uResourceManager.LoadModel("zombies/zombie_sci_torso", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_zombie_grunt"))
            {
                uResourceManager.LoadModel("zombies/zombie_grunt", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_zombie_grunt_torso"))
            {
                uResourceManager.LoadModel("zombies/zombie_grunt_torso", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_zombie_security"))
            {
                uResourceManager.LoadModel("zombies/zombie_guard", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_zombie_hev"))
            {
                uResourceManager.LoadModel("zombies/zombie_hev", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("prop_surgerybot"))
            {
                uResourceManager.LoadModel("props_questionableethics/qe_surgery_bot_main", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("env_xen_healpool"))
            {
                uResourceManager.LoadModel("props_xen/xen_healingpool_full", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("camera_satellite"))
            {
                uResourceManager.LoadModel("editor/camera", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_alien_controller"))
            {
                uResourceManager.LoadModel("xenians/controller", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_crow"))
            {
                uResourceManager.LoadModel("crow", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_seagull"))
            {
                uResourceManager.LoadModel("seagull", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_pigeon"))
            {
                uResourceManager.LoadModel("pigeon", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("nihilanth_pylon"))
            {
                uResourceManager.LoadModel("props_xen/nil_pylon", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_alien_slave_dummy"))
            {
                uResourceManager.LoadModel("vortigaunt_slave", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_alien_slave"))
            {
                uResourceManager.LoadModel("vortigaunt_slave", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_xort"))
            {
                uResourceManager.LoadModel("vortigaunt_slave", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }

            if (Classname.Equals("npc_xenturret"))
            {
                uResourceManager.LoadModel("props_xen/xen_turret", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }


            if (Classname.Equals("npc_xentree"))
            {
                uResourceManager.LoadModel("props_xen/foliage/hacker_tree", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_sniper"))
            {
                uResourceManager.LoadModel("combine_soldier", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_puffballfungus"))
            {
                uResourceManager.LoadModel("props_xen/xen_puffballfungus", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_plantlight_stalker"))
            {
                uResourceManager.LoadModel("props_xen/xen_plantlightstalker", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }
            if (Classname.Equals("npc_plantlight"))
            {
                uResourceManager.LoadModel("props_xen/xen_protractinglight", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
            }

            if (Classname.Equals("info_player_marine"))
            {
                uResourceManager.LoadModel("Player/mp_marine", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);

            }
            if (Classname.Equals("info_player_scientist"))
            {

                uResourceManager.LoadModel("Player/mp_scientist_hev", uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);


            }

            #endregion
            Int32 RenderModeIndex = Data.FindIndex(n => n == "rendermode");
            if (RenderModeIndex != -1)
            {
                if (Data[RenderModeIndex + 1] == "10")
                {
                    for (Int32 i = 0; i < transform.childCount; i++)
                    {
                        GameObject Child = transform.GetChild(i).gameObject;
                        Child.GetComponent<Renderer>().enabled = false;
                    }
                }
            }

            if (Classname.Contains("prop_") || Classname.Contains("npc_"))// || Classname.Equals("asw_door"))
            {
                string ModelName = Data[Data.FindIndex(n => n == "model") + 1];

                if (!string.IsNullOrEmpty(ModelName))
                {
                    uResourceManager.LoadModel(ModelName, uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
                    return;
                }

                return;
            }

            if (Classname.Contains("generic_actor"))// || Classname.Equals("asw_door"))
            {
                string ModelName = Data[Data.FindIndex(n => n == "model") + 1];

                if (!string.IsNullOrEmpty(ModelName))
                {
                    uResourceManager.LoadModel(ModelName, uLoader.UseFullAnims, uLoader.UseHitboxesOnModel).SetParent(transform, false);
                    return;
                }

                return;
            }



            // Example materials for rope/track/physbox
            Material ropeMaterial = new Material(Shader.Find("HDRP/Unlit")) { color = Color.gray };
            Texture2D[,] ropeTexArray = uResourceManager.LoadTexture("cable/cable.vtf");
            if (ropeTexArray != null && ropeTexArray.Length > 0)
            {
                ropeMaterial.mainTexture = ropeTexArray[0, 0];
            }

            Material trackMaterial = new Material(Shader.Find("HDRP/Unlit")) { color = Color.black };
            Texture2D[,] trackTexArray = uResourceManager.LoadTexture("cable/steel.vtf");
            if (trackTexArray != null && trackTexArray.Length > 0)
            {
                trackMaterial.mainTexture = trackTexArray[0, 0];
            }

            Material physicsMaterial = new Material(Shader.Find("HDRP/Lit")) { color = Color.black };

            // -----------------------------------------------------
            // 1) keyframe_rope / move_rope
            //    Create a rope purely via code (LineRenderer, etc.)
            // -----------------------------------------------------
            if (Classname.Equals("keyframe_rope") || Classname.Equals("move_rope"))
            {
                // Create a rope gameobject
                GameObject ropeObject = new GameObject("RopeObject");
                ropeObject.transform.position = transform.position;
                ropeObject.transform.SetParent(transform.parent);

                // Add a LineRenderer for the visual rope
                LineRenderer lineRenderer = ropeObject.AddComponent<LineRenderer>();
                lineRenderer.material = ropeMaterial;
                lineRenderer.widthMultiplier = 0.05f;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, transform.position + transform.forward * 5f);

                // Add your rope simulation logic (if you have it)
                RopeSim ropeSim = ropeObject.AddComponent<RopeSim>();
                ropeSim.start = transform;
                ropeSim.end = GetNextKeyframeTarget(Data);
                ApplyRopePropertiesFromData(ropeSim, Data);

                return;
            }

            // -----------------------------------------------------
            // 2) path_track
            //    Create a new GameObject that visually represents
            //    a track (e.g. a line or simple mesh).
            // -----------------------------------------------------
            else if (Classname.Equals("path_track"))
            {
                // Create a track object
                GameObject trackObject = new GameObject("TrackObject");
                trackObject.transform.position = transform.position;
                trackObject.transform.SetParent(transform.parent);

                // For simplicity, let's give it a small cylinder or line
                // Here, we do a line renderer to connect this track to the next node
                LineRenderer lineRenderer = trackObject.AddComponent<LineRenderer>();
                lineRenderer.material = trackMaterial;
                lineRenderer.widthMultiplier = 0.1f;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, transform.position);

                // If the track has a next track target, link them
                Transform nextTarget = GetNextKeyframeTarget(Data);
                if (nextTarget != null)
                {
                    lineRenderer.SetPosition(1, nextTarget.position);
                }
                else
                {
                    lineRenderer.SetPosition(1, transform.position + transform.forward * 2f);
                }

                return;
            }

            // -----------------------------------------------------
            // 3) func_physbox
            //    Create a new physics-enabled box with MeshFilter +
            //    MeshRenderer + BoxCollider + Rigidbody.
            // -----------------------------------------------------
            else if (Classname.Equals("func_physbox"))
            {
                // Create a new empty
                GameObject physObject = new GameObject("PhysboxObject");
                physObject.transform.position = transform.position;
                physObject.transform.SetParent(transform.parent);

                // Add a box mesh
                MeshFilter meshFilter = physObject.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = CreateBoxMesh(1f, 1f, 1f);
                // Alternatively, you can load a specific mesh if you have one

                // Render it
                MeshRenderer meshRenderer = physObject.AddComponent<MeshRenderer>();
                meshRenderer.material = physicsMaterial;

                // Add a BoxCollider for collision
                BoxCollider boxCollider = physObject.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(1f, 1f, 1f);

                // Add a rigidbody
                Rigidbody rb = physObject.AddComponent<Rigidbody>();
                rb.mass = 10f;
                return;
            }

            // Get the next keyframe target for the rope
            Transform GetNextKeyframeTarget(List<string> Data)
            {
                int nextKeyIndex = Data.FindIndex(n => n == "NextKey");
                if (nextKeyIndex != -1 && nextKeyIndex + 1 < Data.Count)
                {
                    string nextKeyName = Data[nextKeyIndex + 1];
                    GameObject nextKeyObject = GameObject.Find(nextKeyName);
                    if (nextKeyObject != null)
                    {
                        return nextKeyObject.transform;
                    }
                }

                return null;
            }

            // Apply properties like slack, barbed, etc. to RopeSim from entity data
            static void ApplyRopePropertiesFromData(RopeSim ropeSim, List<string> Data)
            {
                // Slack
                int slackIndex = Data.FindIndex(n => n == "slack");
                if (slackIndex != -1 && slackIndex + 1 < Data.Count)
                {
                    ropeSim.slack = Data[slackIndex + 1].ToSingle();
                }

                // Barbed wire
                int barbedIndex = Data.FindIndex(n => n == "barbed");
                if (barbedIndex != -1 && barbedIndex + 1 < Data.Count && Data[barbedIndex + 1] == "1")
                {
                    AddBarbedWireEffect(ropeSim.gameObject);
                }

                // Breakable
                int breakableIndex = Data.FindIndex(n => n == "breakable");
                if (breakableIndex != -1 && breakableIndex + 1 < Data.Count && Data[breakableIndex + 1] == "1")
                {
                    ropeSim.sticky = false; // Example: toggling sticky off to simulate break
                }

                // Collide
                int collideIndex = Data.FindIndex(n => n == "collide");
                if (collideIndex != -1 && collideIndex + 1 < Data.Count && Data[collideIndex + 1] == "1")
                {
                    ropeSim.collidesWith = LayerMask.GetMask("Player", "Enemies");
                }

                // Dangling
                int danglingIndex = Data.FindIndex(n => n == "dangling");
                if (danglingIndex != -1 && danglingIndex + 1 < Data.Count && Data[danglingIndex + 1] == "1")
                {
                    ropeSim.staticEnd = false;
                }

                // Settle Time
                int settleTimeIndex = Data.FindIndex(n => n == "settle");
                if (settleTimeIndex != -1 && settleTimeIndex + 1 < Data.Count)
                {
                    ropeSim.settleTime = Data[settleTimeIndex + 1].ToSingle();
                }
            }

            static void AddBarbedWireEffect(GameObject ropeObject)
            {
                // Create child object for barbed wire
                GameObject barbedWireObj = new GameObject("BarbedWireObject");
                barbedWireObj.transform.SetParent(ropeObject.transform);
                // Visual line
                LineRenderer barbedLine = barbedWireObj.AddComponent<LineRenderer>();
                barbedLine.widthMultiplier = 0.02f;
                barbedLine.positionCount = 2;
                barbedLine.SetPosition(0, Vector3.zero);
                barbedLine.SetPosition(1, Vector3.forward * 5f);
                barbedLine.material = new Material(Shader.Find("HDRP/Lit")) { color = Color.red };

                // Collider for damage
                BoxCollider collider = barbedWireObj.AddComponent<BoxCollider>();
                collider.isTrigger = true;
                collider.center = new Vector3(0f, 0f, 2.5f);
                collider.size = new Vector3(0.1f, 0.1f, 5f);

                // Damage script (user-defined)
                barbedWireObj.AddComponent<BarbedWireDamage>();
            }


            static Mesh CreateBoxMesh(float width, float height, float depth)
            {
                Mesh mesh = new Mesh();

                // 8 vertices of a cube
                Vector3 p0 = new Vector3(-width * 0.5f, -height * 0.5f, depth * 0.5f);
                Vector3 p1 = new Vector3(width * 0.5f, -height * 0.5f, depth * 0.5f);
                Vector3 p2 = new Vector3(width * 0.5f, -height * 0.5f, -depth * 0.5f);
                Vector3 p3 = new Vector3(-width * 0.5f, -height * 0.5f, -depth * 0.5f);
                Vector3 p4 = new Vector3(-width * 0.5f, height * 0.5f, depth * 0.5f);
                Vector3 p5 = new Vector3(width * 0.5f, height * 0.5f, depth * 0.5f);
                Vector3 p6 = new Vector3(width * 0.5f, height * 0.5f, -depth * 0.5f);
                Vector3 p7 = new Vector3(-width * 0.5f, height * 0.5f, -depth * 0.5f);

                // Assign vertices
                mesh.vertices = new Vector3[]
                {
                    p0, p1, p2, p3, // Bottom
                    p4, p5, p6, p7 // Top
                };

                // Triangles
                int[] triangles = new int[]
                {
                    // Bottom (p0, p1, p2, p3)
                    0, 1, 2,
                    0, 2, 3,
                    // Left (p0, p3, p7, p4)
                    3, 2, 6,
                    3, 6, 7,
                    // Front (p0, p4, p5, p1)
                    0, 4, 5,
                    0, 5, 1,
                    // Back (p2, p6, p7, p3)
                    2, 5, 6,
                    2, 1, 5,
                    // Right (p1, p5, p6, p2)
                    4, 7, 6,
                    4, 6, 5,
                    // Top (p4, p7, p6, p5)
                    3, 7, 4,
                    2, 6, 5
                };

                mesh.triangles = triangles;
                mesh.RecalculateNormals();
                return mesh;
            }


            if (uLoader.ParseDecals && Classname.Equals("infodecal"))
            {
                string DecalName = GetValueSafe(Data, "texture");
                if (string.IsNullOrEmpty(DecalName))
                {
                    Debug.LogWarning("[EntitySetup] infodecal bez parametru 'texture'.");
                    return;
                }

                VMTFile DecalMaterial = uResourceManager.LoadMaterial(DecalName);
                if (DecalMaterial == null || DecalMaterial.Material == null)
                {
                    Debug.LogWarning($"[EntitySetup] Nie udało się wczytać materiału dla decal '{DecalName}'.");
                    return;
                }

                Texture mainTex = DecalMaterial.Material.mainTexture;
                if (mainTex == null)
                {
                    Debug.LogWarning($"[EntitySetup] Materiał decal '{DecalName}' nie ma mainTexture.");
                    return;
                }

                float DecalScale = 1f;
                try
                {
                    DecalScale = DecalMaterial.GetSingle("$decalscale");
                    if (DecalScale <= 0f) DecalScale = 1f;
                }
                catch
                {
                    DecalScale = 1f;
                }

                int DecalWidth = mainTex.width;
                int DecalHeight = mainTex.height;
                if (DecalWidth <= 0 || DecalHeight <= 0)
                {
                    Debug.LogWarning($"[EntitySetup] Nieprawidłowy rozmiar tekstury decal '{DecalName}': {DecalWidth}x{DecalHeight}");
                    return;
                }

                Sprite DecalTexture = Sprite.Create(
                    (Texture2D)mainTex,
                    new Rect(0, 0, DecalWidth, DecalHeight),
                    Vector2.zero,
                    100f,
                    0,
                    SpriteMeshType.FullRect
                );

                Decal_ DecalBuilder = transform.gameObject.GetComponent<Decal_>();
                if (DecalBuilder == null)
                    DecalBuilder = transform.gameObject.AddComponent<Decal_>();

#if UNITY_EDITOR
                if (uLoader.DebugMaterials)
                {
                    var dbg = transform.gameObject.GetComponent<DebugMaterial>();
                    if (dbg == null)
                        dbg = transform.gameObject.AddComponent<DebugMaterial>();
                    dbg.Init(DecalMaterial);
                }
#endif

                DecalBuilder.SetDirection();
                DecalBuilder.MaxAngle = 87.5f;
                DecalBuilder.Offset = 0.001f;
                DecalBuilder.Sprite = DecalTexture;
                DecalBuilder.Material = DecalMaterial.Material;
                if (DecalBuilder.Material.HasProperty("_BaseColorMap"))
                    DecalBuilder.Material.SetTextureScale("_BaseColorMap", new Vector2(-1, 1));

                float ScaleX = (DecalWidth * DecalScale) * uLoader.UnitScale;
                float ScaleY = (DecalHeight * DecalScale) * uLoader.UnitScale;
                float DepthSize = Mathf.Min(ScaleX, ScaleY);

                transform.localScale = new Vector3(ScaleX, ScaleY, DepthSize);
                transform.position += new Vector3(0f, 0f, 0.001f);

#if !UNITY_EDITOR
    DecalBuilder.BuildAndSetDirty();
#endif
            }
        }

// pomocnicza metoda (w tej samej klasie EntitySetup)
private static string GetValueSafe(List<string> data, string key)
        {
            int idx = data.FindIndex(n => string.Equals(n, key, StringComparison.OrdinalIgnoreCase));
            if (idx != -1 && idx + 1 < data.Count)
                return data[idx + 1];
            return null;
        }

    }
}

