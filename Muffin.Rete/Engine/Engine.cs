using Microsoft.ClearScript.V8;
using System.Diagnostics;
using System.Dynamic;
using System.Text;

namespace Muffin.Rete.Engine
{
    public class ReteEngine
    {
        private Dictionary<string, Component> _components = new Dictionary<string, Component>();
        private Dictionary<NodeData, object> _processedNodes = new Dictionary<NodeData, object>();

        public void AddComponents(IEnumerable<Component> components)
        {
            foreach (var component in components)
            {
                AddComponent(component);
            }
        }

        public void AddComponent(Component component)
        {
            _components[component.Name] = component;
        }

        public async Task Run(ReteModel model, ReteEvent @event, CancellationToken cancellationToken)
        {
            var startNode = model.FindByEvent(@event);

            // validieren ob es alle components gibt

            var context = new ReteContext()
            {
                Model = model,
                Engine = this
            };

            if (startNode != null)
            {
                await Task.Run(async () =>
                {
                    await _runNode(startNode, context);
                }, cancellationToken);

            }
        }

        public async Task Run(ReteModel model, int startId, CancellationToken cancellationToken)
        {
            var startNode = model.FindById(startId);

            // validieren ob es alle components gibt

            var context = new ReteContext()
            {
                Model = model,
                Engine = this
            };

            if (startNode != null)
            {
                await Task.Run(async () =>
                {
                    await _runNode(startNode, context);
                }, cancellationToken);

            }
        }

        private Component _getComponent(NodeData node, ReteContext context)
        {
            return _components[node.name];
        }

        private async Task<dynamic> _getInputData(NodeData node, ReteContext context)
        {
            if (node.inputs == null || node.inputs.Count == 0)
            {
                node.InputData = new ExpandoObject();
                return node.InputData;
            }

            node.InputData = (await Task.WhenAll(node.inputs.Select(async x => new { Key = x.Key, Input = await _getInput(x.Value, context) })))
                .ToDictionary(x => x.Key, x => x.Input)
                .Aggregate(new ExpandoObject() as IDictionary<string, Object>,
                            (a, p) => { a.Add(p); return a; });
            return node.InputData;
        }

        private async Task<dynamic> _getInput(InputData inputData, ReteContext context)
        {
            if (inputData.Connections != null)
            {
                if (inputData.Connections.Count == 1)
                {
                    return await _getInput(inputData.Connections[0], context);
                }

                return inputData.Connections.Select(x => _getInput(x, context)).ToArray();
            }
            return null;
        }

        private async Task<dynamic> _getInput(InputConnectionData inputConnectionData, ReteContext context)
        {
            if (inputConnectionData.Data != null)
            {
                return inputConnectionData.Data;
            }

            var node = context.Model.FindById(inputConnectionData.Node);
            if (node.OutputData == null && !node.IsProcessed)
            {
                await _runNode(node, context);
            }

            return (node.OutputData as IDictionary<string, dynamic>)[inputConnectionData.Output];
        }

        private async Task _runNode(NodeData node, ReteContext context)
        {
            node.IsProcessed = true;
            var component = _getComponent(node, context);
            var inputData = await _getInputData(node, context) ?? new ExpandoObject();
            node.OutputData = new ExpandoObject();

            try
            {
                using (var engine = new V8ScriptEngine())
                {
                    engine.AddHostType(typeof(Dictionary<string, object>));
                    engine.AddHostType(typeof(NodeData));
                    if (!Debugger.IsAttached)
                    {
                        engine.AddHostType(typeof(Console));
                        engine.Execute(@"
                            console = {
                                log: value => Console.WriteLine(value),
                                warn: value => Console.WriteLine('WARNING: {0}', value),
                                error: value => Console.WriteLine('ERROR: {0}', value)
                            };
                        ");
                    }
                    else
                    {
                        engine.AddHostType(typeof(ConsoleHelper));
                        engine.Execute(@"
                            console = {
                                log: value => ConsoleHelper.WriteLine(value),
                                warn: value => ConsoleHelper.WriteLine(value),
                                error: value => ConsoleHelper.WriteLine(value)
                            };
                        ");
                    }

                    if (context.Event != null)
                    {
                        engine.Execute(_getManifestString("Muffin.Rete.Sockets.Header.js"));
                        engine.Script.window.Muffin.Rete.Global.Event = context.Event;
                    }

                    var part = component.JsScript.Split("worker(")[1];
                    var sb = new StringBuilder();
                    sb.Append("function worker(");

                    var depth = 0;
                    var functionStarted = false;
                    foreach (var c in part)
                    {
                        sb.Append(c);
                        if (c == '{')
                        {
                            functionStarted = true;
                            depth++;
                        }

                        if (c == '}')
                        {
                            depth--;
                        }

                        if (depth == 0 && functionStarted)
                        {
                            break;
                        }
                    }

                    var script = sb.ToString();

                    engine.Execute(/*new DocumentInfo() { Category = ModuleCategory.CommonJS },*/ script);
                    engine.Script.worker(node, inputData, node.OutputData);
                }
            }
            catch (Exception ex)
            {

            }

            if (node.outputs != null && node.outputs.Count > 0 && node.outputs.Values.Any(x => x.Connections.Count > 0))
            {
                foreach (var key in node.outputs.Keys)
                {
                    var output = node.outputs[key];
                    if (output.IsProcessed)
                    {
                        continue;
                    }
                    output.IsProcessed = true;

                    foreach (var connection in output.Connections)
                    {
                        if (connection.IsProcessed)
                        {
                            continue;
                        }
                        connection.IsProcessed = true;

                        var innerNode = context.Model.FindById(connection.Node);
                        if (innerNode != null)
                        {
                            if (innerNode.IsProcessed)
                            {
                                continue;
                            }
                            await _runNode(innerNode, context);
                        }
                    }

                }
            }
        }

        public string RenderScript()
        {
            var ressourceNames = typeof(ReteEngine).Assembly.GetManifestResourceNames();

            var sb = new StringBuilder();
            sb.AppendLine(_getManifestString("Muffin.Rete.Sockets.Header.js"));
            sb.AppendLine(_getManifestString("Muffin.Rete.Sockets.Sockets.js"));

            var controlRessourceNames = ressourceNames
                .Where(x => x.StartsWith("Muffin.Rete.Controls"))
                .ToArray();
            foreach (var controlRessourceName in controlRessourceNames)
            {
                sb.AppendLine(_getManifestString(controlRessourceName));
            }

            var componentRessourceNames = ressourceNames
                .Where(x => x.StartsWith("Muffin.Rete.Components"))
                .ToArray();
            foreach (var componentRessourceName in componentRessourceNames)
            {
                sb.AppendLine(_getManifestString(componentRessourceName));
            }

            var components = GetSystemComponents();
            sb.AppendLine($"window.Muffin.Rete.ComponentList = [{string.Join(", ", components.Select(x => $"window.Muffin.Rete.Components.{x.Name.Replace("Component", "")}"))}];");

            return sb.ToString();
        }

        public string RenderScriptWithTag()
        {
            return $"<script>{RenderScript()}</script>";
        }

        private static string _getManifestString(string path)
        {
            using (var stream = typeof(ReteEngine).Assembly.GetManifestResourceStream(path))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static Component[] GetSystemComponents()
        {
            var names = typeof(ReteEngine).Assembly
                .GetManifestResourceNames()
                .Where(x => x.StartsWith("Muffin.Rete.Components"))
                .ToArray();
            return names
                .Select(x => new Component()
                {
                    Name = x.Replace("Component.js", "").Split('.').Last(),
                    JsScript = _getManifestString(x)
                })
                .ToArray();
        }
    }

    public static class ConsoleHelper
    {
        public static void WriteLine(object value)
        {
            Trace.WriteLine(value);
        }
    }
}
