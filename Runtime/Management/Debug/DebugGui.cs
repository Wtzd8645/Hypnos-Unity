using System.Collections.Generic;
using UnityEngine;

namespace Blanketmen.Hypnos
{
    public class DebugGui : MonoBehaviour
    {
        #region Singleton
        public static DebugGui Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            Initialize();
        }

        private void OnDestroy()
        {
            if (this == Instance)
            {
                Release();
                Instance = null;
            }
        }
        #endregion

        [SerializeField] private DebugGuiSettings debugGuiSettings = null;

        private HashSet<Log> logs;
        private List<Log> receivedLogs = new List<Log>(32);
        private SortedList<Log, int> logDict = new SortedList<Log, int>();

        private List<Log> totalLogs = new List<Log>(512);
        private List<Log> collapsedLogs = new List<Log>(128);
        private int totalNormalLogCount = 0;
        private int totalWarningLogCount = 0;
        private int totalErrorLogCount = 0;
        private long logsMemoryUsage = 0;

        private List<Log> currentLogs = new List<Log>(512);
        private int logLevelMask = 0xFF;
        private bool isCollapse = false;
        private bool isFilter = false;
        private string filterString = string.Empty;

        private Rect logsRect;
        private Vector2 scrollPosition;

        private bool isDragging;
        private Vector2 startPosition;
        private Vector2 lastPosition;
        private Vector2 deltaPosition;

        public void Initialize()
        {
            Application.logMessageReceivedThreaded += CaptureLogThread;
            CalculateLayout();
        }

        public void Release()
        {
            Application.logMessageReceivedThreaded -= CaptureLogThread;
        }

        private void Update()
        {
            GetInput();
            ProcessLogs();
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(logsRect);
            if (isDragging && deltaPosition.y != 0)
            {
                if (logsRect.Contains(new Vector2(startPosition.x, Screen.height - startPosition.y)))
                {
                    scrollPosition.y += deltaPosition.y;
                }
            }
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            for (int i = 0; i < currentLogs.Count; i++)
            {
                if (GUILayout.Button(currentLogs[i].condition))
                {
                    Logging.Info(currentLogs[i].condition);
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void CaptureLogThread(string condition, string stackTrace, LogType type)
        {
            Log log = new Log(type, condition, stackTrace);
            logsMemoryUsage += log.GetMemoryUsage();

            lock (receivedLogs)
            {
                receivedLogs.Add(log);
            }
        }

        private void ProcessLogs()
        {
            if (receivedLogs.Count == 0)
            {
                return;
            }

            lock (receivedLogs)
            {
                Log log = null;
                int collapsedCount = 0;
                for (int i = 0; i < receivedLogs.Count; i++)
                {
                    log = receivedLogs[i];
                    logDict.TryGetValue(log, out collapsedCount);
                    logDict[log] = ++collapsedCount;
                    totalLogs.Add(log);
                    if (collapsedCount == 1)
                    {
                        collapsedLogs.Add(log);
                    }

                    switch (receivedLogs[i].logType)
                    {
                        case LogType.Log:
                            ++totalNormalLogCount;
                            break;
                        case LogType.Warning:
                            ++totalWarningLogCount;
                            break;
                        case LogType.Error:
                            ++totalErrorLogCount;
                            break;
                        default:
                            break;
                    }
                }
                receivedLogs.Clear();
            }

            CalculateCurrentLog();
        }

        private void CalculateCurrentLog()
        {
            currentLogs.Clear();
            int isShow = 0;
            if (isCollapse)
            {
                for (int i = 0; i < collapsedLogs.Count; i++)
                {
                    isShow = (1 << (int)collapsedLogs[i].logType) & logLevelMask;
                    if (isShow == 0)
                    {
                        continue;
                    }

                    if (isFilter)
                    {
                        if (collapsedLogs[i].condition.Contains(filterString))
                        {
                            currentLogs.Add(collapsedLogs[i]);
                        }
                        continue;
                    }

                    currentLogs.Add(collapsedLogs[i]);
                }
                return;
            }

            for (int i = 0; i < totalLogs.Count; i++)
            {
                isShow = (1 << (int)totalLogs[i].logType) & logLevelMask;
                if (isShow == 0)
                {
                    continue;
                }

                if (isFilter)
                {
                    if (totalLogs[i].condition.Contains(filterString))
                    {
                        currentLogs.Add(totalLogs[i]);
                    }
                    continue;
                }

                currentLogs.Add(totalLogs[i]);
            }
        }

        private void CalculateLayout()
        {
            logsRect.width = 800;// Screen.width;
            logsRect.height = 400;// Screen.height;
        }
#if UNITY_EDITOR || UNITY_STANDALONE
        private void GetInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                startPosition = Input.mousePosition;
                lastPosition = startPosition;
                return;
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                return;
            }

            if (Input.GetMouseButton(0))
            {
                Vector2 pos = Input.mousePosition;
                deltaPosition = pos - lastPosition;
                lastPosition = pos;
            }
        }
#elif UNITY_ANDROID || UNITY_IOS
        private void GetInput()
        {
            if (Input.touches.Length != 1)
            {
                return;
            }

            switch (Input.touches[0].phase)
            {
                case TouchPhase.Began:
                    isDragging = true;
                    startPosition = Input.touches[0].position;
                    lastPosition = startPosition;
                    break;
                case TouchPhase.Moved:
                    Vector2 pos = Input.touches[0].position;
                    deltaPosition = pos - lastPosition;
                    lastPosition = pos;
                    break;
                case TouchPhase.Ended:
                    isDragging = false;
                    break;
            }
        }
#endif
    }
}