using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebInterface
{
    public partial class Search : System.Web.UI.Page
    {
        SearchEngine SearchEngine = new SearchEngine();
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            string TextVal = this.TextBox1.Text;
            string[] SplittedVal = TextVal.Split(' ');

            List<string> StemmedVals = SearchEngine.LinguisticsOnSearch(SplittedVal.ToList());
            StemmedVals = SearchEngine.RemoveSpecialChar(StemmedVals);
            List<string> Result = SearchEngine.Search(StemmedVals);

            for(int i = 0; i < Result.Count; i++)
            {
                HyperLink link = new HyperLink();
                link.NavigateUrl = SearchEngine.GetURL(Result[i]);
                link.Text = SearchEngine.GetURL(Result[i]);
                this.Controls.Add(link);
                this.Controls.Add(new LiteralControl("<br/>"));
            }
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            string TextVal = this.TextBox1.Text;
            string[] SplittedVal = TextVal.Split(' ');

            List<string> StemmedVals = SearchEngine.LinguisticsOnSearch(SplittedVal.ToList());
            StemmedVals = SearchEngine.RemoveSpecialChar(StemmedVals);
            List<string> Result = SearchEngine.ExactSearch(StemmedVals);

            for (int i = 0; i < Result.Count; i++)
            {
                HyperLink link = new HyperLink();
                link.NavigateUrl = SearchEngine.GetURL(Result[i]);
                link.Text = SearchEngine.GetURL(Result[i]);
                this.Controls.Add(link);
                this.Controls.Add(new LiteralControl("<br/>"));
            }
        }
    }
}