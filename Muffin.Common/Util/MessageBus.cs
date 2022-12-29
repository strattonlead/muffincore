using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Muffin.Common.Util
{
    public class MessageBus
    {
        #region Singleton

        public const string DEFAULT_MESSAGE_BUS = nameof(DEFAULT_MESSAGE_BUS);
        private MessageBus() { }
        private static Dictionary<string, MessageBus> _instances = new Dictionary<string, MessageBus>();
        private static object _lock = new object();
        public static MessageBus GetInstance(string name)
        {
            if (name == null)
            {
                name = DEFAULT_MESSAGE_BUS;
            }

            lock (_lock)
            {
                if (!_instances.TryGetValue(name, out MessageBus model))
                {
                    model = new MessageBus()
                    {
                        Name = name
                    };
                    _instances[name] = model;
                }
                return model;
            }
        }
        public static MessageBus GetInstance()
        {
            return GetInstance(DEFAULT_MESSAGE_BUS);
        }

        #endregion

        #region Properties

        public string Name { get; private set; }
        private Dictionary<string, List<Action<object, object[]>>> _actions = new Dictionary<string, List<Action<object, object[]>>>();
        private Dictionary<string, List<MethodCallback>> _methodCallbacks = new Dictionary<string, List<MethodCallback>>();

        #endregion

        #region Bus

        public void Register(string name, Action action)
        {
            if (!_actions.TryGetValue(name, out List<Action<object, object[]>> actions))
            {
                Action<object, object[]> wrapper = (sender, parameters) => { action(); };
                actions.Add(wrapper);
            }
        }

        public void Register<T>(string name, Action<T> action)
        {
            if (!_actions.TryGetValue(name, out List<Action<object, object[]>> actions))
            {
                Action<object, object[]> wrapper = (sender, parameters) => { action((T)parameters[0]); };
                actions.Add(wrapper);
            }
        }

        public void Register<T1, T2>(string name, Action<T1, T2> action)
        {
            if (!_actions.TryGetValue(name, out List<Action<object, object[]>> actions))
            {
                Action<object, object[]> wrapper = (sender, parameters) => { action((T1)parameters[0], (T2)parameters[1]); };
                actions.Add(wrapper);
            }
        }

        public void Register<T1, T2, T3>(string name, Action<T1, T2, T3> action)
        {
            if (!_actions.TryGetValue(name, out List<Action<object, object[]>> actions))
            {
                Action<object, object[]> wrapper = (sender, parameters) => { action((T1)parameters[0], (T2)parameters[1], (T3)parameters[2]); };
                actions.Add(wrapper);
            }
        }

        public void Register(string name, Action<object, object[]> action)
        {
            if (!_actions.TryGetValue(name, out List<Action<object, object[]>> actions))
            {
                actions = new List<Action<object, object[]>>();
            }
            actions.Add(action);
        }

        public void Unregister(string name, Action<object, object[]> action)
        {
            if (_actions.TryGetValue(name, out List<Action<object, object[]>> actions))
            {
                actions.Remove(action);
            }
        }

        public void UnregisterAll(string name)
        {
            if (_actions.ContainsKey(name))
            {
                _actions.Remove(name);
                _methodCallbacks.Remove(name);
            }
        }

        public void UnregisterAll()
        {
            _actions.Clear();
            _methodCallbacks.Clear();
        }

        public MethodInfo[] GetRegisteredMethods(string name)
        {
            if (_methodCallbacks.TryGetValue(name, out List<MethodCallback> list))
            {
                return list
                    .Select(x => x.MethodInfo)
                    .Distinct()
                    .ToArray();
            }
            return new MethodInfo[0];
        }

        public void Register<T>(string name, T receiver, Expression<Action<T>> expression, long maxInvocationCount = long.MaxValue)
        {
            if (!_methodCallbacks.TryGetValue(name, out List<MethodCallback> methodCallbacks))
            {
                methodCallbacks = new List<MethodCallback>();
                _methodCallbacks[name] = methodCallbacks;
            }
            var methodCallback = MethodCallback.Create(receiver, expression, maxInvocationCount);
            methodCallbacks.Add(methodCallback);
        }

        public void RegisterOnce<T>(string name, T receiver, Expression<Action<T>> expression)
        {
            if (!_methodCallbacks.TryGetValue(name, out List<MethodCallback> methodCallbacks))
            {
                methodCallbacks = new List<MethodCallback>();
                _methodCallbacks[name] = methodCallbacks;
            }
            var methodCallback = MethodCallback.Create(receiver, expression, 1);
            methodCallbacks.Add(methodCallback);
        }

        public void Invoke(string name, object sender, params object[] parameters)
        {
            if (_actions.TryGetValue(name, out List<Action<object, object[]>> actions))
            {
                foreach (var action in actions)
                {
                    action?.Invoke(sender, parameters);
                }
            }

            if (_methodCallbacks.TryGetValue(name, out List<MethodCallback> methodCallbacks))
            {
                foreach (var methodCallback in methodCallbacks)
                {
                    methodCallback.TryInvoke(parameters);
                }
            }
        }

        public WaitForHandler RegisterWaitForHandler(string name)
        {
            var waitForHandler = new WaitForHandler(name, this);
            Register(name, (x, y) =>
            {
                waitForHandler.SignalReceived = true;
                waitForHandler.Result = y;
                waitForHandler.ResetEvent.Set();
            });
            return waitForHandler;
        }

        #endregion
    }

    public class MethodCallback
    {
        public object Receiver { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public long InvocationCount { get; set; }
        public long MaxInvocationCount { get; set; }

        public MethodCallback(object receiver, MethodInfo methodInfo, long maxInvocationCount)
        {
            Receiver = receiver;
            MethodInfo = methodInfo;
            MaxInvocationCount = maxInvocationCount;
        }

        public static MethodCallback Create<T>(T receiver, Expression<Action<T>> expression, long maxInvocationCount = long.MaxValue)
        {
            var methodInfo = ((MethodCallExpression)expression.Body).Method;
            return new MethodCallback(receiver, methodInfo, maxInvocationCount);
        }

        public object[] ConvertParameters(object[] parameters)
        {
            if (parameters == null)
            {
                parameters = new object[0];
            }

            var methodParameters = MethodInfo.GetParameters();
            if (parameters.Length != methodParameters.Length)
            {
                throw new ArgumentException($"Expected parameters {methodParameters.Length}. Provided parameters {parameters.Length}");
            }

            var typesafeParameters = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameterInfo = methodParameters[i];
                var value = parameters[i];
                if (value == null || value.GetType() == parameterInfo.ParameterType)
                {
                    typesafeParameters[i] = value;
                }
                else
                {
                    try
                    {
                        typesafeParameters[i] = Convert.ChangeType(value, parameterInfo.ParameterType);
                    }
                    catch
                    {
                        try
                        {
                            var temp = JsonConvert.SerializeObject(value);
                            typesafeParameters[i] = JsonConvert.DeserializeObject(temp, parameterInfo.ParameterType);

                        }
                        catch
                        {
                            throw new ArgumentException($"Tried to convert value {value} with type {value.GetType()} to type {parameterInfo.ParameterType}");
                        }
                    }
                }
            }

            return typesafeParameters;
        }

        public void TryInvoke(object[] parameters)
        {
            try
            {
                Invoke(parameters);
            }
            catch { }
        }

        public void Invoke(object[] parameters)
        {
            var typesafeParameters = ConvertParameters(parameters);
            MethodInfo.Invoke(Receiver, typesafeParameters);
            InvocationCount++;
        }
    }

    public class WaitForHandler
    {
        public string Name { get; set; }
        public ManualResetEvent ResetEvent { get; set; } = new ManualResetEvent(false);
        public object[] Result { get; set; }
        private MessageBus MessageBus { get; set; }
        public bool SignalReceived { get; set; }

        public WaitForHandler(string name, MessageBus messageBus)
        {
            Name = name;
            MessageBus = messageBus;
        }

        public bool DoWait(TimeSpan timeout)
        {
            if (SignalReceived)
            {
                return true;
            }

            return ResetEvent.WaitOne(timeout) || SignalReceived;
        }

        public T ConvertResult<T>()
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(Result[0]));
        }
    }
}
