﻿/*
 * Copyright 2017-2018 Wouter Huysentruit
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;

namespace EntityFrameworkMock
{
    public class DbSetMock<TEntity> : Mock<DbSet<TEntity>>, IDbSetMock
        where TEntity : class
    {
        private readonly DbSetBackingStore<TEntity> _store;

        public DbSetMock(IEnumerable<TEntity> initialEntities, Func<TEntity, KeyContext, object> keyFactory, bool asyncQuerySupport = true)
        {
            _store = new DbSetBackingStore<TEntity>(initialEntities, keyFactory);

            var data = _store.GetDataAsQueryable();
            As<IQueryable<TEntity>>().Setup(x => x.Provider).Returns(asyncQuerySupport ? new DbAsyncQueryProvider<TEntity>(data.Provider) : data.Provider);
            As<IQueryable<TEntity>>().Setup(x => x.Expression).Returns(data.Expression);
            As<IQueryable<TEntity>>().Setup(x => x.ElementType).Returns(data.ElementType);
            As<IQueryable<TEntity>>().Setup(x => x.GetEnumerator()).Returns(() => data.GetEnumerator());
            As<IEnumerable>().Setup(x => x.GetEnumerator()).Returns(() => data.GetEnumerator());

            if (asyncQuerySupport)
            {
                As<IDbAsyncEnumerable<TEntity>>().Setup(x => x.GetAsyncEnumerator()).Returns(() => new DbAsyncEnumerator<TEntity>(data.GetEnumerator()));
            }

            Setup(x => x.AsNoTracking()).Returns(() => Object);
            Setup(x => x.Include(It.IsAny<string>())).Returns(() => Object);

            Setup(x => x.Add(It.IsAny<TEntity>())).Callback<TEntity>(_store.Add);
            Setup(x => x.AddRange(It.IsAny<IEnumerable<TEntity>>())).Callback<IEnumerable<TEntity>>(_store.Add);
            Setup(x => x.Remove(It.IsAny<TEntity>())).Callback<TEntity>(_store.Remove);
            Setup(x => x.RemoveRange(It.IsAny<IEnumerable<TEntity>>())).Callback<IEnumerable<TEntity>>(_store.Remove);

            Setup(x => x.Find(It.IsAny<object[]>())).Returns<object[]>(_store.Find);
            Setup(x => x.FindAsync(It.IsAny<CancellationToken>(), It.IsAny<object[]>())).Returns<CancellationToken, object[]>((_, x) => Task.FromResult(_store.Find(x)));

            _store.UpdateSnapshot();
        }

        public event EventHandler<SavedChangesEventArgs<TEntity>> SavedChanges;

        int IDbSetMock.SaveChanges()
        {
            var changes = _store.ApplyChanges();
            SavedChanges?.Invoke(this, new SavedChangesEventArgs<TEntity> { UpdatedEntities = _store.GetUpdatedEntities() });
            _store.UpdateSnapshot();
            return changes;
        }
    }
}
