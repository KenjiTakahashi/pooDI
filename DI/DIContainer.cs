using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DI {
    [Serializable()]
    public class InterfaceNotRegisteredException : Exception {
        public InterfaceNotRegisteredException() : base() { }

        protected InterfaceNotRegisteredException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }

    [Serializable()]
    public class InfiniteInjectionException : Exception {
        public InfiniteInjectionException() : base() { }

        protected InfiniteInjectionException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Property)]
    public class DInject : Attribute { }

    delegate object ObjectActivator(object[] args);

    public interface IContainer {
        IContainer RegisterType<T>(bool singleton);
        IContainer RegisterType<From, To>(bool singleton) where To : From;
        IContainer RegisterInstance<T>(T instance);

        T Resolve<T>();
        void BuildUp<T>(T instance);
    }

    public class Container : IContainer {
        private Dictionary<Type, object> _singletons;
        private Dictionary<Type, Tuple<ObjectActivator, ParameterInfo[]>> _registers;
        private List<Type> _temp;

        public Container() {
            this._singletons = new Dictionary<Type, object>();
            this._registers = new Dictionary<Type, Tuple<ObjectActivator, ParameterInfo[]>>();
            this._temp = new List<Type>();
        }

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

        private void Singletonize<T>(bool singleton) {
            if(singleton) {
                this._singletons[typeof(T)] = null;
            } else {
                this._singletons.Remove(typeof(T));
            }
        }

        public IContainer RegisterType<T>(bool singleton) {
            this._registers[typeof(T)] = this.CompileConstructor<T>();
            this.Singletonize<T>(singleton);
            return this;
        }

        public IContainer RegisterType<From, To>(bool singleton) where To : From {
            this._registers[typeof(From)] = this.CompileConstructor<To>();
            this.Singletonize<From>(singleton);
            return this;
        }

        public IContainer RegisterInstance<T>(T instance) {
            this._singletons[typeof(T)] = instance;
            return this;
        }

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

        private void ResolveProperties(object obj) {
            foreach(PropertyInfo pinfo in obj.GetType().GetProperties()) {
                MethodInfo setter = pinfo.GetSetMethod();
                if(setter != null && pinfo.GetCustomAttributes<DInject>(false).Count() == 1) {
                    setter.Invoke(obj, new[] { this.Resolve(pinfo.PropertyType) });
                }
            }
        }

        public T Resolve<T>() {
            this._temp.Clear();
            return (T)this.Resolve(typeof(T));
        }

        public void BuildUp<T>(T instance) {
            this._temp.Clear();
            this.ResolveProperties(instance);
        }
    }

    public delegate IContainer ContainerProviderDelegate();

    public class ServiceLocator {
        private static ServiceLocator _instance;
        private static readonly object _padlock = new Object();

        ServiceLocator() { }

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

        private static ContainerProviderDelegate _provider;

        public static void SetContainerProvider(ContainerProviderDelegate ContainerProvider) {
            _provider = ContainerProvider;
        }

        public T GetInstance<T>() {
            if(typeof(IContainer).IsAssignableFrom(typeof(T))) {
                return (T)_provider();
            }
            return _provider().Resolve<T>();
        }
    }
}
