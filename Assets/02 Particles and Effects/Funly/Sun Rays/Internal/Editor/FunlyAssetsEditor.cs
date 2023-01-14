using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Funly.SunRays
{
  [CustomEditor(typeof(FunlyAssets))]
  public class FunlyAssetsEditor : Editor
  {
    
    private class Product {
      public string name;
      public string url;

      public Product(string name, string url) {
        this.name = name;
        this.url = url;
      } 
    }

    private List<Product> m_Products;

    public override void OnInspectorGUI()
    {
      if (m_Products == null) {
        BuildProductsList();
      }

      //EditorGUILayout.HelpBox("Please checkout our other great Unity Assets.", MessageType.None);

      EditorGUILayout.LabelField(new GUIContent("Please checkout our other great Unity Assets."));
      
      foreach (Product prod in m_Products) {
        RenderProduct(prod);
      }
    }

    private void BuildProductsList() {
      m_Products = new List<Product>();
      m_Products.Add(new Product("Sky Studio", "http://bit.ly/2XE21bY"));
      m_Products.Add(new Product("Dynamic Starry Sky", "http://bit.ly/2XE1Jlo"));
    }
    
    private void RenderProduct(Product prod) {
      EditorGUILayout.BeginHorizontal();

      bool didClick = GUILayout.Button(new GUIContent(prod.name));
      if (didClick) {
        Application.OpenURL(prod.url);
      }

      EditorGUILayout.EndHorizontal();
    }
  }
}
