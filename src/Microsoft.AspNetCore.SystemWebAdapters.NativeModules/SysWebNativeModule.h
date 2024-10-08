#pragma once

typedef HRESULT(__stdcall* fSetRequestNotification)(IHttpModuleFactory*, DWORD, DWORD);

struct NativeModuleCallbacks {
    fSetRequestNotification SetRequestNotifications;
};

class ISysWebNativeModule {
public:
    virtual IModuleAllocator* GetAllocator() = 0;
};

extern "C" _declspec(dllexport) ISysWebNativeModule* LoadNativeIISModule(LPCWSTR moduleDll, NativeModuleCallbacks callbacks);
extern "C" _declspec(dllexport) void UnloadNativeIISModule(ISysWebNativeModule* m);

extern "C" _declspec(dllexport) CHttpModule* CreateModule(ISysWebNativeModule* sysWeb, IHttpModuleFactory* factory);
extern "C" _declspec(dllexport) void RemoveModule(ISysWebNativeModule* sysWeb, IHttpModuleFactory* factory);

