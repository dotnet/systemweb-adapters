#pragma once

typedef HRESULT(__stdcall* fSetRequestNotification)(IHttpModuleFactory*, DWORD, DWORD);


typedef HRESULT(__stdcall* fSetServerVariable)(void*, PCSTR, PCWSTR);

struct HttpContextCallbacks {
    fSetServerVariable setServerVariable;
};

struct NativeModuleCallbacks {
    fSetRequestNotification SetRequestNotifications;
    HttpContextCallbacks HttpContextCallbacks;
};

class ISysWebNativeModule {
public:
    virtual IModuleAllocator* GetAllocator() = 0;

    virtual HttpContextCallbacks& GetHttpContextCallbacks() = 0;
};

extern "C" _declspec(dllexport) ISysWebNativeModule* LoadNativeIISModule(LPCWSTR moduleDll, NativeModuleCallbacks& callbacks);
extern "C" _declspec(dllexport) void UnloadNativeIISModule(ISysWebNativeModule* m);

extern "C" _declspec(dllexport) CHttpModule* CreateModule(ISysWebNativeModule* sysWeb, IHttpModuleFactory* factory);
extern "C" _declspec(dllexport) void RemoveModule(ISysWebNativeModule* sysWeb, IHttpModuleFactory* factory);

extern "C" _declspec(dllexport) REQUEST_NOTIFICATION_STATUS CallEvent(ISysWebNativeModule* module, CHttpModule* m, void* context, DWORD request, DWORD isPost);
