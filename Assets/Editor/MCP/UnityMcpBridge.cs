using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KnightCharge.Editor.MCP
{
    [InitializeOnLoad]
    public static class UnityMcpBridge
    {
        private const int DefaultPort = 8080;
        private static HttpListener _listener;
        private static Thread _listenerThread;
        private static bool _isRunning;
        private static readonly List<LogEntry> RecentLogs = new List<LogEntry>();
        private const int MaxLogs = 150;
        private static readonly object LogLock = new object();

        [Serializable]
        public class LogEntry
        {
            public string condition;
            public string stackTrace;
            public string type;
            public string timestamp;
        }

        [Serializable]
        public class McpRequest
        {
            public string action;
            public string name;
            public string primitiveType;
            public float[] position;
            public float[] rotation;
            public float[] scale;
            public string[] components;
            public string componentName;
            public string state;
            public string menuItem;
            public int count = 30;
        }

        [Serializable]
        public class McpResponse
        {
            public bool success;
            public string message;
            public string dataJson;
        }

        static UnityMcpBridge()
        {
            Application.logMessageReceivedThreaded += OnLogMessageReceived;
            EditorApplication.quitting += StopServer;
            
            // Auto start bridge on editor load
            StartServer();
        }

        private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            lock (LogLock)
            {
                if (RecentLogs.Count >= MaxLogs)
                {
                    RecentLogs.RemoveAt(0);
                }

                RecentLogs.Add(new LogEntry
                {
                    condition = condition,
                    stackTrace = stackTrace,
                    type = type.ToString(),
                    timestamp = DateTime.Now.ToString("HH:mm:ss.fff")
                });
            }
        }

        [MenuItem("Tools/MCP Server/Start Server")]
        public static void StartServer()
        {
            if (_isRunning)
            {
                Debug.Log("[UnityMCP] Server is already running on port " + DefaultPort);
                return;
            }

            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://127.0.0.1:{DefaultPort}/");
                _listener.Prefixes.Add($"http://localhost:{DefaultPort}/");
                _listener.Start();
                _isRunning = true;

                _listenerThread = new Thread(ListenLoop)
                {
                    IsBackground = true
                };
                _listenerThread.Start();

                Debug.Log($"[UnityMCP] Server started successfully on http://127.0.0.1:{DefaultPort}/");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityMCP] Failed to start server: {ex.Message}");
                _isRunning = false;
            }
        }

        [MenuItem("Tools/MCP Server/Stop Server")]
        public static void StopServer()
        {
            if (!_isRunning) return;

            _isRunning = false;
            try
            {
                _listener?.Stop();
                _listener?.Close();
                _listener = null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UnityMCP] Error stopping listener: {ex.Message}");
            }

            Debug.Log("[UnityMCP] Server stopped.");
        }

        [MenuItem("Tools/MCP Server/Server Status")]
        public static void CheckStatus()
        {
            if (_isRunning)
            {
                EditorUtility.DisplayDialog("Unity MCP Status", $"Server is RUNNING on http://127.0.0.1:{DefaultPort}/", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Unity MCP Status", "Server is STOPPED", "OK");
            }
        }

        private static void ListenLoop()
        {
            while (_isRunning && _listener != null && _listener.IsListening)
            {
                try
                {
                    var context = _listener.GetContext();
                    ThreadPool.QueueUserWorkItem(ProcessRequest, context);
                }
                catch (HttpListenerException)
                {
                    // Listener stopped
                    break;
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        Debug.LogError($"[UnityMCP] Listen error: {ex.Message}");
                    }
                }
            }
        }

        private static void ProcessRequest(object state)
        {
            var context = (HttpListenerContext)state;
            var req = context.Request;
            var res = context.Response;

            // Add CORS headers
            res.Headers.Add("Access-Control-Allow-Origin", "*");
            res.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
            res.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            if (req.HttpMethod == "OPTIONS")
            {
                res.StatusCode = (int)HttpStatusCode.OK;
                res.Close();
                return;
            }

            string responseJson = "";

            try
            {
                string body = "";
                using (var reader = new StreamReader(req.InputStream, req.ContentEncoding))
                {
                    body = reader.ReadToEnd();
                }

                McpRequest requestData = null;
                if (!string.IsNullOrEmpty(body))
                {
                    requestData = JsonUtility.FromJson<McpRequest>(body);
                }

                string action = requestData?.action ?? req.QueryString["action"] ?? "ping";

                // Execute action on Unity Main Thread
                responseJson = ExecuteOnMainThread(action, requestData);
            }
            catch (Exception ex)
            {
                responseJson = JsonUtility.ToJson(new McpResponse
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }

            byte[] buffer = Encoding.UTF8.GetBytes(responseJson);
            res.ContentType = "application/json; charset=utf-8";
            res.ContentLength64 = buffer.Length;
            try
            {
                using (var output = res.OutputStream)
                {
                    output.Write(buffer, 0, buffer.Length);
                }
            }
            catch { }
        }

        private static string ExecuteOnMainThread(string action, McpRequest req)
        {
            var tcs = new TaskCompletionSource<string>();

            EditorApplication.delayCall += () =>
            {
                try
                {
                    string result = HandleAction(action, req);
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetResult(JsonUtility.ToJson(new McpResponse
                    {
                        success = false,
                        message = $"MainThread Exception: {ex.Message}\n{ex.StackTrace}"
                    }));
                }
            };

            // Wait with timeout
            if (tcs.Task.Wait(TimeSpan.FromSeconds(8)))
            {
                return tcs.Task.Result;
            }
            else
            {
                return JsonUtility.ToJson(new McpResponse
                {
                    success = false,
                    message = "Execution timed out waiting for Unity main thread."
                });
            }
        }

        private static string HandleAction(string action, McpRequest req)
        {
            switch (action.ToLowerInvariant())
            {
                case "ping":
                    return HandlePing();

                case "get_hierarchy":
                    return HandleGetHierarchy();

                case "get_object_details":
                    return HandleGetObjectDetails(req?.name);

                case "create_object":
                    return HandleCreateObject(req);

                case "modify_transform":
                    return HandleModifyTransform(req);

                case "delete_object":
                    return HandleDeleteObject(req?.name);

                case "add_component":
                    return HandleAddComponent(req?.name, req?.componentName);

                case "get_console_logs":
                    return HandleGetConsoleLogs(req?.count ?? 30);

                case "clear_console_logs":
                    return HandleClearConsoleLogs();

                case "set_play_mode":
                    return HandleSetPlayMode(req?.state);

                case "execute_menu_item":
                    return HandleExecuteMenuItem(req?.menuItem);

                default:
                    return JsonUtility.ToJson(new McpResponse
                    {
                        success = false,
                        message = $"Unknown action: '{action}'"
                    });
            }
        }

        #region Action Handlers

        private static string HandlePing()
        {
            var data = new
            {
                status = "connected",
                unityVersion = Application.unityVersion,
                productName = Application.productName,
                activeScene = SceneManager.GetActiveScene().name,
                isPlaying = EditorApplication.isPlaying,
                isPaused = EditorApplication.isPaused,
                isCompiling = EditorApplication.isCompiling
            };

            return JsonUtility.ToJson(new McpResponse
            {
                success = true,
                message = "Unity MCP Bridge is online and connected.",
                dataJson = JsonUtility.ToJson(data)
            });
        }

        [Serializable]
        private class HierarchyNode
        {
            public string name;
            public int instanceId;
            public bool active;
            public string tag;
            public int layer;
            public int childCount;
            public List<string> components = new List<string>();
            public List<HierarchyNode> children = new List<HierarchyNode>();
        }

        [Serializable]
        private class HierarchyData
        {
            public string sceneName;
            public int rootCount;
            public List<HierarchyNode> roots = new List<HierarchyNode>();
        }

        private static HierarchyNode BuildNode(GameObject go)
        {
            var node = new HierarchyNode
            {
                name = go.name,
                instanceId = go.GetInstanceID(),
                active = go.activeSelf,
                tag = go.tag,
                layer = go.layer,
                childCount = go.transform.childCount
            };

            foreach (var comp in go.GetComponents<Component>())
            {
                if (comp != null)
                {
                    node.components.Add(comp.GetType().Name);
                }
            }

            for (int i = 0; i < go.transform.childCount; i++)
            {
                node.children.Add(BuildNode(go.transform.GetChild(i).gameObject));
            }

            return node;
        }

        private static string HandleGetHierarchy()
        {
            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();

            var data = new HierarchyData
            {
                sceneName = scene.name,
                rootCount = rootObjects.Length
            };

            foreach (var root in rootObjects)
            {
                data.roots.Add(BuildNode(root));
            }

            return JsonUtility.ToJson(new McpResponse
            {
                success = true,
                message = $"Retrieved {rootObjects.Length} root objects from scene '{scene.name}'",
                dataJson = JsonUtility.ToJson(data)
            });
        }

        [Serializable]
        private class GameObjectDetails
        {
            public string name;
            public int instanceId;
            public bool active;
            public string tag;
            public int layer;
            public float[] position;
            public float[] localPosition;
            public float[] eulerAngles;
            public float[] localScale;
            public List<string> components = new List<string>();
        }

        private static string HandleGetObjectDetails(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return JsonUtility.ToJson(new McpResponse { success = false, message = "objectName cannot be empty" });
            }

            var go = GameObject.Find(objectName);
            if (go == null)
            {
                return JsonUtility.ToJson(new McpResponse { success = false, message = $"GameObject '{objectName}' not found in scene." });
            }

            var details = new GameObjectDetails
            {
                name = go.name,
                instanceId = go.GetInstanceID(),
                active = go.activeSelf,
                tag = go.tag,
                layer = go.layer,
                position = new[] { go.transform.position.x, go.transform.position.y, go.transform.position.z },
                localPosition = new[] { go.transform.localPosition.x, go.transform.localPosition.y, go.transform.localPosition.z },
                eulerAngles = new[] { go.transform.eulerAngles.x, go.transform.eulerAngles.y, go.transform.eulerAngles.z },
                localScale = new[] { go.transform.localScale.x, go.transform.localScale.y, go.transform.localScale.z }
            };

            foreach (var comp in go.GetComponents<Component>())
            {
                if (comp != null)
                {
                    details.components.Add(comp.GetType().FullName);
                }
            }

            return JsonUtility.ToJson(new McpResponse
            {
                success = true,
                message = $"Details for GameObject '{objectName}'",
                dataJson = JsonUtility.ToJson(details)
            });
        }

        private static string HandleCreateObject(McpRequest req)
        {
            string name = string.IsNullOrEmpty(req?.name) ? "New GameObject" : req.name;
            GameObject go;

            if (!string.IsNullOrEmpty(req?.primitiveType))
            {
                if (Enum.TryParse<PrimitiveType>(req.primitiveType, true, out var primType))
                {
                    go = GameObject.CreatePrimitive(primType);
                    go.name = name;
                }
                else
                {
                    go = new GameObject(name);
                }
            }
            else
            {
                go = new GameObject(name);
            }

            if (req?.position != null && req.position.Length >= 3)
            {
                go.transform.position = new Vector3(req.position[0], req.position[1], req.position[2]);
            }

            if (req?.rotation != null && req.rotation.Length >= 3)
            {
                go.transform.eulerAngles = new Vector3(req.rotation[0], req.rotation[1], req.rotation[2]);
            }

            if (req?.scale != null && req.scale.Length >= 3)
            {
                go.transform.localScale = new Vector3(req.scale[0], req.scale[1], req.scale[2]);
            }

            if (req?.components != null)
            {
                foreach (var compName in req.components)
                {
                    AttachComponentByName(go, compName);
                }
            }

            Undo.RegisterCreatedObjectUndo(go, $"Create {name} via MCP");
            Selection.activeGameObject = go;

            return JsonUtility.ToJson(new McpResponse
            {
                success = true,
                message = $"Successfully created GameObject '{name}' (InstanceID: {go.GetInstanceID()})",
                dataJson = JsonUtility.ToJson(new { name = go.name, instanceId = go.GetInstanceID() })
            });
        }

        private static string HandleModifyTransform(McpRequest req)
        {
            if (string.IsNullOrEmpty(req?.name))
            {
                return JsonUtility.ToJson(new McpResponse { success = false, message = "GameObject name must be provided." });
            }

            var go = GameObject.Find(req.name);
            if (go == null)
            {
                return JsonUtility.ToJson(new McpResponse { success = false, message = $"GameObject '{req.name}' not found." });
            }

            Undo.RecordObject(go.transform, $"Modify Transform {req.name} via MCP");

            if (req.position != null && req.position.Length >= 3)
            {
                go.transform.position = new Vector3(req.position[0], req.position[1], req.position[2]);
            }

            if (req.rotation != null && req.rotation.Length >= 3)
            {
                go.transform.eulerAngles = new Vector3(req.rotation[0], req.rotation[1], req.rotation[2]);
            }

            if (req.scale != null && req.scale.Length >= 3)
            {
                go.transform.localScale = new Vector3(req.scale[0], req.scale[1], req.scale[2]);
            }

            return JsonUtility.ToJson(new McpResponse
            {
                success = true,
                message = $"Updated Transform for '{req.name}'",
                dataJson = JsonUtility.ToJson(new
                {
                    position = new[] { go.transform.position.x, go.transform.position.y, go.transform.position.z },
                    rotation = new[] { go.transform.eulerAngles.x, go.transform.eulerAngles.y, go.transform.eulerAngles.z },
                    scale = new[] { go.transform.localScale.x, go.transform.localScale.y, go.transform.localScale.z }
                })
            });
        }

        private static string HandleDeleteObject(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return JsonUtility.ToJson(new McpResponse { success = false, message = "objectName must be provided." });
            }

            var go = GameObject.Find(objectName);
            if (go == null)
            {
                return JsonUtility.ToJson(new McpResponse { success = false, message = $"GameObject '{objectName}' not found." });
            }

            Undo.DestroyObjectImmediate(go);
            return JsonUtility.ToJson(new McpResponse
            {
                success = true,
                message = $"Successfully deleted GameObject '{objectName}'."
            });
        }

        private static string HandleAddComponent(string objectName, string componentName)
        {
            if (string.IsNullOrEmpty(objectName) || string.IsNullOrEmpty(componentName))
            {
                return JsonUtility.ToJson(new McpResponse { success = false, message = "Both objectName and componentName are required." });
            }

            var go = GameObject.Find(objectName);
            if (go == null)
            {
                return JsonUtility.ToJson(new McpResponse { success = false, message = $"GameObject '{objectName}' not found." });
            }

            var comp = AttachComponentByName(go, componentName);
            if (comp != null)
            {
                Undo.RegisterCreatedObjectUndo(comp, $"Add {componentName} via MCP");
                return JsonUtility.ToJson(new McpResponse
                {
                    success = true,
                    message = $"Successfully added '{componentName}' to '{objectName}'."
                });
            }
            else
            {
                return JsonUtility.ToJson(new McpResponse
                {
                    success = false,
                    message = $"Could not find component type '{componentName}'."
                });
            }
        }

        private static Component AttachComponentByName(GameObject go, string compName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(compName) ?? assembly.GetType("UnityEngine." + compName);
                if (type != null && typeof(Component).IsAssignableFrom(type))
                {
                    return go.AddComponent(type);
                }
            }
            return null;
        }

        [Serializable]
        private class LogListWrapper
        {
            public List<LogEntry> logs = new List<LogEntry>();
            public int totalCount;
        }

        private static string HandleGetConsoleLogs(int count)
        {
            var result = new LogListWrapper();
            lock (LogLock)
            {
                int start = Math.Max(0, RecentLogs.Count - count);
                for (int i = start; i < RecentLogs.Count; i++)
                {
                    result.logs.Add(RecentLogs[i]);
                }
                result.totalCount = RecentLogs.Count;
            }

            return JsonUtility.ToJson(new McpResponse
            {
                success = true,
                message = $"Retrieved {result.logs.Count} recent logs.",
                dataJson = JsonUtility.ToJson(result)
            });
        }

        private static string HandleClearConsoleLogs()
        {
            lock (LogLock)
            {
                RecentLogs.Clear();
            }

            // Also clear editor log if possible
            var logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMethod = logEntries?.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod?.Invoke(null, null);

            return JsonUtility.ToJson(new McpResponse
            {
                success = true,
                message = "Cleared Unity console logs."
            });
        }

        private static string HandleSetPlayMode(string state)
        {
            switch (state?.ToLowerInvariant())
            {
                case "play":
                    EditorApplication.isPlaying = true;
                    break;
                case "stop":
                    EditorApplication.isPlaying = false;
                    break;
                case "pause":
                    EditorApplication.isPaused = !EditorApplication.isPaused;
                    break;
                default:
                    return JsonUtility.ToJson(new McpResponse { success = false, message = "state must be 'play', 'stop', or 'pause'." });
            }

            return JsonUtility.ToJson(new McpResponse
            {
                success = true,
                message = $"Play mode state updated to: isPlaying={EditorApplication.isPlaying}, isPaused={EditorApplication.isPaused}"
            });
        }

        private static string HandleExecuteMenuItem(string menuItem)
        {
            if (string.IsNullOrEmpty(menuItem))
            {
                return JsonUtility.ToJson(new McpResponse { success = false, message = "menuItem name cannot be empty." });
            }

            bool executed = EditorApplication.ExecuteMenuItem(menuItem);
            return JsonUtility.ToJson(new McpResponse
            {
                success = executed,
                message = executed ? $"Successfully executed menu item '{menuItem}'." : $"Failed to execute menu item '{menuItem}'."
            });
        }

        #endregion
    }
}
