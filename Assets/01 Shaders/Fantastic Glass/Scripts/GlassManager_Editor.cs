#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace FantasticGlass
{
	/// <summary>
	/// Glass manager editor.
	/// NOTE: Do not place this file in an 'Editor' folder as it will break.
	/// </summary>
	[CustomEditor (typeof(GlassManager))]
	[CanEditMultipleObjects]
	public class GlassManager_Editor : Editor
	{
		#region Member Variables

		GlassManager manager;

		//GlassSystemSettings glassEditorSettings;

		EditorTools tools;
		public static string toolsName = "GlassManager_Editor";

		public static string title_layerWarning = "Layer Missing";
		public static string message_layerWarning = "Please make sure the following Layers exist:\n\nGlassFront\n\nGlassBack\n\n(Edit > Project Settings > Tags and Layers)";
		public static string okButton_layerWarning = "OK";
		public static string cancelButton_layerWarning = "Disable This Warning";

		string[] textureAALabels = new string[] { "None", "2 Samples", "4 Samples", "8 Samples" };
		int[] textureAAItems = new int[] { 1, 2, 4, 8 };
		int textureAAIndex = 0;

		GlassSystemSettings settings;

		GlassRenderOrderManager renderOrderManager;

		#endregion

		#region Callbacks (Enable)

		void OnEnable ()
		{
			if (tools == null)
				tools = new EditorTools (toolsName);

			if (manager == null)
				manager = (GlassManager)target;

			if (manager == null)
				return;

			if (renderOrderManager == null)
				renderOrderManager = GlassRenderOrderManager.Instance;

			manager.InitPaths ();

			settings = manager.LoadSystemSettings ();

			manager.LoadManager ();

			UpdateTextureAAIndex ();

			manager.UpdateActiveMaterials ();

			if (!manager.LayersExist ())
				Show_LayersWarning ();
		}

		void UpdateTextureAAIndex ()
		{
			for (int i = 0; i < textureAAItems.Length; i++) {
				if (textureAAItems [i] == manager.depthTextureAA) {
					textureAAIndex = i;
					break;
				}
			}
		}

		#endregion

		#region GUI

		public override void OnInspectorGUI ()
		{
			manager = (GlassManager)target;

			if (tools == null)
				tools = new EditorTools (toolsName);

			if (settings.useCustomGUI) {
				GUI.skin = settings.guiSkin;
			}

			Section_SettingsType ();

			Section_Glass ();

			Section_Materials ();

			Section_Rendering ();

			Section_Debugging ();

			Section_Info ();

			Section_GUI ();

			Section_Camera ();

			if (settings.useCustomGUI) {
				GUI.skin = null;
			}
		}

		#endregion

		#region GUI Section - Settings Type

		void Section_SettingsType ()
		{
			tools.StartEdit (manager, "Changed Settings Type");
			tools.StartChangeCheck ();
			manager.managerSettingsType = (GlassManagerSettingsType)tools.EnumOption ("Current Type", manager.managerSettingsType);
			switch (manager.managerSettingsType) {
			case GlassManagerSettingsType.LoadFromGlass:
				{
					tools.StartSection ();
					tools.GlassOption ("Glass Source", ref manager.chosenGlassSettingsSource);
					if (tools.ShowSection ("LoadFromGlass?", ref manager.showSection_SettingsType)) {
						tools.Label ("When specified, GlassManager settings are loaded from the Glass Source.", true);
						tools.Label ("If 'None', GlassManager settings are chosen from the most popular and recently edited options from Glass objects in the scene.", true);
						tools.EndSection ();
					}
					tools.EndSection ();
					break;
				}
			case GlassManagerSettingsType.LoadFromManager:
				{
					tools.StartSection ();
					if (tools.ShowSection ("LoadFromManager?", ref manager.showSection_SettingsType)) {
						tools.Label ("GlassManager settings will not be affected by Glass objects in the scene.", true);
						tools.EndSection ();
					}
					tools.EndSection ();
					break;
				}
			}
			if (tools.EndChangeCheck ()) {
				manager.LoadManager ();
			}
			tools.FinishEdit (manager);
		}

		#endregion

		#region GUI Section - Glass

		void Section_Glass ()
		{
			tools.StartChangeCheck ();
			if (tools.ShowSection ("Glass (" + manager.glass.Count + ")", ref manager.showGlass)) {
				tools.GUI_List ("Glass in Scene", ref manager.glass, ref manager.showGlassList, ref manager.scrollGlassList);

				/*
				tools.Label("The following list can also define the Render Order, where enabled. Lower = Earlier.", true);
				tools.StartChangeCheck();
				tools.BoolOption("Custom Render Order", ref manager.customRenderOrder);
				if(tools.EndChangeCheck())
				{
					if(!manager.customRenderOrder)
					{
						manager.CustomRenderOrderDisabled();
					}
				}
				tools.StartChangeCheck();
				int movedObjectCurrentIndex = -1;
				int movedObjectPreviousIndex = -1;
				tools.GUI_ReorderableList("Objects in Scene", ref manager.glass, ref manager.showGlassList, ref manager.scrollGlassList,
				                          ref movedObjectCurrentIndex, ref movedObjectPreviousIndex,
				                          "First", "Last");
                if(tools.EndChangeCheck())
				{
					manager.UpdateGlassOrder_ByIndex(movedObjectCurrentIndex, movedObjectPreviousIndex);
				}
                */

				if (tools.Button ("Add from Scene (ignores duplicates)")) {
					manager.FindGlass ();
				}

				if (tools.ShowSection ("Synchronised Settings", ref manager.showSynchroniseSettings)) {
					tools.StartEdit (manager, "Changed Glass Sync Settings");

					tools.BoolOption ("Synchronise Glass", ref manager.synchroniseGlass);
					tools.Label ("The following options define how Glass objects are synchronised.\nWhen a Glass object is modified its settings are copied to any matches.", true);
					tools.BoolOption ("Match by Glass Name", ref manager.matchByGlassName);
					tools.BoolOption ("Match by Materials", ref manager.matchByMaterial);

					tools.FinishEdit (manager);

					tools.EndSection ();
				}
				tools.EndSection (true);
			}
			if (tools.EndChangeCheck ()) {
				manager.FindGlass ();
			}
		}

		#endregion

		#region GUI Section - Materials

		/// <summary>
		/// Shows all Glass materials in the scene.
		/// </summary>
		void Section_Materials ()
		{
			tools.StartChangeCheck ();
			if (tools.ShowSection ("Materials (" + manager.activeMaterials.Count + ")", ref manager.show_materials)) {
				tools.GUI_List ("Materials in Scene", ref manager.activeMaterials, ref manager.show_list_activeMaterials, ref manager.scrollActiveMaterialList);
				tools.EndSection ();
			}
			if (tools.EndChangeCheck ()) {
				manager.UpdateActiveMaterials ();
			}
		}

		#endregion

		#region GUI Section - Rendering

		void Section_Rendering ()
		{
			if (tools.ShowSection ("Rendering", ref manager.showRendering)) {
				//Section_RenderOrder();

				Section_Depth ();

				Section_Performance ();

				Section_Quality ();

				Section_Layers (ref manager.showLayers, ref manager.showLayersAdvanced);

				tools.EndSection ();
			}
		}

		#endregion

		#region GUI Section - Rendering - Render Order

		/*

        void Section_RenderOrder()
        {
            if (tools.ShowSection("Render Order", ref manager.showRenderOrder))
            {
				tools.StartChangeCheck();
                if (tools.BoolOption("Custom Render Order", ref manager.customRenderOrder))
                {
					if(tools.EndChangeCheck())
					{
						if(!manager.customRenderOrder)
						{
							manager.CustomRenderOrderDisabled();
						}
					}
                    tools.StartSection();
                    tools.Label("The following list defines the Render Order. Lower = Earlier.", true);
					tools.StartChangeCheck();
					int movedObjectCurrentIndex = -1;
					int movedObjectPreviousIndex = -1;
                    tools.GUI_ReorderableList("Objects in Scene", ref manager.glass, ref manager.showRenderOrderList, ref manager.scrollRenderOrderList,
					                          ref movedObjectCurrentIndex, ref movedObjectPreviousIndex,
					                          "First", "Last");
                    if(tools.EndChangeCheck())
					{
						manager.UpdateGlassOrder_ByIndex(movedObjectCurrentIndex, movedObjectPreviousIndex);
					}

					if (tools.Button("Add from Scene (ignores duplicates)"))
                    {
                        manager.FindGlass();
                    }

					if(tools.ShowSection("About", ref manager.showAboutRenderOrder))
					{
						tools.Label("Customising the Rendering Order helps overcome issues with transparent objects caused by automatic sorting.", true);
						tools.Space();
						tools.Space();
						tools.Label ("Examples:", true);
						tools.Space();
						tools.Label ("1: Jewels inside (early) a vase (late)", true);
						tools.Space();
						tools.Label ("2: Ocean (late) in which icebergs (early) are floating.", true);
						tools.Space();
						tools.Space();
						tools.Label ("It is also recommended that Depth Quality is set to at least Medium when scenes involve this complexity of intersection.", true);
						tools.EndSection();
					}
                    tools.EndSection();
                }
                tools.EndSection();
            }
        }

        */

		#endregion

		#region GUI Section - Rendering - Depth

		void Section_Depth ()
		{
			if (tools.ShowSection ("Depth", ref manager.showDepth)) {
				if (tools.ShowSection ("Cameras", ref manager.showCameras)) {
					tools.StartEdit (manager, "Changed Depth Camera Settings");

					tools.Label ("(Generated at runtime)");
					//	DEPTH TECHNIQUE 1.1
					//tools.GlassDepthCamOption("Front Face", ref manager.camFront);
					tools.GlassDepthCamOption ("Back Face", ref manager.camBack);
					tools.GlassDepthCamOption ("Other Objects / Background", ref manager.camOther);

					tools.FinishEdit (manager);

					tools.EndSection ();
				}
				Section_Layers (ref manager.showLayers_Depth, ref manager.showLayersAdvanced_Depth);
				if (tools.ShowSection ("Textures", ref manager.showDepthTextures)) {
					tools.StartEdit (manager, "Changed Depth Texture Settings");

					tools.StringOption ("Front Face", ref manager.depthTextureName_Front);
					tools.StringOption ("Back Face", ref manager.depthTextureName_Back);
					tools.StringOption ("Other Objects / Background", ref manager.depthTextureName_Other);

					tools.FinishEdit (manager);

					if (tools.Button ("Set Defaults")) {
						tools.StartEdit (manager, "Set Depth Texture Defaults");

						manager.depthTextureName_Front = "_DepthFront";
						manager.depthTextureName_Back = "_DepthBack";
						manager.depthTextureName_Other = "_DepthOther";

						tools.FinishEdit (manager);
					}
					tools.EndSection ();
				}
				if (tools.ShowSection ("Shaders", ref manager.showShaders)) {
					tools.StartEdit (manager, "Changed Depth Shaders");

					tools.ShaderOption ("Front Face", ref manager.depthShaderFront);
					tools.ShaderOption ("Back Face", ref manager.depthShaderBack);

					tools.FinishEdit (manager);

					if (tools.Button ("Set Defaults")) {
						tools.StartEdit (manager, "Set Shader Defaults");
						manager.LoadDefaultShaders ();
						tools.FinishEdit (manager);
					}
					tools.EndSection ();
				}

				if (tools.ShowSection ("Quality", ref manager.showDepthQuality)) {
					Section_Depth_Quality_Layer ("Front", ref manager.depthQuality_Front, GlassDepthLayer.front);
					Section_Depth_Quality_Layer ("Back", ref manager.depthQuality_Back, GlassDepthLayer.back);
					Section_Depth_Quality_Layer ("Other", ref manager.depthQuality_Other, GlassDepthLayer.other);
					tools.EndSection ();
				}

				if (tools.ShowSection ("Depth & Normal Techniques", ref manager.showDepthNormalTechniques)) {
					tools.StartEdit (manager, "Change Depth Technique");

					tools.StartChangeCheck ();
					manager.depthTechnique = (GlassDepthTechnique)tools.EnumOption ("Depth", manager.depthTechnique);
					if (tools.EndChangeCheck ()) {
						manager.managerSettingsType = GlassManagerSettingsType.LoadFromManager;
						manager.UpdateDepthTechnique ();
					}
					tools.FinishEdit (manager);

					tools.StartEdit (manager, "Change Normal Technique");
					tools.StartChangeCheck ();
					manager.normalTechnique = (GlassNormalTechnique)tools.EnumOption ("Normal", manager.normalTechnique);
					if (tools.EndChangeCheck ()) {
						manager.managerSettingsType = GlassManagerSettingsType.LoadFromManager;
						manager.UpdateNormalTechnique ();
					}
					tools.FinishEdit (manager);

#if UNITY_5_4_OR_NEWER
					tools.StartChangeCheck ();
					tools.BoolOption ("Unity 5.4 Fix (old cards)", ref manager.enable54Workaround);
					if (tools.EndChangeCheck ()) {
						manager.UpdateFrontDepthTechnique (true);
					}

					if(!manager.enable54Workaround)
					{
						tools.StartEdit(manager, "Change Front-Depth Technique");
						tools.StartChangeCheck();
						manager.frontDepthTechnique = (GlassFrontDepthTechnique)tools.EnumOption("Front Depth", manager.frontDepthTechnique);
						if (tools.EndChangeCheck())
						{
							manager.managerSettingsType = GlassManagerSettingsType.LoadFromManager;
							manager.UpdateFrontDepthTechnique(true);
						}
					}
#else
					tools.StartEdit(manager, "Change Front-Depth Technique");
					tools.StartChangeCheck();
					manager.frontDepthTechnique = (GlassFrontDepthTechnique)tools.EnumOption("Front Depth", manager.frontDepthTechnique);
					if (tools.EndChangeCheck())
					{
						manager.managerSettingsType = GlassManagerSettingsType.LoadFromManager;
						manager.UpdateFrontDepthTechnique(true);
					}
#endif

					if (tools.Button ("Apply to All Glass")) {
						if (Application.isPlaying) {
							Debug.Log ("Applying Depth and Normal techniques to all Glass objects' GlassManager settings in scene.");
							foreach (Glass glass in GameObject.FindObjectsOfType<Glass>()) {
								glass.CopyDepthNormalTech (manager);
							}
						} else if (tools.Message ("Apply Depth & Normal Tech to Glass", "This will overwrite the GlassManager settings in all Glass objects in this scene. Continue?", "OK", "Cancel")) {
							foreach (Glass glass in GameObject.FindObjectsOfType<Glass>()) {
								glass.CopyDepthNormalTech (manager);
							}
						}
					}

					tools.FinishEdit (manager);

					tools.EndSection ();
				}
				tools.EndSection (true);
			}
		}

		void Section_Depth_Quality_Layer (string layerName, ref DepthQuality_GlassManager layerQuality, GlassDepthLayer depthLayer)
		{
			tools.StartEdit (manager, "Changed Depth Quality: " + layerName);
			tools.StartChangeCheck ();
			layerQuality = (DepthQuality_GlassManager)tools.EnumOption (layerName, layerQuality);
			if (tools.EndChangeCheck ()) {
				switch (depthLayer) {
				case GlassDepthLayer.front:
					manager.DepthQualityChanged_Front ();
					break;
				case GlassDepthLayer.back:
					manager.DepthQualityChanged_Back ();
					break;
				case GlassDepthLayer.other:
					manager.DepthQualityChanged_Other ();
					break;
				}
			}
			tools.FinishEdit (manager);
		}

		#endregion

		#region GUI Section - Rendering - Performance

		void Section_Performance ()
		{
			if (tools.ShowSection ("Performance & Fidelity", ref manager.showPerformance)) {
				tools.StartEdit (manager, "Change Performance Settings");

				tools.BoldLabel ("Update Rates (low=fast, high=pretty):");

				tools.BoolOption ("Render Depths Seperately", ref manager.renderDepthsSeperately);

				if (manager.renderDepthsSeperately) {
					//	DEPTH TECHNIQUE 1.1
					tools.FloatOption ("Front Depth", ref manager.frontUpdateRate, GlassManager.min_updateRates, GlassManager.max_updateRates);
					tools.FloatOption ("Back Depth", ref manager.backUpdateRate, GlassManager.min_updateRates, GlassManager.max_updateRates);
					tools.FloatOption ("Other Depth", ref manager.otherUpdateRate, GlassManager.min_updateRates, GlassManager.max_updateRates);
				} else {
					tools.FloatOption ("All Depths", ref manager.depthUpdateRate, GlassManager.min_updateRates, GlassManager.max_updateRates);
				}
				//  NORMAL TECHNIQUE 1
				//tools.FloatOption("High Cam Data (direction)", ref manager.camHighUpdateRate, GlassManager.min_updateRates, GlassManager.max_updateRates);
				tools.FloatOption ("Depth Near/Far", ref manager.camLowUpdateRate, GlassManager.min_updateRates, GlassManager.max_updateRates);

				tools.BoldLabel ("Update Delays (high=fast, low=pretty):");
				tools.IntOption ("Depth Wait", ref manager.depthWait, 0, 50);

				EditorGUILayout.BeginHorizontal ();

				if (tools.Button ("Performance Preset")) {
					manager.frontUpdateRate = GlassManager.default_low_frontUpdateRate;
					manager.backUpdateRate = GlassManager.default_low_backUpdateRate;
					manager.otherUpdateRate = GlassManager.default_low_otherUpdateRate;
					manager.camHighUpdateRate = GlassManager.default_low_camHighUpdateRate;
					manager.camLowUpdateRate = GlassManager.default_low_camLowUpdateRate;
					manager.depthWait = GlassManager.default_low_depthWait;
					manager.renderDepthsSeperately = GlassManager.default_low_renderDepthSeperately;
				}

				if (tools.Button ("Balanced Preset")) {
					manager.frontUpdateRate = GlassManager.default_med_frontUpdateRate;
					manager.backUpdateRate = GlassManager.default_med_backUpdateRate;
					manager.otherUpdateRate = GlassManager.default_med_otherUpdateRate;
					manager.camHighUpdateRate = GlassManager.default_med_camHighUpdateRate;
					manager.camLowUpdateRate = GlassManager.default_med_camLowUpdateRate;
					manager.depthWait = GlassManager.default_med_depthWait;
					manager.renderDepthsSeperately = GlassManager.default_med_renderDepthSeperately;
				}

				if (tools.Button ("Quality Preset")) {
					manager.frontUpdateRate = GlassManager.default_frontUpdateRate;
					manager.backUpdateRate = GlassManager.default_backUpdateRate;
					manager.otherUpdateRate = GlassManager.default_otherUpdateRate;
					manager.camHighUpdateRate = GlassManager.default_camHighUpdateRate;
					manager.camLowUpdateRate = GlassManager.default_camLowUpdateRate;
					manager.depthWait = GlassManager.default_depthWait;
					manager.renderDepthsSeperately = GlassManager.default_renderDepthSeperately;
				}

				EditorGUILayout.EndHorizontal ();

				tools.FinishEdit (manager, true);

				tools.EndSection (true);
			}
		}

		#endregion

		#region GUI Section - Rendering - Quality

		void Section_Quality ()
		{
			if (tools.ShowSection ("Quality", ref manager.showQuality)) {
				tools.StartEdit (manager, "Changed Quality Settings");

				tools.BoldLabel ("Layer Quality");

				Section_Depth_Quality_Layer ("Front", ref manager.depthQuality_Front, GlassDepthLayer.front);
				Section_Depth_Quality_Layer ("Back", ref manager.depthQuality_Back, GlassDepthLayer.back);
				Section_Depth_Quality_Layer ("Other", ref manager.depthQuality_Other, GlassDepthLayer.other);

				tools.BoldLabel ("Textures Quality");

				tools.Popup ("Anti-Aliasing*", textureAALabels, textureAAItems, ref textureAAIndex, ref manager.depthTextureAA);
				tools.IntOption ("Aniso Level*", ref manager.depthTextureAniso, 0, 16);
				manager.depthTextureFilterMode = (FilterMode)tools.EnumOption ("Filter Mode*", manager.depthTextureFilterMode);

				tools.StartChangeCheck ();

				manager.depthTextureClearMode = (CameraClearFlags)tools.EnumOption ("Clear Mode", manager.depthTextureClearMode);

				if (tools.EndChangeCheck ()) {
					manager.UpdateCameraSettings ();
				}

				if (tools.Button ("Set Defaults")) {
					manager.depthTextureAA = GlassManager.default_depthTextureAA;
					UpdateTextureAAIndex ();

					manager.depthTextureAniso = GlassManager.default_depthTextureAniso;
					manager.depthTextureFilterMode = GlassManager.default_depthTextureFilterMode;
					manager.depthTextureClearMode = GlassManager.default_depthTextureClearMode;
					manager.UpdateCameraSettings ();

					manager.depthQuality_Front = GlassManager.default_depthQuality;
					manager.depthQuality_Back = GlassManager.default_depthQuality;
					manager.depthQuality_Other = GlassManager.default_depthQuality;
					manager.UpdateDepthTextureSelector_All ();
				}

				if (tools.Button ("Apply to All Glass")) {
					if (Application.isPlaying) {
						Debug.Log ("Applying Quality settings to all Glass objects' GlassManager Settings in the scene.");
						foreach (Glass glass in GameObject.FindObjectsOfType<Glass>()) {
							glass.CopyQuality (manager);
						}
					} else if (tools.Message ("Apply Quality to Glass", "This will overwrite the GlassManager settings in all Glass objects in this scene. Continue?", "OK", "Cancel")) {
						foreach (Glass glass in GameObject.FindObjectsOfType<Glass>()) {
							glass.CopyQuality (manager);
						}
					}
				}

				tools.Label ("*Does not change during Play.");
				tools.Label ("NB: Changes made during Play are not saved.");

				tools.FinishEdit (manager);

				tools.EndSection (true);
			}
		}

		#endregion

		#region GUI Section - Rendering - Layers

		void Section_Layers (ref bool showLayersBool, ref bool showLayersAdvancedBool)
		{
			tools.StartChangeCheck ();
			if (tools.ShowSection ("Layers", ref showLayersBool)) {
				tools.StartEdit (manager, "Changed Layer Settings");

				tools.LayerOption ("Front Face Layer", ref manager.frontLayerMask);
				tools.LayerOption ("Back Face Layer", ref manager.backLayerMask);
				tools.LayerOption ("Other Objects / Background", ref manager.otherLayerMask);

				tools.BoolOption ("Disable Layer Warnings", ref manager.disableLayerWarnings);

				if (tools.ShowSection ("Advanced", ref showLayersAdvancedBool)) {
					tools.Label ("Default Names:");
					tools.Label ("Front", manager.frontLayerName);
					tools.Label ("Back", manager.backLayerName);
					tools.Label ("Other", "(Everything not Front or Back)");
					tools.Label ("Current Layer Names:");

					tools.StartChangeCheck ();

					tools.GUI_List ("Front", ref manager.frontLayerNames, ref manager.show_list_frontLayers, ref manager.scroll_frontLayers);
					tools.GUI_List ("Back", ref manager.backLayerNames, ref manager.show_list_backLayers, ref manager.scroll_backLayers);
					tools.GUI_List ("Other", ref manager.otherLayerNames, ref manager.show_list_otherLayers, ref manager.scroll_otherLayers);

					if (tools.EndChangeCheck ()) {
						manager.UpdateLayerMasks ();
					}

					tools.EndSection ();
				}
				tools.EndSection ();

				tools.FinishEdit (manager);
			}
			if (tools.EndChangeCheck ()) {
				if (!manager.LayersExist ()) {
					Show_LayersWarning ();
				}
			}
		}

		#endregion

		#region GUI Section - Debugging

		void Section_Debugging ()
		{
			if (tools.ShowSection ("Debug", ref manager.showSection_Debug)) {
				tools.StartEdit (manager, "Changed Debug Settings");

				if (tools.BoolOption ("Enable Debugging", ref manager.showDebugInfo)) {
					tools.StartSection ();
					tools.Label ("Debug information will be printed to the console.");
					tools.enableDebugLogging = manager.showDebugInfo;
					tools.EndSection ();
				}

				tools.FinishEdit (manager);

				tools.EndSection ();
			}
		}

		#endregion

		#region GUI Section - Info

		void Section_Info ()
		{
			if (tools.ShowSection ("Info", ref manager.showAdvancedInfo)) {
				if (tools.ShowSection ("Names and Paths", ref manager.show_info_namesAndPaths)) {
					tools.StringOption ("Front Face Shader: ", ref manager.shaderFrontName, false);
					tools.StringOption ("Back Face Shader: ", ref manager.shaderBackName, false);
					tools.StringOption ("Manager Prefab: ", ref GlassManager.managerObjectPath, false);
					tools.EndSection ();
				}
				tools.EndSection ();
			}
		}

		#endregion

		#region GUI Section - GUI

		void Section_GUI ()
		{
			if (tools.ShowSection ("GUI", ref manager.showGUISettings)) {
				if (tools.BoolOption ("Custom GUI Skin", ref settings.useCustomGUI)) {
					tools.StartSection ();
					tools.StartChangeCheck ();
					settings.guiSkin = EditorGUILayout.ObjectField ("GUI Skin", settings.guiSkin, typeof(GUISkin), true) as GUISkin;
					if (tools.EndChangeCheck ()) {
						settings.guiSkinPath = AssetDatabase.GetAssetPath (settings.guiSkin);
					}
					tools.EndSection ();
				}
				tools.EndSection ();
			}
		}

		#endregion

		#region GUI Section - Camera

		void Section_Camera ()
		{
			if (tools.ShowSection ("Main Camera", ref manager.showMainCamera)) {
				if (manager.mainCamera == null)
					manager.FindMainCamera ();

				tools.StartEdit (manager, "Changed Main Camera Settings");

				tools.CameraOption ("", ref manager.mainCamera);

				if (tools.Button ("Set Optimum Camera Settings")) {
					Camera cam = manager.FindMainCamera ();
					cam.allowHDR = settings.optimumCamera_enableHDR;
					cam.renderingPath = settings.optimumCamera_renderingPath;
					tools.Message ("Optimum Camera Settings", "All options set!");
				}

				if (settings != null) {
					tools.BoolOption ("Always Optimise", ref settings.enableAlwaysSetOptimumCamera);
					if (tools.ShowSection ("Optimum Settings", ref settings.showSection_optimumCameraSettings)) {
						settings.optimumCamera_renderingPath = (RenderingPath)tools.EnumOption ("Rendering Path", settings.optimumCamera_renderingPath);
						tools.BoolOption ("Enable HDR", ref settings.optimumCamera_enableHDR);
						if (!settings.IsDefault_OptimumCamera ()) {
							if (tools.Button ("Restore Default Optimum Settings")) {
								settings.SetDefault_OptimumCamera ();
							}
						}
						tools.EndSection ();
					}
				}

				tools.FinishEdit (manager);

				tools.EndSection ();
			}
		}

		#endregion

		#region Messages

		public static void Show_LayersWarning ()
		{
			if (!EditorTools.Message_static (title_layerWarning, message_layerWarning, okButton_layerWarning, cancelButton_layerWarning)) {
				GlassManager.Instance.disableLayerWarnings = true;
			}
		}

		#endregion
	}
}
#endif