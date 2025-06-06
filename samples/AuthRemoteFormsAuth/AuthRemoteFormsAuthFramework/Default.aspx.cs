using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FormsAuth
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.IsAuthenticated)
            {
                WelcomeBackMessage.Text = "Welcome back, " + User.Identity.Name + "!";

                AuthenticatedMessagePanel.Visible = true;
                AnonymousMessagePanel.Visible = false;
            }
            else
            {
                AuthenticatedMessagePanel.Visible = false;
                AnonymousMessagePanel.Visible = true;
            }
        }
    }
}