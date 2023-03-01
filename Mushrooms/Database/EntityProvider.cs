using System.Collections.Generic;
using LiteDB;
using ReusableBits.Platform.Preferences;

namespace Mushrooms.Database {
    public interface IEntityProvider<TEntity> where TEntity : EntityBase {
        void                    Insert( TEntity entity );
        void                    Update( TEntity entity );
        void                    Delete( TEntity entity );

        IEnumerable<TEntity>    GetAll();
        ILiteQueryable<TEntity> Query();
    }

    public class LiteDatabaseProvider<TEntity> : IEntityProvider<TEntity> where TEntity : EntityBase {
        private readonly IEnvironment       mEnvironment;
        private LiteDatabase ?              mDatabase;
        private ILiteCollection<TEntity> ?  mCollection;

        public LiteDatabaseProvider( IEnvironment environment ) {
            mEnvironment = environment;
        }

        protected virtual void InitializeDatabase( LiteDatabase db ) { }
        protected virtual ILiteCollection<TEntity> Include( ILiteCollection<TEntity> list ) {
            return list;
        }

        private ILiteCollection<TEntity> Collection() {
            if( mCollection == null ) {
                var connection = $"filename={mEnvironment.DatabaseDirectory()};journal=false";

                mDatabase = new LiteDatabase( connection );

                InitializeDatabase( mDatabase );

                mCollection = Include( mDatabase.GetCollection<TEntity>());
            }

            return mCollection;
        }

        public IEnumerable<TEntity> GetAll() {
            return Collection().FindAll();
        }

        public ILiteQueryable<TEntity> Query() {
            return Collection().Query();
        }

        public void Insert( TEntity entity ) {
            Collection().Insert( entity );
        }

        public void Update( TEntity entity ) {
            Collection().Update( entity );
        }

        public void Delete( TEntity entity ) {
            Collection().Delete( entity.Id );
        }
    }
}
