using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.FileProviders;
using Moq;
using RazorLight.Internal;
using RazorLight.Rendering;
using RazorLight.Templating;
using RazorLight.Templating.FileSystem;
using Xunit;

namespace RazorLight.Tests
{
	public class TemplatePageTest
	{
		private readonly RenderAsyncDelegate _nullRenderAsyncDelegate = writer => Task.FromResult(0);
		private readonly Func<TextWriter, Task> NullAsyncWrite = writer => writer.WriteAsync(string.Empty);


		[Fact]
		public async Task DefineSection_ThrowsIfSectionIsAlreadyDefined()
		{
			// Arrange
			var page = CreatePage(v =>
			{
				v.DefineSection("qux", _nullRenderAsyncDelegate);
				v.DefineSection("qux", _nullRenderAsyncDelegate);
			});

			// Act & Assert
			var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => page.ExecuteAsync());
			Assert.Equal("Section 'qux' is already defined", ex.Message);
		}

		[Fact]
		public async Task RenderSection_RendersSectionFromPreviousPage()
		{
			// Arrange
			var expected = "Hello world";
			var page = CreatePage(v =>
			{
				v.Write(v.RenderSection("bar"));
			});
			page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
			{
				{ "bar", writer => writer.WriteAsync(expected) }
			};

			// Act
			await page.ExecuteAsync();

			// Assert
			Assert.Equal(expected, page.RenderedContent);
		}

		[Fact]
		public async Task RenderSection_ThrowsIfRequiredSectionIsNotFound()
		{
			// Arrange
			var context = CreateViewContext(viewPath: "/Views/TestPath/Test.cshtml");
			context.ExecutingFilePath = "/Views/TestPath/_Layout.cshtml";
			var page = CreatePage(v =>
			{
				v.RenderSection("bar");
			}, context: context);
			page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
			{
				{ "baz", _nullRenderAsyncDelegate }
			};

			// Act & Assert
			var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => page.ExecuteAsync());
			var message = $"Layout page cannot find section 'bar' in the content page";
			Assert.Equal(message, ex.Message);
		}

		[Fact]
		public async Task IgnoreSection_ThrowsIfSectionIsNotFound()
		{
			// Arrange
			var context = CreateViewContext(viewPath: "/Views/TestPath/Test.cshtml");
			context.ExecutingFilePath = "/Views/TestPath/_Layout.cshtml";
			var page = CreatePage(v =>
			{
				v.Path = "/Views/TestPath/Test.cshtml";
				v.IgnoreSection("bar");
			}, context);
			page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
			{
				{ "baz", _nullRenderAsyncDelegate }
			};

			// Act & Assert
			var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => page.ExecuteAsync());
			var message = $"Layout page cannot find section 'bar' in the content page";
			Assert.Equal(message, ex.Message);
		}

		[Fact]
		public async Task IsSectionDefined_ReturnsFalseIfSectionNotDefined()
		{
			// Arrange
			bool? actual = null;
			var page = CreatePage(v =>
			{
				actual = v.IsSectionDefined("foo");
				v.RenderSection("baz");
				v.RenderBodyPublic();
			});
			page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
			{
				{ "baz", _nullRenderAsyncDelegate }
			};
			page.BodyContent = new HtmlString("body-content");

			// Act
			await page.ExecuteAsync();

			// Assert
			Assert.Equal(false, actual);
		}

		[Fact]
		public async Task IsSectionDefined_ReturnsTrueIfSectionDefined()
		{
			// Arrange
			bool? actual = null;
			var page = CreatePage(v =>
			{
				actual = v.IsSectionDefined("baz");
				v.RenderSection("baz");
				v.RenderBodyPublic();
			});
			page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
			{
				{ "baz", _nullRenderAsyncDelegate }
			};
			page.BodyContent = new HtmlString("body-content");

			// Act
			await page.ExecuteAsync();

			// Assert
			Assert.Equal(true, actual);
		}

		[Fact]
		public async Task RenderSection_ThrowsIfSectionIsRenderedMoreThanOnce()
		{
			// Arrange
			var page = CreatePage(v =>
			{
				v.Path = "/Views/TestPath/Test.cshtml";
				v.RenderSection("header");
				v.RenderSection("header");
			});
			page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
			{
				{ "header", _nullRenderAsyncDelegate }
			};

			// Act & Assert
			var ex = await Assert.ThrowsAsync<InvalidOperationException>(page.ExecuteAsync);
			Assert.Equal("Section 'header' is already rendered",
				ex.Message);
		}

		[Fact]
		public async Task RenderSectionAsync_ThrowsIfSectionIsRenderedMoreThanOnce()
		{
			// Arrange
			var page = CreatePage(async v =>
			{
				v.Path = "/Views/TestPath/Test.cshtml";
				await v.RenderSectionAsync("header");
				await v.RenderSectionAsync("header");
			});
			page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
			{
				{ "header", _nullRenderAsyncDelegate }
			};

			// Act & Assert
			var ex = await Assert.ThrowsAsync<InvalidOperationException>(page.ExecuteAsync);
			Assert.Equal("Section 'header' is already rendered",
				ex.Message);
		}

		[Fact]
		public async Task RenderSectionAsync_ThrowsIfSectionIsRenderedMoreThanOnce_WithSyncMethod()
		{
			// Arrange
			var page = CreatePage(async v =>
			{
				v.Path = "/Views/TestPath/Test.cshtml";
				v.RenderSection("header");
				await v.RenderSectionAsync("header");
			});
			page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
			{
				{ "header", _nullRenderAsyncDelegate }
			};

			// Act & Assert
			var ex = await Assert.ThrowsAsync<InvalidOperationException>(page.ExecuteAsync);
			Assert.Equal("Section 'header' is already rendered",
				ex.Message);
		}

		[Fact]
		public async Task RenderSectionAsync_ThrowsIfNotInvokedFromLayoutPage()
		{
			// Arrange
			var page = CreatePage(async v =>
			{
				v.Path = "/Views/TestPath/Test.cshtml";
				await v.RenderSectionAsync("header");
			});

			// Act & Assert
			var ex = await Assert.ThrowsAsync<InvalidOperationException>(page.ExecuteAsync);
			Assert.Equal($"RenderSectionAsync invocation is invalid",
				ex.Message);
		}

		[Fact]
		public async Task EnsureRenderedBodyOrSections_ThrowsIfRenderBodyIsNotCalledFromPage_AndNoSectionsAreDefined()
		{
			// Arrange
			var path = "page-path";
			var page = CreatePage(v =>
			{
			});
			page.Path = path;
			page.BodyContent = new HtmlString("some content");
			await page.ExecuteAsync();

			// Act & Assert
			var ex = Assert.Throws<InvalidOperationException>(() => page.EnsureRenderedBodyOrSections());
			Assert.Equal($"Render body has not been called. To ignore call IgnoreBody().", ex.Message);
		}

		[Fact]
		public async Task EnsureRenderedBodyOrSections_SucceedsIfRenderBodyIsNotCalledFromPage_AndNoSectionsAreDefined_AndBodyIgnored()
		{
			// Arrange
			var path = "page-path";
			var page = CreatePage(v =>
			{
			});
			page.Path = path;
			page.BodyContent = new HtmlString("some content");
			page.IgnoreBody();

			// Act & Assert (does not throw)
			await page.ExecuteAsync();
			page.EnsureRenderedBodyOrSections();
		}

		[Fact]
		public async Task EnsureRenderedBodyOrSections_ThrowsIfDefinedSectionsAreNotRendered()
		{
			// Arrange
			var path = "page-path";
			var sectionName = "sectionA";
			var page = CreatePage(v =>
			{
			});
			page.Path = path;
			page.BodyContent = new HtmlString("some content");
			page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
			{
				{ sectionName, _nullRenderAsyncDelegate }
			};
			await page.ExecuteAsync();

			// Act & Assert
			var ex = Assert.Throws<InvalidOperationException>(() => page.EnsureRenderedBodyOrSections());
			Assert.Equal($"The following sections have been defined but have not been rendered :'{ sectionName }",
				ex.Message);
		}

		[Fact]
		public async Task EnsureRenderedBodyOrSections_SucceedsIfDefinedSectionsAreNotRendered_AndIgnored()
		{
			// Arrange
			var path = "page-path";
			var sectionName = "sectionA";
			var page = CreatePage(v =>
			{
			});
			page.Path = path;
			page.BodyContent = new HtmlString("some content");
			page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
			{
				{ sectionName, _nullRenderAsyncDelegate }
			};
			page.IgnoreSection(sectionName);

			// Act & Assert (does not throw)
			await page.ExecuteAsync();
			page.EnsureRenderedBodyOrSections();
		}

		[Fact]
		public async Task ExecuteAsync_RendersSectionsThatAreNotIgnored()
		{
			// Arrange
			var path = "page-path";
			var page = CreatePage(async p =>
			{
				p.IgnoreSection("ignored");
				p.Write(await p.RenderSectionAsync("not-ignored-section"));
			});
			page.Path = path;
			page.BodyContent = new HtmlString("some content");
			page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
			{
				{ "ignored", _nullRenderAsyncDelegate },
				{ "not-ignored-section", writer => writer.WriteAsync("not-ignored-section-content") }
			};

			// Act
			await page.ExecuteAsync();

			// Assert
			Assert.Equal("not-ignored-section-content", page.RenderedContent);
		}

		[Fact]
		public async Task EnsureRenderedBodyOrSections_SucceedsIfRenderBodyIsNotCalled_ButAllDefinedSectionsAreRendered()
		{
			// Arrange
			var sectionA = "sectionA";
			var sectionB = "sectionB";
			var page = CreatePage(v =>
			{
				v.RenderSection(sectionA);
				v.RenderSection(sectionB);
			});
			page.BodyContent = new HtmlString("some content");
			page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
			{
				{ sectionA, _nullRenderAsyncDelegate },
				{ sectionB, _nullRenderAsyncDelegate },
			};

			// Act & Assert (does not throw)
			await page.ExecuteAsync();
			page.EnsureRenderedBodyOrSections();
		}

		[Fact]
		public async Task ExecuteAsync_RendersSectionsAndBody()
		{
			// Arrange
			var expected = string.Join(Environment.NewLine,
									   "Layout start",
									   "Header section",
									   "Async Header section",
									   "body content",
									   "Async Footer section",
									   "Footer section",
									   "Layout end");
			var page = CreatePage(async v =>
			{
				v.WriteLiteral("Layout start" + Environment.NewLine);
				v.Write(v.RenderSection("header"));
				v.Write(await v.RenderSectionAsync("async-header"));
				v.Write(v.RenderBodyPublic());
				v.Write(await v.RenderSectionAsync("async-footer"));
				v.Write(v.RenderSection("footer"));
				v.WriteLiteral("Layout end");
			});
			page.BodyContent = new HtmlString("body content" + Environment.NewLine);
			page.PreviousSectionWriters = new Dictionary<string, RenderAsyncDelegate>
			{
				{
					"footer", writer => writer.WriteLineAsync("Footer section")
				},
				{
					"header", writer => writer.WriteLineAsync("Header section")
				},
				{
					"async-header", writer => writer.WriteLineAsync("Async Header section")
				},
				{
					"async-footer", writer => writer.WriteLineAsync("Async Footer section")
				},
			};

			// Act
			await page.ExecuteAsync();

			// Assert
			var actual = page.RenderedContent;
			Assert.Equal(expected, actual);
		}

		[Fact]
		public async Task FlushAsync_InvokesFlushOnWriter()
		{
			// Arrange
			var writer = new Mock<TextWriter>();
			var context = CreateViewContext(writer.Object);
			var page = CreatePage(async p =>
			{
				await p.FlushAsync();
			}, context);

			// Act
			await page.ExecuteAsync();

			// Assert
			writer.Verify(v => v.FlushAsync(), Times.Once());
		}

		[Fact]
		public async Task FlushAsync_ThrowsIfTheLayoutHasBeenSet()
		{
			// Arrange
			var expected = "Layout page cannot be rendered after 'FlushAsync' has been invoked.";
			var writer = new Mock<TextWriter>();
			var context = CreateViewContext(writer.Object);
			var page = CreatePage(async p =>
			{
				p.Path = "/Views/TestPath/Test.cshtml";
				p.Layout = "foo";
				await p.FlushAsync();
			}, context);

			// Act & Assert
			var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => page.ExecuteAsync());
			Assert.Equal(expected, ex.Message);
		}

		[Fact]
		public async Task FlushAsync_DoesNotThrowWhenIsRenderingLayoutIsSet()
		{
			// Arrange
			var writer = new Mock<TextWriter>();
			var context = CreateViewContext(writer.Object);
			var page = CreatePage(p =>
			{
				p.Layout = "bar";
				p.DefineSection("test-section", async _ =>
				{
					await p.FlushAsync();
				});
			}, context);

			// Act
			await page.ExecuteAsync();
			page.IsLayoutBeingRendered = true;

			// Assert (does not throw)
			var renderAsyncDelegate = page.SectionWriters["test-section"];
			await renderAsyncDelegate(TextWriter.Null);
		}

		[Fact]
		public async Task FlushAsync_ReturnsEmptyHtmlString()
		{
			// Arrange
			HtmlString actual = null;
			var writer = new Mock<TextWriter>();
			var context = CreateViewContext(writer.Object);
			var page = CreatePage(async p =>
			{
				actual = await p.FlushAsync();
			}, context);

			// Act
			await page.ExecuteAsync();

			// Assert
			Assert.Same(HtmlString.Empty, actual);
		}

		public static TheoryData WriteAttributeData
		{
			get
			{
				// AttributeValues, ExpectedOutput
				return new TheoryData<Tuple<string, int, object, int, bool>[], string>
				{
					{
						new[]
						{
							Tuple.Create(string.Empty, 9, (object)true, 9, false),
						},
						"someattr=HtmlEncode[[someattr]]"
					},
					{
						new[]
						{
							Tuple.Create(string.Empty, 9, (object)false, 9, false),
						},
						string.Empty
					},
					{
						new[]
						{
							Tuple.Create(string.Empty, 9, (object)null, 9, false),
						},
						string.Empty
					},
					{
						new[]
						{
							Tuple.Create("  ", 9, (object)false, 11, false),
						},
						"someattr=  HtmlEncode[[False]]"
					},
					{
						new[]
						{
							Tuple.Create("  ", 9, (object)null, 11, false),
						},
						"someattr="
					},
					{
						new[]
						{
							Tuple.Create("  ", 9, (object)true, 11, false),
							Tuple.Create("  ", 15, (object)"abcd", 17, true),
						},
						"someattr=  HtmlEncode[[True]]  abcd"
					},
				};
			}
		}

		[Theory]
		[MemberData(nameof(WriteAttributeData))]
		public void WriteAttributeTo_WritesAsExpected(
			Tuple<string, int, object, int, bool>[] attributeValues,
			string expectedOutput)
		{
			// Arrange
			var page = CreatePage(p => { });
			page.HtmlEncoder = new HtmlTestEncoder();
			var writer = new StringWriter();
			var prefix = "someattr=";
			var suffix = string.Empty;

			// Act
			page.BeginWriteAttributeTo(writer, "someattr", prefix, 0, suffix, 0, attributeValues.Length);
			foreach (var value in attributeValues)
			{
				page.WriteAttributeValueTo(
					writer,
					value.Item1,
					value.Item2,
					value.Item3,
					value.Item4,
					value.Item3?.ToString().Length ?? 0,
					value.Item5);
			}
			page.EndWriteAttributeTo(writer);

			// Assert
			Assert.Equal(expectedOutput, writer.ToString());
		}

		[Fact]
		public async Task Write_WithHtmlString_WritesValueWithoutEncoding()
		{
			// Arrange
			var buffer = new ViewBuffer(new TestViewBufferScope(), string.Empty, pageSize: 32);
			var writer = new ViewBufferTextWriter(buffer, Encoding.UTF8);

			var page = CreatePage(p =>
			{
				p.Write(new HtmlString("Hello world"));
			});
			page.PageContext.Writer = writer;

			// Act
			await page.ExecuteAsync();

			// Assert
			Assert.Equal("Hello world", HtmlContentUtilities.HtmlContentToString(buffer));
		}

		[Fact]
		public async Task Ensure_Raw_String_Is_Not_Encoded()
		{
			string expected = "\"Hello\"";
			var model = new { Data = "\"Hello\"" };

			using (var writer = new StringWriter())
			{
				var context = new PageContext() { Writer = writer };

				var page = CreatePage(v =>
				{
					v.Write(v.Raw(model.Data));
				}, context);

				await page.ExecuteAsync();

				string actual = writer.ToString();

				Assert.Equal(expected, actual);
			}
		}

		[Fact]
		public async Task Ensure_Include_Works_Correctly()
		{
			string actual = null;
			string expected = "FirstSecond";

			var toIncludePage = CreatePage(v =>
			{
				v.Write("Second");
			});

			var testLookup = new Mock<IPageLookup>();
			testLookup
				.Setup(v => v.GetPage(It.IsAny<string>()))
				.Returns(new PageLookupResult(
					new PageLookupItem(It.IsAny<string>(), new Func<TemplatePage>(() => toIncludePage)),
					new List<PageLookupItem>()));


			using (var writer = new StringWriter())
			{
				var context = new PageContext() { Writer = writer };
				var outerPage = CreatePage(async v =>
				{
					v.Write("First");
					await v.IncludeAsync("whatever");
				}, context);

				outerPage.PageLookup = testLookup.Object;
				await outerPage.ExecuteAsync();

				actual = writer.ToString();
			}

			Assert.Equal(expected, actual);
		}

		#region "Testing helpers"

		public abstract class TestableRazorPage : TemplatePage
		{
			public string RenderedContent
			{
				get
				{
					var bufferedWriter = Assert.IsType<ViewBufferTextWriter>(Output);
					using (var stringWriter = new StringWriter())
					{
						bufferedWriter.Buffer.WriteTo(stringWriter, HtmlEncoder);
						return stringWriter.ToString();
					}
				}
			}

			public IHtmlContent RenderBodyPublic()
			{
				return base.RenderBody();
			}
		}

		internal static TestableRazorPage CreatePage(
			Action<TestableRazorPage> executeAction,
			PageContext context = null)
		{
			return CreatePage(page =>
			{
				executeAction(page);
				return Task.FromResult(0);
			}, context);
		}


		internal static TestableRazorPage CreatePage(
			Func<TestableRazorPage, Task> executeAction,
			PageContext context = null)
		{
			context = context ?? CreateViewContext();
			var view = new Mock<TestableRazorPage> { CallBase = true };
			if (executeAction != null)
			{
				view.Setup(v => v.ExecuteAsync())
					.Returns(() =>
					{
						return executeAction(view.Object);
					});
			}

			view.Object.PageContext = context;
			return view.Object;
		}

		private static PageContext CreateViewContext(
			TextWriter writer = null,
			IViewBufferScope bufferScope = null,
			string viewPath = null)
		{
			bufferScope = bufferScope ?? new TestViewBufferScope();
			var buffer = new ViewBuffer(bufferScope, viewPath ?? "TEST", 32);
			writer = writer ?? new ViewBufferTextWriter(buffer, Encoding.UTF8);

			return new PageContext()
			{
				Writer = writer
			};
		}
	}

	public class TestViewBufferScope : IViewBufferScope
	{
		public IList<ViewBufferValue[]> CreatedBuffers { get; } = new List<ViewBufferValue[]>();

		public IList<ViewBufferValue[]> ReturnedBuffers { get; } = new List<ViewBufferValue[]>();

		public ViewBufferValue[] GetPage(int size)
		{
			var buffer = new ViewBufferValue[size];
			CreatedBuffers.Add(buffer);
			return buffer;
		}

		public void ReturnSegment(ViewBufferValue[] segment)
		{
			ReturnedBuffers.Add(segment);
		}

		public PagedBufferedTextWriter CreateWriter(TextWriter writer)
		{
			return new PagedBufferedTextWriter(ArrayPool<char>.Shared, writer);
		}
	}

	public sealed class HtmlTestEncoder : HtmlEncoder
	{
		public override int MaxOutputCharactersPerInputCharacter
		{
			get { return 1; }
		}

		public override string Encode(string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			if (value.Length == 0)
			{
				return string.Empty;
			}

			return $"HtmlEncode[[{value}]]";
		}

		public override void Encode(TextWriter output, char[] value, int startIndex, int characterCount)
		{
			if (output == null)
			{
				throw new ArgumentNullException(nameof(output));
			}

			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			if (characterCount == 0)
			{
				return;
			}

			output.Write("HtmlEncode[[");
			output.Write(value, startIndex, characterCount);
			output.Write("]]");
		}

		public override void Encode(TextWriter output, string value, int startIndex, int characterCount)
		{
			if (output == null)
			{
				throw new ArgumentNullException(nameof(output));
			}

			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			if (characterCount == 0)
			{
				return;
			}

			output.Write("HtmlEncode[[");
			output.Write(value.Substring(startIndex, characterCount));
			output.Write("]]");
		}

		public override bool WillEncode(int unicodeScalar)
		{
			return false;
		}

		public override unsafe int FindFirstCharacterToEncode(char* text, int textLength)
		{
			return -1;
		}

		public override unsafe bool TryEncodeUnicodeScalar(
			int unicodeScalar,
			char* buffer,
			int bufferLength,
			out int numberOfCharactersWritten)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}

			numberOfCharactersWritten = 0;
			return false;
		}
	}

	public class HtmlContentUtilities
	{
		public static string HtmlContentToString(IHtmlContent content, HtmlEncoder encoder = null)
		{
			if (encoder == null)
			{
				encoder = new HtmlTestEncoder();
			}

			using (var writer = new StringWriter())
			{
				content.WriteTo(writer, encoder);
				return writer.ToString();
			}
		}
	}

	#endregion
}
