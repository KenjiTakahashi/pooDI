**pooDI** is a Dependency Injection Container for C#. While created mainly for learning purpoes, it supports all basic DI features and is quite efficient at it.

## usage

#### container
```c#
interface ISimpleClass { }
class SimpleClass1 : ISimpleClass { }
class SimpleClass2 { }
class SimpleClass3 {
    public SimpleClass2 {get;set;}
}

var container = new DI.Container();

container.RegisterType<SimpleClass1>(false); // Register a concrete class
container.RegisterType<ISimpleClass, SimpleClass1>(false); // Register an interface implementation
container.RegisterType<SimpleClass2>(true); // Register a singleton

var simpleClass3 = new SimpleClass3();
container.RegisterInstance<SimpleClass3>(simpleClass3); // Register existing instance

var simpleClass1 = container.Resolve<SimpleClass1>(); // Resolve a concrete class
var iSimpleClass1 = container.Resolve<ISimpleClass1>(); // Resolve an interface implementation

var simpleClass3 = new SimpleClass3();
container.BuildUp<SimpleClass3>(simpleClass3); // Resolve existing object's properties

container.RegisterType<SimpleClass1>(false).RegisterType<SimpleClass2>(true); // Methods can be chained
```

#### service locator
A global, simplified resolver.
```c#
var container = new DI.Container();

var serviceLocator = DI.ServiceLocator.Current; // Get a service locator instance
DI.ServiceLocator.setContainerProvider(() => container); // Set a container provider

var simpleClass = serviceLocator.GetInstance<ISimpleClass>(); // Get object, as registered in container

var serviceContainer = serviceLocator.GetInstance<IContainer>(); // Get back current container instance
```

#### mono?
Nope. It can be adjusted to work, but I am not wiling to do so.

## confrontation
There is also a Confrontation project, providing a few simple benchmarks over well-known DI/IoC implementations, including Unity, Ninject, Autofac and Castle Windsor.
