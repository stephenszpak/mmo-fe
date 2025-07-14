This folder should contain third-party plugins used by the project.

To enable WebSocket networking, download the **websocket-sharp.dll** from the
project's releases page at https://github.com/sta/websocket-sharp/releases and
place the DLL in this directory.

Unity will automatically detect the plugin on refresh. Ensure the API
Compatibility Level is set to ".NET Framework 4.x" (or ".NET Standard 2.0" for
Unity 2022+) in **Edit → Project Settings → Player**.
