﻿using Microsoft.EntityFrameworkCore;
using ScreenTemperature.Entities.KeyBindingActions;
using ScreenTemperature.Services;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace ScreenTemperature
{
    public class HotKeyManager
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
        private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        #endregion

        private static HotKeyManager _instance;
        public static HotKeyManager Instance 
        { 
            get
            {
                if( _instance == null ) _instance = new HotKeyManager();

                return _instance;
            }
            private set { }
        }

        private WebApplication _app;
        private BlockingCollection<(uint virtualKeyCode, uint mask, TaskCompletionSource<bool> taskCompletionSource)> _hotkeysToRegister = new BlockingCollection<(uint virtualKeyCode, uint mask, TaskCompletionSource<bool> taskCompletionSource)>();
        private BlockingCollection<(uint virtualKeyCode, TaskCompletionSource<bool> taskCompletionSource)> _hotkeysToUnregister = new BlockingCollection<(uint virtualKeyCode, TaskCompletionSource<bool> taskCompletionSource)>();

        public static void Init(WebApplication app)
        {
            Instance._app = app;

            Task.Factory.StartNew(Instance.RunAsync);

            Task.Factory.StartNew(Instance.RegisterHotKeysAsync);
        }

        private async Task RegisterHotKeysAsync()
        {
            using (var scope = _app.Services.CreateScope())
            {
                var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var keyBindings = await databaseContext.KeyBindings.ToListAsync();

                foreach(var keyBinding in keyBindings)
                {
                    await RegisterHotKeyAsync(keyBinding.KeyCode, keyBinding.Alt, keyBinding.Control, keyBinding.Shift);
                }
            }
        }

        private async Task RunAsync()
        {
            MSG msg;

            while (true)
            {
                // if an unregistration is in queue
                if (_hotkeysToUnregister.Any())
                {
                    var keyCodeToUnregister = _hotkeysToUnregister.Take();

                    UnregisterHotKey(IntPtr.Zero, (int)keyCodeToUnregister.virtualKeyCode);

                    // send result
                    keyCodeToUnregister.taskCompletionSource.SetResult(true);
                }

                // if a registration is in queue
                if (_hotkeysToRegister.Any())
                {
                    var keyCodeToRegister = _hotkeysToRegister.Take();

                    var hotkeyRegistered = RegisterHotKey(IntPtr.Zero, (int)keyCodeToRegister.virtualKeyCode, keyCodeToRegister.mask, keyCodeToRegister.virtualKeyCode);

                    // send result
                    keyCodeToRegister.taskCompletionSource.SetResult(hotkeyRegistered);
                }

                // check if a message is available
                var isMessageAvailable = PeekMessage(out msg, IntPtr.Zero, 0, 0, 1);

                if (isMessageAvailable)
                {
                    if(msg.message == 786)// message received : WM_HOTKEY 0x312
                    {
                        using (var scope = _app.Services.CreateScope())
                        {
                            var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                            var profileService = scope.ServiceProvider.GetRequiredService<IProfileService>();

                            var keyCode = msg.wParam.ToUInt32();
                            var altWasPressed = ((KeyModifiers)msg.lParam & KeyModifiers.Alt) == KeyModifiers.Alt;
                            var controlWasPressed = ((KeyModifiers)msg.lParam & KeyModifiers.Control) == KeyModifiers.Control;
                            var shiftWasPressed = ((KeyModifiers)msg.lParam & KeyModifiers.Shift) == KeyModifiers.Shift;

                            var matchingBinding = databaseContext.KeyBindings.Include(binding => binding.Actions).SingleOrDefault(x => x.KeyCode == keyCode && x.Alt == altWasPressed && x.Control == controlWasPressed && x.Shift == shiftWasPressed);

                            if(matchingBinding != null) 
                            {
                                foreach(var action  in matchingBinding.Actions)
                                {
                                    if(action is ApplyProfileAction applyProfileAction)
                                    {
                                        await profileService.ApplyProfileAsync(applyProfileAction.ProfileId, null, new CancellationToken());
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
            }
        }

        public async static Task<bool> UnregisterHotKeyAsync(int virtualKeyCode)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            Instance._hotkeysToUnregister.Add(((uint)virtualKeyCode, taskCompletionSource));

            return await taskCompletionSource.Task;
        }

        public async static Task<bool> RegisterHotKeyAsync(int virtualKeyCode, bool alt, bool control, bool shift)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            var mask = alt ? (uint)KeyModifiers.Alt : 0;
            mask = mask | (control ? (uint)KeyModifiers.Control : 0);
            mask = mask | (shift ? (uint)KeyModifiers.Shift : 0);
            mask = mask | 0x4000;

            Instance._hotkeysToRegister.Add(((uint)virtualKeyCode, mask, taskCompletionSource));

            return await taskCompletionSource.Task;
        }
    }
}