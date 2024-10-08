#include "pch.h"
#include "SysWebNativeModule.h"

typedef HRESULT(__stdcall* fRegisterModule)(DWORD, IHttpModuleRegistrationInfo*, IHttpServer*);

class ModuleRegistrationImpl :public IHttpModuleRegistrationInfo {
private:
    NativeModuleCallbacks _c;
public:
    ModuleRegistrationImpl(NativeModuleCallbacks c) {
        _c = c;
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
        return _c.SetRequestNotifications(pModuleFactory, dwRequestNotifications, dwPostRequestNotifications);
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

class SysWebNativeModule :public ISysWebNativeModule, public IModuleAllocator 
{
private:
    const DWORD _emulatedVersion = 12;

    HINSTANCE _module;
public:
    SysWebNativeModule(LPCWSTR moduleDll) {
        _module = LoadLibrary(moduleDll);
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

    HRESULT RegisterModule(NativeModuleCallbacks callbacks) const {
        if (_module == nullptr) {
            return E_FAIL;
        }

        auto registerModule = (fRegisterModule)GetProcAddress(_module, "RegisterModule");

        if (registerModule == nullptr)
        {
            return E_FAIL;
        }

        auto c = ModuleRegistrationImpl(callbacks);
        return registerModule(_emulatedVersion, &c, nullptr);
    }

    IModuleAllocator* GetAllocator() {
        return this;
    }

    _Ret_opt_ _Post_writable_byte_size_(cbAllocation)
        VOID*
        AllocateMemory(
            _In_ DWORD                  cbAllocation
        ) {
        return malloc(cbAllocation);
    }
};

ISysWebNativeModule* LoadNativeIISModule(LPCWSTR moduleDll, NativeModuleCallbacks callbacks) {
    auto m = new SysWebNativeModule(moduleDll);

    if (!m->IsLoaded()) {
        return nullptr;
    }

    if (m->RegisterModule(callbacks) == S_OK) {
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
