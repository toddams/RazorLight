using NUnit.Framework;
using RazorLight.Caching;

namespace RazorLight.Precompile.Tests
{
	public class Render2Tests : TestWithCulture
	{
		private static TestCaseData T(string templateFilePath, string templateFilePath2, IFileSystemCachingStrategy s, string expected) =>
			new(templateFilePath, templateFilePath2, s, expected) { TestName = "{m}({0},{1},{2})" };

		private static readonly string s_expected = @"Count of issues with the source information: 2

    <p>
Issue Id: 123
<br />
<a href=""https://www.youtube.com/watch?v=Rhc5jXWu55c&amp;t=4815s"">src/_sqlutil.async.cs:251</a>
<br />
<span>This database query contains a SQL injection flaw.  The call to system_data_dll.System.Data.Common.DbCommand.ExecuteReaderAsync() constructs a dynamic SQL query using a variable derived from untrusted input.  An attacker could exploit this flaw to execute arbitrary SQL queries against the database. ExecuteReaderAsync() was called on the \<command\>5__1 object, which contains tainted data. The tainted data originated from earlier calls to vacationbidding_dll.VirtualController.vc_wcfentry, and system_data_dll.System.Data.SqlClient.SqlCommand.ExecuteReader.</span> <span>Avoid dynamically constructing SQL queries.  Instead, use parameterized prepared statements to prevent the database from interpreting the contents of bind variables as part of the query.  Always validate untrusted input to ensure that it conforms to the expected format, using centralized data validation routines when possible.</span> <span>References: <a href=""https://cwe.mitre.org/data/definitions/89.html"">CWE</a> <a href=""https://owasp.org/www-community/attacks/SQL_Injection"">OWASP</a></span>
<br />
Module: Architecture
<br />
Type Name: SharpTop.Common.Utils._SqlUtil
<br />
Member Name: GetReaderSource
<br />
First Found Date: 9/9/2021 3:35:49 PM
<br />
    <span>Found no annotations.</span>
    </p>
    <p>
Issue Id: 987
<br />
<a href=""https://www.youtube.com/watch?v=Rhc5jXWu55c&amp;t=4815s"">src/sqlconnectioncontext.cs:35</a>
<br />
<span>This call to system_data_dll.System.Data.SqlClient.SqlConnection.!newinit_0_1() allows external control of system settings.  The argument to the function is constructed using untrusted input, which can disrupt service or cause an application to behave in unexpected ways. The first argument to !newinit_0_1() contains tainted data from the variable m_connectionString. The tainted data originated from an earlier call to backgroundjobs_dll.VirtualController.vc_wcfentry.</span> <span>Never allow untrusted or otherwise untrusted data to control system-level settings.  Always validate untrusted input to ensure that it conforms to the expected format, using centralized data validation routines when possible.</span> <span>References: <a href=""https://cwe.mitre.org/data/definitions/15.html"">CWE</a></span>
<br />
Module: Architecture
<br />
Type Name: xyz.UtilitySuite.DbUpgrade.SqlConnectionContext
<br />
Member Name: CreateConnectionAsync
<br />
First Found Date: 7/10/2020 7:00:28 PM
<br />
    <span>Found 3 annotations:</span>
    <table>
        <tr><th>Action</th><th>Created</th><th>User Name</th><th>Comment</th></tr>
            <tr><td>APPROVED</td><td>3/31/2021 6:44:12 PM</td><td>Michael Jackson</td><td>Approved per rationale provided and John Smith&#x27; review and approval on 3/30.</td></tr>
            <tr><td>COMMENT</td><td>3/31/2021 6:42:26 PM</td><td>John Smith</td><td>Mitigation statements reviewed. Recommend for closure and approval</td></tr>
            <tr><td>APPDESIGN</td><td>10/22/2020 6:58:56 PM</td><td>Li Jet</td><td>Some explanation</td></tr>
    </table>
    </p>
" + Environment.NewLine;

		private static readonly TestCaseData[] s_testCases = new TestCaseData[]
		{
			T("FullMessage.cshtml", "folder/MessageItem.cshtml", FileHashCachingStrategy.Instance, s_expected),
			T("FullMessage.cshtml", "folder/MessageItem.cshtml", SimpleFileCachingStrategy.Instance, s_expected),
		};

		[SetUp]
		public void Cleanup()
		{
			PrecompileTestCases.CleanupDlls("Samples");
		}

		[TestCaseSource(nameof(s_testCases))]
		public void RenderOrder1(string key, string key2, IFileSystemCachingStrategy s, string expected)
		{
			var (a1, a2) = Precompile(key, key2, s);

			Run(key, expected, a1 + ',' + a2);
		}

		[TestCaseSource(nameof(s_testCases))]
		public void RenderOrder2(string key, string key2, IFileSystemCachingStrategy s, string expected)
		{
			var (a1, a2) = Precompile(key, key2, s);

			Run(key, expected, a2 + ',' + a1);
		}

		[TestCaseSource(nameof(s_testCases))]
		public void RenderGlobRecursive(string key, string key2, IFileSystemCachingStrategy s, string expected)
		{
			Precompile(key, key2, s);

			Run(key, expected, "**/*.dll");
		}

		[TestCaseSource(nameof(s_testCases))]
		public void RenderFolderRecursive(string key, string key2, IFileSystemCachingStrategy s, string expected)
		{
			Precompile(key, key2, s);

			Run(key, expected, "Samples", "-r");
		}

		[TestCaseSource(nameof(s_testCases))]
		public void RenderFolderNonRecursive(string key, string key2, IFileSystemCachingStrategy s, string expected)
		{
			Precompile(key, key2, s);

			var exc = Assert.Throws<RazorLightException>(() => Run(key, expected, "Samples"));
			Assert.AreEqual("No precompiled template found for the key /folder/MessageItem.cshtml", exc.Message);
		}

		[TestCaseSource(nameof(s_testCases))]
		public void RenderGlobNonRecursive(string key, string key2, IFileSystemCachingStrategy s, string expected)
		{
			Precompile(key, key2, s);

			var exc = Assert.Throws<RazorLightException>(() => Run(key, expected, "Samples/*.dll"));
			Assert.AreEqual("No precompiled template found for the key /folder/MessageItem.cshtml", exc.Message);
		}

		private static (string, string) Precompile(string key, string key2, IFileSystemCachingStrategy s) => (
			Helper.RunCommandTrimNewline("precompile", "-t", key, "-b", "Samples", "-s", s.Name),
			Helper.RunCommandTrimNewline("precompile", "-t", key2, "-b", "Samples", "-s", s.Name)
		);

		private static void Run(string key, string expected, string precompiledFilePath, params string[] args)
		{
			var commandLineArgs = new List<string>
			{
				"render",
				"-p",
				precompiledFilePath,
				"-m",
				"Samples/FindingsWithSourceCodeInfo.json",
				"-k",
				key
			};
			commandLineArgs.AddRange(args);

			var actual = Helper.RunCommand(commandLineArgs.ToArray()).ToString();
			Assert.AreEqual(expected, actual);
		}

		[TestCaseSource(nameof(s_testCases))]
		public void PrecompileAndRender(string templateFilePath, string _, IFileSystemCachingStrategy s, string expected)
		{
			var commandLineArgs = new List<string>
			{
				"precompile",
				"-t",
				templateFilePath,
				"-b",
				"Samples",
				"-s",
				s.Name,
				"-m",
				"Samples/FindingsWithSourceCodeInfo.json"
			};

			var actual = Helper.RunCommand(commandLineArgs.ToArray()).ToString();
			Assert.AreEqual(expected, actual);
		}
	}
}