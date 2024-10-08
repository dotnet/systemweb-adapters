// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <windows.h>
#include <sal.h>
#include <httpserv.h>
#include <memory>

BOOL APIENTRY DllMain(HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved
)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

typedef HRESULT(__stdcall* fRegisterModule)(DWORD, IHttpModuleRegistrationInfo*, IHttpServer*);
typedef HRESULT(__stdcall* fSetRequestNotification)(IHttpModuleFactory*, DWORD, DWORD);
typedef HRESULT(__stdcall* fSetServerVariable)(PCSTR, PCWSTR);

class ModuleRegistrationImpl :public IHttpModuleRegistrationInfo {
private:
    fSetRequestNotification _r;
public:
    ModuleRegistrationImpl(fSetRequestNotification r) {
        _r = r;
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
        return _r(pModuleFactory, dwRequestNotifications, dwPostRequestNotifications);
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

class ModuleHolder {
private:
    const DWORD _emulatedVersion = 12;

    HINSTANCE _module;
public:
    ModuleHolder(LPCWSTR moduleDll) {
        _module = LoadLibrary(moduleDll);
    }

    bool IsLoaded() const {
        return _module != nullptr;
    }

    ~ModuleHolder() {
        if (_module != nullptr) {
            FreeLibrary(_module);
            _module = nullptr;
        }
    }

    HRESULT RegisterModule(fSetRequestNotification callback) const {
        if (_module == nullptr) {
            return E_FAIL;
        }

        auto registerModule = (fRegisterModule)GetProcAddress(_module, "RegisterModule");

        if (registerModule == nullptr)
        {
            return E_FAIL;
        }

        auto c = ModuleRegistrationImpl(callback);
        return registerModule(_emulatedVersion, &c, nullptr);
    }
};

extern "C" _declspec(dllexport) void UnregisterModule(ModuleHolder* m);
extern "C" _declspec(dllexport) ModuleHolder* RegisterModule(LPCWSTR moduleDll, fSetRequestNotification setRequest);
extern "C" _declspec(dllexport) CHttpModule* CreateModule(IHttpModuleFactory* factory);
extern "C" _declspec(dllexport) REQUEST_NOTIFICATION_STATUS CallEvent(CHttpModule* m, IHttpContext* context, DWORD request, DWORD isPost);
extern "C" _declspec(dllexport) IHttpContext* CreateHttpContext(fSetServerVariable setServer);
extern "C" _declspec(dllexport) void DeleteHttpContext(IHttpContext* context);

ModuleHolder* RegisterModule(LPCWSTR moduleDll, fSetRequestNotification setRequest)
{
    auto library = new ModuleHolder(moduleDll);

    if (!library->IsLoaded())
    {
        return nullptr;
    }

    library->RegisterModule(setRequest);
    return library;
}

void UnregisterModule(ModuleHolder* m) {
    if (m != nullptr)
    {
        delete m;
    }
}

CHttpModule* CreateModule(IHttpModuleFactory* factory) {
    CHttpModule* m;
    if (factory->GetHttpModule(&m, nullptr) == S_OK) {
        return m;
    }

    return nullptr;
}

class MyHttpContext :public IHttpContext {
private:
    fSetServerVariable _setServer;
public:
    MyHttpContext(fSetServerVariable setServer) :_setServer(setServer) {
    }

    IHttpSite* GetSite(
        VOID
    ) {
        throw "NotImplemented";
    }

    IHttpApplication* GetApplication(
        VOID
    ) {
        throw "NotImplemented";
    }

    IHttpConnection* GetConnection(
        VOID
    ) {
        throw "NotImplemented";
    }

    IHttpRequest* GetRequest(
        VOID
    ) {
        throw "NotImplemented";
    }

    IHttpResponse* GetResponse(
        VOID
    ) {
        throw "NotImplemented";
    }

    BOOL GetResponseHeadersSent(
        VOID
    ) const {
        throw "NotImplemented";
    }

    IHttpUser* GetUser(
        VOID
    ) const {
        throw "NotImplemented";
    }

    IHttpModuleContextContainer* GetModuleContextContainer(
        VOID
    ) {
        throw "NotImplemented";
    }

    VOID IndicateCompletion(
        _In_ REQUEST_NOTIFICATION_STATUS     notificationStatus
    ) {
    }

    HRESULT PostCompletion(
        _In_ DWORD                cbBytes
    ) {
        return E_NOTIMPL;
    }

    VOID DisableNotifications(
        _In_ DWORD                dwNotifications,
        _In_ DWORD                dwPostNotifications
    ) {
    }

    BOOL GetNextNotification(
        _In_ REQUEST_NOTIFICATION_STATUS    status,
        _Out_ DWORD* pdwNotification,
        _Out_ BOOL* pfIsPostNotification,
        _Outptr_ CHttpModule** ppModuleInfo,
        _Outptr_ IHttpEventProvider** ppRequestOutput
    ) {
        throw "NotImplemented";
    }

    BOOL GetIsLastNotification(
        _In_ REQUEST_NOTIFICATION_STATUS   status
    ) {
        throw "NotImplemented";
    }

    HRESULT ExecuteRequest(
        _In_ BOOL                   fAsync,
        _In_ IHttpContext* pHttpContext,
        _In_ DWORD                  dwExecuteFlags,
        _In_ IHttpUser* pHttpUser,
        _Out_ BOOL* pfCompletionExpected = NULL
    ) {
        return E_NOTIMPL;
    }

    DWORD GetExecuteFlags(
        VOID
    ) const {
        throw "NotImplemented";
    }

    HRESULT GetServerVariable(
        _In_ PCSTR          pszVariableName,
        _Outptr_result_buffer_(*pcchValueLength)
        PCWSTR* ppszValue,
        _Out_ DWORD* pcchValueLength
    ) {
        return E_NOTIMPL;
    }

    HRESULT GetServerVariable(
        _In_ PCSTR          pszVariableName,
        _Outptr_result_bytebuffer_(*pcchValueLength)
        PCSTR* ppszValue,
        _Out_ DWORD* pcchValueLength
    ) {
        return E_NOTIMPL;
    }

    HRESULT SetServerVariable(
        PCSTR               pszVariableName,
        PCWSTR              pszVariableValue
    ) {
        return _setServer(pszVariableName, pszVariableValue);
    }

    _Ret_opt_ _Post_writable_byte_size_(cbAllocation) VOID* AllocateRequestMemory(
        _In_ DWORD          cbAllocation
    ) {
        throw "NotImplemented";
    }

    IHttpUrlInfo* GetUrlInfo(
        VOID
    ) {
        throw "NotImplemented";
    }

    IMetadataInfo* GetMetadata(
        VOID
    ) {
        throw "NotImplemented";
    }

    _Ret_writes_(*pcchPhysicalPath) PCWSTR GetPhysicalPath(
        _Out_ DWORD* pcchPhysicalPath = NULL
    ) {
        throw "NotImplemented";
    }

    _Ret_writes_(*pcchScriptName) PCWSTR GetScriptName(
        _Out_ DWORD* pcchScriptName = NULL
    ) const {
        throw "NotImplemented";
    }

    _Ret_writes_(*pcchScriptTranslated) PCWSTR GetScriptTranslated(
        _Out_ DWORD* pcchScriptTranslated = NULL
    ) {
        throw "NotImplemented";
    }

    IScriptMapInfo* GetScriptMap(
        VOID
    ) const {
        throw "NotImplemented";
    }

    VOID SetRequestHandled(
        VOID
    ) {
        throw "NotImplemented";
    }

    IHttpFileInfo* GetFileInfo(
        VOID
    ) const {
        throw "NotImplemented";
    }

    HRESULT MapPath(
        _In_ PCWSTR         pszUrl,
        _Inout_updates_(*pcbPhysicalPath)
        PWSTR               pszPhysicalPath,
        _Inout_ DWORD* pcbPhysicalPath
    ) {
        return E_NOTIMPL;
    }

    HRESULT NotifyCustomNotification(
        _In_ ICustomNotificationProvider* pCustomOutput,
        _Out_ BOOL* pfCompletionExpected
    ) {
        return E_NOTIMPL;
    }

    IHttpContext* GetParentContext(
        VOID
    ) const {
        throw "NotImplemented";
    }

    IHttpContext* GetRootContext(
        VOID
    ) const {
        throw "NotImplemented";
    }

    HRESULT CloneContext(
        _In_ DWORD          dwCloneFlags,
        _Outptr_
        IHttpContext** ppHttpContext
    ) {
        return E_NOTIMPL;
    }

    HRESULT ReleaseClonedContext(
        VOID
    ) {
        return E_NOTIMPL;
    }

    HRESULT GetCurrentExecutionStats(
        _Out_ DWORD* pdwNotification,
        _Out_ DWORD* pdwNotificationStartTickCount = NULL,
        _Out_ PCWSTR* ppszModule = NULL,
        _Out_ DWORD* pdwModuleStartTickCount = NULL,
        _Out_ DWORD* pdwAsyncNotification = NULL,
        _Out_ DWORD* pdwAsyncNotificationStartTickCount = NULL
    ) const {
        return E_NOTIMPL;
    }

    IHttpTraceContext* GetTraceContext(
        VOID
    ) const {
        throw "NotImplemented";
    }

    HRESULT GetServerVarChanges(
        _In_    DWORD       dwOldChangeNumber,
        _Out_   DWORD* pdwNewChangeNumber,
        _Inout_ DWORD* pdwVariableSnapshot,
        _Inout_ _At_(*ppVariableNameSnapshot, _Pre_readable_size_(*pdwVariableSnapshot) _Post_readable_size_(*pdwVariableSnapshot))
        PCSTR** ppVariableNameSnapshot,
        _Inout_ _At_(*ppVariableValueSnapshot, _Pre_readable_size_(*pdwVariableSnapshot) _Post_readable_size_(*pdwVariableSnapshot))
        PCWSTR** ppVariableValueSnapshot,
        _Out_   DWORD* pdwDiffedVariables,
        _Out_   DWORD** ppDiffedVariableIndices
    ) {
        return E_NOTIMPL;
    }

    HRESULT CancelIo(
        VOID
    ) {
        return E_NOTIMPL;
    }

    HRESULT MapHandler(
        _In_ DWORD          dwSiteId,
        _In_ PCWSTR         pszSiteName,
        _In_ PCWSTR         pszUrl,
        _In_ PCSTR          pszVerb,
        _Outptr_
        IScriptMapInfo** ppScriptMap,
        _In_ BOOL           fIgnoreWildcardMappings = FALSE
    ) {
        return E_NOTIMPL;
    }

    HRESULT GetExtendedInterface(
        _In_ HTTP_CONTEXT_INTERFACE_VERSION version,
        _Outptr_ PVOID* ppInterface
    ) {
        return E_NOTIMPL;
    }
};

IHttpContext* CreateHttpContext(fSetServerVariable setServer) {
    return new MyHttpContext(setServer);
}

void DeleteHttpContext(IHttpContext* context) {
    if (context != nullptr) {
        delete context;
    }
}

REQUEST_NOTIFICATION_STATUS CallEvent(CHttpModule* m, IHttpContext* context, DWORD request, DWORD isPost) {
    if (request == RQ_BEGIN_REQUEST && isPost == false) {
        return m->OnBeginRequest(context, nullptr);
    }

    // TODO: handle unimplemented
    return RQ_NOTIFICATION_CONTINUE;
}
