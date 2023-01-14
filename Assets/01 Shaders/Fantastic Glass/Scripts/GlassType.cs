using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace FantasticGlass
{
    [Serializable]
    public class GlassType : MonoBehaviour
    {
        public string presetName;
        public Material material_front;
        public Material material_back;
        public Material nonGlassMaterial;
        public bool renderFront = true;
        public bool renderBack = true;
        public MeshRenderer theRenderer;
        public GlassManager manager;
        public List<Glass> instances = new List<Glass>();
        public int renderOrder = Glass.default_customRenderOrder;
        public int originalRenderQueue;
        public int previousIndex = -1;
        public GlassType previousItemAbove = null;
        public GlassType previousItemBelow = null;
        public bool isCopy = false;
        //[NonSerialized]
        public List<GlassType> copies = new List<GlassType>();
        public GlassType original = null;
        public bool isGlass = true;
        public static HideFlags glassTypeHideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

        public void Start()
        {

        }

        public static GlassType Create(Glass instance, GlassRenderOrderManager manager)
        {
            GlassType glassType = CreateGlassTypeObject(instance.presetName, manager.gameObject);
            glassType.hideFlags = glassTypeHideFlags;
            glassType.Setup(instance);
            return glassType;
        }

        public static GlassType Create(GlassType original, GlassRenderOrderManager manager)
        {
            GlassType glassType = CreateGlassTypeObject(CopyName(original), manager.gameObject);
            glassType.hideFlags = glassTypeHideFlags;
            glassType.Setup(original);
            return glassType;
        }

        public GlassType CreateCopy(GlassRenderOrderManager _manager)
        {
            return GlassType.CreateCopy(this, _manager);
        }

        public static GlassType CreateCopy(GlassType original, GlassRenderOrderManager _manager)
        {
            GlassType copy = CreateGlassTypeObject(CopyName(original), _manager.gameObject);

            copy.Setup(original);

            copy.isCopy = true;
            copy.original = original;
            original.copies.Add(copy);

            copy.CopyMaterials(original);

            return copy;
        }

        public static GlassType Create(Material _nonGlassMaterial, GlassRenderOrderManager manager)
        {
            GlassType glassType = CreateGlassTypeObject(_nonGlassMaterial.name, manager.gameObject);
            glassType.hideFlags = glassTypeHideFlags;
            glassType.Setup(_nonGlassMaterial);
            glassType.isGlass = false;
            return glassType;
        }

        public static string CopyName(GlassType original)
        {
            return original.presetName + " (" + (original.copies.Count + 1).ToString() + ")";
        }

        public static GlassType CreateGlassTypeObject(string name, GameObject _parent)
        {
            GlassType glassType = null;
            foreach (GlassType gt in _parent.GetComponents<GlassType>())
            {
                if (gt.presetName == name)
                {
                    glassType = gt;
                }
            }
            if (glassType == null)
            {
                glassType = _parent.AddComponent<GlassType>();
            }
            /*
            foreach (GlassType gt in GameObject.FindObjectsOfType<GlassType>())
            {
                if (gt.presetName == name)
                {
                    glassType = gt;
                }
            }
            if (glassType == null)
            {
                GameObject go = new GameObject(name);
                go.transform.parent = _parent.transform;
                glassType = go.AddComponent<GlassType>();
            }
            */
            return glassType;
        }

        /*
        public GlassType(Material _nonGlassMaterial)
        {
            Setup(_nonGlassMaterial);
        }

        public GlassType(Glass instance)
        {
            Setup(instance);
        }

        public GlassType(GlassType glassType)
        {
            Setup(glassType);
        }
        */

        public void Setup(GlassType glassType)
        {
            presetName = glassType.presetName;
            material_front = glassType.material_front;
            material_back = glassType.material_back;
            manager = glassType.manager;
            renderOrder = glassType.renderOrder;
            hideFlags = glassTypeHideFlags;
        }

        public void Setup(Glass instance)
        {
            presetName = instance.presetName;
            material_front = instance.material_front;
            material_back = instance.material_back;
            manager = instance.FindGlassManager();
            //renderOrder = instance.customRenderOrder;
            hideFlags = glassTypeHideFlags;
            //SetRenderOrder(instance.customRenderOrder);
        }

        public void Setup(Material _nonGlassMaterial)
        {
            nonGlassMaterial = _nonGlassMaterial;
            presetName = nonGlassMaterial.name;
            originalRenderQueue = nonGlassMaterial.renderQueue;
            renderOrder = originalRenderQueue;
            hideFlags = glassTypeHideFlags;
            isGlass = false;
        }

        private void CopyMaterials(GlassType original)
        {
            material_back = new Material(Shader.Find(original.material_back.shader.name));
            material_back.CopyPropertiesFromMaterial(original.material_back);
            material_back.renderQueue = material_back.renderQueue;
            //
            material_front = new Material(Shader.Find(original.material_front.shader.name));
            material_front.CopyPropertiesFromMaterial(original.material_front);
            material_front.renderQueue = material_front.renderQueue;
        }

        /*
        public void EnableFrontMaterial(bool enabled = true)
        {
            renderFront = enabled;
            foreach (Glass instance in instances)
            {
                instance.EnableMaterial(GlassFace.front, CopyNumber(), enabled);
            }
        }

        public void EnableBackMaterial(bool enabled = true)
        {
            renderFront = enabled;
            foreach (Glass instance in instances)
            {
                instance.EnableMaterial(GlassFace.back, CopyNumber(), enabled);
            }
        }

        public void UpdateFaceMaterials()
        {
            foreach (Glass instance in instances)
            {
                instance.EnableMaterial(GlassFace.front, CopyNumber(), renderFront);
                instance.EnableMaterial(GlassFace.back, CopyNumber(), renderBack);
            }
        }
        */

        public bool HasMaterial(Material material)
        {
            if (material_front == material)
                return true;
            if (material_back == material)
                return true;
            if (nonGlassMaterial == material)
                return true;
            return false;
        }

        public bool HasInstance(Glass instance)
        {
            if (instances != null)
                if (instances.Contains(instance))
                    return true;
            return false;
        }

        public void RevertRenderQueue()
        {
            renderOrder = originalRenderQueue;
            nonGlassMaterial.renderQueue = originalRenderQueue;
        }

        public bool IsCopy(GlassType potentialCopy)
        {
            if (potentialCopy == this)
                return false;
            if (potentialCopy == null)
                return false;
            return copies.Contains(potentialCopy);
        }

        public bool IsOriginal(GlassType potentialOriginal)
        {
            if (potentialOriginal == null)
                return false;
            if (original == null)
                return false;
            return (original == potentialOriginal);
        }

        public bool CanRevert()
        {
            if (isGlass)
                return false;
            return renderOrder != originalRenderQueue;
        }

        public bool IsCopyOrOriginal(GlassType glassType)
        {
            if (IsOriginal(glassType))
                return true;
            else return IsCopy(glassType);
        }

        public int CopyNumber()
        {
            if (!isCopy)
                return 1;
            else if (original != null)
                return original.CopyNumber(this);
            else
                return -1;
        }

        public int CopyNumber(GlassType glassType)
        {
            if (!IsCopy(glassType))
                return -1;
            return copies.IndexOf(glassType) + 2;
        }

        public bool HasCopies()
        {
            if (copies == null)
            {
                FindInstances();
            }
            if (isCopy)
                return true;
            return copies.Count > 0;
        }

        public void RemoveCopy(GlassType copy)
        {
            copies.Remove(copy);
        }

        public void RemoveNonMatches()
        {
            for (int i = instances.Count - 1; i >= 0; i--)
            {
                Glass instance = instances[i];
                if (!Matches(instance))
                {
                    instances.Remove(instance);
                }
            }
        }

        public void UpdateGlassValues()
        {
            RemoveNullObjects();
            if (instances.Count > 0)
            {
                Glass instance = instances[0];
                UpdateGlassValues(instance);
            }
        }

        void UpdateGlassValues(Glass instance)
        {
            instance.presetName = presetName;
            manager = instance.FindGlassManager();
            SetRenderOrder(renderOrder);
        }

        public void FindInstances()
        {
            if (copies == null)
                copies = new List<GlassType>();
            //
            RemoveNullObjects();
            //
            foreach (Glass glass in GameObject.FindObjectsOfType<Glass>())
            {
                if (glass.presetName == presetName)
                {
                    AddInstance(glass);
                }
            }
        }

        public void RemoveNullObjects()
        {
            while (instances.Contains(null))
                instances.Remove(null);
            while (copies.Contains(null))
                copies.Remove(null);
        }

        public void AddInstance(Glass instance)
        {
            if (!instances.Contains(instance))
            {
                instances.Add(instance);
            }
        }

        public void SetRenderOrderFromOrder(int index, GlassType itemAbove, GlassType itemBelow)
        {
            bool canChangeOrder = true;
            bool valueAfterAbove = false;
            bool valueBeforeBelow = false;
            //
            if (previousIndex == index)
            {
                canChangeOrder = false;
            }
            //
            if (itemAbove == previousItemAbove && itemBelow == previousItemBelow)
            {
                canChangeOrder = false;
            }
            //
            else if (itemAbove != previousItemAbove)
            {
                canChangeOrder = false;
                valueAfterAbove = true;
            }
            //
            else if (itemBelow != previousItemBelow)
            {
                canChangeOrder = false;
                valueBeforeBelow = true;
            }

            //
            previousIndex = index;
            previousItemAbove = itemAbove;
            previousItemBelow = itemBelow;
            //
            if (canChangeOrder)
            {
                SetRenderOrder(Glass.default_customRenderOrder + index);
            }
            else if (valueAfterAbove)
            {
                if (previousItemAbove != null)
                    SetRenderOrder(previousItemAbove.renderOrder + 1);
            }
            else if (valueBeforeBelow)
            {
                if (previousItemBelow != null)
                    SetRenderOrder(previousItemBelow.renderOrder - 1);
            }
        }

        /// <summary>
        /// MoveAmount based on new index, item above (less than), and item below (more than)
        /// </summary>
        /// <param name="i"></param>
        /// <param name="glassTypeAbove"></param>
        /// <param name="glassTypeBelow"></param>
        /// <returns></returns>
        public int MoveAmount(int index, GlassType itemAbove, GlassType itemBelow)
        {
            bool moved = true;

            int direction = 0;

            if (index == previousIndex)
            {
                moved = false;
            }
            else if (itemAbove == previousItemAbove || itemBelow == previousItemBelow)
            {
                moved = false;
            }

            if (moved)
            {
                direction = previousIndex - index;
            }

            return direction;
        }

        public void FinishedMoveChecks(int index, GlassType itemAbove, GlassType itemBelow)
        {
            previousIndex = index;
            previousItemAbove = itemAbove;
            previousItemBelow = itemBelow;
        }

        public bool MakeSureRendeOrderBefore(int otherRenderOrder)
        {
            if (renderOrder >= otherRenderOrder)
            {
                SetRenderOrder(otherRenderOrder - 1);
                return true;
            }
            return false;
        }

        public bool MakeSureRendeOrderAfter(int otherRenderOrder)
        {
            if (renderOrder <= otherRenderOrder)
            {
                SetRenderOrder(otherRenderOrder + 1);
                return true;
            }
            return false;
        }

        public void RenderEarlier()
        {
            SetRenderOrder(renderOrder - 1);
        }

        public void RenderLater()
        {
            SetRenderOrder(renderOrder + 1);
        }

        public void SetRenderOrder(int _renderOrder)
        {
            if (isGlass)
            {
                int newRenderOrder = _renderOrder;
                if (newRenderOrder < Glass.default_customRenderOrder)
                {
                    newRenderOrder = Glass.default_customRenderOrder;
                }
                renderOrder = newRenderOrder;
                /*
                foreach (Glass instance in instances)
                {
                    instance.customRenderOrder = newRenderOrder;
                    instance.useCustomRenderOrder = true;
                    manager.UpdateGlassOrder_ByCustomRenderOrder(instance);
                }
                */
            }
            else
            {
                renderOrder = _renderOrder;
                if (nonGlassMaterial != null)
                {
                    nonGlassMaterial.renderQueue = renderOrder;
                }
            }
        }

        public bool Matches(Glass glass)
        {
            if (manager == null)
                manager = GlassManager.Instance;
            if (manager.matchByGlassName)
                if (glass.presetName == presetName)
                    return true;
            if (manager.matchByMaterial)
                if (glass.material_front == material_front)
                    return true;
            return false;
        }


        public bool MatchingRenderOrder(GlassType other)
        {
            if (other == null)
                return false;
            return renderOrder == other.renderOrder;
        }

        public void AddMaterials(ref List<Material> updatedMaterials)
        {
            if (!isGlass)
                return;

            if (isCopy)
            {
                material_back.CopyPropertiesFromMaterial(original.instances[0].material_back);
                material_front.CopyPropertiesFromMaterial(original.instances[0].material_front);
            }
            else
            {
                material_back = instances[0].material_back;
                material_front = instances[0].material_front;
            }

            material_back.renderQueue = renderOrder;
            material_front.renderQueue = renderOrder;

            if (renderBack)
                updatedMaterials.Add(material_back);
            if (renderFront)
                updatedMaterials.Add(material_front);
        }

        /*
        public Material MyMaterialBack(Material originalMaterialBack)
        {
            Material myMaterialBack = new Material(Shader.Find(originalMaterialBack.shader.name));
            myMaterialBack.CopyPropertiesFromMaterial(originalMaterialBack);
            myMaterialBack.renderQueue = renderOrder;
            return myMaterialBack;
        }

        public Material MyMaterialFront(Material originalMaterialFront)
        {
            Material myMaterialFront = new Material(Shader.Find(originalMaterialFront.shader.name));
            myMaterialFront.CopyPropertiesFromMaterial(originalMaterialFront);
            myMaterialFront.renderQueue = renderOrder;
            return myMaterialFront;
        }
        */

        public void SortCopies()
        {
            copies = copies.OrderBy(o => o.renderOrder).ToList();
        }

        public void UpdateMaterialFromInstance()
        {
            while (instances.Contains(null))
                instances.Remove(null);
            if (instances.Count < 1)
                return;
            material_back = instances[0].material_back;
            material_front = instances[0].material_front;
            foreach (GlassType copy in copies)
            {
                copy.UpdateBackMaterial(material_back);
                copy.UpdateFrontMaterial(material_front);
            }
        }

        private void UpdateBackMaterial(Material original_material_back)
        {
            if (!isCopy)
                return;
            material_back.CopyPropertiesFromMaterial(original_material_back);
        }

        private void UpdateFrontMaterial(Material original_material_front)
        {
            if (!isCopy)
                return;
            material_front.CopyPropertiesFromMaterial(original_material_front);
        }

        //  Depth Quality

        public void DepthQualityChanged_Front(Glass glass)
        {
            foreach (Glass instance in instances)
            {
                instance.depthQuality_front = glass.depthQuality_front;
            }
        }

        public void DepthQualityChanged_Back(Glass glass)
        {
            foreach (Glass instance in instances)
            {
                instance.depthQuality_back = glass.depthQuality_back;
            }
        }

        public void DepthQualityChanged_Other(Glass glass)
        {
            foreach (Glass instance in instances)
            {
                instance.depthQuality_other = glass.depthQuality_other;
            }
        }
    }
}
