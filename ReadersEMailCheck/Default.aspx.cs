﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ReadersEMailCheck
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.StatusCode = 302;
            Response.Redirect("~/EMC", true);
            Response.End();
        }
    }
}