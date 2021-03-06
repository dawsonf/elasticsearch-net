using Elastic.Xunit.XunitPlumbing;
using Nest;
using System.ComponentModel;

namespace Examples.XPack.Docs.En.RestApi.Security
{
	public class EnableUsersPage : ExampleBase
	{
		[U(Skip = "Example not implemented")]
		[Description("../../x-pack/docs/en/rest-api/security/enable-users.asciidoc:44")]
		public void Line44()
		{
			// tag::adf36e2d8fc05c3719c91912481c4e19[]
			var response0 = new SearchResponse<object>();
			// end::adf36e2d8fc05c3719c91912481c4e19[]

			response0.MatchesExample(@"PUT /_security/user/jacknich/_enable");
		}
	}
}