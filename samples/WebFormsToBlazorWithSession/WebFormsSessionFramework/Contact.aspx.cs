using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebFormsSessionFramework
{
    public partial class Contact : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SessionTextBox.Text = string.Empty;
            }
        }

        protected void SubmitSessionButton_Click(object sender, EventArgs e)
        {
            Session["test-value"] = SessionTextBox.Text;
        }
    }
}
