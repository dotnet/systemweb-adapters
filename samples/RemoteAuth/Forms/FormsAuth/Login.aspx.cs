using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FormsAuth
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void LoginButton_Click(object sender, EventArgs e)
        {
            InvalidCredentialsMessage.Visible = false;

            // In a more real-world scenario, this would use a membership database or
            // something similar
            string[] users = { "User1", "User2", "User3" };
            string[] passwords = { "PasswordA", "PasswordB", "PasswordC" };
            for (int i = 0; i < users.Length; i++)
            {
                bool validUsername = (string.Compare(UserName.Text, users[i], true) == 0);
                bool validPassword = (string.Compare(Password.Text, passwords[i], false) == 0);
                if (validUsername && validPassword)
                {
                    FormsAuthentication.RedirectFromLoginPage(UserName.Text, RememberMe.Checked);
                }
            }

            // If we reach here, the user's credentials were invalid
            InvalidCredentialsMessage.Visible = true;
        }
    }
}
