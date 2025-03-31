using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blanketmen.Hypnos.UI
{
    public delegate void UiAoHandler(UiBase ui);

    public class UiManager : MonoBehaviour
    {
        #region Singleton
        public static UiManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this);
        }

        private void OnDestroy()
        {
            if (this == Instance)
            {
                Instance = null;
            }
        }
        #endregion

        public const int CloseOperation = 0;
        public const int OpenOperation = 1;
        public const int AllCanvasRootsId = -1;

        public static int VisibleUiLayer { get; private set; }
        public static int InvisibleUiLayer { get; private set; }
        public static Rect CanvasSafeArea { get; private set; }

        [Header("General Settings")]
        [SerializeField] private int visibleUiLayer = 5;
        [SerializeField] private int invisibleUiLayer = 6;
        [SerializeField] private float closedUiSurvivalTime = 30f;

        [Header("Camera Settings")]
        [SerializeField] private Camera canvasCamera;

        [Header("Canvas Roots Settings")]
        [SerializeField] private UiCanvasRootBase[] canvasRoots;

        private readonly Dictionary<Type, BindingResourceAttribute> uiAttrMap = new Dictionary<Type, BindingResourceAttribute>(107);
        private readonly Dictionary<Type, UiBase> loadedUiMap = new Dictionary<Type, UiBase>(107);

        private UiBase closedUiHeadNode;
        private UiBase closedUiTailNode;

        private void Start()
        {
            VisibleUiLayer = visibleUiLayer;
            InvisibleUiLayer = invisibleUiLayer;
            CalculateSafeArea();
        }

        private void LateUpdate()
        {
            UiBase prevNode = null;
            UiBase currNode = closedUiHeadNode;
            while (currNode != null)
            {
                if (currNode.elapsedTimeAfterClosed < 0f)
                {
                    RemoveClosedUi(prevNode, currNode);

                    UiBase nextNode = currNode.nextNode;
                    currNode.nextNode = null;
                    currNode = nextNode;
                    continue;
                }

                if (currNode.elapsedTimeAfterClosed > closedUiSurvivalTime)
                {
                    RemoveClosedUi(prevNode, currNode);
                    Type uiType = currNode.GetType();
                    loadedUiMap.Remove(uiType);
                    currNode.OnRuin();
                    ResourceManager.Instance.Destroy(currNode.gameObject);
                    ResourceManager.Instance.UnloadAsset(GetBindingResourceAttribute(uiType).AssetId);

                    UiBase nextNode = currNode.nextNode;
                    currNode.nextNode = null;
                    currNode = nextNode;
                    continue;
                }

                currNode.elapsedTimeAfterClosed += GameTimeManager.UnscaledDeltaTime;
                prevNode = currNode;
                currNode = currNode.nextNode;
            }
        }

        public void AddClosedUi(UiBase ui)
        {
            if (closedUiHeadNode == null)
            {
                closedUiHeadNode = ui;
                closedUiTailNode = ui;
            }
            else
            {
                closedUiTailNode.nextNode = ui;
                closedUiTailNode = ui;
            }
        }

        private void RemoveClosedUi(UiBase prevNode, UiBase currNode)
        {
            // Remove node.
            if (currNode == closedUiHeadNode)
            {
                closedUiHeadNode = currNode.nextNode; // NOTE: NextNode can't be self.
            }
            else
            {
                prevNode.nextNode = currNode.nextNode;
            }

            // Set last node.
            if (currNode == closedUiTailNode)
            {
                closedUiTailNode = prevNode;
            }
        }

        public UiBase GetUi(Type uiType)
        {
            loadedUiMap.TryGetValue(uiType, out UiBase ui);
            return ui;
        }

        private BindingResourceAttribute GetBindingResourceAttribute(Type uiType)
        {
            uiAttrMap.TryGetValue(uiType, out BindingResourceAttribute attr);
            if (attr == null)
            {
                attr = Attribute.GetCustomAttribute(uiType, typeof(BindingResourceAttribute)) as BindingResourceAttribute;
                uiAttrMap[uiType] = attr;
            }
            return attr;
        }

        public void CreateUiAsync(Type uiType, Transform parent, UiAoHandler loadedCb)
        {
            loadedUiMap.TryGetValue(uiType, out UiBase ui);
            if (ui != null)
            {
                loadedCb(ui);
                return;
            }

            BindingResourceAttribute attr = GetBindingResourceAttribute(uiType);
            if (attr == null)
            {
                Logging.Error($"The type does not have BindingResourceAttribute. Type: {uiType}", nameof(UiManager));
                loadedCb(null);
                return;
            }

            void onUiLoaded(GameObject go)
            {
                UiBase loadedUi = go.GetComponent<UiBase>();
                loadedUi = ResourceManager.Instance.Create(loadedUi, parent);
                loadedUi.OnCreate();
                loadedUiMap[uiType] = loadedUi;
                loadedCb(loadedUi);
            };

            ResourceManager.Instance.LoadAssetAsync<GameObject>(attr.AssetId, onUiLoaded);
        }

        public void DestroyAllUis()
        {
            for (int i = 0; i < canvasRoots.Length; ++i)
            {
                canvasRoots[i].RemoveAllUis();
            }

            foreach (UiBase ui in loadedUiMap.Values)
            {
                ui.OnRuin();
                ResourceManager.Instance.Destroy(ui.gameObject);
            }
            loadedUiMap.Clear();
            closedUiHeadNode = null;
            closedUiTailNode = null;
        }

        public void OperateAsync(Type uiType, int op, UiAoHandler completeCb = null)
        {
            BindingResourceAttribute attr = GetBindingResourceAttribute(uiType);
            if (attr == null)
            {
                Logging.Error($"The type does not have BindingResourceAttribute. Type: {uiType}", nameof(UiManager));
                completeCb(null);
                return;
            }

            canvasRoots[attr.RootId].OperateAsync(uiType, op, completeCb);
        }

        public void OperateAsync(int rootId, int op, UiAoHandler completeCb = null)
        {
            if (rootId != AllCanvasRootsId)
            {
                canvasRoots[rootId].OperateAsync(op, completeCb);
                return;
            }

            for (int i = 0; i < canvasRoots.Length; ++i)
            {
                canvasRoots[i].OperateAsync(op, completeCb);
            }
        }

        private void CalculateSafeArea()
        {
            Rect safeArea = Screen.safeArea;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            Rect safeAreaRatio = new Rect(
                safeArea.x / screenSize.x,
                safeArea.y / screenSize.y,
                safeArea.width / screenSize.x,
                safeArea.height / screenSize.y);

            Vector2 canvasSize = (canvasRoots[0].transform as RectTransform).sizeDelta;
            CanvasSafeArea = new Rect(
                canvasSize.x * safeAreaRatio.x,
                canvasSize.y * safeAreaRatio.y,
                canvasSize.x * safeAreaRatio.width,
                canvasSize.y * safeAreaRatio.height);

            Logging.Info($"CanvasSafeArea: {CanvasSafeArea}", (int)LogChannel.UI);
        }
    }
}