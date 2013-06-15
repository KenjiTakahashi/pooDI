using Autofac;
using Autofac.Core.Registration;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Microsoft.Practices.Unity;
using Ninject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Confrontation {
    interface ITestClass1 { }

    class TestClass1 : ITestClass1 { }

    class TestClass2 {
        private ITestClass1 t { get; set; }

        public TestClass2(ITestClass1 t) {
            this.t = t;
        }
    }

    class TestClass3 {
        private TestClass2 t { get; set; }

        public TestClass3(TestClass2 t) {
            this.t = t;
        }
    }

    class TestClass4 {
        private TestClass3 t { get; set; }

        public TestClass4(TestClass3 t) {
            this.t = t;
        }
    }

    class TestClass5 {
        [DI.DInject]
        [Dependency]
        [Inject]
        public ITestClass1 t { get; set; }
    }

    class Program {
        static int n = 1;
        static Stopwatch watch = new Stopwatch();

        static void NonRegistered() {
            Console.WriteLine("Straight:\tNot supported");
            Console.Write("pooDI:\t\t");
            watch.Start();
            var pooDI = new DI.Container();
            for(int i = 0; i < n; ++i) {
                try {
                    var c = pooDI.Resolve<ITestClass1>();
                } catch(DI.InterfaceNotRegisteredException) { }
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Unity:\t\t");
            watch.Start();
            var unity = new UnityContainer();
            for(int i = 0; i < n; ++i) {
                try {
                    var c = unity.Resolve<ITestClass1>();
                } catch(ResolutionFailedException) { }
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Ninject:\t");
            watch.Start();
            var kernel = new StandardKernel();
            for(int i = 0; i < n; ++i) {
                try {
                    var c = kernel.Get<ITestClass1>();
                } catch(ActivationException) { }
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Autofac:\t");
            watch.Start();
            var builder = new ContainerBuilder();
            var container = builder.Build();
            for(int i = 0; i < n; ++i) {
                try {
                    var c = container.Resolve<ITestClass1>();
                } catch(ComponentNotRegisteredException) { }
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Castle Windsor:\t");
            watch.Start();
            var windsor = new WindsorContainer();
            for(int i = 0; i < n; ++i) {
                try {
                    var c = windsor.Resolve<ITestClass1>();
                } catch(ComponentNotFoundException) { }
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();
        }

        static void Concrete() {
            Console.Write("Straight:\t");
            watch.Start();
            for(int i = 0; i < n; ++i) {
                var c = new TestClass1();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("pooDI:\t\t");
            watch.Start();
            var pooDI = new DI.Container();
            pooDI.RegisterType<TestClass1>(false);
            for(int i = 0; i < n; ++i) {
                var c = pooDI.Resolve<TestClass1>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Unity:\t\t");
            watch.Start();
            var unity = new UnityContainer();
            unity.RegisterType<TestClass1>();
            for(int i = 0; i < n; ++i) {
                var c = unity.Resolve<TestClass1>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Ninject:\t");
            watch.Start();
            var kernel = new StandardKernel();
            kernel.Bind<TestClass1>().ToSelf();
            for(int i = 0; i < n; ++i) {
                var c = kernel.Get<TestClass1>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Autofac:\t");
            watch.Start();
            var builder = new ContainerBuilder();
            builder.RegisterType<TestClass1>().AsSelf();
            var container = builder.Build();
            for(int i = 0; i < n; ++i) {
                var c = container.Resolve<TestClass1>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Castle Windsor:\t");
            watch.Start();
            var windsor = new WindsorContainer();
            windsor.Register(
                Component.For<TestClass1>()
                    .LifestyleTransient()
            );
            for(int i = 0; i < n; ++i) {
                var c = windsor.Resolve<TestClass1>();
                windsor.Release(c);
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();
        }

        static void Interface() {
            Console.Write("Straight:\t");
            watch.Start();
            for(int i = 0; i < n; ++i) {
                var c = new TestClass1();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("pooDI:\t\t");
            watch.Start();
            var pooDI = new DI.Container();
            pooDI.RegisterType<ITestClass1, TestClass1>(false);
            for(int i = 0; i < n; ++i) {
                var c = pooDI.Resolve<ITestClass1>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Unity:\t\t");
            watch.Start();
            var unity = new UnityContainer();
            unity.RegisterType<ITestClass1, TestClass1>();
            for(int i = 0; i < n; ++i) {
                var c = unity.Resolve<ITestClass1>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Ninject:\t");
            watch.Start();
            var kernel = new StandardKernel();
            kernel.Bind<ITestClass1>().To<TestClass1>();
            for(int i = 0; i < n; ++i) {
                var c = kernel.Get<ITestClass1>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Autofac:\t");
            watch.Start();
            var builder = new ContainerBuilder();
            builder.RegisterType<TestClass1>().As<ITestClass1>();
            var container = builder.Build();
            for(int i = 0; i < n; ++i) {
                var c = container.Resolve<ITestClass1>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Castle Windsor:\t");
            watch.Start();
            var windsor = new WindsorContainer();
            windsor.Register(
                Component.For<ITestClass1>()
                    .ImplementedBy<TestClass1>()
                    .LifestyleTransient()
            );
            for(int i = 0; i < n; ++i) {
                var c = windsor.Resolve<ITestClass1>();
                windsor.Release(c);
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();
        }

        static void ConstructorInjection() {
            Console.Write("Straight:\t");
            watch.Start();
            for(int i = 0; i < n; ++i) {
                var c = new TestClass1();
                var d = new TestClass2(c);
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("pooDI:\t\t");
            watch.Start();
            var pooDI = new DI.Container();
            pooDI.RegisterType<ITestClass1, TestClass1>(false);
            pooDI.RegisterType<TestClass2>(false);
            for(int i = 0; i < n; ++i) {
                var c = pooDI.Resolve<TestClass2>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Unity:\t\t");
            watch.Start();
            var unity = new UnityContainer();
            unity.RegisterType<ITestClass1, TestClass1>();
            unity.RegisterType<TestClass2>();
            for(int i = 0; i < n; ++i) {
                var c = unity.Resolve<TestClass2>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Ninject:\t");
            watch.Start();
            var kernel = new StandardKernel();
            kernel.Bind<ITestClass1>().To<TestClass1>();
            kernel.Bind<TestClass2>().ToSelf();
            for(int i = 0; i < n; ++i) {
                var c = kernel.Get<TestClass2>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Autofac:\t");
            watch.Start();
            var builder = new ContainerBuilder();
            builder.RegisterType<TestClass1>().As<ITestClass1>();
            builder.RegisterType<TestClass2>().AsSelf();
            var container = builder.Build();
            for(int i = 0; i < n; ++i) {
                var c = container.Resolve<TestClass2>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Castle Windsor:\t");
            watch.Start();
            var windsor = new WindsorContainer();
            windsor.Register(
                Component.For<ITestClass1>()
                    .ImplementedBy<TestClass1>()
                    .LifestyleTransient(),
                Component.For<TestClass2>()
                    .LifestyleTransient()
            );
            for(int i = 0; i < n; ++i) {
                var c = windsor.Resolve<TestClass2>();
                windsor.Release(c);
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();
        }

        static void ChainingInjection() {
            Console.Write("Straight:\t");
            watch.Start();
            for(int i = 0; i < n; ++i) {
                var c = new TestClass1();
                var d = new TestClass2(c);
                var e = new TestClass3(d);
                var f = new TestClass4(e);
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("pooDI:\t\t");
            watch.Start();
            var pooDI = new DI.Container();
            pooDI.RegisterType<ITestClass1, TestClass1>(false);
            pooDI.RegisterType<TestClass2>(false);
            pooDI.RegisterType<TestClass3>(false);
            pooDI.RegisterType<TestClass4>(false);
            for(int i = 0; i < n; ++i) {
                var c = pooDI.Resolve<TestClass4>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Unity:\t\t");
            watch.Start();
            var unity = new UnityContainer();
            unity.RegisterType<ITestClass1, TestClass1>();
            unity.RegisterType<TestClass2>();
            unity.RegisterType<TestClass3>();
            unity.RegisterType<TestClass4>();
            for(int i = 0; i < n; ++i) {
                var c = unity.Resolve<TestClass4>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Ninject:\t");
            watch.Start();
            var kernel = new StandardKernel();
            kernel.Bind<ITestClass1>().To<TestClass1>();
            kernel.Bind<TestClass2>().ToSelf();
            kernel.Bind<TestClass3>().ToSelf();
            kernel.Bind<TestClass4>().ToSelf();
            for(int i = 0; i < n; ++i) {
                var c = kernel.Get<TestClass4>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Autofac:\t");
            watch.Start();
            var builder = new ContainerBuilder();
            builder.RegisterType<TestClass1>().As<ITestClass1>();
            builder.RegisterType<TestClass2>().AsSelf();
            builder.RegisterType<TestClass3>().AsSelf();
            builder.RegisterType<TestClass4>().AsSelf();
            var container = builder.Build();
            for(int i = 0; i < n; ++i) {
                var c = container.Resolve<TestClass4>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Castle Windsor:\t");
            watch.Start();
            var windsor = new WindsorContainer();
            windsor.Register(
                Component.For<ITestClass1>()
                    .ImplementedBy<TestClass1>()
                    .LifestyleTransient(),
                Component.For<TestClass2>()
                    .LifestyleTransient(),
                Component.For<TestClass3>()
                    .LifestyleTransient(),
                Component.For<TestClass4>()
                    .LifestyleTransient()
            );
            for(int i = 0; i < n; ++i) {
                var c = windsor.Resolve<TestClass4>();
                windsor.Release(c);
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();
        }

        static void Property() {
            Console.Write("Straight:\t");
            watch.Start();
            for(int i = 0; i < n; ++i) {
                var c = new TestClass5();
                c.t = new TestClass1();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("pooDI:\t\t");
            watch.Start();
            var pooDI = new DI.Container();
            pooDI.RegisterType<ITestClass1, TestClass1>(false);
            pooDI.RegisterType<TestClass5>(false);
            for(int i = 0; i < n; ++i) {
                var c = pooDI.Resolve<TestClass5>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Unity:\t\t");
            watch.Start();
            var unity = new UnityContainer();
            unity.RegisterType<ITestClass1, TestClass1>();
            unity.RegisterType<TestClass5>();
            for(int i = 0; i < n; ++i) {
                var c = unity.Resolve<TestClass5>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Ninject:\t");
            watch.Start();
            var kernel = new StandardKernel();
            kernel.Bind<ITestClass1>().To<TestClass1>();
            kernel.Bind<TestClass5>().ToSelf();
            for(int i = 0; i < n; ++i) {
                var c = kernel.Get<TestClass5>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Autofac:\t");
            watch.Start();
            var builder = new ContainerBuilder();
            builder.RegisterType<TestClass1>().As<ITestClass1>();
            builder.RegisterType<TestClass5>().AsSelf().OnActivated(e => e.Instance.t = e.Context.Resolve<ITestClass1>());
            var container = builder.Build();
            for(int i = 0; i < n; ++i) {
                var c = container.Resolve<TestClass5>();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Castle Windsor:\t");
            watch.Start();
            var windsor = new WindsorContainer();
            windsor.Register(
                Component.For<ITestClass1>()
                    .ImplementedBy<TestClass1>()
                    .LifestyleTransient(),
                Component.For<TestClass5>()
                    .LifestyleTransient()
            );
            for(int i = 0; i < n; ++i) {
                var c = windsor.Resolve<TestClass5>();
                windsor.Release(c);
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();
        }

        static void BuildUp() {
            Console.Write("Straight:\t");
            watch.Start();
            for(int i = 0; i < n; ++i) {
                var c = new TestClass5();
                c.t = new TestClass1();
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("pooDI:\t\t");
            watch.Start();
            var pooDI = new DI.Container();
            pooDI.RegisterType<ITestClass1, TestClass1>(false);
            for(int i = 0; i < n; ++i) {
                var c = new TestClass5();
                pooDI.BuildUp<TestClass5>(c);
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Unity:\t\t");
            watch.Start();
            var unity = new UnityContainer();
            unity.RegisterType<ITestClass1, TestClass1>();
            for(int i = 0; i < n; ++i) {
                var c = new TestClass5();
                unity.BuildUp<TestClass5>(c);
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Ninject:\t");
            watch.Start();
            var kernel = new StandardKernel();
            kernel.Bind<ITestClass1>().To<TestClass1>();
            for(int i = 0; i < n; ++i) {
                var c = new TestClass5();
                kernel.Inject(c);
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.Write("Autofac:\t");
            watch.Start();
            var builder = new ContainerBuilder();
            builder.RegisterType<TestClass1>().As<ITestClass1>();
            var container = builder.Build();
            for(int i = 0; i < n; ++i) {
                var c = new TestClass5();
                container.InjectProperties<TestClass5>(c);
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            watch.Reset();

            Console.WriteLine("Castle Windsor:\tNot supported");
        }

        static void Go() {
            Console.WriteLine("Resolving non-registered type:");
            NonRegistered();
            Console.WriteLine("\nResolving type registered through concrete implementation:");
            Concrete();
            Console.WriteLine("\nResolving type registered through interface:");
            Interface();
            Console.WriteLine("\nConstructor injection:");
            ConstructorInjection();
            Console.WriteLine("\nChaining injection (three objects):");
            ChainingInjection();
            Console.WriteLine("\nProperty injection:");
            Property();
            Console.WriteLine("\nBuild Up:");
            BuildUp();
        }

        static void Main(string[] args) {
            Console.WriteLine("Warm Up...");
            Concrete();
            Interface();
            Console.WriteLine("\n\nOne iteration:");
            Go();
            Console.WriteLine("\n\nTen iterations:");
            n = 10;
            Go();
            Console.WriteLine("\n\nTen thousand iterations:");
            n = 10000;
            Go();
            Console.ReadKey();
        }
    }
}
