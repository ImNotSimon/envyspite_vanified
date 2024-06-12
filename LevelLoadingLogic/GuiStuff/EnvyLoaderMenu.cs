using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

namespace DoomahLevelLoader
{
    public class EnvyLoaderMenu : MonoBehaviour
    {
        private static EnvyLoaderMenu instance;

        public GameObject ContentStuff;
        public Button MenuOpener;
        public GameObject LevelsMenu;
        public GameObject LevelsButton;
        public Button Goback;
        public GameObject FuckingPleaseWait;

        public static EnvyLoaderMenu Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<EnvyLoaderMenu>();

                    if (instance == null)
                    {
                        Debug.LogError("EnvyLoaderMenu instance not found in the scene.");
                    }
                }
                return instance;
            }
        }

        private void Start()
        {
            MenuOpener.onClick.AddListener(OpenLevelsMenu);
            Goback.onClick.AddListener(GoBackToMenu);
            EnvyLoaderMenu.CreateLevels();
        }

        public static void CreateLevels()
        {
            if (Instance == null) return;

            for (int i = 0; i < Loaderscene.loadedAssetBundles.Count; i++)
            {
                string bundlePath = Loaderscene.bundleFolderPaths[i];
                string infoFilePath = Path.Combine(bundlePath, "info.txt");

                if (!Directory.Exists(bundlePath) || !File.Exists(infoFilePath))
                {
                    Debug.LogWarning($"Skipping level at '{bundlePath}' because the directory or info.txt file does not exist.");
                    continue;
                }

                GameObject buttonGO = Instantiate(Instance.LevelsButton, Instance.ContentStuff.transform);
                Button button = buttonGO.GetComponent<Button>();
                int index = i;
                button.onClick.AddListener(() =>
                {
                    Loaderscene.currentAssetBundleIndex = index;
                    Loaderscene.ExtractSceneName();
                    Loaderscene.Loadscene();
                });

                LevelButtonScript levelButtonScript = buttonGO.GetComponent<LevelButtonScript>();

                Loaderscene.UpdateLevelPicture(levelButtonScript.LevelImageButtonThing, levelButtonScript.NoLevel, false, bundlePath);
                string Size = Loaderscene.GetAssetBundleSize(index);
                levelButtonScript.FileSize.text = Size;

                try
                {
                    string[] lines = File.ReadAllLines(infoFilePath);
                    if (lines.Length >= 2)
                    {
                        levelButtonScript.Author.text = lines[0] ?? "Failed to load Author name!";
                        levelButtonScript.LevelName.text = lines[1] ?? "Failed to load Level name!";
                    }
                    else
                    {
                        levelButtonScript.Author.text = "Failed to load Author name!";
                        levelButtonScript.LevelName.text = "Failed to load Level name!";
                    }
                }
                catch
                {
                    Debug.LogError($"Failed to read info.txt in bundle folder '{bundlePath}'");
                }
            }
        }

        public static void ClearContentStuffChildren()
        {
            if (Instance == null) return;

            foreach (Transform child in Instance.ContentStuff.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public static IEnumerator UpdateLevelListingCoroutine()
        {
            Instance.FuckingPleaseWait.SetActive(true);

            ClearContentStuffChildren();
            CreateLevels();

            yield return null; // Ensure the UI updates

            Instance.FuckingPleaseWait.SetActive(false);
        }

        public static void UpdateLevelListing()
        {
            if (Instance != null)
            {
                Instance.StartCoroutine(UpdateLevelListingCoroutine());
            }
        }

        private void OpenLevelsMenu()
        {
            LevelsMenu.SetActive(true);
            MenuOpener.gameObject.SetActive(false);
            MainMenuAgony.isAgonyOpen = true;
        }

        private void GoBackToMenu()
        {
            LevelsMenu.SetActive(false);
            MenuOpener.gameObject.SetActive(true);
            MainMenuAgony.isAgonyOpen = false;
        }
    }

    public class DropdownHandler : MonoBehaviour
    {
        public TMP_Dropdown dropdown;

        private const string selectedDifficultyKey = "difficulty";
        private int savedDifficulty = MonoSingleton<PrefsManager>.Instance.GetInt(selectedDifficultyKey, 2);

        private void OnEnable()
        {
            MonoSingleton<PrefsManager>.Instance.SetInt(selectedDifficultyKey, 2);
            dropdown.value = savedDifficulty;

            dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        public void OnDropdownValueChanged(int index)
        {
            MonoSingleton<PrefsManager>.Instance.SetInt(selectedDifficultyKey, index);
        }
    }
}
