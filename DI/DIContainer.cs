using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DI {
    /// <summary>
    /// Exception thrown when trying to <see cref="IContainer.Resolve"/>
    /// a Class or Interface which is not known to the Container.
    /// </summary>
    [Serializable()]
    public class InterfaceNotRegisteredException : Exception {
        /// <summary>
        /// Constructor.
        /// </summary>
        public InterfaceNotRegisteredException() : base() { }

        /// <summary>
        /// Serializable constructor.
        /// </summary>
        /// <param name="info">N/A</param>
        /// <param name="context">N/A</param>
        protected InterfaceNotRegisteredException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }

    /// <summary>
    /// Exception thrown when <see cref="IContainer.Resolve"/>
    /// would create a cycle (endless loop).
    /// </summary>
    [Serializable()]
    public class InfiniteInjectionException : Exception {
        /// <summary>
        /// Constructor.
        /// </summary>
        public InfiniteInjectionException() : base() { }

        /// <summary>
        /// Serializable constructor.
        /// </summary>
        /// <param name="info">N/A</param>
        /// <param name="context">N/A</param>
        protected InfiniteInjectionException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }

    /// <summary>
    /// Attribute used to prioritize a specific Constructor or Property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Property)]
    public class DInject : Attribute { }

    /// <summary>
    /// A generic delegate used to "store" compiled lambda expressions
    /// for constructors.
    /// </summary>
    /// <param name="args">Parameters to pass to constructor.</param>
    /// <returns>Constructed instance.</returns>
    delegate object ObjectActivator(object[] args);

    /// <summary>
    /// Main interface used to manipulate the Container from outside world.
    /// </summary>
    public interface IContainer {
        /// <summary>
        /// Registers a dependency by concrete Class.
        /// <see cref="IContainer.Resolve"/> will then work on <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Dependency type.</typeparam>
        /// <param name="singleton">
        /// If true, using <see cref="IContainer.Resolve"/> on <typeparamref name="T"/>
        /// will always return the same instance.
        /// </param>
        /// <returns>Container instance (for chaining).</returns>
        IContainer RegisterType<T>(bool singleton);
        /// <summary>
        /// Registers a dependency by Interface.
        /// <see cref="IContainer.Resolve"/> will then work on <typeparamref name="From"/>.
        /// </summary>
        /// <typeparam name="From">Interface to derive from.</typeparam>
        /// <typeparam name="To">
        /// Implementation to supply when using <see cref="IContainer.Resolve"/>
        /// on <typeparamref name="From"/>.
        /// </typeparam>
        /// <param name="singleton">
        /// If true, using <see cref="IContainer.Resolve"/> on <typeparamref name="From"/>
        /// will always return the same instance.
        /// </param>
        /// <returns>Container instance (for chaining).</returns>
        IContainer RegisterType<From, To>(bool singleton) where To : From;
        /// <summary>
        /// Registers a dependency by concrete Instance.
        /// </summary>
        /// <typeparam name="T">Instance type.</typeparam>
        /// <param name="instance">An object of type <typeparamref name="T"/>.</param>
        /// <returns>Container instance (for chaining).</returns>
        IContainer RegisterInstance<T>(T instance);

        /// <summary>
        /// Returns an instance of <typeparamref name="T"/> based on rules defined with
        /// <see cref="IContainer.RegisterType{T}"/> or <see cref="IContainer.RegisterInstance"/>.
        /// </summary>
        /// <seealso cref="IContainer.RegisterType{From,To}"/>
        /// <exception cref="InterfaceNotRegisteredException">
        /// When <typeparamref name="T"/> is not Registered.
        /// </exception>
        /// <exception cref="InfiniteInjectionException">
        /// When <typeparamref name="T"/> dependency tree is not a tree (i.e. has cycles).
        /// </exception>
        /// <typeparam name="T">Type to resolve.</typeparam>
        /// <returns>An instance of type <typeparamref name="T"/>.</returns>
        T Resolve<T>();
        /// <summary>
        /// Resolve dependencies of an existing object instance.
        /// </summary>
        /// <remarks>Works only with properties.</remarks>
        /// <typeparam name="T">Instance type.</typeparam>
        /// <param name="instance">An object of type <typeparamref name="T"/>.</param>
        void BuildUp<T>(T instance);
    }

    /// <summary>
    /// Implementation of <see cref="IContainer"/>.
    /// </summary>
    public class Container : IContainer {
        /// <summary>
        /// Stores produced singleton instances
        /// and instances registered with <see cref="IContainer.RegisterInstance"/>.
        /// </summary>
        private Dictionary<Type, object> _singletons;
        /// <summary>
        /// Stores compiled lambda expressions for constructors of all registered types.
        /// </summary>
        private Dictionary<Type, Tuple<ObjectActivator, ParameterInfo[]>> _registers;
        /// <summary>
        /// Used by <see cref="Container.Resolve"/> for preventing cycles.
        /// <see cref="Container.Resolve"/> stores every produced instance here
        /// and then checks if we are not trying to produce it again.
        /// </summary>
        /// <remarks>Has to be cleared before every <see cref="Container.Resolve"/>.</remarks>
        private List<Type> _temp;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Container() {
            this._singletons = new Dictionary<Type, object>();
            this._registers = new Dictionary<Type, Tuple<ObjectActivator, ParameterInfo[]>>();
            this._temp = new List<Type>();
        }

        /// <summary>
        /// Compiles a lambda expression for constructor of object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to compile constructor for.</typeparam>
        /// <returns>Tuple of <see cref="ObjectActivator"/> and constructor parameters.</returns>
        private Tuple<ObjectActivator, ParameterInfo[]> CompileConstructor<T>() {
            ConstructorInfo constructor = null;
            ParameterInfo[] parameters = null;
            int i = -1;
            foreach(ConstructorInfo cinfo in typeof(T).GetConstructors()) {
                ParameterInfo[] pinfo = cinfo.GetParameters();
                if(cinfo.GetCustomAttributes<DInject>(false).Count() == 1) {
                    constructor = cinfo;
                    parameters = pinfo;
                    break;
                }
                if(pinfo.Length > i) {
                    i = pinfo.Length;
                    constructor = cinfo;
                    parameters = pinfo;
                }
            }

            ParameterExpression param = Expression.Parameter(typeof(object[]), "args");
            Expression[] argsExp = new Expression[parameters.Length];
            for(i = 0; i < parameters.Length; i++) {
                Expression paramAccessorExp = Expression.ArrayIndex(param, Expression.Constant(i));
                argsExp[i] = Expression.Convert(paramAccessorExp, parameters[i].ParameterType);
            }
            NewExpression newExp = Expression.New(constructor, argsExp);
            LambdaExpression lambda = Expression.Lambda(typeof(ObjectActivator), newExp, param);
            ObjectActivator compiled = (ObjectActivator)lambda.Compile();
            return new Tuple<ObjectActivator, ParameterInfo[]>(compiled, parameters);
        }

        /// <summary>
        /// Places instance in <see cref="Container._singletons"/> array when needed.
        /// </summary>
        /// <typeparam name="T">Type of (possible) singleton.</typeparam>
        /// <param name="singleton">Instance to place in <see cref="Container._singletons"/>.</param>
        private void Singletonize<T>(bool singleton) {
            if(singleton) {
                this._singletons[typeof(T)] = null;
            } else {
                this._singletons.Remove(typeof(T));
            }
        }

        /// <summary>
        /// Implemented from <see cref="IContainer.RegisterType{T}"/>.
        /// </summary>
        public IContainer RegisterType<T>(bool singleton) {
            this._registers[typeof(T)] = this.CompileConstructor<T>();
            this.Singletonize<T>(singleton);
            return this;
        }

        /// <summary>
        /// Implemented from <see cref="IContainer.RegisterType{From,To}"/>.
        /// </summary>
        public IContainer RegisterType<From, To>(bool singleton) where To : From {
            this._registers[typeof(From)] = this.CompileConstructor<To>();
            this.Singletonize<From>(singleton);
            return this;
        }

        /// <summary>
        /// Implemented from <see cref="IContainer.RegisterInstance"/>.
        /// </summary>
        public IContainer RegisterInstance<T>(T instance) {
            this._singletons[typeof(T)] = instance;
            return this;
        }

        /// <summary>
        /// Creates instance of type <paramref name="type"/> by resolving it's parameters and
        /// calling a compiled lambda expression stored in <see cref="Container._registers"/>.
        /// </summary>
        /// <param name="type">Type to create instance for.</param>
        /// <returns>Instance of type <paramref name="type"/>.</returns>
        private object CreateInstance(Type type) {
            this._temp.Add(type);
            Tuple<ObjectActivator, ParameterInfo[]> info = this._registers[type];
            ParameterInfo[] parameters = info.Item2;
            object[] values = new object[parameters.Length];
            for(int i = 0; i < parameters.Length; ++i) {
                values[i] = this.Resolve(parameters[i].ParameterType);
            }
            var obj = info.Item1(values);
            this.ResolveProperties(obj);
            return obj;
        }

        /// <summary>
        /// Internal, non-generic, resolver which checks for possible exceptions
        /// and, if non were found, returns either a stored singleton
        /// or creates a new instance using <see cref="Container.CreateInstance"/>.
        /// </summary>
        /// <param name="type">Type to create instance for.</param>
        /// <returns>Instance of type <paramref name="type"/>.</returns>
        private object Resolve(Type type) {
            if(!this._registers.ContainsKey(type) && !this._singletons.ContainsKey(type)) {
                throw new InterfaceNotRegisteredException();
            }
            if(this._singletons.ContainsKey(type)) {
                if(this._singletons[type] == null) {
                    if(this._temp.Contains(type)) {
                        throw new InfiniteInjectionException();
                    }
                    this._singletons[type] = this.CreateInstance(type);
                }
                return this._singletons[type];
            }
            if(this._temp.Contains(type)) {
                throw new InfiniteInjectionException();
            }
            return this.CreateInstance(type);
        }

        /// <summary>
        /// Resolves injectable properties.
        /// Used for both creation and build up.
        /// </summary>
        /// <remarks>
        /// Injectable properties are those with <see cref="DInject"/> attribute.
        /// </remarks>
        /// <param name="obj">Instance for which to resolve properties.</param>
        private void ResolveProperties(object obj) {
            foreach(PropertyInfo pinfo in obj.GetType().GetProperties()) {
                MethodInfo setter = pinfo.GetSetMethod();
                if(setter != null && pinfo.GetCustomAttributes<DInject>(false).Count() == 1) {
                    setter.Invoke(obj, new[] { this.Resolve(pinfo.PropertyType) });
                }
            }
        }

        /// <summary>
        /// Implemented from <see cref="IContainer.Resolve"/>.
        /// </summary>
        public T Resolve<T>() {
            this._temp.Clear();
            return (T)this.Resolve(typeof(T));
        }

        /// <summary>
        /// Implemented from <see cref="IContainer.BuildUp"/>.
        /// </summary>
        public void BuildUp<T>(T instance) {
            this._temp.Clear();
            this.ResolveProperties(instance);
        }
    }

    /// <summary>
    /// Global singleton providing simplified interface over <see cref="IContainer"/>
    /// without need to store it's instance by application itself.
    /// </summary>
    /// <remarks>
    /// You can access <see cref="IContainer"/> instance directly
    /// using <see cref="ServiceLocator.GetInstance"/>.
    /// </remarks>
    public class ServiceLocator {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        private static ServiceLocator _instance;
        /// <summary>
        /// Multithreading lock.
        /// </summary>
        private static readonly object _padlock = new Object();

        ServiceLocator() { }

        /// <summary>
        /// Constructs (if needed) and returns a singleton instance of <see cref="ServiceLocator"/>.
        /// </summary>
        public static ServiceLocator Current {
            get {
                if(_instance == null) {
                    lock(_padlock) {
                        if(_instance == null) {
                            _instance = new ServiceLocator();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// IContainer provider function.
        /// Makes container supply more flexible, e.g.
        ///
        /// var container = new Container; () => return container;
        /// will encapsulate and return always the same instance.
        ///
        /// () => return new Container();
        /// will use new instance for every <see cref="ServiceLocator.GetInstance"/> call.
        /// </summary>
        private static Func<IContainer> _provider;

        /// <summary>
        /// Sets <see cref="ServiceLocator._provider"/>.
        /// </summary>
        /// <param name="ContainerProvider">Provider function.</param>
        public static void SetContainerProvider(Func<IContainer> ContainerProvider) {
            _provider = ContainerProvider;
        }

        /// <summary>
        /// Gets an instance of <typeparamref name="T"/> using container
        /// previously defined by <see cref="ServiceLocator.SetContainerProvider"/>.
        /// </summary>
        /// <remarks>Can also be used to get current container instance.</remarks>
        /// <typeparam name="T">Type for which to get an instance.</typeparam>
        /// <returns>An instance of type <typeparamref name="T"/>.</returns>
        public T GetInstance<T>() {
            if(typeof(IContainer).IsAssignableFrom(typeof(T))) {
                return (T)_provider();
            }
            return _provider().Resolve<T>();
        }
    }
}
