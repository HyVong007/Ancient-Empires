using UnityEngine;
using System;
using AncientEmpires;
using AncientEmpires.Units;
using AncientEmpires.Util;
using UnityEngine.UI;


[Serializable]
public class ArmyColor_Sprite : SerializableDictionary<Army.Color, Sprite> { }

[Serializable]
public class ArmyColor_Anim : SerializableDictionary<Army.Color, RuntimeAnimatorController> { }

[Serializable]
public class UnitName_Unit : SerializableDictionary<Unit.Name, Unit> { }

[Serializable]
public class Int_String : SerializableDictionary<int, string> { }

[Serializable]
public class PoolKey_GameObject : SerializableDictionary<ObjectPool.POOL_KEY, GameObject> { }

[Serializable]
public class ActionName_GameObject : SerializableDictionary<AncientEmpires.Action.Name, GameObject> { }

[Serializable]
public class UnitName_Button : SerializableDictionary<Unit.Name, Button> { }

[Serializable]
public class UnitName_ArmyColor_Sprite : SerializableDictionary<Unit.Name, ArmyColor_Sprite> { }
