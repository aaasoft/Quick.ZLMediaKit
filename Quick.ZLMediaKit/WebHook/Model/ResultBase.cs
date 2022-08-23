using System;
using System.Collections.Generic;
using System.Text;

namespace Quick.ZLMediaKit.WebHook.Model
{
    public class ResultBase
    {

        public virtual int Code => 0;

        public string Msg { get; set; } = "success";
    }
}
