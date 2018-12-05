using UnityEditor;


[CustomPropertyDrawer(typeof(ArmyColor_Sprite))]
[CustomPropertyDrawer(typeof(ArmyColor_Anim))]
[CustomPropertyDrawer(typeof(UnitName_Unit))]
[CustomPropertyDrawer(typeof(Int_String))]
[CustomPropertyDrawer(typeof(PoolKey_GameObject))]
[CustomPropertyDrawer(typeof(ActionName_GameObject))]
[CustomPropertyDrawer(typeof(UnitName_Button))]
[CustomPropertyDrawer(typeof(UnitName_ArmyColor_Sprite))]
public class MyPropertyDrawer : SerializableDictionaryPropertyDrawer { }
