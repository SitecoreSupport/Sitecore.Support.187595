namespace Sitecore.Support.Shell.Applications.ContentEditor
{
    using Sitecore.Configuration;
    using Sitecore.Diagnostics;
    using Sitecore.Web.UI.Sheer;
    using Sitecore.Support.Web.WebUtil;
    public class RichText : Sitecore.Shell.Applications.ContentEditor.RichText
    {
        protected override void UpdateHtml(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            string text = args.Result;
            if (text == "__#!$No value$!#__")
            {
                text = string.Empty;
            }
            text = this.ProcessValidateScripts(text);
            if (text != this.Value)
            {
                this.SetModified();
            }
            SheerResponse.Eval(string.Concat(new string[]
            {
                "scForm.browser.getControl('",
                this.ID,
                "').contentWindow.scSetText(",
                StringUtil.EscapeJavascriptString(text),
                ")"
            }));
            SheerResponse.Eval("scContent.startValidators()");
        }
        protected string ProcessValidateScripts(string value)
        {
            if (Settings.HtmlEditor.RemoveScripts)
            {
                value = SupporWebUtil.RemoveAllScripts(value, true, true, true);
            }
            return value;
        }
    }
}