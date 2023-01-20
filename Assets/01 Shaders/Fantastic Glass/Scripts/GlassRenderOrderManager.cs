using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace FantasticGlass
{
    [Serializable]
    public class GlassRenderOrderManager : MonoBehaviour
    {
        public List<GlassType> glassTypes = new List<GlassType>();

        #region SINGLETON

        public static GlassRenderOrderManager _instance = null;

        public static GlassRenderOrderManager Instance
        {
            get
            {
                if (_instance)
                {
                    return _instance;
                }
                _instance = FindObjectOfType<GlassRenderOrderManager>();
                if (_instance != null)
                {
                    return _instance;
                }
                if (_instance != null)
                {
                    return _instance;
                }
                _instance = new GameObject("Glass Render Order Manager").AddComponent<GlassRenderOrderManager>();
                _instance.transform.parent = GlassManager.Instance.transform;
                _instance.Initialise();
                return _instance;
            }
        }

        #endregion

        public void Start()
        {
            //FindGlassTypes();
        }

        public void Initialise()
        {
            FindGlassTypes();
        }

        public void FindGlassTypes()
        {
            foreach (Glass glass in GameObject.FindObjectsOfType<Glass>())
            {
                AddGlass(glass);
            }
            foreach (MeshRenderer renderer in GameObject.FindObjectsOfType<Renderer>())
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (FindType(material) == null)
                    {
                        if (!MaterialInUseByGlass(material))
                        {
                            AddGlassType(material);
                        }
                    }
                }
            }
        }

        private bool MaterialInUseByGlass(Material material)
        {
            foreach (Glass glass in GameObject.FindObjectsOfType<Glass>())
            {
                if (glass.materials.Contains(material))
                    return true;
            }
            return false;
        }

        private GlassType FindType(Material material)
        {
            foreach (GlassType glassType in glassTypes)
            {
                if (glassType.HasMaterial(material))
                {
                    return glassType;
                }
            }
            return null;
        }

        private GlassType FindType(Glass instance)
        {
            foreach (GlassType glassType in glassTypes)
            {
                if (glassType.HasInstance(instance))
                {
                    return glassType;
                }
            }
            return null;
        }

        public bool IsGlass(Material material)
        {
            foreach (GlassType glassType in glassTypes)
            {
                if (glassType.isGlass)
                {
                    if (glassType.HasMaterial(material))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void RefreshGlassTypes()
        {
            FindGlassTypes();
            RemoveNullypes();
            RemoveNonMatches();
            RemoveEmptyTypes();
            SortByRenderOrder();
            UpdateGlassTypes();
        }

        public void RemoveNullypes()
        {
            while (glassTypes.Contains(null))
            {
                glassTypes.Remove(null);
            }
            for (int i = glassTypes.Count - 1; i >= 0; i--)
            {
                GlassType glassType = glassTypes[i];
                if (glassType.isCopy)
                {
                    if (glassType.original == null)
                    {
                        RemoveCopy(glassType);
                    }
                    else
                    {
                        if (!glassType.original.IsCopy(glassType))
                        {
                            RemoveCopy(glassType);
                        }
                    }
                }
            }
            foreach (GlassType glassType in glassTypes)
            {
                glassType.RemoveNullObjects();
            }
        }

        public void RemoveNonMatches()
        {
            while (glassTypes.Contains(null))
                glassTypes.Remove(null);
            //
            foreach (GlassType glassType in glassTypes)
            {
                glassType.RemoveNonMatches();
            }
        }

        public void UpdateGlassTypes()
        {
            for (int i = 0; i < glassTypes.Count; i++)
            {
                GlassType glassType = glassTypes[i];
                glassType.UpdateGlassValues();
                if (glassType.previousIndex == -1)
                {
                    glassType.previousIndex = i;
                    glassType.previousItemAbove = GetGlassTypeBefore(glassType);
                    glassType.previousItemBelow = GetGlassTypeAfter(glassType);
                }
            }
        }

        public void RemoveEmptyTypes()
        {
            for (int i = glassTypes.Count - 1; i >= 0; i--)
            {
                GlassType glassType = glassTypes[i];
                if (!glassType.isCopy && glassType.isGlass)
                {
                    if (glassType.instances.Count == 0)
                    {
                        glassTypes.Remove(glassType);
                        DestroyGlassType(glassType);
                    }
                }
                else if (glassType.isGlass)
                {
                    if (glassType.original == null)
                    {
                        glassTypes.Remove(glassType);
                        DestroyGlassType(glassType);
                    }
                }
            }
        }

        public void AddGlass(Glass glass)
        {
            foreach (GlassType glassType in glassTypes)
            {
                if (glassType.Matches(glass))
                {
                    glassType.AddInstance(glass);
                    return;
                }
            }
            AddGlassType(glass);
        }

        public void AddGlassType(Glass glass)
        {
            GlassType glassType = GlassType.Create(glass, this);//new GlassType(glass);
            glassType.previousIndex = glassTypes.Count;
            if (glassTypes.Count > 0)
            {
                glassType.previousItemAbove = glassTypes[glassTypes.Count - 1];
                glassTypes[glassTypes.Count - 1].previousItemBelow = glassType;
            }
            glassType.AddInstance(glass);
            glassTypes.Add(glassType);
        }

        public void AddGlassType(Material nonGlassMaterial)
        {
            GlassType glassType = GlassType.Create(nonGlassMaterial, this); //new GlassType(nonGlassMaterial);
            glassType.previousIndex = glassTypes.Count;
            if (glassTypes.Count > 0)
            {
                glassType.previousItemAbove = glassTypes[glassTypes.Count - 1];
                glassTypes[glassTypes.Count - 1].previousItemBelow = glassType;
            }
            glassTypes.Add(glassType);
        }

        public GlassType GetGlassType(int index)
        {
            if (index < 0)
                return null;
            if (index >= glassTypes.Count)
                return null;
            return glassTypes[index];
        }

        public void RenderOrderFromOrder()
        {
            /*
            for(int i=0; i < glassTypes.Count; i++)
            {
                glassTypes[i].SetRenderOrderFromOrder(i, GetGlassType(i-1), GetGlassType(i+1));
            }
            */
            GlassType movedType = null;
            int moveDirection = 0;

            CalculateMoveData(ref movedType, ref moveDirection);

            if (movedType == null)
            {
                FinishedMoveChecks();
                return;
            }

            int indexOfMovedType = glassTypes.IndexOf(movedType);

            //  Testing for simple swap with neighbour (not altering numbers)
            if (moveDirection == 1) //  moving up (earlier)
            {
                if (!SwapRenderOrder(indexOfMovedType, indexOfMovedType + 1))
                {
                    movedType.RenderEarlier();
                }
                FinishedMoveChecks();
                return;
            }
            else if (moveDirection == -1) //  moving down (later)
            {
                if (!SwapRenderOrder(indexOfMovedType - 1, indexOfMovedType))
                {
                    movedType.RenderLater();
                }
                FinishedMoveChecks();
                return;
            }

            FinishedMoveChecks();

            GlassType typeBefore = GetGlassType(movedType.previousIndex - 1);
            GlassType typeAfter = GetGlassType(movedType.previousIndex + 1);

            List<GlassType> itemsBefore = GetGlassTypesBefore(typeBefore);
            List<GlassType> itemsAfter = GetGlassTypesAfter(typeAfter);

            if (moveDirection > 0)//  moving up
            {
                if (typeAfter != null)
                {
                    //  move before below item
                    if (MakeSureRenderOrderBefore(ref movedType, typeAfter))
                    {
                        //  adjust above item(s)
                        if (MakeSureRenderOrderBefore(ref typeBefore, movedType))
                        {
                            RenderEarlier(itemsBefore);
                        }
                        /*
                        if (MakeSureRenderOrderAfter(ref typeAfter, movedType))
                        {
                            RenderLater(itemsAfter);
                        }
                        */
                    }
                }
                else if (typeBefore != null)
                {
                    //  move after above item
                    if (MakeSureRenderOrderAfter(ref movedType, typeBefore))
                    {
                        //  adjust below item(s)
                        if (MakeSureRenderOrderAfter(ref typeAfter, movedType))
                        {
                            RenderLater(itemsAfter);
                        }
                        /*
                        if (MakeSureRenderOrderBefore(ref typeBefore, movedType))
                        {
                            RenderEarlier(itemsBefore);
                        }
                        */
                    }
                }
                else
                {
                    //  nothing to copy from
                }
            }
            else if (moveDirection < 0) //  moving down
            {
                if (typeBefore != null)
                {
                    //  move after above item
                    if (MakeSureRenderOrderAfter(ref movedType, typeBefore))
                    {
                        //  adjust below item(s)
                        if (MakeSureRenderOrderAfter(ref typeAfter, movedType))
                        {
                            RenderEarlier(itemsBefore);
                        }
                        /*
                        if (MakeSureRenderOrderBefore(ref typeBefore, movedType))
                        {
                            RenderLater(itemsAfter);
                        }
                        */
                    }
                }
                else if (typeAfter != null)
                {
                    //  move before above item
                    if (MakeSureRenderOrderBefore(ref movedType, typeAfter))
                    {
                        //  adjust above item(s)
                        if (MakeSureRenderOrderBefore(ref typeBefore, movedType))
                        {
                            RenderLater(itemsAfter);
                        }
                        /*
                        if (MakeSureRenderOrderAfter(ref typeAfter, movedType))
                        {
                            RenderEarlier(itemsBefore);
                        }
                        */
                    }
                }
                else
                {
                    //  nothing to copy from
                }
            }
            //
            //SortByRenderOrder();
        }

        public void AddCopy(GlassType original)
        {
            GlassType copy = original.CreateCopy(this);
            AddCopy(copy, original);
        }

        public void AddCopy(GlassType copy, GlassType original)
        {
            glassTypes.Insert(glassTypes.IndexOf(original) + 1, copy);
            copy.previousIndex = glassTypes.IndexOf(copy);
            copy.previousItemAbove = original;
            copy.previousItemBelow = GetGlassTypeAfter(copy);
        }

        public void RemoveCopy(GlassType copy)
        {
            if (!copy.isCopy)
                return;
            GlassType original = copy.original;
            if (original != null)
            {
                original.RemoveCopy(copy);
            }
            glassTypes.Remove(copy);
            DestroyGlassType(copy);
        }

        public static void DestroyGlassType(GlassType glassType)
        {
            if (glassType == null)
                return;
#if UNITY_EDITOR
            DestroyImmediate(glassType);
#else
            Destroy(glassType);
#endif
        }

        bool SwapRenderOrder(int makeEarliest, int makeLatest)
        {
            GlassType earliestType = glassTypes[makeEarliest];
            GlassType latestType = glassTypes[makeLatest];
            int renderOrder1 = earliestType.renderOrder;
            int renderOrder2 = latestType.renderOrder;
            earliestType.renderOrder = Mathf.Min(renderOrder1, renderOrder2);
            latestType.renderOrder = Mathf.Max(renderOrder1, renderOrder2);
            return earliestType.renderOrder != latestType.renderOrder;
        }

        void FinishedMoveChecks()
        {
            for (int i = 0; i < glassTypes.Count; i++)
            {
                GlassType glassType = glassTypes[i];
                glassType.FinishedMoveChecks(i, GetGlassType(i - 1), GetGlassType(i + 1));
            }
        }

        private void RenderEarlier(List<GlassType> items)
        {
            foreach (GlassType glassType in items)
            {
                glassType.RenderEarlier();
            }
        }

        private void RenderLater(List<GlassType> items)
        {
            foreach (GlassType glassType in items)
            {
                glassType.RenderLater();
            }
        }

        public List<GlassType> GetGlassTypesBefore(GlassType movedType)
        {
            if (movedType != null)
                return GetGlassTypesBefore(movedType.previousIndex);
            else
                return new List<GlassType>();
        }

        public List<GlassType> GetGlassTypesBefore(int index)
        {
            List<GlassType> types = new List<GlassType>();
            if (index > 0)
            {
                for (int i = 0; i < index; i++)
                {
                    types.Add(glassTypes[i]);
                }
            }
            return types;
        }

        public List<GlassType> GetGlassTypesAfter(GlassType movedType)
        {
            if (movedType != null)
                return GetGlassTypesAfter(movedType.previousIndex);
            else
                return new List<GlassType>();
        }

        public List<GlassType> GetGlassTypesAfter(int index)
        {
            List<GlassType> types = new List<GlassType>();
            if (index < glassTypes.Count - 1)
            {
                for (int i = index + 1; i < glassTypes.Count; i++)
                {
                    types.Add(glassTypes[i]);
                }
            }
            return types;
        }

        public GlassType GetGlassTypeBefore(GlassType glassType)
        {
            int index = glassTypes.IndexOf(glassType);
            index--;
            if (index >= 0)
            {
                return glassTypes[index];
            }
            else
            {
                return null;
            }
        }

        public GlassType GetGlassTypeAfter(GlassType glassType)
        {
            int index = glassTypes.IndexOf(glassType);
            index++;
            if (index < glassTypes.Count)
            {
                return glassTypes[index];
            }
            else
            {
                return null;
            }
        }

        bool MakeSureRenderOrderBefore(ref GlassType typeToMove, GlassType pivot)
        {
            if (pivot == null)
                return false;
            if (typeToMove == null)
                return false;
            return typeToMove.MakeSureRendeOrderBefore(pivot.renderOrder);
        }

        bool MakeSureRenderOrderAfter(ref GlassType typeToMove, GlassType pivot)
        {
            if (pivot == null)
                return false;
            if (typeToMove == null)
                return false;
            return typeToMove.MakeSureRendeOrderAfter(pivot.renderOrder);
        }

        void CalculateMoveData(ref GlassType movedType, ref int moveDirection)
        {
            int moveAmount = 0;
            for (int i = 0; i < glassTypes.Count; i++)
            {
                GlassType glassType = glassTypes[i];
                moveAmount = glassType.MoveAmount(i, GetGlassType(i - 1), GetGlassType(i + 1));
                if (moveAmount != 0)
                {
                    movedType = glassType;
                    moveDirection = moveAmount;
                }
            }
        }

        public void SortByRenderOrder()
        {
            List<GlassType> glassTypesOrdered = new List<GlassType>();
            glassTypesOrdered.AddRange(glassTypes);
            glassTypesOrdered = glassTypesOrdered.OrderBy(o => o.renderOrder).ToList();
            glassTypes.Clear();
            glassTypes.AddRange(glassTypesOrdered);
            FinishedMoveChecks();
        }

        /*
        void OnDestroy()
        {
            for (int i = glassTypes.Count - 1; i >= 0; i--)
            {
                DestroyGlassType(glassTypes[i]);
            }
        }
        */

        public void UpdateMaterials()
        {
            try
            {
                RemoveNonMatches();
            }
            catch (Exception e)
            {
                Debug.LogError("An error occurred trying to RemoveNonMatches. Please try again later. Exception:" + e.Message);
            }

            try
            {
                RemoveEmptyTypes();
            }
            catch (Exception e)
            {
                Debug.LogError("An error occurred trying to RemoveNonMatches. Please try again later. Exception:" + e.Message);
            }

            try
            {
                foreach (GlassType glassType in glassTypes)
                {
                    if (glassType.isGlass && !glassType.isCopy)
                    {
                        glassType.UpdateMaterialFromInstance();
                        List<Material> updatedMaterials = new List<Material>();
                        List<GlassType> allCopiesSorted = new List<GlassType>();
                        allCopiesSorted.Add(glassType);
                        allCopiesSorted.AddRange(glassType.copies);
                        allCopiesSorted = allCopiesSorted.OrderBy(o => o.renderOrder).ToList();
                        foreach (GlassType glassTypeCopy in allCopiesSorted)
                        {
                            glassTypeCopy.AddMaterials(ref updatedMaterials);
                        }
                        foreach (Glass glassInstance in glassType.instances)
                        {
                            glassInstance.SetMaterials(updatedMaterials);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("An error occurred trying to RemoveNonMatches. Please try again later. Exception:" + e.Message);
            }
        }

        public void DepthQualityChanged_Front(Glass glass)
        {
            GlassType glassType = FindType(glass);
            if (glassType != null)
            {
                glassType.DepthQualityChanged_Front(glass);
            }
        }

        public void DepthQualityChanged_Back(Glass glass)
        {
            GlassType glassType = FindType(glass);
            if (glassType != null)
            {
                glassType.DepthQualityChanged_Back(glass);
            }
        }

        public void DepthQualityChanged_Other(Glass glass)
        {
            GlassType glassType = FindType(glass);
            if (glassType != null)
            {
                glassType.DepthQualityChanged_Other(glass);
            }
        }
    }
}