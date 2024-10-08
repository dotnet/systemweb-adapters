#include "pch.h"
#include "CSampleModule.h"

#define _WINSOCKAPI_
#include <windows.h>
#include <sal.h>
#include <httpserv.h>

// Create the module class.
class CHelloWorld : public CHttpModule
{
public:
    REQUEST_NOTIFICATION_STATUS
        OnBeginRequest(
            IN IHttpContext* pHttpContext,
            IN IHttpEventProvider* pProvider
        )
    {
        UNREFERENCED_PARAMETER(pProvider);

        // Create an HRESULT to receive return values from methods.
        HRESULT hr;

        pHttpContext->SetServerVariable("FromNativeModule", L"Hello World!");

        // Return processing to the pipeline.
        return RQ_NOTIFICATION_CONTINUE;
    }
};

// Create the module's class factory.
class CHelloWorldFactory : public IHttpModuleFactory
{
public:
    HRESULT
        GetHttpModule(
            OUT CHttpModule** ppModule,
            IN IModuleAllocator* pAllocator
        )
    {
        UNREFERENCED_PARAMETER(pAllocator);

        // Create a new instance.
        CHelloWorld* pModule = new CHelloWorld;

        // Test for an error.
        if (!pModule)
        {
            // Return an error if the factory cannot create the instance.
            return HRESULT_FROM_WIN32(ERROR_NOT_ENOUGH_MEMORY);
        }
        else
        {
            // Return a pointer to the module.
            *ppModule = pModule;
            pModule = NULL;
            // Return a success status.
            return S_OK;
        }
    }

    void
        Terminate()
    {
        // Remove the class from memory.
        delete this;
    }
};

extern "C" _declspec(dllexport) HRESULT __stdcall LoadNativeIISModule(DWORD dwServerVersion, IHttpModuleRegistrationInfo* pModuleInfo, IHttpServer* pGlobalInfo);

// Create the module's exported registration function.
HRESULT
__stdcall
LoadNativeIISModule(
    DWORD dwServerVersion,
    IHttpModuleRegistrationInfo* pModuleInfo,
    IHttpServer* pGlobalInfo
)
{
    UNREFERENCED_PARAMETER(dwServerVersion);
    UNREFERENCED_PARAMETER(pGlobalInfo);

    // Set the request notifications and exit.
    return pModuleInfo->SetRequestNotifications(
        new CHelloWorldFactory,
        RQ_BEGIN_REQUEST,
        0
    );
}
