using UnityEngine;
using System;
using UnityEngine.UI;
using AncientEmpires.GamePlay.Skirmishes;
using UnityEngine.SceneManagement;
using AncientEmpires.GamePlay.Online;
using AncientEmpires.GamePlay.Campaigns;


namespace AncientEmpires.GamePlay
{
	public class MainMenu : MonoBehaviour
	{
		[SerializeField]
		private Button newGameButton, loadGameButton, editorButton,
			settingButton, helpButton, aboutButton, backButton;


		private void OnEnable()
		{
			newGameButton.onClick.AddListener(OnNewGameClick);
			loadGameButton.onClick.AddListener(OnLoadGameClick);
			editorButton.onClick.AddListener(OnEditorClick);
			settingButton.onClick.AddListener(OnSettingClick);
			helpButton.onClick.AddListener(OnHelpClick);
			aboutButton.onClick.AddListener(OnAboutClick);
			backButton.onClick.AddListener(OnBackClick);

			newGameMenu.OnEnable();
		}


		private void OnDisable()
		{
			newGameButton.onClick.RemoveListener(OnNewGameClick);
			loadGameButton.onClick.RemoveListener(OnLoadGameClick);
			editorButton.onClick.RemoveListener(OnEditorClick);
			settingButton.onClick.RemoveListener(OnSettingClick);
			helpButton.onClick.RemoveListener(OnHelpClick);
			aboutButton.onClick.RemoveListener(OnAboutClick);
			backButton.onClick.RemoveListener(OnBackClick);

			newGameMenu.OnDisable();
		}


		private void OnLoadGameClick()
		{
			throw new NotImplementedException();
		}


		private void OnEditorClick()
		{
			throw new NotImplementedException();
		}


		private void OnSettingClick()
		{
			throw new NotImplementedException();
		}


		private void OnHelpClick()
		{
			throw new NotImplementedException();
		}


		private void OnAboutClick()
		{
			throw new NotImplementedException();
		}


		private void OnBackClick()
		{
			Application.Quit();
		}


		// =========================================================================


		[Serializable]
		private class NewGameMenu
		{
			public RectTransform rectTransform;
			[SerializeField] private Button campaignButton, skirmishButton, onlineButton, LANButton, backButton;


			public void OnEnable()
			{
				campaignButton.onClick.AddListener(OnCampaignClick);
				skirmishButton.onClick.AddListener(OnSkirmishClick);
				onlineButton.onClick.AddListener(OnOnlineClick);
				LANButton.onClick.AddListener(OnLANClick);
				backButton.onClick.AddListener(OnBackClick);
			}


			public void OnDisable()
			{
				campaignButton.onClick.RemoveListener(OnCampaignClick);
				skirmishButton.onClick.RemoveListener(OnSkirmishClick);
				onlineButton.onClick.RemoveListener(OnOnlineClick);
				LANButton.onClick.RemoveListener(OnLANClick);
				backButton.onClick.RemoveListener(OnBackClick);
			}


			private void OnCampaignClick()
			{
				// tạo và chỉnh sửa CampaignConfig
				// CampaignConfig.instance= cfg
				throw new NotImplementedException();
			}


			private void OnSkirmishClick()
			{
				Config.instance = new Config()
				{
					playMode = Config.PlayMode.SKIRMISH,
					playModeConfig = new SkirmishConfig()
				};

				SceneManager.LoadScene(R.SceneBuildIndex.SKIRMISH_CONFIG_MENU);
			}


			private void OnOnlineClick()
			{
				Config.instance = new Config()
				{
					playMode = Config.PlayMode.ONLINE,
					playModeConfig = new OnlineConfig()
				};

				SceneManager.LoadScene(R.SceneBuildIndex.ONLINE_CONFIG_MENU);
			}


			private void OnLANClick()
			{
				throw new NotImplementedException();
			}


			private void OnBackClick()
			{
				rectTransform.gameObject.SetActive(false);
			}
		}
		[SerializeField] private NewGameMenu newGameMenu;


		private void OnNewGameClick()
		{
			newGameMenu.rectTransform.gameObject.SetActive(true);
		}
	}
}
