// Copyright (C) 2013 Zeno Gantner
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
//
using System;
using System.Collections.Generic;
using MyMediaLite.Data;
using MyMediaLite.DataType;

namespace MyMediaLite.Correlation
{
	public class UserCorrelationBuilder : ICorrelationBuilder
	{
		ICorrelation correlation;

		public UserCorrelationBuilder(ICorrelation correlation)
		{
			this.correlation = correlation;
		}
		
		public void Build(IInteractions interactions)
		{
			
		}
		
		public void UpdateRows(IMatrix<float> correlation_matrix, IInteractions interactions, ICollection<int> update_entities)
		{
			foreach (int i in update_entities)
			{
				for (int j = 0; j < correlation_matrix.NumEntities; j++)
				{
					if (j < i && correlation_matrix.IsSymmetric && other_update_entities.Contains(j))
						continue;

					correlation_matrix[i, j] = correlation.Compute(interactions.ByUser(i).Items, interactions.ByUser(j).Items);
				}
			}
		}

	}
}

