using System;
using System.Linq;
using DynamicData;

namespace Mushrooms.Database {
    internal interface IEntityCache<TEntity> where TEntity : EntityBase {
        void        Insert( TEntity entity );
        void        Update( TEntity entity );
        void        Delete( TEntity entity );

        IObservableCache<TEntity, String>   Entities { get; }
    }

    internal class EntityCache<TEntity> : IEntityCache<TEntity> where TEntity : EntityBase {
        private readonly IEntityProvider<TEntity>       mEntityProvider;
        private readonly SourceCache<TEntity, string>   mEntities;

        public IObservableCache<TEntity, string>        Entities => mEntities.AsObservableCache();

        public EntityCache( IEntityProvider<TEntity> entityProvider ) {
            mEntityProvider = entityProvider;

            mEntities = new SourceCache<TEntity, string>( e => e.Id );
            mEntities.AddOrUpdate( mEntityProvider.GetAll());
        }

        public void Insert( TEntity entity ) {
            mEntityProvider.Insert( entity );
            mEntities.AddOrUpdate( mEntityProvider.GetAll());
        }

        public void Update( TEntity entity ) {
            var item = mEntities.Items.FirstOrDefault( e => e.Id.Equals( entity.Id ));

            if( item != null ) {
                mEntities.AddOrUpdate( item );
                mEntityProvider.Update( item );
            }
        }

        public void Delete( TEntity entity ) {
            var item = mEntities.Items.FirstOrDefault( e => e.Id.Equals( entity.Id ));

            if( item != null ) {
                mEntities.Remove( item );
                mEntityProvider.Delete( item );
            }
        }
    }
}
