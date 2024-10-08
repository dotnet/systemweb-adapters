#pragma once

#include "SysWebNativeModule.h"

typedef HRESULT(__stdcall* fSetServerVariable)(PCSTR, PCWSTR);

struct HttpContextCallbacks {
    fSetServerVariable setServerVariable;
};

extern "C" _declspec(dllexport) REQUEST_NOTIFICATION_STATUS CallEvent(CHttpModule* m, IHttpContext* context, DWORD request, DWORD isPost);
extern "C" _declspec(dllexport) IHttpContext* CreateHttpContext(HttpContextCallbacks& callbacks);
extern "C" _declspec(dllexport) void DeleteHttpContext(IHttpContext* context);


