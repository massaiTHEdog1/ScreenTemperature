using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using ScreenTemperature.Entities.Configurations;
using ScreenTemperature.Services;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static ScreenTemperature.Win32;

namespace ScreenTemperature
{
    public class HotKeyPressedEventArgs : EventArgs
    {
        public int KeyCode { get; set; }
        public bool Alt { get; set; }
        public bool Ctrl { get; set; }
        public bool Shift { get; set; }
    }

    public class HotKeyManager
    {
        private static HotKeyManager _instance;
        public static HotKeyManager Instance
        {
            get 
            { 
                if(_instance == null) _instance = new HotKeyManager();
                return _instance; 
            }
        }

        public Task Task { get; private set; }

        private BlockingCollection<(uint virtualKeyCode, bool alt, bool control, bool shift, TaskCompletionSource<bool> taskCompletionSource)> _hotkeysToRegister = new BlockingCollection<(uint virtualKeyCode, bool alt, bool control, bool shift, TaskCompletionSource<bool> taskCompletionSource)>();
        private BlockingCollection<(uint virtualKeyCode, bool alt, bool control, bool shift, TaskCompletionSource<bool> taskCompletionSource)> _hotkeysToUnregister = new BlockingCollection<(uint virtualKeyCode, bool alt, bool control, bool shift, TaskCompletionSource<bool> taskCompletionSource)>();

        public delegate void HotKeyPressedEventHandler(object sender, HotKeyPressedEventArgs e);
        public static event HotKeyPressedEventHandler HotKeyPressed;


        HotKeyManager()
        {
            // private constructor
        }

        public void Start(CancellationToken cancellationToken)
        {
            Task = Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
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
                            PeekMessage(out var msg, IntPtr.Zero, WM_HOTKEY, WM_HOTKEY, 1);

                            var keyCode = ExtractKeyCodeFromHotKeyId(msg.wParam);
                            var altWasPressed = ((KeyModifiers)msg.lParam & KeyModifiers.Alt) == KeyModifiers.Alt;
                            var controlWasPressed = ((KeyModifiers)msg.lParam & KeyModifiers.Control) == KeyModifiers.Control;
                            var shiftWasPressed = ((KeyModifiers)msg.lParam & KeyModifiers.Shift) == KeyModifiers.Shift;

                            HotKeyPressed?.Invoke(this, new HotKeyPressedEventArgs()
                            {
                                KeyCode = keyCode,
                                Alt = altWasPressed,
                                Ctrl = controlWasPressed,
                                Shift = shiftWasPressed,
                            });
                        }

                    }
                    catch (Exception ex) { }
                }
            });
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
