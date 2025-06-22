using System;
using Vanara.PInvoke;

namespace NetAutoGUI.Windows;

internal static class Win32Helpers
{
    /// <summary>
    /// It only works when the calling thread has message queue.
    /// </summary>
    /// <param name="hWnd"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void ActiveWindow(HWND hWnd)
    {
        if (Win32Helpers.GetRootWindow(hWnd) != hWnd)
        {
            throw new ArgumentException(nameof(hWnd),
                "Only handle to root Window is supported.");
        }

        //In some scenarios, there is a classic "window just blinks in the taskbar" issue;
        //This happens because Windows prevents background applications from stealing focus for security and usability reasons.

        var foreground = User32.GetForegroundWindow();
        uint foreThread = User32.GetWindowThreadProcessId(foreground, out _);
        uint appThread = Kernel32.GetCurrentThreadId();

        if (foreThread != appThread)
        {
            User32.AttachThreadInput(foreThread, appThread, true);
            Win32Error.ThrowLastError();
            User32.SetActiveWindow(hWnd);
            Win32Error.ThrowLastError();
            User32.SetForegroundWindow(hWnd);
            Win32Error.ThrowLastError();
            User32.BringWindowToTop(hWnd);
            Win32Error.ThrowLastError();
            User32.AttachThreadInput(foreThread, appThread, false);
            Win32Error.ThrowLastError();
        }
        else
        {
            User32.SetForegroundWindow(hWnd);
            Win32Error.ThrowLastError();
        }
    }

    public static HWND GetRootWindow(HWND hWnd)
    {
        var hWndWindow = User32.GetAncestor(hWnd, User32.GetAncestorFlag.GA_ROOT);
        Win32Error.ThrowLastError();
        return hWndWindow;
    }
}