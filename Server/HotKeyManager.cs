using Microsoft.EntityFrameworkCore;
using ScreenTemperature.Entities.Configurations;
using ScreenTemperature.Services;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScreenTemperature
{
    public class HotKeyManager(IServiceProvider serviceProvider, IScreenService screenService)
    {
        #region DLL import

        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public UIntPtr wParam;
            public UIntPtr lParam;
            public uint time;
            public POINT pt;
        }

        [Flags]
        public enum KeyModifiers : uint
        {
            Alt = 1,
            Control = 2,
            Shift = 4
        }

        [DllImport("user32.dll")]
        private static extern uint GetQueueStatus(uint flags);

        [DllImport("user32.dll")]
        private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        #endregion

        private BlockingCollection<(uint virtualKeyCode, bool alt, bool control, bool shift, TaskCompletionSource<bool> taskCompletionSource)> _hotkeysToRegister = new BlockingCollection<(uint virtualKeyCode, bool alt, bool control, bool shift, TaskCompletionSource<bool> taskCompletionSource)>();
        private BlockingCollection<(uint virtualKeyCode, bool alt, bool control, bool shift, TaskCompletionSource<bool> taskCompletionSource)> _hotkeysToUnregister = new BlockingCollection<(uint virtualKeyCode, bool alt, bool control, bool shift, TaskCompletionSource<bool> taskCompletionSource)>();

        public async Task InitAsync()
        {
            _ = Task.Factory.StartNew(RunAsync);// start listener

            using (var scope = serviceProvider.CreateScope())
            {
                var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var keyBindings = await databaseContext.KeyBindings.ToListAsync();

                foreach (var keyBinding in keyBindings)
                {
                    await RegisterHotKeyAsync(keyBinding.KeyCode, keyBinding.Alt, keyBinding.Control, false);
                }
            }
        }

        private async Task RunAsync()
        {
            MSG msg;

            while (true)
            {
                try
                { 
                    // if an unregistration is in queue
                    if (_hotkeysToUnregister.Any())
                    {
                        var keyCodeToUnregister = _hotkeysToUnregister.Take();

                        UnregisterHotKey(IntPtr.Zero, GenerateHotKeyId(keyCodeToUnregister.virtualKeyCode, keyCodeToUnregister.alt, keyCodeToUnregister.control, keyCodeToUnregister.shift));

                        // send result
                        keyCodeToUnregister.taskCompletionSource.SetResult(true);
                    }

                    // if a registration is in queue
                    if (_hotkeysToRegister.Any())
                    {
                        var keyCodeToRegister = _hotkeysToRegister.Take();

                        var mask = keyCodeToRegister.alt ? (uint)KeyModifiers.Alt : 0;
                        mask = mask | (keyCodeToRegister.control ? (uint)KeyModifiers.Control : 0);
                        mask = mask | (keyCodeToRegister.shift ? (uint)KeyModifiers.Shift : 0);

                        var hotkeyRegistered = RegisterHotKey(IntPtr.Zero, GenerateHotKeyId(keyCodeToRegister.virtualKeyCode, keyCodeToRegister.alt, keyCodeToRegister.control, keyCodeToRegister.shift), mask, keyCodeToRegister.virtualKeyCode);

                        // send result
                        keyCodeToRegister.taskCompletionSource.SetResult(hotkeyRegistered);
                    }

                    // 0x0080 = QS_HOTKEY -> A WM_HOTKEY message is in the queue.
                    var availableMessages = GetQueueStatus(0x0080);// Get hotkey messages

                    if (availableMessages >> 16 == 0x0080)
                    {
                        PeekMessage(out msg, IntPtr.Zero, 0x312, 0x312, 1);// message received : WM_HOTKEY 0x312

                        var keyCode = ExtractKeyCodeFromHotKeyId(msg.wParam);
                        var altWasPressed = ((KeyModifiers)msg.lParam & KeyModifiers.Alt) == KeyModifiers.Alt;
                        var controlWasPressed = ((KeyModifiers)msg.lParam & KeyModifiers.Control) == KeyModifiers.Control;
                        var shiftWasPressed = ((KeyModifiers)msg.lParam & KeyModifiers.Shift) == KeyModifiers.Shift;

                        using (var scope = serviceProvider.CreateScope())
                        {
                            var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                            var matchingBinding = databaseContext.KeyBindings.Include(x => x.Configurations).SingleOrDefault(x => x.KeyCode == keyCode && x.Alt == altWasPressed && x.Control == controlWasPressed);

                            if (matchingBinding != null)
                            {
                                foreach(var config in matchingBinding.Configurations)
                                {
                                    var result = false;
                                        
                                    if (config.ApplyBrightness)
                                    {
                                        try
                                        {
                                            result = await screenService.ApplyBrightnessToScreenAsync(config.Brightness, config.DevicePath);
                                        }
                                        catch (Exception ex) { }

                                        //await Clients.All.SendAsync("ApplyTemperatureResult", result);

                                    }

                                    if (config is ColorConfiguration colorConfiguration)
                                    {
                                        if(colorConfiguration.ApplyColor)
                                        {
                                            result = false;

                                            try
                                            {
                                                result = await screenService.ApplyColorToScreenAsync(colorConfiguration.Color, config.DevicePath);
                                            }
                                            catch (Exception ex) { }
                                        }
                                    }
                                    else if(config is TemperatureConfiguration temperatureConfiguration)
                                    {
                                        if (temperatureConfiguration.ApplyIntensity)
                                        {
                                            result = false;

                                            try
                                            {
                                                result = await screenService.ApplyKelvinToScreenAsync(temperatureConfiguration.Intensity, config.DevicePath);
                                            }
                                            catch (Exception ex) { }
                                        }
                                    }
                                    else
                                    {
                                        throw new NotImplementedException();
                                    }
                                }
                            }
                        }
                    }

                }
                catch(Exception ex) { }

                await Task.Delay(200);
            }
        }

        // generate a unique Id for a key
        private int GenerateHotKeyId(uint keycode, bool alt, bool control, bool shift)
        {
            var mask = (int)keycode << 3;
            if (alt) mask = mask | (int)KeyModifiers.Alt;
            if (control) mask = mask | (int)KeyModifiers.Control;
            if (shift) mask = mask | (int)KeyModifiers.Shift;

            return mask;
        }

        private int ExtractKeyCodeFromHotKeyId(nuint id)
        {
            return (int)id.ToUInt32() >> 3;
        }

        public async Task<bool> UnregisterHotKeyAsync(int virtualKeyCode, bool alt, bool control, bool shift)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            _hotkeysToUnregister.Add(((uint)virtualKeyCode, alt, control, shift, taskCompletionSource));

            return await taskCompletionSource.Task;
        }

        public async Task<bool> RegisterHotKeyAsync(int virtualKeyCode, bool alt, bool control, bool shift)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            _hotkeysToRegister.Add(((uint)virtualKeyCode, alt, control, shift, taskCompletionSource));

            return await taskCompletionSource.Task;
        }
    }
}
