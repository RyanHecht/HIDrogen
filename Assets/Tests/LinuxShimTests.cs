#if UNITY_STANDALONE_LINUX
using HIDrogen.Backend;
using NUnit.Framework;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace HIDrogen.Tests
{
    public class LinuxShimTests : InputTestFixture
    {
        [InputControlLayout]
        private class ExplicitLinuxJoystick : Joystick
        {
        }

        public override void Setup()
        {
            base.Setup();
            InputSystem.RegisterLayout<ExplicitLinuxJoystick>(matches: new InputDeviceMatcher()
                .WithInterface("Linux").WithProduct("Explicit Linux Joystick"));
        }

        public override void TearDown()
        {
            LinuxShim.Uninitialize();
            base.TearDown();
        }

        [Test]
        public void ExistingExplicitLayoutSurvivesShimInitialization()
        {
            int id = InputSystem.AddDevice(Description("Explicit Linux Joystick")).deviceId;
            Assert.That(InputSystem.GetDeviceById(id), Is.InstanceOf<ExplicitLinuxJoystick>());

            LinuxShim.Initialize();
            InputSystem.Update();

            Assert.That(InputSystem.GetDeviceById(id), Is.InstanceOf<ExplicitLinuxJoystick>());
        }

        [Test]
        public void ExplicitLayoutCanConnectAfterShimInitialization()
        {
            LinuxShim.Initialize();
            InputSystem.Update();

            var device = InputSystem.AddDevice(Description("Explicit Linux Joystick"));

            Assert.That(device, Is.InstanceOf<ExplicitLinuxJoystick>());
        }

        [Test]
        public void GenericLinuxJoystickIsStillRemoved()
        {
            int id = InputSystem.AddDevice(Description("Generic Linux Joystick")).deviceId;
            Assert.That(InputSystem.GetDeviceById(id), Is.InstanceOf<Joystick>());

            LinuxShim.Initialize();
            InputSystem.Update();

            Assert.That(InputSystem.GetDeviceById(id), Is.Null);
        }

        [Test]
        public void GenericLinuxJoystickIsStillRejectedAfterInitialization()
        {
            LinuxShim.Initialize();
            InputSystem.Update();

            Assert.Throws<InputControlLayout.LayoutNotFoundException>(() =>
                InputSystem.AddDevice(Description("Generic Linux Joystick")));
        }

        [Test]
        public void OtherInterfacesAreUnaffected()
        {
            var description = Description("Other Joystick");
            description.interfaceName = "Other";
            var device = InputSystem.AddDevice(description);

            LinuxShim.Initialize();
            InputSystem.Update();

            Assert.That(InputSystem.devices, Does.Contain(device));
        }

        private static InputDeviceDescription Description(string product) => new()
        {
            interfaceName = "Linux",
            deviceClass = "Joystick",
            product = product,
            capabilities = "{\"controls\":[]}"
        };
    }
}
#endif
