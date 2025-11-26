using Core;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UUtils;

namespace UI
{
    public class EndLevelPanelUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI endLevelText;
        [SerializeField, Scene]
        private string mainMenuScene;
        [SerializeField]
        private Button firstButton;

        public void OpenEndLevelPanel(bool victory)
        {
            endLevelText.text = victory ? "Victory" : "Defeat";
            DelayHelper.DelayOneFrame(this, () =>
            {
                EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
            });
        }

        public void NextLevel()
        {
            var nextLevel = LevelController.GetSingleton().GetNextLevelName();
            DebugUtils.DebugLogMsg($"Loading next level {nextLevel}.", DebugUtils.DebugType.System);
            SceneManager.LoadScene(nextLevel);
        }

        public void RestartLevel()
        {
            DebugUtils.DebugLogMsg("Restarting level.", DebugUtils.DebugType.System);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void BackToMainMenu()
        {
            DebugUtils.DebugLogMsg($"Loading main menu scene {mainMenuScene}.", DebugUtils.DebugType.System);
            SceneManager.LoadScene(mainMenuScene);
        }
    }
}