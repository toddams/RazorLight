// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Chunks;

namespace RazorLight.Host.Directives
{
	/// <summary>
	/// A utility type for supporting inheritance of directives into a page from applicable <c>_ViewImports</c> pages.
	/// </summary>
	public class ChunkInheritanceUtility
	{
		private readonly LightRazorHost _razorHost;
		private readonly IReadOnlyList<Chunk> _defaultInheritedChunks;

		/// <summary>
		/// Initializes a new instance of <see cref="ChunkInheritanceUtility"/>.
		/// </summary>
		/// <param name="razorHost">The <see cref="LightRazorHost"/> used to parse <c>_ViewImports</c> pages.</param>
		/// <param name="chunkTreeCache"><see cref="IChunkTreeCache"/> that caches <see cref="ChunkTree"/> instances.
		/// </param>
		/// <param name="defaultInheritedChunks">Sequence of <see cref="Chunk"/>s inherited by default.</param>
		public ChunkInheritanceUtility(
			LightRazorHost razorHost,
			IReadOnlyList<Chunk> defaultInheritedChunks)
		{
			if (razorHost == null)
			{
				throw new ArgumentNullException(nameof(razorHost));
			}

			if (defaultInheritedChunks == null)
			{
				throw new ArgumentNullException(nameof(defaultInheritedChunks));
			}

			_razorHost = razorHost;
			_defaultInheritedChunks = defaultInheritedChunks;
		}

		/// <summary>
		/// Gets an ordered <see cref="IReadOnlyList{ChunkTreeResult}"/> of parsed <see cref="ChunkTree"/>s and
		/// file paths for each <c>_ViewImports</c> that is applicable to the page located at
		/// <paramref name="pagePath"/>. The list is ordered so that the <see cref="ChunkTreeResult"/>'s
		/// <see cref="ChunkTreeResult.ChunkTree"/> for the <c>_ViewImports</c> closest to the
		/// <paramref name="pagePath"/> in the file system appears first.
		/// </summary>
		/// <param name="pagePath">The path of the page to locate inherited chunks for.</param>
		/// <returns>A <see cref="IReadOnlyList{ChunkTreeResult}"/> of parsed <c>_ViewImports</c>
		/// <see cref="ChunkTree"/>s and their file paths.</returns>
		/// <remarks>
		/// The resulting <see cref="IReadOnlyList{ChunkTreeResult}"/> is ordered so that the result
		/// for a _ViewImport closest to the application root appears first and the _ViewImport
		/// closest to the page appears last i.e.
		/// [ /_ViewImport, /Views/_ViewImport, /Views/Home/_ViewImport ]
		/// </remarks>
		public virtual IReadOnlyList<ChunkTreeResult> GetInheritedChunkTreeResults(string pagePath)
		{
			return new List<ChunkTreeResult>(); ;
		}

		/// <summary>
		/// Merges <see cref="Chunk"/> inherited by default and <see cref="ChunkTree"/> instances produced by parsing
		/// <c>_ViewImports</c> files into the specified <paramref name="chunkTree"/>.
		/// </summary>
		/// <param name="chunkTree">The <see cref="ChunkTree"/> to merge in to.</param>
		/// <param name="inheritedChunkTrees"><see cref="IReadOnlyList{ChunkTree}"/> inherited from <c>_ViewImports</c>
		/// files.</param>
		/// <param name="defaultModel">The default model <see cref="Type"/> name.</param>
		public void MergeInheritedChunkTrees(
			ChunkTree chunkTree,
			IReadOnlyList<ChunkTree> inheritedChunkTrees,
			string defaultModel)
		{
			if (chunkTree == null)
			{
				throw new ArgumentNullException(nameof(chunkTree));
			}

			if (inheritedChunkTrees == null)
			{
				throw new ArgumentNullException(nameof(inheritedChunkTrees));
			}

			var chunkMergers = GetChunkMergers(chunkTree, defaultModel);
			// We merge chunks into the ChunkTree in two passes. In the first pass, we traverse the ChunkTree visiting
			// a mapped IChunkMerger for types that are registered.
			foreach (var chunk in chunkTree.Children)
			{
				foreach (var merger in chunkMergers)
				{
					merger.VisitChunk(chunk);
				}
			}

			var inheritedChunks = _defaultInheritedChunks.Concat(
				inheritedChunkTrees.SelectMany(tree => tree.Children)).ToArray();

			foreach (var merger in chunkMergers)
			{
				merger.MergeInheritedChunks(chunkTree, inheritedChunks);
			}
		}

		private static IChunkMerger[] GetChunkMergers(ChunkTree chunkTree, string defaultModel)
		{
			var modelType = ChunkHelper.GetModelTypeName(chunkTree, defaultModel);
			return new IChunkMerger[]
			{
				new UsingChunkMerger(),
				new SetBaseTypeChunkMerger(modelType)
			};
		}
	}
}