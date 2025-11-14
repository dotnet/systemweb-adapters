using System;
using System.Web.UI;
using MvcApp;

namespace DependencyInjectionFramework
{
    public partial class WebForms : Page
    {
        private readonly TransientService _transient1;
        private readonly TransientService _transient2;
        private readonly SingletonService _singleton;

        // NOTE: Pages do not support scoped services
        public WebForms(SingletonService singleton, TransientService transient1, TransientService transient2)
        {
            _singleton = singleton;
            _transient1 = transient1;
            _transient2 = transient2;
        }

        public bool Result => TestService.IsValid(_singleton, _transient1, _transient2);
    }
}
