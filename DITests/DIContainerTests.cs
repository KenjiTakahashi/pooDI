using System;
using NUnit.Framework;
using DI;

namespace DITests {
    interface ITestClass { }
    class TestClass : ITestClass { }

    [TestFixture]
    public class ContainerBasicTests {
        [Test]
        public void Should_properly_resolve_types() {
            IContainer c = new Container();
            c.RegisterType<ITestClass, TestClass>(false);

            var result = c.Resolve<ITestClass>();

            Assert.IsInstanceOf<TestClass>(result);
        }

        [Test]
        public void Should_properly_create_singletons_per_interface() {
            IContainer c = new Container();
            c.RegisterType<ITestClass, TestClass>(true);

            var result1 = c.Resolve<ITestClass>();
            var result2 = c.Resolve<ITestClass>();

            Assert.AreSame(result1, result2);
        }

        [Test]
        public void Should_properly_create_singleton_per_concrete_class() {
            IContainer c = new Container();
            c.RegisterType<TestClass>(true);

            var result1 = c.Resolve<TestClass>();
            var result2 = c.Resolve<TestClass>();

            Assert.AreSame(result1, result2);
        }

        [Test]
        [ExpectedException(typeof(InterfaceNotRegisteredException))]
        public void Should_throw_exception_on_not_registered_type() {
            IContainer c = new Container();

            c.Resolve<TestClass>();
        }

        [Test]
        public void Should_return_registered_instance_per_interface() {
            IContainer c = new Container();
            ITestClass instance = new TestClass();
            c.RegisterInstance<ITestClass>(instance);

            var result = c.Resolve<ITestClass>();

            Assert.AreSame(instance, result);
        }

        [Test]
        public void Should_return_registered_instance_per_concrete_class() {
            IContainer c = new Container();
            TestClass instance = new TestClass();
            c.RegisterInstance<TestClass>(instance);

            var result = c.Resolve<TestClass>();

            Assert.AreSame(instance, result);
        }
    }

    class TestClass2 {
        public TestClass t { get; set; }

        public TestClass2(TestClass t) {
            this.t = t;
        }
    }

    class TestClass3 {
        public ITestClass t { get; set; }

        public TestClass3(ITestClass t) {
            this.t = t;
        }
    }

    class TestClass4 {
        public int a { get; set; }
        public TestClass4(int a, string b) { }
        public TestClass4(string a, int b, string c) {
            this.a = 3;
        }
    }

    class TestClass5 {
        public TestClass t { get; set; }

        public TestClass5(int a, string b) { }
        public TestClass5(string a, int b, string c) { }
        [DInject()]
        public TestClass5(TestClass t) {
            this.t = t;
        }
    }

    class TestClass6 {
        public TestClass6(TestClass7 t) { }
    }

    class TestClass7 {
        public TestClass6 t { get; set; }

        public TestClass7() { }
        public TestClass7(TestClass6 t) {
            this.t = t;
        }
    }

    [TestFixture]
    public class ContainerConstructorDITests {
        [Test]
        public void Should_resolve_constructor_parameters_per_interface() {
            IContainer c = new Container();
            c.RegisterType<ITestClass, TestClass>(false);
            c.RegisterType<TestClass3>(false);

            var result = c.Resolve<TestClass3>();

            Assert.IsInstanceOf<TestClass3>(result);
            Assert.IsInstanceOf<TestClass>(result.t);
        }

        [Test]
        public void Should_resolve_constructor_parameters_per_concrete_class() {
            IContainer c = new Container();
            c.RegisterType<TestClass>(false);
            c.RegisterType<TestClass2>(false);

            var result = c.Resolve<TestClass2>();

            Assert.IsInstanceOf<TestClass2>(result);
            Assert.IsInstanceOf<TestClass>(result.t);
        }

        [Test]
        [ExpectedException(typeof(InterfaceNotRegisteredException))]
        public void Should_throw_exception_on_not_registered_type() {
            IContainer c = new Container();
            c.RegisterType<TestClass2>(false);

            c.Resolve<TestClass2>();
        }

        [Test]
        public void Should_prioritize_longest_constructor() {
            IContainer c = new Container();
            c.RegisterInstance<string>("a");
            c.RegisterInstance<int>(1);
            c.RegisterType<TestClass4>(false);

            var result = c.Resolve<TestClass4>();

            Assert.AreEqual(3, result.a);
        }

        [Test]
        public void Should_prioritize_attributed_constructor() {
            IContainer c = new Container();
            c.RegisterType<TestClass>(false);
            c.RegisterType<TestClass5>(false);

            var result = c.Resolve<TestClass5>();

            Assert.IsNotNull(result.t);
            Assert.IsInstanceOf<TestClass>(result.t);
        }

        [Test]
        [ExpectedException(typeof(InfiniteInjectionException))]
        public void Should_deal_with_cycles() {
            IContainer c = new Container();
            c.RegisterType<TestClass6>(false);
            c.RegisterType<TestClass7>(false);

            c.Resolve<TestClass6>();
        }

        [Test]
        [ExpectedException(typeof(InfiniteInjectionException))]
        public void Should_deal_with_cycles_singletons() {
            IContainer c = new Container();
            c.RegisterType<TestClass6>(true);
            c.RegisterType<TestClass7>(true);

            c.Resolve<TestClass7>();
        }

        [Test]
        public void Should_resolve_cycle_if_instance_is_supplied() {
            IContainer c = new Container();
            TestClass6 cls = new TestClass6(new TestClass7());
            c.RegisterInstance<TestClass6>(cls);
            c.RegisterType<TestClass7>(false);

            var result = c.Resolve<TestClass7>();

            Assert.IsInstanceOf<TestClass7>(result);
            Assert.AreSame(cls, result.t);
        }
    }

    class PropertyTest1 { }

    class PropertyTest2 {
        [DInject]
        public PropertyTest1 t { get; set; }
    }

    class PropertyTest3 {
        public PropertyTest1 t { get; set; }
    }

    class PropertyTest4 {
        [DInject]
        public PropertyTest1 t { get; protected set; }
    }

    [TestFixture]
    public class ContainerPropertyDITests {
        [Test]
        public void Should_properly_resolve_attributed_properties() {
            IContainer c = new Container();
            c.RegisterType<PropertyTest1>(false);
            c.RegisterType<PropertyTest2>(false);

            var result = c.Resolve<PropertyTest2>();

            Assert.IsInstanceOf<PropertyTest2>(result);
            Assert.IsInstanceOf<PropertyTest1>(result.t);
        }

        [Test]
        public void Should_ignore_not_attributed_properties() {
            IContainer c = new Container();
            c.RegisterType<PropertyTest1>(false);
            c.RegisterType<PropertyTest3>(false);

            var result = c.Resolve<PropertyTest3>();

            Assert.IsInstanceOf<PropertyTest3>(result);
            Assert.IsNull(result.t);
        }

        [Test]
        public void Should_ignore_properties_with_non_public_setters() {
            IContainer c = new Container();
            c.RegisterType<PropertyTest1>(false);
            c.RegisterType<PropertyTest4>(false);

            var result = c.Resolve<PropertyTest4>();

            Assert.IsInstanceOf<PropertyTest4>(result);
            Assert.IsNull(result.t);
        }
    }

    [TestFixture]
    public class ContainerBuildUpTests {
        [Test]
        public void Should_resolve_attributed_properties_for_existing_objects() {
            IContainer c = new Container();
            c.RegisterType<PropertyTest1>(false);
            PropertyTest2 obj = new PropertyTest2();

            c.BuildUp<PropertyTest2>(obj);

            Assert.IsInstanceOf<PropertyTest1>(obj.t);
        }
    }

    [TestFixture]
    public class ServiceLocatorTests {
        [Test]
        public void Should_properly_resolve_types() {
            //For more specific tests, see Container<>Tests fixtures.
            IContainer c = new Container();
            c.RegisterType<ITestClass, TestClass>(false);
            ServiceLocator.SetContainerProvider(() => c);
            ServiceLocator l = ServiceLocator.Current;

            var result = l.GetInstance<ITestClass>();

            Assert.IsInstanceOf<TestClass>(result);
        }

        [Test]
        public void Should_properly_resolve_container() {
            ServiceLocator.SetContainerProvider(() => new Container());
            ServiceLocator l = ServiceLocator.Current;

            var result = l.GetInstance<Container>();

            Assert.IsInstanceOf<Container>(result);
        }
    }

    [TestFixture]
    public class ContainerChainingTests {
        [Test]
        public void Should_properly_chain_method_calls() {
            //Just a really brief test, there is nothing fancy there.
            PropertyTest2 tc = new PropertyTest2();
            IContainer c = new Container()
                .RegisterType<ITestClass, TestClass>(false)
                .RegisterType<TestClass3>(false)
                .RegisterType<PropertyTest1>(true)
                .RegisterInstance<PropertyTest2>(tc);

            var result1 = c.Resolve<PropertyTest2>();
            var result2 = c.Resolve<TestClass3>();

            Assert.IsInstanceOf<PropertyTest2>(result1);
            Assert.IsNull(result1.t);
            Assert.IsInstanceOf<TestClass3>(result2);
            Assert.IsInstanceOf<TestClass>(result2.t);
        }
    }
}
