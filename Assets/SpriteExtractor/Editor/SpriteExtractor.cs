using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace NavTools
{
    public class SpriteExtractor : EditorWindow
    {
        /// <summary>
        /// Created with <3 by Navarone. v1.0;
        /// </summary>

        [SerializeField]
        private List<SpriteInfo> SpriteInfoList = new();

        #region UIElements
        private ObjectField ojField;
        private Button buttonFolderPicker;
        private Button saveAllBtn;
        private Button cancelButton;
        private ListView bottomPane;
        private Label statusLabel;
        private Label pathLabel;

        #endregion

        #region MemberVars
        private bool isBusy;
        private string statusText;
        private string defaultPath = "";
        private string path;

        private const float timerLength = 1.6f;
        private float timer = 100f;
        private double nextTime = 0;

        private CancellationToken cancelToken;
        private CancellationTokenSource tokenSource;
        #endregion

        [MenuItem("Tools/NavTools/SpriteExtractor")]
        public static void ShowSpriteExtractor()
        {
            SpriteExtractor window =
                GetWindow<SpriteExtractor>();

            window.titleContent = new GUIContent("SpriteExtractor");
        }

        private void OnEditorUpdate()
        {
            double timeToSave = nextTime - EditorApplication.timeSinceStartup;


            //Debug.Log(timeToSave.ToString("N1") + " secs");

            if (EditorApplication.timeSinceStartup > nextTime)
            {
                ChangeStatusText("");
                EditorApplication.update -= OnEditorUpdate;

            }
        }

        private void OnDisable()
        {
            //event cleanup? do we need this?
            if (saveAllBtn != null)
            {
                saveAllBtn.clicked -= SaveAllButtonClicked;
            }
            if (buttonFolderPicker != null)
            {
                buttonFolderPicker.clicked -= BrowseFolderButtonClicked;

            }
        }

        public void CreateGUI()
        {
            SpriteInfoList.Clear();
            VisualElement root = rootVisualElement;

            path = Path.Combine(Application.persistentDataPath, "sprites");
            defaultPath = path;
            pathLabel = new($"<b>{path}</b>");

            CreateTextureField(root);

            root.Add(bottomPane);


            buttonFolderPicker = new()
            {
                name = "pickfile",
                text = "Browse..",
            };
            buttonFolderPicker.SetEnabled(false);


            saveAllBtn = new()
            {
                name = "saveAllbtn",
                text = "Save All",
            };

            saveAllBtn.style.marginTop = 30;
            saveAllBtn.SetEnabled(false);

            cancelButton = new()
            {
                name = "cancelItButton",
                text = "Cancel..",
            };
            cancelButton.visible = false;

            statusLabel = new();
            var savePathLabel = new Label("Save Destination");
            statusLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            root.Add(savePathLabel);
            root.Add(pathLabel);
            root.Add(buttonFolderPicker);

            root.Add(saveAllBtn);
            root.Add(cancelButton);
            root.Add(statusLabel);

            saveAllBtn.clicked += SaveAllButtonClicked;
            buttonFolderPicker.clicked += BrowseFolderButtonClicked;
        }

        private void CreateTextureField(VisualElement root)
        {
            ojField = new()
            {
                name = "A",
                label = "SpriteSheet",
                allowSceneObjects = false,
                objectType = typeof(Texture2D),

            };
            ojField.style.paddingTop = 16;
            ojField.Bind(new SerializedObject(this));
            ojField.RegisterValueChangedCallback(TextureObjectChanged);
            root.Add(ojField);
            bottomPane = CreateListView(SpriteInfoList);
        }
        private ListView CreateListView(List<SpriteInfo> spriteItems)
        {
            var items = spriteItems;

            Func<VisualElement> makeItem = () =>
            {
                var root = new VisualElement();
                var splitViw = new TwoPaneSplitView(0, 64, TwoPaneSplitViewOrientation.Horizontal);

                var image = new Image();
                image.style.height = 24;
                image.style.width = 24;

                splitViw.Add(image);

                var righSide = new VisualElement();

                var label = new Label();
                righSide.Add(label);
                splitViw.Add(righSide);

                root.Add(splitViw);
                return root;
            };

            // Bind the data
            Action<VisualElement, int> bindItem = (e, i) =>
            {
                var label = e.Q<Label>();
                var img = e.Q<Image>();
                label.text = items[i].SpriteName;
                img.sprite = items[i].TheSprite;
            };


            var listView = new ListView(items, 32, makeItem, bindItem)
            {
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                showBorder = true,
                selectionType = SelectionType.Multiple
            };
            listView.style.flexGrow = 1.0f;
            listView.style.maxHeight = 200;
            return listView;
        }

        private void TextureObjectChanged(ChangeEvent<Object> evt)
        {
            SpriteInfoList.Clear();

            if (evt.newValue == null)
            {
                saveAllBtn.SetEnabled(false);
                buttonFolderPicker.SetEnabled(false);
                bottomPane.RefreshItems();
                return;
            }

            buttonFolderPicker.SetEnabled(true);
            saveAllBtn.SetEnabled(true);

            var assetPath = AssetDatabase.GetAssetPath(evt.newValue);

            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter == null)
            {
                Debug.LogError("textureImporter is null");
                return;
            }

            if (!textureImporter.isReadable)
            {
                Debug.LogWarning("Texture is not readable, setting as readble..Try again");
                EditorUtility.DisplayDialog("Texture is not readable", "attempting to set as readable..Try again.", "Ok");
                textureImporter.isReadable = true;
                textureImporter.SaveAndReimport();
                ojField.value = null;

                return;
            }

            Object[] data = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            if (data != null)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i].GetType() == typeof(Sprite))
                    {
                        Sprite sprite = data[i] as Sprite;
                        SpriteInfoList.Add(new(sprite, sprite.name));
                    }
                }

                bottomPane.RefreshItems();
            }
            else
            {
                EditorUtility.DisplayDialog("Sorry", "Sorry this Texture does not contain any Data", "Ok");
                Debug.LogError("Sorry this Texture does not contain any Data");
            }
        }

        private void ChangeStatusText(string text)
        {
            statusText = text;
            statusLabel.text = statusText;
        }

        private void BrowseFolderButtonClicked()
        {
            string newPath = EditorUtility.OpenFolderPanel("Select Folder", null, null);

            if (!string.IsNullOrEmpty(newPath))
            {
                path = newPath;
            }

            pathLabel.text = $"<b>{path}</b>";
        }
        private void SaveAllButtonClicked()
        {
            if (isBusy) return;
            SaveImages();
        }
        private void CancelButtonClicked()
        {
            Debug.Log("Cancel clicked");
            tokenSource.Cancel();
            cancelButton.clicked -= CancelButtonClicked;
        }

        private async void SaveImages()
        {
            tokenSource = new CancellationTokenSource();
            cancelToken = tokenSource.Token;

            var tasks = new ConcurrentBag<Task>();

            cancelButton.visible = true;
            cancelButton.clicked += CancelButtonClicked;
            saveAllBtn.SetEnabled(false);
            ChangeStatusText($"<b><i>..Saving..</i></b>");

            for (int i = 0; i < SpriteInfoList.Count; i++)
            {
                Sprite sprite = SpriteInfoList[i].TheSprite;
                var texName = sprite.name;
                var extracted = ExtractTexture(sprite);
                var texData = extracted.EncodeToPNG();
                var task = Task.Run(() => SaveExtractedToFile(texName, texData, path), cancelToken);
                tasks.Add(task);

            }

            var statusText = "";
            try
            {
                await Task.WhenAll(tasks);

                statusText = $"<b><i>..Complete!..</i></b>";
            }
            catch (TaskCanceledException ex)
            {
                Debug.Log(ex);
                statusText = $"<b><i>..Canceled!..</i></b>";
            }
            finally
            {
                saveAllBtn.SetEnabled(true);
                cancelButton.visible = false;

                //for label
                ChangeStatusText(statusText);
                timer = timerLength;
                EditorApplication.update += OnEditorUpdate;
                nextTime = EditorApplication.timeSinceStartup + timer;
            }
        }
        private async Task SaveExtractedToFile(string texName, byte[] texData, string saveToDirectory)
        {
            isBusy = true;
            try
            {
                Directory.CreateDirectory(saveToDirectory);
                if (cancelToken.IsCancellationRequested)
                {
                    isBusy = false;
                    return;
                }
                await File.WriteAllBytesAsync(Path.Combine(saveToDirectory, texName + ".png"), texData);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
            }
            finally
            {
                isBusy = false;
            }

        }

        private Texture2D ExtractTexture(Sprite sprite)
        {
            int spriteWidth = (int)sprite.rect.width;
            int spriteHeight = (int)sprite.rect.height;
            var output = new Texture2D(spriteWidth, spriteHeight);

            var textureRect = sprite.rect;

            var pixels = sprite.texture.GetPixels32();

            Color32[] spritePixels = new Color32[spriteWidth * spriteHeight];

            for (int y = 0; y < spriteHeight; y++)
            {
                for (int x = 0; x < spriteWidth; x++)
                {
                    int pixelIndex = (int)((y + textureRect.y) * sprite.texture.width + (x + textureRect.x));
                    int spritePixelIndex = y * spriteWidth + x;
                    spritePixels[spritePixelIndex] = pixels[pixelIndex];
                }
            }

            output.filterMode = FilterMode.Point;
            output.SetPixels32(spritePixels);
            output.Apply();

            output.name = sprite.texture.name + " " + sprite.name;

            return output;
        }
    }

    public class SpriteInfo : IComparable<SpriteInfo>
    {
        public Sprite TheSprite;
        public string SpriteName;
        public bool IsGettingSaved;
        public SpriteInfo(Sprite sprite, string name, bool isGettingSaved = true)
        {
            TheSprite = sprite;
            SpriteName = name;
            IsGettingSaved = isGettingSaved;
        }

        public int CompareTo(SpriteInfo other)
        {
            return string.Compare(SpriteName, other.SpriteName, StringComparison.Ordinal);
        }
    }
}
