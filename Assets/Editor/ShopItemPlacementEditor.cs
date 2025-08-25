#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShopItemPlacement))]
public class ShopItemPlacementEditor : Editor
{
    SerializedProperty itemsProp;
    SerializedProperty weaponDatasProp;

    void OnEnable()
    {
        itemsProp = serializedObject.FindProperty("items");
        weaponDatasProp = serializedObject.FindProperty("weaponDatas");
    }

    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 먼저
        DrawDefaultInspector();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Shop Auto-Fill Utilities", EditorStyles.boldLabel);

        if (GUILayout.Button("Fill Items from Resources/Item"))
        {
            FillItemsFromResources("Item");
        }

        if (GUILayout.Button("Fill Weapons from Resources/Weapon"))
        {
            FillWeaponsFromResources("Weapon");
        }
    }

    private void FillItemsFromResources(string resourcesSubFolder)
    {
        var targetComp = (ShopItemPlacement)target;

        // Resources/item 에서 Item 전부 로드
        Item[] found = Resources.LoadAll<Item>(resourcesSubFolder);

        if (found == null || found.Length == 0)
        {
            EditorUtility.DisplayDialog("Fill Items", 
                $"Resources/{resourcesSubFolder} 에서 Item 에셋을 찾지 못했어요.", "확인");
            return;
        }

        // 중복 제거 + 정렬(이름 기준)
        List<Item> unique = found
            .Where(x => x != null)
            .Distinct()                 // 같은 참조 중복 제거
            .OrderBy(x => x.name)       // 보기 좋게 이름 정렬
            .ToList();

        Undo.RecordObject(targetComp, "Fill Items from Resources");
        targetComp.items = unique;

        EditorUtility.SetDirty(targetComp);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Fill Items", 
            $"Items 채움 완료! (총 {unique.Count}개)", "좋아요");
    }

    // (옵션) 무기 채우기
    private void FillWeaponsFromResources(string resourcesSubFolder)
    {
        var targetComp = (ShopItemPlacement)target;

        WeaponData[] found = Resources.LoadAll<WeaponData>(resourcesSubFolder);
        if (found == null || found.Length == 0)
        {
            EditorUtility.DisplayDialog("Fill Weapons", 
                $"Resources/{resourcesSubFolder} 에서 WeaponData 에셋을 찾지 못했어요.", "확인");
            return;
        }

        List<WeaponData> unique = found
            .Where(x => x != null)
            .Distinct()
            .OrderBy(x => x.name)
            .ToList();

        Undo.RecordObject(targetComp, "Fill Weapons from Resources");
        targetComp.weaponDatas = unique;

        EditorUtility.SetDirty(targetComp);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Fill Weapons", 
            $"Weapons 채움 완료! (총 {unique.Count}개)", "좋아요");
    }
}
#endif