using Elastic.Xunit.XunitPlumbing;
using Nest;
using System.ComponentModel;

namespace Examples.Ml.AnomalyDetection.Apis
{
	public class PreviewDatafeedPage : ExampleBase
	{
		[U(Skip = "Example not implemented")]
		[Description("ml/anomaly-detection/apis/preview-datafeed.asciidoc:51")]
		public void Line51()
		{
			// tag::38eed000de433b540116928681c520d3[]
			var response0 = new SearchResponse<object>();
			// end::38eed000de433b540116928681c520d3[]

			response0.MatchesExample(@"GET _ml/datafeeds/datafeed-high_sum_total_sales/_preview");
		}
	}
}