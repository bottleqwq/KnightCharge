#!/usr/bin/env node
const http = require('http');
const readline = require('readline');

const UNITY_PORT = process.env.UNITY_PORT || 8080;
const UNITY_HOST = '127.0.0.1';

/**
 * Send request to Unity C# Bridge HTTP server
 */
function callUnityBridge(action, params = {}) {
  return new Promise((resolve) => {
    const postData = JSON.stringify({ action, ...params });
    const options = {
      hostname: UNITY_HOST,
      port: UNITY_PORT,
      path: '/action',
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Content-Length': Buffer.byteLength(postData)
      },
      timeout: 10000
    };

    const req = http.request(options, (res) => {
      let data = '';
      res.on('data', (chunk) => { data += chunk; });
      res.on('end', () => {
        try {
          const parsed = JSON.parse(data);
          resolve(parsed);
        } catch (e) {
          resolve({ success: false, message: 'Invalid response from Unity: ' + data });
        }
      });
    });

    req.on('error', (err) => {
      resolve({
        success: false,
        message: `Could not connect to Unity Editor on port ${UNITY_PORT}. Please ensure Unity is open with the project running the UnityMcpBridge. Error: ${err.message}`
      });
    });

    req.on('timeout', () => {
      req.destroy();
      resolve({
        success: false,
        message: 'Request to Unity Editor timed out after 10s.'
      });
    });

    req.write(postData);
    req.end();
  });
}

// Tool definitions for MCP
const TOOLS = [
  {
    name: 'unity_ping',
    description: 'Check the connection status with Unity Editor, and retrieve basic info (Unity version, active scene, play mode state).',
    inputSchema: {
      type: 'object',
      properties: {}
    }
  },
  {
    name: 'unity_get_scene_hierarchy',
    description: 'Retrieve the GameObject hierarchy tree for the currently active Unity scene.',
    inputSchema: {
      type: 'object',
      properties: {}
    }
  },
  {
    name: 'unity_get_gameobject_details',
    description: 'Get detailed information about a specific GameObject, including transform (position, rotation, scale), layer, tag, and attached components.',
    inputSchema: {
      type: 'object',
      properties: {
        name: { type: 'string', description: 'The name of the GameObject to inspect.' }
      },
      required: ['name']
    }
  },
  {
    name: 'unity_create_gameobject',
    description: 'Create a new GameObject in the active Unity scene (Cube, Sphere, Capsule, Cylinder, Plane, or Empty).',
    inputSchema: {
      type: 'object',
      properties: {
        name: { type: 'string', description: 'Name of the GameObject.' },
        primitiveType: {
          type: 'string',
          enum: ['Cube', 'Sphere', 'Capsule', 'Cylinder', 'Plane', 'Quad', 'Empty'],
          description: 'Primitive type (leave empty or "Empty" for an empty GameObject).'
        },
        position: {
          type: 'array',
          items: { type: 'number' },
          description: '[x, y, z] world coordinates.'
        },
        rotation: {
          type: 'array',
          items: { type: 'number' },
          description: '[x, y, z] Euler angles.'
        },
        scale: {
          type: 'array',
          items: { type: 'number' },
          description: '[x, y, z] local scale (e.g. [1, 1, 1]).'
        },
        components: {
          type: 'array',
          items: { type: 'string' },
          description: 'List of component class names to attach (e.g. ["Rigidbody", "BoxCollider"]).'
        }
      },
      required: ['name']
    }
  },
  {
    name: 'unity_modify_gameobject',
    description: 'Modify the transform (position, rotation, scale) of an existing GameObject in the scene.',
    inputSchema: {
      type: 'object',
      properties: {
        name: { type: 'string', description: 'The name of the GameObject to modify.' },
        position: {
          type: 'array',
          items: { type: 'number' },
          description: '[x, y, z] world coordinates.'
        },
        rotation: {
          type: 'array',
          items: { type: 'number' },
          description: '[x, y, z] Euler angles.'
        },
        scale: {
          type: 'array',
          items: { type: 'number' },
          description: '[x, y, z] local scale.'
        }
      },
      required: ['name']
    }
  },
  {
    name: 'unity_delete_gameobject',
    description: 'Delete a GameObject from the current active Unity scene.',
    inputSchema: {
      type: 'object',
      properties: {
        name: { type: 'string', description: 'The name of the GameObject to delete.' }
      },
      required: ['name']
    }
  },
  {
    name: 'unity_add_component',
    description: 'Add a component by class name to a GameObject.',
    inputSchema: {
      type: 'object',
      properties: {
        name: { type: 'string', description: 'Name of the GameObject.' },
        componentName: { type: 'string', description: 'Component class name (e.g. "Rigidbody", "AudioSource", or custom script name).' }
      },
      required: ['name', 'componentName']
    }
  },
  {
    name: 'unity_get_console_logs',
    description: 'Retrieve recent log, warning, and error messages from the Unity Editor Console.',
    inputSchema: {
      type: 'object',
      properties: {
        count: { type: 'number', description: 'Number of recent logs to fetch (default: 30).' }
      }
    }
  },
  {
    name: 'unity_clear_console_logs',
    description: 'Clear all entries in the Unity Editor Console.',
    inputSchema: {
      type: 'object',
      properties: {}
    }
  },
  {
    name: 'unity_set_play_mode',
    description: 'Change the play mode state of the Unity Editor.',
    inputSchema: {
      type: 'object',
      properties: {
        state: {
          type: 'string',
          enum: ['play', 'stop', 'pause'],
          description: 'Action to perform: "play" to enter play mode, "stop" to exit, "pause" to toggle pause.'
        }
      },
      required: ['state']
    }
  },
  {
    name: 'unity_execute_menu_item',
    description: 'Execute a Unity Editor menu item command (e.g. "File/Save Project", "Assets/Refresh").',
    inputSchema: {
      type: 'object',
      properties: {
        menuItem: { type: 'string', description: 'Full path of the menu item (e.g. "File/Save Project").' }
      },
      required: ['menuItem']
    }
  }
];

// MCP JSON-RPC Server
async function handleRpcRequest(request) {
  const { id, method, params } = request;

  if (method === 'initialize') {
    return {
      jsonrpc: '2.0',
      id,
      result: {
        protocolVersion: '2024-11-05',
        capabilities: {
          tools: {}
        },
        serverInfo: {
          name: 'unity-mcp-server',
          version: '1.0.0'
        }
      }
    };
  }

  if (method === 'notifications/initialized') {
    return null; // No response for notifications
  }

  if (method === 'ping') {
    return { jsonrpc: '2.0', id, result: {} };
  }

  if (method === 'tools/list') {
    return {
      jsonrpc: '2.0',
      id,
      result: {
        tools: TOOLS
      }
    };
  }

  if (method === 'tools/call') {
    const { name, arguments: args } = params || {};
    let action = '';

    switch (name) {
      case 'unity_ping':
        action = 'ping';
        break;
      case 'unity_get_scene_hierarchy':
        action = 'get_hierarchy';
        break;
      case 'unity_get_gameobject_details':
        action = 'get_object_details';
        break;
      case 'unity_create_gameobject':
        action = 'create_object';
        break;
      case 'unity_modify_gameobject':
        action = 'modify_transform';
        break;
      case 'unity_delete_gameobject':
        action = 'delete_object';
        break;
      case 'unity_add_component':
        action = 'add_component';
        break;
      case 'unity_get_console_logs':
        action = 'get_console_logs';
        break;
      case 'unity_clear_console_logs':
        action = 'clear_console_logs';
        break;
      case 'unity_set_play_mode':
        action = 'set_play_mode';
        break;
      case 'unity_execute_menu_item':
        action = 'execute_menu_item';
        break;
      default:
        return {
          jsonrpc: '2.0',
          id,
          error: { code: -32601, message: `Method or tool '${name}' not found.` }
        };
    }

    const bridgeResult = await callUnityBridge(action, args || {});
    let formattedText = '';

    if (bridgeResult.success) {
      if (bridgeResult.dataJson) {
        try {
          const parsedData = JSON.parse(bridgeResult.dataJson);
          formattedText = `${bridgeResult.message}\n\n${JSON.stringify(parsedData, null, 2)}`;
        } catch {
          formattedText = `${bridgeResult.message}\n\n${bridgeResult.dataJson}`;
        }
      } else {
        formattedText = bridgeResult.message;
      }
    } else {
      formattedText = `[UnityMCP Error] ${bridgeResult.message}`;
    }

    return {
      jsonrpc: '2.0',
      id,
      result: {
        content: [
          {
            type: 'text',
            text: formattedText
          }
        ],
        isError: !bridgeResult.success
      }
    };
  }

  return {
    jsonrpc: '2.0',
    id,
    error: { code: -32601, message: `Unknown method '${method}'` }
  };
}

// Start stdio interface
const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout,
  terminal: false
});

rl.on('line', async (line) => {
  if (!line.trim()) return;
  try {
    const request = JSON.parse(line);
    const response = await handleRpcRequest(request);
    if (response) {
      process.stdout.write(JSON.stringify(response) + '\n');
    }
  } catch (err) {
    const errResponse = {
      jsonrpc: '2.0',
      id: null,
      error: { code: -32700, message: 'Parse error: ' + err.message }
    };
    process.stdout.write(JSON.stringify(errResponse) + '\n');
  }
});
