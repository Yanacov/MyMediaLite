// Copyright (C) 2013 João Vinagre
// Copyright (C) 2010, 2011, 2012, 2013 Zeno Gantner
//
// This file is part of MyMediaLite.
//
// MyMediaLite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MyMediaLite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.IO;
using MyMediaLite.Correlation;
using MyMediaLite.DataType;
using MyMediaLite.IO;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Base class for item recommenders that use some kind of k-nearest neighbors (kNN) model</summary>
	/// <seealso cref="MyMediaLite.ItemRecommendation.KNN"/>
	public abstract class KNN : IncrementalItemRecommender
	{
		/// <summary>The number of neighbors to take into account for prediction</summary>
		public uint K { get { return k; } set { k = value; } }

		/// <summary>Alpha parameter for BidirectionalConditionalProbability</summary>
		public float Alpha { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="MyMediaLite.ItemRecommendation.KNN"/> is weighted.
		/// </summary>
		/// <remarks>
		/// TODO add literature reference
		/// </remarks>
		public bool Weighted { get; set; }

		/// <summary>Exponent to be used for transforming the neighbor's weights</summary>
		/// <remarks>
		///   <para>
		///     A value of 0 leads to counting of the relevant neighbors.
		///     1 is the usual weighted prediction.
		///     Values greater than 1 give higher weight to higher correlated neighbors.
		///   </para>
		///   <para>
		///     TODO LIT
		///   </para>
		/// </remarks>
		public float Q { get; set; }

		/// <summary>The kind of correlation to use</summary>
		public string Correlation { get; set; }

		/// <summary>The number of neighbors to take into account for prediction</summary>
		protected uint k = 80;

		/// <summary>Precomputed nearest neighbors</summary>
		protected IList<IList<int>> nearest_neighbors;

		/// <summary>Correlation matrix over some kind of entity, e.g. users or items</summary>
		protected IMatrix<float> correlation_matrix;

		protected ICorrelationBuilder correlation_builder;

		/// <summary>Default constructor</summary>
		public KNN()
		{
			Correlation = "Cosine";
			Alpha = 0.5f;
			Q = 1.0f;
			UpdateUsers = true;
			UpdateItems = true;
		}

		protected IBinaryCorrelation CreateCorrelation()
		{
			switch (Correlation)
			{
				case "Cosine":
					return new BinaryCosine();
				case "Jaccard":
					return new Jaccard();
				case "ConditionalProbability":
					return new ConditionalProbability();
				case "BidirectionalConditionalProbability":
					return new BidirectionalConditionalProbability(Alpha);
				case "Cooccurrence":
					return new Cooccurrence();
				default:
					throw new NotImplementedException(string.Format("{0} does not support correlation '{1}'.", this.GetType().Name, Correlation));
			}
		}

		/// <summary>Update the correlation matrix for the given feedback</summary>
		/// <param name='feedback'>the feedback (user-item tuples)</param>
		protected void Update(ICollection<Tuple<int, int>> feedback)
		{
			var update_entities = new HashSet<int>();
			foreach (var t in feedback)
				update_entities.Add(t.Item1);

			correlation_builder.UpdateRows(correlation_matrix, Interactions, update_entities);
			RecomputeNeighbors(update_entities);
		}

		private void RecomputeNeighbors(ICollection<int> update_entities)
		{
			foreach (int entity_id in update_entities)
				nearest_neighbors[entity_id] = correlation_matrix.GetNearestNeighbors(entity_id, k);
		}

		protected abstract void InitModel();

		///
		public override void Train()
		{
			correlation_matrix = correlation_builder.Build(Interactions);
		}

		///
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Model.GetWriter(filename, this.GetType(), "4.0") )
			{
				writer.WriteLine(Correlation);
				writer.WriteLine(nearest_neighbors.Count);
				foreach (IList<int> nn in nearest_neighbors)
					writer.WriteLine(String.Join(" ", nn));

				correlation_matrix.Write(writer);
			}
		}

		///
		public override void LoadModel(string filename)
		{
			throw new NotImplementedException();
		}

		/// <summary>Resizes the nearest neighbors list if necessary</summary>
		/// <param name='new_size'>the new size</param>
		protected void ResizeNearestNeighbors(int new_size)
		{
			if (new_size > nearest_neighbors.Count)
				for (int i = nearest_neighbors.Count; i < new_size; i++)
					nearest_neighbors.Add(null);
		}

		///
		public override string ToString()
		{
			return string.Format(
				"{0} k={1} correlation={2} q={3} weighted={4} alpha={5} (only for BidirectionalConditionalProbability)",
				this.GetType().Name, k == uint.MaxValue ? "inf" : k.ToString(), Correlation, Q, Weighted, Alpha);
		}
	}
}