#include "pch.h"
#include "SysWebNativeModule.h"

CHttpModule* CreateModule(ISysWebNativeModule* sysWeb, IHttpModuleFactory* factory) {
    if (sysWeb == nullptr || factory == nullptr) {
        return nullptr;
    }

    CHttpModule* m;
    if (factory->GetHttpModule(&m, sysWeb->GetAllocator()) == S_OK) {
        return m;
    }

    return nullptr;
}

void RemoveModule(ISysWebNativeModule* sysWeb, IHttpModuleFactory* factory) {
    if (factory != nullptr) {
        factory->Terminate();
    }
}
