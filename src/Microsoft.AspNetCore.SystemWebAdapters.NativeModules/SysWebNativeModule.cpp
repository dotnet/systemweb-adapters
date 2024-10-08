#include "pch.h"
#include "SysWebNativeModule.h"

typedef HRESULT(__stdcall* fRegisterModule)(DWORD, IHttpModuleRegistrationInfo*, IHttpServer*);

class SysWebNativeModule :public ISysWebNativeModule, IModuleAllocator, IHttpModuleRegistrationInfo 
{
private:
    const DWORD _emulatedVersion = 12;

    HINSTANCE _module;
    NativeModuleCallbacks _callbacks;
public:
    SysWebNativeModule(LPCWSTR moduleDll, NativeModuleCallbacks& callbacks) :_callbacks(callbacks) {
        _module = LoadLibrary(moduleDll);

        if (_module == nullptr) {
            return;
        }

        auto registerModule = (fRegisterModule)GetProcAddress(_module, "RegisterModule");

        if (registerModule == nullptr)
        {
            return;
        }

        registerModule(_emulatedVersion, this, nullptr);
    }

    bool IsLoaded() const {
        return _module != nullptr;
    }

    ~SysWebNativeModule() {
        if (_module != nullptr) {
            FreeLibrary(_module);
            _module = nullptr;
        }
    }

    IModuleAllocator* GetAllocator() {
        return this;
    }

    HttpContextCallbacks& GetHttpContextCallbacks() {
        return _callbacks.HttpContextCallbacks;
    }

    _Ret_opt_ _Post_writable_byte_size_(cbAllocation)
        VOID*
        AllocateMemory(
            _In_ DWORD                  cbAllocation
        ) {
        return malloc(cbAllocation);
    }


    PCWSTR GetName(VOID) const {
        return L"";
    }

    HTTP_MODULE_ID GetId(VOID) const {
        return 0;
    }

    HRESULT SetRequestNotifications(
        _In_ IHttpModuleFactory* pModuleFactory,
        _In_ DWORD dwRequestNotifications,
        _In_ DWORD dwPostRequestNotifications
    ) {
        return _callbacks.SetRequestNotifications(pModuleFactory, dwRequestNotifications, dwPostRequestNotifications);
    }

    HRESULT SetGlobalNotifications(
        _In_ CGlobalModule* pGlobalModule,
        _In_ DWORD dwGlobalNotifications
    ) {
        return E_NOTIMPL;
    }

    HRESULT SetPriorityForRequestNotification(
        _In_ DWORD                dwRequestNotification,
        _In_ PCWSTR               pszPriority
    ) {
        return E_NOTIMPL;
    }

    HRESULT SetPriorityForGlobalNotification(
        _In_ DWORD                dwGlobalNotification,
        _In_ PCWSTR               pszPriority
    ) {
        return E_NOTIMPL;
    }
};

ISysWebNativeModule* LoadNativeIISModule(LPCWSTR moduleDll, NativeModuleCallbacks& callbacks) {
    auto m = new SysWebNativeModule(moduleDll, callbacks);

    if (m->IsLoaded()) {
        return m;
    }

    delete m;
    return nullptr;
}

void UnloadNativeIISModule(ISysWebNativeModule* module) {
    if (module != nullptr) {
        delete module;
    }
}
