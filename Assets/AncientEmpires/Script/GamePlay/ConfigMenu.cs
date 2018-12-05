using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using AncientEmpires.Util;


namespace AncientEmpires.GamePlay
{
	public class ConfigMenu : MonoBehaviour
	{
		[Serializable]
		public class MapPreview
		{
			public ScrollRect scrollRect;
			public RectTransform content;
		}
		[SerializeField] private MapPreview mapPreview;


		[Serializable]
		public class MapDescription
		{
			public ScrollRect scrollRect;
			public RectTransform content;
			public Dropdown mapSelector;
		}
		[SerializeField] private MapDescription mapDescription;

		private Config config;
		private static ConfigMenu instance;

		[SerializeField] private Button backButton, playButton;
		[SerializeField] private Dropdown selectMoney, selectTurnTime;


		[Serializable]
		public class ArmyConfig
		{
			[SerializeField] private GameObject gameObject;
			[SerializeField] private Image panel;


			public bool isOn
			{
				get { return _isOn; }

				private set
				{
					_isOn = value;
					panel.gameObject.SetActive(value);
					OnIsOnChanged(this, _isOn);
				}
			}
			private bool _isOn;
			[SerializeField] private Toggle isOnToggle;


			public string name { get; private set; }
			[SerializeField] private InputField inputName;


			public Army.Color color
			{
				get { return _color; }

				private set
				{
					panel.color = InfoUI.armyColors[value];
					var oldColor = _color;
					_color = value;
					OnColorChanged(this, oldColor, value);
				}
			}
			private Army.Color _color;
			[SerializeField] private Dropdown selectColor;
			private List<Army.Color> colorList;


			public Army.Type type { get; private set; }
			[SerializeField] private Dropdown selectType;
			private List<Army.Type> typeList;

			public Army.Group group { get; private set; }
			[SerializeField] private Dropdown selectGroup;
			private List<Army.Group> groupList;


			// ========================  ArmyConfig behaviours  ====================


			public void Awake()
			{
				isOnToggle.onValueChanged.AddListener((bool value) => { isOn = value; });
				inputName.onEndEdit.AddListener((string text) => { name = text; });
				selectColor.onValueChanged.AddListener((int value) => { color = colorList[value]; });
				selectType.onValueChanged.AddListener((int value) => { type = typeList[value]; });
				selectGroup.onValueChanged.AddListener((int value) => { group = groupList[value]; });
			}


			public static void OnMapChanged(Map map)
			{
				var configs = instance.armyConfigs;
				for (int i = 0; i < map.armyColors.Length; ++i)
				{
					var cfg = configs[i];
					cfg.gameObject.SetActive(true);
					cfg._isOn = true;
					cfg.panel.gameObject.SetActive(true);

					// color & name
					cfg._color = map.armyColors[i];
					cfg.panel.color = InfoUI.armyColors[cfg.color];
					cfg.colorList = new List<Army.Color>() { cfg.color };
					cfg.selectColor.options = new List<Dropdown.OptionData>() { new Dropdown.OptionData(cfg.color.ToString()) };
					cfg.name = cfg.color.ToString() + " PLAYER";
					cfg.inputName.text = cfg.name;

					// type
					cfg.type = Army.Type.LOCAL;
					cfg.typeList = new List<Army.Type>() { Army.Type.LOCAL, Army.Type.AI };
					cfg.selectType.options = new List<Dropdown.OptionData>()
					{
						new Dropdown.OptionData("HUMAN"),
						new Dropdown.OptionData("AI")
					};

					// group
					cfg.group = (Army.Group)i;
					cfg.groupList = new List<Army.Group>() { cfg.group };
					for (int j = 0; j < map.armyColors.Length; ++j)
						if (j != i) cfg.groupList.Add((Army.Group)j);
					cfg.selectGroup.options.Clear();
					foreach (var group in cfg.groupList)
						cfg.selectGroup.options.Add(new Dropdown.OptionData(group.ToString()));
				}

				for (int i = map.armyColors.Length; i < configs.Length; ++i)
				{
					var cfg = configs[i];
					cfg._isOn = false;
					cfg.gameObject.SetActive(false);
				}
			}

			// =====================  EVENTS  ===================================

			private static void OnIsOnChanged(ArmyConfig cfg, bool value)
			{
				if (value)
				{
					// Show army config

					// Check group
					List<Army.Group> currentList = null;
					foreach (var c in instance.armyConfigs)
						if (c != cfg && c.isOn) { currentList = c.groupList; break; }

					if (currentList == null) currentList = new List<Army.Group>();
					var mapGroups = new List<Army.Group>();
					int length = instance.config.map.armyColors.Length;
					for (int i = 0; i < length; ++i) mapGroups.Add((Army.Group)i);

					var freeGroups = new List<Army.Group>();
					foreach (var group in mapGroups)
						if (!currentList.Contains(group)) freeGroups.Add(group);

					var freeGroup = freeGroups[0];
					var opt = new Dropdown.OptionData(freeGroup.ToString());
					cfg.group = freeGroup;
					cfg.selectGroup.options = new List<Dropdown.OptionData>() { opt };
					foreach (var group in currentList)
						cfg.selectGroup.options.Add(new Dropdown.OptionData(group.ToString()));
					cfg.groupList = new List<Army.Group>() { freeGroup };
					cfg.groupList.AddRange(currentList);
					foreach (var c in instance.armyConfigs)
						if (c != cfg && c.isOn)
						{
							c.selectGroup.options.Add(opt);
							c.groupList.Add(freeGroup);
						}

					// Check color
					List<Army.Color> freeColors = null;
					foreach (var c in instance.armyConfigs)
						if (c != cfg && c.isOn)
						{
							freeColors = new List<Army.Color>(c.colorList);
							freeColors.Remove(c.color);
							break;
						}

					if (freeColors == null) freeColors = new List<Army.Color>(instance.config.map.armyColors);
					cfg._color = freeColors[0];
					cfg.panel.color = InfoUI.armyColors[cfg.color];
					opt = new Dropdown.OptionData(cfg.color.ToString());
					cfg.selectColor.options = new List<Dropdown.OptionData>() { opt };
					cfg.colorList = new List<Army.Color>() { cfg.color };
					foreach (var color in freeColors)
						if (color != cfg.color)
						{
							cfg.selectColor.options.Add(new Dropdown.OptionData(color.ToString()));
							cfg.colorList.Add(color);
						}

					foreach (var c in instance.armyConfigs)
						if (c != cfg && c.isOn)
						{
							int index = c.colorList.IndexOf(cfg.color);
							c.colorList.RemoveAt(index);
							c.selectColor.options.RemoveAt(index);
						}
				}
				else
				{
					// Hide army config

					// Check group
					var currentOtherGroups = new List<Army.Group>();
					foreach (var c in instance.armyConfigs)
						if (c.isOn) currentOtherGroups.Add(c.group);

					var freeGroups = new List<Army.Group>();
					foreach (var group in cfg.groupList)
						if (!currentOtherGroups.Contains(group)) freeGroups.Add(group);

					var freeGroup = freeGroups[0];
					foreach (var c in instance.armyConfigs)
						if (c.isOn)
						{
							c.selectGroup.options.RemoveAt(c.groupList.IndexOf(freeGroup));
							c.groupList.Remove(freeGroup);
						}

					// Check color
					var opt = new Dropdown.OptionData(cfg.color.ToString());
					foreach (var c in instance.armyConfigs)
						if (c.isOn)
						{
							c.selectColor.options.Add(opt);
							c.colorList.Add(cfg.color);
						}
				}
			}


			private static void OnColorChanged(ArmyConfig cfg, Army.Color oldColor, Army.Color newColor)
			{
				var opt = new Dropdown.OptionData(oldColor.ToString());
				foreach (var c in instance.armyConfigs)
					if (c != cfg && c.isOn)
					{
						int index = c.colorList.IndexOf(newColor);
						c.colorList[index] = oldColor;
						c.selectColor.options[index] = opt;
					}
			}
		}
		[SerializeField] private ArmyConfig[] armyConfigs;


		// =====================  ConfigMenu behaviours  =========================


		private void Awake()
		{
			if (!instance) instance = this;
			else if (this != instance) { Destroy(gameObject); return; }

			// register listeners
			foreach (var cfg in armyConfigs) cfg.Awake();
			mapDescription.mapSelector.onValueChanged.AddListener(OnMapChanged);
			selectMoney.onValueChanged.AddListener((int value) =>
			{
				config.money = Convert.ToInt32(selectMoney.options[value].text);
			});

			selectTurnTime.onValueChanged.AddListener((int value) =>
			{
				config.turnTime = Convert.ToSingle(selectTurnTime.options[value].text);
			});

			playButton.onClick.AddListener(OnPlayClick);

			config = Config.instance;
			config.money = Convert.ToInt32(selectMoney.options[0].text);
			config.turnTime = Convert.ToSingle(selectTurnTime.options[0].text);
			var mapList = new List<Dropdown.OptionData>(R.asset.maps.defaultMaps.Length);
			foreach (var map in R.asset.maps.defaultMaps) mapList.Add(new Dropdown.OptionData(map.name));
			mapDescription.mapSelector.options = mapList;
			OnMapChanged(0);
		}


		private void OnMapChanged(int value)
		{
			config.map = R.asset.maps.defaultMaps[value];

			// Show map to map preview and map description

			ArmyConfig.OnMapChanged(config.map);
		}


		private void OnPlayClick()
		{
			var onCfgs = new List<ArmyConfig>();
			foreach (var cfg in armyConfigs) if (cfg.isOn) onCfgs.Add(cfg);
			if (onCfgs.Count < 2)
			{
				Debug.LogWarning("At least 2 armies and 2 different groups to play !");
				return;
			}

			var group = onCfgs[0].group;
			foreach (var cfg in onCfgs)
				if (group != cfg.group)
				{
					// Play
					config.playModeConfig = Config.instance.playModeConfig;
					config.aiConfig = Config.instance.aiConfig;
					config.armyInfos = new Config.ArmyInfo[onCfgs.Count];
					for (int i = 0; i < onCfgs.Count; ++i)
					{
						var c = onCfgs[i];
						config.armyInfos[i] = new Config.ArmyInfo()
						{
							color = c.color,
							group = c.group,
							name = c.name
						};
					}

					// Tạo AI Config
					var aiColors = new List<Army.Color>();
					foreach (var c in onCfgs) if (c.type == Army.Type.AI) aiColors.Add(c.color);
					if (aiColors.Count > 0) config.aiConfig = new AI.AIConfig() { armyColors = aiColors.ToArray() };

					Config.instance = Config.DeSerialize(Config.Serialize(config));
					DontDestroyOnLoad(Config.instance.map);
					SceneManager.LoadScene(R.SceneBuildIndex.BATTLE);
					return;
				}

			Debug.LogWarning("At least 2 armies and 2 different groups to play !");
		}


		private void OnBackClick()
		{

		}
	}
}
